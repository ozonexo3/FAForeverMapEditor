using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using Selection;

namespace UndoHistory
{
	public class HistoryPropsMove : HistoryObject
	{
		private PropsMoveHistoryParameter parameter;
		public class PropsMoveHistoryParameter : HistoryParameter
		{
			public bool UndoMenu;

			public PropsMoveHistoryParameter(bool UndoMenu)
			{
				this.UndoMenu = UndoMenu;
			}
		}

		// MarkersPos
		GameObject[] Objs;
		Vector3[] PosSelection;
		Quaternion[] RotSelection;
		public bool UndoToMarkerMenu;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as PropsMoveHistoryParameter);
			UndoToMarkerMenu = parameter.UndoMenu;

			Objs = SelectionManager.Current.GetAllSelectedObjects();
			PosSelection = new Vector3[Objs.Length];
			RotSelection = new Quaternion[Objs.Length];


			for(int i = 0; i < Objs.Length; i++)
			{
				if (Objs[i])
				{
					PosSelection[i] = Objs[i].transform.localPosition;
					RotSelection[i] = Objs[i].transform.localRotation;
				}
			}

		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryPropsMove(), new PropsMoveHistoryParameter(UndoToMarkerMenu));
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{

			for (int i = 0; i < Objs.Length; i++)
			{
				if (Objs[i])
				{
					Objs[i].transform.localPosition = PosSelection[i];
					Objs[i].transform.localRotation = RotSelection[i];
				}
			}

			if (UndoToMarkerMenu)
			{
				Undo.Current.EditMenu.ChangeCategory(6);
				PropsInfo.Current.ShowTab(0);
			}

		}
	}
}