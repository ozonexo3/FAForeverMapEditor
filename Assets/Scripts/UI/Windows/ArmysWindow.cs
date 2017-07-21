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

		Rect AreaSize = MapLuaParser.Current.GetAreaRect();

		float X = (AreaSize.x / ScmapEditor.Current.map.Width) * ImageSize;
		float Y = (AreaSize.y / ScmapEditor.Current.map.Height) * ImageSize;
		float W = ((ScmapEditor.Current.map.Width - AreaSize.width) / ScmapEditor.Current.map.Width) * ImageSize;
		float H = ((ScmapEditor.Current.map.Height - AreaSize.height) / ScmapEditor.Current.map.Height) * ImageSize;

		AreaImages[0].sizeDelta = new Vector2(Mathf.Clamp(X, 0, ImageSize), 0);
		AreaImages[1].sizeDelta = new Vector2(0, Mathf.Clamp(Y, 0, ImageSize));
		AreaImages[2].sizeDelta = new Vector2(Mathf.Clamp(W, 0, ImageSize), 0);
		AreaImages[3].sizeDelta = new Vector2(0, Mathf.Clamp(H, 0, ImageSize));

		//AreaImages[0].sizeDelta = new Vector2(Mathf.Clamp(X, 0, ImageSize), 0);
		//AreaImages[1].sizeDelta = new Vector2(0, Mathf.Clamp(Y, 0, ImageSize));
		//AreaImages[2].sizeDelta = new Vector2(Mathf.Clamp(W, 0, ImageSize), 0);
		//AreaImages[3].sizeDelta = new Vector2(0, Mathf.Clamp(H, 0, ImageSize));

		/*
		if(Scenario.ScenarioData.DefaultArea){
			//float X = (Scenario.ScenarioData.Area.x / Scenario.ScenarioData.Size.x) * ImageSize;
			//float Y = (Scenario.ScenarioData.Area.y / Scenario.ScenarioData.Size.y) * ImageSize;
			//float W = ((Scenario.ScenarioData.Size.x - Scenario.ScenarioData.Area.width) / Scenario.ScenarioData.Size.x) * ImageSize;
			//float H = ((Scenario.ScenarioData.Size.y - Scenario.ScenarioData.Area.height) / Scenario.ScenarioData.Size.y) * ImageSize;


			//AreaImages[0].sizeDelta = new Vector2(Mathf.Clamp(X, 0, ImageSize), 0);
			//AreaImages[1].sizeDelta = new Vector2(0, Mathf.Clamp(Y, 0, ImageSize));
			//AreaImages[2].sizeDelta = new Vector2(Mathf.Clamp(W, 0, ImageSize), 0);
			//AreaImages[3].sizeDelta = new Vector2(0, Mathf.Clamp(H, 0, ImageSize));
		}
		else{
			AreaImages[0].sizeDelta = new Vector2(1, 0);
			AreaImages[1].sizeDelta = new Vector2(0, 1);
			AreaImages[2].sizeDelta = new Vector2(1, 0);
			AreaImages[3].sizeDelta = new Vector2(0, 1);
		}
		*/
	}

	public void Clean(){
		foreach(ArmyMinimapButton but in ArmyButtons){
			Destroy(but.gameObject);
		}
		ArmyButtons = new List<ArmyMinimapButton>();
	}

	public void UpdateArmys(){
		Clean();

		int i = 0;
		for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
		{
			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
			for (int m = 0; m < Mcount; m++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj != null &&
					MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker && 
					ArmyInfo.ArmyExist(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name))
				{
					GameObject NewBut = Instantiate(ArmyButtonPrefab) as GameObject;
					NewBut.transform.SetParent(Pivot);
					ArmyButtons.Add(NewBut.GetComponent<ArmyMinimapButton>());
					ArmyButtons[i].Controler = this;
					ArmyButtons[i].InstanceId = i;
					ArmyButtons[i].ArmyId = 0;
					ArmyButtons[i].ArmyTeam = 0;
					ArmyInfo.GetArmyId(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name, out ArmyButtons[i].ArmyId, out ArmyButtons[i].ArmyTeam);

					//ArmyButtons[i].Name.text = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name.ToString();
					ArmyButtons[i].Name.text = (i + 1).ToString();

					Vector3 IconPos = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.transform.localPosition;
					IconPos.y = IconPos.z;
					IconPos.z = 0;

					IconPos.x /= MapLuaParser.GetMapSizeX() / 10f;
					IconPos.y /= MapLuaParser.GetMapSizeY() / 10f;

					IconPos.x *= ImageSize;
					IconPos.y *= ImageSize;

					NewBut.GetComponent<RectTransform>().localPosition = IconPos;

					i++;
				}
			}
		}

		/*
		for (int a = 0; a < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count; a++)
		{
		}
		*/

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

				if(but.ArmyId % 2 == 0) but.Team.color = Color.red;
				else but.Team.color = Color.blue;
				break;

			}
		}
	}

	int SelectedArmy = 0;
	int SelectedTeam = 0;

	public void ClickedArmy(int id){
		SelectedArmy = ArmyButtons[id].ArmyId;
		SelectedTeam = ArmyButtons[id].ArmyTeam;
		ArmyIdsList.GenerateIds(id);

		ArmyIdsList.GetComponent<RectTransform>().position = ArmyButtons[id].GetComponent<RectTransform>().position + Vector3.right * 12;

	}

	public void ChangeSelectedToId(int newId, int newT){
		//List<MapLuaParser.Army> NewArmys = new List<MapLuaParser.Army>();

		/*
		int c = 0;
		for (int t = 0; t < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length; t++)
		{
			List<MapLua.ScenarioLua.Army> NewArmies = new List<MapLua.ScenarioLua.Army>();

			for (int a = 0; a < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys.Count; a++)
			{
				if(a == newId && t == newT)
				{
					NewArmies.Add(MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[SelectedTeam].Armys[SelectedArmy]);
				}
				else if(a == SelectedArmy && t == SelectedTeam)
				{
					NewArmies.Add(MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[newT].Armys[newId]);
				}
				else
				{
					NewArmies.Add(MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys[a]);
				}


				//if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys[a])
			}

			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys = NewArmies;
		}
		*/


		MapLua.SaveLua.Marker FromMarker = null;
		MapLua.SaveLua.Marker ToMarker = null;

		for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
		{
			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
			for (int m = 0; m < Mcount; m++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj != null &&
					MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker &&
					ArmyInfo.ArmyExist(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name))
				{

					int ArmyId = 0;
					int TeamId = 0;
					ArmyInfo.GetArmyId(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name, out ArmyId, out TeamId);


					if(ArmyId == newId && TeamId == newT)
					{
						ToMarker = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m];
					}
					else if(ArmyId == SelectedArmy && TeamId == SelectedTeam)
					{
						FromMarker = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m];
					}

					if (ToMarker != null && FromMarker != null)
						break;

					//Vector3 IconPos = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.transform.localPosition;
				}
			}
		}

		if (ToMarker != null && FromMarker != null)
		{
			Undo.Current.RegisterMarkersMove(false);

			Vector3 OldPos = FromMarker.MarkerObj.Tr.localPosition;
			FromMarker.MarkerObj.Tr.localPosition = ToMarker.MarkerObj.Tr.localPosition;
			ToMarker.MarkerObj.Tr.localPosition = OldPos;
		}


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
