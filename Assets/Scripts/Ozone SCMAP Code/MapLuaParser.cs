// ***************************************************************************************
// * Simple SupCom map LUA parser
// * TODO : should read all values. Right now it only search for known, hardcoded values
// ***************************************************************************************

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using NLua;

public class MapLuaParser : MonoBehaviour {

	[Header("Objects")]
	public		MarkersRenderer	MarkerRend;
	public		Editing			EditMenu;
	public		Undo			History;
	public		MapHelperGui	HelperGui;
	public		string			FolderName;
	public		string			ScenarioFileName;
	public		Scenario		ScenarioData;
	public		GameObject[]	Prefabs;
	public		Transform		MarkersParent;
	public		GameObject		MapElements;
	public		Transform[]		MapBorders;
	public		CameraControler	CamControll;
	public		ScmapEditor		HeightmapControler;
	public		GetGamedataFile	Gamedata;
	public		GenericInfoPopup	InfoPopup;
	public static		bool			Water;


	[Header("Local Data")]
	public		List<Mex>		Mexes = new List<Mex>();
	public		List<Hydro>		Hydros = new List<Hydro>();
	public		List<Army>		ARMY_ = new List<Army>();
	public		List<Marker>	SiMarkers = new List<Marker>();

	public		List<int>		MexesTrash;
	public		List<int>		HydrosTrash;
	public		List<int>		ArmiesTrash;
	public		List<int>		AiTrash;


	//[HideInInspector]
	public		Vector3			MapCenterPoint;

	public		int				ScriptId = 0;

	public		string			BackupPath;
	public		string			StructurePath;


	[System.Serializable]
	public class Mex{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		Vector3		position;
		public		Vector3		Orientation;
		public		armys		SpawnWithArmy;
	}

	[System.Serializable]
	public class Hydro{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		Vector3		position;
		public		Vector3		Orientation;
		public		armys		SpawnWithArmy;
	}

	[System.Serializable]
	public class Marker{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		string		type;
		public		string		prop;
		public		Vector3		position;
		public		armys		SpawnWithArmy;
		public		float		Size;
		public		Color		Kolor;
	}

	[System.Serializable]
	public class Army{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		Vector3		position;
		public		Vector3		Orientation;
	}

	[System.Serializable]
	public class Scenario{
		public		string			MapName;
		public		string			MapDesc;
		public		string			Preview;
		public		string			Type;
		public		float			Version;
		public		int				Players;
		public		Vector2			Size;
		public		float			MaxHeight;
		public		Vector3			WaterLevels;
		public		float			NoRushRadius;
		public		Vector2[]		NoRushARMY;
		public		string			SaveLua;
		public		string			ScriptLua;
		public		string			Scmap;
		public		bool			DefaultArea;
		public		Rect			Area;
		public		Rect[]			Areas;
	}
	
	public	enum armys 
	{
		none, ARMY1, ARMY2, ARMY3, ARMY4, ARMY5, ARMY6, ARMY7, ARMY8, ARMY9, ARMY10, ARMY11, ARMY12
	}


	Lua env;
	Lua save;

	void Awake(){
		StructurePath = Application.dataPath + "/Structure/";;
		#if UNITY_EDITOR
		StructurePath = StructurePath.Replace("Assets", "");
		#endif

		BrushGenerator.SetScenario(this);

	}

	public IEnumerator ForceLoadMapAtPath(string path){
		path = path.Replace("\\", "/");
		Debug.Log("Load from: " + path);

		string LastMapPatch = PlayerPrefs.GetString("MapsPath", "maps/");

		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = path.Split(NameSeparator);

		FolderName = Names[Names.Length - 2];
		ScenarioFileName = Names[Names.Length - 1].Replace(".lua", "");
		string NewMapPatch = path.Replace(FolderName + "/" + ScenarioFileName + ".lua", "");

		Debug.Log("Parsed args: \n"
			+ FolderName + "\n"
			+ ScenarioFileName + "\n"
			+ FolderName);


		PlayerPrefs.SetString("MapsPath", NewMapPatch);
		loadSave = false;
		LoadProps = false;
		var LoadFile = StartCoroutine("LoadingFile");
		yield return LoadFile;

		InfoPopup.Show(false);
		PlayerPrefs.SetString("MapsPath", LastMapPatch);
	}

	public void LoadFile(){
		StartCoroutine("LoadingFile");
	}

	bool loadSave = true;
	bool LoadProps = true;

