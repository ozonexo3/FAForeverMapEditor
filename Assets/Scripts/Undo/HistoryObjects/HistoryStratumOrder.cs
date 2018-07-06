using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;

namespace UndoHistory
{
	public class HistoryStratumReorder : HistoryObject
	{


		private StratumReorderParam parameter;
		public class StratumReorderParam : HistoryParameter
		{
			public int FromId;
			public int ToId;

			public StratumReorderParam(int FromId, int ToId)
			{
				this.FromId = FromId;
				this.ToId = ToId;
			}
		}


		public override void Register(HistoryParameter Param)
		{
			UndoCommandName = "Reorder layers";
			parameter = (Param as StratumReorderParam);
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryStratumReorder(), new StratumReorderParam(parameter.FromId, parameter.ToId));
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			if (Undo.Current.EditMenu.State != Editing.EditStates.TexturesStat)
			{
				Undo.Current.EditMenu.State = Editing.EditStates.TexturesStat;
				Undo.Current.EditMenu.ChangeCategory(2);
			}

			Undo.Current.EditMenu.EditStratum.SwitchLayers(parameter.ToId, parameter.FromId);


		}
	}
}