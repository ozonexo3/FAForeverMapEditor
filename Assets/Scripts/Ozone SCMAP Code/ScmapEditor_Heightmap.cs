// ***************************************************************************************
// * SCmap editor
// * Set Unity objects and scripts using data loaded from Scm
// ***************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScmapEditor : MonoBehaviour
{

	static int heightsLength;
	static float[,] heights = new float[1, 1];

	public static void ApplyHeightmap(bool delayed = true)
	{
		if (delayed)
			Current.Data.SetHeightsDelayLOD(0, 0, heights);
		else
		{
			Current.Data.SetHeights(0, 0, heights);
			Current.Teren.Flush();
		}
	}

	public static void GetAllHeights(ref float[,] destArray)
	{
		int width = heights.GetLength(0);
		int height = heights.GetLength(1);

		if (destArray == null || destArray.GetLength(0) != width || destArray.GetLength(1) != height)
		{
			destArray = new float[width, height];
		}

		for(int x = 0; x < width; x++)
		{
			for (int y = 0; y < width; y++)
			{
				destArray[x, y] = heights[x, y];
			}
		}
		ApplyHeightmap(false);
	}

	public static void ClampTop(float MaxHeight)
	{
		int width = heights.GetLength(0);
		int height = heights.GetLength(1);

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (heights[x, y] > MaxHeight)
					heights[x, y] = MaxHeight;
			}
		}
		ApplyHeightmap(false);
	}

	public static void ClampBottom(float MinHeight)
	{
		int width = heights.GetLength(0);
		int height = heights.GetLength(1);

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (heights[x, y] < MinHeight)
					heights[x, y] = MinHeight;
			}
		}
		ApplyHeightmap(false);
	}

	public static void SetAllHeights(float[,] newHeights)
	{
		SetHeights(0, 0, newHeights, false);
	}

	public static void SetHeights(int X, int Y, float[,] values, bool delayed = true)
	{
		int width = values.GetLength(1);
		int height = values.GetLength(0);

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				heights[X + x, Y + y] = values[x, y];
			}
		}

		if (delayed)
			Current.Data.SetHeightsDelayLOD(X, Y, values);
		else
		{
			Current.Data.SetHeights(X, Y, values);
			Current.Teren.Flush();
		}
	}

	public static void ApplyChanges(int X, int Y)
	{
		if (X >= heightsLength || Y >= heightsLength)
			return;

		int x = 0;
		int y = 0;
		for (x = 0; x < LastGetWidth; x++)
		{
			for (y = 0; y < LastGetHeight; y++)
			{
				int hx = Y + x;
				int hy = X + y;
				if (hx >= heightsLength || hy >= heightsLength)
					continue;

				heights[hx, hy] = ReturnValues[x, y];
			}
		}

		if(X + ReturnValues.GetLength(0) >= heightsLength || Y + ReturnValues.GetLength(1) >= heightsLength)
		{
			if (X >= heightsLength || Y >= heightsLength)
			{
				ApplyHeightmap(true);
			}
			else
			{
				/*
				int LeftX = ReturnValues.GetLength(0);
				int LeftY = ReturnValues.GetLength(1);

				if(X + ReturnValues.GetLength(0) >= heightsLength)
					LeftX = heightsLength - X;

				if (Y + ReturnValues.GetLength(1) >= heightsLength)
					LeftY = heightsLength - Y;



				Debug.Log(heightsLength + " / " + X);
				Debug.Log(heightsLength + " / " + Y);
				Debug.Log(LeftX + " / " + LeftY);
				Debug.Log(ReturnValues.GetLength(0) + " / " + ReturnValues.GetLength(1));
				float[,] NewReturnValues = new float[LeftX, LeftY];
				for (x = 0; x < LeftX; x++)
				{
					for (y = 0; y < LeftY; y++)
					{
						NewReturnValues[x, y] = ReturnValues[x, y];
					}
				}
				*/
				Current.Data.SetHeightsDelayLOD(X, Y, ReturnValues);
			}

		}
		else
			Current.Data.SetHeightsDelayLOD(X, Y, ReturnValues);
		//ApplyHeightmap();
	}

	public static void SetHeight(int x, int y, float value)
	{
		heights[x, y] = value;
	}



	public static float GetHeight(int x, int y)
	{
		return heights[x, y];
	}

	public static float[,] ReturnValues;
	public static int LastGetWidth = 0;
	public static int LastGetHeight = 0;
	public static float[,] GetValues(int X, int Y, int Width, int Height)
	{
		if(Width != LastGetWidth || Height != LastGetHeight)
		{
			ReturnValues = new float[Width, Height];
			LastGetWidth = Width;
			LastGetHeight = Height;
		}

		int x = 0;
		int y = 0;

		for(x = 0; x < LastGetWidth; x++)
		{
			for(y = 0; y < LastGetHeight; y++)
			{
				int hx = Y + x;
				int hy = X + y;
				if (hx >= heightsLength || hy >= heightsLength)
					continue;

				ReturnValues[x, y] = heights[hx, hy];
			}
		}

		//ReturnValues = Current.Data.GetHeights(X, Y, Width, Height);

		return ReturnValues;
	}

	public static void SyncHeightmap(bool Flush = false)
	{
		//Teren.ApplyDelayedHeightmapModification();
		Current.Data.SyncHeightmap();
		if(Flush)
			Current.Teren.Flush();
	}

	public static bool IsOverMinMaxDistance()
	{
		float dist = Current.Teren.terrainData.bounds.max.y - Current.Teren.terrainData.bounds.min.y;
		return dist > 5f;

	}
}
