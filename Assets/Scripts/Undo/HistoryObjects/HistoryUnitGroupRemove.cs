using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditMap;
using Markers;

namespace UndoHistory
{
	public class HistoryUnitGroupRemove : HistoryObject
	{

		private UnitGroupRemoveHistoryParameter parameter;
		public class UnitGroupRemoveHistoryParameter : HistoryParameter
		{
			public MapLua.SaveLua.Army.UnitsGroup RegisterGroup;

			public UnitGroupRemoveHistoryParameter(MapLua.SaveLua.Army.UnitsGroup RegisterGroup)
			{
				this.RegisterGroup = RegisterGroup;
			}
		}

		public MapLua.SaveLua.Army.UnitsGroup RegisteredGroup;
		public MapLua.SaveLua.Army.UnitsGroup[] AllGroups;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as UnitGroupRemoveHistoryParameter);
			RegisteredGroup = parameter.RegisterGroup;
			AllGroups = new MapLua.SaveLua.Army.UnitsGroup[RegisteredGroup.UnitGroups.Count];
			RegisteredGroup.UnitGroups.CopyTo(AllGroups);
		}

		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				Undo.RegisterRedo(new HistoryUnitGroupRemove(), new UnitGroupRemoveHistoryParameter(RegisteredGroup));
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
				AllGroups[i].ClearUnitInstances();
			}

			RegisteredGroup.UnitGroups.Clear();

			for (int i = 0; i < AllGroups.Length; i++)
			{
				RegisteredGroup.AddGroup(AllGroups[i]);
				AllGroups[i].InstantiateGroup(true);
			}


			Undo.Current.EditMenu.ChangeCategory(7);
			MapLuaParser.Current.UnitsMenu.ChangePage(0);
			MapLuaParser.Current.UnitsMenu.Generate();

		}


	}
}