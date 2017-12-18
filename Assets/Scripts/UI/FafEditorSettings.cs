using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using SFB;

public class FafEditorSettings : MonoBehaviour {

	public InputField		PathField;
	public InputField		MapsPathField;
	public InputField		BackupPathField;
	public	Slider			HistorySlider;
	public	Undo			History;

	public Dropdown PlayAs;
	public Toggle FogOfWar;

	public const int DefaultUndoHistory = 50;
	public const string UndoHistory = "UndoHistrySteps";

	void OnEnable(){
		PathField.text = EnvPaths.GetInstalationPath();
		MapsPathField.text = EnvPaths.GetMapsPath();
		BackupPathField.text = EnvPaths.GetBackupPath();

		PlayAs.value = GetFaction();
		FogOfWar.isOn = GetFogOfWar();
	}
	

	public void Open(){
		HistorySlider.value = PlayerPrefs.GetInt(UndoHistory, DefaultUndoHistory);
		gameObject.SetActive(true);
	}

	public void Close(){
		gameObject.SetActive(false);
	}

	public void Save(){
		EnvPaths.SetInstalationPath (PathField.text);

		EnvPaths.SetMapsPath (MapsPathField.text);

		EnvPaths.SetBackupPath(BackupPathField.text);

		PlayerPrefs.SetInt(UndoHistory, (int)HistorySlider.value);
		if(History)History.MaxHistoryLength = (int)HistorySlider.value;

		PlayerPrefs.SetInt("PlayMap_Faction", PlayAs.value);
		PlayerPrefs.SetInt("PlayMap_FogOfWar", FogOfWar.isOn?1:0);

		PlayerPrefs.Save();
		gameObject.SetActive(false);

	}

	[DllImport("user32.dll")]
	private static extern void SaveFileDialog(); //in your case : OpenFileDialog

	public void BrowseGamedataPath(){

		var paths = StandaloneFileBrowser.OpenFolderPanel("Select Forged Alliance instalation path", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			PathField.text = paths[0];
		}
	}

	public void BrowseMapPath()
	{

		var paths = StandaloneFileBrowser.OpenFolderPanel("Select 'Maps' folder.", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			MapsPathField.text = paths[0];
		}
	}

	public void BrowseBackupPath()
	{
		var paths = StandaloneFileBrowser.OpenFolderPanel("Select backup 'Maps' folder.", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			BackupPathField.text = paths[0];
		}
	}


	public void ResetGamedata(){
		EnvPaths.GenerateGamedataPath ();
		PathField.text = EnvPaths.DefaultGamedataPath;

	}

	public void ResetMap(){
		EnvPaths.GenerateMapPath ();
		MapsPathField.text = EnvPaths.DefaultMapPath;
	}

	public void ResetBackup()
	{
		BackupPathField.text = EnvPaths.DefaultMapPath.Replace("Maps/", "MapsBackup/");
	}

	public static int GetFaction()
	{
		return PlayerPrefs.GetInt("PlayMap_Faction", 0);
	}

	public static bool GetFogOfWar()
	{
		return PlayerPrefs.GetInt("PlayMap_FogOfWar", 1) == 1;
	}
}
