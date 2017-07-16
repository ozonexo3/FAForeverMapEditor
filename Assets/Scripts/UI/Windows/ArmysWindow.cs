using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ArmysWindow : MonoBehaviour {

	public		MapLuaParser		Scenario;
	public		Text				AutoTeamText;
	public		int					AutoteamId;
	public		string[]			AutoteamNames;
	public		ArmyListId			ArmyIdsList;

	public		RectTransform[]		AreaImages;
	const		float				ImageSize = 400;

	public		GameObject					ArmyButtonPrefab;
	public		Transform					Pivot;
	public		List<ArmyMinimapButton>		ArmyButtons = new List<ArmyMinimapButton>();

	int SelectedArmy = 0;

	void OnEnable(){
		AutoteamId = PlayerPrefs.GetInt("AutoTeam", 0);
		AutoTeamText.text = AutoteamNames[AutoteamId];
		UpdateArea();
		UpdateArmys();
	}

	void OnDisable(){
		Clean();
	}

	void UpdateArea(){
		//if(Scenario.ScenarioData.DefaultArea){
			//float X = (Scenario.ScenarioData.Area.x / Scenario.ScenarioData.Size.x) * ImageSize;
			//float Y = (Scenario.ScenarioData.Area.y / Scenario.ScenarioData.Size.y) * ImageSize;
			//float W = ((Scenario.ScenarioData.Size.x - Scenario.ScenarioData.Area.width) / Scenario.ScenarioData.Size.x) * ImageSize;
			//float H = ((Scenario.ScenarioData.Size.y - Scenario.ScenarioData.Area.height) / Scenario.ScenarioData.Size.y) * ImageSize;


			//AreaImages[0].sizeDelta = new Vector2(Mathf.Clamp(X, 0, ImageSize), 0);
			//AreaImages[1].sizeDelta = new Vector2(0, Mathf.Clamp(Y, 0, ImageSize));
			//AreaImages[2].sizeDelta = new Vector2(Mathf.Clamp(W, 0, ImageSize), 0);
			//AreaImages[3].sizeDelta = new Vector2(0, Mathf.Clamp(H, 0, ImageSize));
		//}
		//else{
			AreaImages[0].sizeDelta = new Vector2(1, 0);
			AreaImages[1].sizeDelta = new Vector2(0, 1);
			AreaImages[2].sizeDelta = new Vector2(1, 0);
			AreaImages[3].sizeDelta = new Vector2(0, 1);
		//}
	}

	public void Clean(){
		foreach(ArmyMinimapButton but in ArmyButtons){
			Destroy(but.gameObject);
		}
		ArmyButtons = new List<ArmyMinimapButton>();
	}

	public void UpdateArmys(){
		Clean();

		//TODO
		/*
		for(int i = 0; i < Scenario.ARMY_.Count; i++){
			GameObject NewBut = Instantiate(ArmyButtonPrefab) as GameObject;
			NewBut.transform.SetParent(Pivot);
			ArmyButtons.Add(NewBut.GetComponent<ArmyMinimapButton>());
			ArmyButtons[i].Controler = this;
			ArmyButtons[i].ArmyId = i;
			ArmyButtons[i].Name.text = (i + 1).ToString();

			Vector3 IconPos = new Vector3(Scenario.ARMY_[i].position.x, Scenario.ARMY_[i].position.z, 0);
			IconPos.x /= MapLuaParser.GetMapSizeX() / 10f;
			IconPos.y /= MapLuaParser.GetMapSizeY() / 10f;

			IconPos.x *= ImageSize;
			IconPos.y *= ImageSize;

			NewBut.GetComponent<RectTransform>().localPosition = IconPos;
		}
		*/

		UpdateAutoteam();
	}

	public void ChangeAutoteam(){
		AutoteamId++;
		if(AutoteamId > 2) AutoteamId = 0;
		PlayerPrefs.SetInt("AutoTeam", AutoteamId);
		AutoTeamText.text = AutoteamNames[AutoteamId];

		UpdateAutoteam();
	}

	void UpdateAutoteam(){
		if(ArmyButtons.Count == 0) return;

		foreach(ArmyMinimapButton but in ArmyButtons){
			switch(AutoteamId){
			case 0:
				if(but.GetComponent<RectTransform>().localPosition.y > ImageSize / -2f) but.Team.color = Color.red;
				else but.Team.color = Color.blue;
				break;
			case 1:
				if(but.GetComponent<RectTransform>().localPosition.x > ImageSize / 2f) but.Team.color = Color.red;
				else but.Team.color = Color.blue;
				break;
			case 2:

				if(ArmyButtons.IndexOf(but) %2 == 0) but.Team.color = Color.red;
				else but.Team.color = Color.blue;
				break;

			}
		}
	}

	public void ClickedArmy(int id){
		SelectedArmy = id;
		ArmyIdsList.GenerateIds(id);

		ArmyIdsList.GetComponent<RectTransform>().position = ArmyButtons[id].GetComponent<RectTransform>().position + Vector3.right * 12;

	}

	public void ChangeSelectedToId(int newId){
		//List<MapLuaParser.Army> NewArmys = new List<MapLuaParser.Army>();

		//TODO
		/*
		for(int i = 0; i < Scenario.ARMY_.Count; i++){ 
			if(i == newId){
				NewArmys.Add(Scenario.ARMY_[SelectedArmy]);
			}
			else if(i == SelectedArmy){
				NewArmys.Add(Scenario.ARMY_[newId]);
			}
			else{
				NewArmys.Add(Scenario.ARMY_[i]);
			}
			NewArmys[i].name = "ARMY_" + (i + 1).ToString();
		}

		Scenario.ARMY_ = NewArmys;
		//Scenario.SortArmys();
		*/
		UpdateArmys();
	}
}
