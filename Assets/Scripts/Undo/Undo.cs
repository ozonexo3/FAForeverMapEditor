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

	// keys
	void Update(){
		if(Input.GetKey(KeyCode.LeftControl)){
			if(Input.GetKey(KeyCode.LeftShift)){
				if(Input.GetKeyDown(KeyCode.Z)){
					DoRedo();
				}
			}
			else if(Input.GetKeyDown(KeyCode.Z)){
				DoUndo();
			}
			else if(Input.GetKeyDown(KeyCode.Y)){
				DoRedo();
			}
		}
	}

	public void DoUndo(){
		// Add current stage of same type as "Undo" command
		int UndoTo = CurrentStage - 1;

		if(UndoTo < 0){
			Debug.LogWarning("Cant undo: last");
			return;
		}

		switch (History [UndoTo].UndoType) {
		case HistoryObject.UndoTypes.MapInfo:
			RegisterRedoMapInfo();
			HistoryMapInfo MapInfoNew = History [UndoTo].GetComponent<HistoryMapInfo>();
			Scenario.ScenarioData.MapName = MapInfoNew.Name;
			Scenario.ScenarioData.MapDesc = MapInfoNew.Desc;
			Scenario.ScenarioData.Version = MapInfoNew.Version;
			Scenario.ScriptId = MapInfoNew.Script;
			MapInfoMenu.UpdateScriptToggles(MapInfoNew.Script);
			MapInfoMenu.UpdateFields();
			break;
		case HistoryObject.UndoTypes.MarkersMove:
			RegisterRedoMarkersMove();
			HistoryMarkersMove MarkerMoveNew = History [UndoTo].GetComponent<HistoryMarkersMove>();

			EditMenu.SelectedMarker.position = MarkerMoveNew.SelectedMarker;
			for(int i = 0; i < EditMenu.SelectedSymmetryMarkers.Count; i++){
				EditMenu.SelectedSymmetryMarkers[i].position = MarkerMoveNew.SelectedSymmetryMarkers[i];
			}
			for(int i = 0; i < EditMenu.Selected.Count; i++){
				EditMenu.Selected[i].transform.position = MarkerMoveNew.MarkersPosSelection[i];
			}
			for(int i = 0; i < EditMenu.SymmetrySelectionList.Length; i++){
				for(int e = 0; e < EditMenu.SymmetrySelectionList[i].MirrorSelected.Count; e++){
					EditMenu.SymmetrySelectionList[i].MirrorSelected[e].transform.position = MarkerMoveNew.MirrorPos[i].MarkersPosSelection[e];
				}
			}
			break;
		}

		CurrentStage = UndoTo;
	}

	public void DoRedo(){
		// Use RedoCommand
		if(CurrentStage == History.Count){
			Debug.LogWarning("Cant redo: current");
			return;
		}
		int RedoTo = MaxHistoryLength - 1 - CurrentStage;

		switch (History [RedoTo].UndoType) {
			case HistoryObject.UndoTypes.MapInfo:
			HistoryMapInfo MapInfoNew = RedoHistory [RedoTo].GetComponent<HistoryMapInfo>();
			Scenario.ScenarioData.MapName = MapInfoNew.Name;
			Scenario.ScenarioData.MapDesc = MapInfoNew.Desc;
			Scenario.ScenarioData.Version = MapInfoNew.Version;
			Scenario.ScriptId = MapInfoNew.Script;
			MapInfoMenu.UpdateScriptToggles(MapInfoNew.Script);
			MapInfoMenu.UpdateFields();
			break;
		case HistoryObject.UndoTypes.MarkersMove:
			HistoryMarkersMove MarkerMoveNew = RedoHistory [RedoTo].GetComponent<HistoryMarkersMove>();
			
			EditMenu.SelectedMarker.position = MarkerMoveNew.SelectedMarker;
			for(int i = 0; i < EditMenu.SelectedSymmetryMarkers.Count; i++){
				EditMenu.SelectedSymmetryMarkers[i].position = MarkerMoveNew.SelectedSymmetryMarkers[i];
			}
			for(int i = 0; i < EditMenu.Selected.Count; i++){
				EditMenu.Selected[i].transform.position = MarkerMoveNew.MarkersPosSelection[i];
			}
			for(int i = 0; i < EditMenu.SymmetrySelectionList.Length; i++){
				for(int e = 0; e < EditMenu.SymmetrySelectionList[i].MirrorSelected.Count; e++){
					EditMenu.SymmetrySelectionList[i].MirrorSelected[e].transform.position = MarkerMoveNew.MirrorPos[i].MarkersPosSelection[e];
				}
			}
			break;
		}
		CurrentStage++;
	}

