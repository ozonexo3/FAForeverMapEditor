// ***************************************************************************************
// * Simple SupCom map LUA parser
// * TODO : should read all values. Right now it only search for known, hardcoded values
// ***************************************************************************************

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using NLua;
using MapLua;

public class MapLuaParser : MonoBehaviour {

	public static MapLuaParser Current;


	public ScenarioLua ScenarioLuaFile;
	public SaveLua SaveLuaFile;

	#region Variables
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
	public PropsInfo PropsMenu;
	public static		bool			Water;


	[Header("Local Data")]
	public		List<Mex>		Mexes = new List<Mex>();
	public		List<Hydro>		Hydros = new List<Hydro>();
	public		List<Army>		ARMY_ = new List<Army>();
	public		List<Marker>	SiMarkers = new List<Marker>();
	public int ArmyHidenCount = 0;

	public		List<int>		MexesTrash;
	public		List<int>		HydrosTrash;
	public		List<int>		ArmiesTrash;
	public		List<int>		AiTrash;

	public		List<SaveArmy> 		SaveArmys = new List<SaveArmy>();

	public		int MexTotalCount = 0;
	public		int HydrosTotalCount = 0;
	public		int SiTotalCount = 0;

	//[HideInInspector]
	public		Vector3			MapCenterPoint;

	public		int				ScriptId = 0;

	public static string			BackupPath;
	public static string			StructurePath;

	// LUA
	Lua env;
	Lua save;

	#endregion

	#region Classes
	[System.Serializable]
	public class Mex{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		Vector3		position;
		public		Vector3		orientation;
		public		armys		SpawnWithArmy;
	}

	[System.Serializable]
	public class Hydro{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		float size = 3;
		public		float amount = 100;
		public		bool resource = true;
		public		string prop = "/env/common/props/markers/M_Hydrocarbon_prop.bp";
		public		Color color;
		public		Vector3		position;
		public		Vector3		orientation;
		public		armys		SpawnWithArmy;

		public Hydro(){
			size = 3;
			amount = 100;
			resource = true;
			prop = "/env/common/props/markers/M_Hydrocarbon_prop.bp";
			color = new Color(1, 0, 0.5f, 0);
			SpawnWithArmy = armys.none;
		}
	}

	[System.Serializable]
	public class Marker{
		public		string		name;
		public		Transform	Mark;
		public		MarkerScript		Script;

		public		string 		hintValue;
		public		bool 		hint = true;
		public		string		graph;
		public		string 		adjacentTo;
		public		string		type;
		public		Color		Kolor;
		public		string		prop;
		public		Vector3		orientation;
		public		Vector3		position;
		public		armys		SpawnWithArmy;
		//public		float		Size;
	}

	[System.Serializable]
	public class Army{
		public		string		name;
		public		bool 		Hidden = false;
		public		int			Id;
		public		Transform	Mark;
		public		MarkerScript		Script;
		public		Vector3		position;
		public		Vector3		orientation;
	}

	[System.Serializable]
	public class SaveArmy{
		public string Name = "";
		public int ArmysOffset = 0;
		public string personality = "";
		public string plans = "";
		public int color = 0;
		public int faction = 0;
		public int EconomyMass = 0;
		public int EconomyEnergy = 0;
		public string[] Alliances = new string[0];
		public string UnitsOrders = "";
		public string UnitsPlatoon = "";
		public List<UnitGroup> Units = new List<UnitGroup>();
		public int PlatoonBuilders_NextBuilderId = 0;
		public List<string> PlatoonBuilders_Builders = new List<string>();

		/*public SaveArmy(){
			Name = "";
			ArmysOffset = 0;
			personality = "";
			plans = "";
			color = 0;
			faction = 0;
			EconomyMass = 0;
			EconomyEnergy = 0;
			Alliances = "";
			UnitsOrders = "";
			UnitsPlatoon = "";
			Units = new List<UnitGroup>();
			PlatoonBuilders_NextBuilderId = 0;
			PlatoonBuilders_Builders = new List<string>();
		}*/
	}

	[System.Serializable]
	public class UnitGroup{
		public string Name = "";
		public string orders = "";
		public string platoon = "";
		public List<Unit> Units = new List<Unit>();
	}

	[System.Serializable]
	public class Unit{
		public string Name = "";
		public string type = "";
		public string orders = "";
		public string platoon = "";
		public Vector3 Position;
		public Vector3 Orientation;
	}

	[System.Serializable]
	public class Scenario{
		public		string			MapName;
		public		string			MapDesc;
		public		string			Preview;
		public		string			Type;
		public		float			Version;
		public		bool			AdaptiveMap;
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
		public		List<customprop> CustomProps;
	}
	
	public	enum armys 
	{
		none, ARMY1, ARMY2, ARMY3, ARMY4, ARMY5, ARMY6, ARMY7, ARMY8, ARMY9, ARMY10, ARMY11, ARMY12, ARMY13, ARMY14, ARMY15, ARMY16
	}
	#endregion

