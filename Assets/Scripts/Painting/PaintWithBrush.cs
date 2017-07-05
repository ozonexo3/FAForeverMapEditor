using UnityEngine;
using System.Collections;

public class PaintWithBrush : MonoBehaviour
{

	[System.Serializable]
	public class BrushData
	{
		public float size;
		public float strength;
		public Vector2 MinMax;
		//public Vector2 Pos;
		//public Texture2D[] values;
		public BrushTypes BrushType;
		public bool Invert;
	}

	public enum BrushTypes
	{
		Standard, Smooth, Sharpen
	}

	public static void PaintWithSymmetry(ref float[,] texture, BrushData brush)
	{
		for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
		{
			Paint(ref texture, brush, i);
		}
	}

	static int hmWidth = 1;
	static int hmHeight = 1;
	static int posXInTerrain;
	static int posYInTerrain;
	static float PixelPower = 0;
	static int offset;

	static int i = 0;
	static int j = 0;
	static int tx = 0;
	static int ty = 0;
	static int x = 0;
	static int y = 0;
	static Color BrushValue;
	static float SambleBrush;

	public static void Paint(ref float[,] texture, BrushData brush, int brushId = 0)
	{
		hmWidth = texture.GetLength(0);
		hmHeight = texture.GetLength(1);

		posXInTerrain = (int)(BrushGenerator.Current.PaintPositions[brushId].x * hmWidth);
		posYInTerrain = (int)(BrushGenerator.Current.PaintPositions[brushId].z * hmHeight);
		offset = (int)(brush.size / 2);


		// Horizontal Brush Offsets
		int OffsetLeft = 0;
		if (posXInTerrain - offset < 0) OffsetLeft = Mathf.Abs(posXInTerrain - offset);
		int OffsetRight = 0;
		if (posXInTerrain - offset + brush.size > hmWidth) OffsetRight = (int)(posXInTerrain - offset + brush.size - hmWidth);

		// Vertical Brush Offsets
		int OffsetDown = 0;
		if (posYInTerrain - offset < 0) OffsetDown = Mathf.Abs(posYInTerrain - offset);
		int OffsetTop = 0;
		if (posYInTerrain - offset + brush.size > hmHeight) OffsetTop = (int)(posYInTerrain - offset + brush.size - hmHeight);


		//float[,] heights = Map.Teren.terrainData.GetHeights(posXInTerrain-offset + OffsetLeft, posYInTerrain-offset + OffsetDown ,(brushSize - OffsetLeft) - OffsetRight, (brushSize - OffsetDown) - OffsetTop);
		float CenterHeight = 0;

		if (brush.BrushType == BrushTypes.Smooth || brush.BrushType == BrushTypes.Sharpen)
		{
			for (i = 0; i < (brush.size - OffsetDown) - OffsetTop; i++)
			{
				for (j = 0; j < (brush.size - OffsetLeft) - OffsetRight; j++)
				{
					tx = i + posXInTerrain - offset + OffsetLeft;
					ty = j + posYInTerrain - offset + OffsetDown;
					CenterHeight += texture[tx, ty];
				}
			}
			CenterHeight /= brush.size * brush.size;
		}

		for (i = 0; i < (brush.size - OffsetDown) - OffsetTop; i++)
		{
			for (j = 0; j < (brush.size - OffsetLeft) - OffsetRight; j++)
			{
				tx = i + posXInTerrain - offset + OffsetLeft;
				ty = j + posYInTerrain - offset + OffsetDown;

				// Brush strength
				x = (int)(((i + OffsetDown) / (float)brush.size) * BrushGenerator.Current.PaintImage[brushId].width);
				y = (int)(((j + OffsetLeft) / (float)brush.size) * BrushGenerator.Current.PaintImage[brushId].height);
				BrushValue = BrushGenerator.Current.PaintImage[brushId].GetPixel(y, x);
				SambleBrush = BrushValue.r;
				if (SambleBrush >= 0.02f)
				{
					switch (brush.BrushType)
					{
						case BrushTypes.Standard:
							texture[tx, ty] += SambleBrush * brush.strength * 0.0002f * (brush.Invert ? (-1) : 1);
							break;
						case BrushTypes.Smooth:
							PixelPower = Mathf.Abs(texture[tx, ty] - CenterHeight);
							texture[tx, ty] = Mathf.Lerp(texture[tx, ty], CenterHeight, brush.strength * 0.4f * Mathf.Pow(SambleBrush, 2) * PixelPower);
							break;
						case BrushTypes.Sharpen:
							PixelPower = texture[tx, ty] - CenterHeight;
							texture[tx, ty] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * brush.strength * 0.01f * Mathf.Pow(SambleBrush, 2);
							break;
					}

					texture[tx, ty] = Mathf.Clamp(texture[tx, ty], brush.MinMax.x, brush.MinMax.y);
				}
			}
		}

	}
}
