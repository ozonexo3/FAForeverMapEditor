using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markers
{
	public class MarkersControler : MonoBehaviour
	{

		public static MarkersControler Current;

		public GameObject MarkerPrefab;
		public MarkerPropGraphic[] MarkerPropGraphics;

		public Transform[] MasterChains;

		[System.Serializable]
		public class MarkerPropGraphic
		{
			public MapLua.SaveLua.Marker.MarkerTypes mType;
			public Mesh SharedMesh;
			public Material SharedMaterial;
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

				for (int m = 0; m < MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Length; m++)
				{
					if(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj == null)
					{
						CreateMarker(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m], mc);
					}
				}
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
		}

		public static GameObject[] GetMarkerObjects()
		{
			List<GameObject> AllGameObjects = new List<GameObject>();
			for (int mc = 0; mc < MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length; mc++)
			{
				for (int m = 0; m < MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Length; m++)
				{
					AllGameObjects.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.gameObject);
				}
			}
			return AllGameObjects.ToArray();
		}


		static void CreateMarker(MapLua.SaveLua.Marker Owner, int mc)
		{
			GameObject NewMarker = Instantiate(Current.MarkerPrefab, Current.MasterChains[mc]);
			NewMarker.name = Owner.Name;
			MarkerObject NewObj = NewMarker.GetComponent<MarkerObject>();
			NewObj.Owner = Owner;
			Owner.MarkerObj = NewObj;

			MarkerPropGraphic PropGraphic = GetPropByType(Owner.MarkerType);
			NewObj.Mf.sharedMesh = PropGraphic.SharedMesh;
			NewObj.Mr.sharedMaterial = PropGraphic.SharedMaterial;
			NewObj.Bc.size = PropGraphic.SharedMesh.bounds.size;
			NewObj.Bc.center = PropGraphic.SharedMesh.bounds.center;

			NewObj.Tr.localPosition = ScmapEditor.MapPosInWorld(Owner.position);
		}

		public static MarkerPropGraphic GetPropByType(MapLua.SaveLua.Marker.MarkerTypes mType)
		{
			for(int i = 0; i < Current.MarkerPropGraphics.Length; i++)
			{
				if(Current.MarkerPropGraphics[i].mType == mType)
				{
					return Current.MarkerPropGraphics[i];
				}
			}

			return Current.MarkerPropGraphics[0];
		}


		public static void RemoveMarker(int mc, int m)
		{
			// TODO

		}

		public static void AddMarker(int mc, MapLua.SaveLua.Marker.MarkerTypes MarkerType)
		{
			// TODO



		}

	}
}
