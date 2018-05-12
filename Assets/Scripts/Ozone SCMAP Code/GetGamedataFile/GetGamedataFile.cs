// ******************************************************************************
//
// * System for getting files from GameData SCD files or from Map folder
// * It also converts them to Unity objects: Texture2D, Mesh, Materials
// * Copyright ozonexo3 2017
//
// ******************************************************************************

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

#pragma warning disable 0162

public partial struct GetGamedataFile
{
	static bool DebugTextureLoad = false;
	static bool IsDxt3 = false;

	public static float MipmapBias = 0.0f;
	public static int AnisoLevel = 6;

	static Dictionary<string, Texture2D> LoadedTextures = new Dictionary<string, Texture2D>();
	public static void CleanTextureMemory()
	{
		LoadedTextures = new Dictionary<string, Texture2D>();
	}

	public static Texture2D LoadTexture2DFromGamedata(string scd, string LocalPath, bool NormalMap = false, bool StoreInMemory = true)
	{
		string TextureKey = scd + "_" + LocalPath;

		if (LoadedTextures.ContainsKey(TextureKey))
			return LoadedTextures[TextureKey];


		byte[] FinalTextureData2 = LoadBytes(scd, LocalPath);

		if (FinalTextureData2 == null || FinalTextureData2.Length == 0)
		{
			//Debug.LogWarning("File bytes are empty!");
			return Texture2D.whiteTexture;
		}

		TextureFormat format = GetFormatOfDdsBytes(FinalTextureData2);
		bool Mipmaps = LoadDDsHeader.mipmapcount > 0;
		Texture2D texture = new Texture2D((int)LoadDDsHeader.width, (int)LoadDDsHeader.height, format, Mipmaps, false);


		int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[FinalTextureData2.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(FinalTextureData2, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData2.Length - DDS_HEADER_SIZE);

		if (IsDxt3)
		{
			texture = DDS.DDSReader.LoadDDSTexture(new MemoryStream(FinalTextureData2), false).ToTexture2D();
		}
		else
		{
			try
			{
				texture.LoadRawTextureData(dxtBytes);
			}
			catch (System.Exception e)
			{
				Debug.Log("Texture load fallback: " + LocalPath + "\n" + e);
				texture = DDS.DDSReader.LoadDDSTexture(new MemoryStream(FinalTextureData2), false).ToTexture2D();
			}
		}

		if (NormalMap)
		{
		}

		texture.mipMapBias = MipmapBias;
		texture.filterMode = FilterMode.Bilinear;
		texture.anisoLevel = AnisoLevel;
		texture.Apply(false);

		if(StoreInMemory)
			LoadedTextures.Add(TextureKey, texture);

		return texture;
	}


	public static void LoadTextureFromGamedata(string scd, string LocalPath, int Id, bool NormalMap = false)
	{
		if (NormalMap)
		{
			ScmapEditor.Current.Textures[Id].Normal = LoadTexture2DFromGamedata(scd, LocalPath, NormalMap);

			if (ScmapEditor.Current.Textures[Id].Normal.width > 4 && ScmapEditor.Current.Textures[Id].Normal.height > 4 && ScmapEditor.Current.Textures[Id].Normal.mipmapCount <= 1)
			{
				Debug.Log("Force mipmaps: " + LocalPath + " has " + ScmapEditor.Current.Textures[Id].Normal.mipmapCount + " mipmaps");
				ScmapEditor.Current.Textures[Id].Normal = ConvertWithMipmaps(ScmapEditor.Current.Textures[Id].Normal);
			}
		}
		else
		{
			ScmapEditor.Current.Textures[Id].Albedo = LoadTexture2DFromGamedata(scd, LocalPath, NormalMap);
			if (ScmapEditor.Current.Textures[Id].Albedo.width > 4 && ScmapEditor.Current.Textures[Id].Albedo.height > 4 && ScmapEditor.Current.Textures[Id].Albedo.mipmapCount <= 1)
			{
				Debug.Log("Force mipmaps: " + LocalPath + " has " + ScmapEditor.Current.Textures[Id].Albedo.mipmapCount + " mipmaps");
				ScmapEditor.Current.Textures[Id].Albedo = ConvertWithMipmaps(ScmapEditor.Current.Textures[Id].Albedo);
			}
		}
	}

	public static Texture2D ConvertWithMipmaps(Texture2D Old){
		Texture2D ToReturn = new Texture2D(Old.width, Old.height, TextureFormat.RGBA32, true, false);

		ToReturn.SetPixels(Old.GetPixels(0), 0);
		ToReturn.Apply(true);

		return ToReturn;
	}


	static HeaderClass LoadDDsHeader;

	[System.Serializable]
	public class HeaderClass
	{
		public TextureFormat Format;
		public uint Magic;

		public uint size;
		public uint flags;
		public uint height;
		public uint width;
		public uint sizeorpitch;
		public uint depth;
		public uint mipmapcount;
		public uint alphabitdepth;
		//public uint[] reserved;

		public uint reserved0;
		public uint reserved1;
		public uint reserved2;
		public uint reserved3;
		public uint reserved4;
		public uint reserved5;
		public uint reserved6;
		public uint reserved7;
		public uint reserved8;
		public uint reserved9;

		public uint pixelformatSize;
		public uint pixelformatflags;
		public uint pixelformatFourcc;
		public uint pixelformatRgbBitCount;
		public uint pixelformatRbitMask;
		public uint pixelformatGbitMask;
		public uint pixelformatBbitMask;
		public uint pixelformatAbitMask;

		public uint caps1;
		public uint caps2;
		public uint caps3;
		public uint caps4;
	}

	static TextureFormat GetFormatOfDdsBytes(byte[] bytes)
	{

		Stream ms = new MemoryStream(bytes);
		BinaryReader Stream = new BinaryReader(ms);
		LoadDDsHeader = BinaryStreamDdsHeader(Stream);

		return ReadFourcc(LoadDDsHeader.pixelformatFourcc);
	}

	public static TextureFormat GetFormatOfDds(string FinalImagePath)
	{

		if (!File.Exists(FinalImagePath))
		{
			Debug.LogError("File not exist!");
			return TextureFormat.DXT5;
		}

		System.IO.FileStream fs = new System.IO.FileStream(FinalImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		BinaryReader Stream = new BinaryReader(fs);
		LoadDDsHeader = BinaryStreamDdsHeader(Stream);

		return ReadFourcc(LoadDDsHeader.pixelformatFourcc);
	}

	public static HeaderClass GetDdsFormat(byte[] Bytes)
	{
		Stream fs = new MemoryStream(Bytes);
		BinaryReader Stream = new BinaryReader(fs);
		return BinaryStreamDdsHeader(Stream);
	}

	static HeaderClass BinaryStreamDdsHeader(BinaryReader Stream)
	{
		HeaderClass DDsHeader = new HeaderClass();

		DDsHeader.Magic = Stream.ReadUInt32();
		DDsHeader.size = Stream.ReadUInt32();
		DDsHeader.flags = Stream.ReadUInt32();
		DDsHeader.height = Stream.ReadUInt32();
		DDsHeader.width = Stream.ReadUInt32();
		DDsHeader.sizeorpitch = Stream.ReadUInt32();
		DDsHeader.depth = Stream.ReadUInt32();
		DDsHeader.mipmapcount = Stream.ReadUInt32();

		DDsHeader.alphabitdepth = Stream.ReadUInt32();
		/*DDsHeader.reserved = new uint[10];
		for (int i = 0; i < 10; i++)
		{
			DDsHeader.reserved[i] = Stream.ReadUInt32();
		}*/

		DDsHeader.reserved0 = Stream.ReadUInt32();
		DDsHeader.reserved1 = Stream.ReadUInt32();
		DDsHeader.reserved2 = Stream.ReadUInt32();
		DDsHeader.reserved3 = Stream.ReadUInt32();
		DDsHeader.reserved4 = Stream.ReadUInt32();
		DDsHeader.reserved5 = Stream.ReadUInt32();
		DDsHeader.reserved6 = Stream.ReadUInt32();
		DDsHeader.reserved7 = Stream.ReadUInt32();
		DDsHeader.reserved8 = Stream.ReadUInt32();
		DDsHeader.reserved9 = Stream.ReadUInt32();

		DDsHeader.pixelformatSize = Stream.ReadUInt32();
		DDsHeader.pixelformatflags = Stream.ReadUInt32();
		DDsHeader.pixelformatFourcc = Stream.ReadUInt32();
		DDsHeader.pixelformatRgbBitCount = Stream.ReadUInt32();
		DDsHeader.pixelformatRbitMask = Stream.ReadUInt32();
		DDsHeader.pixelformatGbitMask = Stream.ReadUInt32();
		DDsHeader.pixelformatBbitMask = Stream.ReadUInt32();
		DDsHeader.pixelformatAbitMask = Stream.ReadUInt32();

		DDsHeader.caps1 = Stream.ReadUInt32();
		DDsHeader.caps2 = Stream.ReadUInt32();
		DDsHeader.caps3 = Stream.ReadUInt32();
		DDsHeader.caps4 = Stream.ReadUInt32();

		return DDsHeader;
	}


	static TextureFormat ReadFourcc(uint fourcc)
	{
		IsDxt3 = false;
		if (DebugTextureLoad) Debug.Log(
			 "Size: " + LoadDDsHeader.size +
			 " flags: " + LoadDDsHeader.flags +
			 " height: " + LoadDDsHeader.height +
			 " width: " + LoadDDsHeader.width +
			 " sizeorpitch: " + LoadDDsHeader.sizeorpitch +
			 " depth: " + LoadDDsHeader.depth +
			 " mipmapcount: " + LoadDDsHeader.mipmapcount +
			 " alphabitdepth: " + LoadDDsHeader.alphabitdepth +
			 " pixelformatSize: " + LoadDDsHeader.pixelformatSize +
			 " pixelformatflags: " + LoadDDsHeader.pixelformatflags +
			 " pixelformatFourcc: " + LoadDDsHeader.pixelformatFourcc +
			 " pixelformatRgbBitCount: " + LoadDDsHeader.pixelformatRgbBitCount +
			 " pixelformatRbitMask: " + LoadDDsHeader.pixelformatRbitMask +
			 " pixelformatGbitMask: " + LoadDDsHeader.pixelformatGbitMask +
			 " pixelformatBbitMask: " + LoadDDsHeader.pixelformatBbitMask +
			 " pixelformatAbitMask: " + LoadDDsHeader.pixelformatAbitMask
		 );


		switch (LoadDDsHeader.pixelformatFourcc)
		{
			case 861165636:
				IsDxt3 = true;
				return TextureFormat.DXT5;
			case 827611204:
				return TextureFormat.DXT1;
			case 894720068:
				return TextureFormat.DXT5;
			case 64:
				return TextureFormat.RGB24;
			case 0:
				if (LoadDDsHeader.pixelformatflags == 528391)
				{
					return TextureFormat.BGRA32;
				}
				else if (LoadDDsHeader.pixelformatRgbBitCount == 24)
				{
					return TextureFormat.RGB24;
				}
				else
				{
					return TextureFormat.BGRA32;
				}
		}

		return TextureFormat.DXT5;
	}



}