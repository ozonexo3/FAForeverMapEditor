using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using OzoneDecals;

public class HistoryDecalsValues : HistoryObject
{
	public float[] CutOffLOD;
	public float[] NearCutOffLOD;

	public static bool UndoMenu;
	public bool UndoToDecalsMenu;

	public override void Register()
	{
		UndoToDecalsMenu = UndoMenu;

		CutOffLOD = new float[DecalsControler.Current.AllDecals.Count];
		NearCutOffLOD = new float[CutOffLOD.Length];

		for (int i = 0; i < CutOffLOD.Length; i++)
		{
			CutOffLOD[i] = DecalsControler.Current.AllDecals[i].Obj.CutOffLOD;
			NearCutOffLOD[i] = DecalsControler.Current.AllDecals[i].Obj.NearCutOffLOD;
		}
	}

	public override void DoUndo()
	{
		UndoMenu = UndoToDecalsMenu;

		if (!RedoGenerated)
			HistoryDecalsValues.GenerateRedo(Undo.Current.Prefabs.DecalValues).Register();
		RedoGenerated = true;
		DoRedo();
	}

	public override void DoRedo()
	{
		for (int i = 0; i < CutOffLOD.Length; i++)
		{
			DecalsControler.Current.AllDecals[i].Obj.CutOffLOD = CutOffLOD[i];
			DecalsControler.Current.AllDecals[i].Obj.NearCutOffLOD = NearCutOffLOD[i];
		}

		Undo.Current.EditMenu.ChangeCategory(5);

		DecalsInfo.Current.GoToSelection();
		DecalsInfo.Current.DecalSettingsUi.Load(DecalSettings.GetLoaded);
		Selection.SelectionManager.Current.FinishSelectionChange();
	}
}
