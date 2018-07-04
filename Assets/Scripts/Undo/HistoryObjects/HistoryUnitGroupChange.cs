using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditMap;
using Markers;

namespace UndoHistory
{
	public class HistoryUnitGroupChange : HistoryObject
	{

		private UnitGroupChangeParam parameter;
		public class UnitGroupChangeParam : HistoryParameter
		{
			public MapLua.SaveLua.Army.UnitsGroup RegisterGroup;

			public UnitGroupChangeParam(MapLua.SaveLua.Army.UnitsGroup RegisterGroup)
			{
				this.RegisterGroup = RegisterGroup;
			}
		}

		public MapLua.SaveLua.Army.UnitsGroup RegisteredGroup;
		public string Name;
		public string Prefix;
		public string Orders;
		public string Platoons;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as UnitGroupChangeParam);
			RegisteredGroup = parameter.RegisterGroup;
			Name = RegisteredGroup.NoPrefixName;
			Prefix = RegisteredGroup.PrefixName;
			Orders = RegisteredGroup.orders;
			Platoons = RegisteredGroup.platoon;
		}

		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				Undo.RegisterRedo(new HistoryUnitGroupChange(), new UnitGroupChangeParam(RegisteredGroup));
			}
			RedoGenerated = true;
			DoRedo();
		}



		public override void DoRedo()
		{

			if (RegisteredGroup.NoPrefixName != Name)
			{
				RegisteredGroup.NoPrefixName = Name;


			}
			if (RegisteredGroup.PrefixName != Prefix)
			{
				RegisteredGroup.PrefixName = Prefix;

				UnitsInfo.ChangeAllPrefix(RegisteredGroup, RegisteredGroup.PrefixName, Prefix);
			}



			RegisteredGroup.orders = Orders;
			RegisteredGroup.platoon = Platoons;

			Undo.Current.EditMenu.ChangeCategory(7);
			MapLuaParser.Current.UnitsMenu.ChangePage(0);
			MapLuaParser.Current.UnitsMenu.Generate();
		}

	}
}