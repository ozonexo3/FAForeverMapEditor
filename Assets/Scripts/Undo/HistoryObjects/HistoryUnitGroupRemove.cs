using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Markers;

public class HistoryUnitGroupRemove : HistoryObject
{

	public static MapLua.SaveLua.Army.UnitsGroup RegisterGroup;


	public MapLua.SaveLua.Army.UnitsGroup RegisteredGroup;
	public MapLua.SaveLua.Army.UnitsGroup[] AllGroups;

	public override void Register()
	{
		RegisteredGroup = RegisterGroup;
		AllGroups = new MapLua.SaveLua.Army.UnitsGroup[RegisteredGroup.UnitGroups.Count];
		RegisteredGroup.UnitGroups.CopyTo(AllGroups);
	}

	public override void DoUndo()
	{
		if (!RedoGenerated)
		{
			RegisterGroup = RegisteredGroup;
			HistoryUnitGroupRemove.GenerateRedo(Undo.Current.Prefabs.UnitGroupRemove).Register();
		}
		RedoGenerated = true;
		DoRedo();
	}



	public override void DoRedo()
	{

		MapLua.SaveLua.Army.UnitsGroup[] RemoveOld = new MapLua.SaveLua.Army.UnitsGroup[RegisteredGroup.UnitGroups.Count];
		RegisteredGroup.UnitGroups.CopyTo(RemoveOld);

		for (int i = 0; i < RemoveOld.Length; i++)
		{
			RegisteredGroup.RemoveGroup(AllGroups[i]);
		}

		RegisteredGroup.UnitGroups.Clear();

		for (int i = 0; i < AllGroups.Length; i++)
		{
			//RegisteredGroup.UnitGroups.Add(AllGroups[i]);
			RegisteredGroup.AddGroup(AllGroups[i]);
		}


		Undo.Current.EditMenu.ChangeCategory(7);
		MapLuaParser.Current.UnitsMenu.ChangePage(0);
		MapLuaParser.Current.UnitsMenu.Generate();
		//NewMarkersInfo.Current.ClearCreateNew();
		//MarkersInfo.Current.ChangePage(0);

		//NewMarkersInfo.Current.GoToSelection();

	}


}