	IEnumerator LoadingFile(){
		// Begin load
		InfoPopup.Show(true, "Loading map...");
		yield return null;
		// Scenario LUA
		LoadScenarioLua();
		yield return null;

		// SCMAP
		var LoadScmapFile = HeightmapControler.StartCoroutine( "LoadScmapFile");
		yield return LoadScmapFile;
		CamControll.RestartCam();

		if(loadSave){
		// Save LUA
			LoadSaveLua();
			yield return null;
		}

		// Finish Load
		HelperGui.MapLoaded = true;

		// Load Props
		if(LoadProps){
			PropsReader.LoadProps(HeightmapControler);
			yield return null;
		}

		InfoPopup.Show(false);


	}

	public void SaveFile(){

	}

	public static string FormatException(NLua.Exceptions.LuaException e) {
		string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
		return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
	}

	public void UpdateArea(){

		if(ScenarioData.Area.width == 0 && ScenarioData.Area.height == 0){
			ScenarioData.DefaultArea = false;
			HeightmapControler.TerrainMaterial.SetInt("_Area", ScenarioData.DefaultArea?1:0);
			return;
		}
		HeightmapControler.TerrainMaterial.SetInt("_Area", ScenarioData.DefaultArea?1:0);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaX", ScenarioData.Area.x / 10f);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaY", ScenarioData.Area.y / 10f);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaWidht", ScenarioData.Area.width / 10f);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaHeight", ScenarioData.Area.height / 10f);
	}

	private void LoadScenarioLua(){
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");

		string loadedFile = "";
		Debug.Log("Load file:" + MapPath + FolderName + "/" + ScenarioFileName + ".lua");
		string loc = MapPath + FolderName + "/" + ScenarioFileName + ".lua";
		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		string loadedFileFunctions = "";
		loadedFileFunctions = System.IO.File.ReadAllText(StructurePath + "lua_variable_functions.lua", encodeType);
		
		env = new Lua();
		env.LoadCLRPackage();
		
		try {
			env.DoString(loadedFileFunctions + loadedFile);
		} catch(NLua.Exceptions.LuaException e) {
			Debug.LogError(FormatException(e), gameObject);
			
			HelperGui.MapLoaded = false;
			return;
		}
		
		// Load Map Prop
		ScenarioData.MapName = env.GetTable("ScenarioInfo").RawGet("name").ToString();
		ScenarioData.MapDesc = env.GetTable("ScenarioInfo").RawGet("description").ToString();
		ScenarioData.Type = env.GetTable("ScenarioInfo").RawGet("type").ToString();

		if(env.GetTable("ScenarioInfo").RawGet("map_version") != null && env.GetTable("ScenarioInfo").RawGet("map_version").ToString() != "null"){
			ScenarioData.Version = float.Parse(env.GetTable("ScenarioInfo").RawGet("map_version").ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}
		else{
			ScenarioData.Version = 1;
		}

		if(env.GetTable("ScenarioInfo").RawGet("preview") != null && env.GetTable("ScenarioInfo").RawGet("preview").ToString() != "null"){
			ScenarioData.Preview = env.GetTable("ScenarioInfo").RawGet("preview").ToString();
		}
		else{
			ScenarioData.Preview = "";
		}

		ScenarioData.SaveLua =  env.GetTable("ScenarioInfo").RawGet("save").ToString();
		ScenarioData.Scmap = env.GetTable("ScenarioInfo").RawGet("map").ToString();
		ScenarioData.ScriptLua = env.GetTable("ScenarioInfo").RawGet("script").ToString();
		
		// Load Map Size
		var width = env.GetTable("ScenarioInfo.size")[1].ToString();
		var height = env.GetTable("ScenarioInfo.size")[2].ToString();
		
		ScenarioData.Size.x = float.Parse(width.ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		ScenarioData.Size.y = float.Parse(height.ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		ScenarioData.MaxHeight = 128;
		CamControll.MapSize = Mathf.Max(ScenarioData.Size.x, ScenarioData.Size.y);
		CamControll.RestartCam();
		
		// Load Players
		LuaTable TeamsTab = env.GetTable("ScenarioInfo.Configurations.standard.teams")[1] as LuaTable;
		LuaTable ArmyTab = TeamsTab.RawGet("armies") as LuaTable;
		
		ScenarioData.Players = ArmyTab.Values.Count;

		ScenarioData.NoRushRadius = float.Parse(env.GetTable("ScenarioInfo").RawGet("norushradius").ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		ScenarioData.NoRushARMY = new Vector2[ScenarioData.Players];
		
		for(int id = 0; id < ScenarioData.Players; id++) {
			ScenarioData.NoRushARMY[id] = Vector2.zero;
			if(env.GetTable("ScenarioInfo").RawGet("norushoffsetX_ARMY_" + (id + 1)) != null)
				ScenarioData.NoRushARMY[id].x = float.Parse(env.GetTable("ScenarioInfo").RawGet("norushoffsetX_ARMY_" + (id + 1)).ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			if(env.GetTable("ScenarioInfo").RawGet("norushoffsetY_ARMY_" + (id + 1)) != null)
				ScenarioData.NoRushARMY[id].y = float.Parse(env.GetTable("ScenarioInfo").RawGet("norushoffsetY_ARMY_" + (id + 1)).ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}
	}

	private void LoadSaveLua(){
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loadedFileSave = "";
		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");

		loadedFileSave = System.IO.File.ReadAllText(ScenarioData.SaveLua.Replace("/maps/", MapPath), encodeType);
		
		string loadedFileFunctions = "";
		loadedFileFunctions = System.IO.File.ReadAllText(StructurePath + "lua_variable_functions.lua", encodeType);
		
		string loadedFileEndFunctions = "";
		loadedFileEndFunctions = System.IO.File.ReadAllText(StructurePath + "lua_variable_end_functions.lua", encodeType);
		
		loadedFileSave = loadedFileFunctions + loadedFileSave + loadedFileEndFunctions;
		
		save = new Lua();
		save.LoadCLRPackage();
		
		try {
			save.DoString(loadedFileSave);
		} catch(NLua.Exceptions.LuaException e) {
			Debug.LogError(FormatException(e), gameObject);
			
			HelperGui.MapLoaded = false;
			return;
		}
		// LoadArea
		ScenarioData.Area = new Rect();
		if(save.GetTable("Scenario.Areas.AREA_1") != null && save.GetTable("Scenario.Areas.AREA_1").ToString() != "null"){
			ScenarioData.DefaultArea = true;
			ScenarioData.Area.x = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			ScenarioData.Area.y = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			ScenarioData.Area.width = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			ScenarioData.Area.height = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[4].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			UpdateArea();
		}
		else{
			ScenarioData.DefaultArea = false;
			HeightmapControler.TerrainMaterial.SetInt("_Area", ScenarioData.DefaultArea?1:0);
			MapElements.SetActive(false);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaX", 0);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaY", 0);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaWidht", ScenarioData.Size.x / 10f);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaHeight", ScenarioData.Size.y / 10f);
		}

		MapCenterPoint = Vector3.zero;
		MapCenterPoint.x = (ScenarioData.Size.x / 20);
		MapCenterPoint.z = -1 * (ScenarioData.Size.y / 20);
		
		// LoadMarkers
		int MarkersCount = save.GetTable("Scenario.MasterChain._MASTERCHAIN_.Markers").Values.Count;
		int SpawnCount = 0;
		int MassCount = 0;
		int HydroCount = 0;
		int SiCount = 0;

		LuaTable MasterChain = save.GetTable("Scenario.MasterChain._MASTERCHAIN_.Markers") as LuaTable;
		Mexes = new List<Mex>();
		Hydros = new List<Hydro>();
		ARMY_ = new List<Army>();
		SiMarkers = new List<Marker>();

		for(int m = 0; m < MarkersCount; m++){
			LuaTable MarkerTab = MasterChain[save.GetTable("AllKeyMarkers")[m + 1].ToString()] as LuaTable;
			LuaTable MarkerPos = MarkerTab["position"] as LuaTable;
			if(MarkerTab["type"].ToString() == "Mass"){
				Mexes.Add(new Mex());
				Mexes[MassCount].name = save.GetTable("AllKeyMarkers")[m + 1].ToString();
				
				Mexes[MassCount].position.x = float.Parse(MarkerPos[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				Mexes[MassCount].position.y = float.Parse(MarkerPos[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				Mexes[MassCount].position.z = float.Parse(MarkerPos[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				
				Mexes[MassCount].SpawnWithArmy = armys.ARMY1;
				if(MarkerTab["position"].ToString() != null && MarkerTab["position"].ToString() != "null"){
					switch(MarkerTab["position"].ToString()){
					case "ARMY_1":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY1;
						break;
					case "ARMY_2":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY2;
						break;
					case "ARMY_3":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY3;
						break;
					case "ARMY_4":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY4;
						break;
					case "ARMY_5":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY5;
						break;
					case "ARMY_6":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY6;
						break;
					case "ARMY_7":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY7;
						break;
					case "ARMY_8":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY8;
						break;
					case "ARMY_9":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY9;
						break;
					case "ARMY_10":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY10;
						break;
					case "ARMY_11":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY11;
						break;
					case "ARMY_12":
						Mexes[MassCount].SpawnWithArmy = armys.ARMY12;
						break;
					}
				}
				//GameObject NewMex =  Instantiate(Prefabs[1], Vector3.zero, Quaternion.identity) as GameObject;
				//NewMex.name = Mexes[MassCount].name;
				//Mexes[MassCount].Mark = NewMex.transform;
				//Mexes[MassCount].Mark.parent = MarkersParent;
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(Mexes[MassCount].position);
				//Mexes[MassCount].Mark.localPosition = MexLocPos;
				Mexes[MassCount].position = MexLocPos;
				//Mexes[MassCount].Script = Mexes[MassCount].Mark.gameObject.GetComponent<MarkerScript>();
				//Mexes[MassCount].Script.Typ = MarkerScript.MarkersTypes.Mex;
				//if(Water) Mexes[MassCount].Script.WaterLevel =  ScenarioData.MaxHeight / 12.8f;
				//else  Mexes[MassCount].Script.WaterLevel = 0;
				MassCount++;
				
			}
			else if(MarkerTab["type"].ToString() == "Hydrocarbon"){
				Hydros.Add(new Hydro());
				Hydros[HydroCount].name = save.GetTable("AllKeyMarkers")[m + 1].ToString();
				
				Hydros[HydroCount].position.x = float.Parse(MarkerPos[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				Hydros[HydroCount].position.y = float.Parse(MarkerPos[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				Hydros[HydroCount].position.z = float.Parse(MarkerPos[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				
				Hydros[HydroCount].SpawnWithArmy = armys.ARMY1;
				if(MarkerTab["position"].ToString() != null && MarkerTab["position"].ToString() != "null"){
					switch(MarkerTab["position"].ToString()){
					case "ARMY_1":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY1;
						break;
					case "ARMY_2":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY2;
						break;
					case "ARMY_3":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY3;
						break;
					case "ARMY_4":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY4;
						break;
					case "ARMY_5":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY5;
						break;
					case "ARMY_6":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY6;
						break;
					case "ARMY_7":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY7;
						break;
					case "ARMY_8":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY8;
						break;
					case "ARMY_9":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY9;
						break;
					case "ARMY_10":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY10;
						break;
					case "ARMY_11":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY11;
						break;
					case "ARMY_12":
						Hydros[HydroCount].SpawnWithArmy = armys.ARMY12;
						break;
					}
				}
				//GameObject NewMex =  Instantiate(Prefabs[2], Vector3.zero, Quaternion.identity) as GameObject;
				//NewMex.name = Hydros[HydroCount].name;
				//Hydros[HydroCount].Mark = NewMex.transform;
				//Hydros[HydroCount].Mark.parent = MarkersParent;
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(Hydros[HydroCount].position);
				Hydros[HydroCount].position = MexLocPos;
				//Hydros[HydroCount].Mark.localPosition = MexLocPos;
				//Hydros[HydroCount].Script = Hydros[HydroCount].Mark.gameObject.GetComponent<MarkerScript>();
				//Hydros[HydroCount].Script.Typ = MarkerScript.MarkersTypes.Mex;
				//if(Water) Hydros[HydroCount].Script.WaterLevel = ScenarioData.MaxHeight / 12.8f;
				//else Hydros[HydroCount].Script.WaterLevel = 0;
				
				HydroCount++;
			}
			else if(save.GetTable("AllKeyMarkers")[m + 1].ToString().Contains("ARMY_")){
				
				ARMY_.Add(new Army());
				ARMY_[SpawnCount].name = save.GetTable("AllKeyMarkers")[m + 1].ToString();
				//GameObject NewSpawn =  Instantiate(Prefabs[0], Vector3.zero, Quaternion.identity) as GameObject;
				//ARMY_[SpawnCount].Mark = NewSpawn.transform;
				//ARMY_[SpawnCount].Mark.parent = MarkersParent;
				//Vector3 MexLocPos = Vector3.zero;
				//NewSpawn.name = ARMY_[SpawnCount].name;


				ARMY_[SpawnCount].position.x = float.Parse(MarkerPos[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				ARMY_[SpawnCount].position.y = float.Parse(MarkerPos[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				ARMY_[SpawnCount].position.z = float.Parse(MarkerPos[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				
				//MexLocPos.x = -1 * (ScenarioData.Size.x / 20) + 1 * (ARMY_[SpawnCount].position.x / ScenarioData.Size.x) * (ScenarioData.Size.x / 10);
				//MexLocPos.z = -1 * (ScenarioData.Size.y / 20) + 1 * (ARMY_[SpawnCount].position.z / ScenarioData.Size.y) * (ScenarioData.Size.y / 10);
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(ARMY_[SpawnCount].position);
				ARMY_[SpawnCount].position = MexLocPos;
				//ARMY_[SpawnCount].Mark.localPosition = MexLocPos;
				//ARMY_[SpawnCount].Script = ARMY_[SpawnCount].Mark.gameObject.GetComponent<MarkerScript>();
				//ARMY_[SpawnCount].Script.Typ = MarkerScript.MarkersTypes.Army;
				//if(Water) ARMY_[SpawnCount].Script.WaterLevel = ScenarioData.MaxHeight / 12.8f;
				//else ARMY_[SpawnCount].Script.WaterLevel = 0;
				SpawnCount++;
			}
			else{
				SiMarkers.Add(new Marker());
				SiMarkers[SiCount].name = save.GetTable("AllKeyMarkers")[m + 1].ToString();
				
				SiMarkers[SiCount].position.x = float.Parse(MarkerPos[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				SiMarkers[SiCount].position.y = float.Parse(MarkerPos[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				SiMarkers[SiCount].position.z = float.Parse(MarkerPos[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				
				SiMarkers[SiCount].SpawnWithArmy = armys.ARMY1;
				if(MarkerTab["position"].ToString() != null && MarkerTab["position"].ToString() != "null"){
					switch(MarkerTab["position"].ToString()){
					case "ARMY_1":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY1;
						break;
					case "ARMY_2":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY2;
						break;
					case "ARMY_3":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY3;
						break;
					case "ARMY_4":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY4;
						break;
					case "ARMY_5":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY5;
						break;
					case "ARMY_6":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY6;
						break;
					case "ARMY_7":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY7;
						break;
					case "ARMY_8":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY8;
						break;
					case "ARMY_9":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY9;
						break;
					case "ARMY_10":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY10;
						break;
					case "ARMY_11":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY11;
						break;
					case "ARMY_12":
						SiMarkers[SiCount].SpawnWithArmy = armys.ARMY12;
						break;
					}
				}
				//GameObject NewMex;

				switch(MarkerTab["type"].ToString()){
				case "Rally Point":
					//NewMex = Instantiate(Prefabs[3], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				case "Combat Zone":
					//NewMex = Instantiate(Prefabs[4], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				case "Defensive Point":
					//NewMex = Instantiate(Prefabs[5], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				case "Naval Area":
					//NewMex = Instantiate(Prefabs[6], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				default:
					//NewMex = Instantiate(Prefabs[5], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				}

				SiMarkers[SiCount].type = MarkerTab["type"].ToString();
				SiMarkers[SiCount].prop = MarkerTab["prop"].ToString();

				//NewMex.name = SiMarkers[SiCount].name;
				//SiMarkers[SiCount].Mark = NewMex.transform;
				//SiMarkers[SiCount].Mark.parent = MarkersParent;
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(SiMarkers[SiCount].position);
				SiMarkers[SiCount].position = MexLocPos;
				//SiMarkers[SiCount].Mark.localPosition = MexLocPos;
				//SiMarkers[SiCount].Script = SiMarkers[SiCount].Mark.gameObject.GetComponent<MarkerScript>();
				//SiMarkers[SiCount].Script.Typ = MarkerScript.MarkersTypes.Mex;
				//if(Water) SiMarkers[SiCount].Script.WaterLevel = ScenarioData.MaxHeight / 12.8f;
				//else  SiMarkers[SiCount].Script.WaterLevel = 0;
				
				SiCount++;
			}
		}

		SortArmys();
	}


	public void SortArmys(){
		List<Army> NewArmys = new List<Army>();
		for(int i = 0; i < ARMY_.Count; i++){
			for(int a = 0; a < ARMY_.Count; a++){
				if(ARMY_[a].name == "ARMY_" + (i + 1).ToString()){
					NewArmys.Add(ARMY_[a]);
					break;
				}
			}
		}

		ARMY_ = NewArmys;
	}


	public IEnumerator SaveMap(){

		InfoPopup.Show(true, "Saving map...");
		yield return null;

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string BackupId = System.DateTime.Now.Month.ToString() +System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString() + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString();
		BackupPath = MapPath + FolderName + "/Backup_" + BackupId;

		System.IO.Directory.CreateDirectory(BackupPath);

		yield return null;
		SaveScenarioLua();
		yield return null;
		SaveSaveLua();
		yield return null;

		SaveScriptLua(ScriptId);
		yield return null;

		SaveScmap();
		yield return null;

		InfoPopup.Show(false);
	}

	public void SaveScmap(){

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string MapFilePath = ScenarioData.Scmap.Replace("/maps/", MapPath);

		string FileName = ScenarioData.Scmap;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		Debug.Log(BackupPath + "/" + Names[Names.Length - 1]);
		System.IO.File.Move(MapFilePath, BackupPath + "/" + Names[Names.Length - 1]);


		HeightmapControler.SaveScmapFile();
	}

	public void SaveScenarioLua(){
		string SaveData = "";
		string loadedFile = "";
		
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = StructurePath + "scenario_structure.lua";
		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		char[] Separator = "\n".ToCharArray();
		string[] AllLines = loadedFile.Split(Separator);
		foreach(string line in AllLines){
			if(line.Contains("[*name*]")){
				SaveData += "    name = '" + ScenarioData.MapName + "',\n";
			}
			else if(line.Contains("[*desc*]")){
				SaveData += "    description = '" + ScenarioData.MapDesc + "',\n";
			}
			else if(line.Contains("[*size*]")){
				SaveData += "    size = {" + ScenarioData.Size.x + ", " + ScenarioData.Size.y + "},\n";
			}
			else if(line.Contains("[*paths*]")){
				SaveData += "    map = '" + ScenarioData.Scmap + "',\n";
				SaveData += "    save = '" + ScenarioData.SaveLua + "',\n";
				SaveData += "    script = '" + ScenarioData.ScriptLua + "',\n";
				SaveData += "    preview = '" + ScenarioData.Preview + "',\n";
			}
			else if(line.Contains("[*norushoffset*]")){
				SaveData += "    norushradius = " + ScenarioData.NoRushRadius.ToString("0.000000") + ",\n";
				for(int a = 0; a < ScenarioData.NoRushARMY.Length; a++){
					SaveData += "    norushoffsetX_ARMY_" + (a + 1).ToString() + " = " + ScenarioData.NoRushARMY[a].x.ToString("0.000000") + ",\n";
					SaveData += "    norushoffsetY_ARMY_" + (a + 1).ToString() + " = " + ScenarioData.NoRushARMY[a].y.ToString("0.000000") + ",\n";
				}
			}
			else if(line.Contains("[*armies*]")){
				SaveData += "					armies = {";

				for(int a = 0; a < ARMY_.Count; a++){
					SaveData += "'ARMY_" + (a + 1).ToString() + "',";
				}

				SaveData += "}\n";
			}
			else{
				SaveData += line;
			}
		}

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string ScenarioFilePath = MapPath + FolderName + "/" + ScenarioFileName + ".lua";
		//string SavePath = Application.dataPath + ScenarioFilePath.Replace(".lua", "_new.lua");
		//string SavePath = Application.dataPath + ScenarioFilePath;
		//#if UNITY_EDITOR
		//SavePath = SavePath.Replace("Assets/", "");
		//#endif

		System.IO.File.Move(ScenarioFilePath, BackupPath + "/" + ScenarioFileName + ".lua");
		System.IO.File.WriteAllText(ScenarioFilePath, SaveData);
	}


	public void SaveSaveLua(){
		string SaveData = "";
		string loadedFile = "";

		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = StructurePath + "save_structure.lua";
		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		char[] Separator = "\n".ToCharArray();
		string[] AllLines = loadedFile.Split(Separator);

		//Mex
		string MexStructure = System.IO.File.ReadAllText(StructurePath + "save_mex.lua", encodeType);

		//Hydro
		string HydroStructure = System.IO.File.ReadAllText(StructurePath + "save_hydro.lua", encodeType);

		//Army Marker
		string ArmyMarkerStructure = System.IO.File.ReadAllText(StructurePath + "save_armymarker.lua", encodeType);

		//Si
		string SiStructure = System.IO.File.ReadAllText(StructurePath + "save_marker.lua", encodeType);

		//Army
		string ArmyStructure = System.IO.File.ReadAllText(StructurePath + "save_army.lua", encodeType);

		foreach(string line in AllLines){
			if(line.Contains("[*AREA*]")){
				SaveData += "            ['rectangle'] = RECTANGLE( " + ScenarioData.Area.x.ToString() +", "+ ScenarioData.Area.y.ToString() +", "+ ScenarioData.Area.width.ToString() +", "+ ScenarioData.Area.height.ToString() +"),";
				SaveData += "\n";
			}
			else if(line.Contains("[*MARKERS*]")){
				for(int a = 0; a < ARMY_.Count; a++){
					string NewArmy = ArmyMarkerStructure;

					string ArmyName = "                ['" + ARMY_[a].name + "'] = {";
					NewArmy = NewArmy.Replace("['ARMY_']", ArmyName);

					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( GetPosOfMarkerId(0, a) );
					string Position = "                    ['position'] = VECTOR3( "+ MexPos.x.ToString() +", "+ MexPos.y.ToString() +", "+ MexPos.z.ToString() +" ),";
					NewArmy = NewArmy.Replace("['position']", Position);

					SaveData += NewArmy + "\n";
				}

				for(int h = 0; h < Hydros.Count; h++){
					string NewHydro = HydroStructure;
					
					string HydroName = "                ['"+ Hydros[h].name +"'] = {";
					NewHydro = NewHydro.Replace("['Hydrocarbon']", HydroName);
					
					string SpawnWithArmy = "                    ['SpawnWithArmy'] = STRING( '" + Hydros[h].SpawnWithArmy.ToString() + "' ),";
					if( Hydros[h].SpawnWithArmy == armys.none) NewHydro = NewHydro.Replace("['SpawnWithArmy']\n", "");
					else NewHydro = NewHydro.Replace("['SpawnWithArmy']", SpawnWithArmy);
					
					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( GetPosOfMarkerId(2, h) );
					string Position = "                    ['position'] = VECTOR3( "+ MexPos.x.ToString() +", "+ MexPos.y.ToString() +", "+ MexPos.z.ToString() +" ),";
					NewHydro = NewHydro.Replace("['position']", Position);
					
					SaveData += NewHydro + "\n";
				}

				for(int m = 0; m < Mexes.Count; m++){
					string NewMex = MexStructure;

					string MexName = "                ['"+ Mexes[m].name +"'] = {";
					NewMex = NewMex.Replace("['Mass1']", MexName);

					string SpawnWithArmy = "                    ['SpawnWithArmy'] = STRING( '" + Mexes[m].SpawnWithArmy.ToString() + "' ),";
					NewMex = NewMex.Replace("['SpawnWithArmy']", SpawnWithArmy);

					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( GetPosOfMarkerId(1, m) );
					string Position = "                    ['position'] = VECTOR3( "+ MexPos.x.ToString() +", "+ MexPos.y.ToString() +", "+ MexPos.z.ToString() +" ),";
					NewMex = NewMex.Replace("['position']", Position);

					SaveData += NewMex + "\n";
				}

				for(int s = 0; s < SiMarkers.Count; s++){

					string NewSi = SiStructure;


					string SiName = "                ['"+ SiMarkers[s].name +"'] = {";
					NewSi = NewSi.Replace("['Marker']", SiName);

					string SiType = "                    ['type'] = STRING( '" + SiMarkers[s].type + "' ),";
					NewSi = NewSi.Replace("['type']", SiType);

					string SiProp = "                    ['prop'] = STRING( '"+ SiMarkers[s].prop + "' ),";
					NewSi = NewSi.Replace("['prop']", SiProp);


					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( GetPosOfMarkerId(3, s) );
					string Position = "                    ['position'] = VECTOR3( "+ MexPos.x.ToString() +", "+ MexPos.y.ToString() +", "+ MexPos.z.ToString() +" ),";
					NewSi = NewSi.Replace("['position']", Position);
					
					SaveData += NewSi + "\n";

				}
			}
			else if(line.Contains("[*CHAINS*]")){
				
			}
			else if(line.Contains("[*ARMYS*]")){
				for(int a = 0; a < ARMY_.Count; a++){

					SaveData += "        ['ARMY_" + (a + 1).ToString() + "'] =  \n";
					SaveData += ArmyStructure;
					SaveData += "\n";
				}
			}
			else{
				SaveData += line;
			}
		}

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string SaveFilePath = ScenarioData.SaveLua.Replace("/maps/", MapPath);

		string FileName = ScenarioData.SaveLua;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SaveFilePath, BackupPath + "/" + Names[Names.Length - 1]);

		System.IO.File.WriteAllText(SaveFilePath, SaveData);
	}

	public void SaveScriptLua(int ID = 0){
		string SaveData = "";
		string loadedFile = "";
		
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = "";
		if(ID == 0) return;
		else if(ID == 1) loc = StructurePath + "script_structure1.lua";
		else if(ID == 2) loc = StructurePath + "script_structure2.lua";

		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		SaveData = loadedFile;

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string SaveFilePath = ScenarioData.ScriptLua.Replace("/maps/", MapPath);

		string FileName = ScenarioData.ScriptLua;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SaveFilePath, BackupPath + "/" + Names[Names.Length - 1]);
		
		System.IO.File.WriteAllText(SaveFilePath, SaveData);
	}

	public Vector3 GetPosOfMarker(EditMap.EditingMarkers.WorkingElement Element){
		switch(Element.ListId){
		case 0:
			return MarkerRend.Armys[Element.InstanceId].transform.position;
		case 1:
			return MarkerRend.Mex[Element.InstanceId].transform.position;
		case 2:
			return MarkerRend.Hydro[Element.InstanceId].transform.position;
		case 3:
			return MarkerRend.Ai[Element.InstanceId].transform.position;
		}
		return Vector3.zero;
	}

	public Vector3 GetPosOfMarkerId(int list, int instance){
		switch(list){
		case 0:
			return MarkerRend.Armys[instance].transform.position;
		case 1:
			return MarkerRend.Mex[instance].transform.position;
		case 2:
			return MarkerRend.Hydro[instance].transform.position;
		case 3:
			return MarkerRend.Ai[instance].transform.position;
		}
		return Vector3.zero;
	}

	public void SetPosOfMarker(EditMap.EditingMarkers.WorkingElement Element, Vector3 NewPos){
		switch(Element.ListId){
		case 0:
			ARMY_[Element.InstanceId].position = NewPos;
			break;
		case 1:
			Mexes[Element.InstanceId].position = NewPos;
			break;
		case 2:
			Hydros[Element.InstanceId].position = NewPos;
			break;
		case 3:
			SiMarkers[Element.InstanceId].position = NewPos;
			break;
		}
	}

	public GameObject GetMarkerRenderer(EditMap.EditingMarkers.WorkingElement Element){
		switch(Element.ListId){
		case 0:
			return MarkerRend.Armys[Element.InstanceId];
		case 1:
			return MarkerRend.Mex[Element.InstanceId];
		case 2:
			return MarkerRend.Hydro[Element.InstanceId];
		case 3:
			return MarkerRend.Ai[Element.InstanceId];
		}
		return null;
	}

	public void CreateMarker(int MarkerType, Vector3 position, string name){
		if(MarkerType == 0){
			ARMY_.Add(new Army());
			ARMY_[ARMY_.Count - 1].name = "ARMY_" + ARMY_.Count.ToString();
			ARMY_[ARMY_.Count - 1].position = position;
		}
		else if(MarkerType == 1){
			Mexes.Add(new Mex());
			Mexes[Mexes.Count - 1].name = "Mex_" + Mexes.Count.ToString();
			Mexes[Mexes.Count - 1].position = position;
		}
		else if(MarkerType == 2){
			Hydros.Add(new Hydro());
			Hydros[Hydros.Count - 1].name = "Hydro_" + Hydros.Count.ToString();
			Hydros[Hydros.Count - 1].position = position;
		}
		else if(MarkerType == 3){
			SiMarkers.Add(new Marker());
			SiMarkers[SiMarkers.Count - 1].name = "AI_" + SiMarkers.Count.ToString();
			SiMarkers[SiMarkers.Count - 1].position = position;
		}
		EditMenu.EditMarkers.AllMarkersList.UpdateList();
	}

	public void AddMarkerToTrash(EditMap.EditingMarkers.WorkingElement Element){
		switch (Element.ListId) {
		case 0:
			ArmiesTrash.Add(Element.InstanceId);
			break;
		case 1:
			MexesTrash.Add(Element.InstanceId);
			break;
		case 2:
			HydrosTrash.Add(Element.InstanceId);
			break;
		case 3:
			AiTrash.Add(Element.InstanceId);
			break;
		}
	}
	public void CleanMarkersTrash(){
		List<Army>	NewArmy_ = new List<Army> ();
		List<Mex>	NewMexes = new List<Mex> ();
		List<Hydro> NewHydros = new List<Hydro>();
		List<Marker> NewAi = new List<Marker>();

		for (int i = 0; i < ARMY_.Count; i++) {
			bool InTrash = false;
			for(int t = 0; t < ArmiesTrash.Count; t++){
				if(ArmiesTrash[t] == i){
					InTrash = true;
					break;
				}
			}
			if(!InTrash) NewArmy_.Add( ARMY_[i]);
		}

		for (int i = 0; i < Mexes.Count; i++) {
			bool InTrash = false;
			for(int t = 0; t < MexesTrash.Count; t++){
				if(MexesTrash[t] == i){
					InTrash = true;
					break;
				}
			}
			if(!InTrash) NewMexes.Add( Mexes[i]);
		}

		for (int i = 0; i < Hydros.Count; i++) {
			bool InTrash = false;
			for(int t = 0; t < HydrosTrash.Count; t++){
				if(HydrosTrash[t] == i){
					InTrash = true;
					break;
				}
			}
			if(!InTrash) NewHydros.Add( Hydros[i]);
		}
		for (int i = 0; i < SiMarkers.Count; i++) {
			bool InTrash = false;
			for(int t = 0; t < AiTrash.Count; t++){
				if(AiTrash[t] == i){
					InTrash = true;
					break;
				}
			}
			if(!InTrash) NewAi.Add( SiMarkers[i]);
		}

		ARMY_ = NewArmy_;
		Mexes = NewMexes;
		Hydros = NewHydros;
		SiMarkers = NewAi;

		ArmiesTrash = new List<int> ();
		MexesTrash = new List<int> ();
		HydrosTrash = new List<int> ();
		AiTrash = new List<int> ();
		EditMenu.EditMarkers.AllMarkersList.UpdateList();
	}

	public void DeleteMarker(EditMap.EditingMarkers.WorkingElement Element){
		switch (Element.ListId) {
		case 0:
			ArmiesTrash.Add(Element.InstanceId);
			break;
		case 1:
			MexesTrash.Add(Element.InstanceId);
			break;
		case 2:
			HydrosTrash.Add(Element.InstanceId);
			break;
		case 3:
			AiTrash.Add(Element.InstanceId);
			break;
		}
	}
}
