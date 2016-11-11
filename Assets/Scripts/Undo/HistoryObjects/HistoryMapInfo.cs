using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

public class HistoryMapInfo : HistoryObject {

	public		string			Name;
	public		string			Desc;
	public		float			Version;
	public		int				Script;

	public override void Register(){
		Name = Undo.Current.Scenario.ScenarioData.MapName;
		Desc = Undo.Current.Scenario.ScenarioData.MapDesc;
		Version = Undo.Current.Scenario.ScenarioData.Version;
		Script = Undo.Current.Scenario.ScriptId;
	}


	public override void DoUndo(){
		HistoryMapInfo.GenerateRedo (Undo.Current.Prefabs.MapInfo).Register();
		DoRedo ();
	}

	public override void DoRedo(){
		if(Undo.Current.EditMenu.State != Editing.EditStates.MapStat) Undo.Current.EditMenu.ButtonFunction("Map");
		Undo.Current.Scenario.ScenarioData.MapName = Name;
		Undo.Current.Scenario.ScenarioData.MapDesc = Desc;
		Undo.Current.Scenario.ScenarioData.Version = Version;
		Undo.Current.Scenario.ScriptId = Script;
		Undo.Current.MapInfoMenu.UpdateScriptToggles(Script);
		Undo.Current.MapInfoMenu.UpdateFields();
	}
}
