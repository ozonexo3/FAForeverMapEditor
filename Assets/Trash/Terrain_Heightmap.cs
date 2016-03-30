using UnityEngine;
using System.Collections;
//using Vectrosity;

public class Terrain_Heightmap : MonoBehaviour 
{
	public static Terrain_Heightmap instance;
	public Material material;
	public float terrainModifySpeed = 0.2f;
	Vector3 size;// = new Vector3(16, 30, 16);
	public int brushSize = 2;
	int width = 128;
	int height = 128;
	public int chunkSize = 16;
	public int chunkNum = 8;
	GameObject[,] chunks;
//	GameObject waterController;
	
	public float[,] density;
	public Color32[,] terrainColor;

	public Light sun;

	//static Color32 grass = new Color32(0,50,0,255);
	Map map;
	
	void Awake()
	{
		instance = this;
	}

	//Simple debug print out of all the map details...
	void printMapDebug(Map map)
	{
		Debug.Log("FA Map Information");
		Debug.Log("----------------------------------------------------------------");
	//	Debug.Log("    Random Number Seed: " + randomSeed);
		Debug.Log("    Dimensions: " + map.Width + "x" + map.Height);
		Debug.Log("    Map Data Version: " + map.VersionMajor + "." + map.VersionMinor);
		Debug.Log("    Height Scale: " + map.HeightScale);
		Debug.Log("    Decal Count: " + map.Decals.Count);
		Debug.Log("    Prop Count: " + map.Props.Count);
		Debug.Log("    Wave Generator Count: " + map.WaveGenerators.Count);
		
		Debug.Log("");
		Debug.Log("    Lighting");
		Debug.Log("        Terrain Shader: " + map.TerrainShader);
		Debug.Log("        Background: " + map.TexPathBackground);
		Debug.Log("        Sky Cube: " + map.TexPathSkyCubemap);
		Debug.Log("        Enviroment Lookup Textures");
		for (int i = 0; i <= map.EnvCubemapsFile.Length - 1; i++)
		{
			Debug.Log("            Texture " + (i + 1).ToString());
			Debug.Log("                Texture Label: " + map.EnvCubemapsName[i]);
			Debug.Log("                Texture Path: " + map.EnvCubemapsFile[i]);
			Debug.Log("");
		}
		Debug.Log("        Light Direction: RA=" + (180 / Mathf.PI) * Mathf.Acos(map.SunDirection.z) + " Dec=" + (180 / Mathf.PI) * Mathf.Atan2(map.SunDirection.y, map.SunDirection.x) + " Vector=" + map.SunDirection.ToString());
		Debug.Log("        Multiplier: " + map.LightingMultiplier);
		Debug.Log("        Light Color: R=" + map.SunColor.x + " G=" + map.SunColor.y + " B=" + map.SunColor.z);
		Debug.Log("        Ambient Light Color: R=" + map.SunAmbience.x + " G=" + map.SunAmbience.y + " B=" + map.SunAmbience.z);
		Debug.Log("        Shadow Color: R=" + map.ShadowFillColor.x + " G=" + map.ShadowFillColor.y + " B=" + map.ShadowFillColor.z);
		Debug.Log("        Specular: ");
		Debug.Log("        Glow: ");
		Debug.Log("        Bloom: " + map.Bloom);
		Debug.Log("        Fog Color: R=" + map.FogColor.x + " G=" + map.FogColor.y + " B=" + map.FogColor.z);
		Debug.Log("        Fog Start: " + map.FogStart);
		Debug.Log("        Fog End: " + map.FogEnd);
		Debug.Log("    Water");
		Debug.Log("        Enabled: " + map.Water.HasWater);
		Debug.Log("        Surface Elevation: " + map.Water.Elevation);
		Debug.Log("        Deep Elevation: " + map.Water.ElevationDeep);
		Debug.Log("        Abyss Elevation: " + map.Water.ElevationAbyss);
		Debug.Log("        Reflected Sun Color: R=" + map.Water.SunColor.x + " G=" + map.Water.SunColor.y + " B=" + map.Water.SunColor.z);
		Debug.Log("        Water Surface Color: R=" + map.Water.SurfaceColor.x + " G=" + map.Water.SurfaceColor.y + " B=" + map.Water.SurfaceColor.z);
		Debug.Log("        Color Lerp: Max=" + map.Water.ColorLerp.y + " Min=" + map.Water.ColorLerp.x);
		Debug.Log("        Sun Reflection: " + map.Water.SunReflection);
		Debug.Log("        Sky Reflection: " + map.Water.SkyReflection);
		Debug.Log("        Unit Reflection: " + map.Water.UnitReflection);
		Debug.Log("        Refraction: " + map.Water.RefractionScale);
		Debug.Log("        Sun Shininess: " + map.Water.SunShininess);
		Debug.Log("        Sun Strength: " + map.Water.SunStrength);
		Debug.Log("        Sun Glow: " + map.Water.SunGlow);
		Debug.Log("        Fresnel Bias: " + map.Water.FresnelBias);
		Debug.Log("        Fresnel Power: " + map.Water.FresnelPower);
		Debug.Log("        Texture-Environment: " + map.Water.TexPathCubemap);
		Debug.Log("        Texture-Water Ramp: " + map.Water.TexPathWaterRamp);
		Debug.Log("        Wave Normals");
		for (int i = 0; i <= map.Water.WaveTextures.Length - 1; i++)
		{
			Debug.Log("            Wave Normal " + (i + 1).ToString());
			Debug.Log("                Texture: " + map.Water.WaveTextures[i].TexPath);
			Debug.Log("                Direction Vector: X=" + map.Water.WaveTextures[i].NormalMovement.x + " " + map.Water.WaveTextures[i].NormalMovement.y);
			Debug.Log("                Frequency: " + map.Water.WaveTextures[i].NormalRepeat);
			Debug.Log("");
		}
		
		Debug.Log("    Stratum");
		for (int i = 0; i <= map.Layers.Count - 1; i++)
		{
			Layer stratum = map.Layers[i];
			Debug.Log("        Stratum " + (i + 1).ToString());
			Debug.Log("                Texture Path: " + stratum.PathTexture);
			Debug.Log("                Texture Scale: " + stratum.ScaleTexture);
			Debug.Log("                Normal Map Path: " + stratum.PathNormalmap);
			Debug.Log("                Normal Map Scale: " + stratum.ScaleNormalmap);
			Debug.Log("");
		}
		
		Debug.Log("    Unknown Settings");
		Debug.Log("        Unknown Value 7:" + map.Unknown7);
		Debug.Log("        Unknown Value 8:" + map.Unknown8);
		Debug.Log("        Unknown Value 10:" + map.Unknown10);
		Debug.Log("        Unknown Value 11:" + map.Unknown11);
		Debug.Log("        Unknown Value 12:" + map.Unknown12);
		Debug.Log("        Unknown Value 13:" + map.Unknown13);
		Debug.Log("        Unknown Value 14:" + map.Unknown14);
	}

