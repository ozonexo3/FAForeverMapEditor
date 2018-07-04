using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Markers;

namespace UndoHistory
{
	public class HistoryMarkersRemove : HistoryObject
	{

		public MapLua.SaveLua.Marker[] AllMarkers;

		public override void Register(HistoryParameter Param)
		{
			int mc = 0;
			AllMarkers = new MapLua.SaveLua.Marker[MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count];
			MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.CopyTo(AllMarkers);
		}

		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryMarkersRemove());
			RedoGenerated = true;
			DoRedo();
		}



		public override void DoRedo()
		{
			int mc = 0; // MasterChainID


			List<MapLua.SaveLua.Marker> NewMarkersList = AllMarkers.ToList();

			for (int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count; i++)
			{
				if (!NewMarkersList.Contains(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i]))
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj)
					{
						MapLua.SaveLua.RemoveMarker(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].Name);
						UnityEngine.Object.Destroy(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject);
					}
				}
			}

			MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = AllMarkers.ToList();

			for (int i = 0; i < AllMarkers.Length; i++)
			{
				MapLua.SaveLua.AddNewMarker(AllMarkers[i]);
			}

			MarkersControler.RegenerateMarkers();


			Undo.Current.EditMenu.ChangeCategory(4);
			NewMarkersInfo.Current.ClearCreateNew();
			MarkersInfo.Current.ChangePage(0);

			NewMarkersInfo.Current.GoToSelection();

		}
	}
}