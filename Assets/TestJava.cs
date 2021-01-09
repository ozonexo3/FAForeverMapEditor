using Ozone;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using neroxis;
using neroxis.biomes;
using neroxis.brushes;
using neroxis.generator;

public class TestJava : MonoBehaviour
{
	//public Texture2D brushTex;

	void OnEnable()
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

		/*long seed = 123;
		java.lang.Long javaSeed = new java.lang.Long(seed);
		java.util.Random random = new java.util.Random(seed);


		string resource = Biomes.BIOMES_LIST.get(random.nextInt(Biomes.BIOMES_LIST.size())).ToString();

		neroxis.map.TerrainMaterials terrainMaterials = new neroxis.map.TerrainMaterials();

		Biome randomBiome = Biomes.loadBiome(resource);

		Debug.Log("Generated random biome: " + randomBiome.getName());

		neroxis.map.TerrainMaterials biomeTerrainMaterials = randomBiome.getTerrainMaterials();

		string[] texturePaths = biomeTerrainMaterials.getTexturePaths();
		string[] normalPaths = biomeTerrainMaterials.getNormalPaths();
		float[] textureScales = biomeTerrainMaterials.getTextureScales();
		float[] normalScales = biomeTerrainMaterials.getNormalScales();
		Debug.Log("Biome layer cont: " + texturePaths.Length);
		for (int i = 0; i < texturePaths.Length; i++)
		{
			if(i < normalPaths.Length)
				Debug.Log(i + ": " + texturePaths[i] + "(" + textureScales[i] + "), " + normalPaths[i] + "(" + normalScales[i] + ")");
			else
				Debug.Log(i + ": " + texturePaths[i] + "(" + textureScales[i] + ")");
		}*/

		//Debug.Log((brushes.Brushes.goodBrushes.get(0) as java.lang.String).ToString());

		//MapGenerator.main(new string[] {"--no-hash", "--debug" }); //"--debug", 

		/*MapGenerator generator = new MapGenerator();
		generator.interpretArguments(new string[] { });
		neroxis.map.SCMap map = generator.generate();

		Debug.Log(map.getHeightmap().getWidth() + " x " + map.getHeightmap().getHeight());*/

		GenerateMapTask.GenerateSCMP(new MapGeneratorSettings() { width = 256, seed = 123, mexCount = 0, reclaimDensity = 0 }, OnFinishSCMPgenerator);

		//Debug.Log(generator.MapGenerator.LAND_DENSITY_MAX);


		/*map.SymmetrySettings customSymmetrySettings = new map.SymmetrySettings(map.Symmetry.DIAG, map.Symmetry.POINT10, map.Symmetry.X);
		
		map.BinaryMask bm = new map.BinaryMask(0, seed, customSymmetrySettings);*/

	}

	void OnFinishSCMPgenerator()
	{

	}
}