	public void loadMap(string filePath)
	{
		map = new Map();
		if(map.Load(filePath))
		{
			printMapDebug(map);
			sun.transform.rotation = Quaternion.Euler(map.SunDirection);
			sun.color = new Color(map.SunColor.x, map.SunColor.y, map.SunColor.z,255);
			//This assumes SunAmbience means the color of the ambient light.. might not be the case.. change if it looks odd when textures are in.
			RenderSettings.ambientLight = new Color(map.SunAmbience.x, map.SunAmbience.y, map.SunAmbience.z,255);
			/*System.IO.FileStream file = new System.IO.FileStream("C:/Users/Vybe/Desktop/test.dds",System.IO.FileMode.Create);
			BinaryWriter writer = new BinaryWriter(file);
			writer.Close();*/
		}
		else
		{
			Debug.Log("File not found");
		}
	}

	void Start()
	{
		loadMap("maps/SCMP_009/SCMP_009.scmap");
		chunkNum = map.Width/16;
		width = chunkSize * chunkNum;
		height = chunkSize * chunkNum;
		size = new Vector3(chunkSize,map.HeightScale,chunkSize);

		density = new float[width,height];
		terrainColor = new Color32[width,height];
		chunks = new GameObject[chunkNum,chunkNum];
		for(int x = 0; x < width; x++)
		{
			for(int z = 0; z < height; z++)
			{
				density[x,z] = map.GetHeight(width-1-x,z);// * map.HeightScale;
			}
		}
		/*
		for(int x = 0; x < width; x++)
		{
			for(int z = 0; z < height; z++)
			{
				float n;
				//n = noise.getValue(x,z);
				n = 0.0f;
				float gain = 0.65f;
				float lacunarity = 1.8715f;
				float frequency = 1.0f/100f;
				float amplitude = gain;
				int octaves = 16;
				for(int i = 0; i < octaves; i++)
				{
					n += Mathf.PerlinNoise((float)x * frequency, (float)z * frequency) * amplitude;         
	               frequency *= lacunarity;
	               amplitude *= gain;
				}

				density[x,z] = 0.4f;//n;
				terrainColor[x,z] = new Color32(0,50,0,255);
			}
		}
		//*/
		float[,] chunk = new float[chunkSize+1,chunkSize+1];
		Color32[,] chunkColor = new Color32[chunkSize+1,chunkSize+1];

		//Generate Chunks
		for(int chunkX = 0; chunkX < chunkNum; chunkX++)
		{
			for(int chunkZ = 0; chunkZ < chunkNum; chunkZ++)
			{
				for(int cx = 0; cx <= chunkSize; cx++)
				{
					for(int cz = 0; cz <= chunkSize; cz++)
					{
						//0-15 [cx,cz], chunkSize = 16, chunk * 15 + 0-15
						if(chunkX*chunkSize+cx < width && chunkZ*chunkSize+cz < height)
						{
						//	if((cx == 0 && chunkX == 0) || (cz == 0 && chunkZ == 0))
						//	{
						//		chunk[cx,cz] = -0.1f;
						//		chunkColor[cx,cz] = new Color32(0,0,0,255);
						//	}
						//	if((cx == chunkSize && chunkX == chunkNum-1) || (cz == chunkSize && chunkZ == chunkNum-1))
					//		{
				//				chunk[cx,cz] = -0.1f;
			//					chunkColor[cx,cz] = new Color32(0,0,0,255);
			//				}
			//				else
			//				{
								chunk[cx,cz] = density[chunkX*(chunkSize) + cx, chunkZ*(chunkSize) + cz];
								chunkColor[cx,cz] = terrainColor[chunkX*(chunkSize) + cx, chunkZ*(chunkSize) + cz];
			//				}

						}
						else
						{
							chunk[cx,cz] = -0.1f;	
							chunkColor[cx,cz] = new Color32(0,0,0,255);
						}
						//chunk[cx,cz] = density[chunkX*(chunkSize) + cx, chunkZ*(chunkSize) + cz];
						//chunkColor[cx,cz] = terrainColor[chunkX*(chunkSize) + cx, chunkZ*(chunkSize) + cz];
					}
				}
				chunks[chunkX,chunkZ] = new GameObject("chunk[" + chunkX + ", " + chunkZ + "]");
				chunks[chunkX,chunkZ].tag = "ground";
				chunks[chunkX,chunkZ].layer = LayerMask.NameToLayer("terrain");
				chunks[chunkX,chunkZ].AddComponent<MeshFilter>();
				chunks[chunkX,chunkZ].AddComponent<MeshRenderer>();
				chunks[chunkX,chunkZ].AddComponent<MeshCollider>();
			//	if(chunkX < chunkNum-1 && chunkZ < chunkNum-1)
			//	{
					chunks[chunkX,chunkZ] = GenerateHeightmap(chunk, chunkColor, chunks[chunkX, chunkZ], chunkSize+1, chunkX, chunkZ);
			//	}
			//	else
			//	{
			//		chunks[chunkX,chunkZ] = GenerateHeightmap(chunk, chunks[chunkX, chunkZ], chunkSize);					
			//	}
		//		Debug.Log(chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals);
				chunks[chunkX,chunkZ].transform.position = new Vector3(chunkX*(chunkSize),0,chunkZ*(chunkSize));
				chunks[chunkX,chunkZ].transform.parent = this.transform;
			}
		}	
		for(int chunkX = 0; chunkX < chunkNum; chunkX++)
		{
			for(int chunkZ = 0; chunkZ < chunkNum; chunkZ++)
			{
				fixNormals(chunkX, chunkZ);
			}
		}
		//GenerateHeightmap();
	}

