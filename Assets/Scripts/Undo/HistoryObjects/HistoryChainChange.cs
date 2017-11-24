using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class HistoryChainChange : HistoryObject {


	public MapLua.SaveLua.Chain[] AllChains;


	public override void Register(){

		AllChains = new MapLua.SaveLua.Chain[MapLuaParser.Current.SaveLuaFile.Data.Chains.Length];
		MapLuaParser.Current.SaveLuaFile.Data.Chains.CopyTo(AllChains, 0);

	}


	public override void DoUndo(){
		if (!RedoGenerated)
			HistoryMarkersMove.GenerateRedo (Undo.Current.Prefabs.ChainChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){

		MapLuaParser.Current.SaveLuaFile.Data.Chains = new MapLua.SaveLua.Chain[AllChains.Length];

		AllChains.CopyTo(MapLuaParser.Current.SaveLuaFile.Data.Chains, 0);

		MarkersInfo.Current.ChainsInfo.CleanMenu();


		//Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		Undo.Current.EditMenu.ChangeCategory(4);
		//NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(1);

	}
}
