using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using System.Text;
using System.Runtime.InteropServices;
using SFB;
using System.Linq;
using B83.Win32;
using System.IO;

public partial class AppMenu : MonoBehaviour
{

	public GameObject Symmetry;
	public Editing EditingMenu;

	public Button[] Buttons;
	public GameObject[] Popups;
	public GameObject RecentMaps;
	public Toggle GridToggle;
	public Toggle BuildGridToggle;
	public Toggle GeneralGridToggle;
	public Toggle AIGridToggle;
	public Toggle SlopeToggle;
	public Toggle RulerToggle;

	//Local
	bool MenuOpen = false;
	bool ButtonClicked;

	// important to keep the instance alive while the hook is active.
	UnityDragAndDropHook hook;
	void OnEnable()
	{
		Application.targetFrameRate = 60;
		// must be created on the main thread to get the right thread id.
		hook = new UnityDragAndDropHook();
		hook.InstallHook();
		hook.OnDroppedFiles += OnFiles;
		Application.logMessageReceived += HandleLog;
	}
	void OnDisable()
	{
		hook.UninstallHook();
		Application.logMessageReceived -= HandleLog;
	}

	bool ErrorFound = false;
	void HandleLog(string logString, string stackTrace, LogType type)
	{
		switch (type)
		{
			case LogType.Exception:
				if(!ErrorFound)
					GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "Crash! (Exception)", "Editor crashed is now unsafe! Report bug with log file!\n" + logString, "Show log", ShowEditorLog, "Continue", null);
				ErrorFound = true;
				break;
			case LogType.Error:
				if (!ErrorFound)
					GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "Error", logString, "Show log", ShowEditorLog, "Continue", null);
				ErrorFound = true;
				break;
		}
	}

	void ShowEditorLog()
	{
		string LogPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
		LogPath += "Low";
		LogPath += "\\" + Application.companyName;
		LogPath += "\\" + Application.productName;
		LogPath += "\\" + "Player.log";

		Debug.Log(LogPath);

		System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + LogPath + "\"");
	}

	void LateUpdate()
	{
		if (MenuOpen)
		{
			if (ButtonClicked)
			{
				ButtonClicked = false;
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				foreach (GameObject obj in Popups)
				{
					obj.SetActive(false);
				}
				foreach (Button but in Buttons)
				{
					but.interactable = true;
				}
				MenuOpen = false;
			}
		}



		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S) && !CameraControler.IsInputFieldFocused())
		{
			MapLuaParser.Current.SaveMap();
		}
	}

	public void MenuButton(string func)
	{
		if (!MenuOpen) return;
		switch (func)
		{
			case "NewMap":
				OpenNewMap();
				break;
			case "Open":
				OpenMap();
				//MapHelper.Map = false;
				//MapHelper.OpenComposition(0);
				break;
			case "OpenRecent":
				RecentMaps.SetActive(true);
				ButtonClicked = true;
				break;
			case "Save":
				MapLuaParser.Current.SaveMap();
				break;
			case "SaveAs":
				SaveMapAs();
				break;
			case "SaveAsNewVersion":
				SaveAsNewVersion();
				break;
			case "Undo":
				break;
			case "Redo":
				break;
			case "Symmetry":
				Symmetry.SetActive(true);
				break;
			case "Grid":
				ScmapEditor.Current.ToogleGrid(GridToggle.isOn);
				break;
			case "BuildGrid":
				ScmapEditor.Current.ToogleBuildGrid(BuildGridToggle.isOn);
				break;
			case "GeneralGrid":
				ScmapEditor.Current.ToogleGeneraldGrid(GeneralGridToggle.isOn);
				break;
			case "AIGrid":
				ScmapEditor.Current.ToogleAIGrid(AIGridToggle.isOn);
				break;
			case "Slope":
				ScmapEditor.Current.ToogleSlope(SlopeToggle.isOn);
				break;
			case "Ruler":
				Ruler.Toggle(RulerToggle.isOn);
				break;
			case "Forum":
				Application.OpenURL("http://forums.faforever.com/viewtopic.php?f=45&t=10647");
				break;
			case "Wiki":
				Application.OpenURL("https://wiki.faforever.com/index.php?title=FA_Forever_Map_Editor");
				break;
			case "UnitDb":
				Application.OpenURL("http://direct.faforever.com/faf/unitsDB/");
				break;
			case "EditorLog":
				ShowEditorLog();
				break;
			case "Donate":
				Application.OpenURL("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=LUYMTPBDH5V4E&lc=GB&item_name=FAF%20Map%20Editor&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
				break;
			case "Shortcuts":
				Application.OpenURL("https://wiki.faforever.com/index.php?title=FA_Forever_Map_Editor#Useful_shortuts");
				break;
			case "PlayMap":
				if (MapLuaParser.IsMapLoaded)
				{
					string Arguments = "";
					Arguments += "/map \"" + "/maps/" + MapLuaParser.Current.FolderName + "/" + MapLuaParser.Current.ScenarioFileName + ".lua" + "\"";
					//Arguments += "/map \"" + MapLuaParser.LoadedMapFolderPath + MapLuaParser.Current.ScenarioFileName + ".lua" + "\"";
					Arguments += " /faction " + (FafEditorSettings.GetFaction() + 1).ToString();
					Arguments += " /victory sandbox";
					Arguments += " /gamespeed adjustable";
					Arguments += " /civilians";
					Arguments += " /enablediskwatch";
					//Arguments += " / predeployed/";

					if (!FafEditorSettings.GetFogOfWar())
						Arguments += " /nofog";

					string GamePath = EnvPaths.GetInstalationPath() + "bin/SupremeCommander.exe";

					if (!System.IO.File.Exists(GamePath))
					{
						string OtherPath = EnvPaths.GetInstalationPath() + "bin/ForgedAlliance.exe";
						if (System.IO.File.Exists(OtherPath))
						{
							GamePath = OtherPath;
						}
						else
						{

							Debug.LogWarning("Game executable not exist at given path: " + EnvPaths.GetInstalationPath() + "bin/");
							return;
						}
					}
					Debug.Log("Start game: " + GamePath);
					Debug.Log("Args: " + Arguments);

					System.Diagnostics.Process.Start(GamePath, Arguments);
				}
				break;
		}
	}

	public void ToogleCurrentGrid()
	{
		switch (ScmapEditor.Current.GridType)
		{
			case ScmapEditor.GridTypes.Standard:
				GridToggle.isOn = !GridToggle.isOn;
				ScmapEditor.Current.ToogleGrid(GridToggle.isOn);
				break;
			case ScmapEditor.GridTypes.Build:
				BuildGridToggle.isOn = !BuildGridToggle.isOn;
				ScmapEditor.Current.ToogleBuildGrid(BuildGridToggle.isOn);
				break;
			case ScmapEditor.GridTypes.General:
				GeneralGridToggle.isOn = !GeneralGridToggle.isOn;
				ScmapEditor.Current.ToogleGeneraldGrid(GeneralGridToggle.isOn);
				break;
			case ScmapEditor.GridTypes.AI:
				AIGridToggle.isOn = !AIGridToggle.isOn;
				ScmapEditor.Current.ToogleAIGrid(AIGridToggle.isOn);
				break;

		}
	}

	public void OpenMenu(string func)
	{
		foreach (GameObject obj in Popups)
		{
			obj.SetActive(false);
		}
		foreach (Button but in Buttons)
		{
			but.interactable = true;
		}


		RecentMaps.SetActive(false);

		switch (func)
		{
			case "File":
				Popups[0].SetActive(true);
				Buttons[0].interactable = false;
				break;
			case "Edit":
				Popups[1].SetActive(true);
				Buttons[1].interactable = false;
				break;
			case "Tools":
				Popups[2].SetActive(true);
				Buttons[2].interactable = false;
				break;
			case "Symmetry":
				Popups[3].SetActive(true);
				Buttons[3].interactable = false;
				break;
			case "Help":
				Popups[4].SetActive(true);
				Buttons[4].interactable = false;
				break;
		}

		MenuOpen = true;
		ButtonClicked = true;
	}

	public bool IsMenuOpen()
	{
		return MenuOpen;
	}

	#region OpenMap

	public void OpenMap()
	{
		if (MapLuaParser.IsMapLoaded)
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.TriButton, "Save map", "Save current map before opening another map?", "Yes", OpenMapYes, "No", OpenMapNo, "Cancel", OpenMapCancel);
		else
			OpenMapProcess();
	}

	public void OpenMapYes()
	{
		StartCoroutine(OpenMapSave());
	}

	IEnumerator OpenMapSave()
	{
		MapLuaParser.Current.SaveMap();
		yield return null;

		while (MapLuaParser.SavingMapProcess)
			yield return null;

		OpenMapNo();
	}

	public void OpenMapNo()
	{
		ScmapEditor.Current.UnloadMap();
		MapLuaParser.Current.ResetUI();
		OpenMapProcess();
	}

	public void OpenMapCancel()
	{

	}


	public void OpenMapProcess()
	{
		LateUpdate();

		if (string.IsNullOrEmpty(StoreSelectedFile))
		{

			var extensions = new[]
			{
			new ExtensionFilter("Scenario", "lua")
		};

			var paths = StandaloneFileBrowser.OpenFilePanel("Open map", EnvPaths.GetMapsPath(), extensions, false);

			if (paths.Length > 0 && IsLuaFilePath(paths[0]))// && !string.IsNullOrEmpty(paths[0]) && IsScenarioPath(paths[0]))
			{
				StoreSelectedFile = paths[0];
			}
		}


		LoadMapAtPath(StoreSelectedFile);

	}

	static void LoadMapAtPath(string path)
	{
		if (string.IsNullOrEmpty(path))
			return;

		string[] PathSeparation = path.Replace("\\", "/").Replace(".lua", "").Split("/".ToCharArray());

		MapLuaParser.Current.ScenarioFileName = PathSeparation[PathSeparation.Length - 1];
		MapLuaParser.Current.FolderName = PathSeparation[PathSeparation.Length - 2];
		string ParentPath = "";
		for (int i = 0; i < PathSeparation.Length - 2; i++)
		{
			ParentPath += PathSeparation[i] + "/";
		}

		MapLuaParser.Current.FolderParentPath = ParentPath;
		MapLuaParser.Current.LoadFile();
		StoreSelectedFile = "";
	}

	bool IsScenarioPath(string path)
	{
		return path.EndsWith("scenario.lua");
	}

	bool IsLuaFilePath(string path)
	{
		return path.EndsWith(".lua");
	}

	static string StoreSelectedFile;
	void OnFiles(List<string> aFiles, POINT aPos)
	{
		// do something with the dropped file names. aPos will contain the 
		// mouse position within the window where the files has been dropped.
		Debug.Log("Dropped " + aFiles.Count + " files at: " + aPos + "\n" +
			aFiles.Aggregate((a, b) => a + "\n" + b));

		if (aFiles.Count == 0)
			return;


		if (IsScenarioPath(aFiles[0]))
		{
			StoreSelectedFile = aFiles[0];

		}
		else 
		{
			string[] AllFiles = System.IO.Directory.GetFiles(aFiles[0]);

			for(int i = 0; i < AllFiles.Length; i++)
			{
				if (IsScenarioPath(AllFiles[i]))
				{
					StoreSelectedFile = AllFiles[i];
					break;
				}

			}

		}


		if (!string.IsNullOrEmpty(StoreSelectedFile))
		{
			if (MapLuaParser.IsMapLoaded)
				GenericPopup.ShowPopup(GenericPopup.PopupTypes.TriButton, "Save map", "Save current map before opening another map?", "Yes", OpenMapYes, "No", OpenMapNo, "Cancel", OpenMapCancel);
			else
				OpenMapProcess();
		}
	}


	#endregion

	#region OpenRecentMap

	public void OpenRecentMap()
	{
		if (MapLuaParser.Current.ScenarioFileName == PlayerPrefs.GetString(LoadRecentMaps.ScenarioFile + LoadRecentMaps.RecentMapSelected, "")
			&& MapLuaParser.Current.FolderName == PlayerPrefs.GetString(LoadRecentMaps.FolderPath + LoadRecentMaps.RecentMapSelected, "")
			&& MapLuaParser.Current.FolderParentPath == PlayerPrefs.GetString(LoadRecentMaps.ParentPath + LoadRecentMaps.RecentMapSelected, ""))
		{
			Debug.LogWarning("Same map: Ignore loading recent map");
			return; // Same map
		}

		if (MapLuaParser.IsMapLoaded)
		{
			PlaySystemSound.PlayBeep();
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.TriButton, "Save map", "Save current map before opening another map?", "Yes", OpenRecentMapYes, "No", OpenRecentMapNo, "Cancel", OpenMapCancel);

		}
		else
			OpenRecentMapNo();
	}


	public void OpenRecentMapYes()
	{
		if (!MapLuaParser.SavingMapProcess)
			StartCoroutine(OpenRecentMapSave());
	}

	IEnumerator OpenRecentMapSave()
	{
		MapLuaParser.Current.SaveMap();
		yield return null;

		while (MapLuaParser.SavingMapProcess)
			yield return null;

		OpenRecentMapNo();
	}

	public void OpenRecentMapNo()
	{
		ScmapEditor.Current.UnloadMap();
		MapLuaParser.Current.ResetUI();

		MapLuaParser.Current.ScenarioFileName = PlayerPrefs.GetString(LoadRecentMaps.ScenarioFile + LoadRecentMaps.RecentMapSelected, "");
		MapLuaParser.Current.FolderName = PlayerPrefs.GetString(LoadRecentMaps.FolderPath + LoadRecentMaps.RecentMapSelected, "");
		MapLuaParser.Current.FolderParentPath = PlayerPrefs.GetString(LoadRecentMaps.ParentPath + LoadRecentMaps.RecentMapSelected, "");


		MapLuaParser.Current.LoadFile();
	}

	#endregion

	#region New Map
	public GameObject NewMapWindow;

	public void OpenNewMap()
	{
		if (MapLuaParser.IsMapLoaded)
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.TriButton, "Save map", "Save current map before creating new map?", "Yes", OpenNewMapYes, "No", OpenNewMapNo, "Cancel", OpenNewMapCancel);
		else
			NewMapWindow.SetActive(true);
	}

	public void OpenNewMapYes()
	{
		StartCoroutine(OpenNewMapSave());
	}

	IEnumerator OpenNewMapSave()
	{
		MapLuaParser.Current.SaveMap();
		yield return null;

		while (MapLuaParser.SavingMapProcess)
			yield return null;

		OpenNewMapNo();
	}

	public void OpenNewMapNo()
	{
		ScmapEditor.Current.UnloadMap();
		MapLuaParser.Current.ResetUI();
		NewMapWindow.SetActive(true);
	}

	public void OpenNewMapCancel()
	{
		NewMapWindow.SetActive(false);

	}
	#endregion


}
