using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MapLua;

namespace Markers
{
	public class MarkersControler : MonoBehaviour
	{

		public static MarkersControler Current;

		public GameObject MarkerPrefab;
		public MarkerPropGraphic SpawnGraphic;
		public MarkerPropGraphic[] MarkerPropGraphics;
		public MarkerLayers MarkerLayersSettings;

		public Transform[] MasterChains;

		[System.Serializable]
		public class MarkerLayers
		{
			[Header("Basic")]
			public bool Blank = true;
			public bool Spawn = true;
			public bool Resource = true;
			public bool Camera = true;

			[Header("Paths")]
			public bool LandNodes = true;
			public bool AmphibiousNodes = true;
			public bool NavyNodes = true;
			public bool AirNodes = true;
			public bool ConnectedNodes = false;

			[Header("RallyPoints")]
			public bool RallyPoint = true;
			public bool NavyRallyPoint = true;

			[Header("SI")]
			public bool Other = true;


			public bool ActiveByType(SaveLua.Marker.MarkerTypes type)
			{
				switch (type)
				{
					case SaveLua.Marker.MarkerTypes.BlankMarker:
						return Blank;
					case SaveLua.Marker.MarkerTypes.Mass:
						return Resource;
					case SaveLua.Marker.MarkerTypes.Hydrocarbon:
						return Resource;
					case SaveLua.Marker.MarkerTypes.CameraInfo:
						return Camera;
					case SaveLua.Marker.MarkerTypes.LandPathNode:
						return LandNodes;
					case SaveLua.Marker.MarkerTypes.AmphibiousPathNode:
						return AmphibiousNodes;
					case SaveLua.Marker.MarkerTypes.WaterPathNode:
						return NavyNodes;
					case SaveLua.Marker.MarkerTypes.NavalLink:
						return NavyNodes;
					case SaveLua.Marker.MarkerTypes.AirPathNode:
						return AirNodes;
					case SaveLua.Marker.MarkerTypes.RallyPoint:
						return RallyPoint;
					case SaveLua.Marker.MarkerTypes.NavalRallyPoint:
						return NavyRallyPoint;
				}
				return Other;
			}

		}

		[System.Serializable]
		public class MarkerPropGraphic
		{
			public SaveLua.Marker.MarkerTypes mType;
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

			RenderMarkersConnections.Current.UpdateConnections();
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

			RenderMarkersConnections.Current.UpdateConnections();
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


		public static void CreateMarker(SaveLua.Marker Owner, int mc)
		{
			GameObject NewMarker = Instantiate(Current.MarkerPrefab, Current.MasterChains[mc]);
			NewMarker.name = Owner.Name;
			MarkerObject NewObj = NewMarker.GetComponent<MarkerObject>();
			NewObj.Owner = Owner;
			Owner.MarkerObj = NewObj;

			MarkerPropGraphic PropGraphic;
			if (Owner.MarkerType == SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(Owner.Name))
				PropGraphic = Current.SpawnGraphic;
			else
				PropGraphic = GetPropByType(Owner.MarkerType);
			NewObj.Mf.sharedMesh = PropGraphic.SharedMesh;
			NewObj.Mr.sharedMaterial = PropGraphic.SharedMaterial;
			NewObj.Bc.size = PropGraphic.SharedMesh.bounds.size;
			NewObj.Bc.center = PropGraphic.SharedMesh.bounds.center;

			NewObj.Tr.localPosition = ScmapEditor.ScmapPosToWorld(Owner.position);
			NewObj.Tr.localRotation = Quaternion.Euler(Owner.orientation);

			NewMarker.SetActive(Current.MarkerLayersSettings.ActiveByType(Owner.MarkerType));

		}


#region Refresh
		public static void UpdateGraphics(SaveLua.Marker Owner, int mc)
		{
			MarkerPropGraphic PropGraphic;
			if (Owner.MarkerType == SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(Owner.Name))
				PropGraphic = Current.SpawnGraphic;
			else
				PropGraphic = GetPropByType(Owner.MarkerType);

			Owner.MarkerObj.Mf.sharedMesh = PropGraphic.SharedMesh;
			Owner.MarkerObj.Mr.sharedMaterial = PropGraphic.SharedMaterial;
			Owner.MarkerObj.Bc.size = PropGraphic.SharedMesh.bounds.size;
			Owner.MarkerObj.Bc.center = PropGraphic.SharedMesh.bounds.center;

			Owner.MarkerObj.gameObject.SetActive(Current.MarkerLayersSettings.ActiveByType(Owner.MarkerType));

		}

		public static void UpdateBlankMarkersGraphics()
		{
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType == SaveLua.Marker.MarkerTypes.BlankMarker &&
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj != null)
					{
						UpdateGraphics(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m], mc);
					}
				}
			}
		}

		public static void UpdateLayers()
		{
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj != null)
					{
						bool Active = Current.MarkerLayersSettings.ActiveByType(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType);

						if (Active && !MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject.activeSelf)
							MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject.SetActive(true);
						else if(!Active && MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject.activeSelf)
							MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject.SetActive(false);
					}
				}
			}

			RenderMarkersConnections.Current.UpdateConnections();
		}


		public static int RecreateMarker(int mc, SaveLua.Marker Marker, int Insert = -1)
		{
			CreateMarker(Marker, mc);
			int ToReturn = 0;

			if (Insert >= 0 && Insert <= MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count)
			{
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Insert(Insert, Marker);
				ToReturn = Insert;
			}
			else
			{
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Add(Marker);
				ToReturn = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count - 1;
			}
			
			int ChainsCount = Marker.ConnectedToChains.Count;
			for (int c = 0; c < ChainsCount; c++)
			{
				if (Marker.ConnectedToChains[c] != null && !Marker.ConnectedToChains[c].ConnectedMarkers.Contains(Marker))
				{
					Marker.ConnectedToChains[c].ConnectedMarkers.Add(Marker);
				}
			}

			return ToReturn;

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

			RenderMarkersConnections.Current.UpdateConnections();
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
				UpdatingMarkers = true;
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
			UpdatingMarkers = false;

			yield return null;
			if (BufforMarkersUpdate)
			{
				BufforMarkersUpdate = false;
				UpdateMarkersHeights();
			}
		}
#endregion

	}
}
