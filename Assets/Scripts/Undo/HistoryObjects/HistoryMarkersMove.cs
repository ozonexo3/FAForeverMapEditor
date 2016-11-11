using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class HistoryMarkersMove : HistoryObject {

	// MarkersPos
	Vector3[]							MarkersPosSelection; 
	MirrorMarkersPos[]					MirrorPos;

	// SelectionPos
	Vector3								SelectedMarker;
	Vector3[]							SelectedSymmetryMarkers;

	[System.Serializable]
	class MirrorMarkersPos{
		public		Vector3[]							MarkersPosSelection;
	}


	public override void Register(){
		SelectedMarker = Undo.Current.EditMenu.EditMarkers.SelectedMarker.position;
		SelectedSymmetryMarkers = new Vector3[Undo.Current.EditMenu.EditMarkers.SelectedSymmetryMarkers.Count];
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.SelectedSymmetryMarkers.Count; i++){
			SelectedSymmetryMarkers[i] = Undo.Current.EditMenu.EditMarkers.SelectedSymmetryMarkers[i].position;
		}

		MarkersPosSelection = new Vector3[Undo.Current.EditMenu.EditMarkers.Selected.Count];
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.Selected.Count; i++){
			MarkersPosSelection[i] = Undo.Current.Scenario.GetPosOfMarker(Undo.Current.EditMenu.EditMarkers.Selected[i]);
		}

		MirrorPos = new HistoryMarkersMove.MirrorMarkersPos[Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList.Length];
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList.Length; i++){
			MirrorPos[i] = new HistoryMarkersMove.MirrorMarkersPos();
			MirrorPos[i].MarkersPosSelection = new Vector3[Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count];
			for(int e = 0; e < Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count; e++){
				MirrorPos[i].MarkersPosSelection[e] = Undo.Current.Scenario.GetPosOfMarker(Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected[e]);
			}
		}
	}


	public override void DoUndo(){
		HistoryMarkersMove.GenerateRedo (Undo.Current.Prefabs.MarkersMove).Register();
		DoRedo ();
	}

	public override void DoRedo(){
		Undo.Current.EditMenu.EditMarkers.SelectedMarker.position = SelectedMarker;
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.SelectedSymmetryMarkers.Count; i++){
			Undo.Current.EditMenu.EditMarkers.SelectedSymmetryMarkers[i].position = SelectedSymmetryMarkers[i];
		}
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.Selected.Count; i++){
			Undo.Current.Scenario.SetPosOfMarker(Undo.Current.EditMenu.EditMarkers.Selected[i], MarkersPosSelection[i]);
		}
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList.Length; i++){
			for(int e = 0; e < Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected.Count; e++){
				Undo.Current.Scenario.SetPosOfMarker(Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList[i].MirrorSelected[e], MirrorPos[i].MarkersPosSelection[e]);
			}
		}
	}
}
