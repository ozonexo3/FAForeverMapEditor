// ***************************************************************************************
// * SCmap editor
// * Set Unity objects and scripts using data loaded from Scm
// ***************************************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine.PostProcessing;

public class ScmapEditor : MonoBehaviour
{

	public static ScmapEditor Current;

	[Header("Connections")]
	public Camera Cam;
	public Terrain Teren;
	public TerrainData Data;
	public Transform WaterLevel;
	public ResourceBrowser ResBrowser;
	public Light Sun;
	public Material TerrainMaterial;
	public Material WaterMaterial;
	public PostProcessingProfile PostProcessing;
	public BloomOptimized BloomOpt;

	[Header("Loaded variables")]
	public TerrainTexture[] Textures; // Loaded textures
	public Map map; // Loaded Scmap data
	public SkyboxData.SkyboxValues DefaultSkyboxData;

	float[,] heights = new float[1, 1];
	bool Grid;
	[HideInInspector]
	public bool Slope;
	//string Shader;
	//public		TerrainMesh		TerrainM;

	public const float MapHeightScale = 2048;


	// Stratum Layer
	[System.Serializable]
	public class TerrainTexture
	{
		public Texture2D Albedo;
		public Texture2D Normal;
		public Vector2 Tilling = Vector2.one;
		//Scmap Data
		public string AlbedoPath;
		public string NormalPath;
		public float AlbedoScale;
		public float NormalScale;
	}


	void Awake()
	{
		Current = this;
		ResBrowser.Instantiate();
		EnvPaths.CurrentGamedataPath = EnvPaths.GetGamedataPath();
	}

	void Start()
	{
		ToogleGrid(false);
		heights = new float[10, 10];
		RestartTerrainAsset();
	}

	public void UpdateLighting()
	{
		Vector3 SunDIr = new Vector3(-map.SunDirection.x, -map.SunDirection.y, map.SunDirection.z);
		Sun.transform.rotation = Quaternion.LookRotation(SunDIr);
		Sun.color = new Color(map.SunColor.x, map.SunColor.y, map.SunColor.z, 1);
		Sun.intensity = map.LightingMultiplier * EditMap.LightingInfo.SunMultipiler;
		RenderSettings.ambientLight = new Color(map.ShadowFillColor.x, map.ShadowFillColor.y, map.ShadowFillColor.z, 1);

		BloomModel.Settings Bs = PostProcessing.bloom.settings;
		Bs.bloom.intensity = map.Bloom * 10;
		PostProcessing.bloom.settings = Bs;

		BloomOpt.intensity = map.Bloom;

		RenderSettings.fogColor = new Color(map.FogColor.x, map.FogColor.y, map.FogColor.z, 1);
		RenderSettings.fogStartDistance = map.FogStart * 2;
		RenderSettings.fogEndDistance = map.FogEnd * 2;

		Shader.SetGlobalFloat("_LightingMultiplier", map.LightingMultiplier);
		Shader.SetGlobalColor("_SunColor", new Color(map.SunColor.x * 0.5f, map.SunColor.y * 0.5f, map.SunColor.z * 0.5f, 1));
		Shader.SetGlobalColor("_SunAmbience", new Color(map.SunAmbience.x * 0.5f, map.SunAmbience.y * 0.5f, map.SunAmbience.z * 0.5f, 1));
		Shader.SetGlobalColor("_ShadowColor", new Color(map.ShadowFillColor.x * 0.5f, map.ShadowFillColor.y * 0.5f, map.ShadowFillColor.z * 0.5f, 1));

	}

