using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class HistoryPropsChange : HistoryObject
{

	HashSet<PropsInfo.PropTypeGroup> Groups;

	public override void Register()
	{
		Groups = new HashSet<PropsInfo.PropTypeGroup>();


	}


	public override void DoUndo()
	{
		if (!RedoGenerated)
			HistoryPropsChange.GenerateRedo(Undo.Current.Prefabs.PropsChange).Register();
		RedoGenerated = true;
		DoRedo();
	}

	public override void DoRedo()
	{
		Selection.SelectionManager.Current.CleanSelection();



		Undo.Current.EditMenu.ChangeCategory(6);
	}
}
