using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using OzoneDecals;

namespace UndoHistory
{
	public class HistoryDecalsChange : HistoryObject
	{

		public Decal[] Decals;

		public override void Register(HistoryParameter Param)
		{
			int count = DecalsControler.AllDecals.Count;
			Decals = new Decal[count];
			DecalsControler.AllDecals.CopyTo(Decals);
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryDecalsChange());
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			bool CleanSelection = DecalsControler.AllDecals.Count != Decals.Length || DecalsInfo.Current.DecalSettingsUi.IsCreating || !DecalsInfo.Current.gameObject.activeInHierarchy;

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
}