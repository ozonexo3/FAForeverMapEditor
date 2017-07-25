using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;
using System.Runtime.InteropServices;

public class AppMenu : MonoBehaviour
{

	public GameObject Symmetry;
	public Editing EditingMenu;
	public MapHelperGui MapHelper;

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
	}

	public void MenuButton(string func)
	{
		if (!MenuOpen) return;
		switch (func)
		{
			case "NewMap":
				MapHelper.Map = false;
				MapHelper.OpenComposition(0);
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
			case "Donate":
				Application.OpenURL("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=LUYMTPBDH5V4E&lc=GB&item_name=FAF%20Map%20Editor&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
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


	public void OpenMap()
	{
		LateUpdate();

		System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog();
		FileDialog.InitialDirectory = EnvPaths.GetMapsPath();
		FileDialog.Title = "Open map";
		FileDialog.AddExtension = true;
		FileDialog.DefaultExt = ".lua";
		FileDialog.Filter = "Scenario (*.lua)|*.lua";
		FileDialog.CheckFileExists = true;

		if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			string[] PathSeparation = FileDialog.FileName.Replace("\\", "/").Replace(".lua", "").Split("/".ToCharArray());

			MapLuaParser.Current.ScenarioFileName = PathSeparation[PathSeparation.Length - 1];
			MapLuaParser.Current.FolderName = PathSeparation[PathSeparation.Length - 2];
			string ParentPath = "";
			for(int i = 0; i < PathSeparation.Length - 2; i++)
			{
				ParentPath += PathSeparation[i] + "/";
			}

			MapLuaParser.Current.FolderParentPath = ParentPath;


			MapLuaParser.Current.LoadFile();
		}
	}

	public void SaveMapAs()
	{
		if (!MapLuaParser.Current.MapLoaded())
			return;

		LateUpdate();

		System.Windows.Forms.FolderBrowserDialog FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
		FolderDialog.SelectedPath = MapLuaParser.Current.FolderParentPath;
		FolderDialog.Description = "Save map to folder";
		FolderDialog.ShowNewFolderButton = true;

		if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			if (System.IO.Directory.GetDirectories(FolderDialog.SelectedPath).Length > 0 || System.IO.Directory.GetFiles(FolderDialog.SelectedPath).Length > 0)
			{
				Debug.LogError("Selected directory is not empty! " + FolderDialog.SelectedPath);
				return;
			}

			string[] PathSeparation = FolderDialog.SelectedPath.Replace("\\", "/").Split("/".ToCharArray());

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
