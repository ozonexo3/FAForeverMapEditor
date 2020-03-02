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

			[Header("SI")]
			public bool Combat = true;
			public bool Defense = true;
			public bool ProtExp = true;
			public bool RallyPoint = true;
			public bool Expand = true;
			public bool Other = true;

			static bool GetPrefs(string key, bool value)
			{
				return PlayerPrefs.GetInt(key, value ? 1 : 0) > 0;
			}

			static void SetPrefs(string key, bool value)
			{
				PlayerPrefs.SetInt(key, value ? 1 : 0);
			}

			public void LoadPrefs()
			{
				Blank = GetPrefs("Layers_BlankActive", Blank);
				Spawn = GetPrefs("Layers_SpawnActive", Spawn);
				Resource = GetPrefs("Layers_ResourcesActive", Resource);
				Camera = GetPrefs("Layers_CameraActive", Camera);

				LandNodes = GetPrefs("Layers_LandNodesActive", LandNodes);
				AmphibiousNodes = GetPrefs("Layers_AmphibiousNodesActive", AmphibiousNodes);
				NavyNodes = GetPrefs("Layers_NavalNodesActive", NavyNodes);
				AirNodes = GetPrefs("Layers_AirNodesActive", AirNodes);
				ConnectedNodes = GetPrefs("Layers_ConnectionsActive", ConnectedNodes);

				Combat = GetPrefs("Layers_CombatActive", Combat);
				Defense = GetPrefs("Layers_DefenseActive", Defense);
				ProtExp = GetPrefs("Layers_ProtExpActive", ProtExp);
				RallyPoint = GetPrefs("Layers_RallyPointActive", RallyPoint);
				Expand = GetPrefs("Layers_ExpandActive", Expand);
				Other = GetPrefs("Layers_OtherActive", Other);
			}

			public void SavePrefs()
			{
				SetPrefs("Layers_BlankActive", Blank);
				SetPrefs("Layers_SpawnActive", Spawn);
				SetPrefs("Layers_ResourcesActive", Resource);
				SetPrefs("Layers_CameraActive", Camera);

				SetPrefs("Layers_LandNodesActive", LandNodes);
				SetPrefs("Layers_AmphibiousNodesActive", AmphibiousNodes);
				SetPrefs("Layers_NavalNodesActive", NavyNodes);
				SetPrefs("Layers_AirNodesActive", AirNodes);
				SetPrefs("Layers_ConnectionsActive", ConnectedNodes);

				SetPrefs("Layers_CombatActive", Combat);
				SetPrefs("Layers_DefenseActive", Defense);
				SetPrefs("Layers_ProtExpActive", ProtExp);
				SetPrefs("Layers_RallyPointActive", RallyPoint);
				SetPrefs("Layers_ExpandActive", Expand);
				SetPrefs("Layers_OtherActive", Other);

				PlayerPrefs.Save();
			}

			public bool ActiveByType(SaveLua.Marker.MarkerTypes type)
			{
				switch (type)
				{
					case SaveLua.Marker.MarkerTypes.BlankMarker:
						return Blank;
					case SaveLua.Marker.MarkerTypes.Mass:
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
					case SaveLua.Marker.MarkerTypes.AirPathNode:
						return AirNodes;
					case SaveLua.Marker.MarkerTypes.RallyPoint:
					case SaveLua.Marker.MarkerTypes.NavalRallyPoint:
						return RallyPoint;
					case SaveLua.Marker.MarkerTypes.CombatZone:
						return Combat;
					case SaveLua.Marker.MarkerTypes.DefensivePoint:
					case SaveLua.Marker.MarkerTypes.NavalDefensivePoint:
						return Defense;
					case SaveLua.Marker.MarkerTypes.ProtectedExperimentalConstruction:
						return ProtExp;
					case SaveLua.Marker.MarkerTypes.ExpansionArea:
					case SaveLua.Marker.MarkerTypes.LargeExpansionArea:
					case SaveLua.Marker.MarkerTypes.NavalArea:
						return Expand;
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
			MarkerLayersSettings.LoadPrefs();
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
			if (UpdateProcess != null)
				Current.StopCoroutine(UpdateProcess);

			Clear();

			if (MapLuaParser.Current.SaveLuaFile == null || MapLuaParser.Current.SaveLuaFile.Data == null || MapLuaParser.Current.SaveLuaFile.Data.MasterChains == null)
				return;

			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc] == null || MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers == null)
					continue;

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
			for (int i = 0; i < Current.MasterChains.Length; i++)
			{
				Destroy(Current.MasterChains[i].gameObject);
			}
			Current.MasterChains = new Transform[0];
		}


		public static GameObject[] GetMarkerObjects(out int[] Types)
		{
			List<GameObject> AllGameObjects = new List<GameObject>();
			List<int> AllTypes = new List<int>();
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					AllGameObjects.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject);
					AllTypes.Add((int)MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType);
				}
			}
			Types = AllTypes.ToArray();
			return AllGameObjects.ToArray();
		}

		public static SaveLua.Marker[] GetMarkers()
		{
			List<SaveLua.Marker> AllObjects = new List<SaveLua.Marker>();
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					AllObjects.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m]);
				}
			}
			return AllObjects.ToArray();
		}

		public static int[] GetMarkerTypes()
		{
			List<int> AllTypes = new List<int>();
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					AllTypes.Add((int)MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType);
				}
			}
			return AllTypes.ToArray();
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
			NewObj.Tr.localRotation = MarkerObject.ScmapRotToMarkerRot(Owner.orientation, Owner.MarkerType);

			NewMarker.SetActive(Current.MarkerLayersSettings.ActiveByType(Owner.MarkerType));
		}


