using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryTerrainHeight : HistoryObject
	{

		private TerrainHeightHistoryParameter parameter;
		public class TerrainHeightHistoryParameter : HistoryParameter
		{
			public float[,] newheights;

			public TerrainHeightHistoryParameter(float[,] newheights)
			{
				this.newheights = newheights;
			}
		}
		public float[,] Pixels;

		public override void Register(HistoryParameter Param)
		{
			UndoCommandName = "Terrain Heightmap";
			parameter = Param as TerrainHeightHistoryParameter;

			int x = parameter.newheights.GetLength(0);
			int y = parameter.newheights.GetLength(1);

			Pixels = new float[x, y];
			for (int i = 0; i < x; i++)
			{
				for (int j = 0; j < y; j++)
				{
					Pixels[i, j] = parameter.newheights[i, j];
				}
			}
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				float[,] UndoData_newheights = new float[0, 0];
				ScmapEditor.GetAllHeights(ref UndoData_newheights);
				Undo.RegisterRedo(new HistoryTerrainHeight(), new TerrainHeightHistoryParameter(UndoData_newheights));

			}
			RedoGenerated = true;
			DoRedo();

		}

		public override void DoRedo()
		{
			Undo.Current.EditMenu.SetState(Editing.EditStates.TerrainStat);

			Undo.Current.EditMenu.EditTerrain.ChangePage(0);

			ScmapEditor.SetAllHeights(Pixels);

			Undo.Current.EditMenu.EditTerrain.OnTerrainChanged();
			Undo.Current.EditMenu.EditTerrain.RegenerateMaps();
		}
	}
}