	[System.Serializable]
	public class customprop
	{
		public string key;
		public string value;
	}

	void Awake(){
		ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = 0;

		#if UNITY_EDITOR
		//PlayerPrefs.DeleteAll();
		#endif

		EnvPaths.GenerateDefaultPaths ();

		Current = this;
		StructurePath = Application.dataPath + "/Structure/";;
		#if UNITY_EDITOR
		StructurePath = StructurePath.Replace("Assets", "");
		#endif

		ParsingStructureData.LoadData ();
	}

	#region Loading

	public IEnumerator ForceLoadMapAtPath(string path){
		path = path.Replace("\\", "/");
		Debug.Log("Load from: " + path);

		//string LastMapPatch = EnvPaths.GetMapsPath();

		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = path.Split(NameSeparator);

		FolderName = Names[Names.Length - 2];
		ScenarioFileName = Names[Names.Length - 1].Replace(".lua", "");
		string NewMapPatch = path.Replace(FolderName + "/" + ScenarioFileName + ".lua", "");

		Debug.Log("Parsed args: \n"
			+ FolderName + "\n"
			+ ScenarioFileName + "\n"
			+ FolderName);


		//PlayerPrefs.SetString("MapsPath", NewMapPatch);
		loadSave = false;
		LoadProps = false;
		var LoadFile = StartCoroutine("LoadingFile");
		yield return LoadFile;

		InfoPopup.Show(false);
		//PlayerPrefs.SetString("MapsPath", LastMapPatch);
	}

	public void LoadFile(){
		StartCoroutine("LoadingFile");
	}

	bool loadSave = true;
	bool LoadProps = true;

	IEnumerator LoadingFile(){

		bool AllFilesExists = true;
		string MapPath = EnvPaths.GetMapsPath();
		string Error = "";
		if (!System.IO.Directory.Exists (MapPath)) {
			Error = "Map folder not exist: " + MapPath;
			Debug.LogError (Error);
			AllFilesExists = false;
		}

		if (AllFilesExists && !System.IO.File.Exists (MapPath + FolderName + "/" + ScenarioFileName + ".lua")) {
			Error = "Scenario.lua not exist: " + MapPath + FolderName + "/" + ScenarioFileName + ".lua";
			Debug.LogError (Error);
			AllFilesExists = false;
		}
			

		if(AllFilesExists && !System.IO.File.Exists(EnvPaths.GetGamedataPath() + "/env.scd")){
			Error = "No source files in gamedata folder: " + EnvPaths.GetGamedataPath();
			Debug.LogError (Error);
			AllFilesExists = false;
		}

		if (AllFilesExists) {
			// Begin load
			InfoPopup.Show (true, "Loading map...");
			yield return null;
			// Scenario LUA
			LoadScenarioLua ();
			// NewLoading
			ScenarioLuaFile.Load_ScenarioLua(FolderName, ScenarioFileName);
			yield return null;

			// SCMAP
			var LoadScmapFile = HeightmapControler.StartCoroutine ("LoadScmapFile");
			yield return LoadScmapFile;
			CamControll.RestartCam ();

			EditMenu.MapInfoMenu.SaveAsFa.isOn = HeightmapControler.map.VersionMinor >= 60;


			SaveLuaFile.Load_SaveLua();

			if (loadSave) {
				// Save LUA
				LoadSaveLua ();
				yield return null;
			}

			// Finish Load
			HelperGui.MapLoaded = true;

			// Load Props
			if (LoadProps) {
				PropsMenu.gameObject.SetActive(true);

				PropsMenu.AllowBrushUpdate = false;
				yield return PropsMenu.StartCoroutine(PropsMenu.LoadProps());

				PropsMenu.gameObject.SetActive(false);
			}

			InfoPopup.Show (false);

			EditMenu.Categorys [0].GetComponent<MapInfo> ().UpdateFields ();
		} else {
			HelperGui.ReturnLoadingWithError (Error);
		}

	}

	#endregion


	public static string loadedFileFunctions = "";

	public static string GetLoadedFileFunctions()
	{
		if (loadedFileFunctions.Length == 0)
		{
			loadedFileFunctions = System.IO.File.ReadAllText(StructurePath + "lua_variable_functions.lua", System.Text.Encoding.ASCII);
		}
		return loadedFileFunctions;
	}

