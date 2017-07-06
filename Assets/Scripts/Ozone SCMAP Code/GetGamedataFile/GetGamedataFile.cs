﻿using UnityEngine;
using System.Collections;
using System;
using System.IO;

#pragma warning disable 0162

public partial struct GetGamedataFile
{


	static bool DebugTextureLoad = false;
	static bool IsDxt3 = false;

	public static float MipmapBias = 0.5f;
	public static int AnisoLevel = 10;

	public static Texture2D LoadTexture2DFromGamedata(string scd, string LocalPath, bool NormalMap = false)
	{
		byte[] FinalTextureData2 = LoadBytes(scd, LocalPath);

		if (FinalTextureData2 == null || FinalTextureData2.Length == 0)
		{
			return new Texture2D(32, 32, TextureFormat.RGBA32, false);
		}

		TextureFormat format = GetFormatOfDdsBytes(FinalTextureData2);
		bool Mipmaps = LoadDDsHeader.mipmapcount > 0;
		Texture2D texture = new Texture2D((int)LoadDDsHeader.width, (int)LoadDDsHeader.height, format, Mipmaps, true);


		int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[FinalTextureData2.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(FinalTextureData2, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData2.Length - DDS_HEADER_SIZE);

		if (IsDxt3)
		{
			texture = DDS.DDSReader.LoadDDSTexture(new MemoryStream(FinalTextureData2), false).ToTexture2D();
			//texture.Apply(false);
		}
		else
		{
			try
			{
				texture.LoadRawTextureData(dxtBytes);
			}
			catch
			{
				texture = DDS.DDSReader.LoadDDSTexture(new MemoryStream(FinalTextureData2), false).ToTexture2D();
			}
		}
		texture.Apply(false);

		if (NormalMap)
		{
			texture.Compress(true);

			Texture2D normalTexture = new Texture2D((int)LoadDDsHeader.width, (int)LoadDDsHeader.height, TextureFormat.RGBA32, Mipmaps, true);

			//Color theColour = new Color();
			Color[] Pixels;

			int m = 0;
			int i = 0;
			int Texwidth = 0;
			int Texheight = 0;

			for (m = 0; m < LoadDDsHeader.mipmapcount + 1; m++)
			{
				Texwidth = texture.width;
				Texheight = texture.height;

				if (m > 0)
				{
					Texwidth /= (int)Mathf.Pow(2, m);
					Texheight /= (int)Mathf.Pow(2, m);
				}
				Pixels = texture.GetPixels(0, 0, Texwidth, Texheight, m);

				for (i = 0; i < Pixels.Length; i++)
				{
					/*theColour.r = Pixels[i].r;
					theColour.g = Pixels[i].g;
					theColour.b = 1;
					theColour.a = Pixels[i].g;
					Pixels[i] = theColour;*/
					Pixels[i].b = 1;
					Pixels[i].a = Pixels[i].g;
				}
				normalTexture.SetPixels(0, 0, Texwidth, Texheight, Pixels, m);
			}

			normalTexture.Apply(false);

			normalTexture.mipMapBias = MipmapBias;
			normalTexture.filterMode = FilterMode.Bilinear;
			normalTexture.anisoLevel = AnisoLevel;
			return normalTexture;
		}


		texture.mipMapBias = MipmapBias;
		texture.filterMode = FilterMode.Bilinear;
		texture.anisoLevel = AnisoLevel;

		return texture;
	}


	public static void LoadTextureFromGamedata(string scd, string LocalPath, int Id, bool NormalMap = false)
	{
		if (NormalMap)
		{
			ScmapEditor.Current.Textures[Id].Normal = LoadTexture2DFromGamedata(scd, LocalPath, NormalMap);
		}
		else
		{
			ScmapEditor.Current.Textures[Id].Albedo = LoadTexture2DFromGamedata(scd, LocalPath, NormalMap);
		}
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
		public uint[] reserved;

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
		DDsHeader.reserved = new uint[10];
		for (int i = 0; i < 10; i++)
		{
			DDsHeader.reserved[i] = Stream.ReadUInt32();
		}

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