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
	//public		MarkersRenderer	MarkerRend;
	public		Editing			EditMenu;
	public		Undo			History;
	public		MapHelperGui	HelperGui;
	public		string			FolderName;
	public		string			ScenarioFileName;
	//public		Scenario		ScenarioData;
	//public		GameObject[]	Prefabs;
	//public		Transform		MarkersParent;
	//public		GameObject		MapElements;
	//public		Transform[]		MapBorders;
	public		CameraControler	CamControll;
	public		ScmapEditor		HeightmapControler;
	//public		GetGamedataFile	Gamedata;
	public		GenericInfoPopup	InfoPopup;
	public PropsInfo PropsMenu;
	public static		bool			Water;


	[Header("Local Data")]
	//public		List<Mex>		Mexes = new List<Mex>();
	//public		List<Hydro>		Hydros = new List<Hydro>();
	//public		List<Army>		ARMY_ = new List<Army>();
	//public		List<Marker>	SiMarkers = new List<Marker>();
	//public int ArmyHidenCount = 0;

	//public		List<int>		MexesTrash;
	//public		List<int>		HydrosTrash;
	//public		List<int>		ArmiesTrash;
	//public		List<int>		AiTrash;

	//public		List<SaveArmy> 		SaveArmys = new List<SaveArmy>();

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
	
	public	enum armys 
	{
		none, ARMY1, ARMY2, ARMY3, ARMY4, ARMY5, ARMY6, ARMY7, ARMY8, ARMY9, ARMY10, ARMY11, ARMY12, ARMY13, ARMY14, ARMY15, ARMY16
	}
	#endregion


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
		//string NewMapPath = path.Replace(FolderName + "/" + ScenarioFileName + ".lua", "");

		Debug.Log("Parsed args: \n"
			+ FolderName + "\n"
			+ ScenarioFileName + "\n"
			+ FolderName);


		//PlayerPrefs.SetString("MapsPath", NewMapPath);
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

			ScenarioLuaFile = new ScenarioLua();
			SaveLuaFile = new SaveLua();
			AsyncOperation ResUn = Resources.UnloadUnusedAssets();
			while (!ResUn.isDone)
			{
				yield return null;
			}

			// Scenario LUA
			if (ScenarioLuaFile.Load(FolderName, ScenarioFileName)){
				//Map Loaded
			}
			else
			{
				HelperGui.MapLoaded = false;
			}

			CamControll.MapSize = Mathf.Max(ScenarioLuaFile.Data.Size[0], ScenarioLuaFile.Data.Size[1]);
			CamControll.RestartCam();
			yield return null;

			// SCMAP
			var LoadScmapFile = HeightmapControler.StartCoroutine ("LoadScmapFile");
			yield return LoadScmapFile;
			CamControll.RestartCam ();

			EditMenu.MapInfoMenu.SaveAsFa.isOn = HeightmapControler.map.VersionMinor >= 60;



			if (loadSave) {
				// Save LUA
				SaveLuaFile.Load();
				SetSaveLua();
				//LoadSaveLua();
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


	#region Load Save Lua
	void SetSaveLua()
	{
		UpdateArea();

		//MapElements.SetActive(false);

		MapCenterPoint = Vector3.zero;
		MapCenterPoint.x = (GetMapSizeX() / 20f);
		MapCenterPoint.z = -1 * (GetMapSizeY() / 20f);

		//SortArmys();
	}
	#endregion

	#region SaveMap

	public void SaveMap()
	{
		if (string.IsNullOrEmpty(FolderName) || string.IsNullOrEmpty(ScenarioFileName))
			return;
		SavingMapProcess = true;
		InfoPopup.Show(true, "Saving map...");

		StartCoroutine(SaveMapProcess());
	}

	public static bool SavingMapProcess = false;
	public IEnumerator SaveMapProcess(){

		yield return null;

		string MapPath = EnvPaths.GetMapsPath ();
		string BackupId = System.DateTime.Now.Month.ToString() +System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString() + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString();
		BackupPath = MapPath + FolderName + "/Backup_" + BackupId;

		System.IO.Directory.CreateDirectory(BackupPath);
		yield return null;

		// Scenario.lua
		string ScenarioFilePath = EnvPaths.GetMapsPath() + FolderName + "/" + ScenarioFileName + ".lua";
		System.IO.File.Move(ScenarioFilePath, BackupPath + "/" + ScenarioFileName + ".lua");
		ScenarioLuaFile.Save(ScenarioFilePath);
		yield return null;


		//Save.lua
		string SaveFilePath = ScenarioLuaFile.Data.save.Replace("/maps/", MapPath);
		string FileName = ScenarioLuaFile.Data.save;
		string[] Names = FileName.Split(("/").ToCharArray());

		System.IO.File.Move(SaveFilePath, BackupPath + "/" + Names[Names.Length - 1]);
		SaveLuaFile.Save(SaveFilePath);
		yield return null;

		//SaveScenarioLua();
		//SaveSaveLua();
		//SaveScriptLua(ScriptId);

		SaveScmap();
		yield return null;

		InfoPopup.Show(false);
		SavingMapProcess = false;
	}

	public void SaveScmap(){

		string MapPath = EnvPaths.GetMapsPath();
		string MapFilePath = ScenarioLuaFile.Data.map.Replace("/maps/", MapPath);

		string FileName = ScenarioLuaFile.Data.map;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		//Debug.Log(BackupPath + "/" + Names[Names.Length - 1]);
		System.IO.File.Move(MapFilePath, BackupPath + "/" + Names[Names.Length - 1]);

		HeightmapControler.SaveScmapFile();
	}

	/*
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


	}
	*/


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
		string SaveFilePath = ScenarioLuaFile.Data.script.Replace("/maps/", MapPath);

		string FileName = ScenarioLuaFile.Data.script;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		System.IO.File.Move(SaveFilePath, BackupPath + "/" + Names[Names.Length - 1]);
		
		System.IO.File.WriteAllText(SaveFilePath, SaveData);
	}

	#endregion


	#region Map functions
	public void SortArmys(){
	
	}

	public void UpdateArea(){
		if(SaveLuaFile.Data.areas.Length > 0)
		{
			//int bigestAreaId = 0;
			Rect bigestAreaRect = new Rect(SaveLuaFile.Data.areas[0].rectangle);
			for(int i = 1; i < SaveLuaFile.Data.areas.Length; i++)
			{
				if (bigestAreaRect.x > SaveLuaFile.Data.areas[i].rectangle.x)
					bigestAreaRect.x = SaveLuaFile.Data.areas[i].rectangle.x;

				if (bigestAreaRect.y > SaveLuaFile.Data.areas[i].rectangle.y)
					bigestAreaRect.y = SaveLuaFile.Data.areas[i].rectangle.y;

				if (bigestAreaRect.width < SaveLuaFile.Data.areas[i].rectangle.width)
					bigestAreaRect.width = SaveLuaFile.Data.areas[i].rectangle.width;

				if (bigestAreaRect.height < SaveLuaFile.Data.areas[i].rectangle.height)
					bigestAreaRect.height = SaveLuaFile.Data.areas[i].rectangle.height;
			}

			if (bigestAreaRect.width > 0 && bigestAreaRect.height > 0)
			{
				HeightmapControler.TerrainMaterial.SetInt("_Area", 1);
				HeightmapControler.TerrainMaterial.SetFloat("_AreaX", bigestAreaRect.x / 10f);
				HeightmapControler.TerrainMaterial.SetFloat("_AreaY", bigestAreaRect.y / 10f);
				HeightmapControler.TerrainMaterial.SetFloat("_AreaWidht", bigestAreaRect.width / 10f);
				HeightmapControler.TerrainMaterial.SetFloat("_AreaHeight", bigestAreaRect.height / 10f);
			}
			else
			{
				HeightmapControler.TerrainMaterial.SetInt("_Area", 0);
			}

		}
		else
			HeightmapControler.TerrainMaterial.SetInt("_Area", 0);

		/*
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
		*/
	}
	#endregion



	#region Lua values
	public static Vector2 GetMapSize()
	{
		return new Vector2(Current.ScenarioLuaFile.Data.Size[0], Current.ScenarioLuaFile.Data.Size[1]);
	}

	public static float GetMapSizeX()
	{
		return Current.ScenarioLuaFile.Data.Size[0];
	}

	public static float GetMapSizeY()
	{
		return Current.ScenarioLuaFile.Data.Size[1];
	}

	#endregion
}
