using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Markers
{
	public class MarkersControler : MonoBehaviour
	{

		public static MarkersControler Current;

		public GameObject MarkerPrefab;
		public MarkerPropGraphic SpawnGraphic;
		public MarkerPropGraphic[] MarkerPropGraphics;

		public Transform[] MasterChains;

		[System.Serializable]
		public class MarkerPropGraphic
		{
			public MapLua.SaveLua.Marker.MarkerTypes mType;
			public Mesh SharedMesh;
			public Material SharedMaterial;
			public Sprite Icon;
		}


		void Awake()
		{
			Current = this;
		}
		
		public static void LoadMarkers()
		{
			Clear();
			Current.MasterChains = new Transform[MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length];


			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				Current.MasterChains[mc] = new GameObject(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Name).transform;
				Current.MasterChains[mc].parent = Current.transform;

				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj == null)
					{
						CreateMarker(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m], mc);
					}
				}
			}
		}

		public static void UnloadMarkers()
		{
			Clear();

			if (MapLuaParser.Current.SaveLuaFile == null || MapLuaParser.Current.SaveLuaFile.Data == null || MapLuaParser.Current.SaveLuaFile.Data.MasterChains == null)
				return;

			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj)
						Destroy(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject);
				}
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = null;
			}
		}

		public static void Save()
		{
			// TODO
			// Sort markers
			// Apply dynamic values
		}

		public static void Clear()
		{
			for(int i = 0; i < Current.MasterChains.Length; i++)
			{
				Destroy(Current.MasterChains[i].gameObject);
			}
			Current.MasterChains = new Transform[0];
		}


		public static GameObject[] GetMarkerObjects()
		{
			List<GameObject> AllGameObjects = new List<GameObject>();
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					AllGameObjects.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject);
				}
			}
			return AllGameObjects.ToArray();
		}


		public static void CreateMarker(MapLua.SaveLua.Marker Owner, int mc)
		{
			GameObject NewMarker = Instantiate(Current.MarkerPrefab, Current.MasterChains[mc]);
			NewMarker.name = Owner.Name;
			MarkerObject NewObj = NewMarker.GetComponent<MarkerObject>();
			NewObj.Owner = Owner;
			Owner.MarkerObj = NewObj;

			MarkerPropGraphic PropGraphic;
			if (Owner.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(Owner.Name))
				PropGraphic = Current.SpawnGraphic;
			else
				PropGraphic = GetPropByType(Owner.MarkerType);
			NewObj.Mf.sharedMesh = PropGraphic.SharedMesh;
			NewObj.Mr.sharedMaterial = PropGraphic.SharedMaterial;
			NewObj.Bc.size = PropGraphic.SharedMesh.bounds.size;
			NewObj.Bc.center = PropGraphic.SharedMesh.bounds.center;

			NewObj.Tr.localPosition = ScmapEditor.ScmapPosToWorld(Owner.position);
			NewObj.Tr.localRotation = Quaternion.Euler(Owner.orientation);
		}


#region Refresh
		public static void UpdateGraphics(MapLua.SaveLua.Marker Owner, int mc)
		{
			MarkerPropGraphic PropGraphic;
			if (Owner.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(Owner.Name))
				PropGraphic = Current.SpawnGraphic;
			else
				PropGraphic = GetPropByType(Owner.MarkerType);

			Owner.MarkerObj.Mf.sharedMesh = PropGraphic.SharedMesh;
			Owner.MarkerObj.Mr.sharedMaterial = PropGraphic.SharedMaterial;
			Owner.MarkerObj.Bc.size = PropGraphic.SharedMesh.bounds.size;
			Owner.MarkerObj.Bc.center = PropGraphic.SharedMesh.bounds.center;
		}

		public static void UpdateBlankMarkersGraphics()
		{
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj == null)
					{
						UpdateGraphics(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m], mc);
					}
				}
			}
		}


		public static int RecreateMarker(int mc, MapLua.SaveLua.Marker Marker, int Insert = -1)
		{
			CreateMarker(Marker, mc);

			if (Insert >= 0 && Insert <= MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count)
			{
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Insert(Insert, Marker);
				return Insert;
			}
			else
			{
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Add(Marker);
				return MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count - 1;
			}

			int ChainsCount = Marker.ConnectedToChains.Count;
			for (int c = 0; c < ChainsCount; c++)
			{
				if (Marker.ConnectedToChains[c] != null && !Marker.ConnectedToChains[c].ConnectedMarkers.Contains(Marker))
				{
					Marker.ConnectedToChains[c].ConnectedMarkers.Add(Marker);
				}
			}

		}

		public static void RegenerateMarkers()
		{
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj == null)
						CreateMarker(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m], mc);
				}
			}
		}

		#endregion

		#region Public functions
		public static MarkerPropGraphic GetPropByType(MapLua.SaveLua.Marker.MarkerTypes mType)
		{

			if (mType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(mType.ToString()))
				return Current.SpawnGraphic;

			for (int i = 0; i < Current.MarkerPropGraphics.Length; i++)
			{
				if(Current.MarkerPropGraphics[i].mType == mType)
				{
					return Current.MarkerPropGraphics[i];
				}
			}

			return Current.MarkerPropGraphics[0];
		}

		public static Sprite GetIconByType(MapLua.SaveLua.Marker.MarkerTypes mType)
		{
			for (int i = 0; i < Current.MarkerPropGraphics.Length; i++)
			{
				if (Current.MarkerPropGraphics[i].mType == mType)
				{
					return Current.MarkerPropGraphics[i].Icon;
				}
			}

			return Current.MarkerPropGraphics[0].Icon;

		}
		#endregion




#region Update

		static bool UpdatingMarkers = false;
		static bool BufforMarkersUpdate = false;
		public static void UpdateMarkersHeights()
		{
			//TODO
			if (!UpdatingMarkers)
			{
				Current.StartCoroutine(Current.UpdatingMarkersHeights());
			}
			else
				BufforMarkersUpdate = true;

		}

		public IEnumerator UpdatingMarkersHeights()
		{
			const int BreakEvery = 50;
			int UpdateCount = 0;
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{

					if (!MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj)
						continue;

					Vector3 CurrPos = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.Tr.localPosition;
					CurrPos.y = ScmapEditor.Current.Teren.SampleHeight(CurrPos);
					MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.Tr.localPosition = CurrPos;
					MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].position = ScmapEditor.WorldPosToScmap(CurrPos);


					UpdateCount++;
					if (UpdateCount > BreakEvery)
					{
						UpdateCount = 0;
						yield return null;
					}
				}
			}


			yield return null;
		}
#endregion

	}
}
