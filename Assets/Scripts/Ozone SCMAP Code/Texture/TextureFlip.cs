using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TextureFlip {

	public static Texture2D FlipTextureHorizontal(Texture2D original, bool mipmaps = false)
	{
		Texture2D flipped = new Texture2D(original.width, original.height, original.format, mipmaps);

		int xN = original.width;
		int yN = original.height;


		for (int i = 0; i < xN; i++)
		{
			for (int j = 0; j < yN; j++)
			{
				flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
			}
		}
		flipped.Apply(mipmaps);

		return flipped;
	}

	public static Texture2D FlipTextureVertical(Texture2D original, bool mipmaps = false)
	{
		Texture2D flipped = new Texture2D(original.width, original.height, original.format, mipmaps);

		int xN = original.width;
		int yN = original.height;


		for (int i = 0; i < xN; i++)
		{
			for (int j = 0; j < yN; j++)
			{
				flipped.SetPixel(i, yN - 1 - j, original.GetPixel(i, j));
			}
		}
		flipped.Apply(mipmaps);

		return flipped;
	}
}
