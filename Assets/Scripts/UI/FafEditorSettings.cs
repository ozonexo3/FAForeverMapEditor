using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using SFB;

public class FafEditorSettings : MonoBehaviour {

	public InputField		PathField;
	public InputField		MapsPathField;
	public	Slider			HistorySlider;
	public	Undo			History;

	public const int DefaultUndoHistory = 50;
	public const string UndoHistory = "UndoHistrySteps";

	void OnEnable(){
		PathField.text = EnvPaths.GetInstalationPath();
		MapsPathField.text = EnvPaths.GetMapsPath();
	}
	

	public void Open(){
		HistorySlider.value = PlayerPrefs.GetInt(UndoHistory, DefaultUndoHistory);
		gameObject.SetActive(true);
	}

	public void Close(){
		gameObject.SetActive(false);
	}

	public void Save(){
		//string newPath = PathField.text.Replace("\\", "/");
		//if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		//if(newPath[0].ToString() == "/") newPath = newPath.Remove(0,1);
		//PlayerPrefs.SetString("GameDataPath", newPath);

		EnvPaths.SetInstalationPath (PathField.text);

		//newPath = MapsPathField.text.Replace("\\", "/");
		//if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		//if(newPath[0].ToString() == "/") newPath = newPath.Remove(0,1);
		//PlayerPrefs.SetString("MapsPath", newPath);

		EnvPaths.SetMapsPath (MapsPathField.text);

		PlayerPrefs.SetInt(UndoHistory, (int)HistorySlider.value);
		if(History)History.MaxHistoryLength = (int)HistorySlider.value;

		PlayerPrefs.Save();
		gameObject.SetActive(false);

	}

	[DllImport("user32.dll")]
	private static extern void SaveFileDialog(); //in your case : OpenFileDialog


	public void BrowseMapPath(){

		var paths = StandaloneFileBrowser.OpenFolderPanel("Select 'Maps' folder.", EnvPaths.GetMapsPath(), false);

		/*System.Windows.Forms.FolderBrowserDialog FolderDialog = new FolderBrowserDialog ();

		FolderDialog.ShowNewFolderButton = false;
		FolderDialog.Description = "Select 'Maps' folder.";*/

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			MapsPathField.text = paths[0];
		}
	}

	public void BrowseGamedataPath(){

		var paths = StandaloneFileBrowser.OpenFolderPanel("Select 'Gamedata' folder in FA instalation directory", EnvPaths.GetMapsPath(), false);

		/*
		System.Windows.Forms.FolderBrowserDialog FolderDialog = new FolderBrowserDialog ();

		FolderDialog.ShowNewFolderButton = false;
		FolderDialog.Description = "Select 'Gamedata' folder in Supreme Commander Forget Alliance instalation directory.";
		*/

		//if (FolderDialog.ShowDialog() == DialogResult.OK)
		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			PathField.text = paths[0];
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

}
