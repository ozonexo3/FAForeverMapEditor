using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class HistoryMarkersChange : HistoryObject {

	public		List<MapLuaParser.Mex>			Mexes = new List<MapLuaParser.Mex>();
	public		List<MapLuaParser.Hydro>		Hydros = new List<MapLuaParser.Hydro>();
	public		List<MapLuaParser.Army>			ARMY_ = new List<MapLuaParser.Army>();
	public		List<MapLuaParser.Marker>		SiMarkers = new List<MapLuaParser.Marker>();
	
	public		List<EditingMarkers.WorkingElement>			Selected = new List<EditingMarkers.WorkingElement>();
	public		EditingMarkers.SymmetrySelection[]			SymmetrySelectionList = new EditingMarkers.SymmetrySelection[0];

	public		List<MapLuaParser.SaveArmy> 		SaveArmys = new List<MapLuaParser.SaveArmy>();


	public override void Register(){
		Selected = new List<EditingMarkers.WorkingElement>();
		for(int i = 0; i < Undo.Current.EditMenu.EditMarkers.Selected.Count; i++){
			Selected.Add(Undo.Current.EditMenu.EditMarkers.Selected[i]);
		}
		SymmetrySelectionList = Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList;

		ARMY_ = Undo.Current.Scenario.ARMY_;
		Mexes = Undo.Current.Scenario.Mexes;
		Hydros = Undo.Current.Scenario.Hydros;
		SiMarkers = Undo.Current.Scenario.SiMarkers;
		SaveArmys = Undo.Current.Scenario.SaveArmys;
	}


	public override void DoUndo(){
		HistoryMarkersChange.GenerateRedo (Undo.Current.Prefabs.MarkersChange).Register();
		DoRedo ();
	}

	public override void DoRedo(){

		if(Undo.Current.EditMenu.State != Editing.EditStates.MarkersStat){
			Undo.Current.EditMenu.State = Editing.EditStates.MarkersStat;
			Undo.Current.EditMenu.ChangeCategory(4);
		}
			
		Undo.Current.Scenario.ARMY_ = ARMY_;
		Undo.Current.Scenario.Mexes = Mexes;
		Undo.Current.Scenario.Hydros = Hydros;
		Undo.Current.Scenario.SiMarkers = SiMarkers;
		Undo.Current.Scenario.SaveArmys = SaveArmys;

		Undo.Current.EditMenu.EditMarkers.GenerateAllWorkingElements();
		Undo.Current.EditMenu.EditMarkers.AllMarkersList.UpdateList();
		Undo.Current.Scenario.MarkerRend.Regenerate();

		Undo.Current.EditMenu.EditMarkers.Selected = Selected;
		Undo.Current.EditMenu.EditMarkers.SymmetrySelectionList = SymmetrySelectionList;
		Undo.Current.EditMenu.EditMarkers.UpdateSelectionRing();

	}
}
