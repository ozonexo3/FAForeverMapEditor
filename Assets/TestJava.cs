using Ozone;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestJava : MonoBehaviour
{
	//public Texture2D brushTex;

	void Start()
    {

		//biomes.Biomes.list.add(new biomes.Biome());

		/*
		//Load brush texture
		var brush = brushes.Brushes.loadBrush("mountain1.png", new java.lang.Long(123));
		int width = brush.getSize();
		Color[] pixels = new Color[width * width];
		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < width; x++)
			{
				java.lang.Float jVale = brush.getValueAt(x, y) as java.lang.Float;
				float value = jVale.floatValue();
				pixels[x + y * width] = new Color(value, value, value, 1f);
			}
		}
		brushTex = new Texture2D(width, width, TextureFormat.RGBA32, false, false);
		brushTex.SetPixels(pixels);
		brushTex.Apply();
		*/


		Debug.Log("Good brushes count: " + brushes.Brushes.goodBrushes.size());
		for(int i = 0; i < brushes.Brushes.goodBrushes.size(); i++)
		{
			Debug.Log(brushes.Brushes.goodBrushes.get(i).ToString());
		}


		Debug.Log("Biomes count: " + biomes.Biomes.list);
		
		
		//Debug.Log((brushes.Brushes.goodBrushes.get(0) as java.lang.String).ToString());

		//generator.MapGenerator.main(new string[] {"--help"});

		//Debug.Log(generator.MapGenerator.LAND_DENSITY_MAX);


		map.SymmetrySettings customSymmetrySettings = new map.SymmetrySettings(map.Symmetry.DIAG, map.Symmetry.POINT10, map.Symmetry.X);
		java.lang.Long seed = new java.lang.Long(123);
		map.BinaryMask bm = new map.BinaryMask(0, seed, customSymmetrySettings);


	}
}
