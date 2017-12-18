using UnityEngine;
using System.Collections;

namespace UndoHistory
{
	public class HistoryClasses : MonoBehaviour
	{


	}

	[System.Serializable]
	public class UndoPrefabs
	{
		public GameObject MapInfo;
		public GameObject ArmiesChange;
		public GameObject ArmyChange;
		public GameObject AreasChange;
		public GameObject AreaChange;
		//public GameObject SelectionChange;
		//public GameObject SelectionRange;
		public GameObject MarkersRemove;
		public GameObject MarkersMove;
		//public GameObject MarkersSelection;
		public GameObject MarkersChange;
		public GameObject ChainChange;
		public GameObject ChainMarkers;
		public GameObject DecalsMove;
		public GameObject DecalsChange;
		public GameObject DecalValues;
		public GameObject DecalSharedValues;
		public GameObject PropsChange;
		public GameObject TerrainHeightChange;
		public GameObject StratumPaint;
		public GameObject StratumChange;
		public GameObject LightingChange;
	}
}
