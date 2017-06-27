using UnityEngine;
using System.Collections;

namespace UndoHistory{
	public class HistoryClasses : MonoBehaviour {


	}

	[System.Serializable]
	public class UndoPrefabs{
		public		GameObject		MapInfo;
		public		GameObject		MarkersMove;
		public		GameObject		MarkersSelection;
		public		GameObject		MarkersChange;
		public		GameObject		TerrainHeightChange;
		public		GameObject		StratumPaint;
		public		GameObject		StratumChange;
		public		GameObject		LightingChange;
	}
}
