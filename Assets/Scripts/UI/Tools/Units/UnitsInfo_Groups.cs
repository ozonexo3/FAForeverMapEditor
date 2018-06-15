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
				ulo.UpdateSelection(SelectedGroups.Contains(ulo), ulo == FirstSelected);
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

		public void Generate(bool HoldSelection = true)
		{
			if (HoldSelection)
			{
				StoreSelection.Clear();
				StoreSelection = GetAllSelectedGroups();
			}

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
			ulo.RenameAction = RenameGroup;
			ulo.PrefixAction = PrefixChangeGroup;
			ulo.SetGroup(Army, Grp, Parent, Root);
			UnitGroups.Add(ulo);

			if (StoreSelection.Contains(Grp))
			{
				AddToGrpSelection(ulo);
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
		const string DefaultGroupName = "GROUP";

		public void AddNewGroup(UnitListObject parent)
		{
			Undo.RegisterGroupRemove(parent.Source);

			ClearGrpSelection();
			UpdateGroupSelection();

			string NamePrefix = DefaultGroupName;
			int NameCount = 0;
			bool found = false;

			if (parent.IsRoot && parent.Source.UnitGroups.Count == 0)
			{
				NamePrefix = "INITIAL";
			}
			else if (parent.Source.UnitGroups.Count > 0)
			{
				HashSet<string> AllNames = new HashSet<string>();
				foreach (MapLua.SaveLua.Army.UnitsGroup ug in parent.Source.UnitGroups)
					AllNames.Add(ug.Name);

				while (!found)
				{
					if (!AllNames.Contains(NamePrefix + "_" + NameCount.ToString("00")))
					{
						found = true;
						break;
					}
					else
						NameCount++;
				}
				NamePrefix = DefaultGroupName + "_" + NameCount.ToString("00");

			}
			else
			{
				NamePrefix = DefaultGroupName + "_00";

			}


			MapLua.SaveLua.Army.UnitsGroup NewGroup = new MapLua.SaveLua.Army.UnitsGroup();
			NewGroup.Name = NamePrefix;

			parent.Source.UnitGroups.Add(NewGroup);

			StoreSelection.Clear();
			StoreSelection.Add(NewGroup);


			Generate(false);
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

			Undo.RegisterGroupRemove(parent.Parent);

			StoreSelection.Clear();
			StoreSelection = GetAllSelectedGroups();

			ClearGrpSelection();
			UpdateGroupSelection();


			parent.Source.ClearGroup(true);

			parent.Parent.UnitGroups.Remove(parent.Source);

			Generate(false);
		}

		public void SelectGroup(UnitListObject parent)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				AddToGrpSelection(parent);
			}
			else if (Input.GetKey(KeyCode.LeftControl))
			{
				if (SelectedGroups.Contains(parent))
					RemoveFromGrpSelection(parent);
				else
					AddToGrpSelection(parent);
			}
			else
			{
				ClearGrpSelection();
				AddToGrpSelection(parent);
			}

			UpdateGroupSelection();
		}

		public void RenameGroup(UnitListObject parent)
		{
			string NewValue = parent.NameInputField.text;

			if (parent.Source.Name == NewValue)
				return; // No changes

			if (parent.Source.UnitGroups.Count > 1)
			{
				HashSet<string> AllNames = new HashSet<string>();
				foreach (MapLua.SaveLua.Army.UnitsGroup ug in parent.Source.UnitGroups)
					AllNames.Add(ug.Name);

				if (AllNames.Contains(NewValue))
					return; // Already exist
			}

			// TODO Register undo: Group name
			Undo.RegisterGroupChange(parent.Source);

			parent.Source.NoPrefixName = NewValue;
			Generate(true);
		}

		public void PrefixChangeGroup(UnitListObject parent)
		{
			string OldPrefix = parent.Source.PrefixName;
			string NewValue = parent.PrefixInputField.text;

			//TODO Register undo: Army Groups prefix
			if(!string.IsNullOrEmpty(OldPrefix))
				ChangeAllPrefix(parent.Owner.Units, OldPrefix, NewValue);
		}

		public static void ChangeAllPrefix(MapLua.SaveLua.Army.UnitsGroup Source, string Old, string New)
		{
			if(Source.PrefixName == Old)
			{
				Source.PrefixName = New;
			}
			foreach(MapLua.SaveLua.Army.UnitsGroup ug in Source.UnitGroups)
			{
				ChangeAllPrefix(ug, Old, New);
			}
		}

		#endregion

		public void Reparrent()
		{

		}

		public void TransferUnits()
		{

		}

		#region Selection
		static UnitListObject FirstSelected
		{
			get
			{
				if (SelectedGroups.Count == 0)
					return null;
				return SelectedGroups[SelectedGroups.Count - 1];
			}
		}
		static List<UnitListObject> SelectedGroups = new List<UnitListObject>();
		static List<MapLua.SaveLua.Army.UnitsGroup> StoreSelection = new List<MapLua.SaveLua.Army.UnitsGroup>();

		static void AddToGrpSelection(UnitListObject ulo)
		{
			SelectedGroups.Add(ulo);
		}
		static void RemoveFromGrpSelection(UnitListObject ulo)
		{
			SelectedGroups.Remove(ulo);
		}
		static void ClearGrpSelection()
		{
			SelectedGroups.Clear();
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
