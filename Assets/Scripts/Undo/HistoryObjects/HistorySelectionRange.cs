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
	public int MarkersPage = 0;

	public int[] Ids;
	public GameObject[] AffectedGameObjects;
	public int[] AffectedTypes;

	public static bool DoingRedo = false;

	public override void Register()
	{
		MenuState = Undo.Current.EditMenu.State;
		CategoryId = Undo.Current.EditMenu.GetCategoryId();

		if (MenuState == Editing.EditStates.MarkersStat)
			MarkersPage = MarkersInfo.Current.PreviousCurrentPage();


		Ids = SelectionManager.Current.Selection.Ids.ToArray();
		AffectedGameObjects = SelectionManager.Current.AffectedGameObjects;
		AffectedTypes = SelectionManager.Current.AffectedTypes;
	}


	public override void DoUndo()
	{
		//Undo.Current.RegisterRedoMarkerSelection();
		HistoryMarkersSelection.GenerateRedo(Undo.Current.Prefabs.MarkersSelection).Register();
		DoRedo();
	}

	public override void DoRedo()
	{
		DoingRedo = true;
		if (Undo.Current.EditMenu.State != MenuState)
		{
			Undo.Current.EditMenu.State = MenuState;
			Undo.Current.EditMenu.ChangeCategory(CategoryId);
		}

		SelectionManager.Current.SetAffectedGameObjects(AffectedGameObjects, SelectionManager.SelectionControlTypes.Last);
		SelectionManager.Current.SetAffectedTypes(AffectedTypes);
		SelectionManager.Current.Selection.Ids = Ids.ToList<int>();
		SelectionManager.Current.FinishSelectionChange();

		if (MenuState == Editing.EditStates.MarkersStat)
		{
			NewMarkersInfo.Current.ClearCreateNew();
			MarkersInfo.Current.ChangePage(MarkersPage);
		}

		//Selection.SelectionManager.Current.Selection.Ids = Ids.ToList<int>();
		//Selection.SelectionManager.Current.FinishSelectionChange();
		DoingRedo = false;
	}
}