	public void updateChunk(int chunkX, int chunkZ)
	{
		float[,] chunk;
	//	if(chunkX < chunkNum-1 || chunkZ < chunkNum-1)
	//	{
		//previous if statement here ruined edge chunk terraforming, this should be redoable in a proper way
		//to save some minute amount of memory later if needed.
		chunk = new float[chunkSize+1,chunkSize+1];
		Color32[,] chunkColor = new Color32[chunkSize+1,chunkSize+1];
	//	}
	//	else
	//	{
	//		chunk = new float[chunkSize,chunkSize];
	//	}
		for(int cx = 0; cx <= chunkSize; cx++)
		{
			for(int cz = 0; cz <= chunkSize; cz++)
			{
				//127 = chunk 7
				//7*15 = 105     || 7*16 = 112
				//105 + 15 = 120 || 112 + 15 = 127
				if(chunkX*chunkSize+cx < width && chunkZ*chunkSize+cz < height)
				{
					if((cx == 0 && chunkX == 0) || (cz == 0 && chunkZ == 0))
					{
						chunk[cx,cz] = -0.1f;
						chunkColor[cx,cz] = new Color32(0,0,0,255);
					}
					else if((cx == chunkSize && chunkX == chunkNum-1) || (cz == chunkSize && chunkZ == chunkNum-1))
					{
						chunk[cx,cz] = -0.1f;
						chunkColor[cx,cz] = new Color32(0,0,0,255);
					}
					else
					{
						chunk[cx,cz] = density[chunkX*(chunkSize) + cx, chunkZ*(chunkSize) + cz];
						chunkColor[cx,cz] = terrainColor[chunkX*(chunkSize) + cx, chunkZ*(chunkSize) + cz];
					}
				}
				else
				{
					chunk[cx,cz] = -0.1f;
					chunkColor[cx,cz] = new Color32(0,0,0,255);
				}
			}
		}
	//	}
	//	if(chunkX < chunkNum-1 && chunkZ < chunkNum-1)
	//	{
			chunks[chunkX,chunkZ] = GenerateHeightmap(chunk, chunkColor, chunks[chunkX,chunkZ], chunkSize+1, chunkX, chunkZ);
	//	}
	//	else
	//	{
	//		chunks[chunkX,chunkZ] = GenerateHeightmap(chunk, chunks[chunkX,chunkZ], chunkSize);
	//	}
}

