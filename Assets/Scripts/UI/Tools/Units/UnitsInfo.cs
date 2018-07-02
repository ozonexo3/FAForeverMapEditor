using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Selection;
using FAF.MapEditor;
using Ozone.UI;

namespace EditMap
{
	public partial class UnitsInfo : MonoBehaviour
	{
		public static UnitsInfo Current;

		[Header("Pages")]
		public GameObject[] PageSelected;
		public GameObject[] Page;

		[Header("UI")]
		public Transform Pivot;
		public GameObject GroupPrefab;
		public GameObject UnitPrefab;
		public RectTransform RenameField;
		public UiTextField NameInputField;
		public UiTextField PrefixInputField;

		[Header("Objects")]
		public GameObject UnitInstancePrefab;
		public Mesh NoUnitMesh;
		public Material NoUnitMaterial;

		private void OnEnable()
		{
			ForceExitCreate();
			PlacementManager.OnDropOnGameplay += DropAtGameplay;
			SelectionManager.Current.DisableLayer = 15;
			SelectionManager.Current.SetRemoveAction(DestroyUnits);
			SelectionManager.Current.SetSelectionChangeAction(SelectUnit);
			SelectionManager.Current.SetCustomSnapAction(SnapAction);

			GoToSelection();

			Generate();
		}

		private void OnDisable()
		{
			ClearRename();
			ForceExitCreate();
			PlacementManager.OnDropOnGameplay -= DropAtGameplay;
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

			if (Input.GetKeyDown(KeyCode.P) && FirstSelected != null && !CameraControler.IsInputFieldFocused())
			{
				// Parent all groups to selected
				ReparrentGroups();

				// Parent all units to selected
				ReparrentUnits();
			}
		}
		GameObject[] AllObjects;
		public void GoToSelection()
		{

			PlacementManager.Clear();


			int[] AllTypes;
			AllObjects = UnitInstance.GetAllUnitGo(out AllTypes);
			SelectionManager.Current.SetAffectedGameObjects(AllObjects, SelectionManager.SelectionControlTypes.Units);
			SelectionManager.Current.SetAffectedTypes(AllTypes);

			OnChangeFreeRotation();

			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

		}

		int CurrentPage = 0;
		public static bool TerrainPageChange = false;
		public void ChangePage(int PageId)
		{
			if (CurrentPage == PageId && Page[CurrentPage].activeSelf && PageSelected[CurrentPage].activeSelf)
				return;
			TerrainPageChange = true;

			ForceExitCreate();

			//PreviousPage = CurrentPage;
			CurrentPage = PageId;

			for (int i = 0; i < Page.Length; i++)
			{
				Page[i].SetActive(false);
				PageSelected[i].SetActive(false);
			}


			Page[CurrentPage].SetActive(true);
			PageSelected[CurrentPage].SetActive(true);
			TerrainPageChange = false;
		}

	}
}