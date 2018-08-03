using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Markers;
using Selection;
//using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using SFB;

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
			SelectionManager.Current.DisableLayer = 9;
			SelectionManager.Current.SetRemoveAction(DestroyMarkers);
			SelectionManager.Current.SetSelectionChangeAction(SelectMarkers);
			SelectionManager.Current.SetCustomSnapAction(SnapAction);

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
				SelectionManager.Current.CleanSelection();
			}

			int[] Types;
			SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects(out Types), SelectionManager.SelectionControlTypes.Marker);
			SelectionManager.Current.SetAffectedTypes(Types);
			//Selection.SelectionManager.Current.SetCustomSettings(true, false, false);


			PlacementManager.Clear();
			if(ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

			MarkerSelectionOptions.UpdateOptions();

		}

		void GoToCreation()
		{
			Selection.SelectionManager.Current.ClearAffectedGameObjects(false);
			PlacementManager.InstantiateAction = null;
			PlacementManager.MinRotAngle = 90;
			PlacementManager.BeginPlacement(GetCreationObject(), Place);
			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();

			MarkerSelectionOptions.UpdateOptions();
		}


		public GameObject[] CreateButtonSelections;
		int CreationId;
		public Dropdown AiCreationDropdown_Nodes;
		public Dropdown AiCreationDropdown_Combat;
		public Dropdown AiCreationDropdown_Defense;
		public Dropdown AiCreationDropdown_Expand;
		public Dropdown AiCreationDropdown_Experimental;
		public Dropdown AiCreationDropdown_Rally;
		public Dropdown AiCreationDropdown_Other;
		public Dropdown SpawnPressetDropdown;
		public GameObject CreateFromViewBtn;


		public GameObject MarkerPrefab;
		public GameObject[] MarkerPresets;

		public void ClearCreateNew()
		{
			for (int i = 0; i < CreateButtonSelections.Length; i++)
				CreateButtonSelections[i].SetActive(false);

			CreationId = -1;
			UpdateDropdowns();
		}


		const int CREATE_CAM = 3;
		const int CREATE_PRESET = 4;

		const int CREATE_AI_NODES = 5;
		const int CREATE_AI_COMBAT = 6;
		const int CREATE_AI_DEFENSE = 7;
		const int CREATE_AI_EXPAND = 8;
		const int CREATE_AI_EXPERIMENTAL = 9;
		const int CREATE_AI_RALLY = 10;
		const int CREATE_AI_OTHER = 11;

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

			UpdateDropdowns();
		}

		void UpdateDropdowns()
		{
			CreateFromViewBtn.SetActive(CreationId == CREATE_CAM);
			SpawnPressetDropdown.transform.parent.gameObject.SetActive(CreationId == CREATE_PRESET);

			//AI
			AiCreationDropdown_Nodes.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_NODES);
			//AiCreationDropdown_Combat.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_COMBAT);
			AiCreationDropdown_Defense.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_DEFENSE);
			AiCreationDropdown_Expand.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_EXPAND);
			//AiCreationDropdown_Experimental.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_EXPERIMENTAL);
			AiCreationDropdown_Rally.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_RALLY);
			AiCreationDropdown_Other.transform.parent.gameObject.SetActive(CreationId == CREATE_AI_OTHER);

		}

		public void ChangeList()
		{
			PlacementManager.BeginPlacement(GetCreationObject(), Place);
		}


		public void CreateFromView()
		{
			Place(new Vector3[] { CameraControler.Current.Pivot.localPosition }, new Quaternion[] { CameraControler.Current.Pivot.localRotation }, new Vector3[] { Vector3.one });
			int mc = 0;
			MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[LastAddedMarkers[0]].zoom = CameraControler.GetCurrentZoom();
		}

		public List<int> LastAddedMarkers;

		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales)
		{
			//List<MapLua.SaveLua.Marker> NewMarkers = new List<MapLua.SaveLua.Marker>();
			int mc = 0; // MasterChainID
			LastAddedMarkers = new List<int>();
			int TotalMarkersCount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
			bool AnyCreated = false;

			if (CreationId == CREATE_PRESET)
			{
				Vector3 NewPos;
				MarkerPreset Mpreset = MarkerPresets[SpawnPressetDropdown.value].GetComponent<MarkerPreset>();

				for (int i = 0; i < Positions.Length; i++)
				{
					for (int m = 0; m < Mpreset.Markers.Length; m++)
					{
						if(!AnyCreated)
							Undo.RegisterUndo(new UndoHistory.HistoryMarkersRemove());
						AnyCreated = true;

						//Debug.Log(Mpreset.Markers[m].Tr.localPosition);
						NewPos = Positions[i] + Rotations[i] * Mpreset.Markers[m].Tr.localPosition;

						if (SelectionManager.Current.SnapToGrid)
							NewPos = ScmapEditor.SnapToGridCenter(NewPos, true, SelectionManager.Current.SnapToWater);

						//NewPos.y = ScmapEditor.Current.Teren.SampleHeight(NewPos);

						MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(Mpreset.Markers[m].MarkerType);
						NewMarker.position = ScmapEditor.WorldPosToScmap(NewPos);
						//NewMarker.orientation = 
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
						Undo.RegisterUndo(new UndoHistory.HistoryMarkersRemove());
					AnyCreated = true;

					MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(LastCreationType);

					bool snapToWater = SelectionManager.Current.SnapToWater;

					if (LastCreationType == MapLua.SaveLua.Marker.MarkerTypes.Mass || LastCreationType == MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon)
						snapToWater = false;

					if (SelectionManager.Current.SnapToGrid) 
						Positions[i] = ScmapEditor.SnapToGridCenter(Positions[i], true, snapToWater);

					//Positions[i].y = ScmapEditor.Current.Teren.SampleHeight(Positions[i]);

					ChainsList.AddToCurrentChain(NewMarker);

					NewMarker.position = ScmapEditor.WorldPosToScmap(Positions[i]);
					if (CreationId == CREATE_CAM)
						NewMarker.orientation = MarkerObject.MarkerRotToScmapRot( Rotations[i], MapLua.SaveLua.Marker.MarkerTypes.CameraInfo);
					else
						NewMarker.orientation = Vector3.zero;
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
				MarkersControler.UpdateBlankMarkersGraphics();
				RenderMarkersWarnings.Generate();
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
						if (MarkerObjects[m] == null)
							break;

						if (MarkerObjects[m] == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject)
						{
							LastDestroyedMarkers[Step] = i;
							Step++;
							break;
						}
					}
				}

				Undo.RegisterUndo(new UndoHistory.HistoryMarkersRemove());
			}

			//List<MapLua.SaveLua.Marker> NewMarkers = new List<MapLua.SaveLua.Marker>();

			//Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;

			for (int i = 0; i < Mcount; i++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj == null)
					continue;

				//bool Removed = false;
				for(int m = 0; m < MarkerObjects.Count; m++)
				{
					if(MarkerObjects[m] == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.gameObject)
					{
						MapLua.SaveLua.RemoveMarker(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].Name);
						Destroy(MarkerObjects[m]);
						MarkerObjects.RemoveAt(m);
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i] = null;
						MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.RemoveAt(i);
						Mcount--;
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
				SelectionManager.Current.CleanSelection();
				int[] Types;
				SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects(out Types), SelectionManager.SelectionControlTypes.Marker);
				SelectionManager.Current.SetAffectedTypes(Types);
				MarkerSelectionOptions.UpdateOptions();

				RenderMarkersConnections.Current.UpdateConnections();
			}
		}

		public void SelectMarkers()
		{

		}

		public void SnapAction(Transform marker, GameObject Connected)
		{
			//if (LastCreationType != MapLua.SaveLua.Marker.MarkerTypes.CameraInfo)
			//	marker.localRotation = Quaternion.identity;
		}

		MapLua.SaveLua.Marker.MarkerTypes GetCreationType()
		{
			switch (CreationId)
			{
				case 0:
					return MapLua.SaveLua.Marker.MarkerTypes.BlankMarker;
				case 1:
					return MapLua.SaveLua.Marker.MarkerTypes.Mass;
				case 2:
					return MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon;
				case CREATE_CAM:
					return MapLua.SaveLua.Marker.MarkerTypes.CameraInfo;
				case CREATE_AI_NODES:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Nodes.options[AiCreationDropdown_Nodes.value].text);
				case CREATE_AI_COMBAT:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Combat.options[AiCreationDropdown_Combat.value].text);
				case CREATE_AI_DEFENSE:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Defense.options[AiCreationDropdown_Defense.value].text);
				case CREATE_AI_EXPAND:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Expand.options[AiCreationDropdown_Expand.value].text);
				case CREATE_AI_EXPERIMENTAL:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Experimental.options[AiCreationDropdown_Experimental.value].text);
				case CREATE_AI_RALLY:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Rally.options[AiCreationDropdown_Rally.value].text);
				case CREATE_AI_OTHER:
					return MapLua.SaveLua.Marker.StringToMarkerType(AiCreationDropdown_Other.options[AiCreationDropdown_Other.value].text);
			}


			return MapLua.SaveLua.Marker.MarkerTypes.Mass;
		}


		MapLua.SaveLua.Marker.MarkerTypes LastCreationType = MapLua.SaveLua.Marker.MarkerTypes.BlankMarker;
		GameObject GetCreationObject()
		{
			if(CreationId == CREATE_PRESET)
			{
				PlacementManager.SnapToWater = false;
				return MarkerPresets[SpawnPressetDropdown.value];
			}
			else
			{
				LastCreationType = GetCreationType();
				MarkersControler.MarkerPropGraphic Mpg = MarkersControler.GetPropByType(LastCreationType);

				switch (LastCreationType)
				{
					case MapLua.SaveLua.Marker.MarkerTypes.BlankMarker:
						PlacementManager.SnapToWater = false;
						break;
					case MapLua.SaveLua.Marker.MarkerTypes.Mass:
						PlacementManager.SnapToWater = false;
						break;
					case MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon:
						PlacementManager.SnapToWater = false;
						break;
					default:
						PlacementManager.SnapToWater = true;
						break;
				}

				MarkerNew NewMarkerObject = MarkerPrefab.GetComponent<MarkerNew>();
				NewMarkerObject.Mf.sharedMesh = Mpg.SharedMesh;
				NewMarkerObject.Mr.sharedMaterial = Mpg.SharedMaterial;

				NewMarkerObject.gameObject.SetActive(true);

				return MarkerPrefab;
			}
		}


		const string ExportPathKey = "MarkersMarkerExport";
		static string DefaultPath
		{
			get
			{
				return EnvPaths.GetLastPath(ExportPathKey, EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName);
			}
		}

		[System.Serializable]
		public class ExportMarkers
		{
			public ExportMarker[] Markers;
			public int MapWidth;
			public int MapHeight;

			[System.Serializable]
			public class ExportMarker
			{
				public MapLua.SaveLua.Marker.MarkerTypes MarkerType;
				public Vector3 Pos;
				public Quaternion Rot;
				public int[] Connected;
			}
		}

		public void ExportSelectedMarkers() {

			var extensions = new[] {
				new ExtensionFilter("Faf Markers", "fafmapmarkers")
			};

			var paths = StandaloneFileBrowser.SaveFilePanel("Export markers", DefaultPath, "", extensions);

			if (!string.IsNullOrEmpty(paths))
			{
				ExportMarkers ExpMarkers = new ExportMarkers();
				ExpMarkers.MapWidth = ScmapEditor.Current.map.Width;
				ExpMarkers.MapHeight = ScmapEditor.Current.map.Height;

				List<MapLua.SaveLua.Marker> SelectedObjects = new List<MapLua.SaveLua.Marker>();
				for(int i = 0; i < SelectionManager.Current.Selection.Ids.Count; i++)
				{
					SelectedObjects.Add(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<MarkerObject>().Owner);
				}
				ExpMarkers.Markers = new ExportMarkers.ExportMarker[SelectedObjects.Count];
				for(int i = 0; i < ExpMarkers.Markers.Length; i++)
				{
					ExpMarkers.Markers[i] = new ExportMarkers.ExportMarker();
					ExpMarkers.Markers[i].MarkerType = SelectedObjects[i].MarkerType;
					ExpMarkers.Markers[i].Pos = SelectedObjects[i].MarkerObj.Tr.localPosition;
					ExpMarkers.Markers[i].Rot = SelectedObjects[i].MarkerObj.Tr.localRotation;

					List<int> Connected = new List<int>();
					for(int c = 0; c < SelectedObjects[i].AdjacentToMarker.Count; c++)
					{
						if (SelectedObjects.Contains(SelectedObjects[i].AdjacentToMarker[c]))
						{
							Connected.Add(SelectedObjects.IndexOf(SelectedObjects[i].AdjacentToMarker[c]));
						}
					}
					ExpMarkers.Markers[i].Connected = Connected.ToArray();
				}



				System.IO.File.WriteAllText(paths, JsonUtility.ToJson(ExpMarkers));
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths));
				GenericInfoPopup.ShowInfo("Markers exported");
			}
		}

		public void ImportMarkers()
		{

			var extensions = new[] {
				new ExtensionFilter("Faf Markers", "fafmapmarkers")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import markers", DefaultPath, extensions, false);

			if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
			{

				ExportMarkers ImpMarkers = JsonUtility.FromJson<ExportMarkers>(System.IO.File.ReadAllText(paths[0]));

				bool AnyCreated = false;
				int mc = 0;

				MapLua.SaveLua.Marker[] CreatedMarkers = new MapLua.SaveLua.Marker[ImpMarkers.Markers.Length];

				for (int m = 0; m < ImpMarkers.Markers.Length; m++)
				{
					if (!AnyCreated)
						Undo.RegisterUndo(new UndoHistory.HistoryMarkersRemove());
					AnyCreated = true;


					if (SelectionManager.Current.SnapToGrid)
						ImpMarkers.Markers[m].Pos = ScmapEditor.SnapToGridCenter(ImpMarkers.Markers[m].Pos, true, SelectionManager.Current.SnapToWater);

					MapLua.SaveLua.Marker NewMarker = new MapLua.SaveLua.Marker(ImpMarkers.Markers[m].MarkerType);
					CreatedMarkers[m] = NewMarker;
					NewMarker.position = ScmapEditor.WorldPosToScmap(ImpMarkers.Markers[m].Pos);
					NewMarker.orientation = ImpMarkers.Markers[m].Rot.eulerAngles;
					MarkersControler.CreateMarker(NewMarker, mc);
					ChainsList.AddToCurrentChain(NewMarker);


					MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Add(NewMarker);
				}


				for (int m = 0; m < ImpMarkers.Markers.Length; m++)
				{
					CreatedMarkers[m].AdjacentToMarker = new List<MapLua.SaveLua.Marker>();
					for (int c = 0; c < ImpMarkers.Markers[m].Connected.Length; c++)
					{
						CreatedMarkers[m].AdjacentToMarker.Add(CreatedMarkers[ImpMarkers.Markers[m].Connected[c]]);
					}

				}

				RenderMarkersConnections.Current.UpdateConnections();
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths[0]));
				GenericInfoPopup.ShowInfo("Markers imported");
			}
		}


	}
}
