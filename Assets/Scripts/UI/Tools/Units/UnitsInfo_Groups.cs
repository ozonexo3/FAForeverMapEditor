using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Selection;
using FAF.MapEditor;

namespace EditMap
{
	public partial class UnitsInfo : MonoBehaviour
	{


		public void UpdateGroupSelection()
		{
			foreach (UnitListObject ulo in UnitGroups)
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
					if (ulo != null)
						Destroy(ulo.gameObject);
				}

				UnitGroups.Clear();
			}

			SelectedGroups.Clear();
		}

		HashSet<UnitListObject> UnitGroups = new HashSet<UnitListObject>();
		HashSet<UnitListObject> SelectedGroups = new HashSet<UnitListObject>();
		static List<MapLua.SaveLua.Army.UnitsGroup> StoreSelection;

		public void Generate()
		{
			StoreSelection = GetAllSelectedGroups();

			Clear();

			var ScenarioData = MapLuaParser.Current.ScenarioLuaFile.Data;

			for (int c = 0; c < ScenarioData.Configurations.Length; c++)
			{
				for (int t = 0; t < ScenarioData.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < ScenarioData.Configurations[c].Teams[t].Armys.Count; a++)
					{
						CreateGroup(ScenarioData.Configurations[c].Teams[t].Armys[a].Data, ScenarioData.Configurations[c].Teams[t].Armys[a].Data.Units, null, Pivot, true);
					}
				}
				for (int e = 0; e < ScenarioData.Configurations[c].ExtraArmys.Count; e++)
				{
					CreateGroup(ScenarioData.Configurations[c].ExtraArmys[e].Data, ScenarioData.Configurations[c].ExtraArmys[e].Data.Units, null, Pivot, true);
				}
			}

			UpdateGroupSelection();
		}

		public void GenerateGroups(MapLua.SaveLua.Army Army, MapLua.SaveLua.Army.UnitsGroup Grp, UnitListObject ParentGrp)
		{
			int GrpCount = Grp.UnitGroups.Count;
			if (GrpCount == 0)
				return;

			foreach (MapLua.SaveLua.Army.UnitsGroup iGrp in Grp.UnitGroups)
			{
				CreateGroup(Army, iGrp, ParentGrp.Source, ParentGrp.Pivot);
			}
		}


		public void CreateGroup(MapLua.SaveLua.Army Army, MapLua.SaveLua.Army.UnitsGroup Grp, MapLua.SaveLua.Army.UnitsGroup Parent, Transform Pivot, bool Root = false)
		{
			GameObject NewGroupObject = Instantiate(GroupPrefab, Pivot);
			UnitListObject ulo = NewGroupObject.GetComponent<UnitListObject>();
			ulo.AddAction = AddNewGroup;
			ulo.RemoveAction = RemoveGroup;
			ulo.SelectAction = SelectGroup;
			ulo.SetGroup(Army, Grp, Parent, Root);
			UnitGroups.Add(ulo);

			if (StoreSelection.Contains(Grp))
			{
				SelectedGroups.Add(ulo);
			}

			GenerateGroups(Army, Grp, ulo);
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
			if (Connected == null)
				tr.localPosition = ScmapEditor.SnapToTerrain(tr.localPosition);
			else
				tr.localPosition = Connected.GetComponent<UnitInstance>().GetSnapPosition(tr.localPosition);
		}


		#region Groups
		public void AddNewGroup(UnitListObject parent)
		{
			//TODO Register Undo

			SelectedGroups.Clear();
			UpdateGroupSelection();


		}

		void RemoveGroupYes()
		{
			RemoveGroup(LastRemoveObject, true);
		}
		static UnitListObject LastRemoveObject;
		public void RemoveGroup(UnitListObject parent)
		{
			RemoveGroup(parent, false);
		}

		public void RemoveGroup(UnitListObject parent, bool Forced = false)
		{
			if (parent == null || parent.IsRoot || parent.Parent == null)
			{
				return;
			}

			if (!Forced && (parent.Source.Units.Count > 0 || parent.Source.UnitGroups.Count > 0))
			{
				LastRemoveObject = parent;
				GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "Remove group", "Group " + parent.Source.Name + " is not empty!\nRemove it anyway?", "Yes", RemoveGroupYes, "No", null);
				return;
			}

			//TODO Register Undo

			StoreSelection = GetAllSelectedGroups();

			SelectedGroups.Clear();
			UpdateGroupSelection();


			parent.Source.ClearGroup(true);

			parent.Parent.UnitGroups.Remove(parent.Source);

			Generate();
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

		public List<MapLua.SaveLua.Army.UnitsGroup> GetAllSelectedGroups()
		{
			List<MapLua.SaveLua.Army.UnitsGroup> ToReturn = new List<MapLua.SaveLua.Army.UnitsGroup>(SelectedGroups.Count);
			foreach (UnitListObject ulo in SelectedGroups)
			{
				ToReturn.Add(ulo.Source);
			}

			return ToReturn;
		}

		#endregion

	}
}
