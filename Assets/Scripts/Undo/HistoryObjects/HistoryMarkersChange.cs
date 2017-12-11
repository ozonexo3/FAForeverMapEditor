using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;
using Selection;
using Markers;

public class HistoryMarkersChange : HistoryObject
{

	//public List<MapLuaParser.Mex> Mexes = new List<MapLuaParser.Mex>();
	//public List<MapLuaParser.Hydro> Hydros = new List<MapLuaParser.Hydro>();
	//public List<MapLuaParser.Army> ARMY_ = new List<MapLuaParser.Army>();
	//public List<MapLuaParser.Marker> SiMarkers = new List<MapLuaParser.Marker>();

	//public List<MarkersInfo.WorkingElement> Selected = new List<MarkersInfo.WorkingElement>();
	//public MarkersInfo.SymmetrySelection[] SymmetrySelectionList = new MarkersInfo.SymmetrySelection[0];

	//public List<MapLuaParser.SaveArmy> SaveArmys = new List<MapLuaParser.SaveArmy>();

	public static MapLua.SaveLua.Marker[] RegisterMarkers;


	public MarkerChange[] Markers;

	[System.Serializable]
	public class MarkerChange
	{
		public MapLua.SaveLua.Marker Marker;
		public bool Many;
		public string Name;
		public float zoom;
		public bool canSetCamera;
		public bool canSyncCamera;
		public float size;
		public float amount;

		public void Load(MapLua.SaveLua.Marker RegisterMarker)
		{
			Marker = RegisterMarker;
			Name = Marker.Name;
			zoom = Marker.zoom;
			canSetCamera = Marker.canSetCamera;
			canSyncCamera = Marker.canSyncCamera;
			size = Marker.size;
			amount = Marker.amount;
		}

		public void Redo()
		{
			if (Marker.Name != Name)
			{
				if (MapLua.SaveLua.NameExist(Name))
				{
					// Cant Undo, Name already exist
				}
				else
				{
					MapLua.SaveLua.RemoveMarkerName(Marker.Name);
					MapLua.SaveLua.RegisterMarkerName(Name);
					Marker.Name = Name;
					Marker.MarkerObj.gameObject.name = Name;
				}
			}
			Marker.zoom = zoom;
			Marker.canSetCamera = canSetCamera;
			Marker.canSyncCamera = canSyncCamera;
			Marker.size = size;
			Marker.amount = amount;
		}
	}


	public override void Register()
	{
		/*
		Selected = new List<MarkersInfo.WorkingElement>();
		for (int i = 0; i < Undo.Current.EditMenu.EditMarkers.Selected.Count; i++)
		{
			Selected.Add(Undo.Current.EditMenu.EditMarkers.Selected[i]);
		}
		SymmetrySelectionList = Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList;
		*/
		/*
		ARMY_ = Undo.Current.Scenario.ARMY_;
		Mexes = Undo.Current.Scenario.Mexes;
		Hydros = Undo.Current.Scenario.Hydros;
		SiMarkers = Undo.Current.Scenario.SiMarkers;
		SaveArmys = Undo.Current.Scenario.SaveArmys;
		*/

		//OldName = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].GetComponent<MarkerObject>().Owner.Name;

		//Marker = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].GetComponent<MarkerObject>().Owner;
		Markers = new MarkerChange[RegisterMarkers.Length];
		for (int i = 0; i < RegisterMarkers.Length; i++)
		{
			Markers[i] = new MarkerChange();
			Markers[i].Load(RegisterMarkers[i]);
		}


	}


	public override void DoUndo()
	{
		HistoryMarkersChange.RegisterMarkers = new MapLua.SaveLua.Marker[Markers.Length];
		for (int i = 0; i < Markers.Length; i++)
			HistoryMarkersChange.RegisterMarkers[i] = Markers[i].Marker;

		HistoryMarkersChange.GenerateRedo(Undo.Current.Prefabs.MarkersChange).Register();
		DoRedo();
	}

	public override void DoRedo()
	{

		if (Undo.Current.EditMenu.State != Editing.EditStates.MarkersStat)
		{
			Undo.Current.EditMenu.State = Editing.EditStates.MarkersStat;
			Undo.Current.EditMenu.ChangeCategory(4);
		}

		for(int i = 0; i < Markers.Length; i++)
		{
			Markers[i].Redo();
			if(i == 0)
				SelectionManager.Current.SelectObject(Markers[i].Marker.MarkerObj.gameObject);
			else
				SelectionManager.Current.SelectObjectAdd(Markers[i].Marker.MarkerObj.gameObject);

		}


		MarkerSelectionOptions.UpdateOptions();


		/*
		Undo.Current.Scenario.ARMY_ = ARMY_;
		Undo.Current.Scenario.Mexes = Mexes;
		Undo.Current.Scenario.Hydros = Hydros;
		Undo.Current.Scenario.SiMarkers = SiMarkers;
		Undo.Current.Scenario.SaveArmys = SaveArmys;
		*/

		/*
		Undo.Current.EditMenu.EditMarkers.GenerateAllWorkingElements();
		Undo.Current.EditMenu.EditMarkers.AllMarkersList.UpdateList();
		Undo.Current.Scenario.MarkerRend.Regenerate();

		Undo.Current.EditMenu.EditMarkers.Selected = Selected;
		Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList = SymmetrySelectionList;
		Undo.Current.EditMenu.EditMarkers.UpdateSelectionRing();
		*/

	}
}
