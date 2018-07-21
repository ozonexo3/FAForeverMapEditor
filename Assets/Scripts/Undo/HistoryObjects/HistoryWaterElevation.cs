using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryWaterElevation : HistoryObject
	{

		public bool HasWater;
		public float Elevation;
		public float ElevationDeep;
		public float ElevationAbyss;

		public override void Register(HistoryParameter Param)
		{

			HasWater = ScmapEditor.Current.map.Water.HasWater;
			Elevation = ScmapEditor.Current.map.Water.Elevation;
			ElevationDeep = ScmapEditor.Current.map.Water.ElevationDeep;
			ElevationAbyss = ScmapEditor.Current.map.Water.ElevationAbyss;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				Undo.RegisterRedo(new HistoryWaterElevation());
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

			ScmapEditor.Current.map.Water.HasWater = HasWater;
			ScmapEditor.Current.map.Water.Elevation = Elevation;
			ScmapEditor.Current.map.Water.ElevationDeep = ElevationDeep;
			ScmapEditor.Current.map.Water.ElevationAbyss = ElevationAbyss;

			WaterInfo.Current.ReloadValues(true);
		}
	}
}