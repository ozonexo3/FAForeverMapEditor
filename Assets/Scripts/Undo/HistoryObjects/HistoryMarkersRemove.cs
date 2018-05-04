using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Markers;

public class HistoryMarkersRemove : HistoryObject
{

	//public int[] AddedMarkersIds;
	//public MapLua.SaveLua.Marker[] RedoMarkers;

	public MapLua.SaveLua.Marker[] AllMarkers;

	public override void Register()
	{
		int mc = 0;
		AllMarkers = new MapLua.SaveLua.Marker[MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count];
		 MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.CopyTo(AllMarkers);
	}

	public override void DoUndo()
	{
		if(!RedoGenerated)
			HistoryMarkersRemove.GenerateRedo(Undo.Current.Prefabs.MarkersRemove).Register();
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
					Destroy(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject);
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