	public IEnumerator LoadScmapFile()
	{
		//UnloadMap();

		map = new Map();

		string MapPath = EnvPaths.GetMapsPath();
		string path = MapLuaParser.Current.ScenarioLuaFile.Data.map.Replace("/maps/", MapPath);

		Debug.Log("Load SCMAP file: " + path);


		if (map.Load(path))
		{
			UpdateLighting();
		}
		else
		{
			Debug.LogError("File not found");
			StopCoroutine("LoadScmapFile");
		}




		if (map.VersionMinor >= 60)
		{
			
			// Create Default Values
			/*
			DefaultSkyboxData = new SkyboxData.SkyboxValues();
			DefaultSkyboxData.BeginBytes = map.AdditionalSkyboxData.Data.BeginBytes;
			DefaultSkyboxData.Albedo = map.AdditionalSkyboxData.Data.Albedo;
			DefaultSkyboxData.Glow = map.AdditionalSkyboxData.Data.Glow;
			DefaultSkyboxData.Length = map.AdditionalSkyboxData.Data.Length;
			DefaultSkyboxData.MidBytes = map.AdditionalSkyboxData.Data.MidBytes;
			DefaultSkyboxData.MidBytesStatic = map.AdditionalSkyboxData.Data.MidBytesStatic;
			DefaultSkyboxData.Clouds = map.AdditionalSkyboxData.Data.Clouds;
			DefaultSkyboxData.EndBytes = map.AdditionalSkyboxData.Data.EndBytes;
			*/
		}
		else
		{
			map.AdditionalSkyboxData.Data = new SkyboxData.SkyboxValues();
			map.AdditionalSkyboxData.Data.BeginBytes = DefaultSkyboxData.BeginBytes;
			map.AdditionalSkyboxData.Data.Albedo = DefaultSkyboxData.Albedo;
			map.AdditionalSkyboxData.Data.Glow = DefaultSkyboxData.Glow;
			map.AdditionalSkyboxData.Data.Length = DefaultSkyboxData.Length;
			//map.AdditionalSkyboxData.Data.MidBytes = DefaultSkyboxData.MidBytes;
			map.AdditionalSkyboxData.Data.MidBytesStatic = DefaultSkyboxData.MidBytesStatic;
			map.AdditionalSkyboxData.Data.Clouds = DefaultSkyboxData.Clouds;
			map.AdditionalSkyboxData.Data.EndBytes = DefaultSkyboxData.EndBytes;

		}


		EnvPaths.CurrentGamedataPath = EnvPaths.GetGamedataPath();

		//Shader = map.TerrainShader;
		MapLuaParser.Current.EditMenu.TexturesMenu.TTerrainXP.isOn = map.TerrainShader == "TTerrainXP";
		ToogleShader();

	

		// Set Variables
		int xRes = MapLuaParser.Current.ScenarioLuaFile.Data.Size[0];
		int zRes = MapLuaParser.Current.ScenarioLuaFile.Data.Size[1];
		float HalfxRes = xRes / 10f;
		float HalfzRes = zRes / 10f;

		float yRes = (float)map.HeightScale; ;
		float HeightResize = 512 * 40;

		TerrainMaterial.SetTexture("_TerrainNormal", map.UncompressedNormalmapTex);
		Shader.SetGlobalTexture("_UtilitySamplerC", map.UncompressedWatermapTex);
		//WaterMaterial.SetTexture("_UtilitySamplerC", map.UncompressedWatermapTex);
		//WaterMaterial.SetFloat("_WaterScaleX", xRes);
		//WaterMaterial.SetFloat("_WaterScaleZ", zRes);
		//TerrainMaterial.SetFloat("_WaterScaleX", xRes);
		//TerrainMaterial.SetFloat("_WaterScaleZ", zRes);
		Shader.SetGlobalFloat("_WaterScaleX", xRes);
		Shader.SetGlobalFloat("_WaterScaleZ", xRes);

		//*****************************************
		// ***** Set Terrain proportives
		//*****************************************
		if (Teren) DestroyImmediate(Teren.gameObject);

		// Load Stratum Textures Paths
		LoadStratumScdTextures();
		MapLuaParser.Current.InfoPopup.Show(true, "Loading map...\n( Assing scmap data )");


		Teren = Terrain.CreateTerrainGameObject(Data).GetComponent<Terrain>();
		Teren.gameObject.name = "TERRAIN";
		Teren.materialType = Terrain.MaterialType.Custom;
		Teren.materialTemplate = TerrainMaterial;
		Teren.heightmapPixelError = 5f;
		Teren.basemapDistance = 10000;
		Teren.castShadows = false;
		Teren.drawTreesAndFoliage = false;
		Teren.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		//Teren.terrainData.detailResolution = 32;
		//Teren.terrainData.alphamapResolution = 32;
		//Teren.terrainData.baseMapResolution = 32;

		Data.heightmapResolution = (int)(xRes + 1);
		Data.size = new Vector3(
			HalfxRes,
			yRes * MapHeightScale,
			HalfzRes
			);
		//Data.SetDetailResolution((int)(xRes / 2), 8);
		//Data.baseMapResolution = (int)(xRes / 2);
		//Data.alphamapResolution = (int)(xRes / 2);

		Teren.transform.localPosition = new Vector3(0, 0, -HalfzRes);


		WaterLevel.transform.localScale = new Vector3(HalfxRes, 1, HalfzRes);
		TerrainMaterial.SetFloat("_GridScale", HalfxRes);
		TerrainMaterial.SetTexture("_UtilitySamplerC", map.UncompressedWatermapTex);





		/*
		 * // Cubemap
		Texture2D Cubemap = GetGamedataFile.LoadTexture2DFromGamedata("textures.scd", map.Water.TexPathCubemap);
		Debug.Log(Cubemap.width);
		Cubemap cb = new Cubemap(Cubemap.width, TextureFormat.RGB24, Cubemap.mipmapCount > 1);
		cb.SetPixels(Cubemap.GetPixels(), CubemapFace.PositiveX);
		WaterMaterial.SetTexture("_Reflection", cb);
		*/

		SetWaterTextures();


		SetWater();


		int Max = (int)Mathf.Max((map.Height + 1), (map.Width + 1));
		heights = new float[Max, Max];

		float HeightWidthMultiply = (map.Height / (float)map.Width);

		// Modify heights array data
		int y = 0;
		int x = 0;
		int localY = 0;

		for (y = 0; y < Max; y++)
		{
			for (x = 0; x < Max; x++)
			{
				localY = (int)(((Max - 1) - y) * HeightWidthMultiply);

				heights[y, x] = map.GetHeight(x, localY) / HeightResize;

				if (HeightWidthMultiply == 0.5f && y > 0 && y % 2f == 0)
				{
					heights[y - 1, x] = Mathf.Lerp(heights[y, x], heights[y - 2, x], 0.5f);
				}
			}
		}

		// Set terrain heights from heights array
		Data.SetHeights(0, 0, heights);

		Teren.gameObject.layer = 8;

		SetTextures();

		if (Slope)
		{
			ToogleSlope(Slope);
		}

		yield return null;
		Debug.Log("Scmap load complited");
	}