	//Update all chunks corresponding to this point
	public void updatePoint(int x, int z)
	{
		int chunkX = Mathf.FloorToInt(x/(chunkSize)); 
		int chunkZ = Mathf.FloorToInt(z/(chunkSize));

		bool east = (x - (chunkX*chunkSize) == chunkSize && chunkX < chunkNum-1);
		bool west = (x - (chunkX*chunkSize) == 0 && chunkX > 0);
		bool north = (z - (chunkZ*chunkSize) == chunkSize && chunkZ < chunkNum-1);
		bool south = (z - (chunkZ*chunkSize) == 0 && chunkZ > 0);

		if(west)
		{
			updateChunk(chunkX-1,chunkZ);
			fixNormals(chunkX-1,chunkZ);
		}
		if(east)
		{
			updateChunk(chunkX+1, chunkZ);
			fixNormals(chunkX+1, chunkZ);
		}
		if(south)
		{
			updateChunk(chunkX, chunkZ-1);
			fixNormals(chunkX, chunkZ-1);
		}
		if(north)
		{
			updateChunk(chunkX, chunkZ+1);
			fixNormals(chunkX, chunkZ+1);
		}
		if(south && east)
		{
			updateChunk(chunkX+1, chunkZ-1);
			fixNormals(chunkX+1, chunkZ-1);
		}
		if(south && west)
		{
			updateChunk(chunkX-1, chunkZ-1);
			fixNormals(chunkX-1, chunkZ-1);
		}
		if(north && east)
		{
			updateChunk(chunkX+1, chunkZ+1);
			fixNormals(chunkX+1, chunkZ+1);
		}
		if(north && west)
		{
			updateChunk(chunkX-1, chunkZ+1);
			fixNormals(chunkX-1, chunkZ+1);
		}
		updateChunk(chunkX, chunkZ);
		fixNormals(chunkX, chunkZ);
	}

