using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Markers;
using MapLua;

public class HistoryUnitsRemove : HistoryObject
{

	public static MapLua.SaveLua.Army.UnitsGroup[] RegisterGroups;

	public GroupUnits[] GroupsUnits;

	[System.Serializable]
	public class GroupUnits
	{
		public MapLua.SaveLua.Army.UnitsGroup Parent;
		public HashSet<MapLua.SaveLua.Army.Unit> Units;
	}

	public override void Register()
	{
		GroupsUnits = new GroupUnits[RegisterGroups.Length];
		for (int i = 0; i < GroupsUnits.Length; i++)
		{
			GroupsUnits[i] = new GroupUnits();
			GroupsUnits[i].Parent = RegisterGroups[i];
			GroupsUnits[i].Units = new HashSet<SaveLua.Army.Unit>();
			foreach (SaveLua.Army.Unit u in GroupsUnits[i].Parent.Units)
				GroupsUnits[i].Units.Add(u);
		}
	}

	public override void DoUndo()
	{
		if (!RedoGenerated)
		{
			RegisterGroups = new MapLua.SaveLua.Army.UnitsGroup[GroupsUnits.Length];
			for (int i = 0; i < GroupsUnits.Length; i++)
				RegisterGroups[i] = GroupsUnits[i].Parent;

			HistoryUnitsRemove.GenerateRedo(Undo.Current.Prefabs.UnitRemove).Register();
		}
		RedoGenerated = true;
		DoRedo();
	}



	public override void DoRedo()
	{

		for (int i = 0; i < GroupsUnits.Length; i++)
		{

			foreach (SaveLua.Army.Unit u in GroupsUnits[i].Units)
			{
				if (GroupsUnits[i].Parent.Units.Contains(u))
					continue;

				// Add

				GroupsUnits[i].Parent.AddUnit(u);
				u.Instantiate();
			}

			List<SaveLua.Army.Unit> ToRemove = new List<SaveLua.Army.Unit>();
			foreach (SaveLua.Army.Unit u in GroupsUnits[i].Parent.Units)
			{
				if (GroupsUnits[i].Units.Contains(u))
					continue;

				ToRemove.Add(u);
			}

			foreach (SaveLua.Army.Unit u in ToRemove)
			{
				GroupsUnits[i].Parent.RemoveUnit(u);
				u.ClearInstance();
			}


			Undo.Current.EditMenu.ChangeCategory(7);
			MapLuaParser.Current.UnitsMenu.ChangePage(1);
			MapLuaParser.Current.UnitsMenu.Generate();

			UnitsInfo.Current.GoToSelection();

		}

	}


}
