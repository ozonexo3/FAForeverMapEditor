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
			public string plans = "/lua/ai/OpAI/DefaultBlankPlanlist.lua";
			public int color = 0;
			public int faction = 0;
			public EconomyTab Economy;
			public List<Aliance> Alliances;
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
			public class EconomyTab
			{
				public float mass = 0;
				public float energy = 0;

				public const string KEY_MASS = "mass";
				public const string KEY_ENERGY = "energy";
			}

			public class Aliance
			{
				public string Army = "";
				public Army ConnectedArmy;
				public string AllianceType = "Enemy";
			}

			public enum AllianceTypes
			{
				None, Ally, Enemy
			}


			public Color ArmyColor
			{
				get
				{
					return GetArmyColor(color);
				}
			}


			//[System.Serializable]
			public class UnitsGroup
			{
				public string Name;
				public string Prefix;

				public string orders;
				public string platoon;
				public HashSet<UnitsGroup> UnitGroups;
				public HashSet<Unit> Units;

				#region Editor Values
				public bool Expanded;
				public bool IsWreckage = false;
				#endregion

				#region Parents
				public Army Owner;
				public UnitsGroup Parent;
				public UnitListObject Instance;
				public void AddGroup(UnitsGroup ug)
				{
					ug.Owner = Owner;
					ug.Parent = this;
					UnitGroups.Add(ug);
				}
				public void RemoveGroup(UnitsGroup ug)
				{
					ug.Owner = null;
					ug.Parent = null;
					UnitGroups.Remove(ug);
				}
				public void AddUnit(Unit u)
				{
					if(u.Parent != null && u.Parent != this)
					{
						// Already used by other group!
						u.Parent.RemoveUnit(u);
					}

					u.Parent = this;
					u.Owner = Owner;
					Units.Add(u);
				}
				public void RemoveUnit(Unit u)
				{
					if (u.Parent == this)
					{
						u.Parent = null;
						u.Owner = null;
					}
					Units.Remove(u);
				}
				public void ClearUnitInstances()
				{

				}
				#endregion

				public const string KEY_PREFIX = "prefix";
				public const string KEY_ORDERS = "orders";
				public const string KEY_PLATOON = "platoon";
				public const string KEY_UNITS = "Units";
				public const string KEY_TYPE = "type";
				public const string KEY_POSITION = "Position";
				public const string KEY_ORIENTATION = "Orientation";


				public string NoPrefixName
				{
					get
					{
						if (string.IsNullOrEmpty(Prefix))
							return Name;

						string ToReturn = Name.Remove(0, Prefix.Length);

						if (ToReturn.StartsWith("_"))
							ToReturn = ToReturn.Remove(0, 1);

						return ToReturn;
					}
					set
					{
						if (string.IsNullOrEmpty(Prefix))
						{
							Prefix = "";
							Name = value;
						}
						else
							Name = Prefix + "_" + value;

						UpdateGroupTags();
					}
				}

				public string PrefixName
				{
					get
					{
						return Prefix;
					}
					set
					{
						if (string.IsNullOrEmpty(value))
						{
							Name = NoPrefixName;
							Prefix = "";
						}
						else
						{
							Name = value + "_" + NoPrefixName;
							Prefix = value;
						}
						UpdateGroupTags();
					}
				}

				public void UpdateGroupTags()
				{
					bool ShouldBeWreckage = NoPrefixName.ToUpper().Contains("WRECKAGE");

					if (IsWreckage == ShouldBeWreckage)
						return;

					IsWreckage = ShouldBeWreckage;

					if (Units != null)
					foreach (Unit u in Units) {
						if (u.Instance)
							u.Instance.IsWreckage = IsWreckage?1:0;
					}
				}

				public UnitsGroup(Army Owner, string Name = "Units")
				{
					this.Owner = Owner;
					this.Name = Name;
					orders = "";
					platoon = "";

					UnitGroups = new HashSet<UnitsGroup>();
					Units = new HashSet<Unit>();
				}

				public UnitsGroup(string name, LuaTable Table, Army Owner, bool StoreForLaterInstantiate = false)
				{
					this.Owner = Owner;

					Name = name;

					Prefix = LuaParser.Read.StringFromTable(Table, KEY_PREFIX);

					orders = LuaParser.Read.StringFromTable(Table, KEY_ORDERS);
					platoon = LuaParser.Read.StringFromTable(Table, KEY_PLATOON);

					UpdateGroupTags();

					UnitGroups = new HashSet<UnitsGroup>();
					Units = new HashSet<Unit>();

					LuaTable[] UnitsTables = LuaParser.Read.GetTableTables((LuaTable)Table.RawGet(KEY_UNITS));
					string[] UnitsNames = LuaParser.Read.GetTableKeys((LuaTable)Table.RawGet(KEY_UNITS));


					if (UnitsNames.Length > 0)
					{
						for (int i = 0; i < UnitsNames.Length; i++)
						{
							if (LuaParser.Read.ValueExist(UnitsTables[i], KEY_TYPE))
							{
								Unit NewUnit = new Unit();
								NewUnit.Name = UnitsNames[i];
								NewUnit.type = LuaParser.Read.StringFromTable(UnitsTables[i], KEY_TYPE);
								NewUnit.orders = LuaParser.Read.StringFromTable(UnitsTables[i], KEY_ORDERS);
								NewUnit.platoon = LuaParser.Read.StringFromTable(UnitsTables[i], KEY_PLATOON);
								NewUnit.Position = LuaParser.Read.Vector3FromTable(UnitsTables[i], KEY_POSITION);
								NewUnit.Orientation = LuaParser.Read.Vector3FromTable(UnitsTables[i], KEY_ORIENTATION);
								AddUnit(NewUnit);
							}
							else
							{
								UnitsGroup NewUnitsGroup = new UnitsGroup(UnitsNames[i], UnitsTables[i], Owner, StoreForLaterInstantiate);
								AddGroup(NewUnitsGroup);
							}

						}

					}
					else
					{
						UnitGroups = new HashSet<UnitsGroup>();
						Units = new HashSet<Unit>();
					}



					if (StoreForLaterInstantiate)
					{
						InstantiateGroupLater(false);
					}
					else
						InstantiateGroup(false);
				}

				public void SaveUnitsGroup(LuaParser.Creator LuaFile)
				{
					LuaFile.OpenTab(LuaParser.Write.PropertieToLua(Name) + LuaParser.Write.SetValue + "GROUP " + LuaParser.Write.OpenBracket);

					if (!string.IsNullOrEmpty(Prefix))
						LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PREFIX, Prefix));

					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_ORDERS, orders));
					LuaFile.AddLine(LuaParser.Write.StringToLua(KEY_PLATOON, platoon));


					LuaFile.OpenTab(KEY_UNITS + LuaParser.Write.OpenBracketValue);
					foreach (UnitsGroup g in UnitGroups)
					{
						g.SaveUnitsGroup(LuaFile);
					}

					foreach (Unit u in Units)
					{
						Unit.SaveUnit(LuaFile, u.Instance);
					}


					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
				}

				public void ClearGroup(bool childs)
				{
					foreach(Unit u in Units)
					{
						u.ClearInstance();
					}

					foreach (UnitsGroup ug in UnitGroups)
						ug.ClearGroup(childs);
				}

				public void InstantiateGroup(bool childs)
				{
					foreach (Unit u in Units)
					{
						u.Instantiate();
					}

					if(childs)
					foreach (UnitsGroup ug in UnitGroups)
						ug.InstantiateGroup(childs);
				}

				public void InstantiateGroupLater(bool childs)
				{

					foreach (Unit u in Units)
					{
						//u.Instantiate(this);
						if (!UnitsToLoad.Contains(u))
						{
							//u.Parent = this;
							UnitsToLoad.Add(u);
						}
					}

					if (childs)
						foreach (UnitsGroup ug in UnitGroups)
							ug.InstantiateGroupLater(childs);
				}


				public void UpdateGroupArmy(Army SourceOwner)
				{
					Owner = SourceOwner;

					foreach (Unit u in Units)
					{
						u.Instance.ArmyColor = SourceOwner.ArmyColor;
					}

					foreach (UnitsGroup ug in UnitGroups)
						ug.UpdateGroupArmy(SourceOwner);
				}


				public void GetAllUnitInstances(ref List<UnitInstance> AllUnits)
				{
					if (Units.Count > 0)
					{
						var ListEnum = Units.GetEnumerator();
						while (ListEnum.MoveNext())
						{
							if (ListEnum.Current.Instance != null)
								AllUnits.Add(ListEnum.Current.Instance);
						}
						ListEnum.Dispose();
					}

					if(UnitGroups.Count > 0)
					{
						var ListEnum = UnitGroups.GetEnumerator();
						while (ListEnum.MoveNext())
						{
							ListEnum.Current?.GetAllUnitInstances(ref AllUnits);
						}
						ListEnum.Dispose();
					}
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
				public UnitInstance Instance;
				public Army Owner;
				public UnitsGroup Parent;


				static HashSet<string> AllNames = new HashSet<string>();
				public static string GetFreeName(string Tag)
				{
					int Id = 0;
					while(AllNames.Contains(Tag + Id))
					{
						Id++;
					}

					return Tag + Id;
				}

				public static bool NameExist(string NameToCheck)
				{
					return AllNames.Contains(NameToCheck);
				}

				public static bool ReplaceName(string oldName, string newName)
				{
					AllNames.Add(newName);

					if (AllNames.Contains(oldName))
					{
						AllNames.Remove(oldName);
						return true;
					}
					return false;
				}

				public static void SaveUnit(LuaParser.Creator LuaFile, UnitInstance Instance)
				{
					LuaFile.OpenTab(LuaParser.Write.PropertieToLua(Instance.Owner.Name) + LuaParser.Write.OpenBracketValue);

					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_TYPE, Instance.UnitRenderer.BP.CodeName.ToLower()));
					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_ORDERS, Instance.orders));
					LuaFile.AddLine(LuaParser.Write.StringToLua(UnitsGroup.KEY_PLATOON, Instance.platoon));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLua(UnitsGroup.KEY_POSITION, ScmapEditor.WorldPosToScmap(Instance.transform.localPosition)));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLua(UnitsGroup.KEY_ORIENTATION, Instance.GetScmapRotation()));

					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				}


				public void ClearInstance()
				{
					if (Instance && Instance.gameObject)
					{
						Object.Destroy(Instance.gameObject);
						AllNames.Remove(Name);
					}
				}

				public void Instantiate()
				{
					if (Instance && Instance.gameObject)
						return;
					UnitInstance ui = GetGamedataFile.LoadUnit(type).CreateUnitObject(this, Parent);

					ui.IsWreckage = Parent.IsWreckage ? 1 : 0;
					if (AllNames.Contains(Name))
					{
						Name = GetFreeName("UNIT_");
					}
					AllNames.Add(Name);

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

			public Army(bool Initial = false)
			{
				Name = "";
				personality = "";
				if(MapLuaParser.Current == null || MapLuaParser.Current.ScenarioLuaFile.Data == null)
					plans = "";
				else if (MapLuaParser.Current.ScenarioLuaFile.Data.type == "campaign_coop" || MapLuaParser.Current.ScenarioLuaFile.Data.type == "campaign")
					plans = "/lua/ai/OpAI/DefaultBlankPlanlist.lua";
				else
					plans = "";
				color = 0;
				faction = 0;
				Economy = new EconomyTab();
				Alliances = new List<Aliance>();
				Units = new UnitsGroup(this);
				nextPlatoonBuilderId = "1";
				PlatoonBuilders = new PlatoonBuilder[0];

				if (Initial)
				{
					Units.AddGroup(new UnitsGroup(this, "INITIAL"));
				}
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

				Alliances = new List<Aliance>();
				for (int a = 0; a < AlliancesKeys.Length; a++)
				{
					Aliance ala = new Aliance();
					ala.Army = AlliancesKeys[a];
					ala.AllianceType = AlliancesValues[a];
					Alliances.Add(ala);
				}

				LuaTable UnitsTable = (LuaTable)Table.RawGet(KEY_UNITS);
				Units = new UnitsGroup(KEY_UNITS, UnitsTable, this, true);

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
				for (int a = 0; a < Alliances.Count; a++)
				{
					if (Alliances[a].ConnectedArmy == null)
						continue;

					LuaFile.AddLine(LuaParser.Write.StringToLua(LuaParser.Write.PropertieToLua(Alliances[a].ConnectedArmy.Name), Alliances[a].AllianceType));
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
