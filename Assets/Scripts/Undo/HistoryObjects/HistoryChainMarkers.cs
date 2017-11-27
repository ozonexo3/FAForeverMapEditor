using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;

public class HistoryChainMarkers : HistoryObject {

	public int ChainId;
	public string Name;
	public MapLua.SaveLua.Marker[] ConnectedMarkers;


	public override void Register(){
		ChainId = Undo.LastChainId;

		ConnectedMarkers = new MapLua.SaveLua.Marker[MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].ConnectedMarkers.Count];
		MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].ConnectedMarkers.CopyTo(ConnectedMarkers, 0);
		Name = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].Name;
	}


	public override void DoUndo(){
		Undo.LastChainId = ChainId;
		if (!RedoGenerated)
			HistoryChainMarkers.GenerateRedo (Undo.Current.Prefabs.ChainMarkers).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){

		//MapLuaParser.Current.SaveLuaFile.Data.Chains = new MapLua.SaveLua.Chain[AllChains.Length];
		//AllChains.CopyTo(MapLuaParser.Current.SaveLuaFile.Data.Chains, 0);

		MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].ConnectedMarkers = ConnectedMarkers.ToList();
		MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].Name = Name;

		MarkersInfo.Current.ChainsInfo.CleanMenu();

		//Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		Undo.Current.EditMenu.ChangeCategory(4);
		//NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(1);

		MarkersInfo.Current.ChainsInfo.SelectChain(ChainId);

	}
}
