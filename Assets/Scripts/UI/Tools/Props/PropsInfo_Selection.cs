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
			if (Positions.Length > 0 && RegisterUndo)
			{
				//TODO Register Undo
			}

			for (int i = 0; i < Positions.Length; i++)
			{
				//TODO Paint props

			}
		}


		public void DestroyUnits(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			int Count = MarkerObjects.Count;

			//Unpaint props


			SelectionManager.Current.CleanSelection();
			GoToSelection();
		}

		public void SelectUnit()
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
		}
	}
}
