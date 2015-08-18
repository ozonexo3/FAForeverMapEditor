using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;

public class Undo : MonoBehaviour {

	[Header("Classes")]
	public		MapLuaParser		Scenario;
	public		ScmapEditor			Scmap;
	public		AppMenu				Menu;
	public		Editing				EditMenu;
	public		MapInfo				MapInfoMenu;

	[Header("Config")]
	public		int				MaxHistoryLength;
	public		UndoPrefabs		Prefabs;

	[Header("History")]
	public		List<HistoryObject>		History;
	public		List<HistoryObject>		RedoHistory;
	public		int				CurrentStage;
	public		bool			CurrentSaved = false;


	public void DoUndo(){
		// Add current stage of same type as "Undo" command
		int UndoTo = History.Count - 1;

		switch (History [UndoTo].UndoType) {
		case HistoryObject.UndoTypes.MapInfo:
			Debug.Log("Undo: " + History [UndoTo].UndoCommandName);
			HistoryMapInfo MapInfoNew = History [UndoTo].GetComponent<HistoryMapInfo>();
			Scenario.ScenarioData.MapName = MapInfoNew.Name;
			Scenario.ScenarioData.MapDesc = MapInfoNew.Desc;
			MapInfoMenu.UpdateFields();
			break;
		case HistoryObject.UndoTypes.Markers:

			break;
		}

	}

	public void DoRedo(){
		// Use RedoCommand


	}


	public void RegisterMapInfo(){
		GameObject NewHistoryStep = Instantiate (Prefabs.MapInfo) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		CurrentStage = ListId;
		Debug.Log (NewHistoryStep.transform.position);
		HistoryMapInfo MapInfoNew = NewHistoryStep.GetComponent<HistoryMapInfo>();
		MapInfoNew.Name = Scenario.ScenarioData.MapName;
		MapInfoNew.Desc = Scenario.ScenarioData.MapDesc;
		MapInfoNew.Version = Scenario.ScenarioData.Version;
		MapInfoNew.Script = Scenario.ScriptId;
	}

	public void RegisterMarkers(){

	}

	int AddToHistory(HistoryObject NewHistoryStep){
		History.Add (NewHistoryStep);
		if (History.Count - 1 >= MaxHistoryLength) {
			Destroy(History[0].gameObject);
			History.RemoveAt(0);
		}
		return History.Count - 1;
	}

	int AddToUndoHistory(HistoryObject NewHistoryStep){
		RedoHistory.Add (NewHistoryStep);
		if (RedoHistory.Count >= MaxHistoryLength) {
			//Destroy(RedoHistory[0].gameObject);
			//RedoHistory.RemoveAt(0);
		}
		return RedoHistory.Count - 1;
	}

}
