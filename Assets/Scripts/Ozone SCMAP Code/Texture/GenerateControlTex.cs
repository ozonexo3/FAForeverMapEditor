using UnityEngine;
using System.Collections;

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
		if(GeneratingWaterTex)
			Current.StopCoroutine(WaterCoroutine);
	}

	#region Water

	static bool GeneratingWaterTex = false;
	static bool BufforWaterTex = false;
	static Coroutine WaterCoroutine;
	public static void GenerateWater(){
		if (GeneratingWaterTex)
		{
			BufforWaterTex = true;
		}
		else
			WaterCoroutine = Current.StartCoroutine(Current.GeneratingWater());
	}

	public IEnumerator GeneratingWater()
	{
		GeneratingWaterTex = true;
		Color[] AllColors = ScmapEditor.Current.map.UncompressedWatermapTex.GetPixels();

		float WaterHeight = ScmapEditor.Current.map.Water.Elevation * 0.1f;
		if (WaterHeight == 0)
			WaterHeight = 1;
		float WaterDeep = ScmapEditor.Current.map.Water.ElevationAbyss * 0.1f;

		float DeepDifference = (WaterHeight - WaterDeep) / WaterHeight;

		int Width = ScmapEditor.Current.map.UncompressedWatermapTex.width;
		int Height = ScmapEditor.Current.map.UncompressedWatermapTex.height;
		int i = 0;
		int x = 0;
		int y = 0;
		float WaterDepth = 0;
		int counter = 0;

		for (x = 0; x < Width; x++)
		{
			for (y = 0; y < Height; y++)
			{
				i = x + y * Width;

				WaterDepth = ScmapEditor.Current.Data.GetInterpolatedHeight((x + 0.5f) / (Width + 1f), 1f - (y + 0.5f) / (Height + 1f));

				WaterDepth = (WaterHeight - WaterDepth) / WaterHeight;
				WaterDepth /= DeepDifference;

				AllColors[i] = new Color(AllColors[i].r, Mathf.Clamp01(WaterDepth), (1f - Mathf.Clamp01(WaterDepth * 100f)), 0);

				counter++;
				if (counter > 40000)
				{
					counter = 0;
					yield return null;
				}
			}
		}

		ScmapEditor.Current.map.UncompressedWatermapTex.SetPixels(AllColors);
		ScmapEditor.Current.map.UncompressedWatermapTex.Apply(false);

		yield return null;
		GeneratingWaterTex = false;

		if (BufforWaterTex)
		{
			BufforWaterTex = false;
			Current.StartCoroutine(Current.GeneratingWater());
		}
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




	static bool GeneratingNormalTex = false;
	static bool BufforNormalTex = false;

	public IEnumerator GeneratingNormal()
	{
		GeneratingNormalTex = true;
		ScmapEditor.Current.TerrainMaterial.SetFloat("_GeneratingNormal", 1);
		Color[] AllColors = ScmapEditor.Current.map.UncompressedNormalmapTex.GetPixels();

		float Width = ScmapEditor.Current.map.UncompressedNormalmapTex.width;
		float Height = ScmapEditor.Current.map.UncompressedNormalmapTex.height;
		int i = 0;
		int x = 0;
		int y = 0;
		Vector3 Normal;

		int counter = 0;

		for (x = 0; x < Width; x++)
		{
			for (y = 0; y < Height; y++)
			{
				i = x + y * ScmapEditor.Current.map.UncompressedNormalmapTex.width;
				Normal = ScmapEditor.Current.Data.GetInterpolatedNormal((x + 0.5f) / (Width), 1f - (y + 0.5f) / (Height));

				AllColors[i] = new Color(0, 1f - (Normal.z * 0.5f + 0.5f), 0, Normal.x * 0.5f + 0.5f);

				counter++;
				if(counter > 40000)
				{
					counter = 0;
					yield return null;
				}

			}
			
		}

		ScmapEditor.Current.map.UncompressedNormalmapTex.SetPixels(AllColors);
		ScmapEditor.Current.map.UncompressedNormalmapTex.Apply(false);

		yield return null;
		GeneratingNormalTex = false;
		ScmapEditor.Current.TerrainMaterial.SetFloat("_GeneratingNormal", 0);

		if (BufforNormalTex)
		{
			BufforNormalTex = false;
			Current.StartCoroutine(Current.GeneratingNormal());
		}
	}

	#endregion

	public Texture2D SlopeData;

	public void GenerateSlopeTexture()
	{
		if(SlopeData == null || SlopeData.width != ScmapEditor.Current.map.Width || SlopeData.height != ScmapEditor.Current.map.Height)
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
			StartCoroutine(GeneratingSlope());
		}

	}

	static bool GeneratingSlopeTex = false;
	static bool BufforSlopeTex = false;

	public float FlatHeight = 0.000045f;
	public float NonFlatHeight = 0.0022f;
	public float UnpassableHeight = 0.005f;

	IEnumerator GeneratingSlope()
	{
		GeneratingSlopeTex = true;
		//ScmapEditor.Current.TerrainMaterial.SetFloat("_UseSlopeTex", 0);

		Color Flat = new Color(0, 0.8f, 0, 1);
		Color LowAngle = new Color(0.3f, 0.89f, 0, 1);
		Color HighAngle = new Color(0.6f, 0.9f, 0, 1);
		Color Unpassable = new Color(0.9f, 0, 0, 1);

		Color[] Pixels = new Color[ScmapEditor.Current.map.Width * ScmapEditor.Current.map.Height];
		float[,] HeightmapPixels = ScmapEditor.Current.Teren.terrainData.GetHeights(0, 0, ScmapEditor.Current.Teren.terrainData.heightmapWidth, ScmapEditor.Current.Teren.terrainData.heightmapHeight);

		int x = 0;
		int y = 0;
		int i = 0;
		int counter = 0;


		for (x = 0; x < ScmapEditor.Current.map.Width; x++)
		{
			for (y = 0; y < ScmapEditor.Current.map.Height; y++)
			{
				i = y + x * ScmapEditor.Current.map.Height;

				float Slope0 = HeightmapPixels[x, y];
				float Slope1 = HeightmapPixels[x + 1, y];
				float Slope2 = HeightmapPixels[x, y + 1];
				float Slope3 = HeightmapPixels[x + 1, y + 1];

				float Min = Mathf.Min(Slope0, Slope1, Slope2, Slope3);
				float Max = Mathf.Max(Slope0, Slope1, Slope2, Slope3);

				float Slope = Mathf.Abs(Max - Min);

				//Pixels[i] = Color.Lerp(Color.black, Color.white, Slope / 0.008f);


				if (Slope < FlatHeight)
					Pixels[i] = Flat;
				else if (Slope < NonFlatHeight)
					Pixels[i] = LowAngle;
				else if (Slope < UnpassableHeight)
					Pixels[i] = HighAngle;
				else
					Pixels[i] = Unpassable;

				counter++;
				if (counter > 80000)
				{
					counter = 0;
					yield return null;
				}
			}
		}

		SlopeData.SetPixels(Pixels);
		SlopeData.Apply(false);


		ScmapEditor.Current.TerrainMaterial.SetTexture("_SlopeTex", SlopeData);
		ScmapEditor.Current.TerrainMaterial.SetFloat("_UseSlopeTex", 1);
		yield return null;
		GeneratingSlopeTex = false;


		if (BufforSlopeTex)
		{
			BufforSlopeTex = false;
			StartCoroutine(GeneratingSlope());
		}
	}



}
