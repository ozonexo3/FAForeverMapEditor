using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class HistoryMarkersSelection : HistoryObject
{

	public List<MarkersInfo.WorkingElement> Selected = new List<MarkersInfo.WorkingElement>();
	public MarkersInfo.SymmetrySelection[] SymmetrySelectionList = new MarkersInfo.SymmetrySelection[0];


	public override void Register()
	{
		Selected = new List<MarkersInfo.WorkingElement>();
		for (int i = 0; i < Undo.Current.EditMenu.EditMarkers.Selected.Count; i++)
		{
			Selected.Add(Undo.Current.EditMenu.EditMarkers.Selected[i]);
		}
		SymmetrySelectionList = Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList;
	}


	public override void DoUndo()
	{
		//Undo.Current.RegisterRedoMarkerSelection();
		HistoryMarkersSelection.GenerateRedo(Undo.Current.Prefabs.MarkersSelection).Register();
		DoRedo();
	}

	public override void DoRedo()
	{

		if (Undo.Current.EditMenu.State != Editing.EditStates.MarkersStat)
		{
			Undo.Current.EditMenu.State = Editing.EditStates.MarkersStat;
			Undo.Current.EditMenu.ChangeCategory(4);
		}

		Undo.Current.EditMenu.EditMarkers.Selected = Selected;
		Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList = SymmetrySelectionList;
		Undo.Current.EditMenu.EditMarkers.UpdateSelectionRing();

	}
}