//*********************************************  REGISTER UNDO
	public void RegisterMapInfo(){
		GameObject NewHistoryStep = Instantiate (Prefabs.MapInfo) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		CurrentStage = ListId;
		HistoryMapInfo MapInfoNew = NewHistoryStep.GetComponent<HistoryMapInfo>();
		MapInfoNew.Name = Scenario.ScenarioData.MapName;
		MapInfoNew.Desc = Scenario.ScenarioData.MapDesc;
		MapInfoNew.Version = Scenario.ScenarioData.Version;
		MapInfoNew.Script = Scenario.ScriptId;

		CurrentStage = History.Count;
	}

	public void RegisterMarkersMove(){
		GameObject NewHistoryStep = Instantiate (Prefabs.MarkersMove) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		CurrentStage = ListId;
		HistoryMarkersMove HistoryNew = NewHistoryStep.GetComponent<HistoryMarkersMove>();

		HistoryNew.SelectedMarker = EditMenu.SelectedMarker.position;
		HistoryNew.SelectedSymmetryMarkers = new Vector3[EditMenu.SelectedSymmetryMarkers.Count];
		for(int i = 0; i < EditMenu.SelectedSymmetryMarkers.Count; i++){
			HistoryNew.SelectedSymmetryMarkers[i] = EditMenu.SelectedSymmetryMarkers[i].position;
		}

		HistoryNew.MarkersPosSelection = new Vector3[EditMenu.Selected.Count];
		for(int i = 0; i < EditMenu.Selected.Count; i++){
			HistoryNew.MarkersPosSelection[i] = EditMenu.Selected[i].transform.position;
		}

		HistoryNew.MirrorPos = new HistoryMarkersMove.MirrorMarkersPos[EditMenu.SymmetrySelectionList.Length];
		for(int i = 0; i < EditMenu.SymmetrySelectionList.Length; i++){
			HistoryNew.MirrorPos[i] = new HistoryMarkersMove.MirrorMarkersPos();
			HistoryNew.MirrorPos[i].MarkersPosSelection = new Vector3[EditMenu.SymmetrySelectionList[i].MirrorSelected.Count];
			for(int e = 0; e < EditMenu.SymmetrySelectionList[i].MirrorSelected.Count; e++){
				HistoryNew.MirrorPos[i].MarkersPosSelection[e] = EditMenu.SymmetrySelectionList[i].MirrorSelected[e].transform.position;
			}
		}

		CurrentStage = History.Count;
	}


//********************************************	REGISTER REDO
	public void RegisterRedoMapInfo(){
		GameObject NewHistoryStep = Instantiate (Prefabs.MapInfo) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToRedoHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		CurrentStage = ListId;
		HistoryMapInfo MapInfoNew = NewHistoryStep.GetComponent<HistoryMapInfo>();
		MapInfoNew.Name = Scenario.ScenarioData.MapName;
		MapInfoNew.Desc = Scenario.ScenarioData.MapDesc;
		MapInfoNew.Version = Scenario.ScenarioData.Version;
		MapInfoNew.Script = Scenario.ScriptId;
	}


	public void RegisterRedoMarkersMove(){
		GameObject NewHistoryStep = Instantiate (Prefabs.MarkersMove) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToRedoHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		CurrentStage = ListId;
		HistoryMarkersMove HistoryNew = NewHistoryStep.GetComponent<HistoryMarkersMove>();
		
		HistoryNew.SelectedMarker = EditMenu.SelectedMarker.position;
		HistoryNew.SelectedSymmetryMarkers = new Vector3[EditMenu.SelectedSymmetryMarkers.Count];
		for(int i = 0; i < EditMenu.SelectedSymmetryMarkers.Count; i++){
			HistoryNew.SelectedSymmetryMarkers[i] = EditMenu.SelectedSymmetryMarkers[i].position;
		}
		
		HistoryNew.MarkersPosSelection = new Vector3[EditMenu.Selected.Count];
		for(int i = 0; i < EditMenu.Selected.Count; i++){
			HistoryNew.MarkersPosSelection[i] = EditMenu.Selected[i].transform.position;
		}
		
		HistoryNew.MirrorPos = new HistoryMarkersMove.MirrorMarkersPos[EditMenu.SymmetrySelectionList.Length];
		for(int i = 0; i < EditMenu.SymmetrySelectionList.Length; i++){
			HistoryNew.MirrorPos[i] = new HistoryMarkersMove.MirrorMarkersPos();
			HistoryNew.MirrorPos[i].MarkersPosSelection = new Vector3[EditMenu.SymmetrySelectionList[i].MirrorSelected.Count];
			for(int e = 0; e < EditMenu.SymmetrySelectionList[i].MirrorSelected.Count; e++){
				HistoryNew.MirrorPos[i].MarkersPosSelection[e] = EditMenu.SymmetrySelectionList[i].MirrorSelected[e].transform.position;
			}
		}
	}


//********************************************* FUNCTIONS
	int AddToHistory(HistoryObject NewHistoryStep){
		History.Add (NewHistoryStep);
		if (History.Count - 1 >= MaxHistoryLength) {
			Destroy(History[0].gameObject);
			History.RemoveAt(0);
		}
		return History.Count - 1;
	}

	int AddToRedoHistory(HistoryObject NewHistoryStep){
		RedoHistory.Add (NewHistoryStep);
		if (RedoHistory.Count - 1 >= MaxHistoryLength) {
			Destroy(RedoHistory[0].gameObject);
			RedoHistory.RemoveAt(0);
		}
		return RedoHistory.Count - 1;
	}
}