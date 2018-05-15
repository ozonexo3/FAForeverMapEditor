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
		public Dropdown AiCreationDropdown;
		public Dropdown SpawnPressetDropdown;
		public GameObject CreateFromViewBtn;


		public GameObject MarkerPrefab;
		public GameObject[] MarkerPresets;

		public void ClearCreateNew()
		{
			for (int i = 0; i < CreateButtonSelections.Length; i++)
				CreateButtonSelections[i].SetActive(false);

			CreationId = -1;
			CreateFromViewBtn.SetActive(CreationId == 3);
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

			CreateFromViewBtn.SetActive(CreationId == 3);
			AiCreationDropdown.transform.parent.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.transform.parent.gameObject.SetActive(CreationId == 5);
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
						Undo.Current.RegisterMarkersAdd();
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
					if (CreationId == 3)
						NewMarker.orientation = Rotations[i].eulerAngles;
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

				Undo.Current.RegisterMarkersRemove();
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

		public void SnapAction(Transform marker)
		{
			//if (LastCreationType != MapLua.SaveLua.Marker.MarkerTypes.CameraInfo)
			//	marker.localRotation = Quaternion.identity;
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


		MapLua.SaveLua.Marker.MarkerTypes LastCreationType = MapLua.SaveLua.Marker.MarkerTypes.BlankMarker;
		GameObject GetCreationObject()
		{
			if(CreationId == 5)
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

				return MarkerPrefab;
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

			var paths = StandaloneFileBrowser.SaveFilePanel("Export markers", EnvPaths.GetMapsPath(), "", extensions);

			/*
			SaveFileDialog FileDialog = new SaveFileDialog();
			FileDialog.Title = "Export markers";
			FileDialog.AddExtension = true;

			FileDialog.DefaultExt = ".fafmapmarkers";
			FileDialog.Filter = "Faf Markers (*.fafmapmarkers)|*.fafmapmarkers";
			*/

			//System.Windows.Forms.FolderBrowserDialog FolderDialog = new FolderBrowserDialog();

			//FolderDialog.ShowNewFolderButton = false;
			//FolderDialog.Description = "Select 'Maps' folder.";

			//if (FileDialog.ShowDialog() == DialogResult.OK)
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
			}
		}

		public void ImportMarkers()
		{

			var extensions = new[] {
				new ExtensionFilter("Faf Markers", "fafmapmarkers")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import markers", EnvPaths.GetMapsPath(), extensions, false);


			/*
			OpenFileDialog FileDialog = new OpenFileDialog();
			FileDialog.Title = "Import markers";
			FileDialog.AddExtension = true;
			FileDialog.DefaultExt = ".fafmapmarkers";
			FileDialog.Filter = "Faf Markers (*.fafmapmarkers)|*.fafmapmarkers";
			FileDialog.CheckFileExists = true;
			*/

			if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
			{

				ExportMarkers ImpMarkers = JsonUtility.FromJson<ExportMarkers>(System.IO.File.ReadAllText(paths[0]));

				bool AnyCreated = false;
				int mc = 0;

				MapLua.SaveLua.Marker[] CreatedMarkers = new MapLua.SaveLua.Marker[ImpMarkers.Markers.Length];

				for (int m = 0; m < ImpMarkers.Markers.Length; m++)
				{
					if (!AnyCreated)
						Undo.Current.RegisterMarkersAdd();
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

			}
		}


	}
}
