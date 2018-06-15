using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using MapLua;

public class HistoryArmyChange : HistoryObject {

	public static ScenarioLua.Army CurrentArmy;

	public ScenarioLua.Army Army;
	public string ArmyName;
	public float NoRushX;
	public float NoRushY;

	public string personality = "";
	public string plans = "";
	public int color = 0;
	public int faction = 0;
	public string nextPlatoonBuilderId = "";

	public float Mass;
	public float Energy;

	public SaveLua.Army.Aliance[] Alliances;

	public override void Register(){
		Army = CurrentArmy;

		ArmyName = Army.Name;
		NoRushX = Army.NoRush.X;
		NoRushY = Army.NoRush.Y;

		Mass = Army.Data.Economy.mass;
		Energy = Army.Data.Economy.energy;

		personality = Army.Data.personality;
		plans = Army.Data.plans;
		color = Army.Data.color;
		faction = Army.Data.faction;
		nextPlatoonBuilderId = Army.Data.nextPlatoonBuilderId;

		Alliances = new SaveLua.Army.Aliance[Army.Data.Alliances.Count];
		for(int i = 0; i < Alliances.Length; i++)
		{
			Alliances[i] = new SaveLua.Army.Aliance();
			Alliances[i].AllianceType = Army.Data.Alliances[i].AllianceType;
			Alliances[i].ConnectedArmy = Army.Data.Alliances[i].ConnectedArmy;
		}
	}


	public override void DoUndo(){
		CurrentArmy = Army;

		if (!RedoGenerated)
			HistoryArmyChange.GenerateRedo (Undo.Current.Prefabs.ArmyChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){

		Army.Name = ArmyName;
		Army.NoRush.X = NoRushX;
		Army.NoRush.Y = NoRushY;

		Army.Data.Economy.mass = Mass;
		Army.Data.Economy.energy = Energy;

		Army.Data.personality = personality;
		Army.Data.plans = plans;
		Army.Data.color = color;
		Army.Data.faction = faction;
		Army.Data.nextPlatoonBuilderId = nextPlatoonBuilderId;


		if (ArmyInfo.Current)
		{
			//ArmyInfo.Current.ReturnFromArmy();
			ArmyInfo.Current.ForceCurrentArmy(Army);
			ArmyInfo.Current.UpdateList();
		}

		Undo.Current.EditMenu.ChangeCategory(0);
		Undo.Current.EditMenu.MapInfoMenu.ChangePage(1);
		Markers.MarkersControler.UpdateBlankMarkersGraphics();


	}
}