	public void LoadStratumScdTextures(bool Loading = true)
	{
		// Load Stratum Textures Paths
		for (int i = 0; i < Textures.Length; i++)
		{
			if (Loading)
			{
				MapLuaParser.Current.InfoPopup.Show(true, "Loading map...\n( Stratum textures " + (i + 1) + " )");

				Textures[i].AlbedoPath = map.Layers[i].PathTexture;
				Textures[i].NormalPath = map.Layers[i].PathNormalmap;
				if (Textures[i].AlbedoPath.StartsWith("/"))
				{
					Textures[i].AlbedoPath = Textures[i].AlbedoPath.Remove(0, 1);
				}
				if (Textures[i].NormalPath.StartsWith("/"))
				{
					Textures[i].NormalPath = Textures[i].NormalPath.Remove(0, 1);
				}
			
				Textures[i].AlbedoScale = map.Layers[i].ScaleTexture;
				Textures[i].NormalScale = map.Layers[i].ScaleNormalmap;
			}
			//Debug.Log("Load textures: " + i);

			try
			{
				GetGamedataFile.LoadTextureFromGamedata("env.scd", Textures[i].AlbedoPath, i, false);
			}
			catch (System.Exception e)
			{
				Debug.LogError(i + ", Albedo tex: " + Textures[i].AlbedoPath);
				Debug.LogError(e);
			}
			try
			{
				GetGamedataFile.LoadTextureFromGamedata("env.scd", Textures[i].NormalPath, i, true);
			}
			catch (System.Exception e)
			{
				Debug.LogError(i + ", Normal tex: " + Textures[i].NormalPath);
				Debug.LogError(e);
			}
		}
	}

	#region Water
	public void SetWater()
	{
		WaterLevel.gameObject.SetActive(map.Water.HasWater);
		WaterLevel.transform.position = Vector3.up * (map.Water.Elevation / 10.0f);

		WaterMaterial.SetColor("waterColor", new Color(map.Water.SurfaceColor.x * 0.5f, map.Water.SurfaceColor.y * 0.5f, map.Water.SurfaceColor.z * 0.5f, 1));
		WaterMaterial.SetColor("sunColor", new Color(map.Water.SunColor.x * 0.5f, map.Water.SunColor.y * 0.5f, map.Water.SunColor.z * 0.5f, 1));

		Shader.SetGlobalVector("waterLerp", map.Water.ColorLerp);
		Shader.SetGlobalVector("SunDirection", new Vector3(map.Water.SunDirection.x, map.Water.SunDirection.y, -map.Water.SunDirection.z));
		Shader.SetGlobalFloat("SunShininess", map.Water.SunShininess);
		Shader.SetGlobalFloat("sunReflectionAmount", map.Water.SunReflection);
		Shader.SetGlobalFloat("unitreflectionAmount", map.Water.UnitReflection);
		Shader.SetGlobalFloat("skyreflectionAmount", map.Water.SkyReflection);
		Shader.SetGlobalFloat("refractionScale", map.Water.RefractionScale);

		Shader.SetGlobalFloat("fresnelPower", map.Water.FresnelPower);
		Shader.SetGlobalFloat("fresnelBias", map.Water.FresnelBias);


		//Shader.SetGlobalVector("waterLerp", map.Water.WaveTextures);

		TerrainMaterial.SetFloat("_WaterLevel", map.Water.Elevation / 10.0f);
		TerrainMaterial.SetFloat("_DepthLevel", map.Water.ElevationDeep / 10.0f);
		TerrainMaterial.SetFloat("_AbyssLevel", map.Water.ElevationAbyss / 10.0f);
		TerrainMaterial.SetInt("_Water", map.Water.HasWater ? 1 : 0);
	}

