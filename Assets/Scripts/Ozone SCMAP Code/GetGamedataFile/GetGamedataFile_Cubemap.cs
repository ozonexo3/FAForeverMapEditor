// ******************************************************************************
//
// * System for getting files from GameData SCD files or from Map folder
// * It also converts them to Unity objects: Texture2D, Mesh, Materials
// * Copyright ozonexo3 2017
//
// ******************************************************************************


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public partial struct GetGamedataFile
{

	static Dictionary<string, Cubemap> CubemapMemory = new Dictionary<string, Cubemap>();

	public static Cubemap GetGamedataCubemap(string scd, string LocalPath)
	{
		if (string.IsNullOrEmpty(LocalPath))
			return null;

		string TextureKey = scd + "_" + LocalPath;

		if (CubemapMemory.ContainsKey(TextureKey))
			return CubemapMemory[TextureKey];

		if (DebugTextureLoad)
			Debug.Log(LocalPath);

		byte[] FinalTextureData2 = LoadBytes(scd, LocalPath);

		if (FinalTextureData2 == null || FinalTextureData2.Length == 0)
		{
			//Debug.LogWarning("File bytes are empty!");
			return null;
		}

		TextureFormat format = GetFormatOfDdsBytes(FinalTextureData2);
		bool Mipmaps = LoadDDsHeader.mipmapcount > 0;
		Texture2D texture = new Texture2D((int)LoadDDsHeader.width, (int)LoadDDsHeader.height, format, Mipmaps, false);

		int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[FinalTextureData2.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(FinalTextureData2, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData2.Length - DDS_HEADER_SIZE);

		Cubemap NewTes = new Cubemap(texture.width, TextureFormat.RGBA32, false);

		int SideBytesLength = dxtBytes.Length / 6;
		for (int side = 0; side < 6; side++)
		{
			byte[] sideBytes = new byte[dxtBytes.Length / 6];
			Buffer.BlockCopy(dxtBytes, SideBytesLength * side, sideBytes, 0, SideBytesLength);

			try
			{
				texture.LoadRawTextureData(sideBytes);
				texture.Apply();
				NewTes.SetPixels(texture.GetPixels(0), (CubemapFace)side, 0);

			}
			catch (System.Exception e)
			{
				Debug.Log("Texture load fallback: " + LocalPath + "\n" + e);
				//texture = DDS.DDSReader.LoadDDSTexture(new MemoryStream(FinalTextureData2), false).ToTexture2D();
			}


		}

		NewTes.Apply(true);

		return NewTes;
	}


}
