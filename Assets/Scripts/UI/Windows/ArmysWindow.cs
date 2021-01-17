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

	}

	public void Clean(){
		foreach(ArmyMinimapButton but in ArmyButtons){
			Destroy(but.gameObject);
		}
		ArmyButtons = new List<ArmyMinimapButton>();
	}

	public void UpdateArmys(){
		Clean();


		if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations.Length == 0 || MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams.Length == 0)
			return;

		var AllArmies = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams[0].Armys;

		for (int a = 0; a < AllArmies.Count; a++)
		{
			MapLua.SaveLua.Marker ArmyMarker = MapLua.SaveLua.GetMarker(AllArmies[a].Name);

			if(ArmyMarker != null && ArmyMarker.MarkerObj != null && ArmyMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker)
			{
				GameObject NewBut = Instantiate(ArmyButtonPrefab, Pivot) as GameObject;
				//NewBut.transform.SetParent(Pivot);
				ArmyButtons.Add(NewBut.GetComponent<ArmyMinimapButton>());
				ArmyButtons[a].Controler = this;
				ArmyButtons[a].InstanceId = a;
				ArmyButtons[a].ArmyId = a;
				ArmyButtons[a].ArmyTeam = 0;
				//ArmyInfo.GetArmyId(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name, out ArmyButtons[i].ArmyId, out ArmyButtons[i].ArmyTeam);

				//ArmyButtons[i].Name.text = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name.ToString();
				ArmyButtons[a].Name.text = (a + 1).ToString();

				Vector3 IconPos = ArmyMarker.MarkerObj.transform.localPosition;
				IconPos.y = IconPos.z;
				IconPos.z = 0;

				IconPos.x /= MapLuaParser.GetMapSizeX() / 10f;
				IconPos.y /= MapLuaParser.GetMapSizeY() / 10f;

				IconPos.x *= ImageSize;
				IconPos.y *= ImageSize;

				NewBut.GetComponent<RectTransform>().localPosition = IconPos;
			}
		}


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
	//int SelectedTeam = 0;

	public void ClickedArmy(int id){
		SelectedArmy = ArmyButtons[id].ArmyId;
		//SelectedTeam = ArmyButtons[id].ArmyTeam;
		ArmyIdsList.GenerateIds(id);

		ArmyIdsList.GetComponent<RectTransform>().position = ArmyButtons[id].GetComponent<RectTransform>().position + Vector3.right * 12;

	}

	public void ChangeSelectedToId(int newId, int newT){



		if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations.Length == 0 || MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams.Length == 0)
			return;

		var AllArmies = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams[0].Armys;

		MapLua.SaveLua.Marker FromMarker = MapLua.SaveLua.GetMarker(AllArmies[SelectedArmy].Name);
		MapLua.SaveLua.Marker ToMarker = MapLua.SaveLua.GetMarker(AllArmies[newId].Name);

		if (ToMarker != null && FromMarker != null)
		{
			Undo.RegisterUndo(new UndoHistory.HistoryMarkersMove(), new UndoHistory.HistoryMarkersMove.MarkersMoveHistoryParameter(false));

			Vector3 OldPos = FromMarker.MarkerObj.Tr.localPosition;
			FromMarker.MarkerObj.Tr.localPosition = ToMarker.MarkerObj.Tr.localPosition;
			ToMarker.MarkerObj.Tr.localPosition = OldPos;
		}


		UpdateArmys();
	}
}
