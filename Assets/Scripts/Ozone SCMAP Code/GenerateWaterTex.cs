using UnityEngine;
using System.Collections;

public class GenerateWaterTex : MonoBehaviour {

	public static void Generate(ref Texture2D WaterTex, ScmapEditor Map){
		Color[] AllColors = WaterTex.GetPixels ();

		float WaterHeight = Map.map.Water.Elevation;
		if (WaterHeight == 0)
			WaterHeight = 1;
		float WaterDeep = Map.map.Water.ElevationDeep / WaterHeight;

		for (int x = 0; x < WaterTex.width; x++) {
			for (int y = 0; y < WaterTex.height; y++) {
				int i = x + y * WaterTex.width;

				float WaterDepth = (WaterHeight - Map.Data.GetInterpolatedHeight (1f - x / (float) WaterTex.width, y / (float)WaterTex.height)) / WaterHeight;
				WaterDepth = Mathf.Clamp01 (WaterDepth - WaterDeep / WaterHeight);

				AllColors [i] = new Color (AllColors [i].r, WaterDepth, (1f - Mathf.Clamp01(WaterDepth * 100f)) , 0);
			}
		}

		WaterTex.SetPixels(AllColors) ;
		WaterTex.Apply ();
	}
}
