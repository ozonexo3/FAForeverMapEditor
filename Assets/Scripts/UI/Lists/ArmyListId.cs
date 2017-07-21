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
					GameObject NewBut = Instantiate(ButtonPrefab) as GameObject;
					NewBut.transform.SetParent(Pivot);
					ArmyButtons.Add(NewBut.GetComponent<ArmyIdButton>());
					ArmyButtons[i].Controler = this;
					ArmyButtons[i].Id = i;

					ArmyInfo.GetArmyId(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name, out ArmyButtons[i].ArmyId, out ArmyButtons[i].ArmyTeam);


					ArmyButtons[i].Name.text = (i + 1).ToString();

					NewBut.GetComponent<RectTransform>().localPosition = Vector3.up * -27 * i;
					if (i == defaultId) ArmyButtons[i].Select.color = new Color(0.15f, 0.15f, 0.5f, 1);

					i++;
				}
			}
		}


		//TODO
		/*
		for(int i = 0; i < Armys.Scenario.ARMY_.Count; i++){
			GameObject NewBut = Instantiate(ButtonPrefab) as GameObject;
			NewBut.transform.SetParent(Pivot);
			ArmyButtons.Add(NewBut.GetComponent<ArmyIdButton>());
			ArmyButtons[i].Controler = this;
			ArmyButtons[i].Id = i;
			ArmyButtons[i].Name.text = (i + 1).ToString();

			NewBut.GetComponent<RectTransform>().localPosition = Vector3.up * -27 * i;
			if(i == defaultId) ArmyButtons[i].Select.color = new Color(0.15f, 0.15f, 0.5f, 1);
		}
		

		GetComponent<RectTransform>().sizeDelta = new Vector2(52, 6 + 27 * Armys.Scenario.ARMY_.Count);
		*/
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