	#region Load Scenario Lua
	private void LoadScenarioLua(){

		


		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

		string MapPath = EnvPaths.GetMapsPath();

		string loadedFile = "";
		Debug.Log("Load file:" + MapPath + FolderName + "/" + ScenarioFileName + ".lua");
		string loc = MapPath + FolderName + "/" + ScenarioFileName + ".lua";
		loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		env = new Lua();
		env.LoadCLRPackage();
		
		try {
			env.DoString(GetLoadedFileFunctions() + loadedFile);
		} catch(NLua.Exceptions.LuaException e) {
			Debug.LogError(ParsingStructureData.FormatException(e), gameObject);
			
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

		if(env.GetTable("ScenarioInfo").RawGet("AdaptiveMap") != null && env.GetTable("ScenarioInfo").RawGet("AdaptiveMap").ToString() != "null"){
			ScenarioData.AdaptiveMap = env.GetTable("ScenarioInfo").RawGet("AdaptiveMap").ToString() == "true";
		}
		else{
			ScenarioData.AdaptiveMap = false;
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

		if(env.GetTable("ScenarioInfo") != null && env.GetTable("ScenarioInfo").RawGet("norushradius") != null)
			ScenarioData.NoRushRadius = float.Parse(env.GetTable("ScenarioInfo").RawGet("norushradius").ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		else
		{
			ScenarioData.NoRushRadius = 0;
		}

		ScenarioData.NoRushARMY = new Vector2[ScenarioData.Players];
		
		for(int id = 0; id < ScenarioData.Players; id++) {
			ScenarioData.NoRushARMY[id] = Vector2.zero;
			if(env.GetTable("ScenarioInfo").RawGet("norushoffsetX_ARMY_" + (id + 1)) != null)
				ScenarioData.NoRushARMY[id].x = float.Parse(env.GetTable("ScenarioInfo").RawGet("norushoffsetX_ARMY_" + (id + 1)).ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			if(env.GetTable("ScenarioInfo").RawGet("norushoffsetY_ARMY_" + (id + 1)) != null)
				ScenarioData.NoRushARMY[id].y = float.Parse(env.GetTable("ScenarioInfo").RawGet("norushoffsetY_ARMY_" + (id + 1)).ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		ScenarioData.CustomProps = new List<customprop> ();
		LuaHelper.LHTable CustomPropsTable = new LuaHelper.LHTable(env.GetTable("ScenarioInfo.Configurations.standard.customprops"));

		for (int i = 0; i < CustomPropsTable.Count; i++) {
			customprop NewProp = new customprop ();
			NewProp.key = CustomPropsTable.Keys [i];
			NewProp.value = CustomPropsTable.GetStringValue (CustomPropsTable.Keys [i]);
			ScenarioData.CustomProps.Add (NewProp);
		}


	}
	#endregion

	#region Load Save Lua
	private void LoadSaveLua(){
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loadedFileSave = "";
		string MapPath = EnvPaths.GetMapsPath();

		loadedFileSave = System.IO.File.ReadAllText(ScenarioData.SaveLua.Replace("/maps/", MapPath), encodeType);

		string loadedFileFunctions = LuaHelper.GetStructureText ("lua_variable_functions.lua");
		//loadedFileFunctions = System.IO.File.ReadAllText(StructurePath + "lua_variable_functions.lua", encodeType);

		string loadedFileEndFunctions = LuaHelper.GetStructureText ("lua_variable_end_functions.lua");
		//loadedFileEndFunctions = System.IO.File.ReadAllText(StructurePath + "lua_variable_end_functions.lua", encodeType);
		
		loadedFileSave = loadedFileFunctions + loadedFileSave + loadedFileEndFunctions;
		
		save = new Lua();
		save.LoadCLRPackage();
		
		try {
			save.DoString(loadedFileSave);
		}
		catch (NLua.Exceptions.LuaException e) {
			Debug.LogError(ParsingStructureData.FormatException(e), gameObject);
			
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
		#region Load Markers
		//int MarkersCount = save.GetTable("Scenario.MasterChain._MASTERCHAIN_.Markers").Values.Count;

		Mexes = new List<Mex>();
		Hydros = new List<Hydro>();
		ARMY_ = new List<Army>();
		SiMarkers = new List<Marker>();


		//LuaTable MasterChain = save.GetTable("Scenario.MasterChain._MASTERCHAIN_.Markers") as LuaTable;
		//string[] KeysArray = new string[MarkersCount];
		//MasterChain.Keys.CopyTo(KeysArray, 0);

		LuaHelper.LHTable MarkersTable = new LuaHelper.LHTable(save.GetTable("Scenario.MasterChain._MASTERCHAIN_.Markers"));

		for(int m = 0; m < MarkersTable.Count; m++){
			//LuaTable MarkerTab = MasterChain[KeysArray[m]] as LuaTable;
			LuaHelper.LHTable MarkerTable = new LuaHelper.LHTable(MarkersTable, MarkersTable.Keys[m]);

			Vector3 MarkerPosParsed = Vector3.zero;
			MarkerTable.GetVector3Value("position", out MarkerPosParsed);

			Vector3 MarkerRotParsed = Vector3.zero;
			MarkerTable.GetVector3Value("orientation", out MarkerRotParsed);

			string TypeOfMarker = MarkerTable.GetStringValue("type");

			if(TypeOfMarker == "Mass"){
				Mex newMex = new Mex();
				newMex.name = MarkersTable.Keys[m];
				
				newMex.position = ScmapEditor.MapPosInWorld(MarkerPosParsed);
				newMex.orientation = MarkerRotParsed;
				
				LuaHelper.ReadSpawnWithArmy(out newMex.SpawnWithArmy, MarkerTable);

				Mexes.Add(newMex);
			}
			else if(TypeOfMarker == "Hydrocarbon"){
				Hydro NewHydro = new Hydro();
				NewHydro.name = MarkersTable.Keys[m];

				NewHydro.size = MarkerTable.GetFloatValue("size");
				NewHydro.amount = MarkerTable.GetFloatValue("amount");
				NewHydro.resource = MarkerTable.GetBoolValue("resource");
				NewHydro.prop = MarkerTable.GetStringValue("prop");

				NewHydro.color = MarkerTable.GetColorValue("color");

				NewHydro.position = ScmapEditor.MapPosInWorld(MarkerPosParsed);
				NewHydro.orientation = MarkerRotParsed;

				LuaHelper.ReadSpawnWithArmy(out NewHydro.SpawnWithArmy, MarkerTable);

				Hydros.Add(NewHydro);
			}
			else if(MarkersTable.Keys[m].Contains("ARMY_")){
				Army NewArmy = new Army();
				NewArmy.name = MarkersTable.Keys[m];

				NewArmy.Id = int.Parse(MarkersTable.Keys[m].Replace("ARMY_", ""));

				NewArmy.position = ScmapEditor.MapPosInWorld(MarkerPosParsed);
				NewArmy.orientation = MarkerRotParsed;

				ARMY_.Add(NewArmy);
			}
			else{
				Marker NewMarker = new Marker();
				NewMarker.name = MarkersTable.Keys[m];
				NewMarker.position = ScmapEditor.MapPosInWorld(MarkerPosParsed);
				NewMarker.orientation = MarkerRotParsed;

				NewMarker.type = TypeOfMarker;
				NewMarker.prop = MarkerTable.GetStringValue("prop");

				// HINT
				NewMarker.hintValue = MarkerTable.GetStringValue("hint");
				NewMarker.hint = MarkerTable.GetBoolValue("hint");

				// GRAPH
				NewMarker.graph = MarkerTable.GetStringValue("graph");

				// adjacentTo
				NewMarker.adjacentTo = MarkerTable.GetStringValue("adjacentTo");


				// Color
				if (!string.IsNullOrEmpty(MarkerTable.GetStringValue("color"))) {
					//Color MyColor = Color.white;
					//ColorUtility.TryParseHtmlString (MarkerTable.GetStringValue("color"), out MyColor);
					NewMarker.Kolor = MarkerTable.GetColorValue("color");

				} else
					NewMarker.Kolor = Color.white;
				

				LuaHelper.ReadSpawnWithArmy(out NewMarker.SpawnWithArmy, MarkerTable);

				SiMarkers.Add(NewMarker);
			}
		}

		SortArmys();

		MexTotalCount = Mexes.Count;
		HydrosTotalCount = Hydros.Count;
		SiTotalCount = SiMarkers.Count;
		#endregion

		#region Load Army Save
		SaveArmys = new List<SaveArmy> ();
		LuaHelper.LHTable ArmiesTable = new LuaHelper.LHTable(save.GetTable("Scenario.Armies"));

		for(int m = 0; m < ArmiesTable.Count; m++){
			LuaHelper.LHTable ArmyTable = new LuaHelper.LHTable(ArmiesTable, ArmiesTable.Keys[m]);
			SaveArmy NewArmy = new SaveArmy();
			NewArmy.Name = ArmiesTable.Keys[m];

			AddSaveArmyMarker(NewArmy.Name);

			NewArmy.personality = ArmyTable.GetStringValue("personality");
			NewArmy.plans = ArmyTable.GetStringValue("plans");
			NewArmy.color = ArmyTable.GetIntValue("color");
			NewArmy.faction = ArmyTable.GetIntValue("faction");

			LuaHelper.LHTable EconomyTable = new LuaHelper.LHTable(ArmyTable, "Economy");
			NewArmy.EconomyMass = EconomyTable.GetIntValue("mass");
			NewArmy.EconomyEnergy = EconomyTable.GetIntValue("energy");

			NewArmy.Alliances = ArmyTable.GetStringArrayValue("Alliances");


			// Get units

			NewArmy.UnitsOrders = "";
			NewArmy.UnitsPlatoon = "";
			NewArmy.Units = new List<UnitGroup>();
			LuaHelper.LHTable ArmyUnitsTable;
			ArmyTable.GetLuaArmyGroup("Units", out NewArmy.UnitsOrders, out NewArmy.UnitsPlatoon, out ArmyUnitsTable);

		
			for(int i = 0; i < ArmyUnitsTable.Count; i++){
				UnitGroup NewUnitGroup = new UnitGroup();
				NewUnitGroup.Name = ArmyUnitsTable.Keys[i];

				LuaHelper.LHTable UnitsGroupTable;
				ArmyUnitsTable.GetLuaArmyGroup(ArmyUnitsTable.Keys[i], out NewUnitGroup.orders, out NewUnitGroup.platoon, out UnitsGroupTable);

				for(int u = 0; u < UnitsGroupTable.Count; u++){
					Unit NewUnit = new Unit();
					LuaHelper.LHTable UnitTable = new LuaHelper.LHTable(UnitsGroupTable, UnitsGroupTable.Keys[u]);

					NewUnit.Name = UnitsGroupTable.Keys[u];
					NewUnit.type = UnitTable.GetStringValue("type");
					NewUnit.orders = UnitTable.GetStringValue("orders");
					NewUnit.platoon = UnitTable.GetStringValue("platoon");
					UnitTable.GetVector3Value("Position", out NewUnit.Position);
					UnitTable.GetVector3Value("Orientation", out NewUnit.Orientation);

					NewUnitGroup.Units.Add(NewUnit);
				}

				NewArmy.Units.Add(NewUnitGroup);
			}
				
			SaveArmys.Add(NewArmy);
		}

		#endregion
	}

	#endregion

	#region SaveMap

	public IEnumerator SaveMap(){

		InfoPopup.Show(true, "Saving map...");
		yield return null;

		string MapPath = EnvPaths.GetMapsPath ();
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

		string MapPath = EnvPaths.GetMapsPath();
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
				SaveData += "    description = \"" + ScenarioData.MapDesc.Replace("\"", "'") + "\",\n";
			}
			else if(line.Contains("[version]")){
				SaveData += "    map_version = " + ((int)ScenarioData.Version).ToString() + ",\n";
			}
			else if(line.Contains("[AdaptiveMap]")){
				SaveData += "    AdaptiveMap = " + (ScenarioData.AdaptiveMap?("true"):("false")) + ",\n";
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
					if (ARMY_ [a].Hidden)
						continue;
					SaveData += "'ARMY_" + (a + 1).ToString() + "',";
				}

				SaveData += "}\n";
				//SaveData += line;
			}
			else{
				SaveData += line;
			}
		}

		string CustomPropsCode = "";
		for (int i = 0; i < ScenarioData.CustomProps.Count; i++) {
			CustomPropsCode += "\n";
			CustomPropsCode += "\t\t\t\t['" + ScenarioData.CustomProps [i].key + "'] = " + ParsingStructureData.ToLuaStringVaue (ScenarioData.CustomProps [i].value);
		}

		SaveData = SaveData.Replace ("[customprops]", CustomPropsCode);

		string MapPath = EnvPaths.GetMapsPath();
		string ScenarioFilePath = MapPath + FolderName + "/" + ScenarioFileName + ".lua";
		//string SavePath = Application.dataPath + ScenarioFilePath.Replace(".lua", "_new.lua");
		//string SavePath = Application.dataPath + ScenarioFilePath;
		//#if UNITY_EDITOR
		//SavePath = SavePath.Replace("Assets/", "");
		//#endif

		System.IO.File.Move(ScenarioFilePath, BackupPath + "/" + ScenarioFileName + ".lua");
		System.IO.File.WriteAllText(ScenarioFilePath, SaveData);

		ScenarioLuaFile.Save_ScenarioLua(ScenarioFilePath.Replace(".lua", "_new.lua"));

	}


	public void SaveSaveLua(){
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
		string loc = StructurePath + "save_structure.lua";
		string loadedFile = System.IO.File.ReadAllText(loc, encodeType);

		string SaveData = loadedFile;


		//char[] Separator = "\n".ToCharArray();
		//string[] AllLines = loadedFile.Split(Separator);

		//Mex
		string MexStructure = System.IO.File.ReadAllText(StructurePath + "save_mex.lua", encodeType);

		//Hydro
		string HydroStructure = System.IO.File.ReadAllText(StructurePath + "save_hydro.lua", encodeType);

		//Army Marker
		string ArmyMarkerStructure = System.IO.File.ReadAllText(StructurePath + "save_armymarker.lua", encodeType);

		//Si
		string SiStructure = System.IO.File.ReadAllText(StructurePath + "save_marker.lua", encodeType);

		//string ArmyStructure = System.IO.File.ReadAllText(StructurePath + "save_army.lua", encodeType);

		string MarkersData = "";

		for(int a = 0; a < ARMY_.Count; a++){
			if (ARMY_ [a].Hidden)
				continue;
			string NewArmy = ArmyMarkerStructure;

			string ArmyName = "                ['" + ARMY_[a].name + "'] = {";
			NewArmy = NewArmy.Replace("['ARMY_']", ArmyName);

			Vector3 MexPos = ScmapEditor.MapWorldPosInSave( GetPosOfMarkerId(0, a) );
			string Position = ParsingStructureData.Save_Position.Replace("#", Vector3ToLua(MexPos));
			NewArmy = NewArmy.Replace("['position']", Position);

			Position = ParsingStructureData.Save_Rotation.Replace("#", Vector3ToLua(ARMY_[a].orientation));
			NewArmy = NewArmy.Replace("['orientation']", Position);

			MarkersData += NewArmy + "\n";
		}

		for(int h = 0; h < Hydros.Count; h++){
			string NewHydro = HydroStructure;

			//string HydroName = "                ['"+ Hydros[h].name +"'] = {";
			NewHydro = NewHydro.Replace("[name]", Hydros[h].name);

			string StringValue = ParsingStructureData.LuaMarkerProperty("size", ParsingStructureData.ToLuaFloatVaue(ParsingStructureData.ToFloatString(Hydros[h].size)));
			NewHydro = NewHydro.Replace("[size]", StringValue);

			StringValue = ParsingStructureData.LuaMarkerProperty("amount", ParsingStructureData.ToLuaFloatVaue(ParsingStructureData.ToFloatString(Hydros[h].amount)));
			NewHydro = NewHydro.Replace("[amount]", StringValue);

			StringValue = ParsingStructureData.LuaMarkerProperty("prop", ParsingStructureData.ToLuaStringVaue(Hydros[h].prop));
			NewHydro = NewHydro.Replace("[prop]", StringValue);

			StringValue = ParsingStructureData.LuaMarkerProperty("color", ParsingStructureData.ToLuaStringVaue( ParsingStructureData.ToColorString(Hydros[h].color)));
			NewHydro = NewHydro.Replace("[color]", StringValue);


			// Spawn With Army
			string SpawnWithArmy = ParsingStructureData.SpawnWithArmy.Replace("#", Hydros[h].SpawnWithArmy.ToString());
			if( Hydros[h].SpawnWithArmy == armys.none) NewHydro = NewHydro.Replace("[SpawnWithArmy]\r\n", "");
			else NewHydro = NewHydro.Replace("[SpawnWithArmy]", SpawnWithArmy);

			// Position
			Vector3 MexPos = ScmapEditor.MapWorldPosInSave( GetPosOfMarkerId(2, h) );
			string Position = ParsingStructureData.Save_Position.Replace("#", Vector3ToLua(MexPos));
			NewHydro = NewHydro.Replace("[position]", Position);

			// Orientation
			Position = ParsingStructureData.Save_Rotation.Replace("#", Vector3ToLua(Hydros[h].orientation));
			NewHydro = NewHydro.Replace("[orientation]", Position);

			MarkersData += NewHydro + "\n";
		}

		for(int m = 0; m < Mexes.Count; m++){
			string NewMex = MexStructure;

			string MexName = "                ['"+ Mexes[m].name +"'] = {";
			NewMex = NewMex.Replace("['MassName']", MexName);

			string SpawnWithArmy = ParsingStructureData.SpawnWithArmy.Replace("#", Mexes[m].SpawnWithArmy.ToString());
			if( Mexes[m].SpawnWithArmy == armys.none) NewMex = NewMex.Replace("['SpawnWithArmy']\r\n", "");
			else NewMex = NewMex.Replace("['SpawnWithArmy']", SpawnWithArmy);

			Vector3 MexPos = ScmapEditor.MapWorldPosInSave( GetPosOfMarkerId(1, m) );
			string Position = ParsingStructureData.Save_Position.Replace("#", Vector3ToLua(MexPos));
			NewMex = NewMex.Replace("['position']", Position);

			Position = ParsingStructureData.Save_Rotation.Replace("#", Vector3ToLua(Mexes[m].orientation));
			NewMex = NewMex.Replace("['orientation']", Position);

			MarkersData += NewMex + "\n";
		}

		for(int s = 0; s < SiMarkers.Count; s++){
			string NewSi = SiStructure;

			string SiName = "                ['"+ SiMarkers[s].name +"'] = {";
			NewSi = NewSi.Replace("['Marker']", SiName);

			string SiType = ParsingStructureData.LuaMarkerProperty ("type", ParsingStructureData.ToLuaStringVaue (SiMarkers [s].type));
			NewSi = NewSi.Replace("['type']", SiType);

			string SiProp = ParsingStructureData.LuaMarkerProperty ("prop", ParsingStructureData.ToLuaStringVaue (SiMarkers [s].prop));
			NewSi = NewSi.Replace("['prop']", SiProp);

			// Additional values
			// * hint
			if (string.IsNullOrEmpty (SiMarkers [s].hintValue)) {
				NewSi = NewSi.Replace("['hint']\r\n", "");
			} else {
				SiProp = ParsingStructureData.LuaMarkerProperty ("hint", ParsingStructureData.ToLuaBooleanVaue (SiMarkers [s].hint));
				NewSi = NewSi.Replace("['hint']", SiProp);
			}

			// * Graph
			if (string.IsNullOrEmpty (SiMarkers [s].graph)) {
				NewSi = NewSi.Replace("['graph']\r\n", "");
			} else {
				SiProp = ParsingStructureData.LuaMarkerProperty ("graph", ParsingStructureData.ToLuaStringVaue (SiMarkers [s].graph));
				NewSi = NewSi.Replace("['graph']", SiProp);
			}

			// * AdjacentTo
			if (string.IsNullOrEmpty (SiMarkers [s].adjacentTo)) {
				NewSi = NewSi.Replace("['adjacentTo']\r\n", "");
			} else {
				SiProp = ParsingStructureData.LuaMarkerProperty ("adjacentTo", ParsingStructureData.ToLuaStringVaue (SiMarkers [s].adjacentTo));
				NewSi = NewSi.Replace("['adjacentTo']", SiProp);
			}

			// * Color
			SiProp = ParsingStructureData.LuaMarkerProperty ("color", ParsingStructureData.ToLuaStringVaue (ColorUtility.ToHtmlStringRGBA(SiMarkers[s].Kolor).ToLower()));
			NewSi = NewSi.Replace("['color']", SiProp);

			Vector3 MexPos = ScmapEditor.MapWorldPosInSave( GetPosOfMarkerId(3, s) );
			string Position = ParsingStructureData.Save_Position.Replace("#", Vector3ToLua(MexPos));
			NewSi = NewSi.Replace("['position']", Position);

			Position = ParsingStructureData.Save_Rotation.Replace("#", Vector3ToLua(SiMarkers [s].orientation));
			NewSi = NewSi.Replace("['orientation']", Position);

			MarkersData += NewSi + "\n";

		}
		SaveData = SaveData.Replace ("[MARKERS]", MarkersData);
		SaveData = SaveData.Replace ("[CHAINS]", "");

		string AreaData = "";
		AreaData += "            ['rectangle'] = RECTANGLE( " + ScenarioData.Area.x.ToString() +", "+ ScenarioData.Area.y.ToString() +", "+ ScenarioData.Area.width.ToString() +", "+ ScenarioData.Area.height.ToString() +"),";
		SaveData = SaveData.Replace ("[AREA]", AreaData);


		string ArmyUnitsStructure = LuaHelper.GetStructureText ("save_army.lua");
		string ArmyUnitGruopStructure = LuaHelper.GetStructureText ("save_armyunits.lua");
		string ArmyUnitStructure = LuaHelper.GetStructureText ("save_armyunit.lua");

		string SaveArmysData = "";
		for(int a = 0; a < SaveArmys.Count; a++){
			string NewArmySave = ArmyUnitsStructure;

			NewArmySave = NewArmySave.Replace ("[name]", SaveArmys [a].Name);

			NewArmySave = NewArmySave.Replace ("[Alliances]", "");

			NewArmySave = NewArmySave.Replace ("[personality]", SaveArmys [a].personality);
			NewArmySave = NewArmySave.Replace ("[plans]", SaveArmys [a].plans);
			NewArmySave = NewArmySave.Replace ("[color]", SaveArmys [a].color.ToString());
			NewArmySave = NewArmySave.Replace ("[faction]", SaveArmys [a].faction.ToString());

			NewArmySave = NewArmySave.Replace ("[mass]", SaveArmys [a].EconomyMass.ToString());
			NewArmySave = NewArmySave.Replace ("[energy]", SaveArmys [a].EconomyEnergy.ToString());

			NewArmySave = NewArmySave.Replace ("[armyorders]", SaveArmys [a].UnitsOrders);
			NewArmySave = NewArmySave.Replace ("[armyplatoon]", SaveArmys [a].UnitsPlatoon);
			string AllUnitGroups = "";

			for (int i = 0; i < SaveArmys [a].Units.Count; i++) {
				string NewUnitsGroup = ArmyUnitGruopStructure;

				NewUnitsGroup = NewUnitsGroup.Replace ("[name]", SaveArmys [a].Units [i].Name);
				NewUnitsGroup = NewUnitsGroup.Replace ("[orders]", SaveArmys [a].Units [i].orders);
				NewUnitsGroup = NewUnitsGroup.Replace ("[platoon]", SaveArmys [a].Units [i].platoon);

				string UnitsArray = "";
				for (int u = 0; u < SaveArmys [a].Units [i].Units.Count; u++) {
					string NewUnit = ArmyUnitStructure;

					NewUnit = NewUnit.Replace ("[name]", SaveArmys [a].Units [i].Units [u].Name);
					NewUnit = NewUnit.Replace ("[type]", SaveArmys [a].Units [i].Units [u].type);
					NewUnit = NewUnit.Replace ("[orders]", SaveArmys [a].Units [i].Units [u].orders);
					NewUnit = NewUnit.Replace ("[platoon]", SaveArmys [a].Units [i].Units [u].platoon);

					NewUnit = NewUnit.Replace ("[position]", ParsingStructureData.ToLuaSimpleVector3Value( SaveArmys [a].Units [i].Units [u].Position));
					NewUnit = NewUnit.Replace ("[rotation]", ParsingStructureData.ToLuaSimpleVector3Value( SaveArmys [a].Units [i].Units [u].Orientation));

					//if (u > 0)
					//	UnitsArray += "\n";
					UnitsArray += "\n" + NewUnit;
				}
				NewUnitsGroup = NewUnitsGroup.Replace ("[UnitsArray]", UnitsArray);

				if (i > 0)
					AllUnitGroups += "\n";
				AllUnitGroups += NewUnitsGroup;
			}
			NewArmySave = NewArmySave.Replace ("[armyunits]", AllUnitGroups);
			if (a > 0)
				SaveArmysData += "\n";
			SaveArmysData += NewArmySave;
		}
		SaveData = SaveData.Replace ("[ARMYS]", SaveArmysData);



		string MapPath = EnvPaths.GetMapsPath();
		string SaveFilePath = ScenarioData.SaveLua.Replace("/maps/", MapPath);

		string FileName = ScenarioData.SaveLua;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SaveFilePath, BackupPath + "/" + Names[Names.Length - 1]);

		SaveData = SaveData.Replace ("\r", "");

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

		string MapPath = EnvPaths.GetMapsPath();
		string SaveFilePath = ScenarioData.ScriptLua.Replace("/maps/", MapPath);

		string FileName = ScenarioData.ScriptLua;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SaveFilePath, BackupPath + "/" + Names[Names.Length - 1]);
		
		System.IO.File.WriteAllText(SaveFilePath, SaveData);
	}

	#endregion

	public static string Vector3ToLua(Vector3 vec){
		return vec.x.ToString() +", "+ vec.y.ToString() +", "+ vec.z.ToString();
	}

	#region Map functions
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
	#endregion

	#region Marker functions

	public Vector3 GetPosOfMarker(EditMap.MarkersInfo.WorkingElement Element){
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

	public void SetPosOfMarker(EditMap.MarkersInfo.WorkingElement Element, Vector3 NewPos){
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

	public GameObject GetMarkerRenderer(EditMap.MarkersInfo.WorkingElement Element){
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

	#endregion

	#region Marker Create & Destroy

	public void CreateMarker(int MarkerType, Vector3 position, string name){
		if(MarkerType == 0){
			ARMY_.Add(new Army());
			ARMY_[ARMY_.Count - 1].name = "ARMY_" + ARMY_.Count.ToString();
			ARMY_[ARMY_.Count - 1].position = position;
			AddSaveArmy (ARMY_ [ARMY_.Count - 1].name);
		}
		else if(MarkerType == 1){
			MexTotalCount++;
			Mexes.Add(new Mex());
			Mexes[Mexes.Count - 1].name = ParsingStructureData.MexName.Replace("#", MexTotalCount.ToString(ParsingStructureData.MarkerIdToString));
			Mexes[Mexes.Count - 1].position = position;
		}
		else if(MarkerType == 2){
			HydrosTotalCount++;
			Hydros.Add(new Hydro());
			Hydros[Hydros.Count - 1].name = ParsingStructureData.HydroName.Replace("#", HydrosTotalCount.ToString(ParsingStructureData.MarkerIdToString));
			Hydros[Hydros.Count - 1].position = position;
		}
		else if(MarkerType == 3){
			SiMarkers.Add(new Marker());
			SiMarkers[SiMarkers.Count - 1].name = "AI_" + SiTotalCount.ToString(ParsingStructureData.MarkerIdToString);
			SiMarkers[SiMarkers.Count - 1].position = position;
		}
		MarkerRend.Regenerate();
		EditMenu.EditMarkers.AllMarkersList.UpdateList();
	}

	public void AddMarkerToTrash(EditMap.MarkersInfo.WorkingElement Element){
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
			if (!InTrash) {
				RemoveSaveArmy (ARMY_ [i].name);
				NewArmy_.Add (ARMY_ [i]);
			}
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

	public void DeleteMarker(EditMap.MarkersInfo.WorkingElement Element){
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

	public void AddSaveArmyMarker(string name){
		for (int i = 0; i < ARMY_.Count; i++) {
			if (ARMY_ [i].name == name)
				return;
		}

		ARMY_.Add(new Army());
		ARMY_[ARMY_.Count - 1].name = name;
		ARMY_ [ARMY_.Count - 1].Hidden = true;
		ARMY_[ARMY_.Count - 1].position = Vector3.zero;
		//AddSaveArmy (ARMY_ [ARMY_.Count - 1].name);
	}

	public void RemoveSaveArmy(string name){
		for (int i = 0; i < SaveArmys.Count; i++) {
			if (SaveArmys [i].Name == name) {
				SaveArmys.RemoveAt (i);
				break;
			}
		}
	}

	public void AddSaveArmy(string name){
		SaveArmy NewArmy = new SaveArmy ();
		NewArmy.Name = name;
		SaveArmys.Add (NewArmy);
	}

	public int GetHigestArmy(){
		int ToReturn = 0;
		for (int i = 0; i < ARMY_.Count; i++) {
			if (ARMY_ [i].Id > ToReturn)
				ToReturn = ARMY_ [i].Id;
		}
		return ToReturn;
	}
		
	#endregion
}
