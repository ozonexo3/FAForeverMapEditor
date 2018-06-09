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
		public GameObject GroupPrefab;
		public GameObject UnitPrefab;

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

			Generate();
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

		public void UpdateGroupSelection()
		{
			foreach(UnitListObject ulo in UnitGroups)
			{
				ulo.UpdateSelection(SelectedGroups.Contains(ulo));
			}
		}

		public void Clear()
		{
			if (UnitGroups == null)
				UnitGroups = new HashSet<UnitListObject>();
			else
			{

				foreach (UnitListObject ulo in UnitGroups)
				{
					if(ulo != null)
					Destroy(ulo.gameObject);
				}

				UnitGroups.Clear();
			}

			SelectedGroups.Clear();
		}

		HashSet<UnitListObject> UnitGroups = new HashSet<UnitListObject>();
		HashSet<UnitListObject> SelectedGroups = new HashSet<UnitListObject>();
		public void Generate()
		{
			Clear();

			var ScenarioData = MapLuaParser.Current.ScenarioLuaFile.Data;

			for (int c = 0; c < ScenarioData.Configurations.Length; c++)
			{
				for(int t = 0; t < ScenarioData.Configurations[c].Teams.Length; t++)
				{
					for(int a = 0; a < ScenarioData.Configurations[c].Teams[t].Armys.Count; a++)
					{
						GameObject NewGroupObject = Instantiate(GroupPrefab, Pivot);
						UnitListObject ulo = NewGroupObject.GetComponent<UnitListObject>();
						ulo.AddAction = AddNewGroup;
						ulo.SetGroup(ScenarioData.Configurations[c].Teams[t].Armys[a].Data.Name, true);
						UnitGroups.Add(ulo);

						GenerateArmy(ScenarioData.Configurations[c].Teams[t].Armys[a], ulo);
					}
				}
				for(int e = 0; e < ScenarioData.Configurations[c].ExtraArmys.Count; e++)
				{
					GameObject NewGroupObject = Instantiate(GroupPrefab, Pivot);
					UnitListObject ulo = NewGroupObject.GetComponent<UnitListObject>();
					ulo.AddAction = AddNewGroup;
					ulo.SetGroup(ScenarioData.Configurations[c].ExtraArmys[e].Data.Name, true);
					UnitGroups.Add(ulo);
				}
			}
		}

		public void GenerateArmy(MapLua.ScenarioLua.Army Army, UnitListObject ArmyGroup)
		{

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


		#region Groups
		public void AddNewGroup(UnitListObject parent)
		{
			SelectedGroups.Clear();
			UpdateGroupSelection();
		}

		public void RemoveGroup(UnitListObject parent)
		{
			SelectedGroups.Clear();
			UpdateGroupSelection();
		}

		public void SelectGroup(UnitListObject parent)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				SelectedGroups.Add(parent);
			}
			else if (Input.GetKey(KeyCode.LeftControl))
			{
				if (SelectedGroups.Contains(parent))
					SelectedGroups.Remove(parent);
				else
					SelectedGroups.Add(parent);
			}
			else
			{
				SelectedGroups.Clear();
				SelectedGroups.Add(parent);
			}

			UpdateGroupSelection();
		}

		#endregion

	}
}