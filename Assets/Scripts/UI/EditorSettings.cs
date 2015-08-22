using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorSettings : MonoBehaviour {

	public InputField		PathField;
	public InputField		MapsPathField;
	public	Slider			HistorySlider;
	public	Undo			History;

	void OnEnable(){
		PathField.text = PlayerPrefs.GetString("GameDataPath", "gamedata/");
		MapsPathField.text = PlayerPrefs.GetString("MapsPath", "maps/");
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
		if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		if(newPath[0].ToString() == "/") newPath = newPath.Remove(0,1);
		PlayerPrefs.SetString("GameDataPath", newPath);
		gameObject.SetActive(false);

		newPath = MapsPathField.text.Replace("\\", "/");
		if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		if(newPath[0].ToString() == "/") newPath = newPath.Remove(0,1);
		PlayerPrefs.SetString("MapsPath", newPath);
		gameObject.SetActive(false);

		PlayerPrefs.SetInt("UndoHistry", (int)HistorySlider.value);
		History.MaxHistoryLength = (int)HistorySlider.value;

		PlayerPrefs.Save();
	}
}
