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

			Selection.SelectionManager.Current.SetSelectionChangeAction(SelectMarkers);

			Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects(), false, false, false, false);
			Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			//ChainSelected = -1;

			UpdateList();

		}

		void OnDisable()
		{
			//ChainSelected = -1;
			if (MarkersInfo.MarkerPageChange)
			{

			}
			else
			{
				Selection.SelectionManager.Current.ClearAffectedGameObjects();
			}
		}


		int LastGeneratedChains = -2;

		public void CleanMenu()
		{
			ChainSelected = -1;
			RenderChainConnection.RenderChain = null;
			UpdateList();

		}

		void UpdateList()
		{
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
			foreach (RectTransform child in Pivot)
			{
				AllFields = new List<ListObject>();
				Destroy(child.gameObject);
			}
		}

		#region Update List
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

				NewListObject.Icon.sprite = Markers.MarkersControler.GetIconByType(CurrentMarker.MarkerType);
				AllFields.Add(NewListObject);
			}
		}
		#endregion


		public void RemoveCurrentChain()
		{
			Undo.Current.RegisterChainsChange();

			List<MapLua.SaveLua.Chain> Chains = MapLuaParser.Current.SaveLuaFile.Data.Chains.ToList();

			Chains.RemoveAt(ChainSelected);

			MapLuaParser.Current.SaveLuaFile.Data.Chains = Chains.ToArray();

			ChainSelected = -1;
			RenderChainConnection.RenderChain = null;

			Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			Selection.SelectionManager.Current.CleanSelection();

			UpdateList();
		}

		public void AddChain()
		{
			Undo.Current.RegisterChainsChange();

			List<MapLua.SaveLua.Chain> Chains = MapLuaParser.Current.SaveLuaFile.Data.Chains.ToList();
			MapLua.SaveLua.Chain NewChain = new MapLua.SaveLua.Chain();
			NewChain.Name = "Chain_" + Chains.Count.ToString();
			NewChain.Markers = new string[0];
			NewChain.ConnectedMarkers = new List<MapLua.SaveLua.Marker>();
			Chains.Add(NewChain);

			MapLuaParser.Current.SaveLuaFile.Data.Chains = Chains.ToArray();

			ChainSelected = -1;
			RenderChainConnection.RenderChain = null;

			Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
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

		public void DragEnded(int AtId)
		{
			if (ChainSelected < 0)
				return;
			//Debug.Log(AtId + ", " + ListObject.DragBeginId);

			int count = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers.Count;

			if (AtId < 0 || ListObject.DragBeginId < 0 || AtId >= count || ListObject.DragBeginId >= count)
				return; // Wrong IDs

			if (AtId == ListObject.DragBeginId)
				return; // Same object

			List<MapLua.SaveLua.Marker> NewMarkerList = new List<MapLua.SaveLua.Marker>();

			for (int i = 0; i < count; i++)
			{
				if (i == AtId)
				{
					if(ListObject.DragBeginId > AtId)
					{
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[ListObject.DragBeginId]);
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i]);
					}
					else
					{
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[i]);
						NewMarkerList.Add(MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].ConnectedMarkers[ListObject.DragBeginId]);
					}
					
				}
				else if (i == ListObject.DragBeginId)
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

		}

		public void RemoveSelected()
		{

		}

		public void ReturnFromChain()
		{
			ChainSelected = -1;
			RenderChainConnection.RenderChain = null;
			Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			//Selection.SelectionManager.Current.CleanSelection();
			RenderChainConnection.RenderChain = null;
			UpdateList();
		}

		public void SelectChain(int i)
		{

			Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
			//Selection.SelectionManager.Current.CleanSelection();

			/*
			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers.Count;
			for (int m = 0; m < Mcount; m++) {
				if(MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers[m] != null && MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers[m].MarkerObj != null)
				Selection.SelectionManager.Current.SelectObjectAdd(MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers[m].MarkerObj.gameObject);
			}
			*/

			ChainSelected = i;

			MapLuaParser.Current.SaveLuaFile.Data.Chains[i].BakeMarkers();
			RenderChainConnection.RenderChain = MapLuaParser.Current.SaveLuaFile.Data.Chains[i];

			//RenderChainConnection.Transforms = MapLuaParser.Current.SaveLuaFile.Data.Chains[i].con

			UpdateList();
		}

		public void OnNameChanged()
		{
			if (ChainSelected < 0)
				return;

			if (MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name != ChainName.text.Replace("'", ""))
			{
				Undo.Current.RegisterChainMarkersChange(ChainSelected);

				MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name = ChainName.text.Replace("'", "");
				ChainName.text = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainSelected].Name;
			}
		}
		#endregion

		public void SelectMarkers()
		{
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