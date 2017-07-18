using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapLua;

public class ArmyInfo : MonoBehaviour {

	private void OnEnable()
	{
		
	}

	private void OnDisable()
	{
		
	}

	public void UpdateList()
	{
		if(GetCurrentArmy() != null)
		{
			// Show Army

		}
		else
		{
			// Show Teams
		}
	}

	void Clean()
	{

	}


	void Generate()
	{
		for(int c = 0; c < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations.Length; c++)
		{
			for (int t = 0; t < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length; t++)
			{
				//Teams



			}

			for (int p = 0; p < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops.Length; p++)
			{
				if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops[p].key == ScenarioLua.ScenarioInfo.KEY_EXTRAARMIES)
				{
					// Extra armies
					string[] ArmyNames = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops[p].value.Split(" ".ToCharArray());

					for(int a = 0; a < ArmyNames.Length; a++)
					{


					}

					break;
				}
			}

		}
	}

	public SaveLua.Army GetCurrentArmy()
	{
		for (int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.Armies.Length; i++)
			if (MapLuaParser.Current.SaveLuaFile.Data.Armies[i].Name == SelectedArmy)
				return MapLuaParser.Current.SaveLuaFile.Data.Armies[i];

		return null;
	}

	SaveLua.Army ArmyByName(string Name){
		for (int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.Armies.Length; i++)
			if (MapLuaParser.Current.SaveLuaFile.Data.Armies[i].Name == Name)
				return MapLuaParser.Current.SaveLuaFile.Data.Armies[i];

		return null;
	}

	string SelectedArmy = "";
	public void SelectArmy(string selected)
	{
		SelectedArmy = selected;
	}
}
