using UnityEngine;
using System.Collections;
using CielaSpike;
using System.Threading;

public class GenerateControlTex : MonoBehaviour
{

	public static GenerateControlTex Current;

	void Awake()
	{
		Current = this;
	}

	public static void StopAllTasks()
	{
		if (GeneratingNormalTex)
			Current.StopCoroutine(NormalCoroutine);
		if (GeneratingWaterTex)
			Current.StopCoroutine(WaterCoroutine);
	}

	#region Water

	static bool GeneratingWaterTex
	{
		get
		{
			return WaterCoroutine != null;
		}
	}
	static bool BufforWaterTex = false;
	static Coroutine WaterCoroutine;
	public static void GenerateWater()
	{
		if (GeneratingWaterTex)
		{
			BufforWaterTex = true;
		}
		else
			WaterCoroutine = Current.StartCoroutine(Current.GeneratingWater());
	}

	public IEnumerator GeneratingWater()
	{
		Task task;
		this.StartCoroutineAsync(GeneratingWaterTask(), out task);
		yield return StartCoroutine(task.Wait());

		WaterCoroutine = null;
		yield return null;

		if (BufforWaterTex)
		{
			BufforWaterTex = false;
			WaterCoroutine = Current.StartCoroutine(Current.GeneratingWater());
		}
	}

	public IEnumerator GeneratingWaterTask()
	{

		float WaterHeight = ScmapEditor.Current.map.Water.Elevation * 0.1f;
		if (WaterHeight == 0)
			WaterHeight = 1;
		float WaterDeep = ScmapEditor.Current.map.Water.ElevationAbyss * 0.1f;

		float DeepDifference = (WaterHeight - WaterDeep) / WaterHeight;


		int i = 0;
		int x = 0;
		int y = 0;
		float WaterDepth = 0;

		yield return Ninja.JumpToUnity;
		int Width = ScmapEditor.Current.map.UncompressedWatermapTex.width;
		int Height = ScmapEditor.Current.map.UncompressedWatermapTex.height;
		Color[] AllColors = ScmapEditor.Current.map.UncompressedWatermapTex.GetPixels();
		float[,] HeightmapPixels = ScmapEditor.Current.Teren.terrainData.GetHeights(0, 0, ScmapEditor.Current.Teren.terrainData.heightmapWidth, ScmapEditor.Current.Teren.terrainData.heightmapHeight);
		int HeightmapWidth = ScmapEditor.Current.Teren.terrainData.heightmapWidth - 1;
		int HeightmapHeight = ScmapEditor.Current.Teren.terrainData.heightmapHeight - 1;

		yield return Ninja.JumpBack;

		for (x = 0; x < Width; x++)
		{
			for (y = 0; y < Height; y++)
			{
				i = y + x * Height;

				//WaterDepth = ScmapEditor.Current.Data.GetInterpolatedHeight((x + 0.5f) / (Width + 1f), 1f - (y + 0.5f) / (Height + 1f));
				int LerpX = (int)(HeightmapWidth * (1f - (x) / ((float)Width)));
				int LerpY = (int)(HeightmapHeight * ((y) / ((float)Height)));

				if (LerpX < 0) LerpX = 0;
				else if (LerpX >= HeightmapWidth)
					LerpX = HeightmapWidth - 1;

				if (LerpY < 0) LerpY = 0;
				else if (LerpY >= HeightmapHeight)
					LerpY = HeightmapHeight - 1;

				WaterDepth = HeightmapPixels[LerpX, LerpY] + HeightmapPixels[LerpX + 1, LerpY] + HeightmapPixels[LerpX, LerpY + 1] + HeightmapPixels[LerpX + 1, LerpY + 1];
				WaterDepth /= 4f;
				WaterDepth *= ScmapEditor.TerrainHeight; //16
				//WaterDepth /= 0.1f;


				WaterDepth = (WaterHeight - WaterDepth) / WaterHeight;
				WaterDepth /= DeepDifference;


				AllColors[i] = new Color(AllColors[i].r, Mathf.Clamp01(WaterDepth), (1f - Mathf.Clamp01(WaterDepth * 100f)), 0);
			}
		}

		yield return Ninja.JumpToUnity;

		ScmapEditor.Current.map.UncompressedWatermapTex.SetPixels(AllColors);
		ScmapEditor.Current.map.UncompressedWatermapTex.Apply(false);
	}

	#endregion

	#region Normal

	static Coroutine NormalCoroutine;
	public static void GenerateNormal()
	{
		if (GeneratingNormalTex)
		{
			BufforNormalTex = true;
		}
		else
			NormalCoroutine = Current.StartCoroutine(Current.GeneratingNormal());
	}

	public static void StopGenerateNormal()
	{
		if (GeneratingNormalTex)
		{
			Current.StopCoroutine(NormalCoroutine);
		}
	}


	static bool GeneratingNormalTex
	{
		get
		{
			return NormalCoroutine != null;
		}
	}
	static bool BufforNormalTex = false;

