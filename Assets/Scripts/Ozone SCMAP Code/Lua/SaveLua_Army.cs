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
			public string nextPlatoonBuilderId = "";
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
					Name = "Units";
					orders = "";
					platoon = "";

					UnitGroups = new UnitsGroup[0];
					Units = new Unit[0];
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

					for(int u = 0; u < Units.Length; u++)
					{
						GetGamedataFile.LoadUnit(Units[u].type).CreateUnitObject(ScmapEditor.ScmapPosToWorld(Units[u].Position), Quaternion.Euler(Units[u].Orientation));
					}
				}

				public void SaveUnitsGroup(LuaParser.Creator LuaFile)
				{
					LuaFile.OpenTab(LuaParser.Write.PropertieToLua(Name) + LuaParser.Write.SetValue + "GROUP " + LuaParser.Write.OpenBracket);

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
					LuaFile.OpenTab(LuaParser.Write.PropertieToLua(Name) + LuaParser.Write.OpenBracketValue);

					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_TYPE, type));
					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_ORDERS, orders));
					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_PLATOON, platoon));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLua(UnitsGroup.KEY_POSITION, Position));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLua(UnitsGroup.KEY_ORIENTATION, Orientation));

					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				}
			}

			[System.Serializable]
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
				public PlatoonAIFunction PlatoonAiFunctions;
				public BuildCondition[] BuildConditions;
				public PlatoonAddFunction[] PlatoonAddFunctions;
				public PlatoonData[] PlatoonDatas;


				public const string KEY_NEXTPLATOONBUILDERID = "next_platoon_builder_id";
				public const string KEY_BUILDERS = "Builders";
				public const string KEY_PLATOONTEMPLATE = "PlatoonTemplate";
				public const string KEY_PRIORITY = "Priority";
				public const string KEY_INSTANCECOUNT = "InstanceCount";
				public const string KEY_LOCATIONTYPE = "LocationType";
				public const string KEY_BUILDTIMEOUT = "BuildTimeOut";
				public const string KEY_PLATOONTYPE = "PlatoonType";
				public const string KEY_REQUIRESCONSTRUCTION = "RequiresConstruction";
				public const string KEY_PLATOONAIFUNCTION = "PlatoonAIFunction";
				public const string KEY_BUILDCONDITIONS = "BuildConditions";
				public const string KEY_PLATOONADDFUNCTION = "PlatoonAddFunctions";
				public const string KEY_PLATOONDATA = "PlatoonData";



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

				public class PlatoonAddFunction
				{

				}

				public class PlatoonData
				{
					int type;
					string name;
					bool value;
					string stringValue;
				}

				public PlatoonBuilder()
				{

				}

				public PlatoonBuilder(string name, LuaTable Table)
				{
					Name = name;
					PlatoonTemplate = LuaParser.Read.StringFromTable(Table, KEY_PLATOONTEMPLATE);
					Priority = LuaParser.Read.IntFromTable(Table, KEY_PRIORITY);
					InstanceCount = LuaParser.Read.IntFromTable(Table, KEY_INSTANCECOUNT);
					LocationType = LuaParser.Read.StringFromTable(Table, KEY_LOCATIONTYPE);
					BuildTimeOut = LuaParser.Read.IntFromTable(Table, KEY_BUILDTIMEOUT);
					PlatoonType = LuaParser.Read.StringFromTable(Table, KEY_PLATOONTYPE);
					RequiresConstruction = LuaParser.Read.BoolFromTable(Table, KEY_PLATOONTYPE);
				}

				public void SavePlatoonBuilder(LuaParser.Creator LuaFile)
				{
					LuaFile.OpenTab(LuaParser.Write.PropertieToLua(Name) + LuaParser.Write.OpenBracketValue);

					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PLATOONTEMPLATE, PlatoonTemplate));
					LuaFile.AddLine(LuaParser.Write.IntToLua(KEY_PLATOONTEMPLATE, Priority));
					LuaFile.AddLine(LuaParser.Write.IntToLua(KEY_PLATOONTEMPLATE, InstanceCount));
					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PLATOONTEMPLATE, LocationType));
					LuaFile.AddLine(LuaParser.Write.IntToLua(KEY_PLATOONTEMPLATE, BuildTimeOut));
					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PLATOONTEMPLATE, PlatoonType));
					LuaFile.AddLine(LuaParser.Write.BoolToLua(KEY_PLATOONTEMPLATE, RequiresConstruction));

					//TODO
					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
				}
			}


			#endregion

			public Army()
			{
				Name = "";
				personality = "";
				plans = "";
				color = 0;
				faction = 0;
				Economy = new EconomyTab();
				Alliances = new Aliance[0];
				Units = new UnitsGroup();
				nextPlatoonBuilderId = "1";
				PlatoonBuilders = new PlatoonBuilder[0];
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
				LuaTable PbTable = (LuaTable)Table.RawGet(KEY_PLATOONBUILDERS);
				nextPlatoonBuilderId = LuaParser.Read.StringFromTable(PbTable, PlatoonBuilder.KEY_NEXTPLATOONBUILDERID);

				LuaTable BuildersTable = (LuaTable)PbTable.RawGet(PlatoonBuilder.KEY_BUILDERS);
				LuaTable[] PbTabs = LuaParser.Read.GetTableTables(BuildersTable);
				string[] PbNames = LuaParser.Read.GetTableKeys(BuildersTable);
				PlatoonBuilders = new PlatoonBuilder[PbNames.Length];
				for (int c = 0; c < PbNames.Length; c++)
				{
					PlatoonBuilders[c] = new PlatoonBuilder(PbNames[c], PbTabs[c]);
				}

				MapLuaParser.Current.ScenarioLuaFile.AddDataToArmy(this);

			}

			public void SaveArmy(LuaParser.Creator LuaFile, string ArmyName)
			{
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent(ArmyName);
				LuaFile.AddSaveComent("");

				LuaFile.AddLine(LuaParser.Write.PropertieToLua(ArmyName) + LuaParser.Write.SetValue);
				LuaFile.OpenTab(LuaParser.Write.OpenBracket);


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
					LuaFile.AddLine(LuaParser.Write.StringToLua(LuaParser.Write.PropertieToLua(Alliances[a].Army), Alliances[a].AllianceType));
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				// Units
				Units.SaveUnitsGroup(LuaFile);

				//Platoon Builders
				LuaFile.OpenTab(KEY_PLATOONBUILDERS + LuaParser.Write.OpenBracketValue);

				LuaFile.AddLine(LuaParser.Write.StringToLua(PlatoonBuilder.KEY_NEXTPLATOONBUILDERID, "1"));

				LuaFile.OpenTab(PlatoonBuilder.KEY_BUILDERS + LuaParser.Write.OpenBracketValue);
				for (int i = 0; i < PlatoonBuilders.Length; i++)
				{
					PlatoonBuilders[i].SavePlatoonBuilder(LuaFile);
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
			}
		}
	}
}
