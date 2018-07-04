using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;

namespace UndoHistory
{
	public class HistoryStratumPaint : HistoryObject
	{

		private StratumPaintHistoryParameter parameter;
		public class StratumPaintHistoryParameter : HistoryParameter
		{
			public int StratumId;
			public Color[] Stratum;

			public StratumPaintHistoryParameter(int StratumId, Color[] Stratum)
			{
				this.StratumId = StratumId;
				this.Stratum = Stratum;
			}
		}

		int Id;
		Color[] Colors;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as StratumPaintHistoryParameter);

			Colors = new Color[parameter.Stratum.Length];
			parameter.Stratum.CopyTo(Colors, 0);
			Id = parameter.StratumId;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				Color[] Stratum;

				if (Id == 1)
				{
					Stratum = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
				}
				else
				{
					Stratum = ScmapEditor.Current.map.TexturemapTex.GetPixels();
				}

				Undo.RegisterRedo(new HistoryStratumPaint(), new StratumPaintHistoryParameter(Id, Stratum));
			}
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

			if (Id == 1)
			{
				ScmapEditor.Current.map.TexturemapTex2.SetPixels(Colors);
				ScmapEditor.Current.map.TexturemapTex2.Apply();
			}
			else
			{
				ScmapEditor.Current.map.TexturemapTex.SetPixels(Colors);
				ScmapEditor.Current.map.TexturemapTex.Apply();
			}

		}
	}
}