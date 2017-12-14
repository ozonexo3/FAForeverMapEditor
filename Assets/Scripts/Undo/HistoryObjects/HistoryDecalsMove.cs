using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class HistoryDecalsMove : HistoryObject
{

	Vector3[] Pos;
	Quaternion[] Rot;
	Vector3[] Scale;

	public static bool UndoMenu;
	public bool UndoToDecalsMenu;

	public override void Register()
	{
		UndoToDecalsMenu = UndoMenu;

		Pos = new Vector3[DecalsControler.Current.AllDecals.Count];
		Rot = new Quaternion[Pos.Length];
		Scale = new Vector3[Pos.Length];

		for(int i = 0; i < Pos.Length; i++)
		{
			Transform tr = DecalsControler.Current.AllDecals[i].Obj.tr;
			Pos[i] = tr.localPosition;
			Rot[i] = tr.localRotation;
			Scale[i] = tr.localScale;
		}
	}

	public override void DoUndo()
	{
		UndoMenu = UndoToDecalsMenu;

		if (!RedoGenerated)
			HistoryMarkersMove.GenerateRedo(Undo.Current.Prefabs.DecalsMove).Register();
		RedoGenerated = true;
		DoRedo();
	}

	public override void DoRedo()
	{
		for (int i = 0; i < Pos.Length; i++)
		{
			Transform tr = DecalsControler.Current.AllDecals[i].Obj.tr;
			tr.localPosition = Pos[i];
			tr.localRotation = Rot[i];
			tr.localScale = Scale[i];
		}

		if (UndoToDecalsMenu)
		{
			Undo.Current.EditMenu.ChangeCategory(5);
			//NewMarkersInfo.Current.ClearCreateNew();
			//MarkersInfo.Current.ChangePage(0);

			//NewMarkersInfo.Current.GoToSelection();
			DecalsInfo.Current.GoToSelection();
			Selection.SelectionManager.Current.FinishSelectionChange();

		}
	}
}
