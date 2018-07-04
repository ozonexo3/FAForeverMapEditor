using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using MapLua;

namespace UndoHistory
{
	public class HistoryArmiesChange : HistoryObject
	{

		public string[] TeamNames;
		public ScenarioLua.Team[] Teams;
		public ScenarioLua.Army[][] TeamArmies;
		public ScenarioLua.Army[] ExtraArmies;


		public override void Register(HistoryParameter Param)
		{
			int c = 0;

			Teams = new ScenarioLua.Team[MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.CopyTo(Teams, 0);
			TeamNames = new string[Teams.Length];
			TeamArmies = new ScenarioLua.Army[Teams.Length][];
			for (int t = 0; t < Teams.Length; t++)
			{
				TeamArmies[t] = new ScenarioLua.Army[Teams[t].Armys.Count];
				Teams[t].Armys.CopyTo(TeamArmies[t]);
				TeamNames[t] = Teams[t].name;
			}

			ExtraArmies = new ScenarioLua.Army[MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.CopyTo(ExtraArmies, 0);

		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryArmiesChange());
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			int c = 0;
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams = new ScenarioLua.Team[Teams.Length];
			Teams.CopyTo(MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams, 0);

			for (int t = 0; t < Teams.Length; t++)
			{
				MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys = TeamArmies[t].ToList();
				Teams[t].name = TeamNames[t];
			}

			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys = ExtraArmies.ToList();

			if (ArmyInfo.Current)
			{
				ArmyInfo.Current.ReturnFromArmy();
				ArmyInfo.Current.UpdateList();
			}

			Undo.Current.EditMenu.ChangeCategory(0);
			Undo.Current.EditMenu.MapInfoMenu.ChangePage(1);

			Markers.MarkersControler.UpdateBlankMarkersGraphics();
		}
	}
}