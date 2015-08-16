using UnityEngine;
using System.Collections;

public class MapHelperGui : MonoBehaviour {

	public 		bool 					MapLoaded;
	public 		MapLuaParser 			Loader;
	public		CameraControler			KameraKontroler;

	public		bool					Map;
	public		bool					Markers;
	public		bool					Players;

	public		GameObject[]			Kompositions;
	public		GameObject				BackGround;

	public		StartingScreen			StartScreen;
	public		LastMapsPopup			LastMapsList;

	void Start(){
		OpenComposition(0);
	}

	public void ButtonFunction(string func){
		Debug.Log("Button pressed: " + func);
		switch(func){
		case "LoadMap":
			MoveLastMaps(Loader.ScenarioFileName, Loader.FolderName);
			Loader.LoadFile();
			Map = true;
			OpenComposition(1);
			break;
		case "SearchMap":

			break;
		case "LastMaps":
			Kompositions[2].SetActive(true);
			break;
		case "LastMapCancel":
			Kompositions[2].SetActive(false);
			break;
		case "LastMapSelect":
			if(string.IsNullOrEmpty(PlayerPrefs.GetString("MapScenarioFile_" + LastMapsList.Selected, ""))) return;
			Kompositions[2].SetActive(false);
			Loader.ScenarioFileName = PlayerPrefs.GetString("MapScenarioFile_" + LastMapsList.Selected, "");  
			Loader.FolderName = PlayerPrefs.GetString("MapFolder_" + LastMapsList.Selected, ""); 
			StartScreen.UpdateFields();
			break;
		}
	}

	void OpenComposition(int id){
		foreach(GameObject obj in Kompositions){
			if(obj.activeSelf) obj.SetActive(false);
		}
		Kompositions[id].SetActive(true);
		if(id == 0){
			BackGround.SetActive(true);
		}
		else{
			BackGround.SetActive(false);
		}

	}

	public void MapFilesInputEnd(){

	}

	public void MoveLastMaps(string scenario = "", string folder = ""){

		if(scenario == PlayerPrefs.GetString("MapScenarioFile_" + 0, "") && folder == PlayerPrefs.GetString("MapFolder_" + 0, "")) return;

		string[] NewScenario = new string[9];
		string[] NewFolder = new string[9];

		int offset = 0;
		NewScenario[0] = scenario;
		NewFolder[0] = folder;

		for(int i = 0; i < 9; i++){
			if(i == 0){

				//PlayerPrefs.SetString("MapScenarioFile_" + 0, scenario);
				//PlayerPrefs.SetString("MapFolder_" + 0, folder);
			}
			else{
				while(PlayerPrefs.GetString("MapScenarioFile_" + (i - 1 + offset), "") == scenario && scenario != "" && offset < 9){
					offset ++;
				}
				NewScenario[i] = PlayerPrefs.GetString("MapScenarioFile_" + (i - 1 + offset), "");
				NewFolder[i] = PlayerPrefs.GetString("MapFolder_" + (i - 1 + offset), "");
			}
		}

		for(int i = 0; i < 9; i++){

			PlayerPrefs.SetString("MapScenarioFile_" + i, NewScenario[i]);
			PlayerPrefs.SetString("MapFolder_" + i, NewFolder[i]);

		}


	}
}
