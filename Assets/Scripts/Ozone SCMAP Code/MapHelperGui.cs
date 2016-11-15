using UnityEngine;
using System.Collections;

public class MapHelperGui : MonoBehaviour {

	public 		bool 					MapLoaded;
	public		CameraControler			KameraKontroler;

	public		bool					Map;
	public		bool					Markers;
	public		bool					Players;

	public		GameObject[]			Kompositions;
	public		GameObject				BackGround;
	public		GameObject				LoadingPopup;

	public		StartingScreen			StartScreen;
	public		LastMapsPopup			LastMapsList;
	public		SearchMapPopup 			SearchMap;
	public		GenericInfoPopup ErrorPopup;

	static int More = 1;

	static string[] Args;
	void Start(){
		//OpenComposition(1);
		Args = System.Environment.GetCommandLineArgs();
		if(Args.Length > 0)
		/*for(int i = 0; i < Args.Length; i++){
			Debug.Log(Args[i]);
		}*/

		if(Args.Length == 3 && Args[1] == "-setInstalationPath"){
			//PlayerPrefs.SetString("GameDataPath", Args[2]);
			EnvPaths.SetInstalationPath (Args [2]);
			Debug.Log("Success! Instalation path changed to: " + Args[2]);
		}


		if(Args.Length == 6 + More && Args[1 + More] == "-renderPreviewImage"){
			foreach(GameObject obj in Kompositions) obj.SetActive(false);
			BackGround.SetActive(false);
			LoadingPopup.SetActive(true);
			MapLuaParser.Current.HeightmapControler.WaterLevel.gameObject.SetActive(false);
			GetGamedataFile.MipmapBias = -0.9f;
			StartCoroutine("RenderImageAndClose");
		}
	}


	public IEnumerator RenderImageAndClose(){
		var LoadScmapFile = MapLuaParser.Current.StartCoroutine("ForceLoadMapAtPath", Args[4 + More]);
		yield return LoadScmapFile;

		foreach(GameObject obj in Kompositions) obj.SetActive(false);
		BackGround.SetActive(false);

		int Widht = int.Parse(Args[2 + More]);
		int Height = int.Parse(Args[3 + More] );

		KameraKontroler.RestartCam();

		KameraKontroler.RenderCamera(Widht, Height, Args[5+ More]);

		Debug.Log("Success! Preview rendered to: " + Args[5+ More]);

		Application.Quit();
	}

	public void ButtonFunction(string func){
		switch(func){
		case "LoadMap":
			MoveLastMaps(MapLuaParser.Current.ScenarioFileName, MapLuaParser.Current.FolderName);
			OpenComposition(1);
			Map = true;
			MapLuaParser.Current.LoadFile();
			break;
		case "SearchMap":
			SearchMap.CreateFolderList ();
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
			MapLuaParser.Current.ScenarioFileName = PlayerPrefs.GetString("MapScenarioFile_" + LastMapsList.Selected, "");  
			MapLuaParser.Current.FolderName = PlayerPrefs.GetString("MapFolder_" + LastMapsList.Selected, ""); 
			StartScreen.UpdateFields();
			break;
		}
	}

	public void ReturnLoadingWithError(string Error){
		Map = false;
		OpenComposition(0);
		ErrorPopup.Show (true, Error);
		ErrorPopup.InvokeHide ();
	}

	public void OpenComposition(int id){
		foreach(GameObject obj in Kompositions){
			if(obj)
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