	public void SetWaterTextures()
	{
		Texture2D WaterRamp = GetGamedataFile.LoadTexture2DFromGamedata("textures.scd", map.Water.TexPathWaterRamp);
		WaterRamp.wrapMode = TextureWrapMode.Clamp;
		Shader.SetGlobalTexture("_WaterRam", WaterRamp);

		const int WaterAnisoLevel = 4;

		Texture2D WaterNormal = GetGamedataFile.LoadTexture2DFromGamedata("textures.scd", map.Water.WaveTextures[0].TexPath);
		WaterNormal.anisoLevel = WaterAnisoLevel;
		WaterMaterial.SetTexture("NormalSampler0", WaterNormal);
		WaterNormal = GetGamedataFile.LoadTexture2DFromGamedata("textures.scd", map.Water.WaveTextures[1].TexPath);
		WaterNormal.anisoLevel = WaterAnisoLevel;
		WaterMaterial.SetTexture("NormalSampler1", WaterNormal);
		WaterNormal = GetGamedataFile.LoadTexture2DFromGamedata("textures.scd", map.Water.WaveTextures[2].TexPath);
		WaterNormal.anisoLevel = WaterAnisoLevel;
		WaterMaterial.SetTexture("NormalSampler2", WaterNormal);
		WaterNormal = GetGamedataFile.LoadTexture2DFromGamedata("textures.scd", map.Water.WaveTextures[3].TexPath);
		WaterNormal.anisoLevel = WaterAnisoLevel;
		WaterMaterial.SetTexture("NormalSampler3", WaterNormal);

		Shader.SetGlobalVector("normal1Movement", map.Water.WaveTextures[0].NormalMovement);
		Shader.SetGlobalVector("normal2Movement", map.Water.WaveTextures[1].NormalMovement);
		Shader.SetGlobalVector("normal3Movement", map.Water.WaveTextures[2].NormalMovement);
		Shader.SetGlobalVector("normal4Movement", map.Water.WaveTextures[3].NormalMovement);
		Shader.SetGlobalVector("normalRepeatRate", new Vector4(map.Water.WaveTextures[0].NormalRepeat, map.Water.WaveTextures[1].NormalRepeat, map.Water.WaveTextures[2].NormalRepeat, map.Water.WaveTextures[3].NormalRepeat));
	}

	#endregion

