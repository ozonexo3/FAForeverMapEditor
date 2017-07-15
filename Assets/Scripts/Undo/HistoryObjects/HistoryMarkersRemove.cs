using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using Markers;

public class HistoryMarkersRemove : HistoryObject
{

	public int[] AddedMarkersIds;
	public MapLua.SaveLua.Marker[] RedoMarkers;

	public MapLua.SaveLua.Marker[] AllMarkers;

	public override void Register()
	{
		int mc = 0;
		AllMarkers = new MapLua.SaveLua.Marker[MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count];
		 MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.CopyTo(AllMarkers);
		/*
		AddedMarkersIds = NewMarkersInfo.Current.LastDestroyedMarkers;

		RedoMarkers = new MapLua.SaveLua.Marker[AddedMarkersIds.Length];
		for (int i = 0; i < AddedMarkersIds.Length; i++)
		{
			RedoMarkers[i] = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[AddedMarkersIds[i]];
		}*/
	}


	public override void DoUndo()
	{
		HistoryMarkersRemove.GenerateRedo(Undo.Current.Prefabs.MarkersRemove).Register();
		DoRedo();
		/*
		HistoryMarkersRemove RedoCommand = (HistoryMarkersRemove)HistoryMarkersMove.GenerateRedo(Undo.Current.Prefabs.MarkersRemove);
		//RedoCommand.Register();

		RedoCommand.RedoMarkers = new MapLua.SaveLua.Marker[AddedMarkersIds.Length];

		int mc = 0;

		//Create markers again
		for (int i = 0; i < RedoMarkers.Length; i++)
		{
			RedoCommand.RedoMarkers[i] = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[MarkersControler.RecreateMarker(mc, RedoMarkers[i], AddedMarkersIds[i])];
		}

		Undo.Current.EditMenu.ChangeCategory(4);
		NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(0);

		NewMarkersInfo.Current.GoToSelection();
		*/
		/*
		List<GameObject> AddedMarkersList = new List<GameObject>();
		RedoCommand.RedoMarkers = new MapLua.SaveLua.Marker[AddedMarkersIds.Length];
		RedoCommand.AddedMarkersIds = new int[AddedMarkersIds.Length];
		for (int i = 0; i < AddedMarkersIds.Length; i++)
		{
			AddedMarkersList.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[AddedMarkersIds[i]].MarkerObj.gameObject);
			RedoCommand.RedoMarkers[i] = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[AddedMarkersIds[i]];
			RedoCommand.AddedMarkersIds[i] = AddedMarkersIds[i];
		}

		Undo.Current.EditMenu.ChangeCategory(4);
		NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(0);

		NewMarkersInfo.Current.DestroyMarkers(AddedMarkersList, false);
		NewMarkersInfo.Current.GoToSelection();
		*/
		//NewMarkersInfo.Current.GoToSelection();
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
					Destroy(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject);

			}
		}

		MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = AllMarkers.ToList();

		MarkersControler.RegenerateMarkers();


		Undo.Current.EditMenu.ChangeCategory(4);
		NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(0);

		NewMarkersInfo.Current.GoToSelection();


		/*List<GameObject> AddedMarkersList = new List<GameObject>();
		for (int i = 0; i < RedoMarkers.Length; i++)
		{
			AddedMarkersList.Add(RedoMarkers[i].MarkerObj.gameObject);

			//AddedMarkersList.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[AddedMarkersIds[i]].MarkerObj.gameObject);
		}

		Undo.Current.EditMenu.ChangeCategory(4);
		NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(0);

		NewMarkersInfo.Current.DestroyMarkers(AddedMarkersList, false);
		NewMarkersInfo.Current.GoToSelection();*/
	}


}
