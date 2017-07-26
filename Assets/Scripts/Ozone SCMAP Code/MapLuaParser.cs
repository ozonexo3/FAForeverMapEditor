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
	public GameObject Background;
	public		Undo			History;
	public		MapHelperGui	HelperGui;
	public string FolderParentPath;
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


	//[Header("Local Data")]
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

	//public		int MexTotalCount = 0;
	//public		int HydrosTotalCount = 0;
	//public		int SiTotalCount = 0;

	//[HideInInspector]
	[Header("Local Data")]
	public		Vector3			MapCenterPoint;

	public		int				ScriptId = 0;

	public static string			BackupPath;
	public static string			StructurePath;


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

	}

	public bool MapLoaded()
	{
		return !string.IsNullOrEmpty(FolderName) && !string.IsNullOrEmpty(ScenarioFileName) && !string.IsNullOrEmpty(FolderParentPath);
	}

	public void ResetUI()
	{
		EditMenu.gameObject.SetActive(false);
		Background.SetActive(true);

		FolderParentPath = "";
		FolderName = "";
		ScenarioFileName = "";
	}

	#region Loading

	public IEnumerator ForceLoadMapAtPath(string path){
		path = path.Replace("\\", "/");
		Debug.Log("Load from: " + path);

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
		if (LoadingMapProcess)
			return;

		ScmapEditor.Current.UnloadMap();

		EditMenu.gameObject.SetActive(true);
		Background.SetActive(false);


		StartCoroutine("LoadingFile");
	}

	bool loadSave = true;
	bool LoadProps = true;

	bool LoadingMapProcess = false;
	IEnumerator LoadingFile(){

		while (SavingMapProcess)
			yield return null;

		bool AllFilesExists = true;
		string Error = "";
		if (!System.IO.Directory.Exists (FolderParentPath)) {
			Error = "Map folder not exist: " + FolderParentPath;
			Debug.LogError (Error);
			AllFilesExists = false;
		}

		if (AllFilesExists && !System.IO.File.Exists (FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua")) {
			Error = "Scenario.lua not exist: " + FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua";
			Debug.LogError (Error);
			AllFilesExists = false;
		}
		string ScenarioText = System.IO.File.ReadAllText(FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua");
		if (AllFilesExists && !ScenarioText.StartsWith("version = 3"))
		{
			//Debug.Log(ScenarioText);
			Error = "Selected file is not a proper scenario.lua file";
			Debug.LogError(Error);
			AllFilesExists = false;
		}

		if(AllFilesExists && !System.IO.File.Exists(EnvPaths.GetGamedataPath() + "/env.scd")){
			Error = "No source files in gamedata folder: " + EnvPaths.GetGamedataPath();
			Debug.LogError (Error);
			AllFilesExists = false;
		}

		if (AllFilesExists) {
			// Begin load
			LoadRecentMaps.MoveLastMaps(ScenarioFileName, FolderName, FolderParentPath);
			LoadingMapProcess = true;
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
			if (ScenarioLuaFile.Load(FolderName, ScenarioFileName, FolderParentPath)){
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
				Coroutine LoadingProps = PropsMenu.StartCoroutine(PropsMenu.LoadProps());
				while (PropsMenu.LoadingProps)
					yield return null;

				PropsMenu.gameObject.SetActive(false);
			}

			InfoPopup.Show (false);

			EditMenu.Categorys [0].GetComponent<MapInfo> ().UpdateFields ();
			LoadingMapProcess = false;
		}
		else {
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

	public void SaveMapAs()
	{

	}

	public bool BackupFiles = true;
	public void SaveMap(bool Backup = true)
	{
		if (!MapLuaParser.Current.MapLoaded())
			return;

		BackupFiles = Backup;

		Debug.Log("Save map");

		SavingMapProcess = true;
		InfoPopup.Show(true, "Saving map...");

		StartCoroutine(SaveMapProcess());
	}

	public static bool SavingMapProcess = false;
	public IEnumerator SaveMapProcess(){

		yield return null;

		string BackupId = System.DateTime.Now.Month.ToString() +System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString() + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString();
		BackupPath = FolderParentPath + FolderName + "/Backup_" + BackupId;

		if(BackupFiles)
			System.IO.Directory.CreateDirectory(BackupPath);
		yield return null;

		// Scenario.lua
		string ScenarioFilePath = FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua";
		if(BackupFiles && System.IO.File.Exists(ScenarioFilePath))
			System.IO.File.Move(ScenarioFilePath, BackupPath + "/" + ScenarioFileName + ".lua");
		ScenarioLuaFile.Save(ScenarioFilePath);
		yield return null;


		//Save.lua
		string SaveFilePath = ScenarioLuaFile.Data.save.Replace("/maps/", FolderParentPath);
		string FileName = ScenarioLuaFile.Data.save;
		string[] Names = FileName.Split(("/").ToCharArray());
		if (BackupFiles && System.IO.File.Exists(SaveFilePath))
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

		string MapFilePath = ScenarioLuaFile.Data.map.Replace("/maps/", FolderParentPath);

		string FileName = ScenarioLuaFile.Data.map;
		char[] NameSeparator = ("/").ToCharArray();
		string[] Names = FileName.Split(NameSeparator);
		//Debug.Log(BackupPath + "/" + Names[Names.Length - 1]);
		if(BackupFiles && System.IO.File.Exists(MapFilePath))
			System.IO.File.Move(MapFilePath, BackupPath + "/" + Names[Names.Length - 1]);
			 

		HeightmapControler.SaveScmapFile();
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

		string SaveFilePath = ScenarioLuaFile.Data.script.Replace("/maps/", FolderParentPath);

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

	public Rect GetAreaRect()
	{
		if (SaveLuaFile.Data.areas.Length > 0 && !AreaInfo.HideArea)
		{
			//int bigestAreaId = 0;
			Rect bigestAreaRect = new Rect(SaveLuaFile.Data.areas[0].rectangle);

			if (AreaInfo.SelectedArea != null)
			{
				bigestAreaRect = AreaInfo.SelectedArea.rectangle;
			}
			else
			{
				for (int i = 1; i < SaveLuaFile.Data.areas.Length; i++)
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
			}

			float LastY = bigestAreaRect.y;
			bigestAreaRect.y = ScmapEditor.Current.map.Width - bigestAreaRect.height;
			bigestAreaRect.height = ScmapEditor.Current.map.Width - LastY;
			return bigestAreaRect;
		}
		else
		{

			return new Rect(0, 0, ScmapEditor.Current.map.Width, ScmapEditor.Current.map.Height);
		}

	}

	public void UpdateArea(){
		if(SaveLuaFile.Data.areas.Length > 0 && !AreaInfo.HideArea)
		{
			//int bigestAreaId = 0;
			Rect bigestAreaRect = GetAreaRect();

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