	#region Textures
	public void SetTextures(int OnlyOne = -1)
	{

		if (OnlyOne < 0)
		{
			TerrainMaterial.SetTexture("_ControlXP", map.TexturemapTex);
			if (Textures[5].Albedo || Textures[6].Albedo || Textures[7].Albedo || Textures[8].Albedo)
				TerrainMaterial.SetTexture("_Control2XP", map.TexturemapTex2);
		}


		if (OnlyOne <= 0)
		{
			TerrainMaterial.SetFloat("_LowerScale", map.Width / Textures[0].AlbedoScale);
			TerrainMaterial.SetFloat("_LowerScaleNormal", map.Width / Textures[0].NormalScale);
			TerrainMaterial.SetTexture("_SplatLower", Textures[0].Albedo);
			TerrainMaterial.SetTexture("_NormalLower", Textures[0].Normal);
		}

		if (OnlyOne > 0 && OnlyOne < 9)
		{
			string IdStrig = (OnlyOne - 1).ToString();
			//TerrainMaterial.SetTexture("_Splat" + IdStrig + "XP", Textures[OnlyOne].Albedo);
			TerrainMaterial.SetFloat("_Splat" + IdStrig + "Scale", map.Width / Textures[OnlyOne].AlbedoScale);
			//TerrainMaterial.SetTexture("_SplatNormal" + IdStrig, Textures[OnlyOne].Normal);
			TerrainMaterial.SetFloat("_Splat" + IdStrig + "ScaleNormal", map.Width / Textures[OnlyOne].NormalScale);
		}
		else
		{
			//TerrainMaterial.SetTexture("_Splat0XP", Textures[1].Albedo);
			//TerrainMaterial.SetTexture("_Splat1XP", Textures[2].Albedo);
			//TerrainMaterial.SetTexture("_Splat2XP", Textures[3].Albedo);
			//TerrainMaterial.SetTexture("_Splat3XP", Textures[4].Albedo);
			//TerrainMaterial.SetTexture("_Splat4XP", Textures[5].Albedo);
			//TerrainMaterial.SetTexture("_Splat5XP", Textures[6].Albedo);
			//TerrainMaterial.SetTexture("_Splat6XP", Textures[7].Albedo);
			//TerrainMaterial.SetTexture("_Splat7XP", Textures[8].Albedo);



			TerrainMaterial.SetFloat("_Splat0Scale", map.Width / Textures[1].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat1Scale", map.Width / Textures[2].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat2Scale", map.Width / Textures[3].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat3Scale", map.Width / Textures[4].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat4Scale", map.Width / Textures[5].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat5Scale", map.Width / Textures[6].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat6Scale", map.Width / Textures[7].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat7Scale", map.Width / Textures[8].AlbedoScale);

			//TerrainMaterial.SetTexture("_SplatNormal0", Textures[1].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal1", Textures[2].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal2", Textures[3].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal3", Textures[4].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal4", Textures[5].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal5", Textures[6].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal6", Textures[7].Normal);
			//TerrainMaterial.SetTexture("_SplatNormal7", Textures[8].Normal);

			TerrainMaterial.SetFloat("_Splat0ScaleNormal", map.Width / Textures[1].NormalScale);
			TerrainMaterial.SetFloat("_Splat1ScaleNormal", map.Width / Textures[2].NormalScale);
			TerrainMaterial.SetFloat("_Splat2ScaleNormal", map.Width / Textures[3].NormalScale);
			TerrainMaterial.SetFloat("_Splat3ScaleNormal", map.Width / Textures[4].NormalScale);
			TerrainMaterial.SetFloat("_Splat4ScaleNormal", map.Width / Textures[5].NormalScale);
			TerrainMaterial.SetFloat("_Splat5ScaleNormal", map.Width / Textures[6].NormalScale);
			TerrainMaterial.SetFloat("_Splat6ScaleNormal", map.Width / Textures[7].NormalScale);
			TerrainMaterial.SetFloat("_Splat7ScaleNormal", map.Width / Textures[8].NormalScale);
		}

		if (OnlyOne == 9 || OnlyOne < 0)
		{
			TerrainMaterial.SetFloat("_UpperScale", map.Width / Textures[9].AlbedoScale);
			TerrainMaterial.SetFloat("_UpperScaleNormal", map.Width / Textures[9].NormalScale);
			TerrainMaterial.SetTexture("_SplatUpper", Textures[9].Albedo);
			TerrainMaterial.SetTexture("_NormalUpper", Textures[9].Normal);
		}

		if(OnlyOne != 9 && OnlyOne != 0)
		{
			GenerateArrays();
		}
	}

	void GenerateArrays()
	{
		int AlbedoSize = 256;

		for (int i = 0; i < 8; i++)
		{
			if (Textures[i + 1].Albedo.width > AlbedoSize)
			{
				AlbedoSize = Textures[i + 1].Albedo.width;
			}
			if (Textures[i + 1].Albedo.height > AlbedoSize)
			{
				AlbedoSize = Textures[i + 1].Albedo.height;
			}
		}


		Texture2DArray AlbedoArray = new Texture2DArray(AlbedoSize, AlbedoSize, 8, TextureFormat.RGBA32, true);

		int MipMapCount = 10;
		for (int i = 0; i < 8; i++)
		{
			if (Textures[i + 1].Albedo.width != AlbedoSize || Textures[i + 1].Albedo.height != AlbedoSize)
			{

				Textures[i + 1].Albedo = TextureScale.Bilinear(Textures[i + 1].Albedo, AlbedoSize, AlbedoSize);
			}


			if (i == 0)

				MipMapCount = Textures[i + 1].Albedo.mipmapCount;

			if (MipMapCount != Textures[i + 1].Albedo.mipmapCount)
				Debug.LogWarning("Wrong mipmap Count for texture" + Textures[i + 1].AlbedoPath);
			for (int m = 0; m < MipMapCount; m++)
			{
				AlbedoArray.SetPixels(Textures[i + 1].Albedo.GetPixels(m), i, m);

			}
		}

		//AlbedoArray.mipMapBias = 0.5f;
		AlbedoArray.filterMode = FilterMode.Bilinear;
		AlbedoArray.anisoLevel = 4;

		AlbedoArray.Apply(true);
		TerrainMaterial.SetTexture("_SplatAlbedoArray", AlbedoArray);

		AlbedoSize = 256;

		for (int i = 0; i < 8; i++)
		{
			if (Textures[i + 1].Normal == null)
				continue;
			if (Textures[i + 1].Normal.width > AlbedoSize)
			{
				AlbedoSize = Textures[i + 1].Normal.width;
			}
			if (Textures[i + 1].Normal.height > AlbedoSize)
			{
				AlbedoSize = Textures[i + 1].Normal.height;
			}
		}

		Texture2DArray NormalArray = new Texture2DArray(AlbedoSize, AlbedoSize, 8, TextureFormat.RGBA32, true);

		for (int i = 0; i < 8; i++)
		{
			if (Textures[i + 1].Normal == null)
				continue;

			if (Textures[i + 1].Normal.width != 1024 || Textures[i + 1].Normal.height != 1024)
			{
				Textures[i + 1].Normal = TextureScale.Bilinear(Textures[i + 1].Normal, AlbedoSize, AlbedoSize);
			}

			for (int m = 0; m < Textures[i + 1].Normal.mipmapCount; m++)
			{
				NormalArray.SetPixels(Textures[i + 1].Normal.GetPixels(m), i, m);
			}
		}

		//NormalArray.mipMapBias = -0.5f;
		NormalArray.filterMode = FilterMode.Bilinear;
		NormalArray.anisoLevel = 4;
		NormalArray.Apply(true);

		TerrainMaterial.SetTexture("_SplatNormalArray", NormalArray);
	}

	public void UpdateScales(int id)
	{
		if (id == 0)
		{
			TerrainMaterial.SetFloat("_LowerScale", map.Width / Textures[0].AlbedoScale);
			TerrainMaterial.SetFloat("_LowerScaleNormal", map.Width / Textures[0].NormalScale);
		}
		else if (id == 9)
		{
			TerrainMaterial.SetFloat("_UpperScale", map.Width / Textures[9].AlbedoScale);
			TerrainMaterial.SetFloat("_UpperScaleNormal", map.Width / Textures[9].NormalScale);
		}
		else
		{
			string IdStrig = (id - 1).ToString();
			TerrainMaterial.SetFloat("_Splat" + IdStrig + "Scale", map.Width / Textures[id].AlbedoScale);
			TerrainMaterial.SetFloat("_Splat" + IdStrig + "ScaleNormal", map.Width / Textures[id].NormalScale);
		}
	}
	#endregion

	#region Saving
	public void SaveScmapFile()
	{
		if (Teren)
		{
			heights = Teren.terrainData.GetHeights(0, 0, Teren.terrainData.heightmapWidth, Teren.terrainData.heightmapHeight);

			float HeightResize = 512 * 40;
			for (int y = 0; y < map.Width + 1; y++)
			{
				for (int x = 0; x < map.Height + 1; x++)
				{
					map.SetHeight(y, map.Height - x, (short)(heights[x, y] * HeightResize));
				}
			}
		}

		if (MapLuaParser.Current.EditMenu.MapInfoMenu.SaveAsFa.isOn)
		{
			map.VersionMinor = 60;

		}
		else if(map.VersionMinor >= 60)
		{
			if (map.AdditionalSkyboxData.Data.BeginBytes.Length == 0)
			{
				map.AdditionalSkyboxData.Data.BeginBytes = DefaultSkyboxData.BeginBytes;
				map.AdditionalSkyboxData.Data.Albedo = DefaultSkyboxData.Albedo;
				map.AdditionalSkyboxData.Data.Glow = DefaultSkyboxData.Glow;
				map.AdditionalSkyboxData.Data.Length = DefaultSkyboxData.Length;
				//map.AdditionalSkyboxData.Data.MidBytes = DefaultSkyboxData.MidBytes;
				map.AdditionalSkyboxData.Data.MidBytesStatic = DefaultSkyboxData.MidBytesStatic;
				map.AdditionalSkyboxData.Data.Clouds = DefaultSkyboxData.Clouds;
				map.AdditionalSkyboxData.Data.EndBytes = DefaultSkyboxData.EndBytes;
			}
			map.VersionMinor = 56;
		}

		//Debug.Log("Set Heightmap to map " + map.Width + ", " + map.Height);

		//string MapPath = EnvPaths.GetMapsPath();
		string path = MapLuaParser.Current.ScenarioLuaFile.Data.map.Replace("/maps/", MapLuaParser.Current.FolderParentPath);

		//TODO force values if needed
		//map.TerrainShader = Shader;
		map.TerrainShader = MapLuaParser.Current.EditMenu.TexturesMenu.TTerrainXP.isOn ? ("TTerrainXP") : ("TTerrain");

		map.MinimapContourColor = new Color32(0, 0, 0, 255);
		map.MinimapDeepWaterColor = new Color32(71, 140, 181, 255);
		map.MinimapShoreColor = new Color32(141, 200, 225, 255);
		map.MinimapLandStartColor = new Color32(119, 101, 108, 255);
		map.MinimapLandEndColor = new Color32(206, 206, 176, 255);
		//map.MinimapLandEndColor = new Color32 (255, 255, 215, 255);
		map.MinimapContourInterval = 10;

		map.WatermapTex = new Texture2D(map.UncompressedWatermapTex.width, map.UncompressedWatermapTex.height, map.UncompressedWatermapTex.format, false);
		map.WatermapTex.SetPixels(map.UncompressedWatermapTex.GetPixels());
		map.WatermapTex.Apply();
		map.WatermapTex.Compress(true);
		map.WatermapTex.Apply();

		map.NormalmapTex = new Texture2D(map.UncompressedNormalmapTex.width, map.UncompressedNormalmapTex.height, map.UncompressedNormalmapTex.format, false);
		map.NormalmapTex.SetPixels(map.UncompressedNormalmapTex.GetPixels());
		map.NormalmapTex.Apply();
		map.NormalmapTex.Compress(true);
		map.NormalmapTex.Apply();


		for (int i = 0; i < map.Layers.Count; i++)
		{
			map.Layers[i].PathTexture = Textures[i].AlbedoPath;
			map.Layers[i].PathNormalmap = Textures[i].NormalPath;

			map.Layers[i].ScaleTexture = Textures[i].AlbedoScale;
			map.Layers[i].ScaleNormalmap = Textures[i].NormalScale;
		}



		List<Prop> AllProps = new List<Prop>();
		if (EditMap.PropsInfo.AllPropsTypes != null)
		{
			int Count = EditMap.PropsInfo.AllPropsTypes.Count;
			for (int i = 0; i < EditMap.PropsInfo.AllPropsTypes.Count; i++)
			{
				AllProps.AddRange(EditMap.PropsInfo.AllPropsTypes[i].GenerateSupComProps());
			}
		}
		map.Props = AllProps;


		map.Save(path,  map.VersionMinor);
	}

	#endregion

	#region Clean
	public void RestartTerrainAsset()
	{
		int xRes = (int)(256 + 1);
		int zRes = (int)(256 + 1);
		int yRes = (int)(128);
		heights = new float[xRes, zRes];

		// Set Terrain proportives
		Data.heightmapResolution = xRes;
		Data.size = new Vector3(
			256 / 10.0f,
			yRes / 10.0f,
			256 / 10.0f
			);

		if (map != null) WaterLevel.transform.localScale = new Vector3(map.Width * 0.1f, 1f, map.Height * 0.1f);
		if (Teren) Teren.transform.localPosition = new Vector3(-xRes / 20.0f, 1, -zRes / 20.0f);

		// Modify heights array data
		for (int y = 0; y < zRes; y++)
		{
			for (int x = 0; x < xRes; x++)
			{
				heights[x, y] = 0;
			}
		}

		// Set terrain heights from heights array
		Data.SetHeights(0, 0, heights);
	}

	public void UnloadMap()
	{

		Textures[0].AlbedoPath = "/env/evergreen2/layers/eg_gravel005_albedo.dds";
		Textures[0].NormalPath = "/env/tundra/layers/tund_sandlight_normal.dds";
		Textures[0].AlbedoScale = 4;
		Textures[0].NormalScale = 8.75f;

		Textures[1].AlbedoPath = "";
		Textures[1].NormalPath = "";
		Textures[1].AlbedoScale = 4;
		Textures[1].NormalScale = 4;

		Textures[2].AlbedoPath = "";
		Textures[2].NormalPath = "";
		Textures[2].AlbedoScale = 4;
		Textures[2].NormalScale = 4;

		Textures[3].AlbedoPath = "";
		Textures[3].NormalPath = "";
		Textures[3].AlbedoScale = 4;
		Textures[3].NormalScale = 4;

		Textures[4].AlbedoPath = "";
		Textures[4].NormalPath = "";
		Textures[4].AlbedoScale = 4;
		Textures[4].NormalScale = 4;

		Textures[5].AlbedoPath = "";
		Textures[5].NormalPath = "";
		Textures[5].AlbedoScale = 4;
		Textures[5].NormalScale = 4;

		Textures[6].AlbedoPath = "";
		Textures[6].NormalPath = "";
		Textures[6].AlbedoScale = 4;
		Textures[6].NormalScale = 4;

		Textures[7].AlbedoPath = "";
		Textures[7].NormalPath = "";
		Textures[7].AlbedoScale = 4;
		Textures[7].NormalScale = 4;

		Textures[8].AlbedoPath = "";
		Textures[8].NormalPath = "";
		Textures[8].AlbedoScale = 4;
		Textures[8].NormalScale = 4;

		Textures[9].AlbedoPath = "/env/evergreen/layers/macrotexture000_albedo.dds";
		Textures[9].NormalPath = "";
		Textures[9].AlbedoScale = 128;
		Textures[9].NormalScale = 4;


		EditMap.PropsInfo.UnloadProps();
		Markers.MarkersControler.UnloadMarkers();
		DecalsControler.Current.UnloadDecals();
		GenerateControlTex.StopAllTasks();
	}
	#endregion

	#region Converters
	/// <summary>
	/// Convert Scmap position to editor world position
	/// </summary>
	/// <param name="MapPos"></param>
	/// <returns></returns>
	public static Vector3 ScmapPosToWorld(Vector3 MapPos)
	{
		Vector3 ToReturn = MapPos;

		// Position
		ToReturn.x = MapPos.x / 10f;
		ToReturn.z = -MapPos.z / 10f;

		// Height
		ToReturn.y = 1 * (MapPos.y / 10);

		return ToReturn;
	}

	/// <summary>
	/// Convert Editor world position to Scmap position
	/// </summary>
	/// <param name="MapPos"></param>
	/// <returns></returns>
	public static Vector3 WorldPosToScmap(Vector3 MapPos)
	{
		Vector3 ToReturn = MapPos;

		//Position
		ToReturn.x = MapPos.x * 10;
		ToReturn.z = MapPos.z * -10f;

		// Height
		ToReturn.y = MapPos.y * 10;

		return ToReturn;
	}

	public static Vector3 SnapToSmallGridCenter(Vector3 Pos)
	{
		Pos.x += 0.05f;
		Pos.z -= 0.05f;

		Pos.x *= 20;
		Pos.x = (int)(Pos.x + 0.0f);
		Pos.x /= 20.0f;

		Pos.z *= 20;
		Pos.z = (int)(Pos.z + 0.0f);
		Pos.z /= 20.0f;

		return Pos;
	}

	public static Vector3 SnapToSmallGrid(Vector3 Pos)
	{
		Pos.x *= 20;
		Pos.x = (int)(Pos.x + 0.0f);
		Pos.x /= 20.0f;

		Pos.z *= 20;
		Pos.z = (int)(Pos.z + 0.0f);
		Pos.z /= 20.0f;

		return Pos;
	}

	public static Vector3 SnapToGridCenter(Vector3 Pos, bool SampleHeight = false, bool MinimumWaterLevel = false)
	{
		Pos.x += 0.1f;
		//Pos.z += 0.1f;

		Pos.x *= 10;
		Pos.x = (int)(Pos.x);
		Pos.x /= 10.0f;

		Pos.z *= 10;
		Pos.z = (int)(Pos.z);
		Pos.z /= 10.0f;

		Pos.x -= 0.05f;
		Pos.z -= 0.05f;

		if (SampleHeight)
			Pos.y = Current.Teren.SampleHeight(Pos);
		if (MinimumWaterLevel)
			Pos.y = Mathf.Clamp(Pos.y, GetWaterLevel(), 10000);
		return Pos;
	}

	public static Vector3 ClampToWater(Vector3 Pos)
	{
		Pos.y = Mathf.Clamp(Pos.y, GetWaterLevel(), 10000);
		return Pos;
	}

	public static Vector3 SnapToGrid(Vector3 Pos)
	{
		Pos.x *= 10;
		Pos.x = (int)(Pos.x);
		Pos.x /= 10.0f;

		Pos.z *= 10;
		Pos.z = (int)(Pos.z);
		Pos.z /= 10.0f;

		return Pos;
	}

	public static float GetWaterLevel()
	{
		if (!Current.map.Water.HasWater)
			return 0;
		return Current.WaterLevel.localPosition.y;
	}

	#endregion

	#region Rendering
	public void ToogleGrid(bool To)
	{
		Grid = To;
		TerrainMaterial.SetInt("_Grid", Grid ? 1 : 0);
	}

	public void ToogleSlope(bool To)
	{
		Slope = To;
		TerrainMaterial.SetInt("_Slope", Slope ? 1 : 0);
		if (To)
		{
			GenerateControlTex.Current.GenerateSlopeTexture();

		}

	}

	public void ToogleShader()
	{
		TerrainMaterial.SetInt("_TTerrainXP", (MapLuaParser.Current.EditMenu.TexturesMenu.TTerrainXP.isOn) ? 1 : 0);
	}
	#endregion
}
