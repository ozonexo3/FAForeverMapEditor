using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Markers;
using Selection;

public class ChainsList : MonoBehaviour {

	int ChainSelected = -1;
	public GameObject NewBtn;
	public GameObject ReturnBtn;
	public InputField ChainName;

	public RectTransform Pivot;
	public GameObject ListPrefab;
	public GameObject ListPrefab_Marker;
	public List<ListObject> AllFields = new List<ListObject>();


	// Use this for initialization
	void OnEnable () {

		Selection.SelectionManager.Current.SetSelectionChangeAction(SelectMarkers);

		Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects(), false, false, false, false);
		Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		ChainSelected = -1;

		UpdateList();

	}

	void OnDisable()
	{
		ChainSelected = -1;
		Selection.SelectionManager.Current.ClearAffectedGameObjects();
	}


	int LastGeneratedChains = -2;

	public void CleanMenu()
	{
		ChainSelected = -1;
		UpdateList();

	}

	void UpdateList()
	{
		NewBtn.SetActive(ChainSelected < 0);
		ReturnBtn.SetActive(ChainSelected >= 0);
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
			NewListObject.ClickActionId = SelectMarker;

			//NewListObject.ConnectedGameObject = CurrentMarker.MarkerObj.gameObject;

			NewListObject.Icon.sprite = Markers.MarkersControler.GetIconByType(CurrentMarker.MarkerType);
			AllFields.Add(NewListObject);
		}
	}

	public void ReturnFromChain()
	{
		ChainSelected = -1;
		Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		UpdateList();
	}

	public void RemoveCurrentChain()
	{
		Undo.Current.RegisterChainsChange();

		List<MapLua.SaveLua.Chain> Chains = MapLuaParser.Current.SaveLuaFile.Data.Chains.ToList();

		Chains.RemoveAt(ChainSelected);

		MapLuaParser.Current.SaveLuaFile.Data.Chains = Chains.ToArray();

		ChainSelected = -1;

		Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		UpdateList();
	}

	public void AddChain()
	{
		Undo.Current.RegisterChainsChange();

		List<MapLua.SaveLua.Chain> Chains = MapLuaParser.Current.SaveLuaFile.Data.Chains.ToList();
		MapLua.SaveLua.Chain NewChain = new MapLua.SaveLua.Chain();
		NewChain.Name = "Chain_" + Chains.Count.ToString();
		Chains.Add(NewChain);

		MapLuaParser.Current.SaveLuaFile.Data.Chains = Chains.ToArray();

		ChainSelected = -1;

		Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		UpdateList();
	}

	public void MoveUp()
	{
		//TODO
	}

	public void MoveDown()
	{
		//TODO
	}

	public void SelectChain(int i)
	{

		Selection.SelectionManager.Current.SetCustomSettings(true, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		int Mcount = MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers.Count;
		for (int m = 0; m < Mcount; m++) {
			if(MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers[m] != null && MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers[m].MarkerObj != null)
			Selection.SelectionManager.Current.SelectObjectAdd(MapLuaParser.Current.SaveLuaFile.Data.Chains[i].ConnectedMarkers[m].MarkerObj.gameObject);
		}

		ChainSelected = i;

		MapLuaParser.Current.SaveLuaFile.Data.Chains[i].BakeMarkers();

		UpdateList();
	}

	public void SelectMarkers()
	{
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
	}

	public void SelectMarker(int i)
	{
		// TODO Focus on marker
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
}
