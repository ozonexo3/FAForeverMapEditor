using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ArmyListId : MonoBehaviour {

	public		ArmysWindow					Armys;
	public		GameObject					ButtonPrefab;
	public		Transform					Pivot;
	public		List<ArmyIdButton>			ArmyButtons = new List<ArmyIdButton>();

	public void GenerateIds(int defaultId = 0){
		gameObject.SetActive(true);

		Clean();

		if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations.Length == 0 || MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams.Length == 0)
			return;

		var AllArmies = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams[0].Armys;

		for (int a = 0; a < AllArmies.Count; a++)
		{
			MapLua.SaveLua.Marker ArmyMarker = MapLua.SaveLua.GetMarker(AllArmies[a].Name);

			if (ArmyMarker != null && ArmyMarker.MarkerObj != null && ArmyMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker)
			{
				GameObject NewBut = Instantiate(ButtonPrefab) as GameObject;
				NewBut.transform.SetParent(Pivot);
				ArmyButtons.Add(NewBut.GetComponent<ArmyIdButton>());
				ArmyButtons[a].Controler = this;
				ArmyButtons[a].Id = a;
				ArmyButtons[a].ArmyId = a;
				ArmyButtons[a].ArmyTeam = 0;

				//ArmyInfo.GetArmyId(AllArmies[a].Name, out ArmyButtons[i].ArmyId, out ArmyButtons[i].ArmyTeam);


				ArmyButtons[a].Name.text = (a + 1).ToString();

				NewBut.GetComponent<RectTransform>().localPosition = Vector3.up * -27 * a;
				if (a == defaultId) ArmyButtons[a].Select.color = new Color(0.15f, 0.15f, 0.5f, 1);
			}
		}

	}

	public void Selected(int id){
		Armys.ChangeSelectedToId(ArmyButtons[id].ArmyId, ArmyButtons[id].ArmyTeam);
		gameObject.SetActive(false);
	}

	void OnDisable(){
		Clean();
	}

	void Clean()
	{
		foreach (ArmyIdButton but in ArmyButtons)
		{
			Destroy(but.gameObject);
		}
		ArmyButtons = new List<ArmyIdButton>();
	}
}
