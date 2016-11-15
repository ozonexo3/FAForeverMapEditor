using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class EditorSettings : MonoBehaviour {

	public InputField		PathField;
	public InputField		MapsPathField;
	public	Slider			HistorySlider;
	public	Undo			History;

	void OnEnable(){
		PathField.text = EnvPaths.GetInstalationPath();
		MapsPathField.text = EnvPaths.GetMapsPath();
	}
	

	public void Open(){
		HistorySlider.value = PlayerPrefs.GetInt("UndoHistry", 5);
		gameObject.SetActive(true);
	}

	public void Close(){
		gameObject.SetActive(false);
	}

	public void Save(){
		string newPath = PathField.text.Replace("\\", "/");
		//if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		//if(newPath[0].ToString() == "/") newPath = newPath.Remove(0,1);
		//PlayerPrefs.SetString("GameDataPath", newPath);

		EnvPaths.SetInstalationPath (PathField.text);

		//newPath = MapsPathField.text.Replace("\\", "/");
		//if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		//if(newPath[0].ToString() == "/") newPath = newPath.Remove(0,1);
		//PlayerPrefs.SetString("MapsPath", newPath);

		EnvPaths.SetMapsPath (MapsPathField.text);

		PlayerPrefs.SetInt("UndoHistry", (int)HistorySlider.value);
		if(History)History.MaxHistoryLength = (int)HistorySlider.value;

		PlayerPrefs.Save();
		gameObject.SetActive(false);

	}

	[DllImport("user32.dll")]
	private static extern void SaveFileDialog(); //in your case : OpenFileDialog

	private void myMethod()
	{
		System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

		//your code
	}

	public void BrowseMapPath(){

		System.Windows.Forms.FolderBrowserDialog FolderDialog = new FolderBrowserDialog ();

		FolderDialog.ShowNewFolderButton = false;
		FolderDialog.Description = "Select 'Maps' folder.";

		if (FolderDialog.ShowDialog() == DialogResult.OK)
		{
			MapsPathField.text = FolderDialog.SelectedPath;
		}
	}

	public void BrowseGamedataPath(){
		System.Windows.Forms.FolderBrowserDialog FolderDialog = new FolderBrowserDialog ();

		FolderDialog.ShowNewFolderButton = false;
		FolderDialog.Description = "Select 'Gamedata' folder in Supreme Commander Forget Alliance instalation directory.";

		if (FolderDialog.ShowDialog() == DialogResult.OK)
		{
			PathField.text = FolderDialog.SelectedPath;
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
