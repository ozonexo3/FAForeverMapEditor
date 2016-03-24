using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;

public static class TextureLoader
{
	/*This function loads a .dds file at runtime from a byte array.	
	 * Example usage of this function:
	 * 
	 * byte[] bytes = System.IO.File.ReadAllBytes(ddsFilePath); 
	 * Texture2D myTexture = TextureLoader.LoadTextureDXT(bytes, TextureFormat.DXT1);
	 */
	public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
	{
		/*if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
			throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");*/
		
		byte ddsSizeCheck = ddsBytes[4];
		if (ddsSizeCheck != 124)
			throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files
		
		int height = ddsBytes[13] * 256 + ddsBytes[12];
		int width = ddsBytes[17] * 256 + ddsBytes[16];
		
		int DDS_HEADER_SIZE = 128;
		byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);
		//Debug.Log("Texture data load size: " + dxtBytes.Length + " , " + width +"x"+ height + ", " + textureFormat);
		Texture2D texture = new Texture2D(width, height, textureFormat, false);
		texture.LoadRawTextureData(dxtBytes);
		texture.Apply();
		
		return (texture);
	}

	public static T[] Concat<T>(this T[] x, T[] y)
	{
		if (x == null) throw new ArgumentNullException("x");
		if (y == null) throw new ArgumentNullException("y");
		int oldLen = x.Length;
		Array.Resize<T>(ref x, x.Length + y.Length);
		Array.Copy(y, 0, x, oldLen, y.Length);
		return x;
	}

	 //Note that these SaveTextureDDS functions do not currently work.
	public static void SaveTextureDDSTester(Texture2D texture, BinaryWriter writer)
	{
		//Here we get to write a dds file from scratch! yay lack of unity's dds saving abilities....
		//DWORD dwMagic = 0x20534444 or 'DDS '
		//DDS_HEADER 
		/* 
		 * DWORD           dwSize; Must be set to 124
		 * DWORD           dwFlags; flags which contain valid data...
		 * DDSD_CAPS = 0x1;DDSD_HEIGHT = 0x2;DDSD_WIDTH = 0x4;
		 * DWORD           dwHeight;
		 * DWORD           dwWidth;
		 * DWORD           dwPitchOrLinearSize;
		 * DWORD           dwDepth;
		 * DWORD           dwMipMapCount;
		 * DWORD           dwReserved1[11];
		 * DDS_PIXELFORMAT ddspf;
		 * DWORD           dwCaps;
		 * DWORD           dwCaps2;
		 * DWORD           dwCaps3;
		 * DWORD           dwCaps4;
		 * DWORD           dwReserved2;
		 */
		//BYTE bdata[] = main surface data array
		//BYTE bdata2[] pointer to an array of bytes containing remaining surfaces such as mipmap levels, faces in a cube map, depths in volume textures, etc.
		
		//DDS_HEADER
		uint dwMagic = 0x2053444;
		uint dwSize = 124;
		/*
			DDSD_CAPS	Required in every .dds file.
			DDSD_HEIGHT	Required in every .dds file.
			DDSD_WIDTH	Required in every .dds file.
			DDSD_PITCH	Required when pitch is provided for an uncompressed texture.
			DDSD_PIXELFORMAT	Required in every .dds file.
			DDSD_MIPMAPCOUNT	Required in a mipmapped texture.
			DDSD_LINEARSIZE	Required when pitch is provided for a compressed texture.
			DDSD_DEPTH Required in a depth texture
		 */
		uint dwFlags = Convert.ToUInt32("000010111",2);
		
		uint dwHeight = (uint)texture.height;
		uint dwWidth = (uint)texture.width;
		uint dwPitchOrLinearSize = (dwWidth * 8 + 7)/8;
		uint dwDepth = 0;
		uint dwMipMapCount = (uint)texture.mipmapCount;
		int[] dwReserved1 = new int[11];
		//DDS_PIXELFORMAT
		uint pf_dwSize = 32;
		uint pf_dwFlags = Convert.ToUInt32("001001",2);
		uint pf_dwFourCC = 0;
		uint pf_dwRGBBitCount = 32;
		uint pf_dwRBitMask = 0x00ff0000;
		uint pf_dwGBitMask = 0x0000ff00;
		uint pf_dwBBitMask = 0x000000ff;
		uint pf_dwABitMask = 0xff000000;
		//DDCAPS2
		uint dwCaps1 = 0;
		uint dwCaps2 = 0;
		uint dwCaps3 = 0;
		uint dwCaps4 = 0;
		uint dwReserved2 = 0;
		
		//int argb = _with1.ReadInt32();
		//int r = (argb)&0xFF;
		//int g = (argb>>8)&0xFF;
		//int b = (argb>>16)&0xFF;
		//int a = (argb>>24)&0xFF;
		Color32[] colors = texture.GetPixels32();
		int length = 4 * colors.Length;
		byte[] bdata = new byte[length];//Main Surface Data
		IntPtr ptr = Marshal.AllocHGlobal(length);
		Marshal.StructureToPtr(colors, ptr, true);
		Marshal.Copy(ptr, bdata, 0, length);
		Marshal.FreeHGlobal(ptr);
		
		byte[] bdata2;
		
		if(dwMipMapCount > 1)
		{
			int bdata2Length = 4 * colors.Length * (int)dwMipMapCount;
			bdata2 = new byte[bdata2Length];//contains the remaining surfaces such as; mipmap levels, faces in a cube map
			for(int i = 0; i < dwMipMapCount; i++)
			{
				Color32[] colors2 = texture.GetPixels32(i);
				int length2 = 4 * colors2.Length;
				IntPtr ptr2 = Marshal.AllocHGlobal(i*length2);
				Marshal.StructureToPtr(colors2, ptr2, true);
				Marshal.Copy(ptr2, bdata, 0, length2);//Marshal.Copy(ptr2, bdata, 0, length2);
				Marshal.FreeHGlobal(ptr2);
			}
		}
		else
		{
			bdata2 = new byte[0];
		}
		
		//To write an actual dds file out
		writer.Write(dwMagic);
		writer.Write(dwSize);
		writer.Write(dwFlags);
		writer.Write(dwHeight);
		writer.Write(dwWidth);
		writer.Write(dwPitchOrLinearSize);
		writer.Write(dwDepth);
		writer.Write(dwMipMapCount);
		writer.Write(dwReserved1);
		writer.Write(pf_dwSize);
		writer.Write(pf_dwFlags);
		writer.Write(pf_dwFourCC);
		writer.Write(pf_dwRGBBitCount);
		writer.Write(pf_dwRBitMask);
		writer.Write(pf_dwGBitMask);
		writer.Write(pf_dwBBitMask);
		writer.Write(pf_dwABitMask);
		writer.Write(dwCaps1);
		writer.Write(dwCaps2);
		writer.Write(dwCaps3);
		writer.Write(dwCaps4);
		writer.Write(dwReserved2);
		writer.Write(bdata);
		writer.Write(bdata2);
		writer.Close();
		//*/
	}

	public static byte[] SaveTextureDDS(Texture2D texture, GetGamedataFile.HeaderClass header)
	{
		//Here we get to write a dds file from scratch! yay lack of unity's dds saving abilities....
		//DWORD dwMagic = 0x20534444 or 'DDS '
		//DDS_HEADER 
		/* 
		 * DWORD           dwSize; Must be set to 124
		 * DWORD           dwFlags; flags which contain valid data...
		 * DDSD_CAPS = 0x1;DDSD_HEIGHT = 0x2;DDSD_WIDTH = 0x4;
		 * DWORD           dwHeight;
		 * DWORD           dwWidth;
		 * DWORD           dwPitchOrLinearSize;
		 * DWORD           dwDepth;
		 * DWORD           dwMipMapCount;
		 * DWORD           dwReserved1[11];
		 * DDS_PIXELFORMAT ddspf;
		 * DWORD           dwCaps;
		 * DWORD           dwCaps2;
		 * DWORD           dwCaps3;
		 * DWORD           dwCaps4;
		 * DWORD           dwReserved2;
		 */
		//BYTE bdata[] = main surface data array
		//BYTE bdata2[] pointer to an array of bytes containing remaining surfaces such as mipmap levels, faces in a cube map, depths in volume textures, etc.

		//DDS_HEADER
		uint dwMagic = 0x2053444;
		uint dwSize = header.size;
		/*
			DDSD_CAPS	Required in every .dds file.
			DDSD_HEIGHT	Required in every .dds file.
			DDSD_WIDTH	Required in every .dds file.
			DDSD_PITCH	Required when pitch is provided for an uncompressed texture.
			DDSD_PIXELFORMAT	Required in every .dds file.
			DDSD_MIPMAPCOUNT	Required in a mipmapped texture.
			DDSD_LINEARSIZE	Required when pitch is provided for a compressed texture.
			DDSD_DEPTH Required in a depth texture
		 */
		uint dwFlags = header.flags;

		uint dwHeight = (uint)header.height;
		uint dwWidth = (uint)header.width;
		uint dwPitchOrLinearSize = header.sizeorpitch;
		uint dwDepth = header.depth;
		uint dwMipMapCount = (uint)texture.mipmapCount;
		int[] dwReserved1 = new int[11];
		//DDS_PIXELFORMAT
		uint pf_dwSize = header.pixelformatSize;
		uint pf_dwFlags = header.pixelformatflags;
		uint pf_dwFourCC = header.pixelformatFourcc;
		uint pf_dwRGBBitCount = header.pixelformatRgbBitCount;
			uint pf_dwRBitMask = header.pixelformatRbitMask;
		uint pf_dwGBitMask = header.pixelformatGbitMask;
		uint pf_dwBBitMask = header.pixelformatBbitMask;
		uint pf_dwABitMask = header.pixelformatAbitMask;
		//DDCAPS2
		uint dwCaps1 = 0;
		uint dwCaps2 = 0;
		uint dwCaps3 = 0;
		uint dwCaps4 = 0;
		uint dwReserved2 = 0;

		//int argb = _with1.ReadInt32();
		//int r = (argb)&0xFF;
		//int g = (argb>>8)&0xFF;
		//int b = (argb>>16)&0xFF;
		//int a = (argb>>24)&0xFF;

		Color32[] colors = texture.GetPixels32();

		int length = 4 * colors.Length;
		byte[] bdata = new byte[length];//Main Surface Data
		IntPtr ptr = Marshal.AllocHGlobal(length);
		Marshal.StructureToPtr(colors, ptr, true);
		Marshal.Copy(ptr, bdata, 0, length);
		Marshal.FreeHGlobal(ptr);

		bdata = texture.GetRawTextureData();

		byte[] bdata2;

		//Debug.Log(dwMipMapCount);
		if(dwMipMapCount > 1)
		{
			int bdata2Length = 4 * colors.Length * (int)dwMipMapCount;
			bdata2 = new byte[bdata2Length];//contains the remaining surfaces such as; mipmap levels, faces in a cube map
			for(int i = 0; i < dwMipMapCount; i++)
			{
				Color32[] colors2 = texture.GetPixels32(i);
				int length2 = 4 * colors2.Length;
				IntPtr ptr2 = Marshal.AllocHGlobal(i*length2);
				Marshal.StructureToPtr(colors2, ptr2, true);
				Marshal.Copy(ptr2, bdata, 0, length2);
				Marshal.FreeHGlobal(ptr2);
			}
		}
		else
		{
			bdata2 = new byte[0];
		}

		byte[] finalByteData = BitConverter.GetBytes(dwMagic);
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwSize));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwFlags));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwHeight));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwWidth));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwPitchOrLinearSize));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwDepth));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwMipMapCount));
		//byte[] dwReserveBytes = new byte[dwReserved1.Length]; //dwReserved is empty... but allocated space for a set number of uints...
		for(int i = 0; i < 10; i++){
			finalByteData = finalByteData.Concat(BitConverter.GetBytes((uint)header.reserved[i]));
		}
		//finalByteData = finalByteData.Concat(dwReserveBytes);
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwSize));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwFlags));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwFourCC));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwRGBBitCount));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwRBitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwGBitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwBBitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(pf_dwABitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwCaps1));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwCaps2));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwCaps3));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwCaps4));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwReserved2));
		Debug.Log("Header size: " + finalByteData.Length);
		finalByteData = finalByteData.Concat(bdata);
		//finalByteData = finalByteData.Concat(bdata2);
		//Debug.Log("Texture data size: " + bdata.Length);
		//Debug.Log("dds data size: " + finalByteData.Length);
		//Debug.Log("Load data size: " + header.DebugSize);
		return finalByteData;

		//To write an actual dds file out
	/*	writer.Write(dwMagic);
		writer.Write(dwSize);
		writer.Write(dwFlags);
		writer.Write(dwHeight);
		writer.Write(dwWidth);
		writer.Write(dwPitchOrLinearSize);
		writer.Write(dwDepth);
		writer.Write(dwMipMapCount);
		writer.Write(dwReserved1);
		writer.Write(pf_dwSize);
		writer.Write(pf_dwFlags);
		writer.Write(pf_dwFourCC);
		writer.Write(pf_dwRGBBitCount);
		writer.Write(pf_dwRBitMask);
		writer.Write(pf_dwGBitMask);
		writer.Write(pf_dwBBitMask);
		writer.Write(pf_dwABitMask);
		writer.Write(dwCaps1);
		writer.Write(dwCaps2);
		writer.Write(dwCaps3);
		writer.Write(dwCaps4);
		writer.Write(dwReserved2);
		writer.Write(bdata);
		writer.Write(bdata2);
		writer.Close();
		*/
	}
}
