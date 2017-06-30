// ***************************************************************************************
// * SCmap editor
// * Set Unity objects and scripts using data loaded from Scm
// ***************************************************************************************
using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class ScmapEditor : MonoBehaviour {

	static bool SaveStratumToPng = false; // Can save stratum masks to debug if everything is ok

	public		ResourceBrowser	ResBrowser;
	public		Terrain			Teren;
	public		TerrainData		Data;
	public		Transform		WaterLevel;
	public		Camera			Kamera;
	private		float[,] 		heights = new float[1,1];
	public		Light			Sun;
	public		TerrainTexture[]	Textures;
	public		Material			TerrainMaterial;
	public		Material			WaterMaterial;
	public		float			MapHeightScale = 1;
	public		GetGamedataFile	Gamedata;
	public		bool			Grid;
	public		bool			Slope;
	public		TerrainMesh		TerrainM;

	public		string			Shader;

	// Stratum Layer
	[System.Serializable]
	public class TerrainTexture{
		public	 Texture2D	Albedo;
		public	 Texture2D	Normal;
		public	Vector2		Tilling = Vector2.one;
		//Scmap Data
		public	string		AlbedoPath;
		public	string		NormalPath;
		public	float		AlbedoScale;
		public	float		NormalScale;
	}

	public SkyboxData.SkyboxValues DefaultSkyboxData;
	public Map map; // Map Object

	void Awake(){
		ResBrowser.Instantiate ();
	}

	public Mesh LoadedTestModel;
	public MeshFilter TestFilter;

	void Start(){
		ToogleGrid(false);
		heights = new float[10,10];
		RestartTerrain();

		GetGamedataFile.LoadProp("env.scd", "env/Evergreen/Props/Trees/Groups/Pine07_GroupA_prop.bp");
		//LoadedTestModel = GetGamedataFile.LoadModel("units.scd", "units/UEL0001/UEL0001_LOD0.scm");
		LoadedTestModel = GetGamedataFile.LoadModel("env.scd", "env/Evergreen/Props/Trees/Groups/Pine07_groupA_lod0.scm");

		TestFilter.sharedMesh = LoadedTestModel;
	}
	
	public IEnumerator LoadScmapFile(){

		map = new Map();

		string MapPath = EnvPaths.GetMapsPath();
		string path = MapLuaParser.Current.ScenarioData.Scmap.Replace("/maps/", MapPath);

		Debug.Log("Load SCMAP file: " + path);


		if(map.Load(path)){
			Vector3 SunDIr = new Vector3(-map.SunDirection.x, -map.SunDirection.y, map.SunDirection.z);
			Sun.transform.rotation = Quaternion.LookRotation( SunDIr);
			Sun.color = new Color(map.SunColor.x, map.SunColor.y , map.SunColor.z, 1) ;
			Sun.intensity = map.LightingMultiplier * 0.5f;
			RenderSettings.ambientLight = new Color(map.ShadowFillColor.x, map.ShadowFillColor.y, map.ShadowFillColor.z, 1);

			Kamera.GetComponent<Bloom>().bloomIntensity = map.Bloom * 4;

			RenderSettings.fogColor = new Color(map.FogColor.x, map.FogColor.y, map.FogColor.z, 1);
			RenderSettings.fogStartDistance = map.FogStart * 2;
			RenderSettings.fogEndDistance = map.FogEnd * 2;

			TerrainMaterial.SetFloat("_LightingMultiplier", map.LightingMultiplier);
			TerrainMaterial.SetColor("_SunColor",  new Color(map.SunColor.x * 0.5f, map.SunColor.y * 0.5f, map.SunColor.z * 0.5f, 1));
			TerrainMaterial.SetColor("_SunAmbience",  new Color(map.SunAmbience.x * 0.5f, map.SunAmbience.y * 0.5f, map.SunAmbience.z * 0.5f, 1));
			TerrainMaterial.SetColor("_ShadowColor",  new Color(map.ShadowFillColor.x * 0.5f, map.ShadowFillColor.y * 0.5f, map.ShadowFillColor.z * 0.5f, 1));
		}
		else{
			Debug.LogError("File not found");
			StopCoroutine( "LoadScmapFile" );
		}


		if (map.VersionMinor >= 60)
		{
			/*
			// Create Default Values
			DefaultSkyboxData = new SkyboxData.SkyboxValues();
			DefaultSkyboxData.BeginBytes = map.AdditionalSkyboxData.Data.BeginBytes;
			DefaultSkyboxData.Albedo = map.AdditionalSkyboxData.Data.Albedo;
			DefaultSkyboxData.Glow = map.AdditionalSkyboxData.Data.Glow;
			DefaultSkyboxData.Length = map.AdditionalSkyboxData.Data.Length;
			DefaultSkyboxData.MidBytes = map.AdditionalSkyboxData.Data.MidBytes;
			DefaultSkyboxData.MidBytesStatic = map.AdditionalSkyboxData.Data.MidBytesStatic;
			DefaultSkyboxData.Clouds = map.AdditionalSkyboxData.Data.Clouds;
			DefaultSkyboxData.EndBytes = map.AdditionalSkyboxData.Data.EndBytes;*/
		}
		else
		{
			map.AdditionalSkyboxData.Data = new SkyboxData.SkyboxValues();
			map.AdditionalSkyboxData.Data.BeginBytes = DefaultSkyboxData.BeginBytes;
			map.AdditionalSkyboxData.Data.Albedo = DefaultSkyboxData.Albedo;
			map.AdditionalSkyboxData.Data.Glow = DefaultSkyboxData.Glow;
			map.AdditionalSkyboxData.Data.Length = DefaultSkyboxData.Length;
			map.AdditionalSkyboxData.Data.MidBytes = DefaultSkyboxData.MidBytes;
			map.AdditionalSkyboxData.Data.MidBytesStatic = DefaultSkyboxData.MidBytesStatic;
			map.AdditionalSkyboxData.Data.Clouds = DefaultSkyboxData.Clouds;
			map.AdditionalSkyboxData.Data.EndBytes = DefaultSkyboxData.EndBytes;

		}


		Shader = map.TerrainShader;

		MapLuaParser.Current.ScenarioData.MaxHeight = map.Water.Elevation;
		MapLuaParser.Water = map.Water.HasWater;
		WaterLevel.gameObject.SetActive(map.Water.HasWater);

		// Set Variables
		int xRes = (int)MapLuaParser.Current.ScenarioData.Size.x;
		int zRes = (int)MapLuaParser.Current.ScenarioData.Size.y;
		float HalfxRes = xRes / 10f;
		float HalfzRes = zRes / 10f;

		float yRes = (float)map.HeightScale;;
		float HeightResize = 512 * 40;

		WaterMaterial.SetTexture("_UtilitySamplerC", map.UncompressedWatermapTex);
		WaterMaterial.SetFloat("_WaterScale", HalfxRes);

		                     

//*****************************************
// ***** Set Terrain proportives
//*****************************************
		if(Teren) DestroyImmediate(Teren.gameObject);

		// Load Stratum Textures Paths
		for (int i = 0; i < Textures.Length; i++) {
			Textures[i].AlbedoPath = map.Layers[i].PathTexture;
			Textures[i].NormalPath = map.Layers[i].PathNormalmap;
			if(Textures[i].AlbedoPath.StartsWith("/")){
				Textures[i].AlbedoPath = Textures[i].AlbedoPath.Remove(0, 1);
			}
			if(Textures[i].NormalPath.StartsWith("/")){
				Textures[i].NormalPath = Textures[i].NormalPath.Remove(0, 1);
			}
			Textures[i].AlbedoScale = map.Layers[i].ScaleTexture;
			Textures[i].NormalScale = map.Layers[i].ScaleNormalmap;

			try
			{
				Gamedata.LoadTextureFromGamedata("env.scd", Textures[i].AlbedoPath, i, false);
			}
			catch(System.Exception e)
			{
				Debug.LogError(i + ", Albedo tex: " + Textures[i].AlbedoPath);
				Debug.LogError(e);
			}
			yield return null;
			try
			{
				Gamedata.LoadTextureFromGamedata("env.scd", Textures[i].NormalPath, i, true);
			}
			catch (System.Exception e)
			{
				Debug.LogError(i + ", Normal tex: " + Textures[i].NormalPath);
				Debug.LogError(e);
			}
			yield return null;
		}

		Teren = Terrain.CreateTerrainGameObject( Data ).GetComponent<Terrain>();
		Teren.gameObject.name = "TERRAIN";
		Teren.materialType = Terrain.MaterialType.Custom;
		Teren.materialTemplate = TerrainMaterial;
		Teren.heightmapPixelError = 1;
		Teren.basemapDistance = 10000;
		Teren.castShadows = false;
		Teren.drawTreesAndFoliage = false;
		Teren.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;


		Data.heightmapResolution = (int)(xRes + 1);
		Data.size = new Vector3(
			HalfxRes,
			yRes * MapHeightScale,
			HalfzRes
			);
		Data.SetDetailResolution((int)(xRes / 2), 8);
		Data.baseMapResolution = (int)(xRes / 2);
		Data.alphamapResolution = (int)(xRes / 2);

		Teren.transform.localPosition = new Vector3(0, 0, -HalfzRes);


		WaterLevel.transform.localScale = new Vector3(HalfxRes, 1, HalfzRes);
		WaterLevel.transform.position = Vector3.up * (map.Water.Elevation / 10.0f);
		TerrainMaterial.SetFloat("_WaterLevel", map.Water.Elevation / 10.0f);
		TerrainMaterial.SetFloat("_AbyssLevel", map.Water.ElevationAbyss / 10.0f);
		TerrainMaterial.SetInt("_Water", MapLuaParser.Water?1:0);
		TerrainMaterial.SetTexture("_UtilitySamplerC", map.UncompressedWatermapTex);
		TerrainMaterial.SetFloat("_GridScale", HalfxRes);


		heights = new float[map.Width + 1, map.Height + 1];
		// Modify heights array data
		for (int y = 0; y < map.Width + 1; y++) {
			for (int x = 0; x < map.Height + 1; x++) {
				heights[x,y] = map.GetHeight(y, map.Height - x) / HeightResize ;
			}
		}

		// Set terrain heights from heights array
		Data.SetHeights(0, 0, heights);

		//TerrainM.Heights = heights;
		//TerrainM.GenerateMesh ();

		// Save stratum mask to files
		if(SaveStratumToPng){
			byte[] bytes;
			string filename = "temfiles/tex1";
			bytes =  map.TexturemapTex.EncodeToPNG();
			filename += ".png";
			System.IO.File.WriteAllBytes(filename, bytes);

			bytes = null;
			filename = "temfiles/tex2";
			bytes =  map.TexturemapTex2.EncodeToPNG();
			filename += ".png";
			System.IO.File.WriteAllBytes(filename, bytes);
		}


		Teren.gameObject.layer = 8;
		Teren.heightmapPixelError = 0; // Force terrain pixel error to 0, to get more sharp terrain

		SetTextures ();
		yield return null;
	}

	public void SetTextures(int OnlyOne = -1){

		if (OnlyOne < 0) {
			TerrainMaterial.SetTexture ("_ControlXP", map.TexturemapTex);
			if (Textures [5].Albedo || Textures [6].Albedo || Textures [7].Albedo || Textures [8].Albedo)
				TerrainMaterial.SetTexture ("_Control2XP", map.TexturemapTex2);
		}


		if (OnlyOne <= 0) {
			TerrainMaterial.SetFloat("_LowerScale", map.Width / Textures[0].AlbedoScale);
			TerrainMaterial.SetFloat("_LowerScaleNormal", map.Width / Textures[0].NormalScale);
			TerrainMaterial.SetTexture("_SplatLower", Textures[0].Albedo);
			TerrainMaterial.SetTexture("_NormalLower", Textures[0].Normal);
		}

		if (OnlyOne > 0 && OnlyOne < 9) {
			string IdStrig = (OnlyOne - 1).ToString ();
			TerrainMaterial.SetTexture ("_Splat" + IdStrig + "XP", Textures [OnlyOne].Albedo);
			TerrainMaterial.SetFloat ("_Splat" + IdStrig + "Scale", map.Width / Textures [OnlyOne].AlbedoScale);
			TerrainMaterial.SetTexture ("_SplatNormal" + IdStrig, Textures [OnlyOne].Normal);
			TerrainMaterial.SetFloat ("_Splat" + IdStrig + "ScaleNormal", map.Width / Textures [OnlyOne].NormalScale);
		}else {

			TerrainMaterial.SetTexture ("_Splat0XP", Textures [1].Albedo);
			TerrainMaterial.SetTexture ("_Splat1XP", Textures [2].Albedo);
			TerrainMaterial.SetTexture ("_Splat2XP", Textures [3].Albedo);
			TerrainMaterial.SetTexture ("_Splat3XP", Textures [4].Albedo);
			TerrainMaterial.SetTexture ("_Splat4XP", Textures [5].Albedo);
			TerrainMaterial.SetTexture ("_Splat5XP", Textures [6].Albedo);
			TerrainMaterial.SetTexture ("_Splat6XP", Textures [7].Albedo);
			TerrainMaterial.SetTexture ("_Splat7XP", Textures [8].Albedo);

			TerrainMaterial.SetFloat ("_Splat0Scale", map.Width / Textures [1].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat1Scale", map.Width / Textures [2].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat2Scale", map.Width / Textures [3].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat3Scale", map.Width / Textures [4].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat4Scale", map.Width / Textures [5].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat5Scale", map.Width / Textures [6].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat6Scale", map.Width / Textures [7].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat7Scale", map.Width / Textures [8].AlbedoScale);

			TerrainMaterial.SetTexture ("_SplatNormal0", Textures [1].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal1", Textures [2].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal2", Textures [3].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal3", Textures [4].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal4", Textures [5].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal5", Textures [6].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal6", Textures [7].Normal);
			TerrainMaterial.SetTexture ("_SplatNormal7", Textures [8].Normal);


			TerrainMaterial.SetFloat ("_Splat0ScaleNormal", map.Width / Textures [1].NormalScale);
			TerrainMaterial.SetFloat ("_Splat1ScaleNormal", map.Width / Textures [2].NormalScale);
			TerrainMaterial.SetFloat ("_Splat2ScaleNormal", map.Width / Textures [3].NormalScale);
			TerrainMaterial.SetFloat ("_Splat3ScaleNormal", map.Width / Textures [4].NormalScale);
			TerrainMaterial.SetFloat ("_Splat4ScaleNormal", map.Width / Textures [5].NormalScale);
			TerrainMaterial.SetFloat ("_Splat5ScaleNormal", map.Width / Textures [6].NormalScale);
			TerrainMaterial.SetFloat ("_Splat6ScaleNormal", map.Width / Textures [7].NormalScale);
			TerrainMaterial.SetFloat ("_Splat7ScaleNormal", map.Width / Textures [8].NormalScale);
		}

		if (OnlyOne == 9 || OnlyOne < 0) {
			TerrainMaterial.SetFloat ("_UpperScale", map.Width / Textures [9].AlbedoScale);
			TerrainMaterial.SetFloat ("_UpperScaleNormal", map.Width / Textures [9].NormalScale);
			TerrainMaterial.SetTexture ("_SplatUpper", Textures [9].Albedo);
			TerrainMaterial.SetTexture ("_NormalUpper", Textures [9].Normal);
		}
	}

	public void UpdateScales(int id){
		if (id == 0) {
			TerrainMaterial.SetFloat ("_LowerScale", map.Width / Textures [0].AlbedoScale);
			TerrainMaterial.SetFloat ("_LowerScaleNormal", map.Width / Textures [0].NormalScale);

		} else if (id == 9) {
			TerrainMaterial.SetFloat ("_UpperScale", map.Width / Textures [9].AlbedoScale);
			TerrainMaterial.SetFloat ("_UpperScaleNormal", map.Width / Textures [9].NormalScale);

		} else {
			string IdStrig = (id - 1).ToString ();
			TerrainMaterial.SetFloat ("_Splat" + IdStrig + "Scale", map.Width / Textures [id].AlbedoScale);
			TerrainMaterial.SetFloat ("_Splat" + IdStrig + "ScaleNormal", map.Width / Textures [id].NormalScale);
		}
	}

	public void SaveScmapFile(){
		heights = Teren.terrainData.GetHeights(0,0,Teren.terrainData.heightmapWidth, Teren.terrainData.heightmapHeight);

		float HeightResize = 512 * 40;
		for (int y = 0; y < map.Width + 1; y++) {
			for (int x = 0; x < map.Height + 1; x++) {
				map.SetHeight(y, map.Height - x,  (short)(heights[x,y] * HeightResize) );
			}
		}
		Debug.Log("Set Heightmap to map " + map.Width + ", " + map.Height);

		string MapPath = EnvPaths.GetMapsPath();
		string path = MapLuaParser.Current.ScenarioData.Scmap.Replace("/maps/", MapPath);

		//TODO force values if needed
		//map.TerrainShader = Shader;
		map.TerrainShader = MapLuaParser.Current.EditMenu.TexturesMenu.TTerrainXP.isOn?("TTerrainXP"):("TTerrain");

		map.MinimapContourColor = new Color32 (0, 0, 0, 255);
		map.MinimapDeepWaterColor = new Color32 (71, 140, 181, 255);
		map.MinimapShoreColor = new Color32 (141, 200, 225, 255);
		map.MinimapLandStartColor = new Color32 (119, 101, 108, 255);
		map.MinimapLandEndColor = new Color32 (206, 206, 176, 255);
		//map.MinimapLandEndColor = new Color32 (255, 255, 215, 255);
		map.MinimapContourInterval = 10;

		map.WatermapTex = new Texture2D (map.UncompressedWatermapTex.width, map.UncompressedWatermapTex.height, map.UncompressedWatermapTex.format, false);
		map.WatermapTex.SetPixels (map.UncompressedWatermapTex.GetPixels ());
		map.WatermapTex.Apply ();
		map.WatermapTex.Compress (true);
		map.WatermapTex.Apply ();


		for (int i = 0; i < map.Layers.Count; i++) {
			map.Layers [i].PathTexture = Textures [i].AlbedoPath;
			map.Layers [i].PathNormalmap = Textures [i].NormalPath;

			map.Layers [i].ScaleTexture = Textures [i].AlbedoScale;
			map.Layers [i].ScaleNormalmap = Textures [i].NormalScale;
		}

		map.Save(path, map.VersionMinor);
	}

	public void RestartTerrain(){
		int xRes = (int)(256 + 1);
		int zRes = (int)(256 + 1);
		int yRes = (int)(128);
		heights = new float[xRes,zRes];
		
		// Set Terrain proportives
		Data.heightmapResolution = xRes;
		Data.size = new Vector3(
			256 / 10.0f,
			yRes / 10.0f,
			256 / 10.0f
			);
		
		if(map != null) WaterLevel.transform.localScale = new Vector3(map.Width * 0.1f, MapLuaParser.Current.ScenarioData.WaterLevels.x, map.Height * 0.1f);
		if(Teren) Teren.transform.localPosition = new Vector3(-xRes / 20.0f, 1, -zRes / 20.0f);
		
		// Modify heights array data
		for (int y = 0; y < zRes; y++) {
			for (int x = 0; x < xRes; x++) {
				heights[x,y] = 0;
			}
		}
		
		// Set terrain heights from heights array
		Data.SetHeights(0, 0, heights);
	}

	public Vector3 MapPosInWorld(Vector3 MapPos){
		Vector3 ToReturn = MapPos;
		
		// Position
		ToReturn.x = MapPos.x / 10f;
		ToReturn.z = -MapPos.z / 10f;
		
		// Height
		ToReturn.y =  1 * (MapPos.y / 10);
		
		return ToReturn;
	}

	public Vector3 MapWorldPosInSave(Vector3 MapPos){
		Vector3 ToReturn = MapPos;

		//Position
		ToReturn.x = MapPos.x * 10;
		ToReturn.z = MapPos.z * -10f;
		
		// Height
		ToReturn.y = MapPos.y * 10;
		
		return ToReturn;
	}

	public void ToogleGrid(bool To){
		Grid = To;
		TerrainMaterial.SetInt("_Grid", Grid?1:0);
	}

	public void ToogleSlope(bool To){
		Slope = To;
		TerrainMaterial.SetInt("_Slope", Slope?1:0);
	}
}