	void Update()
	{
		//Terraforming Land -- really just a debug button right now, currently set to draw color.
		//Input.GetMouseButton(0) should be used here when not debugging so that we can hold the button to raise..
		if(Input.GetMouseButton(0))
		{
			Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			int layerMask = 1 << LayerMask.NameToLayer("terrain");
			if(Physics.Raycast(r, out hit, 1000f, layerMask))
			{
				int x = Mathf.FloorToInt(hit.point.x);
				int z = Mathf.FloorToInt(hit.point.z);
				terrainColor[x,z] = new Color32(0,0,50,255);
				updatePoint(x,z);
			}
		}
		
		//Water Test
		/*if(Input.GetMouseButton(1))
		{
			Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			int layerMask = 1 << LayerMask.NameToLayer("terrain");
			if(Physics.Raycast(r, out hit, 1000f, layerMask))
			{
				int x = Mathf.FloorToInt(hit.point.x);
				int z = Mathf.FloorToInt(hit.point.z);
				if(waterController.GetComponent<Water_DynamicHeightmap>().density[x,z] < density[x,z])
				{
					waterController.GetComponent<Water_DynamicHeightmap>().density[x,z] = density[x,z];
				}
				waterController.GetComponent<Water_DynamicHeightmap>().density[x,z] += terrainModifySpeed * Time.deltaTime;
			}
		}*/
		
	}

	//If we calculate every normal for the entire terrain area...
	//- Can easily pull existing values on chunk recalculation...
	//- will be less efficient if we generate new chunks midgame or edit tons of vertices in a given chunk...
	//
	//Most efficient method is attempted below... take edges of each chunk, average them for normal.Currently this does not work properly...
	void fixNormals(int chunkX, int chunkZ)
	{
		//Fix normals between adjusted chunk and its neighbors
		Vector3[] centerNorms = chunks[chunkX, chunkZ].GetComponent<MeshFilter>().mesh.normals;
		Vector3[] adjacent = null;
		
		//Handle 4 corners of chunks...
		//4 corners...
		//x+1, z+1 (16x, 16z) -- first corner... On initial generation this is enough, on update we need all 4 corners updated...
		//      
		//    _|_     lTop 16x 0z | rTop 0x 0z
		//     |      main 16x 16z| rBottom 0x 16z
		//top right corner
		if(chunkX < chunkNum-1 && chunkZ < chunkNum-1)
		{
			Vector3[] tRight = chunks[chunkX+1, chunkZ+1].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] tLeft = chunks[chunkX, chunkZ+1].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] bRight = chunks[chunkX+1, chunkZ].GetComponent<MeshFilter>().mesh.normals;
			Vector3 fixedNormal = (centerNorms[chunkSize*(chunkSize+1) + (chunkSize)] + tLeft[0*(chunkSize+1) + chunkSize] + tRight[0*(chunkSize+1) + 0] + bRight[chunkSize*(chunkSize+1) + 0]) /4f;
			centerNorms[chunkSize * (chunkSize+1) + (chunkSize)] = fixedNormal;
			tLeft[0*(chunkSize+1) + chunkSize] = fixedNormal;
			tRight[0*(chunkSize+1) + 0] = fixedNormal;
			bRight[chunkSize*(chunkSize+1) + 0] = fixedNormal;
			
			chunks[chunkX+1,chunkZ+1].GetComponent<MeshFilter>().mesh.normals = tRight;
			chunks[chunkX+1,chunkZ].GetComponent<MeshFilter>().mesh.normals = bRight;
			chunks[chunkX,chunkZ+1].GetComponent<MeshFilter>().mesh.normals = tLeft;
			chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
		}
		//The corners on the very edges of the map need to be calculated as normal edges since only 2 chunks will be next to one another.
		//However, this is the very far edge of the map, it won't be hugely noticeable and thus isn't a primary concern until polishing? -- no concern since edges are walls on side of map..
