using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NLua;

public class MapLuaParser : MonoBehaviour {

	[Header("Objects")]
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

	//[HideInInspector]
	public		Vector3			MapCenterPoint;

	public		int				ScriptId = 0;

	public		string			BackupPath;


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
		public		Rect			Area;
	}
	
	public	enum armys 
	{
		none, ARMY1, ARMY2, ARMY3, ARMY4, ARMY5, ARMY6, ARMY7, ARMY8, ARMY9, ARMY10, ARMY11, ARMY12
	}


	Lua env;
	Lua save;

	public void LoadFile(){
		StartCoroutine("LoadingFile");
	}

	IEnumerator LoadingFile(){
		// Begin load
		InfoPopup.Show(true, "Loading map...");

		// Scenario LUA
		LoadScenarioLua();
		yield return null;

		// SCMAP
		//MapElements.SetActive(true);
		var LoadScmapFile = HeightmapControler.StartCoroutine( "LoadScmapFile");
		yield return LoadScmapFile;
		
		yield return null;
		// Save LUA
		LoadSaveLua();
		yield return null;
		CamControll.RestartCam();

		// Finish Load
		yield return null;
		HelperGui.MapLoaded = true;
		InfoPopup.Show(false);
	}

	public void SaveFile(){

	}

	public static string FormatException(NLua.Exceptions.LuaException e) {
		string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
		return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
	}

	public void UpdateArea(){

		HeightmapControler.TerrainMaterial.SetFloat("_AreaX", ScenarioData.Area.x / 15f);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaY", ScenarioData.Area.y / 15f);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaWidht", ScenarioData.Area.width / 15f);
		HeightmapControler.TerrainMaterial.SetFloat("_AreaHeight", ScenarioData.Area.height / 15f);

		/*MapBorders[0].localPosition = Vector3.right * (ScenarioData.Size.x / 20) - Vector3.forward * ((ScenarioData.Size.y - ScenarioData.Area.height) / ScenarioData.Size.y) * (ScenarioData.Size.x / 10);
		MapBorders[1].localPosition = Vector3.right * (ScenarioData.Size.x / 20) -Vector3.forward * (ScenarioData.Size.y / 10) + Vector3.forward * (ScenarioData.Area.y / ScenarioData.Size.y) * (ScenarioData.Size.y / 10);

		MapBorders[2].localPosition = -Vector3.forward * (ScenarioData.Size.y / 20) + Vector3.zero + Vector3.right * (ScenarioData.Area.x / ScenarioData.Size.x) * (ScenarioData.Size.x / 10);
		MapBorders[3].localPosition = -Vector3.forward * (ScenarioData.Size.y / 20) + Vector3.right * (ScenarioData.Size.x / 10) - Vector3.right * ((ScenarioData.Size.x - ScenarioData.Area.width) / ScenarioData.Size.x) * (ScenarioData.Size.x / 10);


		MapBorders[0].localScale = new Vector3((MapBorders[3].localPosition.x - MapBorders[2].localPosition.x) / 10, 1, 1);
		MapBorders[1].localScale = new Vector3((MapBorders[3].localPosition.x - MapBorders[2].localPosition.x) / 10, 1, 1);

		MapBorders[2].localScale = new Vector3((MapBorders[1].localPosition.z - MapBorders[0].localPosition.z) / 10, 1, 1);
		MapBorders[3].localScale = new Vector3((MapBorders[1].localPosition.z - MapBorders[0].localPosition.z) / 10, 1, 1);*/
	}

	private void LoadScenarioLua(){
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");

		string loadedFile = "";
		Debug.Log("Load file:" + MapPath + FolderName + "/" + ScenarioFileName + ".lua");
		string loc = MapPath + FolderName + "/" + ScenarioFileName + ".lua";
		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		string loadedFileFunctions = "";
		loadedFileFunctions = System.IO.File.ReadAllText("Structure/lua_variable_functions.lua", encodeType);
		
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
		string locSave = Application.dataPath + ScenarioData.SaveLua;
#if UNITY_EDITOR
		locSave = locSave.Replace("Assets/", "");
#endif

		Debug.Log("Load Save file: " + locSave);
		loadedFileSave = System.IO.File.ReadAllText(locSave, encodeType);
		
		string loadedFileFunctions = "";
		loadedFileFunctions = System.IO.File.ReadAllText("Structure/lua_variable_functions.lua", encodeType);
		
		string loadedFileEndFunctions = "";
		loadedFileEndFunctions = System.IO.File.ReadAllText("Structure/lua_variable_end_functions.lua", encodeType);
		
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
			ScenarioData.Area.x = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			ScenarioData.Area.y = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			ScenarioData.Area.width = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			ScenarioData.Area.height = float.Parse(save.GetTable("Scenario.Areas.AREA_1.rectangle")[4].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			UpdateArea();
		}
		else{
			MapElements.SetActive(false);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaX", 0);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaY", 0);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaWidht", ScenarioData.Size.x / 15f);
			HeightmapControler.TerrainMaterial.SetFloat("_AreaHeight", ScenarioData.Size.y / 15f);
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
				GameObject NewMex =  Instantiate(Prefabs[1], Vector3.zero, Quaternion.identity) as GameObject;
				NewMex.name = Mexes[MassCount].name;
				Mexes[MassCount].Mark = NewMex.transform;
				Mexes[MassCount].Mark.parent = MarkersParent;
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(Mexes[MassCount].position);
				Mexes[MassCount].Mark.localPosition = MexLocPos;
				Mexes[MassCount].Script = Mexes[MassCount].Mark.gameObject.GetComponent<MarkerScript>();
				Mexes[MassCount].Script.Typ = MarkerScript.MarkersTypes.Mex;
				if(Water) Mexes[MassCount].Script.WaterLevel =  ScenarioData.MaxHeight / 12.8f;
				else  Mexes[MassCount].Script.WaterLevel = 0;
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
				GameObject NewMex =  Instantiate(Prefabs[2], Vector3.zero, Quaternion.identity) as GameObject;
				NewMex.name = Hydros[HydroCount].name;
				Hydros[HydroCount].Mark = NewMex.transform;
				Hydros[HydroCount].Mark.parent = MarkersParent;
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(Hydros[HydroCount].position);
				Hydros[HydroCount].Mark.localPosition = MexLocPos;
				Hydros[HydroCount].Script = Hydros[HydroCount].Mark.gameObject.GetComponent<MarkerScript>();
				Hydros[HydroCount].Script.Typ = MarkerScript.MarkersTypes.Mex;
				if(Water) Hydros[HydroCount].Script.WaterLevel = ScenarioData.MaxHeight / 12.8f;
				else Hydros[HydroCount].Script.WaterLevel = 0;
				
				HydroCount++;
			}
			else if(save.GetTable("AllKeyMarkers")[m + 1].ToString().Contains("ARMY_")){
				
				ARMY_.Add(new Army());
				ARMY_[SpawnCount].name = save.GetTable("AllKeyMarkers")[m + 1].ToString();
				GameObject NewSpawn =  Instantiate(Prefabs[0], Vector3.zero, Quaternion.identity) as GameObject;
				ARMY_[SpawnCount].Mark = NewSpawn.transform;
				ARMY_[SpawnCount].Mark.parent = MarkersParent;
				//Vector3 MexLocPos = Vector3.zero;
				NewSpawn.name = ARMY_[SpawnCount].name;


				ARMY_[SpawnCount].position.x = float.Parse(MarkerPos[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				ARMY_[SpawnCount].position.y = float.Parse(MarkerPos[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				ARMY_[SpawnCount].position.z = float.Parse(MarkerPos[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				
				//MexLocPos.x = -1 * (ScenarioData.Size.x / 20) + 1 * (ARMY_[SpawnCount].position.x / ScenarioData.Size.x) * (ScenarioData.Size.x / 10);
				//MexLocPos.z = -1 * (ScenarioData.Size.y / 20) + 1 * (ARMY_[SpawnCount].position.z / ScenarioData.Size.y) * (ScenarioData.Size.y / 10);
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(ARMY_[SpawnCount].position);
				ARMY_[SpawnCount].Mark.localPosition = MexLocPos;
				ARMY_[SpawnCount].Script = ARMY_[SpawnCount].Mark.gameObject.GetComponent<MarkerScript>();
				ARMY_[SpawnCount].Script.Typ = MarkerScript.MarkersTypes.Army;
				if(Water) ARMY_[SpawnCount].Script.WaterLevel = ScenarioData.MaxHeight / 12.8f;
				else ARMY_[SpawnCount].Script.WaterLevel = 0;
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
				GameObject NewMex;

				switch(MarkerTab["type"].ToString()){
				case "Rally Point":
					NewMex = Instantiate(Prefabs[3], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				case "Combat Zone":
					NewMex = Instantiate(Prefabs[4], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				case "Defensive Point":
					NewMex = Instantiate(Prefabs[5], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				case "Naval Area":
					NewMex = Instantiate(Prefabs[6], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				default:
					NewMex = Instantiate(Prefabs[5], Vector3.zero, Quaternion.identity) as GameObject;
					break;
				}

				/*if(MarkerTab["type"].ToString() == "Rally Point"){
					NewMex = Instantiate(Prefabs[3], Vector3.zero, Quaternion.identity) as GameObject;
				}
				else if(MarkerTab["type"].ToString() == "Combat Zone"){
					NewMex = Instantiate(Prefabs[4], Vector3.zero, Quaternion.identity) as GameObject;
				}
				else if(MarkerTab["type"].ToString() == "Defensive Point"){
					NewMex = Instantiate(Prefabs[5], Vector3.zero, Quaternion.identity) as GameObject;
				}
				else if(MarkerTab["type"].ToString() == "Naval Area"){
					NewMex = Instantiate(Prefabs[6], Vector3.zero, Quaternion.identity) as GameObject;
				}
				else{
					NewMex = Instantiate(Prefabs[5], Vector3.zero, Quaternion.identity) as GameObject;
				}*/

				SiMarkers[SiCount].type = MarkerTab["type"].ToString();
				SiMarkers[SiCount].prop = MarkerTab["prop"].ToString();

				NewMex.name = SiMarkers[SiCount].name;
				SiMarkers[SiCount].Mark = NewMex.transform;
				SiMarkers[SiCount].Mark.parent = MarkersParent;
				Vector3 MexLocPos = HeightmapControler.MapPosInWorld(SiMarkers[SiCount].position);
				SiMarkers[SiCount].Mark.localPosition = MexLocPos;
				SiMarkers[SiCount].Script = SiMarkers[SiCount].Mark.gameObject.GetComponent<MarkerScript>();
				SiMarkers[SiCount].Script.Typ = MarkerScript.MarkersTypes.Mex;
				if(Water) SiMarkers[SiCount].Script.WaterLevel = ScenarioData.MaxHeight / 12.8f;
				else  SiMarkers[SiCount].Script.WaterLevel = 0;
				
				SiCount++;
			}
		}


	}


	public IEnumerator SaveMap(){

		InfoPopup.Show(true, "Saving map...");
		yield return null;

		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string BackupId = System.DateTime.Now.Month.ToString() +System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString() + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString();
		BackupPath = Application.dataPath + "/" + MapPath + FolderName + "/Backup_" + BackupId;

		#if UNITY_EDITOR
			BackupPath = BackupPath.Replace("Assets/", "");
		#endif

		System.IO.Directory.CreateDirectory(BackupPath);

		yield return null;
		SaveScenarioLua();
		yield return null;
		SaveSaveLua();
		yield return null;

		SaveScriptLua(ScriptId);
		yield return null;

		InfoPopup.Show(false);
	}

	public void SaveScenarioLua(){
		string SaveData = "";
		string loadedFile = "";
		
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = "Structure/scenario_structure.lua";
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
		string ScenarioFilePath = "/" + MapPath + FolderName + "/" + ScenarioFileName + ".lua";
		//string SavePath = Application.dataPath + ScenarioFilePath.Replace(".lua", "_new.lua");
		string SavePath = Application.dataPath + ScenarioFilePath;
		#if UNITY_EDITOR
		SavePath = SavePath.Replace("Assets/", "");
		#endif

		System.IO.File.Move(SavePath, BackupPath + "/" + ScenarioFileName + ".lua");
		System.IO.File.WriteAllText(SavePath, SaveData);
	}


	public void SaveSaveLua(){
		string SaveData = "";
		string loadedFile = "";

		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = "Structure/save_structure.lua";
		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		char[] Separator = "\n".ToCharArray();
		string[] AllLines = loadedFile.Split(Separator);

		//Mex
		string MexStructure = System.IO.File.ReadAllText("Structure/save_mex.lua", encodeType);

		//Hydro
		string HydroStructure = System.IO.File.ReadAllText("Structure/save_hydro.lua", encodeType);

		//Army Marker
		string ArmyMarkerStructure = System.IO.File.ReadAllText("Structure/save_armymarker.lua", encodeType);

		//Si
		string SiStructure = System.IO.File.ReadAllText("Structure/save_marker.lua", encodeType);

		//Army
		string ArmyStructure = System.IO.File.ReadAllText("Structure/save_army.lua", encodeType);

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

					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( ARMY_[a].Mark.position );
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
					
					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( Hydros[h].Mark.position );
					string Position = "                    ['position'] = VECTOR3( "+ MexPos.x.ToString() +", "+ MexPos.y.ToString() +", "+ MexPos.z.ToString() +" ),";
					NewHydro = NewHydro.Replace("['position']", Position);
					
					SaveData += NewHydro + "\n";
				}

				for(int m = 0; m < Mexes.Count; m++){
					string NewMex = MexStructure;

					string MexName = "                ['"+ Mexes[m].name +"'] = {";
					NewMex = NewMex.Replace("['Mass1']", MexName);

					string SpawnWithArmy = "                    ['SpawnWithArmy'] = STRING( '" + Mexes[m].SpawnWithArmy.ToString() + "' ),";
					if( Mexes[m].SpawnWithArmy == armys.none) NewMex = NewMex.Replace("['SpawnWithArmy']\n", "");
					else NewMex = NewMex.Replace("['SpawnWithArmy']", SpawnWithArmy);

					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( Mexes[m].Mark.position );
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


					Vector3 MexPos = HeightmapControler.MapWorldPosInSave( SiMarkers[s].Mark.position );
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
		//string SavePath = Application.dataPath + ScenarioData.SaveLua.Replace(".lua", "_new.lua");
		string SavePath = Application.dataPath + ScenarioData.SaveLua;
		#if UNITY_EDITOR
		SavePath = SavePath.Replace("Assets/", "");
		#endif

		string FileName = ScenarioData.SaveLua;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SavePath, BackupPath + "/" + Names[Names.Length - 1]);

		System.IO.File.WriteAllText(SavePath, SaveData);
	}

	public void SaveScriptLua(int ID = 0){
		string SaveData = "";
		string loadedFile = "";
		
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = "";
		if(ID == 0) return;
		else if(ID == 1) loc = "Structure/script_structure1.lua";
		else if(ID == 2) loc = "Structure/script_structure2.lua";

		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		SaveData = loadedFile;

		//string SavePath = Application.dataPath + ScenarioData.ScriptLua.Replace(".lua", "_new.lua");
		string SavePath = Application.dataPath + ScenarioData.ScriptLua;
		#if UNITY_EDITOR
		SavePath = SavePath.Replace("Assets/", "");
		#endif

		string FileName = ScenarioData.ScriptLua;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SavePath, BackupPath + "/" + Names[Names.Length - 1]);
		
		System.IO.File.WriteAllText(SavePath, SaveData);
	}
}
