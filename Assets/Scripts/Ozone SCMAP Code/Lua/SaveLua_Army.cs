using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	public partial class SaveLua
	{

		[System.Serializable]
		public class Army
		{
			public string Name;
			public string personality = "";
			public string plans = "";
			public int color = 0;
			public int faction = 0;
			public EconomyTab Economy;
			public Aliance[] Alliances;
			public UnitsGroup Units;
			public PlatoonBuilder[] PlatoonBuilders;

			const string KEY_PERSONALITY = "personality";
			const string KEY_PLANS = "plans";
			const string KEY_COLOR = "color";
			const string KEY_FACTION = "faction";
			const string KEY_ECONOMY = "Economy";
			const string KEY_ALLIANCES = "Alliances";
			const string KEY_UNITS = "Units";
			const string KEY_PLATOONBUILDERS = "PlatoonBuilders";

			#region Classes
			[System.Serializable]
			public class EconomyTab
			{
				public float mass = 0;
				public float energy = 0;

				public const string KEY_MASS = "mass";
				public const string KEY_ENERGY = "energy";
			}

			[System.Serializable]
			public class Aliance
			{
				public string Army = "";
				public string AllianceType = "Enemy";
			}

			//[System.Serializable]
			public class UnitsGroup
			{
				public string Name;
				public string orders;
				public string platoon;
				public UnitsGroup[] UnitGroups;
				public Unit[] Units;

				public const string KEY_ORDERS = "orders";
				public const string KEY_PLATOON = "platoon";
				public const string KEY_UNITS = "Units";
				public const string KEY_TYPE = "type";
				public const string KEY_POSITION = "Position";
				public const string KEY_ORIENTATION = "Orientation";

				public UnitsGroup()
				{
				}

				public UnitsGroup(string name, LuaTable Table)
				{
					Name = name;
					orders = LuaParser.Read.StringFromTable(Table, KEY_ORDERS);
					platoon = LuaParser.Read.StringFromTable(Table, KEY_PLATOON);

					LuaTable[] UnitsTables = LuaParser.Read.GetTableTables((LuaTable)Table.RawGet(KEY_UNITS));
					string[] UnitsNames = LuaParser.Read.GetTableKeys((LuaTable)Table.RawGet(KEY_UNITS));

					if (UnitsNames.Length > 0)
					{
						List<UnitsGroup> UnitGroupsList = new List<UnitsGroup>();
						List<Unit> UnitsList = new List<Unit>();

						for (int i = 0; i < UnitsNames.Length; i++)
						{
							if (LuaParser.Read.ValueExist(UnitsTables[i], KEY_TYPE))
							{
								Unit NewUnit = new Unit();
								NewUnit.Name = UnitsNames[i];
								NewUnit.type = LuaParser.Read.StringFromTable(UnitsTables[i], KEY_TYPE);
								NewUnit.orders = LuaParser.Read.StringFromTable(UnitsTables[i], KEY_ORDERS);
								NewUnit.platoon = LuaParser.Read.StringFromTable(UnitsTables[i], KEY_PLATOON);
								//Debug.Log(UnitsTables[i].RawGet(KEY_POSITION));
								NewUnit.Position = LuaParser.Read.Vector3FromTable(UnitsTables[i], KEY_POSITION);
								NewUnit.Orientation = LuaParser.Read.Vector3FromTable(UnitsTables[i], KEY_ORIENTATION);
								UnitsList.Add(NewUnit);
							}
							else
							{
								UnitsGroup NewUnitsGroup = new UnitsGroup(UnitsNames[i], UnitsTables[i]);
								UnitGroupsList.Add(NewUnitsGroup);
							}
						}

						UnitGroups = UnitGroupsList.ToArray();
						Units = UnitsList.ToArray();
						UnitGroupsList = null;
						UnitsList = null;
					}
					else
					{
						UnitGroups = new UnitsGroup[0];
						Units = new Unit[0];
					}
				}

				public void SaveUnitsGroup(LuaParser.Creator LuaFile)
				{
					LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Name) + LuaParser.Write.SetValue + "GROUP " + LuaParser.Write.OpenBracket);

					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_ORDERS, orders));
					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PLATOON, platoon));


					LuaFile.OpenTab(KEY_UNITS + LuaParser.Write.OpenBracketValue);
					for (int g = 0; g < UnitGroups.Length; g++)
					{
						UnitGroups[g].SaveUnitsGroup(LuaFile);
					}
					for (int u = 0; u < Units.Length; u++)
					{
						Units[u].SaveUnit(LuaFile);
					}
					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
				}
			}

			//[System.Serializable]
			public class Unit
			{
				public string Name;
				public string type;
				public string orders;
				public string platoon;
				public Vector3 Position;
				public Vector3 Orientation;

				public void SaveUnit(LuaParser.Creator LuaFile)
				{
					LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Name) + LuaParser.Write.OpenBracketValue);

					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_TYPE, type));
					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_ORDERS, orders));
					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_PLATOON, platoon));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLua(UnitsGroup.KEY_POSITION, Position));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLua(UnitsGroup.KEY_ORIENTATION, Orientation));

					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				}
			}

			public class PlatoonBuilder
			{
				public string Name;
				public string PlatoonTemplate;
				public int Priority;
				public int InstanceCount;
				public string LocationType;
				public int BuildTimeOut;
				public string PlatoonType;
				public bool RequiresConstruction;

				public const string KEY_NEXTPLATOONBUILDERID = "next_platoon_builder_id";
				public const string KEY_BUILDERS = "Builders";


				public class PlatoonAIFunction
				{


				}

				public class BuildCondition
				{
					int ID;
					string lua;
					string Name;
					// Brains;

				}

			}



			#endregion

			public Army()
			{
			}

			public Army(string name, LuaTable Table)
			{
				Name = name;

				personality = LuaParser.Read.StringFromTable(Table, KEY_PERSONALITY);
				plans = LuaParser.Read.StringFromTable(Table, KEY_PLANS);
				color = LuaParser.Read.IntFromTable(Table, KEY_COLOR);
				faction = LuaParser.Read.IntFromTable(Table, KEY_FACTION);

				LuaTable EconomyTable = (LuaTable)Table.RawGet(KEY_ECONOMY);
				Economy = new EconomyTab();
				Economy.mass = LuaParser.Read.FloatFromTable(EconomyTable, EconomyTab.KEY_MASS);
				Economy.energy = LuaParser.Read.FloatFromTable(EconomyTable, EconomyTab.KEY_ENERGY);

				LuaTable AlliancesTable = (LuaTable)Table.RawGet(KEY_ALLIANCES);
				string[] AlliancesKeys = LuaParser.Read.GetTableKeys(AlliancesTable);
				string[] AlliancesValues = LuaParser.Read.GetTableValues(AlliancesTable);

				Alliances = new Aliance[AlliancesKeys.Length];
				for (int a = 0; a < AlliancesKeys.Length; a++)
				{
					Alliances[a] = new Aliance();
					Alliances[a].Army = AlliancesKeys[a];
					Alliances[a].AllianceType = AlliancesValues[a];
				}

				LuaTable UnitsTable = (LuaTable)Table.RawGet(KEY_UNITS);
				Units = new UnitsGroup(KEY_UNITS, UnitsTable);

				//TODO Read PlatoonBuilders
			}

			public void SaveArmy(LuaParser.Creator LuaFile)
			{
				LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PERSONALITY, personality));
				LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PLANS, plans));
				LuaFile.AddLine(LuaParser.Write.IntToLua(KEY_COLOR, color));
				LuaFile.AddLine(LuaParser.Write.IntToLua(KEY_FACTION, faction));

				// Economy
				LuaFile.OpenTab(KEY_ECONOMY + LuaParser.Write.OpenBracketValue);
				LuaFile.AddLine(LuaParser.Write.FloatToLua(EconomyTab.KEY_MASS, Economy.mass));
				LuaFile.AddLine(LuaParser.Write.FloatToLua(EconomyTab.KEY_ENERGY, Economy.energy));
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				// Aliances
				LuaFile.OpenTab(KEY_ALLIANCES + LuaParser.Write.OpenBracketValue);
				for (int a = 0; a < Alliances.Length; a++)
				{
					LuaFile.AddLine(LuaParser.Write.StringToLua(LuaParser.Write.PropertiveToLua(Alliances[a].Army), Alliances[a].AllianceType));
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				// Units
				Units.SaveUnitsGroup(LuaFile);

				LuaFile.OpenTab(KEY_PLATOONBUILDERS + LuaParser.Write.OpenBracketValue);
				LuaFile.AddLine(LuaParser.Write.StringToLua(PlatoonBuilder.KEY_NEXTPLATOONBUILDERID, "1"));
				LuaFile.OpenTab(PlatoonBuilder.KEY_BUILDERS + LuaParser.Write.OpenBracketValue);
				//TODO
				// Write all PlatoonBuilders
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
			}
		}
	}
}
