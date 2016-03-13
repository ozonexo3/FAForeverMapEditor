using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class ScmapEditor : MonoBehaviour {

	public		Terrain			Teren;
	public		TerrainData		Data;
	public		Transform		WaterLevel;
	public		MapLuaParser	Scenario;
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
	
	public Map map;

	void Start(){
		ToogleGrid(false);
		heights = new float[10,10];
		RestartTerrain();
	}
	
	public IEnumerator LoadScmapFile(){

		map = new Map();

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string path = Scenario.ScenarioData.Scmap.Replace("/maps/", MapPath);

	/*	string path = Application.dataPath + Scenario.ScenarioData.Scmap;
#if UNITY_EDITOR
		path = path.Replace("Assets/", "");
#endif*/
		Debug.Log("Load SCMAP file: " + path);


		if(map.Load(path)){
			//printMapDebug(map);
			Sun.transform.rotation = Quaternion.LookRotation( - map.SunDirection);
			float lightScale = (map.SunColor.x + map.SunColor.y + map.SunColor.z) / 3;
			Sun.color = new Color(map.SunColor.x, map.SunColor.y , map.SunColor.z, 1) ;
			Sun.intensity = map.LightingMultiplier * 1.0f;
			//RenderSettings.ambientLight = new Color(map.SunAmbience.x, map.SunAmbience.y, map.SunAmbience.z, 1);
			RenderSettings.ambientLight = new Color(map.ShadowFillColor.x, map.ShadowFillColor.y, map.ShadowFillColor.z, 1);
			//Sun.shadowStrength = 1 - (map.ShadowFillColor.x + map.ShadowFillColor.y + map.ShadowFillColor.z) / 3;

			Kamera.GetComponent<Bloom>().bloomIntensity = map.Bloom * 4;

			RenderSettings.fogColor = new Color(map.FogColor.x, map.FogColor.y, map.FogColor.z, 1);
			RenderSettings.fogStartDistance = map.FogStart * 2;
			RenderSettings.fogEndDistance = map.FogEnd * 2;

			TerrainMaterial.SetFloat("_LightingMultiplier", map.LightingMultiplier);
			TerrainMaterial.SetColor("_SunColor",  new Color(map.SunColor.x, map.SunColor.y, map.SunColor.z, 1));
			TerrainMaterial.SetColor("_SunAmbience",  new Color(map.SunAmbience.x, map.SunAmbience.y, map.SunAmbience.z, 1));
			TerrainMaterial.SetColor("_ShadowColor",  new Color(map.ShadowFillColor.x * 0.5f, map.ShadowFillColor.y * 0.5f, map.ShadowFillColor.z * 0.5f, 1));
		}
		else{
			Debug.LogError("File not found");
			StopCoroutine( "LoadScmapFile" );
		}

		Scenario.ScenarioData.MaxHeight = map.Water.Elevation;
		MapLuaParser.Water = map.Water.HasWater;
		WaterLevel.gameObject.SetActive(MapLuaParser.Water);

		// Set Variables
		int xRes = (int)Scenario.ScenarioData.Size.x;
		int zRes = (int)Scenario.ScenarioData.Size.y;
		float yRes = (float)map.HeightScale;;
		float HeightResize = 512 * 40;

		WaterMaterial.SetTexture("_UtilitySamplerC", map.WatermapTex);
		WaterMaterial.SetFloat("_WaterScale", xRes / -10f);
		//

		                     

//*****************************************
// ***** Set Terrain proportives
//*****************************************
		if(Teren) DestroyImmediate(Teren.gameObject);
		//Teren = null;


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
			Gamedata.LoadTextureFromGamedata("env.scd", Textures[i].AlbedoPath, i, false);
			yield return null;
			Gamedata.LoadTextureFromGamedata("env.scd", Textures[i].NormalPath, i, true);
			yield return null;
		}
		// LoadTextures
		SplatPrototype[] tex = new SplatPrototype [Textures.Length - 1];

		for (int i = 0; i < tex.Length; i++) {
			tex[i] = new SplatPrototype ();
			tex[i].texture = Textures[i + 1].Albedo; 
			tex[i].normalMap = Textures[i + 1].Normal;
			tex[i].tileSize = Textures[i + 1].Tilling;
			tex[i].metallic = 0;
			tex[i].smoothness = 0.5f;
		}

		Data.splatPrototypes = tex;

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
			xRes / 10.0f,
			yRes * MapHeightScale,
			zRes / 10.0f
			);
		Data.SetDetailResolution((int)(xRes / 2), 8);
		Data.baseMapResolution = (int)(xRes / 2);
		Data.alphamapResolution = (int)(xRes / 2);

		Teren.transform.localPosition = new Vector3(0, 0, -zRes / 10.0f);


		WaterLevel.transform.localScale = new Vector3(xRes / 10, 1, zRes / 10);
		WaterLevel.transform.position = Vector3.up * (map.Water.Elevation / 10.0f);
		TerrainMaterial.SetFloat("_WaterLevel", map.Water.Elevation / 10.0f);
		TerrainMaterial.SetFloat("_AbyssLevel", map.Water.ElevationAbyss / 10.0f);


		TerrainMaterial.SetInt("_Water", MapLuaParser.Water?1:0);
		TerrainMaterial.SetFloat("_LowerScale", Textures[0].AlbedoScale / Textures[1].AlbedoScale);
		TerrainMaterial.SetTexture("_SplatLower", Textures[0].Albedo);
		TerrainMaterial.SetTexture("_NormalLower", Textures[0].Normal);
		TerrainMaterial.SetTexture("_UtilitySamplerC", map.WatermapTex);
		TerrainMaterial.SetFloat("_GridScale", xRes / 10f);


		heights = new float[map.Width + 1, map.Height + 1];
		// Modify heights array data
		for (int y = 0; y < map.Width + 1; y++) {
			for (int x = 0; x < map.Height + 1; x++) {
				heights[x,y] = map.GetHeight(y, map.Height - x) / HeightResize ;
			}
		}

		// Set terrain heights from heights array
		Data.SetHeights(0, 0, heights);


		// Mask textures
		float[,,] maps = new float[Data.alphamapWidth, Data.alphamapHeight, 4];
		Debug.Log("Load maps: " + Data.alphamapWidth);

		for(int i = 0; i < Data.alphamapWidth; i++){
			for(int e = 0; e < Data.alphamapHeight; e++){

				float stratum1 = map.TexturemapTex.GetPixel(e, Data.alphamapWidth - i - 1).b;
				float stratum2 = map.TexturemapTex.GetPixel(e, Data.alphamapWidth - i - 1).g;
				float stratum3 = map.TexturemapTex.GetPixel(e, Data.alphamapWidth - i - 1).r;
				float stratum4 = map.TexturemapTex.GetPixel(e, Data.alphamapWidth - i - 1).a;

				maps[i, e, 0] = stratum1; // stratum 1
				maps[i, e, 1] = stratum2; // stratum 2
				maps[i, e, 2] = stratum3; // stratum 3
				maps[i, e, 3] = stratum4; // stratum 4

			}
		}
		yield return null;
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

		Data.SetAlphamaps(0, 0, maps);
		Teren.gameObject.layer = 8;
		Teren.heightmapPixelError = 0;

		string AllProps = "";

		for(int i = 0; i < map.Props.Count; i++){
			if( !map.Props[i].BlueprintPath.Contains("pine")){
				AllProps += map.Props[i].BlueprintPath + "\n";
			}
		}
		Debug.Log("All Props\n" + AllProps);
		yield return null;
	}

	public void SaveScmapFile(){

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string path = Scenario.ScenarioData.Scmap.Replace("/maps/", MapPath);

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
		//Data.SetDetailResolution((int)(256 / 2), 8);
		//Data.baseMapResolution = (int)(256 / 2);
		//Data.alphamapResolution = (int)(256 / 2);
		
		WaterLevel.transform.localScale = new Vector3(xRes, Scenario.ScenarioData.WaterLevels.x, Scenario.ScenarioData.Size.y);
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
		//ToReturn.x =  1 * (MapPos.x / Scenario.ScenarioData.Size.x) * (Scenario.ScenarioData.Size.x / 10);
		//ToReturn.z = - 1 * (MapPos.z / Scenario.ScenarioData.Size.y) * (Scenario.ScenarioData.Size.y / 10);
		ToReturn.x = MapPos.x / 10f;
		ToReturn.z = -MapPos.z / 10f;
		
		// Height
		ToReturn.y =  1 * (MapPos.y / 10);
		
		return ToReturn;
	}

	public Vector3 MapWorldPosInSave(Vector3 MapPos){
		Vector3 ToReturn = MapPos;
		
		// Position
		//ToReturn.x = (MapPos.x / (Scenario.ScenarioData.Size.x / 10)) * (Scenario.ScenarioData.Size.x) - 0.5f;
		//ToReturn.z = (MapPos.z / -(Scenario.ScenarioData.Size.y / 10)) * (Scenario.ScenarioData.Size.y) - 0.5f;

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
