using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Markers;
using Selection;

namespace EditMap
{
	public class ChainsList : MonoBehaviour
	{

		public static ChainsList Current;

		int ChainSelected = -1;
		public GameObject NewBtn;
		public GameObject ReturnBtn;
		public InputField ChainName;
		public GameObject MarkersAddButtons;

		public RectTransform Pivot;
		public GameObject ListPrefab;
		public GameObject ListPrefab_Marker;
		public List<ListObject> AllFields = new List<ListObject>();
		public RenderChainLine RenderChainConnection;

		public static int GetCurretnChain()
		{
			if (Current)
				return Current.ChainSelected;
			else
				return -1;
		}

		public static void AddToCurrentChain(MapLua.SaveLua.Marker Marker)
		{
			if (Current && Current.ChainSelected >= 0)
			{
				MapLuaParser.Current.SaveLuaFile.Data.Chains[Current.ChainSelected].ConnectedMarkers.Add(Marker);
				MapLuaParser.Current.SaveLuaFile.Data.Chains[Current.ChainSelected].BakeMarkers();
				Marker.ConnectedToChains.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[Current.ChainSelected]);
			}
		}

		private void Awake()
		{
			Current = this;
		}

		// Use this for initialization
		void OnEnable()
		{

			SelectionManager.Current.SetSelectionChangeAction(SelectMarkers);

			int[] Types;
			SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects(out Types), SelectionManager.SelectionControlTypes.MarkerChain);
			SelectionManager.Current.SetAffectedTypes(Types);
			//Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			//ChainSelected = -1;
			RenderChainLine.DisplayAll = true;
			UpdateList();

		}

		void OnDisable()
		{
			RenderChainLine.DisplayAll = false;
			//ChainSelected = -1;
			if (MarkersInfo.MarkerPageChange)
			{

			}
			else
			{
				Selection.SelectionManager.Current.ClearAffectedGameObjects();
			}
		}


		public void CleanMenu()
		{
			ChainSelected = -1;
			RenderChainLine.SelectedChain = -1;
			UpdateList();

		}

		void UpdateList()
		{
			if(MapLuaParser.Current.SaveLuaFile.Data == null || MapLuaParser.Current.SaveLuaFile.Data.Chains == null || ChainSelected >= MapLuaParser.Current.SaveLuaFile.Data.Chains.Length)
			{
				CleanMenu();
				return;
			}
				

			NewBtn.SetActive(ChainSelected < 0);
			ReturnBtn.SetActive(ChainSelected >= 0);
			MarkersAddButtons.SetActive(ChainSelected >= 0);
			ChainName.transform.parent.gameObject.SetActive(ChainSelected >= 0);

			Clean();

			if (ChainSelected < 0)
			{
				GenerateChains();
			}
			else
			{
				GenerateMarkers();
			}
		}

		void Clean()
		{
			AllFields = new List<ListObject>();
			AllFields.Capacity = 1024;

			foreach (RectTransform child in Pivot)
			{
				Destroy(child.gameObject);
			}
		}

		#region Update List
		public void UpdateSelection()
		{
			if (ChainSelected < 0)
				return;

			for (int g = 0; g < AllFields.Count; g++)
			{
				//int i = AllFields[g].InstanceId;

				int ObjectId = SelectionManager.Current.GetIdOfObject(AllFields[g].ConnectedGameObject);

				if (Selection.SelectionManager.Current.Selection.Ids.Contains(ObjectId))
					AllFields[g].SetSelection(1);
				else
				{
					AllFields[g].SetSelection(0);

					if (Selection.SelectionManager.Current.SymetrySelection.Length > 0)
					{
						for (int s = 0; s < Selection.SelectionManager.Current.SymetrySelection.Length; s++)
						{
							if (Selection.SelectionManager.Current.SymetrySelection[s].Ids.Contains(ObjectId))
								AllFields[g].SetSelection(2);
						}
					}
				}

			}
		}


		void GenerateChains()
		{
			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.Chains.Length;
			for (int i = 0; i < Mcount; i++)
			{
				//MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectMarkers(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[0].Markers);

				GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
				newList.GetComponent<RectTransform>().SetParent(Pivot);
				ListObject NewListObject = newList.GetComponent<ListObject>();

				NewListObject.ObjectName.text = MapLuaParser.Current.SaveLuaFile.Data.Chains[i].Name + " ( " + MapLuaParser.Current.SaveLuaFile.Data.Chains[i].Markers.Length + " )";
				NewListObject.InstanceId = i;
				NewListObject.ClickActionId = SelectChain;
				AllFields.Add(NewListObject);
			}
		}

		void GenerateMarkers()
		{
			ChainName.text = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name;

			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Count;
			for (int i = 0; i < Mcount; i++)
			{
				MapLua.SaveLua.Marker CurrentMarker = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i];

				GameObject newList = Instantiate(ListPrefab_Marker, Pivot.position, Quaternion.identity) as GameObject;
				newList.GetComponent<RectTransform>().SetParent(Pivot);
				ListObject NewListObject = newList.GetComponent<ListObject>();

				NewListObject.ObjectName.text = CurrentMarker.Name;
				NewListObject.InstanceId = i;
				//NewListObject.ClickActionId = SelectMarker;
				NewListObject.DragAction = DragEnded;

				NewListObject.ConnectedGameObject = CurrentMarker.MarkerObj.gameObject;
				NewListObject.ClickAction = Selection.SelectionManager.Current.SelectObject;

				int ObjectId = SelectionManager.Current.GetIdOfObject(NewListObject.ConnectedGameObject);

				if (Selection.SelectionManager.Current.Selection.Ids.Contains(ObjectId))
					NewListObject.SetSelection(1);
				else
				{
					NewListObject.SetSelection(0);

					if (Selection.SelectionManager.Current.SymetrySelection.Length > 0)
					{
						for (int s = 0; s < Selection.SelectionManager.Current.SymetrySelection.Length; s++)
						{
							if (Selection.SelectionManager.Current.SymetrySelection[s].Ids.Contains(ObjectId))
								NewListObject.SetSelection(2);
						}
					}
				}

				if (CurrentMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(CurrentMarker.Name))
					NewListObject.Icon.sprite = Markers.MarkersControler.Current.SpawnGraphic.Icon;
				else
					NewListObject.Icon.sprite = Markers.MarkersControler.GetIconByType(CurrentMarker.MarkerType);
				AllFields.Add(NewListObject);
			}
		}
		#endregion


		public void RemoveCurrentChain()
		{
			Undo.RegisterUndo(new UndoHistory.HistoryMarkersMove(), new UndoHistory.HistoryMarkersMove.MarkersMoveHistoryParameter(true));

			List<MapLua.SaveLua.Chain> Chains = MapLuaParser.Current.SaveLuaFile.Data.Chains.ToList();

			Chains.RemoveAt(ChainSelected);

			MapLuaParser.Current.SaveLuaFile.Data.Chains = Chains.ToArray();

			ChainSelected = -1;
			RenderChainLine.SelectedChain = -1;

			//Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			Selection.SelectionManager.Current.CleanSelection();

			UpdateList();
		}

		public void AddChain()
		{
			Undo.RegisterUndo(new UndoHistory.HistoryMarkersMove(), new UndoHistory.HistoryMarkersMove.MarkersMoveHistoryParameter(true));

			List<MapLua.SaveLua.Chain> Chains = MapLuaParser.Current.SaveLuaFile.Data.Chains.ToList();
			MapLua.SaveLua.Chain NewChain = new MapLua.SaveLua.Chain();
			NewChain.Name = "Chain_" + Chains.Count.ToString();
			NewChain.Markers = new string[0];
			NewChain.ConnectedMarkers = new List<MapLua.SaveLua.Marker>();
			Chains.Add(NewChain);

			MapLuaParser.Current.SaveLuaFile.Data.Chains = Chains.ToArray();

			ChainSelected = -1;
			RenderChainLine.SelectedChain = -1;

			//Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			Selection.SelectionManager.Current.CleanSelection();

			UpdateList();
		}


		#region UI
		public void MoveUp()
		{
			//TODO
		}

		public void MoveDown()
		{
			//TODO
		}

		public void DragEnded(ListObject AtId)
		{
			if (ChainSelected < 0)
				return;
			//Debug.Log(AtId + ", " + ListObject.DragBeginId);

			int count = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Count;

			if (AtId.InstanceId < 0 || ListObject.DragBeginId.InstanceId < 0 || AtId.InstanceId >= count || ListObject.DragBeginId.InstanceId >= count)
				return; // Wrong IDs

			if (AtId == ListObject.DragBeginId)
				return; // Same object

			List<MapLua.SaveLua.Marker> NewMarkerList = new List<MapLua.SaveLua.Marker>();

			for (int i = 0; i < count; i++)
			{
				if (i == AtId.InstanceId)
				{
					if (ListObject.DragBeginId.InstanceId > AtId.InstanceId)
					{
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[ListObject.DragBeginId.InstanceId]);
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i]);
					}
					else
					{
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i]);
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[ListObject.DragBeginId.InstanceId]);
					}

				}
				else if (i == ListObject.DragBeginId.InstanceId)
				{
					//i++;
					//NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i]);
				}
				else
					NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i]);
			}

			MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers = NewMarkerList;

			UpdateList();
		}

		public void AddSelected()
		{
			if (ChainSelected < 0)
				return;

			bool AnyChanged = false;
			int count = SelectionManager.Current.Selection.Ids.Count;

			if (count <= 0)
				return;

			for (int i = 0; i < count; i++)
			{

				MarkerObject Mobj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<MarkerObject>();

				if (Mobj != null && Mobj.Owner != null)
				{
					if (!MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Contains(Mobj.Owner))
					{
						if (!AnyChanged)
						{
							Undo.RegisterUndo(new UndoHistory.HistoryChainMarkers(), new UndoHistory.HistoryChainMarkers.ChainMarkersHistoryParameter(ChainSelected));
							AnyChanged = true;
						}
						MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Add(Mobj.Owner);
					}

				}
			}

			for (int s = 0; s < SelectionManager.Current.SymetrySelection.Length; s++)
			{
				count = SelectionManager.Current.SymetrySelection[s].Ids.Count;

				for (int i = 0; i < count; i++)
				{

					MarkerObject Mobj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.SymetrySelection[s].Ids[i]].GetComponent<MarkerObject>();

					if (Mobj != null && Mobj.Owner != null)
					{
						if (!MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Contains(Mobj.Owner))
						{
							if (!AnyChanged)
							{
								Undo.RegisterUndo(new UndoHistory.HistoryChainMarkers(), new UndoHistory.HistoryChainMarkers.ChainMarkersHistoryParameter(ChainSelected));
								AnyChanged = true;
							}
							MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Add(Mobj.Owner);
						}

					}
				}
			}

			UpdateList();

		}

		public void RemoveSelected()
		{
			if (ChainSelected < 0)
				return;

			bool AnyChanged = false;
			int count = SelectionManager.Current.Selection.Ids.Count;

			if (count <= 0)
				return;

			for (int i = 0; i < count; i++)
			{

				MarkerObject Mobj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<MarkerObject>();

				if (Mobj != null && Mobj.Owner != null)
				{
					if (MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Contains(Mobj.Owner))
					{
						if (!AnyChanged)
						{
							Undo.RegisterUndo(new UndoHistory.HistoryChainMarkers(), new UndoHistory.HistoryChainMarkers.ChainMarkersHistoryParameter(ChainSelected));
							AnyChanged = true;
						}
						MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Remove(Mobj.Owner);
					}

				}
			}

			for (int s = 0; s < SelectionManager.Current.SymetrySelection.Length; s++)
			{
				count = SelectionManager.Current.SymetrySelection[s].Ids.Count;

				for (int i = 0; i < count; i++)
				{

					MarkerObject Mobj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.SymetrySelection[s].Ids[i]].GetComponent<MarkerObject>();

					if (Mobj != null && Mobj.Owner != null)
					{
						if (MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Contains(Mobj.Owner))
						{
							if (!AnyChanged)
							{
								Undo.RegisterUndo(new UndoHistory.HistoryChainMarkers(), new UndoHistory.HistoryChainMarkers.ChainMarkersHistoryParameter(ChainSelected));
								AnyChanged = true;
							}
							MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Remove(Mobj.Owner);
						}

					}
				}
			}

			UpdateList();
		}

		public void ReturnFromChain()
		{
			ChainSelected = -1;
			RenderChainLine.SelectedChain = -1;
			//Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			SelectionManager.Current.UpdateControler();
			UpdateList();
		}



		public void SelectChain(ListObject i)
		{
			SelectChain(i.InstanceId);

		}

		public void SelectChain(int i)
		{
			//Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			SelectionManager.Current.UpdateControler();
			ChainSelected = i;

			MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].BakeMarkers();
			RenderChainLine.SelectedChain = ChainSelected;

			UpdateList();
		}

		public void OnNameChanged()
		{
			if (ChainSelected < 0)
				return;

			if (MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name != ChainName.text.Replace("'", ""))
			{
				Undo.RegisterUndo(new UndoHistory.HistoryChainMarkers(), new UndoHistory.HistoryChainMarkers.ChainMarkersHistoryParameter(ChainSelected));

				MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name = ChainName.text.Replace("'", "");
				ChainName.text = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name;
			}
		}
		#endregion

		public void SelectMarkers()
		{
			UpdateSelection();
			/*
			// Set All selected to chain
			if (ChainSelected < 0)
				return;

			// Register Chains Undo
			Undo.Current.RegisterChainMarkersChange(ChainSelected);

			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Count;
			for (int m = 0; m < Mcount; m++)
			{
				MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[m].ConnectedToChains.Remove(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected]);
			}

			MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers = new List<MapLua.SaveLua.Marker>();

			for(int i = 0; i < SelectionManager.Current.Selection.Ids.Count; i++)
			{
				MapLua.SaveLua.Marker SelectedMarker = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<MarkerObject>().Owner;

				MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Add(SelectedMarker);

				if(!SelectedMarker.ConnectedToChains.Contains(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected]))
					SelectedMarker.ConnectedToChains.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected]);
			}

			UpdateList();
			*/
		}

		public void SelectMarker(int i)
		{
			// TODO Focus on marker

		}

	}
}