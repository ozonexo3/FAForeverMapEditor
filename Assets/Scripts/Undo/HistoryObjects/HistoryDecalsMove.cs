using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryDecalsMove : HistoryObject
	{
		private DecalsMoveHistoryParameter parameter;
		public class DecalsMoveHistoryParameter : HistoryParameter
		{
			public bool UndoMenu;

			public DecalsMoveHistoryParameter(bool UndoMenu)
			{
				this.UndoMenu = UndoMenu;
			}
		}


		Vector3[] Pos;
		Quaternion[] Rot;
		Vector3[] Scale;

		public bool UndoToDecalsMenu;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as DecalsMoveHistoryParameter);
			UndoToDecalsMenu = parameter.UndoMenu;

			Pos = new Vector3[DecalsControler.Current.AllDecals.Count];
			Rot = new Quaternion[Pos.Length];
			Scale = new Vector3[Pos.Length];

			for (int i = 0; i < Pos.Length; i++)
			{
				Transform tr = DecalsControler.Current.AllDecals[i].Obj.tr;
				Pos[i] = tr.localPosition;
				Rot[i] = tr.localRotation;
				Scale[i] = tr.localScale;
			}
		}

		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryDecalsMove(), new DecalsMoveHistoryParameter(UndoToDecalsMenu));
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			for (int i = 0; i < Pos.Length; i++)
			{
				Transform tr = DecalsControler.Current.AllDecals[i].Obj.tr;
				tr.localPosition = Pos[i];
				tr.localRotation = Rot[i];
				tr.localScale = Scale[i];
			}

			if (UndoToDecalsMenu)
			{
				Undo.Current.EditMenu.ChangeCategory(5);

				DecalsInfo.Current.GoToSelection();
				Selection.SelectionManager.Current.FinishSelectionChange();

			}
		}
	}
}