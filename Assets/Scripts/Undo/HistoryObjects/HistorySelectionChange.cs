using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Selection;

public class HistorySelectionChange : HistoryObject
{

	public int[] Ids;

	public Editing.EditStates MenuState;
	public int CategoryId;

	public override void Register()
	{
		Ids = Selection.SelectionManager.Current.Selection.Ids.ToArray();
		MenuState = Undo.Current.EditMenu.State;
		CategoryId = Undo.Current.EditMenu.GetCategoryId();
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
		
		Selection.SelectionManager.Current.Selection.Ids = Ids.ToList<int>();
		Selection.SelectionManager.Current.FinishSelectionChange();

	}
}
