using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OzoneDecals;
using Selection;

namespace EditMap
{
	public partial class DecalsInfo : MonoBehaviour
	{

		private void OnEnable()
		{
			PlacementManager.OnDropOnGameplay += DropAtGameplay;
			SelectionManager.Current.DisableLayer = 14;
			SelectionManager.Current.SetRemoveAction(DestroyDetails);
			SelectionManager.Current.SetSelectionChangeAction(SelectDetails);
			SelectionManager.Current.SetCustomSnapAction(OzoneDecal.SnapToGround);

			GoToSelection();
		}

		private void OnDisable()
		{
			if (DecalSettingsUi.IsCreating)
				DecalSettingsUi.OnClickCreate(false);

			PlacementManager.OnDropOnGameplay -= DropAtGameplay;
			Selection.SelectionManager.Current.ClearAffectedGameObjects();
		}

		private void Update()
		{

			HideUpdate();

		}

		public void GoToSelection()
		{
			/*
			if (!MarkersInfo.MarkerPageChange)
			{
				Selection.SelectionManager.Current.CleanSelection();
			}
			*/
			int[] AllTypes;
			SelectionManager.Current.SetAffectedGameObjects(DecalsControler.GetAllDecalsGo(out AllTypes), SelectionManager.SelectionControlTypes.Decal);
			SelectionManager.Current.SetAffectedTypes(AllTypes);
			//SelectionManager.Current.SetCustomSettings(true, true, true);


			PlacementManager.Clear();
			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

			UpdateTotalCount();
		}



		public void SelectDetails()
		{
			if (SelectionManager.Current.AffectedGameObjects.Length == 0 || SelectionManager.Current.Selection.Ids.Count == 0)
				DecalSettingsUi.Load(null);
			else
				DecalSettingsUi.Load(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].GetComponent<OzoneDecal>().Dec.Shared);

			DecalsList.UpdateSelection();
		}

		public void DestroyDetails(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			if (RegisterUndo && MarkerObjects.Count > 0)
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());

			int Count = MarkerObjects.Count;
			for (int i = 0; i < Count; i++)
			{
				DestroyImmediate(MarkerObjects[i]);
			}

			SelectionManager.Current.CleanSelection();
			GoToSelection();
			UpdateTotalCount();
		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales)
		{
			if (Positions.Length > 0)
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());

			for (int i = 0; i < Positions.Length; i++)
			{

				GameObject NewDecalObject = Instantiate(DecalPrefab, DecalPivot);
				OzoneDecal Obj = NewDecalObject.GetComponent<OzoneDecal>();
				Decal component = new Decal();
				component.Obj = Obj;
				Obj.Dec = component;
				Obj.Dec.Shared = DecalSettings.GetLoaded;
				Obj.tr = NewDecalObject.transform;

				Obj.tr.localPosition = Positions[i];
				Obj.tr.localRotation = Rotations[i];
				Obj.tr.localScale = Scales[i];

				Obj.CutOffLOD = DecalSettingsUi.CutOff.value;
				Obj.NearCutOffLOD = DecalSettingsUi.NearCutOff.value;

				Obj.Material = component.Shared.SharedMaterial;

				Obj.Bake();

				DecalsControler.AddDecal(Obj.Dec);
			}
			UpdateTotalCount();
		}



	}
}