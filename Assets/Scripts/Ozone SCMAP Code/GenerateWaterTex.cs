using UnityEngine;
using System.Collections;

public class GenerateWaterTex : MonoBehaviour {

	public static void Generate(ref Texture2D WaterTex, ScmapEditor Map){
		Color[] AllColors = WaterTex.GetPixels ();

		float WaterHeight = Map.map.Water.Elevation * 0.1f;
		if (WaterHeight == 0)
			WaterHeight = 1;
		float WaterDeep = Map.map.Water.ElevationAbyss * 0.1f;

		float DeepDifference = (WaterHeight - WaterDeep) / WaterHeight;

		float Width = WaterTex.width;
		float Height = WaterTex.height;
		int i = 0;
		int x = 0;
		int y = 0;
		float WaterDepth = 0;

		for (x = 0; x < WaterTex.width; x++) {
			for (y = 0; y < WaterTex.height; y++) {
				//int i = x + y * WaterTex.width;
				i = x + y * WaterTex.width;
				//i++;

				WaterDepth = Map.Data.GetInterpolatedHeight (x / Width, 1f - y / Height);

				WaterDepth = (WaterHeight - WaterDepth) / WaterHeight;
				WaterDepth /= DeepDifference;

				AllColors [i] = new Color (AllColors [i].r, Mathf.Clamp01 (WaterDepth), (1f - Mathf.Clamp01(WaterDepth * 100f)) , 0);
			}
		}

		WaterTex.SetPixels(AllColors) ;
		WaterTex.Apply ();
	}
}
