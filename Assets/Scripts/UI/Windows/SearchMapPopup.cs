using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class SearchMapPopup : MonoBehaviour {

	public			StartingScreen		Start;
	public			string				SelectedFolder;
	public			string				SelectedScenario;

	public			GameObject			ListPrefab;
	public			RectTransform		Pivot;
	public			List<MapSearchListField>	AllFields = new List<MapSearchListField>();

	const			float				Margin = 55.0f;
					bool			Folder;

	void OnDisable(){
		foreach(RectTransform child in Pivot){
			Destroy(child.gameObject);
		}
		AllFields = new List<MapSearchListField>();
	}

	public void ClickedField(int id){
		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");

		if (Folder) {
			SelectedFolder = Directory.GetDirectories (MapPath)[id].Replace(MapPath, "");

			foreach(RectTransform child in Pivot){
				Destroy(child.gameObject);
			}
			AllFields = new List<MapSearchListField>();

			CreateScenarioList();


		} else {
			if(id < 0){
				foreach(RectTransform child in Pivot){
					Destroy(child.gameObject);
				}
				AllFields = new List<MapSearchListField>();
				CreateFolderList();
				return;
			}
			// Finish selection
			string Path = MapPath + SelectedFolder;
			SelectedScenario = Directory.GetFiles (Path)[id].Replace("\\", "/").Replace(Path + "/", "").Replace(".lua", "");

			Start.Folder.text = SelectedFolder;
			Start.Name.text = SelectedScenario;

			Start.InputEnd();
			Start.UpdateFields();

			gameObject.SetActive(false);
		}
	}

	public void CreateFolderList(){
		Folder = true;
		SelectedFolder = "";
		SelectedScenario = "";
		gameObject.SetActive (true);
		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");

		string[] Directories = Directory.GetDirectories (MapPath);

		for (int i = 0; i < Directories.Length; i++) {
			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			newList.GetComponent<RectTransform>().localPosition = Vector3.up * ((Margin * -i));
			newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 45);

			AllFields.Add(newList.GetComponent<MapSearchListField>());
			AllFields[i].Controler = this;
			AllFields[i].ObjectName.text = Directories[i].Replace(MapPath, "");
			AllFields[i].Id = i;
		}

		Vector2 PivotRect = Pivot.sizeDelta ;
		PivotRect.y = Margin * Directories.Length;
		Pivot.sizeDelta  = PivotRect;
	}


	public void CreateScenarioList(){
		Folder = false;
		SelectedScenario = "";
		gameObject.SetActive (true);
		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string Path = MapPath + SelectedFolder;
		string[] Files = Directory.GetFiles (Path);

		int count = 0;
		GameObject BackBtn = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
		BackBtn.GetComponent<RectTransform>().SetParent(Pivot);
		BackBtn.GetComponent<RectTransform>().localPosition = Vector3.up * ((Margin * -count));
		BackBtn.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 45);
		
		AllFields.Add(BackBtn.GetComponent<MapSearchListField>());
		AllFields[count].Controler = this;
		AllFields[count].Id = -1;
		AllFields[count].ObjectName.text = "<< back";
		count++;

		for (int i = 0; i < Files.Length; i++) {
			if(!Files[i].EndsWith(".lua")) continue;
			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			newList.GetComponent<RectTransform>().localPosition = Vector3.up * ((Margin * -count));
			newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 45);
			
			AllFields.Add(newList.GetComponent<MapSearchListField>());
			AllFields[count].Controler = this;
			AllFields[count].Id = i;
			AllFields[count].ObjectName.text = Files[i].Replace("\\", "/").Replace(Path + "/", "");
			count++;
		}
		
		Vector2 PivotRect = Pivot.sizeDelta ;
		PivotRect.y = Margin * count;
		Pivot.sizeDelta  = PivotRect;
	}


}
