using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selection;
using FAF.MapEditor;
using MapLua;

namespace EditMap
{
	public partial class PropsInfo : MonoBehaviour
	{

		public void DropAtGameplay()
		{
			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Unit)
				return;

			Vector3 MouseWorldPos = CameraControler.BufforedGameplayCursorPos;
			PlacementManager.PlaceAtPosition(MouseWorldPos, PropObjectPrefab, Place);

			GoToSelection();
		}


		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales)
		{
			Place(Positions, Rotations, Scales, true);
		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales, bool RegisterUndo)
		{
			if (Positions.Length > 0 && RegisterUndo && !IsPasteAction && !UndoRegistered)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryPropsChange());
			}

			for (int i = 0; i < Positions.Length; i++)
			{
				if (!MapLuaParser.IsInArea(Positions[i]))
					continue;

				Positions[i].y = ScmapEditor.Current.Teren.SampleHeight(Positions[i]);

				Prop NewProp = new Prop();
				NewProp.GroupId = RandomPropGroup;
				NewProp.CreateObject(Positions[i], Rotations[i], Vector3.one);

				AllPropsTypes[RandomPropGroup].PropsInstances.Add(NewProp);

				if (IsPasteAction && i == 0)
				{
					PastedObjects.Add(NewProp.Obj.gameObject);
				}
			}
		}


		public void DestroyProps(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			//Unpaint props
			List<GameObject> Objs = SelectionManager.GetAllSelectedGameobjects(true);
			int count = Objs.Count;

			if (count > 0)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryPropsChange());

				for (int i = 0; i < count; i++)
				{
					PropGameObject TestObj = Objs[i].GetComponent<PropGameObject>();
					if (TestObj != null)
					{
						TestObj.Connected.Group.PropsInstances.Remove(TestObj.Connected);
						Destroy(TestObj.gameObject);

					}
				}

				ReloadPropStats();

				SelectionManager.Current.CleanSelection();
				GoToSelection();
			}
		}

		public void SelectProp()
		{
			if (SelectionManager.Current.Selection.Ids.Count <= 0)
			{

			}
			else
			{
				// Prop selected
			}
		}

		public void SnapAction(Transform tr, GameObject Connected)
		{
			//TODO Snap To Terrain
			Vector3 pos = tr.localPosition;
			pos.y = ScmapEditor.Current.Teren.SampleHeight(pos);
			tr.localPosition = pos;
		}

		struct CopyProp
		{
			public string type;
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 scale;
			public CopyProp(string type, Vector3 position, Quaternion rotation, Vector3 scale)
				=> (this.type, this.position, this.rotation, this.scale) = (type, position, rotation, scale);
		}

		List<CopyProp> CopyPropData = new List<CopyProp>(512);

		public void CopyAction()
		{

			CopyPropData.Clear();

			List<GameObject> Objs = SelectionManager.GetAllSelectedGameobjects(false);
			for(int i = 0; i < Objs.Count; i++)
			{
				PropGameObject TestObj = Objs[i].GetComponent<PropGameObject>();

				CopyPropData.Add(new CopyProp(
					TestObj.Connected.BlueprintPath,
					Objs[i].transform.localPosition,
					Objs[i].transform.localRotation,
					Objs[i].transform.localScale
					));
			}

			Debug.Log("Copied " + CopyPropData.Count + " props");

		}

		bool IsPasteAction = false;
		List<GameObject> PastedObjects = new List<GameObject>(128);
		public void PasteAction()
		{

			if (CopyPropData.Count > 0)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryPropsChange());

				UndoRegistered = true;
				IsPasteAction = true;

				Vector3 PlaceOffset = new Vector3(0.5f, 0f, -0.5f);
				PastedObjects.Clear();
				PlacementManager.BeginPlacement(PropObjectPrefab, Place);
				for (int p = 0; p < CopyPropData.Count; p++)
				{
					RandomPropGroup = GetPropType(CopyPropData[p].type);
					//PropGameObject SpawnedProp = Paint(CopyPropData[p].position + PlaceOffset, CopyPropData[p].rotation);
					PlacementManager.PlaceAtPosition(CopyPropData[p].position + PlaceOffset, CopyPropData[p].rotation, Vector3.one);
				}
				PlacementManager.Clear();

				IsPasteAction = false;
				UndoRegistered = false;
			}

			Debug.Log("Pasted " + PastedObjects.Count + " props");


			ReloadPropStats();

			GoToSelection();

			SelectionManager.Current.CleanSelection();
			for (int i = 0; i < PastedObjects.Count; i++)
			{
				SelectionManager.Current.SelectObjectAdd(PastedObjects[i]);
			}
		}

		List<CopyProp> DuplicatePropData = new List<CopyProp>(512);

		void DuplicateAction()
		{
			DuplicatePropData.Clear();

			List<GameObject> Objs = SelectionManager.GetAllSelectedGameobjects(false);
			for (int i = 0; i < Objs.Count; i++)
			{
				PropGameObject TestObj = Objs[i].GetComponent<PropGameObject>();

				DuplicatePropData.Add(new CopyProp(
					TestObj.Connected.BlueprintPath,
					Objs[i].transform.localPosition,
					Objs[i].transform.localRotation,
					Objs[i].transform.localScale
					));
			}

			if (DuplicatePropData.Count > 0)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryPropsChange());

				UndoRegistered = true;
				IsPasteAction = true;

				Vector3 PlaceOffset = new Vector3(0.5f, 0f, -0.5f);
				PastedObjects.Clear();
				PlacementManager.BeginPlacement(PropObjectPrefab, Place);
				for (int p = 0; p < DuplicatePropData.Count; p++)
				{
					RandomPropGroup = GetPropType(DuplicatePropData[p].type);
					//PropGameObject SpawnedProp = Paint(CopyPropData[p].position + PlaceOffset, CopyPropData[p].rotation);
					PlacementManager.PlaceAtPosition(DuplicatePropData[p].position + PlaceOffset, DuplicatePropData[p].rotation, Vector3.one);
				}
				PlacementManager.Clear();

				IsPasteAction = false;
				UndoRegistered = false;
			}

			Debug.Log("Pasted " + PastedObjects.Count + " props");


			ReloadPropStats();

			GoToSelection();

			SelectionManager.Current.CleanSelection();
			for (int i = 0; i < PastedObjects.Count; i++)
			{
				SelectionManager.Current.SelectObjectAdd(PastedObjects[i]);
			}
		}
	}
}