#region Refresh
		public static void UpdateGraphics(SaveLua.Marker Owner, int mc)
		{
			bool Active = true;
			MarkerPropGraphic PropGraphic;
			if (Owner.MarkerType == SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(Owner.Name)) {
				PropGraphic = Current.SpawnGraphic;
				Active = Current.MarkerLayersSettings.Spawn;
			}
			else {
				PropGraphic = GetPropByType(Owner.MarkerType);
				Active = Current.MarkerLayersSettings.ActiveByType(Owner.MarkerType);
			}

			Owner.MarkerObj.Mf.sharedMesh = PropGraphic.SharedMesh;
			Owner.MarkerObj.Mr.sharedMaterial = PropGraphic.SharedMaterial;
			Owner.MarkerObj.Bc.size = PropGraphic.SharedMesh.bounds.size;
			Owner.MarkerObj.Bc.center = PropGraphic.SharedMesh.bounds.center;

			Owner.MarkerObj.gameObject.SetActive(Active);

		}

		public static void UpdateBlankMarkersGraphics()
		{
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc] == null || MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers == null)
					continue;

				int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m] == null)
						continue;

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
					if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj != null) { 

						bool Active = true;

						if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType == SaveLua.Marker.MarkerTypes.BlankMarker && 
						ArmyInfo.ArmyExist(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].Name))
							Active = Current.MarkerLayersSettings.Spawn;
						else 
							Active = Current.MarkerLayersSettings.ActiveByType(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType);


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

		#region 2D markers
		public static HashSet<Marker2D> Marker2DComponents = new HashSet<Marker2D>();

		public static void ForceResetMarkers2D()
		{
			var ListEnum = Marker2DComponents.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				if (ListEnum.Current != null)
					ListEnum.Current.transform.localScale = Vector3.one;
			}
			ListEnum.Dispose();
		}

		public static void UpdateMarkers2D()
		{
			if (!FafEditorSettings.GetMarkers2D())
				return;

			var ListEnum = Marker2DComponents.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				if (ListEnum.Current != null)
					ListEnum.Current.UpdateScale();
			}
			ListEnum.Dispose();
		}
		#endregion



		#region Update
		private void LateUpdate()
		{
			UpdateMarkers2D();
		}


		public static bool IsUpdating
		{
			get
			{
				return Updating;
			}
		}

		static bool Updating = false;
		static bool BufforUpdate = false;
		static Coroutine UpdateProcess;
		public static void UpdateMarkersHeights()
		{
			if (!Updating)
			{
				Updating = true;
				UpdateProcess = Current.StartCoroutine(Current.UpdatingMarkersHeights());
			}
			else
				BufforUpdate = true;
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
			UpdateProcess = null;
			Updating = false;
			if (BufforUpdate)
			{
				BufforUpdate = false;
				UpdateMarkersHeights();
			}
		}
#endregion

	}
}
