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

public class AppMenu : MonoBehaviour
{

	public GameObject Symmetry;
	public Editing EditingMenu;

	public Button[] Buttons;
	public GameObject[] Popups;
	public GameObject RecentMaps;
	public Toggle GridToggle;
	public Toggle SlopeToggle;

	//Local
	bool MenuOpen = false;
	bool ButtonClicked;

	// important to keep the instance alive while the hook is active.
	UnityDragAndDropHook hook;
	void OnEnable()
	{
		// must be created on the main thread to get the right thread id.
		hook = new UnityDragAndDropHook();
		hook.InstallHook();
		hook.OnDroppedFiles += OnFiles;
	}
	void OnDisable()
	{
		hook.UninstallHook();
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



		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
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
			case "Slope":
				ScmapEditor.Current.ToogleSlope(SlopeToggle.isOn);
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
				string LogPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
				LogPath += "Low";
				LogPath += "\\" + Application.companyName;
				LogPath += "\\" + Application.productName;
				LogPath += "\\" + "output_log.txt";

				Debug.Log(LogPath);

				System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + LogPath + "\"");
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
					Arguments += " /predeployed /enablediskwatch";

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

							Debug.LogError("Game executable not exist at given path: " + EnvPaths.GetInstalationPath() + "bin/");
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

	#region SaveAs
	public void SaveMapAs()
	{
		if (!MapLuaParser.IsMapLoaded)
			return;

		LateUpdate();


		var paths = StandaloneFileBrowser.OpenFolderPanel("Save map as...", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			if (System.IO.Directory.GetDirectories(paths[0]).Length > 0 || System.IO.Directory.GetFiles(paths[0]).Length > 0)
			{
				Debug.LogError("Selected directory is not empty! " + paths[0]);
				GenericPopup.ShowPopup(GenericPopup.PopupTypes.Error, "Error", "Selected folder is not empty! Select empty folder.", "OK", null);
				return;
			}

			string[] PathSeparation = paths[0].Replace("\\", "/").Split("/".ToCharArray());

			string FileBeginName = PathSeparation[PathSeparation.Length - 1].ToLower();
			//MapLuaParser.Current.ScenarioFileName = FileBeginName + "_scenario";

			string NewFolderName = PathSeparation[PathSeparation.Length - 1];
			SaveAsNewFolder(NewFolderName, FileBeginName);
		}
	}

	public void SaveAsNewVersion()
	{
		int NextVersion = (int)MapLuaParser.Current.ScenarioLuaFile.Data.map_version + 1;

		GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "Create new version", "Create version " + NextVersion + " of this map?", "Yes", SaveAsNewVersionYes, "No", null);
	}

	public void SaveAsNewVersionYes()
	{
		MapLuaParser.Current.ScenarioLuaFile.Data.map_version++;

		string DeltaFolderName = MapLuaParser.Current.FolderName;
		string NewFolderName = "";
		if (DeltaFolderName.ToLower().Contains(".v"))
		{
			for (int i = 0; i < DeltaFolderName.Length; i++)
			{
				if (DeltaFolderName.ToLower().StartsWith(".v"))
				{
					break;
				}
				else
				{
					NewFolderName += DeltaFolderName[i];
					DeltaFolderName = DeltaFolderName.Remove(0, 1);
					i--;
				}
			}
		}
		else
		{
			NewFolderName = MapLuaParser.Current.FolderName;
		}

		NewFolderName += ".v" + ((int)MapLuaParser.Current.ScenarioLuaFile.Data.map_version).ToString("0000");

		if (SaveAsNewFolder(NewFolderName))
		{

		}
		else
		{
			// Error! Revert changes
			MapLuaParser.Current.ScenarioLuaFile.Data.map_version--;
		}

	}

	bool SaveAsNewFolder(string NewFolderName, string FileBeginName = "")
	{

		string SystemPath = MapLuaParser.Current.FolderParentPath + NewFolderName;

		if (!System.IO.Directory.Exists(SystemPath))
		{
			System.IO.Directory.CreateDirectory(SystemPath);
		}
		else if(System.IO.Directory.GetFiles(SystemPath + "/").Length <= 0)
		{

		}
		else
		{
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.Error, "Error", "Next map version folder already exist.\nRemove it or load never version.", "OK", null);

			return false;
		}

		string OldFolderName = MapLuaParser.Current.FolderName;

		MapLuaParser.Current.FolderName = NewFolderName;


		string OldScript = MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath);
		string OldOptionsFile = OldScript.Replace("_script.lua", "_options.lua");

		if (string.IsNullOrEmpty(FileBeginName))
		{
			FileBeginName = MapLuaParser.Current.ScenarioFileName.Replace("_scenario", "");
		}
		else
		{
			MapLuaParser.Current.ScenarioFileName = FileBeginName + "_scenario";
		}

		MapLuaParser.Current.ScenarioLuaFile.Data.map = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + ".scmap";
		MapLuaParser.Current.ScenarioLuaFile.Data.save = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + "_save.lua";
		MapLuaParser.Current.ScenarioLuaFile.Data.script = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + "_script.lua";
		MapLuaParser.Current.ScenarioLuaFile.Data.preview = "";

		LoadRecentMaps.MoveLastMaps(MapLuaParser.Current.ScenarioFileName, MapLuaParser.Current.FolderName, MapLuaParser.Current.FolderParentPath);


		MapLuaParser.Current.SaveMap(false);

		string LoadScript = System.IO.File.ReadAllText(OldScript);
		//Replace old folder name to new one
		LoadScript = LoadScript.Replace(OldFolderName + "/", NewFolderName + "/");
		System.IO.File.WriteAllText(MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath), LoadScript);

		//System.IO.File.Copy(OldScript, MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath));

		if (System.IO.File.Exists(OldOptionsFile))
		{ // Found options, copy it
			System.IO.File.Copy(OldOptionsFile, MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath).Replace("_script.lua", "_options.lua"));
		}

		string EnvPath = MapLuaParser.Current.FolderParentPath + OldFolderName + "/env";
		if (System.IO.Directory.Exists(EnvPath))
		{
			DirectoryCopy(EnvPath, MapLuaParser.Current.FolderParentPath + NewFolderName + "/env", true);
		}

		EditingMenu.MapInfoMenu.UpdateFields();
		WindowStateSever.WindowStateSaver.ChangeWindowName(NewFolderName);

		return true;
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);

		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, false);
		}

		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}
	#endregion
}
