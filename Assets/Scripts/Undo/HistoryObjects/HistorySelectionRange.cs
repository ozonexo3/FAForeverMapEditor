using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Selection;

public class HistorySelectionRange : HistoryObject
{

	public Editing.EditStates MenuState;
	public int CategoryId;

	public GameObject[] AffectedGameObjects;
	public bool AllowMove;
	public bool AllowUp;
	public bool AllowRotation;
	public bool AllowScale;

	public override void Register()
	{
		MenuState = Undo.Current.EditMenu.State;
		CategoryId = Undo.Current.EditMenu.GetCategoryId();

		AffectedGameObjects = Selection.SelectionManager.Current.AffectedGameObjects;
		AllowMove = Selection.SelectionManager.Current.AllowMove;
		AllowUp = Selection.SelectionManager.Current.AllowUp;
		AllowRotation = Selection.SelectionManager.Current.AllowRotation;
		AllowScale = Selection.SelectionManager.Current.AllowScale;
	}


	public override void DoUndo()
	{
		//Undo.Current.RegisterRedoMarkerSelection();
		HistoryMarkersSelection.GenerateRedo(Undo.Current.Prefabs.MarkersSelection).Register();
		DoRedo();
	}

	public override void DoRedo()
	{
		
		if (Undo.Current.EditMenu.State != MenuState)
		{
			Undo.Current.EditMenu.State = MenuState;
			Undo.Current.EditMenu.ChangeCategory(CategoryId);
		}

		Selection.SelectionManager.Current.SetAffectedGameObjects(AffectedGameObjects, AllowMove, AllowUp, AllowRotation, AllowScale);
		if(MenuState == Editing.EditStates.MarkersStat)
			NewMarkersInfo.Current.ClearCreateNew();

		//Selection.SelectionManager.Current.Selection.Ids = Ids.ToList<int>();
		//Selection.SelectionManager.Current.FinishSelectionChange();

	}
}
