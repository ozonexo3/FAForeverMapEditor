using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Selection;
using FAF.MapEditor;

namespace EditMap
{
	public class UnitsInfo : MonoBehaviour
	{
		public static UnitsInfo Current;

		[Header("UI")]
		public Transform Pivot;

		[Header("Objects")]
		public GameObject UnitInstancePrefab;
		public Mesh NoUnitMesh;
		public Material NoUnitMaterial;

		private void OnEnable()
		{
			SelectionManager.Current.DisableLayer = 15;
			SelectionManager.Current.SetRemoveAction(DestroyUnits);
			SelectionManager.Current.SetSelectionChangeAction(SelectUnit);
			SelectionManager.Current.SetCustomSnapAction(SnapAction);

			GoToSelection();
		}

		private void OnDisable()
		{
			//PlacementManager.OnDropOnGameplay -= DropAtGameplay;
			Selection.SelectionManager.Current.ClearAffectedGameObjects();
		}

		public static void UnloadUnits()
		{
			UnitsControler.StopUnitsUpdate();

			GameObject[] AllUnits = new GameObject[UnitInstance.AllUnitInstances.Count];
			UnitInstance.AllUnitInstances.CopyTo(AllUnits);

			for(int i = 0; i < AllUnits.Length; i++)
			{
				Destroy(AllUnits[i]);
			}
		}

		private void Update()
		{
			if (UpdateSelectedMatrixes)
			{
				UpdateSelectedMatrixes = false;

				for(int i = 0; i < SelectionManager.Current.Selection.Ids.Count; i++)
				{
					AllObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<UnitInstance>().UpdateMatrixTranslated();
				}

				for(int s = 0; s < SelectionManager.Current.SymetrySelection.Length; s++)
				{
					for (int i = 0; i < SelectionManager.Current.SymetrySelection[s].Ids.Count; i++)
					{
						AllObjects[SelectionManager.Current.SymetrySelection[s].Ids[i]].GetComponent<UnitInstance>().UpdateMatrixTranslated();
					}


				}
			}
		}
		GameObject[] AllObjects;
		public void GoToSelection()
		{

			int[] AllTypes;
			AllObjects = UnitInstance.GetAllUnitGo(out AllTypes);
			SelectionManager.Current.SetAffectedGameObjects(AllObjects, SelectionManager.SelectionControlTypes.Units);
			SelectionManager.Current.SetAffectedTypes(AllTypes);


			PlacementManager.Clear();
			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

		}



		public void DestroyUnits(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			if (RegisterUndo && MarkerObjects.Count > 0)
				Undo.Current.RegisterDecalsRemove();

			int Count = MarkerObjects.Count;
			for (int i = 0; i < Count; i++)
			{
				DestroyImmediate(MarkerObjects[i]);
			}

			SelectionManager.Current.CleanSelection();
			GoToSelection();
		}

		public void SelectUnit()
		{
			// ToDo Reload Selection UI info

			//DecalsList.UpdateSelection();
		}

		bool UpdateSelectedMatrixes = false;
		public void SnapAction(Transform tr, GameObject Connected)
		{
			UpdateSelectedMatrixes = true;
			if(Connected == null)
				tr.localPosition = ScmapEditor.SnapToTerrain(tr.localPosition);
			else
				tr.localPosition = Connected.GetComponent<UnitInstance>().GetSnapPosition(tr.localPosition);
		}

	}
}