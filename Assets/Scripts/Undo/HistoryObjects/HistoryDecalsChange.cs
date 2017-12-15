using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using OzoneDecals;

public class HistoryDecalsChange : HistoryObject
{

	public Decal[] Decals;

	public override void Register()
	{
		int count = DecalsControler.Current.AllDecals.Count;
		Decals = new Decal[count];
		DecalsControler.Current.AllDecals.CopyTo(Decals);
	}


	public override void DoUndo()
	{
		if (!RedoGenerated)
			HistoryDecalsChange.GenerateRedo(Undo.Current.Prefabs.DecalsChange).Register();
		RedoGenerated = true;
		DoRedo();
	}

	public override void DoRedo()
	{
		bool CleanSelection = DecalsControler.Current.AllDecals.Count != Decals.Length || DecalsInfo.Current.DecalSettingsUi.IsCreating || !DecalsInfo.Current.gameObject.activeInHierarchy;

		if (CleanSelection)
			Selection.SelectionManager.Current.CleanSelection();

		DecalsControler.ChangeDecalsList(Decals.ToList<Decal>());

		if (CleanSelection)
		{
			Undo.Current.EditMenu.ChangeCategory(5);
			DecalsInfo.Current.GoToSelection();
		}
		//Selection.SelectionManager.Current.FinishSelectionChange();

	}
}
