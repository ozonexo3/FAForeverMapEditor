using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	[System.Serializable]
	public class SaveLua
	{

		public Scenario Data = new Scenario();
		Lua LuaFile;

		#region Area controls
		public int DisplayAreaId = 0;

		public bool HasAreas()
		{
			return Data.areas.Length > 0;
		}

		public Rect GetAreaSize()
		{
			return Data.areas[DisplayAreaId].rectangle;
		}
		#endregion

		#region Structure Objects
		const string KEY_Scenario = "Scenario";

		[System.Serializable]
		public class Scenario
		{
			public int next_area_id = 0;
			public Areas[] areas = new Areas[0];
			public MasterChain[] MasterChains;
			public Chain[] Chains;
			public int next_queue_id = 0;

			public int next_platoon_id = 0;
			public Platoon[] Platoons;

			public int next_army_id = 0;
			public int next_group_id = 0;
			public int next_unit_id = 0;

			public Army[] Armies;

			public const string KEY_NEXTAREAID = "next_area_id";
			public const string KEY_PROPS = "Props";
			public const string KEY_AREAS = "Areas";
			public const string KEY_MASTERCHAIN = "MasterChain";
			public const string KEY_CHAINS = "Chains";
			public const string KEY_NEXTQUEUEID = "next_queue_id";
			public const string KEY_ORDERS = "Orders";
			public const string KEY_NEXTPLATOONID = "next_platoon_id";
			public const string KEY_PLATOONS = "Platoons";
			public const string KEY_NEXTARMYID = "next_army_id";
			public const string KEY_NEXTGROUPID = "next_group_id";
			public const string KEY_NEXTUNITID = "next_unit_id";
			public const string KEY_ARMIES = "Armies";
		}

		[System.Serializable]
		public class Areas
		{
			public string Name;
			public Rect rectangle;


			public const string KEY_RECTANGLE = "rectangle";
		}

		[System.Serializable]
		public class MasterChain
		{
			public string Name = "";
			public Marker[] Markers = new Marker[0];

			public const string KEY_MARKERS = "Markers";
		}

		[System.Serializable]
		public class Chain
		{
			public string Name;
			public string[] Markers;
			public const string KEY_MARKERS = "Markers";
		}


		#region Marker
		[System.Serializable]
		public class Marker
		{
			public string Name = "";
			public MarkerTypes MarkerType;
			public Markers.MarkerObject MarkerObj;
			public float size = 1f;
			public bool resource = false;
			public float amount = 100;
			public string color = "ff808080";
			public string type = "";
			public string prop = "";
			public Vector3 orientation = Vector3.zero;
			public Vector3 position = Vector3.zero;
			public string editorIcon = "";
			public bool hint;

			public string graph = "";
			public string adjacentTo = "";

			public float zoom = 30;
			public bool canSetCamera = true;
			public bool canSyncCamera = true;


			public const string KEY_SIZE = "size";
			public const string KEY_RESOURCE = "resource";
			public const string KEY_AMOUNT = "amount";
			public const string KEY_COLOR = "color";
			public const string KEY_TYPE = "type";
			public const string KEY_PROP = "prop";
			public const string KEY_ORIENTATION = "orientation";
			public const string KEY_POSITION = "position";

			public const string KEY_EDITORICON = "editorIcon";
			public const string KEY_HINT = "hint";

			public const string KEY_ZOOM = "zoom";
			public const string KEY_CANSETCAMERA = "canSetCamera";
			public const string KEY_CANSYNCCAMERA = "canSyncCamera";

			public const string KEY_GRAPH = "graph";
			public const string KEY_ADJACENTTO = "adjacentTo";

			public enum MarkerTypes
			{
				None,
				Mass, Hydrocarbon, BlankMarker, CameraInfo,
				CombatZone,
				DefensivePoint, NavalDefensivePoint,
				ProtectedExperimentalConstruction,
				ExpansionArea, LargeExpansionArea, NavalArea,
				RallyPoint, NavalRallyPoint,
				LandPathNode, AirPathNode, WaterPathNode, AmphibiousPathNode,
				NavalLink,
				TransportMarker,
				Island,
				Count
			}

			string MarkerTypeToString(MarkerTypes MType)
			{
				string str1 = MType.ToString();
				string newstring = "";
				for (int i = 0; i < str1.Length; i++)
				{
					if (char.IsUpper(str1[i]))
						newstring += " ";
					newstring += str1[i].ToString();
				}
				return newstring;
			}

			public bool AllowByType(string Key)
			{
				if (MarkerType == MarkerTypes.Mass)
					return Key == KEY_SIZE || Key == KEY_RESOURCE || Key == KEY_AMOUNT || Key == KEY_EDITORICON;
				else if (MarkerType == MarkerTypes.Hydrocarbon)
					return Key == KEY_SIZE || Key == KEY_RESOURCE || Key == KEY_AMOUNT;
				else if (MarkerType == MarkerTypes.BlankMarker)
					return false;
				else if(MarkerType == MarkerTypes.LandPathNode || MarkerType == MarkerTypes.AirPathNode || MarkerType == MarkerTypes.WaterPathNode || MarkerType == MarkerTypes.AmphibiousPathNode)
					return Key == KEY_HINT || Key == KEY_GRAPH || Key == KEY_ADJACENTTO;
				else if(MarkerType == MarkerTypes.NavalLink)
					return false;
				else if (MarkerType == MarkerTypes.CameraInfo)
					return Key == KEY_ZOOM || Key == KEY_CANSETCAMERA || Key == KEY_CANSYNCCAMERA;
				else //Unknown
					return Key == KEY_HINT;
			}

			public Marker()
			{
			}

			public Marker(string name, LuaTable Table)
			{
				// Create marker from table
				Name = name;
				string[] Keys = LuaParser.Read.GetTableKeys(Table);

				for (int k = 0; k < Keys.Length; k++)
				{
					switch (Keys[k])
					{
						case KEY_POSITION:
							position = LuaParser.Read.Vector3FromTable(Table, KEY_POSITION);
							break;
						case KEY_ORIENTATION:
							orientation = LuaParser.Read.Vector3FromTable(Table, KEY_ORIENTATION);
							break;
						case KEY_SIZE:
							size = LuaParser.Read.FloatFromTable(Table, KEY_SIZE);
							break;
						case KEY_RESOURCE:
							resource = LuaParser.Read.BoolFromTable(Table, KEY_RESOURCE);
							break;
						case KEY_AMOUNT:
							amount = LuaParser.Read.FloatFromTable(Table, KEY_AMOUNT);
							break;
						case KEY_COLOR:
							color = LuaParser.Read.StringFromTable(Table, KEY_COLOR);
							break;
						case KEY_TYPE:
							type = LuaParser.Read.StringFromTable(Table, KEY_TYPE);
							break;
						case KEY_PROP:
							prop = LuaParser.Read.StringFromTable(Table, KEY_PROP);
							break;
						case KEY_EDITORICON:
							editorIcon = LuaParser.Read.StringFromTable(Table, KEY_EDITORICON);
							break;
						case KEY_HINT:
							hint = LuaParser.Read.BoolFromTable(Table, KEY_HINT);
							break;
						case KEY_ZOOM:
							zoom = LuaParser.Read.FloatFromTable(Table, KEY_ZOOM);
							break;
						case KEY_CANSETCAMERA:
							canSetCamera = LuaParser.Read.BoolFromTable(Table, KEY_CANSETCAMERA);
							break;
						case KEY_CANSYNCCAMERA:
							canSyncCamera = LuaParser.Read.BoolFromTable(Table, KEY_CANSYNCCAMERA);
							break;
					}
				}

				if (string.IsNullOrEmpty(type))
					MarkerType = MarkerTypes.BlankMarker;
				else
				{
					MarkerType = (MarkerTypes)System.Enum.Parse(typeof(MarkerTypes), type.Replace(" ", ""));

				}


			}

			public void SaveMarkerValues(LuaParser.Creator LuaFile)
			{
				if (AllowByType(KEY_SIZE))
					LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_SIZE), size));
				if (AllowByType(KEY_RESOURCE))
					LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_RESOURCE), resource));
				if (AllowByType(KEY_AMOUNT))
					LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_AMOUNT), amount));

				LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_COLOR), color));

				if (AllowByType(KEY_EDITORICON))
					LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_EDITORICON), editorIcon));
				if (AllowByType(KEY_HINT))
					LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_HINT), hint));

				//Type
				LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_TYPE), MarkerTypeToString(MarkerType)));
				LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_PROP), prop));

				if (AllowByType(KEY_ZOOM))
					LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_ZOOM), zoom));
				if (AllowByType(KEY_CANSETCAMERA))
					LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_CANSETCAMERA), canSetCamera));
				if (AllowByType(KEY_CANSYNCCAMERA))
					LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_CANSYNCCAMERA), canSyncCamera));

				//Transform
				LuaFile.AddLine(LuaParser.Write.Vector3ToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_POSITION), position));
				LuaFile.AddLine(LuaParser.Write.Vector3ToLuaFunction(LuaParser.Write.PropertiveToLua(KEY_ORIENTATION), orientation));
			}


		}
		#endregion

		#region Platoon
		[System.Serializable]
		public class Platoon
		{
			public string Name;
			public string PlatoonName;
			public string PlatoonFunction;
			public PlatoonAction Action;

			[System.Serializable]
			public class PlatoonAction
			{
				public string Unit;
				public int Id;
				public int count;
				public string Action;
				public string Formation;

				public PlatoonAction() { }

				public PlatoonAction(LuaTable Table)
				{
					object[] Objects = LuaParser.Read.GetTableObjects(Table);
					Unit = Objects[0].ToString();
					Id = LuaParser.Read.StringToInt(Objects[1].ToString());
					count = LuaParser.Read.StringToInt(Objects[2].ToString());
					Action = Objects[3].ToString();
					Formation = Objects[4].ToString();
				}
			}

			public Platoon()
			{
			}

			public Platoon(string name, LuaTable Table)
			{
				Name = name;

				object[] Objects = LuaParser.Read.GetTableObjects(Table);
				PlatoonName = Objects[0].ToString();
				PlatoonFunction = Objects[1].ToString();
				if (Objects.Length > 2)
					Action = new PlatoonAction((LuaTable)Objects[2]);
				else
					Action = new PlatoonAction();

			}
		}
		#endregion

		#region Army
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
				public const string KEY_POSITION = "position";
				public const string KEY_ORIENTATION = "orientation";

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

					for (int g = 0; g < UnitGroups.Length; g++)
					{
						UnitGroups[g].SaveUnitsGroup(LuaFile);
					}
					for (int u = 0; u < UnitGroups.Length; u++)
					{
						Units[u].SaveUnit(LuaFile);
					}
					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
				}
			}

			[System.Serializable]
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
				for(int a = 0; a < AlliancesKeys.Length; a++)
				{
					Alliances[a] = new Aliance();
					Alliances[a].Army = AlliancesKeys[a];
					Alliances[a].AllianceType = AlliancesValues[a];
				}

				LuaTable UnitsTable = (LuaTable)Table.RawGet(KEY_UNITS);
				Units = new UnitsGroup(KEY_UNITS, UnitsTable);
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
				for(int a = 0; a < Alliances.Length; a++)
				{
					LuaFile.AddLine(LuaParser.Write.StringToLua(LuaParser.Write.PropertiveToLua(Alliances[a].Army), Alliances[a].AllianceType));
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				// Units
				Units.SaveUnitsGroup(LuaFile);
				
			}
		}
		#endregion

		#endregion

		public bool Load()
		{
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
			string loadedFileSave = "";
			string MapPath = EnvPaths.GetMapsPath();

			loadedFileSave = System.IO.File.ReadAllText(MapLuaParser.Current.ScenarioLuaFile.Data.save.Replace("/maps/", MapPath), encodeType);

			string loadedFileFunctions = LuaHelper.GetStructureText("lua_variable_functions.lua");
			string loadedFileEndFunctions = LuaHelper.GetStructureText("lua_variable_end_functions.lua");
			loadedFileSave = loadedFileFunctions + loadedFileSave + loadedFileEndFunctions;

			LuaFile = new Lua();
			LuaFile.LoadCLRPackage();

			try
			{
				LuaFile.DoString(loadedFileSave);
			}
			catch (NLua.Exceptions.LuaException e)
			{
				Debug.LogError(ParsingStructureData.FormatException(e), MapLuaParser.Current.gameObject);
				//HelperGui.MapLoaded = false;
				return false;
			}

			Data = new Scenario();
			LuaTable ScenarioInfoTab = LuaFile.GetTable(KEY_Scenario);

			Data.next_area_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTAREAID);


			// Areas
			LuaTable AreasTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_AREAS);
			LuaTable[] AreaTabs = LuaParser.Read.GetTableTables(AreasTable);
			string[] AreaNames = LuaParser.Read.GetTableKeys(AreasTable);

			Data.areas = new Areas[AreaTabs.Length];
			for (int i = 0; i < AreaTabs.Length; i++)
			{
				Data.areas[i] = new Areas();
				Data.areas[i].Name = AreaNames[i];
				Data.areas[i].rectangle = LuaParser.Read.RectFromTable(AreaTabs[i], Areas.KEY_RECTANGLE);
			}

			// Master Chains
			LuaTable MasterChainTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_MASTERCHAIN);
			LuaTable[] MasterChainTabs = LuaParser.Read.GetTableTables(MasterChainTable);
			string[] MasterChainNames = LuaParser.Read.GetTableKeys(MasterChainTable);
			Data.MasterChains = new MasterChain[MasterChainNames.Length];
			for (int mc = 0; mc < MasterChainNames.Length; mc++)
			{
				Data.MasterChains[mc] = new MasterChain();
				Data.MasterChains[mc].Name = MasterChainNames[mc];

				LuaTable MarkersTable = (LuaTable)MasterChainTabs[mc].RawGet(MasterChain.KEY_MARKERS);
				LuaTable[] MarkersTabs = LuaParser.Read.GetTableTables(MarkersTable);
				string[] MarkersNames = LuaParser.Read.GetTableKeys(MarkersTable);
				Data.MasterChains[mc].Markers = new Marker[MarkersTabs.Length];
				for(int m = 0; m < MarkersTabs.Length; m++)
				{
					Data.MasterChains[mc].Markers[m] = new Marker(MarkersNames[m], MarkersTabs[m]);
				}
			}

			Markers.MarkersControler.LoadMarkers();

			// Chains
			LuaTable ChainsTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_CHAINS);
			LuaTable[] ChainTabs = LuaParser.Read.GetTableTables(ChainsTable);
			string[] ChainNames = LuaParser.Read.GetTableKeys(ChainsTable);
			Data.Chains = new Chain[ChainNames.Length];
			for (int c = 0; c < ChainNames.Length; c++)
			{
				Data.Chains[c] = new Chain();
				Data.Chains[c].Name = ChainNames[c];
				Data.Chains[c].Markers = LuaParser.Read.StringArrayFromTable(ChainTabs[c], Chain.KEY_MARKERS);

			}


			Data.next_queue_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTQUEUEID);
			// Orders - leave as empty
			Data.next_platoon_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTPLATOONID);

			// Platoons
			LuaTable PlatoonsTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_PLATOONS);
			LuaTable[] PlatoonsTabs = LuaParser.Read.GetTableTables(PlatoonsTable);
			string[] PlatoonsNames = LuaParser.Read.GetTableKeys(PlatoonsTable);
			Data.Platoons = new Platoon[PlatoonsNames.Length];
			for(int p = 0; p < PlatoonsNames.Length; p++)
			{
				Data.Platoons[p] = new Platoon(PlatoonsNames[p], PlatoonsTabs[p]);
			}


			Data.next_army_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTARMYID);
			Data.next_group_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTGROUPID);
			Data.next_unit_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTUNITID);

			// Armies
			LuaTable ArmiesTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_ARMIES);
			LuaTable[] ArmiesTabs = LuaParser.Read.GetTableTables(ArmiesTable);
			string[] ArmiesNames = LuaParser.Read.GetTableKeys(ArmiesTable);
			Data.Armies = new Army[ArmiesNames.Length];
			for (int a = 0; a < ArmiesNames.Length; a++)
			{
				Data.Armies[a] = new Army(ArmiesNames[a], ArmiesTabs[a]);
			}

			return true;
		}
		
		public void Save(string Path)
		{
			LuaParser.Creator LuaFile = new LuaParser.Creator();

			LuaFile.AddSaveComent("");
			LuaFile.AddSaveComent("Generated by FAF Map Editor");
			LuaFile.AddSaveComent("");

			LuaFile.AddSaveComent("");
			LuaFile.AddSaveComent("Scenario");
			LuaFile.AddSaveComent("");

			LuaFile.AddLine(KEY_Scenario + LuaParser.Write.OpenBracketValue);
			{
				LuaFile.OpenTab();
				LuaFile.AddLine(LuaParser.Write.StringToLua(Scenario.KEY_NEXTAREAID, Data.next_area_id.ToString()));

				// Props
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent("Props");
				LuaFile.AddSaveComent("");
				LuaFile.AddLine(Scenario.KEY_PROPS + LuaParser.Write.OpenBracketValue);
				LuaFile.AddLine(LuaParser.Write.EndBracketNext);

				// Areas
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent("Areas");
				LuaFile.AddSaveComent("");
				LuaFile.OpenTab(Scenario.KEY_AREAS + LuaParser.Write.OpenBracketValue);
				{
					for(int a = 0; a < Data.areas.Length; a++)
					{
						//TODO
						LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Data.areas[a].Name) + LuaParser.Write.OpenBracketValue);
						LuaFile.AddLine(LuaParser.Write.RectangleToLua(LuaParser.Write.PropertiveToLua(Areas.KEY_RECTANGLE), Data.areas[a].rectangle));
						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
					}
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);


				//Markers
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent("Markers");
				LuaFile.AddSaveComent("");
				LuaFile.OpenTab(Scenario.KEY_MASTERCHAIN + LuaParser.Write.OpenBracketValue);
				{
					for(int mc = 0; mc < Data.MasterChains.Length; mc++)
					{
						LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Data.MasterChains[mc].Name) + LuaParser.Write.OpenBracketValue);
						{
							LuaFile.OpenTab(MasterChain.KEY_MARKERS + LuaParser.Write.OpenBracketValue);
							{
								for(int m = 0; m < Data.MasterChains[mc].Markers.Length; m++)
								{
									LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Data.MasterChains[mc].Markers[m].Name) + LuaParser.Write.OpenBracketValue);
									Data.MasterChains[mc].Markers[m].SaveMarkerValues(LuaFile);
									LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
								}
							}
							LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
						}
						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

					}
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				//Chains
				LuaFile.OpenTab(Scenario.KEY_CHAINS + LuaParser.Write.OpenBracketValue);
				{
					for(int c = 0; c < Data.Chains.Length; c++)
					{
						LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Data.Chains[c].Name) + LuaParser.Write.OpenBracketValue);
						LuaFile.OpenTab(Chain.KEY_MARKERS + LuaParser.Write.OpenBracketValue);

						for(int i = 0; i < Data.Chains[c].Markers.Length; i++)
						{
							LuaFile.AddLine("\"" + Data.Chains[c].Markers[i] + "\",");
						}

						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
					}
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				LuaFile.AddLine(LuaParser.Write.StringToLua(Scenario.KEY_NEXTQUEUEID, Data.next_queue_id.ToString()));

				//Orders
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent("Orders");
				LuaFile.AddSaveComent("");
				LuaFile.OpenTab(Scenario.KEY_ORDERS + LuaParser.Write.OpenBracketValue);
				{
					//Leave empty
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

				LuaFile.AddLine(LuaParser.Write.StringToLua(Scenario.KEY_NEXTPLATOONID, Data.next_platoon_id.ToString()));

				//Platoons
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent("Platoons");
				LuaFile.AddSaveComent("");
				LuaFile.OpenTab(Scenario.KEY_ORDERS + LuaParser.Write.OpenBracketValue);
				{
					for (int p = 0; p < Data.Platoons.Length; p++)
					{
						LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Data.Platoons[p].Name) + LuaParser.Write.OpenBracketValue);
						LuaFile.AddLine(LuaParser.Write.Coma + Data.Platoons[p].PlatoonName + LuaParser.Write.Coma + LuaParser.Write.NextValue);
						LuaFile.AddLine(LuaParser.Write.Coma + Data.Platoons[p].PlatoonFunction + LuaParser.Write.Coma + LuaParser.Write.NextValue);

						string Action = LuaParser.Write.OpenBracket;
						Action += LuaParser.Write.Coma + Data.Platoons[p].Action.Unit + LuaParser.Write.Coma + LuaParser.Write.NextValue + " ";
						Action += Data.Platoons[p].Action.Unit.ToString() + LuaParser.Write.NextValue + " ";
						Action += Data.Platoons[p].Action.count.ToString() + LuaParser.Write.NextValue + " ";
						Action += LuaParser.Write.Coma + Data.Platoons[p].Action.Action + LuaParser.Write.Coma + LuaParser.Write.NextValue + " ";
						Action += LuaParser.Write.Coma + Data.Platoons[p].Action.Formation + LuaParser.Write.Coma + LuaParser.Write.NextValue + " ";

						LuaFile.AddLine(Action);

						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
					}
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);


				LuaFile.AddLine(LuaParser.Write.StringToLua(Scenario.KEY_NEXTARMYID, Data.next_army_id.ToString()));
				LuaFile.AddLine(LuaParser.Write.StringToLua(Scenario.KEY_NEXTGROUPID, Data.next_group_id.ToString()));
				LuaFile.AddLine(LuaParser.Write.StringToLua(Scenario.KEY_NEXTUNITID, Data.next_unit_id.ToString()));

				//Armies
				LuaFile.AddSaveComent("");
				LuaFile.AddSaveComent("Armies");
				LuaFile.AddSaveComent("");
				LuaFile.OpenTab(Scenario.KEY_ORDERS + LuaParser.Write.OpenBracketValue);
				{
					for(int a = 0; a < Data.Armies.Length; a++)
					{
						LuaFile.AddSaveComent("");
						LuaFile.AddSaveComent("Army");
						LuaFile.AddSaveComent("");

						LuaFile.AddLine(LuaParser.Write.PropertiveToLua(Data.Armies[a].Name) + LuaParser.Write.SetValue);
						LuaFile.OpenTab(LuaParser.Write.OpenBracket);
						{
							Data.Armies[a].SaveArmy(LuaFile);

						}
						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
					}
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

			}
			LuaFile.CloseTab(LuaParser.Write.EndBracket);



			System.IO.File.WriteAllText(Path, LuaFile.GetFileString());
		}



	}
}
