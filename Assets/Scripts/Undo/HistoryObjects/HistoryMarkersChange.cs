using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;

public class HistoryMarkersChange : MonoBehaviour {

	public		List<MapLuaParser.Mex>			Mexes = new List<MapLuaParser.Mex>();
	public		List<MapLuaParser.Hydro>		Hydros = new List<MapLuaParser.Hydro>();
	public		List<MapLuaParser.Army>			ARMY_ = new List<MapLuaParser.Army>();
	public		List<MapLuaParser.Marker>		SiMarkers = new List<MapLuaParser.Marker>();
	
	public		List<EditingMarkers.WorkingElement>			Selected = new List<EditingMarkers.WorkingElement>();
	public		EditingMarkers.SymmetrySelection[]			SymmetrySelectionList = new EditingMarkers.SymmetrySelection[0];

}
