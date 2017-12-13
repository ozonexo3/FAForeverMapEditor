using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;
using OzoneDecals;

public class HistoryDecalsRemove : HistoryObject
{

	public static HashSet<OzoneDecal> AllSelected;

	Decal.DecalSharedSettings[] Shared;
	Vector3[] Positions;
	Quaternion[] Rotations;
	Vector3[] Scales;
	float[] CutOffLOD;
	float[] NearCutOffLOD;
	int[] Order;


	public override void Register()
	{
		int count = AllSelected.Count;

		Shared = new Decal.DecalSharedSettings[count];
		Positions = new Vector3[count];
		Rotations = new Quaternion[count];
		Scales = new Vector3[count];
		CutOffLOD = new float[count];
		NearCutOffLOD = new float[count];
		Order = new int[count];

		int i = 0;
		foreach(OzoneDecal dec in AllSelected)
		{
			Shared[i] = dec.Shared;
			Positions[i] = dec.tr.localPosition;
			Rotations[i] = dec.tr.localRotation;
			Scales[i] = dec.tr.localScale;
			CutOffLOD[i] = dec.CutOffLOD;
			NearCutOffLOD[i] = dec.NearCutOffLOD;
			Order[i] = dec.tr.GetSiblingIndex();
			i++;
		}
	}

	HashSet<GameObject> RedoRemoveObjects;

	public void RegisterRedo()
	{
		foreach (OzoneDecal dec in AllSelected)
		{
			RedoRemoveObjects.Add(dec.gameObject);
		}
		//RedoRemoveObjects
	}

	public override void DoUndo()
	{
		// Recreate objects
		AllSelected = new HashSet<OzoneDecal>();

		for(int i = 0; i < Shared.Length; i++)
		{
			GameObject NewDecalObject = Instantiate(DecalsInfo.Current.DecalPrefab, DecalsInfo.Current.DecalPivot);
			OzoneDecal Dec = NewDecalObject.GetComponent<OzoneDecal>();
			Dec.Shared = DecalSettings.GetLoaded;
			Dec.tr = NewDecalObject.transform;

			Dec.tr.localPosition = Positions[i];
			Dec.tr.localRotation = Rotations[i];
			Dec.tr.localScale = Scales[i];

			Dec.CutOffLOD = CutOffLOD[i];
			Dec.NearCutOffLOD = NearCutOffLOD[i];

			Dec.Material = Dec.Shared.SharedMaterial;
			DecalsControler.AddDecal(Dec, Order[i]);
		}

		if (!RedoGenerated)
			((HistoryDecalsRemove)HistoryDecalsRemove.GenerateRedo(Undo.Current.Prefabs.DecalsRemove)).RegisterRedo();
		RedoGenerated = true;
		//DoRedo();
	}

	public override void DoRedo()
	{
		// Remove


		Undo.Current.EditMenu.ChangeCategory(5);

		DecalsInfo.Current.GoToSelection();
		Selection.SelectionManager.Current.FinishSelectionChange();

	}
}
