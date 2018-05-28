using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;
using System.Text;
using System.Runtime.InteropServices;
using SFB;

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

		var extensions = new[]
		{
			new ExtensionFilter("Scenario", "lua")
		};

		var paths = StandaloneFileBrowser.OpenFilePanel("Open map", EnvPaths.GetMapsPath(), extensions, false);

		/*
		System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog();
		FileDialog.InitialDirectory = EnvPaths.GetMapsPath();
		FileDialog.Title = "Open map";
		FileDialog.AddExtension = true;
		FileDialog.DefaultExt = ".lua";
		FileDialog.Filter = "Scenario (*.lua)|*.lua";
		FileDialog.CheckFileExists = true;
		*/

		//if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			string[] PathSeparation = paths[0].Replace("\\", "/").Replace(".lua", "").Split("/".ToCharArray());

			MapLuaParser.Current.ScenarioFileName = PathSeparation[PathSeparation.Length - 1];
			MapLuaParser.Current.FolderName = PathSeparation[PathSeparation.Length - 2];
			string ParentPath = "";
			for (int i = 0; i < PathSeparation.Length - 2; i++)
			{
				ParentPath += PathSeparation[i] + "/";
			}

			MapLuaParser.Current.FolderParentPath = ParentPath;


			MapLuaParser.Current.LoadFile();
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



	public void SaveMapAs()
	{
		if (!MapLuaParser.IsMapLoaded)
			return;

		LateUpdate();


		var paths = StandaloneFileBrowser.OpenFolderPanel("Save map as...", EnvPaths.GetMapsPath(), false);

		/*
		System.Windows.Forms.FolderBrowserDialog FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
		FolderDialog.SelectedPath = MapLuaParser.Current.FolderParentPath;
		FolderDialog.Description = "Save map to folder";
		FolderDialog.ShowNewFolderButton = true;
		*/

		//if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			if (System.IO.Directory.GetDirectories(paths[0]).Length > 0 || System.IO.Directory.GetFiles(paths[0]).Length > 0)
			{
				Debug.LogError("Selected directory is not empty! " + paths[0]);
				return;
			}

			string[] PathSeparation = paths[0].Replace("\\", "/").Split("/".ToCharArray());

			string FileBeginName = PathSeparation[PathSeparation.Length - 1].ToLower();
			MapLuaParser.Current.ScenarioFileName = FileBeginName + "_scenario";
			MapLuaParser.Current.FolderName = PathSeparation[PathSeparation.Length - 1];
			string ParentPath = "";
			for (int i = 0; i < PathSeparation.Length - 1; i++)
			{
				ParentPath += PathSeparation[i] + "/";
			}

			MapLuaParser.Current.FolderParentPath = ParentPath;

			MapLuaParser.Current.ScenarioLuaFile.Data.map = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + ".scmap";
			MapLuaParser.Current.ScenarioLuaFile.Data.save = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + "_save.lua";
			MapLuaParser.Current.ScenarioLuaFile.Data.script = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + "_script.lua";
			MapLuaParser.Current.ScenarioLuaFile.Data.preview = "";

			LoadRecentMaps.MoveLastMaps(MapLuaParser.Current.ScenarioFileName, MapLuaParser.Current.FolderName, MapLuaParser.Current.FolderParentPath);

			MapLuaParser.Current.SaveMap(false);
		}
	}
}
