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
			Selection.SelectionManager.Current.SetRemoveAction(DestroyMarkers);
			Selection.SelectionManager.Current.SetSelectionChangeAction(SelectMarkers);

			if (CreationId >= 0)
				SelectCreateNew(CreationId);
			GoToSelection();
		}


		void OnDisable()
		{
			if (MarkersInfo.MarkerPageChange)
			{

			}
			else
			{
				Selection.SelectionManager.Current.ClearAffectedGameObjects();
			}
			PlacementManager.Clear();
		}

		public void GoToSelection()
		{

			if (!MarkersInfo.MarkerPageChange)
			{
				Selection.SelectionManager.Current.CleanSelection();
			}

			Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects());
			Selection.SelectionManager.Current.SetCustomSettings(true, true, true);


			PlacementManager.Clear();
			if(ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

			MarkerSelectionOptions.UpdateOptions();

		}

		void GoToCreation()
		{
			Selection.SelectionManager.Current.ClearAffectedGameObjects();
			PlacementManager.BeginPlacement(GetCreationObject(), Place);
			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

			MarkerSelectionOptions.UpdateOptions();
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
			AiCreationDropdown.transform.parent.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.transform.parent.gameObject.SetActive(CreationId == 5);
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

			AiCreationDropdown.transform.parent.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.transform.parent.gameObject.SetActive(CreationId == 5);
		}

		public void ChangeList()
		{
			PlacementManager.BeginPlacement(GetCreationObject(), Place);
		}

		public List<int> LastAddedMarkers;

		public void Place(Vector3[] Positions, Quaternion[] Rotations)
		{
			//List<MapLua.SaveLua.Marker> NewMarkers = new List<MapLua.SaveLua.Marker>();
			int mc = 0; // MasterChainID
			LastAddedMarkers = new List<int>();
			int TotalMarkersCount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
			bool AnyCreated = false;

			if (CreationId == 5)
			{
				Vector3 NewPos;
				MarkerPreset Mpreset = MarkerPresets[SpawnPressetDropdown.value].GetComponent<MarkerPreset>();

				for (int i = 0; i < Positions.Length; i++)
				{
					for (int m = 0; m < Mpreset.Markers.Length; m++)
					{
						if(!AnyCreated)
							Undo.Current.RegisterMarkersAdd();
						AnyCreated = true;

						//Debug.Log(Mpreset.Markers[m].Tr.localPosition);
						NewPos = Positions[i] + Rotations[i] * Mpreset.Markers[m].Tr.localPosition;

						if (SelectionManager.Current.SnapToGrid)
							NewPos = ScmapEditor.SnapToGridCenter(NewPos, true);

						//NewPos.y = ScmapEditor.Current.Teren.SampleHeight(NewPos);

						MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(Mpreset.Markers[m].MarkerType);
						NewMarker.position = ScmapEditor.WorldPosToScmap(NewPos);
						MarkersControler.CreateMarker(NewMarker, mc);
						ChainsList.AddToCurrentChain(NewMarker);


						LastAddedMarkers.Add(TotalMarkersCount);
						TotalMarkersCount++;
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Add(NewMarker);
					}
				}
			}
			else
			{
				for (int i = 0; i < Positions.Length; i++)
				{
					if (!AnyCreated)
						Undo.Current.RegisterMarkersAdd();
					AnyCreated = true;

					MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(LastCreationType);

					if (SelectionManager.Current.SnapToGrid)
						Positions[i] = ScmapEditor.SnapToGridCenter(Positions[i], true);

					//Positions[i].y = ScmapEditor.Current.Teren.SampleHeight(Positions[i]);

					ChainsList.AddToCurrentChain(NewMarker);

					NewMarker.position = ScmapEditor.WorldPosToScmap(Positions[i]);
					MarkersControler.CreateMarker(NewMarker, mc);
					LastAddedMarkers.Add(TotalMarkersCount);
					TotalMarkersCount++;
					MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Add(NewMarker);
				}

			}

			if (AnyCreated)
			{
				//MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Concat<MapLua.SaveLua.Marker>(NewMarkers.ToArray());
				MarkerSelectionOptions.UpdateOptions();
			}
		}

		public int[] LastDestroyedMarkers;
		public void DestroyMarkers(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			int mc = 0; // MasterChainID
			bool AnyRemoved = false;
			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;

			if (RegisterUndo)
			{
				LastDestroyedMarkers = new int[MarkerObjects.Count];
				int Step = 0;
				for (int i = 0; i < Mcount; i++)
				{
					//bool Removed = false;
					for (int m = 0; m < MarkerObjects.Count; m++)
					{
						if (MarkerObjects[m] == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject)
						{
							LastDestroyedMarkers[Step] = i;
							Step++;
							break;
						}
					}
				}

				Undo.Current.RegisterMarkersRemove();
			}

			//List<MapLua.SaveLua.Marker> NewMarkers = new List<MapLua.SaveLua.Marker>();
			
			for (int i = 0; i < Mcount; i++)
			{
				//bool Removed = false;
				for(int m = 0; m < MarkerObjects.Count; m++)
				{
					if(MarkerObjects[m] == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject)
					{
						MapLua.SaveLua.RemoveMarkerName(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].Name);
						Destroy(MarkerObjects[m]);
						MarkerObjects.RemoveAt(m);
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i] = null;
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.RemoveAt(i);
						i--;
						AnyRemoved = true;
						break;
					}
				}

				//if (!Removed)
				//	NewMarkers.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i]);
			}

			//MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers = NewMarkers.ToArray();
			if (AnyRemoved)
			{
				Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects());
				MarkerSelectionOptions.UpdateOptions();
			}
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
				return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown.options[AiCreationDropdown.value].text);

				//TODO
				//return MapLua.SaveLua.Marker.MarkerTypes.CombatZone;

			}

			return MapLua.SaveLua.Marker.MarkerTypes.Mass;
		}

		public void SelectMarkers()
		{


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
