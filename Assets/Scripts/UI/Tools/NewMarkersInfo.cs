using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Markers;
using Selection;

namespace EditMap
{
	public class NewMarkersInfo : MonoBehaviour
	{

		public static NewMarkersInfo Current;

		private void Awake()
		{
			Current = this;
		}

		void OnEnable()
		{
			if (CreationId >= 0)
				SelectCreateNew(CreationId);
			GoToSelection();

			Selection.SelectionManager.Current.SetRemoveAction(DestroyMarkers);
		}


		void OnDisable()
		{
			Selection.SelectionManager.Current.ClearAffectedGameObjects();
			PlacementManager.Clear();
		}

		void GoToSelection()
		{
			Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects());
			PlacementManager.Clear();
			if(ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();
		}

		void GoToCreation()
		{
			Selection.SelectionManager.Current.ClearAffectedGameObjects();
			PlacementManager.BeginPlacement(GetCreationObject(), Place);
			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();
		}


		public GameObject[] CreateButtonSelections;
		int CreationId;
		public Dropdown AiCreationDropdown;
		public Dropdown SpawnPressetDropdown;


		public GameObject MarkerPrefab;
		public GameObject[] MarkerPresets;

		public void ClearCreateNew()
		{
			for (int i = 0; i < CreateButtonSelections.Length; i++)
				CreateButtonSelections[i].SetActive(false);

			CreationId = -1;
			AiCreationDropdown.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.gameObject.SetActive(CreationId == 5);
		}

		public void SelectCreateNew(int id)
		{
			for (int i = 0; i < CreateButtonSelections.Length; i++)
				CreateButtonSelections[i].SetActive(false);

			if (id == CreationId)
			{
				CreationId = -1;
				GoToSelection();
			}
			else
			{
				CreationId = id;
				CreateButtonSelections[CreationId].SetActive(true);
				GoToCreation();
			}

			AiCreationDropdown.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.gameObject.SetActive(CreationId == 5);
		}

		public void ChangeList()
		{
			PlacementManager.BeginPlacement(GetCreationObject(), Place);

		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations)
		{
			List<MapLua.SaveLua.Marker> NewMarkers = new List<MapLua.SaveLua.Marker>();
			int mc = 0; // MasterChainID

			if (CreationId == 5)
			{
				Vector3 NewPos;
				MarkerPreset Mpreset = MarkerPresets[SpawnPressetDropdown.value].GetComponent<MarkerPreset>();

				for (int i = 0; i < Positions.Length; i++)
				{
					for (int m = 0; m < Mpreset.Markers.Length; m++)
					{
						//Debug.Log(Mpreset.Markers[m].Tr.localPosition);
						NewPos = Positions[i] + Rotations[i] * Mpreset.Markers[m].Tr.localPosition;

						if (SelectionManager.Current.SnapToGrid)
							NewPos = ScmapEditor.SnapToGridCenter(NewPos);

						NewPos.y = ScmapEditor.Current.Teren.SampleHeight(NewPos);

						MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(Mpreset.Markers[m].MarkerType, Mpreset.Markers[m].MarkerType.ToString() + "_" + i + "_" + m);
						NewMarker.position = ScmapEditor.MapWorldPosInSave(NewPos);
						MarkersControler.CreateMarker(NewMarker, mc);

						NewMarkers.Add(NewMarker);
					}
				}
			}
			else
			{
				for (int i = 0; i < Positions.Length; i++)
				{
					MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(LastCreationType, LastCreationType.ToString() + "_" + i);

					if (SelectionManager.Current.SnapToGrid)
						Positions[i] = ScmapEditor.SnapToGridCenter(Positions[i]);

					Positions[i].y = ScmapEditor.Current.Teren.SampleHeight(Positions[i]);

					NewMarker.position = ScmapEditor.MapWorldPosInSave(Positions[i]);
					MarkersControler.CreateMarker(NewMarker, mc);

					NewMarkers.Add(NewMarker);
				}

			}


			if (NewMarkers.Count > 0)
			{
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Concat<MapLua.SaveLua.Marker>(NewMarkers.ToArray());
			}
		}


		public void DestroyMarkers(List<GameObject> MarkerObjects)
		{
			List<MapLua.SaveLua.Marker> NewMarkers = new List<MapLua.SaveLua.Marker>();
			int mc = 0; // MasterChainID

			for(int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Length; i++)
			{
				bool Removed = false;
				for(int m = 0; m < MarkerObjects.Count; m++)
				{
					if(MarkerObjects[m] == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject)
					{
						Destroy(MarkerObjects[m]);
						MarkerObjects.RemoveAt(m);
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i] = null;
						Removed = true;
					}
				}

				if (!Removed)
					NewMarkers.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i]);
			}

			MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = NewMarkers.ToArray();
		}


		MapLua.SaveLua.Marker.MarkerTypes GetCreationType()
		{
			if (CreationId == 0)
				return MapLua.SaveLua.Marker.MarkerTypes.BlankMarker;
			else if (CreationId == 1)
				return MapLua.SaveLua.Marker.MarkerTypes.Mass;
			else if (CreationId == 2)
				return MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon;
			else if (CreationId == 3)
				return MapLua.SaveLua.Marker.MarkerTypes.CameraInfo;
			else if (CreationId == 4)
			{
				//TODO
				return MapLua.SaveLua.Marker.MarkerTypes.CombatZone;

			}

			return MapLua.SaveLua.Marker.MarkerTypes.Mass;
		}


		MapLua.SaveLua.Marker.MarkerTypes LastCreationType = MapLua.SaveLua.Marker.MarkerTypes.BlankMarker;
		GameObject GetCreationObject()
		{
			if(CreationId == 5)
			{
				return MarkerPresets[SpawnPressetDropdown.value];
			}
			else
			{

				LastCreationType = GetCreationType();

				MarkersControler.MarkerPropGraphic Mpg = MarkersControler.GetPropByType(LastCreationType);

				MarkerNew NewMarkerObject = MarkerPrefab.GetComponent<MarkerNew>();
				NewMarkerObject.Mf.sharedMesh = Mpg.SharedMesh;
				NewMarkerObject.Mr.sharedMaterial = Mpg.SharedMaterial;

				return MarkerPrefab;

			}

		}

	}
}
