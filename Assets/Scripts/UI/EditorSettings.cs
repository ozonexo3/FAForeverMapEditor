using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorSettings : MonoBehaviour {

	public InputField		PathField;

	void OnEnable(){
		PathField.text = PlayerPrefs.GetString("GameDataPath", "gamedata/");
	}

	public void Open(){
		gameObject.SetActive(true);
	}

	public void Close(){
		gameObject.SetActive(false);
	}

	public void Save(){
		string newPath = PathField.text.Replace("\\", "/");
		if(newPath[newPath.Length - 1].ToString() != "/") newPath += "/";
		PlayerPrefs.SetString("GameDataPath", newPath);
		PlayerPrefs.Save();
		gameObject.SetActive(false);
	}
}
