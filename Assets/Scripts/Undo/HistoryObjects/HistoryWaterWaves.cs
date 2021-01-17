using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryWaterWaves : HistoryObject
	{

		public WaveGenerator[] Waves;

		public override void Register(HistoryParameter Param)
		{
			Waves = ScmapEditor.Current.map.WaveGenerators.ToArray();
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				Undo.RegisterRedo(new HistoryWaterWaves());
			}
			RedoGenerated = true;
			DoRedo();

		}

		public override void DoRedo()
		{
			if (Undo.Current.EditMenu.State != Editing.EditStates.TerrainStat)
			{
				Undo.Current.EditMenu.State = Editing.EditStates.TerrainStat;
				Undo.Current.EditMenu.ChangeCategory(1);
			}

			Undo.Current.EditMenu.EditTerrain.ChangePage(1);

			ScmapEditor.Current.map.WaveGenerators.Clear();
			ScmapEditor.Current.map.WaveGenerators.AddRange(Waves);

			WavesRenderer.ReloadWaves();

			//WaterInfo.Current.ReloadValues(true);
		}
	}
}