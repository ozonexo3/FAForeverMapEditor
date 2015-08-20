using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

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
			EditMenu.EditMarkers.SelectedMarker.position = MarkerMoveNew.SelectedMarker;
			for(int i = 0; i < EditMenu.EditMarkers.SelectedSymmetryMarkers.Count; i++){
				EditMenu.EditMarkers.SelectedSymmetryMarkers[i].position = MarkerMoveNew.SelectedSymmetryMarkers[i];
			}
			for(int i = 0; i < EditMenu.EditMarkers.Selected.Count; i++){
				Scenario.SetPosOfMarker(EditMenu.EditMarkers.Selected[i], MarkerMoveNew.MarkersPosSelection[i]);
			}
			for(int i = 0; i < EditMenu.EditMarkers.SymmetrySelectionList.Length; i++){
				for(int e = 0; e < EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count; e++){
					Scenario.SetPosOfMarker(EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected[e], MarkerMoveNew.MirrorPos[i].MarkersPosSelection[e]);
				}
			}
			break;
		case HistoryObject.UndoTypes.MarkersSelection:
			RegisterRedoMarkerSelection();
			HistoryMarkersSelection MarkerSelectionNew = History [UndoTo].GetComponent<HistoryMarkersSelection>();
			EditMenu.EditMarkers.Selected = MarkerSelectionNew.Selected;
			EditMenu.EditMarkers.SymmetrySelectionList = MarkerSelectionNew.SymmetrySelectionList;
			EditMenu.EditMarkers.UpdateSelectionRing();
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
		int RedoTo = History.Count - 1 - CurrentStage;

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
			EditMenu.EditMarkers.SelectedMarker.position = MarkerMoveNew.SelectedMarker;
			for(int i = 0; i < EditMenu.EditMarkers.SelectedSymmetryMarkers.Count; i++){
				EditMenu.EditMarkers.SelectedSymmetryMarkers[i].position = MarkerMoveNew.SelectedSymmetryMarkers[i];
			}
			for(int i = 0; i < EditMenu.EditMarkers.Selected.Count; i++){
				//EditMenu.EditMarkers.Selected[i].transform.position = MarkerMoveNew.MarkersPosSelection[i];
				Scenario.SetPosOfMarker(EditMenu.EditMarkers.Selected[i], MarkerMoveNew.MarkersPosSelection[i]);
			}
			for(int i = 0; i < EditMenu.EditMarkers.SymmetrySelectionList.Length; i++){
				for(int e = 0; e < EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count; e++){
					Scenario.SetPosOfMarker(EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected[e], MarkerMoveNew.MirrorPos[i].MarkersPosSelection[e]);
				}
			}
			break;
		case HistoryObject.UndoTypes.MarkersSelection:
			HistoryMarkersSelection MarkerSelectionNew = RedoHistory [RedoTo].GetComponent<HistoryMarkersSelection>();
			EditMenu.EditMarkers.Selected = MarkerSelectionNew.Selected;
			EditMenu.EditMarkers.SymmetrySelectionList = MarkerSelectionNew.SymmetrySelectionList;
			EditMenu.EditMarkers.UpdateSelectionRing();

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

		HistoryNew.SelectedMarker = EditMenu.EditMarkers.SelectedMarker.position;
		HistoryNew.SelectedSymmetryMarkers = new Vector3[EditMenu.EditMarkers.SelectedSymmetryMarkers.Count];
		for(int i = 0; i < EditMenu.EditMarkers.SelectedSymmetryMarkers.Count; i++){
			HistoryNew.SelectedSymmetryMarkers[i] = EditMenu.EditMarkers.SelectedSymmetryMarkers[i].position;
		}

		HistoryNew.MarkersPosSelection = new Vector3[EditMenu.EditMarkers.Selected.Count];
		for(int i = 0; i < EditMenu.EditMarkers.Selected.Count; i++){
			HistoryNew.MarkersPosSelection[i] = Scenario.GetPosOfMarker(EditMenu.EditMarkers.Selected[i]);
		}

		HistoryNew.MirrorPos = new HistoryMarkersMove.MirrorMarkersPos[EditMenu.EditMarkers.SymmetrySelectionList.Length];
		for(int i = 0; i < EditMenu.EditMarkers.SymmetrySelectionList.Length; i++){
			HistoryNew.MirrorPos[i] = new HistoryMarkersMove.MirrorMarkersPos();
			HistoryNew.MirrorPos[i].MarkersPosSelection = new Vector3[EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count];
			for(int e = 0; e < EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count; e++){
				HistoryNew.MirrorPos[i].MarkersPosSelection[e] = Scenario.GetPosOfMarker(EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected[e]);
			}
		}

		CurrentStage = History.Count;
	}

	public void RegisterMarkerSelection(){
		Debug.Log("Register Marker Selection");
		GameObject NewHistoryStep = Instantiate (Prefabs.MarkersSelection) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToHistory (NewHistoryStep.GetComponent<HistoryObject> ());

		CurrentStage = ListId;
		HistoryMarkersSelection HistoryNew = NewHistoryStep.GetComponent<HistoryMarkersSelection>();

		Debug.LogWarning("Already in: " + EditMenu.EditMarkers.Selected.Count);
		HistoryNew.Selected = new List<EditingMarkers.WorkingElement>();
		for(int i = 0; i < EditMenu.EditMarkers.Selected.Count; i++){
			HistoryNew.Selected.Add(EditMenu.EditMarkers.Selected[i]);
		}
		HistoryNew.SymmetrySelectionList = EditMenu.EditMarkers.SymmetrySelectionList;

		CurrentStage = History.Count;
	}


	public void RegisterMarkerChange(){
		Debug.Log("Register Marker Selection");
		GameObject NewHistoryStep = Instantiate (Prefabs.MarkersChange) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		
		CurrentStage = ListId;
		HistoryMarkersChange HistoryNew = NewHistoryStep.GetComponent<HistoryMarkersChange>();
		
		Debug.LogWarning("Already in: " + EditMenu.EditMarkers.Selected.Count);
		HistoryNew.Selected = new List<EditingMarkers.WorkingElement>();
		for(int i = 0; i < EditMenu.EditMarkers.Selected.Count; i++){
			HistoryNew.Selected.Add(EditMenu.EditMarkers.Selected[i]);
		}
		HistoryNew.SymmetrySelectionList = EditMenu.EditMarkers.SymmetrySelectionList;

		HistoryNew.ARMY_ = Scenario.ARMY_;
		HistoryNew.Mexes = Scenario.Mexes;
		HistoryNew.Hydros = Scenario.Hydros;
		HistoryNew.SiMarkers = Scenario.SiMarkers;
		
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
		
		HistoryNew.SelectedMarker = EditMenu.EditMarkers.SelectedMarker.position;
		HistoryNew.SelectedSymmetryMarkers = new Vector3[EditMenu.EditMarkers.SelectedSymmetryMarkers.Count];
		for(int i = 0; i < EditMenu.EditMarkers.SelectedSymmetryMarkers.Count; i++){
			HistoryNew.SelectedSymmetryMarkers[i] = EditMenu.EditMarkers.SelectedSymmetryMarkers[i].position;
		}
		
		HistoryNew.MarkersPosSelection = new Vector3[EditMenu.EditMarkers.Selected.Count];
		for(int i = 0; i < EditMenu.EditMarkers.Selected.Count; i++){
			HistoryNew.MarkersPosSelection[i] = Scenario.GetPosOfMarker(EditMenu.EditMarkers.Selected[i]);
		}
		
		HistoryNew.MirrorPos = new HistoryMarkersMove.MirrorMarkersPos[EditMenu.EditMarkers.SymmetrySelectionList.Length];
		for(int i = 0; i < EditMenu.EditMarkers.SymmetrySelectionList.Length; i++){
			HistoryNew.MirrorPos[i] = new HistoryMarkersMove.MirrorMarkersPos();
			HistoryNew.MirrorPos[i].MarkersPosSelection = new Vector3[EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count];
			for(int e = 0; e < EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count; e++){
				HistoryNew.MirrorPos[i].MarkersPosSelection[e] = Scenario.GetPosOfMarker(EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected[e]);
			}
		}
	}

	public void RegisterRedoMarkerSelection(){
		GameObject NewHistoryStep = Instantiate (Prefabs.MarkersSelection) as GameObject;
		NewHistoryStep.transform.parent = transform;
		int ListId = AddToRedoHistory (NewHistoryStep.GetComponent<HistoryObject> ());
		CurrentStage = ListId;
		HistoryMarkersSelection HistoryNew = NewHistoryStep.GetComponent<HistoryMarkersSelection>();
		
		HistoryNew.Selected = EditMenu.EditMarkers.Selected;
		HistoryNew.SymmetrySelectionList = EditMenu.EditMarkers.SymmetrySelectionList;
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