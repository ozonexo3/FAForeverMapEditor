using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HistoryMarkersMove : MonoBehaviour {

	// MarkersPos
	public		Vector3[]							MarkersPosSelection; 
	public		MirrorMarkersPos[]					MirrorPos;

	// SelectionPos
	public		Vector3								SelectedMarker;
	public		Vector3[]							SelectedSymmetryMarkers;

	[System.Serializable]
	public class MirrorMarkersPos{
		public		Vector3[]							MarkersPosSelection;

	}
}
