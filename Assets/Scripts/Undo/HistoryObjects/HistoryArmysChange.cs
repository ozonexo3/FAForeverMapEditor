using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;
using MapLua;

public class HistoryArmiesChange : HistoryObject {


	public ScenarioLua.Team[] Teams;
	public ScenarioLua.Army[][] TeamArmies;
	public ScenarioLua.Army[] ExtraArmies;


	public override void Register(){
		int c = 0;

		Teams = new ScenarioLua.Team[MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length];
		MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.CopyTo(Teams, 0);

		TeamArmies = new ScenarioLua.Army[Teams.Length][];
		for (int t = 0; t < Teams.Length; t++)
		{
			TeamArmies[t] = new ScenarioLua.Army[Teams[t].Armys.Count];
			Teams[t].Armys.CopyTo(TeamArmies[t]);
		}

		ExtraArmies = new ScenarioLua.Army[MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count];
		MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.CopyTo(ExtraArmies, 0);

		//AllChains = new MapLua.SaveLua.Chain[MapLuaParser.Current.SaveLuaFile.Data.Chains.Length];
		//MapLuaParser.Current.SaveLuaFile.Data.Chains.CopyTo(AllChains, 0);

	}


	public override void DoUndo(){
		if (!RedoGenerated)
			HistoryMarkersMove.GenerateRedo (Undo.Current.Prefabs.ChainChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){

		/*MapLuaParser.Current.SaveLuaFile.Data.Chains = new MapLua.SaveLua.Chain[AllChains.Length];

		AllChains.CopyTo(MapLuaParser.Current.SaveLuaFile.Data.Chains, 0);

		MarkersInfo.Current.ChainsInfo.CleanMenu();


		Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
		Selection.SelectionManager.Current.CleanSelection();

		Undo.Current.EditMenu.ChangeCategory(4);
		//NewMarkersInfo.Current.ClearCreateNew();
		MarkersInfo.Current.ChangePage(1);*/

	}
}
