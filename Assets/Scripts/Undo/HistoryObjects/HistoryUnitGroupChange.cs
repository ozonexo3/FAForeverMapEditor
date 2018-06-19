using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Markers;

public class HistoryUnitGroupChange : HistoryObject
{

	public static MapLua.SaveLua.Army.UnitsGroup RegisterGroup;

	public MapLua.SaveLua.Army.UnitsGroup RegisteredGroup;
	public string Name;
	public string Prefix;
	public string Orders;
	public string Platoons;

	public override void Register()
	{
		RegisteredGroup = RegisterGroup;
		Name = RegisteredGroup.NoPrefixName;
		Prefix = RegisteredGroup.PrefixName;
		Orders = RegisteredGroup.orders;
		Platoons = RegisteredGroup.platoon;
	}

	public override void DoUndo()
	{
		if (!RedoGenerated)
		{
			RegisterGroup = RegisteredGroup;
			HistoryUnitGroupChange.GenerateRedo(Undo.Current.Prefabs.UnitGroupChange).Register();
		}
		RedoGenerated = true;
		DoRedo();
	}



	public override void DoRedo()
	{

		if(RegisteredGroup.NoPrefixName != Name)
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
		//NewMarkersInfo.Current.ClearCreateNew();
		//MarkersInfo.Current.ChangePage(0);

		//NewMarkersInfo.Current.GoToSelection();

	}


}
