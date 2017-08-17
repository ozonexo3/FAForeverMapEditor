using UnityEngine;
using System.Collections;

public class GenerateControlTex : MonoBehaviour
{

	public static GenerateControlTex Current;

	void Awake()
	{
		Current = this;
	}

	public static void GenerateWater(ref Texture2D WaterTex){
		return;

		Color[] AllColors = WaterTex.GetPixels ();

		float WaterHeight = ScmapEditor.Current.map.Water.Elevation * 0.1f;
		if (WaterHeight == 0)
			WaterHeight = 1;
		float WaterDeep = ScmapEditor.Current.map.Water.ElevationAbyss * 0.1f;

		float DeepDifference = (WaterHeight - WaterDeep) / WaterHeight;

		float Width = WaterTex.width;
		float Height = WaterTex.height;
		int i = 0;
		int x = 0;
		int y = 0;
		float WaterDepth = 0;

		for (x = 0; x < Width; x++) {
			for (y = 0; y < Height; y++) {
				i = x + y * WaterTex.width;

				WaterDepth = ScmapEditor.Current.Data.GetInterpolatedHeight ((x + 0.5f) / (Width + 1), 1f - (y + 0.5f) / (Height + 1));

				WaterDepth = (WaterHeight - WaterDepth) / WaterHeight;
				WaterDepth /= DeepDifference;

				AllColors [i] = new Color (AllColors [i].r, Mathf.Clamp01 (WaterDepth), (1f - Mathf.Clamp01(WaterDepth * 100f)) , 0);
			}
		}

		WaterTex.SetPixels(AllColors) ;
		WaterTex.Apply (false);
	}

	public static void GenerateNormal(ref Texture2D NormalTexture)
	{
		/*Color[] AllColors = NormalTexture.GetPixels();

		float Width = NormalTexture.width;
		float Height = NormalTexture.height;
		int i = 0;
		int x = 0;
		int y = 0;
		Vector3 Normal;

		for (x = 0; x < Width; x++)
		{
			for (y = 0; y < Height; y++)
			{
				i = x + y * NormalTexture.width;
				Normal = ScmapEditor.Current.Data.GetInterpolatedNormal((x + 0.5f) / (Width), 1f - (y + 0.5f) / (Height));
				
				AllColors[i] = new Color(0, 1f - (Normal.z * 0.5f + 0.5f), 0, Normal.x * 0.5f + 0.5f);
			}
		}

		NormalTexture.SetPixels(AllColors);
		NormalTexture.Apply(false);*/

		if (GeneratingNormalTex)
		{
			BufforNormalTex = true;
		}
		else
			Current.StartCoroutine(Current.GeneratingNormal());
	}


	static bool GeneratingNormalTex = false;
	static bool BufforNormalTex = false;

	public IEnumerator GeneratingNormal()
	{
		GeneratingNormalTex = true;
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
				if(counter > 100000)
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

		if (BufforNormalTex)
		{
			BufforNormalTex = false;
			Current.StartCoroutine(Current.GeneratingNormal());
		}
	}

}
