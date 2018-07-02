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
			ClearRename();

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
			if (Root)
			{
				Grp.Expanded = true;
			}

			GameObject NewGroupObject = Instantiate(GroupPrefab, Pivot);
			UnitListObject ulo = NewGroupObject.GetComponent<UnitListObject>();
			ulo.AddAction = AddNewGroup;
			ulo.RemoveAction = RemoveGroup;
			ulo.SelectAction = SelectGroup;
			ulo.RenameAction = RenameStart;
			ulo.ExpandAction = ExpandAction;
			ulo.SetGroup(Army, Grp, Parent, Root);
			UnitGroups.Add(ulo);

			if (StoreSelection.Contains(Grp))
			{
				AddToGrpSelection(ulo);
			}

			GenerateGroups(Army, Grp, ulo);
		}




		#region Groups
		const string DefaultGroupName = "GROUP";

		public void AddNewGroup(UnitListObject parent)
		{
			ClearRename();

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


			MapLua.SaveLua.Army.UnitsGroup NewGroup = new MapLua.SaveLua.Army.UnitsGroup(parent.Source.Owner);
			NewGroup.Name = NamePrefix;

			//parent.Source.UnitGroups.Add(NewGroup);
			parent.Source.AddGroup(NewGroup);

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

			ClearRename();

			Undo.RegisterGroupRemove(parent.Parent);

			StoreSelection.Clear();
			StoreSelection = GetAllSelectedGroups();

			ClearGrpSelection();
			UpdateGroupSelection();


			parent.Source.ClearGroup(true);

			//parent.Parent.UnitGroups.Remove(parent.Source);
			parent.Parent.RemoveGroup(parent.Source);

			Generate(false);
		}

		public void SelectGroup(UnitListObject parent)
		{
			ClearRename();
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


		static UnitListObject RenameObject;
		public void RenameStart(UnitListObject parent)
		{
			ClearRename();

			RenameObject = parent;
			RenameField.SetParent(parent.transform, false);
			RenameField.gameObject.SetActive(true);
			RenameField.GetComponent<LayoutElement>().enabled = true;

			NameInputField.SetValue(parent.Source.NoPrefixName);
			PrefixInputField.SetValue(parent.Source.PrefixName);

			NameInputField.InputFieldUi.ActivateInputField();
		}

		bool RenameUndoApplyied = false;
		public void RenameApply(bool Apply)
		{
			if (Apply)
			{
				RenameGroup(RenameObject);
				PrefixChangeGroup(RenameObject);
			}
			RenameObject.RenameEnd();
			RenameField.SetParent(Pivot, false);
			RenameField.gameObject.SetActive(false);
			RenameUndoApplyied = false;
		}

		void ClearRename()
		{
			if (RenameField.gameObject.activeSelf)
			{
				if(RenameObject)
					RenameObject.RenameEnd();
				RenameField.SetParent(Pivot, false);
				RenameField.gameObject.SetActive(false);
				RenameUndoApplyied = false;
			}
		}

		public void RenameGroup(UnitListObject parent)
		{
			string NewValue = NameInputField.text;

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

			RenameUndoApplyied = true;
			Undo.RegisterGroupChange(parent.Source);

			parent.Source.NoPrefixName = NewValue;
			Generate(true);
		}

		public void PrefixChangeGroup(UnitListObject parent)
		{
			string OldPrefix = parent.Source.PrefixName;
			string NewValue = PrefixInputField.text;

			if (OldPrefix == NewValue)
				return; // No changes

			if(!RenameUndoApplyied)
				Undo.RegisterGroupChange(parent.Source);

			if (!string.IsNullOrEmpty(OldPrefix))
			{
				ChangeAllPrefix(parent.Owner.Units, OldPrefix, NewValue);
				foreach (UnitListObject ulo in UnitGroups)
					ulo.GroupName.text = ulo.Source.Name;
			}
			else
			{
				parent.Source.PrefixName = NewValue;
				parent.GroupName.text = parent.Source.Name;

			}

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

		public void ExpandAction(UnitListObject parent)
		{

		}

		void ReparrentGroups()
		{
			// TODO Register Undo

			foreach(UnitListObject sel in SelectedGroups)
			{
				if (sel == FirstSelected)
					continue;

				if (FirstSelected.Source == sel.Parent)
					continue;

				sel.transform.SetParent(FirstSelected.Pivot, false);
				ReparentGroup(sel.Source, FirstSelected.Source, sel.Parent);
			}
		}

		public static void ReparentGroup(MapLua.SaveLua.Army.UnitsGroup Source, MapLua.SaveLua.Army.UnitsGroup NewOwner, MapLua.SaveLua.Army.UnitsGroup OldOwner )
		{

			if(OldOwner == null)
			{

			}

			OldOwner.RemoveGroup(Source);
			NewOwner.AddGroup(Source);
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
