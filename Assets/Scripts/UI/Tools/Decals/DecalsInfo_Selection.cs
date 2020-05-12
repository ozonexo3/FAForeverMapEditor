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
			SelectionManager.Current.SetCopyActionAction(CopyAction);
			SelectionManager.Current.SetPasteActionAction(PasteAction);

			GoToSelection();
		}

		private void OnDisable()
		{
			if (DecalSettingsUi.IsCreating)
				DecalSettingsUi.OnClickCreate(false);

			PlacementManager.OnDropOnGameplay -= DropAtGameplay;
			SelectionManager.Current.ClearAffectedGameObjects();
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

		struct CopyDecalData
		{
			public Decal.DecalSharedSettings Shared;
			public Vector3 Position;
			public Quaternion Rotation;
			public Vector3 Scale;
			public float CutOffLOD;
			public float NearCutOffLOD;
			public int OwnerArmy;

			public CopyDecalData(Decal.DecalSharedSettings Shared, Vector3 Position, Quaternion Rotation, Vector3 Scale, float CutOffLOD, float NearCutOffLOD, int OwnerArmy)
				=> (this.Shared, this.Position, this.Rotation, this.Scale, this.CutOffLOD, this.NearCutOffLOD, this.OwnerArmy) = (Shared, Position, Rotation, Scale, CutOffLOD, NearCutOffLOD, OwnerArmy);
		}

		Vector3 CopyCenterPoint;
		List<CopyDecalData> CopyData;

		public void CopyAction()
		{

			CopyData = new List<CopyDecalData>();

			int count = DecalsControler.AllDecals.Count;
			List<GameObject> Objs = SelectionManager.GetAllSelectedGameobjects(false);

			Debug.Log("Copied " + Objs.Count + " decal");


			int selectionCount = Objs.Count;
			for (int i = 0; i < count; i++)
			{
				for(int s = 0; s < selectionCount; s++)
				{
					if(Objs[s] == DecalsControler.AllDecals[i].Obj.gameObject)
					{
						CopyData.Add(
							new CopyDecalData(DecalsControler.AllDecals[i].Shared,
							DecalsControler.AllDecals[i].Obj.tr.localPosition,
							DecalsControler.AllDecals[i].Obj.tr.localRotation,
							DecalsControler.AllDecals[i].Obj.tr.localScale,
							DecalsControler.AllDecals[i].CutOffLOD,
							DecalsControler.AllDecals[i].NearCutOffLOD,
							DecalsControler.AllDecals[i].OwnerArmy)
							);
						CopyCenterPoint += DecalsControler.AllDecals[i].Obj.tr.localPosition;
						break;
					}
				}
			}

			if(CopyData.Count > 0)
			{
				CopyCenterPoint /= CopyData.Count;
			}

			DecalsControler.Sort();
		}
		bool isPasteAction = false;
		float paste_CutOffLOD;
		float paste_NearCutOffLOD;
		int paste_OwnerArmy;
		List<GameObject> PastedObjects = new List<GameObject>(128);

		public void PasteAction()
		{
			int PasteCount = CopyData.Count;
			isPasteAction = true;
			if (PasteCount > 0)
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());

			PastedObjects.Clear();

			//GoToSelection();

			Vector3 PlaceOffset = new Vector3(0.5f, 0f, -0.5f);

			Decal.DecalSharedSettings storePrevousSettings = PlaceSharedSettings;

			PlacementManager.BeginPlacement(DecalSettingsUi.CreationPrefab, Place);
			for (int i = 0; i < PasteCount; i++)
			{
				if (CopyData[i].Shared == null)
					continue;

				PlaceSharedSettings = CopyData[i].Shared;
				paste_CutOffLOD = CopyData[i].CutOffLOD;
				paste_NearCutOffLOD = CopyData[i].NearCutOffLOD;
				paste_OwnerArmy = CopyData[i].OwnerArmy;

				PlacementManager.PlaceAtPosition(CopyData[i].Position + PlaceOffset, CopyData[i].Rotation, CopyData[i].Scale);
			}
			PlacementManager.Clear();

			PlaceSharedSettings = storePrevousSettings;
			DecalsControler.Sort();
			GoToSelection();
			SelectionManager.Current.CleanSelection();
			for (int i = 0; i < PastedObjects.Count; i++)
			{
				SelectionManager.Current.SelectObjectAdd(PastedObjects[i]);
				DecalsControler.MoveTop(PastedObjects[i].GetComponent<OzoneDecal>().Dec);
			}

			Debug.Log("Pasted " + PastedObjects.Count + " decals");

			UpdateTotalCount();

			//DecalsControler.Sort();
			isPasteAction = false;
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

		public static Decal.DecalSharedSettings PlaceSharedSettings;
		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales)
		{
			if (Positions.Length > 0 && !isPasteAction)
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());

			for (int i = 0; i < Positions.Length; i++)
			{

				GameObject NewDecalObject = Instantiate(DecalPrefab, DecalPivot);
				OzoneDecal Obj = NewDecalObject.GetComponent<OzoneDecal>();
				Decal component = new Decal();
				component.Obj = Obj;
				component.Shared = PlaceSharedSettings;
				Obj.Dec = component;
				Obj.tr = NewDecalObject.transform;

				Obj.tr.localPosition = Positions[i];
				Obj.tr.localRotation = Rotations[i];
				Obj.tr.localScale = Scales[i];

				if (isPasteAction)
				{
					Obj.CutOffLOD = paste_CutOffLOD;
					Obj.NearCutOffLOD = paste_NearCutOffLOD;
					Obj.OwnerArmy = paste_OwnerArmy;
				}
				else
				{
					Obj.CutOffLOD = DecalSettingsUi.CutOff.value;
					Obj.NearCutOffLOD = DecalSettingsUi.NearCutOff.value;
				}

				Obj.Material = component.Shared.SharedMaterial;
				Obj.CreationObject = false;
				Obj.UpdateMatrix();
				Obj.Bake();
				Obj.Index = Obj.tr.GetSiblingIndex();

				if (isPasteAction && i == 0)
				{
					PastedObjects.Add(NewDecalObject);
				}

				//DecalsControler.AddDecal(Obj.Dec);
			}
			UpdateTotalCount();
		}



	}
}