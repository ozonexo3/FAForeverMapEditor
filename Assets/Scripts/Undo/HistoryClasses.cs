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
		[Header("Map")]
		public GameObject MapInfo;
		public GameObject ArmiesChange;
		public GameObject ArmyChange;
		public GameObject AreasChange;
		public GameObject AreaChange;
		[Header("Markers")]
		public GameObject MarkersRemove;
		public GameObject MarkersMove;
		public GameObject MarkersChange;
		public GameObject ChainChange;
		public GameObject ChainMarkers;
		[Header("Decals")]
		public GameObject DecalsMove;
		public GameObject DecalsChange;
		public GameObject DecalValues;
		public GameObject DecalSharedValues;
		public GameObject PropsChange;
		[Header("Terrain")]
		public GameObject TerrainHeightChange;
		public GameObject TerrainWaterElevationChange;
		public GameObject TerrainWaterSettingsChange;
		public GameObject TerrainTypePaint;
		[Header("Stratum")]
		public GameObject StratumPaint;
		public GameObject StratumChange;
		[Header("Lighting")]
		public GameObject LightingChange;

		[Header("Units")]
		public GameObject UnitGroupRemove;
		public GameObject UnitGroupChange;
		public GameObject UnitRemove;
		public GameObject UnitMove;
	}
}
