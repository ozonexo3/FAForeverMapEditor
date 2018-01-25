// ***************************************************************************************
// * SCmap editor
// * Set Unity objects and scripts using data loaded from Scm
// ***************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScmapEditor : MonoBehaviour
{

	static float[,] heights = new float[1, 1];

	public static void ApplyHeightmap()
	{
		Current.Data.SetHeightsDelayLOD(0, 0, heights);
	}

	public static void SetHeights(int X, int Y, float[,] values)
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

		Current.Data.SetHeightsDelayLOD(X, Y, values);
	}

	public static void ApplyChanges(int X, int Y)
	{
		int x = 0;
		int y = 0;
		for (x = 0; x < LastGetWidth; x++)
		{
			for (y = 0; y < LastGetHeight; y++)
			{
				heights[Y + x, X + y] = ReturnValues[x, y];
			}
		}

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
	static int LastGetWidth = 0;
	static int LastGetHeight = 0;
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
				ReturnValues[x, y] = heights[Y + x, X + y];
			}
		}

		//ReturnValues = Current.Data.GetHeights(X, Y, Width, Height);

		return ReturnValues;
	}
}