	public IEnumerator GeneratingNormal()
	{
		ScmapEditor.Current.TerrainMaterial.SetFloat("_GeneratingNormal", 1);
		Color[] AllColors = ScmapEditor.Current.map.UncompressedNormalmapTex.GetPixels();

		float Width = ScmapEditor.Current.map.UncompressedNormalmapTex.width;
		float Height = ScmapEditor.Current.map.UncompressedNormalmapTex.height;
		int i = 0;
		int x = 0;
		int y = 0;
		Vector3 Normal;

		//int counter = 0;
		float Realtime = Time.realtimeSinceStartup;
		const float MaxAllowedOverhead = 0.02f;

		for (x = 0; x < Width; x++)
		{
			for (y = 0; y < Height; y++)
			{
				i = x + y * ScmapEditor.Current.map.UncompressedNormalmapTex.width;
				Normal = ScmapEditor.Current.Data.GetInterpolatedNormal((x + 0.5f) / (Width), 1f - (y + 0.5f) / (Height));
				AllColors[i] = new Color(0, 1f - (Normal.z * 0.5f + 0.5f), 0, Normal.x * 0.5f + 0.5f);

				if(Time.realtimeSinceStartup - Realtime > MaxAllowedOverhead)
				{
					yield return null;
					Realtime = Time.realtimeSinceStartup;
				}
				/*
				counter++;
				if (counter > 40000)
				{
					counter = 0;
					yield return null;
				}
				*/
			}
		}

		ScmapEditor.Current.map.UncompressedNormalmapTex.SetPixels(AllColors);
		ScmapEditor.Current.map.UncompressedNormalmapTex.Apply(false);
		ScmapEditor.Current.TerrainMaterial.SetFloat("_GeneratingNormal", 0);
		ScmapEditor.Current.TerrainMaterial.SetTexture("_TerrainNormal", ScmapEditor.Current.map.UncompressedNormalmapTex);


		yield return null;
		NormalCoroutine = null;



		if (BufforNormalTex)
		{
			BufforNormalTex = false;
			GenerateNormal();
		}
	}

	#endregion

	public Texture2D SlopeData;

	public void GenerateSlopeTexture()
	{
		if (SlopeData == null || SlopeData.width != ScmapEditor.Current.map.Width || SlopeData.height != ScmapEditor.Current.map.Height)
		{
			SlopeData = new Texture2D(ScmapEditor.Current.map.Width, ScmapEditor.Current.map.Height, TextureFormat.RGB24, false);
			SlopeData.filterMode = FilterMode.Point;
			SlopeData.wrapMode = TextureWrapMode.Clamp;
		}

		if (GeneratingSlopeTex)
		{
			BufforSlopeTex = true;
		}
		else
		{
			SlopeTask = StartCoroutine(GeneratingSlope());
		}

	}

	static Coroutine SlopeTask;
	static bool GeneratingSlopeTex
	{
		get
		{
			return SlopeTask != null;
		}
	}
	static bool BufforSlopeTex = false;



	static float[,] SlopeHeightmapPixels;

	IEnumerator GeneratingSlope()
	{
		//ScmapEditor.Current.TerrainMaterial.SetFloat("_UseSlopeTex", 0);
		SlopeHeightmapPixels = ScmapEditor.Current.Teren.terrainData.GetHeights(0, 0, ScmapEditor.Current.Teren.terrainData.heightmapWidth, ScmapEditor.Current.Teren.terrainData.heightmapHeight);
		Task task;
		this.StartCoroutineAsync(GeneratingSlopeTask(), out task);
		yield return StartCoroutine(task.Wait());

		SlopeTask = null;

		if (BufforSlopeTex)
		{
			BufforSlopeTex = false;
			SlopeTask = StartCoroutine(GeneratingSlope());
		}
	}

	const float FlatHeight = 0.000045f;
	const float NonFlatHeight = 0.0022f;
	const float AlmostUnpassableHeight = 0.0055f;
	const float UnpassableHeight = 0.01f;

	IEnumerator GeneratingSlopeTask()
	{
		const float Saturation = 0.8f;
		Color Flat = new Color(0.1f * Saturation, 0.78f * Saturation, 0.1f * Saturation, 1);
		Color LowAngle = new Color(0.3f * Saturation, 0.89f * Saturation, 0.1f * Saturation, 1);
		Color HighAngle = new Color(0.6f * Saturation, 0.9f * Saturation, 0.1f * Saturation, 1);
		Color Unpassable = new Color(0.8f * Saturation, 0.05f * Saturation, 0.05f * Saturation, 1);
		Color AlmostUnpassable = new Color(0.7f * Saturation, 0.4f * Saturation, 0.05f * Saturation, 1);

		Color[] Pixels = new Color[ScmapEditor.Current.map.Width * ScmapEditor.Current.map.Height];
		int x = 0;
		int y = 0;
		int i = 0;


		for (x = 0; x < ScmapEditor.Current.map.Width; x++)
		{
			for (y = 0; y < ScmapEditor.Current.map.Height; y++)
			{
				i = y + x * ScmapEditor.Current.map.Height;

				float Slope0 = SlopeHeightmapPixels[x, y];
				float Slope1 = SlopeHeightmapPixels[x + 1, y];
				float Slope2 = SlopeHeightmapPixels[x, y + 1];
				float Slope3 = SlopeHeightmapPixels[x + 1, y + 1];

				float Min = Mathf.Min(Slope0, Slope1, Slope2, Slope3);
				float Max = Mathf.Max(Slope0, Slope1, Slope2, Slope3);

				float Slope = Mathf.Abs(Max - Min) * 2;

				if (Slope < FlatHeight)
					Pixels[i] = Flat;
				else if (Slope < NonFlatHeight)
					Pixels[i] = LowAngle;
				else if (Slope < AlmostUnpassableHeight)
					Pixels[i] = HighAngle;
				else if (Slope < UnpassableHeight)
					Pixels[i] = AlmostUnpassable;
				else
					Pixels[i] = Unpassable;
			}
		}


		yield return Ninja.JumpToUnity;
		SlopeData.SetPixels(Pixels);
		SlopeData.Apply(false);

		ScmapEditor.Current.TerrainMaterial.SetTexture("_SlopeTex", SlopeData);
		ScmapEditor.Current.TerrainMaterial.SetFloat("_UseSlopeTex", 1);
		//yield return null;
	}

}
