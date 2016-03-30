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

	public static T[] Concat<T>(this T[] x, T[] y)
	{
		if (x == null) throw new ArgumentNullException("x");
		if (y == null) throw new ArgumentNullException("y");
		int oldLen = x.Length;
		Array.Resize<T>(ref x, x.Length + y.Length);
		Array.Copy(y, 0, x, oldLen, y.Length);
		return x;
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
		for(int i = 0; i < header.reserved.Length; i++){
			finalByteData = finalByteData.Concat(BitConverter.GetBytes(header.reserved[i]));
		}

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
