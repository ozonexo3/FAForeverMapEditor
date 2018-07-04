using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;
using Selection;

namespace UndoHistory
{
	public class HistoryUnitsMove : HistoryObject
	{

		MapLua.SaveLua.Army.Unit[] Units;
		Vector3[] Positions;
		Quaternion[] Orientations;

		public override void Register(HistoryParameter Param)
		{
			List<GameObject> AllSelected = SelectionManager.GetAllSelectedGameobjects();

			int count = AllSelected.Count;
			Units = new MapLua.SaveLua.Army.Unit[count];
			Positions = new Vector3[count];
			Orientations = new Quaternion[count];

			for (int i = 0; i < count; i++)
			{
				UnitInstance ui = AllSelected[i].GetComponent<UnitInstance>();
				Units[i] = ui.Owner;
				Positions[i] = ui.transform.position;
				Orientations[i] = ui.transform.rotation;
			}
		}

		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryUnitsMove());
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			for (int i = 0; i < Units.Length; i++)
			{
				if (Units[i].Instance == null)
					Units[i].Instantiate();

				Units[i].Instance.transform.position = Positions[i];
				Units[i].Instance.transform.rotation = Orientations[i];
				Units[i].Instance.UpdateMatrixTranslated();
			}


			Undo.Current.EditMenu.ChangeCategory(7);
			MapLuaParser.Current.UnitsMenu.ChangePage(1);
			MapLuaParser.Current.UnitsMenu.Generate();

		}
	}
}