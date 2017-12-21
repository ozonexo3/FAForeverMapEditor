// ***************************************************************************************
// * DDS texture loader
// * 
// ***************************************************************************************

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.IO;

public static class TextureLoader
{

	const int DDS_HEADER_SIZE = 128;

	public static Texture2D LoadTextureDXT(byte[] ddsBytes, GetGamedataFile.HeaderClass header)
	{
		// Load raw texture data based on its header
		// Use GetGamedataFile.GetDdsFormat(byte[]) to load header

		byte ddsSizeCheck = ddsBytes[4];
		if (ddsSizeCheck != 124)
			throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

		byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
		Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);
		Texture2D texture = new Texture2D((int)header.width, (int)header.height, header.Format, false);
		texture.LoadRawTextureData(dxtBytes);
		texture.Apply();

		return texture;
	}

	public static Texture2D ConvertToRGBA(Texture2D texture){
		Texture2D texture2 = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
		texture2.SetPixels (texture.GetPixels ());
		texture2.Apply ();
		return texture2;
	}

	public static Texture2D ConvertToBGRA(Texture2D texture){
		Texture2D texture2 = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
		//texture2.SetPixels (texture.GetPixels ());
		//texture2.Apply ();
		//texture2.LoadRawTextureData (texture.GetRawTextureData);
		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.width; y++) {
				Color NewPixel = texture.GetPixel (x, y);
				texture2.SetPixel(x, y, new Color(NewPixel.b, NewPixel.g, NewPixel.r, NewPixel.a));
			}
		}
		texture2.Apply ();

		return texture2;
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

	public static void GetHeaderBGRA(Texture2D texture, ref GetGamedataFile.HeaderClass header)
	{
		header = new GetGamedataFile.HeaderClass();
		header.Format = TextureFormat.BGRA32;
		header.Magic = 542327876;
		header.size = 124;
		header.flags = 4103;
		header.width = (uint)texture.width;
		header.height = (uint)texture.height;

		header.sizeorpitch = 0;
		header.depth = 0;
		header.mipmapcount = 0;
		header.alphabitdepth = 0;
		header.reserved0 = 0;
		header.reserved1 = 0;
		header.reserved2 = 0;
		header.reserved3 = 0;
		header.reserved4 = 0;
		header.reserved5 = 0;
		header.reserved6 = 0;
		header.reserved7 = 0;
		header.reserved8 = 0;
		header.reserved9 = 0;


		header.pixelformatSize = 32;
		header.pixelformatflags = 65;
		header.pixelformatFourcc = 0;
		header.pixelformatRgbBitCount = 32;
		header.pixelformatRbitMask = 16711680;
		header.pixelformatGbitMask = 65280;
		header.pixelformatBbitMask = 255;
		header.pixelformatAbitMask = 4278190080;
		header.caps1 = 4098;
		header.caps2 = 0;
		header.caps3 = 0;
		header.caps4 = 0;
	}

	public static void GetHeaderDxt5(Texture2D texture, ref GetGamedataFile.HeaderClass header)
	{
		header = new GetGamedataFile.HeaderClass();
		header.Format = TextureFormat.BGRA32;
		header.Magic = 542327876;
		header.size = 124;
		header.flags = 4103;
		header.width = (uint)texture.width;
		header.height = (uint)texture.height;

		header.sizeorpitch = 0;
		header.depth = 0;
		header.mipmapcount = 0;
		header.alphabitdepth = 0;
		header.reserved0 = 0;
		header.reserved1 = 0;
		header.reserved2 = 0;
		header.reserved3 = 0;
		header.reserved4 = 0;
		header.reserved5 = 0;
		header.reserved6 = 0;
		header.reserved7 = 0;
		header.reserved8 = 0;
		header.reserved9 = 0;


		header.pixelformatSize = 32;
		header.pixelformatflags = 4;
		header.pixelformatFourcc = 894720068;
		header.pixelformatRgbBitCount = 0;
		header.pixelformatRbitMask = 0;
		header.pixelformatGbitMask = 0;
		header.pixelformatBbitMask = 0;
		header.pixelformatAbitMask = 0;
		header.caps1 = 4098;
		header.caps2 = 0;
		header.caps3 = 0;
		header.caps4 = 0;
	}

	const uint dwReserved2 = 0;
	public static byte[] SaveTextureDDS(Texture2D texture, GetGamedataFile.HeaderClass header)
	{
		// Clean DDS file generator

		//DDS_HEADER
		byte[] finalByteData = BitConverter.GetBytes(header.Magic);

		// Flags
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.size));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.flags));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.height));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.width));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.sizeorpitch));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.depth));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.mipmapcount));

		// Reserved x11
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.alphabitdepth));

		/*for (int i = 0; i < header.reserved.Length; i++){
			finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved[i]));
		}*/

		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved0));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved1));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved2));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved3));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved4));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved5));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved6));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved7));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved8));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved9));


		// Pixel Format
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatSize));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatflags));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatFourcc));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatRgbBitCount));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatRbitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatGbitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatBbitMask));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.pixelformatAbitMask));

		// Caps
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.caps1));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.caps2));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.caps3));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.caps4));
		finalByteData = finalByteData.Concat(BitConverter.GetBytes(dwReserved2));

		// Add raw texture data
		finalByteData = finalByteData.Concat(texture.GetRawTextureData());

		// Return
		return finalByteData;
	}
}