//		else if(chunkX < chunkNum-1)
//		{
	//	}
//		else if(chunkZ < chunkNum-1)
	//	{
	//	}
		//    _|_     main 16x 0z | rTop 0x 0z
		//     |      bleft 16x 16z| rBottom 0x 16z
		//x+1, z-1 -- bottom right corner
		if(chunkX < chunkNum-1 && chunkZ > 0)
		{
			Vector3[] tRight = chunks[chunkX+1, chunkZ].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] bLeft = chunks[chunkX, chunkZ-1].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] bRight = chunks[chunkX+1, chunkZ-1].GetComponent<MeshFilter>().mesh.normals;
			Vector3 fixedNormal = (centerNorms[0*(chunkSize+1) + (chunkSize)] + bLeft[chunkSize*(chunkSize+1) + chunkSize] + tRight[0*(chunkSize+1) + 0] + bRight[chunkSize*(chunkSize+1) + 0]) /4f;
			centerNorms[0 * (chunkSize+1) + (chunkSize)] = fixedNormal;
			bLeft[chunkSize*(chunkSize+1) + chunkSize] = fixedNormal;
			tRight[0*(chunkSize+1) + 0] = fixedNormal;
			bRight[chunkSize*(chunkSize+1) + 0] = fixedNormal;
			
			chunks[chunkX+1,chunkZ].GetComponent<MeshFilter>().mesh.normals = tRight;
			chunks[chunkX+1,chunkZ-1].GetComponent<MeshFilter>().mesh.normals = bRight;
			chunks[chunkX,chunkZ-1].GetComponent<MeshFilter>().mesh.normals = bLeft;
			chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
		}
		
		//    _|_     lTop 16x 0z | rTop 0x 0z
		//     |      bleft 16x 16z| main 0x 16z
		//x-1, z+1 -- top left corner
		if(chunkX > 0 && chunkZ < chunkNum-1)
		{
			Vector3[] tRight = chunks[chunkX, chunkZ+1].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] tLeft = chunks[chunkX-1, chunkZ+1].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] bLeft = chunks[chunkX-1, chunkZ].GetComponent<MeshFilter>().mesh.normals;
			Vector3 fixedNormal = (centerNorms[chunkSize*(chunkSize+1) + 0] + tLeft[0*(chunkSize+1) + chunkSize] + tRight[0*(chunkSize+1) + 0] + bLeft[chunkSize*(chunkSize+1) + chunkSize]) /4f;
			centerNorms[chunkSize * (chunkSize+1) + 0] = fixedNormal;
			tLeft[0*(chunkSize+1) + chunkSize] = fixedNormal;
			tRight[0*(chunkSize+1) + 0] = fixedNormal;
			bLeft[chunkSize*(chunkSize+1) + chunkSize] = fixedNormal;
			
			chunks[chunkX-1,chunkZ+1].GetComponent<MeshFilter>().mesh.normals = tLeft;
			chunks[chunkX-1,chunkZ].GetComponent<MeshFilter>().mesh.normals = bLeft;
			chunks[chunkX,chunkZ+1].GetComponent<MeshFilter>().mesh.normals = tRight;
			chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
		}
		//    _|_     lTop 16x 0z | main 0x 0z
		//     |      bleft 16x 16z| rBottom 0x 16z
		//x-1, z-1 -- bottom left corner
		if(chunkX > 0 && chunkZ > 0)
		{
			Vector3[] bLeft = chunks[chunkX-1, chunkZ-1].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] tLeft = chunks[chunkX-1, chunkZ].GetComponent<MeshFilter>().mesh.normals;
			Vector3[] bRight = chunks[chunkX, chunkZ-1].GetComponent<MeshFilter>().mesh.normals;
			Vector3 fixedNormal = (centerNorms[0*(chunkSize+1) + 0] + tLeft[0*(chunkSize+1) + chunkSize] + bLeft[chunkSize*(chunkSize+1) + chunkSize] + bRight[chunkSize*(chunkSize+1) + 0]) /4f;
			centerNorms[0 * (chunkSize+1) + 0] = fixedNormal;
			tLeft[0*(chunkSize+1) + chunkSize] = fixedNormal;
			bLeft[chunkSize*(chunkSize+1) + chunkSize] = fixedNormal;
			bRight[chunkSize*(chunkSize+1) + 0] = fixedNormal;
			
			chunks[chunkX-1,chunkZ-1].GetComponent<MeshFilter>().mesh.normals = bLeft;
			chunks[chunkX,chunkZ-1].GetComponent<MeshFilter>().mesh.normals = bRight;
			chunks[chunkX-1,chunkZ].GetComponent<MeshFilter>().mesh.normals = tLeft;
			chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
		}
		
		//handle chunk edge...
		if(chunkX != 0)
		{
			adjacent = chunks[chunkX-1,chunkZ].GetComponent<MeshFilter>().mesh.normals;
			
			//looking for 0x edge on center, 16x edge on adjacent.
			//vertices[y*chunkSize + x]
			for(int z = 1; z < chunkSize; z++)
			{
				Vector3 fixedNormal = (centerNorms[z*(chunkSize+1) + 0] + adjacent[z*(chunkSize+1) + (chunkSize)])/2f;
				centerNorms[z * (chunkSize+1) + 0] = fixedNormal;
				adjacent[z * (chunkSize+1) + (chunkSize)] = fixedNormal;
				
			//	if(z == 1)Debug.Log("Fixed Normal (!0): " + fixedNormal);
				
				chunks[chunkX-1,chunkZ].GetComponent<MeshFilter>().mesh.normals = adjacent;
				chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
			}
		}
		if(chunkX < chunkNum - 1)
		{
			adjacent = chunks[chunkX+1,chunkZ].GetComponent<MeshFilter>().mesh.normals;
			
			//looking for 16x edge on center, 0x edge on adjacent.
			
			//vertices[y*chunkSize + x]
			for(int z = 1; z < chunkSize; z++)
			{
				Vector3 fixedNormal = (centerNorms[z*(chunkSize+1) + (chunkSize)] + adjacent[z*(chunkSize+1) + 0])/2f;
				centerNorms[z * (chunkSize+1) + (chunkSize)] = fixedNormal;
				adjacent[z * (chunkSize+1) + 0] = fixedNormal;
				
				//if(z == 1)Debug.Log("Fixed Normal (<C): " + fixedNormal);
				
				chunks[chunkX+1,chunkZ].GetComponent<MeshFilter>().mesh.normals = adjacent;
				chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
			}
		}
		
		//Now fix Z side chunks..
		if(chunkZ != 0)
		{
			adjacent = chunks[chunkX,chunkZ-1].GetComponent<MeshFilter>().mesh.normals;
			
			//looking for 0z edge on center, 16z edge on adjacent.
			
			//vertices[y*chunkSize + x]
			for(int x = 1; x < chunkSize; x++)
			{
				Vector3 fixedNormal = (centerNorms[0*(chunkSize+1) + x] + adjacent[(chunkSize)*(chunkSize+1) + x])/2f;
				centerNorms[0 * (chunkSize+1) + x] = fixedNormal;
				adjacent[(chunkSize) * (chunkSize+1) + x] = fixedNormal;
				
				chunks[chunkX,chunkZ-1].GetComponent<MeshFilter>().mesh.normals = adjacent;
				chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
			}
		}
		if(chunkZ < chunkNum - 1)
		{
			adjacent = chunks[chunkX,chunkZ+1].GetComponent<MeshFilter>().mesh.normals;
			
			//looking for 16z edge on center, 0z edge on adjacent.
			
			//vertices[y*chunkSize + x]
			for(int x = 1; x < chunkSize; x++)
			{
				Vector3 fixedNormal = (centerNorms[(chunkSize)*(chunkSize+1) + x] + adjacent[0*(chunkSize+1) + x])/2f;
				centerNorms[(chunkSize) * (chunkSize+1) + x] = fixedNormal;
				adjacent[0 * (chunkSize+1) + x] = fixedNormal;
				
				chunks[chunkX,chunkZ+1].GetComponent<MeshFilter>().mesh.normals = adjacent;
				chunks[chunkX,chunkZ].GetComponent<MeshFilter>().mesh.normals = centerNorms;
			}
		}
	}
	
	GameObject GenerateHeightmap(float[,] chunkDensity, Color32[,] chunkColor, GameObject chunk, int chunkSize, int chunkX, int chunkZ)
	{
		// Create the game object containing the renderer
		if (material)
		{
			chunk.GetComponent<Renderer>().material = material;
		}
		else
		{
			chunk.GetComponent<Renderer>().material.color = Color.white;
		}
	
		// Retrieve a mesh instance
		Mesh mesh = chunk.GetComponent<MeshFilter>().mesh;
	
		//width = Mathf.Min(heightMap.width, width-1);//255);
		//height = Mathf.Min(heightMap.height, height-1);//255);
		int y = 0;
		int x = 0;
	
		// Build vertices and UVs
		int chunkS = chunkSize * chunkSize;
		Vector3[] vertices = new Vector3[chunkS];
		Vector2[] uv = new Vector2[chunkS];
		Vector4[] tangents = new Vector4[chunkS];
		Color32[] colors = new Color32[chunkS];
		
		Vector2 uvScale = new Vector2 (1.0f / (chunkSize - 1), 1.0f/ (chunkSize - 1f));
		Vector3 sizeScale = new Vector3(size.x / (chunkSize - 1), size.y, size.z / (chunkSize - 1));
		
		for (y=0;y < chunkSize;y++)
		{
			for (x=0;x < chunkSize;x++)
			{
				float pixelHeight = chunkDensity[x,y];//heightMap.GetPixel(x, y).grayscale;
				colors[y*chunkSize + x] = chunkColor[x,y];
				Vector3 vertex = new Vector3 (x, pixelHeight, y);
				vertices[y*chunkSize + x] = Vector3.Scale(sizeScale, vertex);
				uv[y*chunkSize + x] = Vector2.Scale(new Vector2 (x, y), uvScale);
	
				// Calculate tangent vector: a vector that goes from previous vertex
				// to next along X direction. We need tangents if we intend to
				// use bumpmap shaders on the mesh.
				if(x-1 >= 0 && x+1 < chunkSize)
				{
					Vector3 vertexL = new Vector3( x-1, chunkDensity[x-1, y], y );
					Vector3 vertexR = new Vector3( x+1, chunkDensity[x+1, y], y );
					Vector3 tan = Vector3.Scale( sizeScale, vertexR - vertexL ).normalized;
					tangents[y*chunkSize + x] = new Vector4( tan.x, tan.y, tan.z, -1.0f );
				}
			}
		}
		
		// Assign them to the mesh
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.colors32 = colors;
	
		// Build triangle indices: 3 indices into vertex array for each triangle
		int[] triangles = new int[(chunkSize - 1) * (chunkSize - 1) * 6];
		int index = 0;
		for (y = 0; y < chunkSize - 1; y++)
		{
			for (x = 0; x < chunkSize - 1; x++)
			{
				// For each grid cell output two triangles
				triangles[index++] = (y     * chunkSize) + x;
				triangles[index++] = ((y+1) * chunkSize) + x;
				triangles[index++] = (y     * chunkSize) + x + 1;
	
				triangles[index++] = ((y+1) * chunkSize) + x;
				triangles[index++] = ((y+1) * chunkSize) + x + 1;
				triangles[index++] = (y     * chunkSize) + x + 1;
			}
		}
	
	//Trying to avoid calculating every vertex of my own... try to only adjust edges..	
	//	Vector3[] norms = new Vector3[(chunkSize-1) * (chunkSize-1)];
	//	int nIndex = 0;
	//	for(int nZ = 0; nZ < chunkSize - 1; nZ++)
	//	{
	//		for(int nX = 0; nX < chunkSize - 1; nX++)
	//		{
	//			norms[nIndex++] = 
	//		}
	//	}
		
		// And assign them to the mesh
		mesh.triangles = triangles;
			
		// Auto-calculate vertex normals from the mesh
		mesh.RecalculateNormals();
		
		// Assign tangents after recalculating normals
		mesh.tangents = tangents;
		chunk.GetComponent<MeshCollider>().sharedMesh = null;
		chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
		return chunk;
	}

}
