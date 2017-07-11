using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	[System.Serializable]
	public class SaveLua
	{

		public Scenario ScenarioInfoData = new Scenario();
		Lua LuaFile;


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
			public string[] Markers;
			public const string KEY_MARKERS = "Markers";
		}


		#region Marker
		[System.Serializable]
		public class Marker
		{
			public string Name = "";

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

			public enum MarkerType
			{
				Mass, Hydrocarbon, BlankMarker, CameraInfo, RallyPoint
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

			}
		}
		#endregion


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
			}

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

			}
		}


		#endregion



		public bool Load_SaveLua()
		{
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;
			string loadedFileSave = "";
			string MapPath = EnvPaths.GetMapsPath();

			loadedFileSave = System.IO.File.ReadAllText(MapLuaParser.Current.ScenarioLuaFile.ScenarioInfoData.save.Replace("/maps/", MapPath), encodeType);

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

			ScenarioInfoData = new Scenario();
			LuaTable ScenarioInfoTab = LuaFile.GetTable(KEY_Scenario);

			ScenarioInfoData.next_area_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTAREAID);


			// Areas
			LuaTable AreasTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_AREAS);
			LuaTable[] AreaTabs = LuaParser.Read.GetTableTables(AreasTable);
			string[] AreaNames = LuaParser.Read.GetTableKeys(AreasTable);

			ScenarioInfoData.areas = new Areas[AreaTabs.Length];
			for (int i = 0; i < AreaTabs.Length; i++)
			{
				ScenarioInfoData.areas[i] = new Areas();
				ScenarioInfoData.areas[i].Name = AreaNames[i];
				ScenarioInfoData.areas[i].rectangle = LuaParser.Read.RectFromTable(AreaTabs[i], Areas.KEY_RECTANGLE);
			}

			// Master Chains
			LuaTable MasterChainTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_MASTERCHAIN);
			LuaTable[] MasterChainTabs = LuaParser.Read.GetTableTables(MasterChainTable);
			string[] MasterChainNames = LuaParser.Read.GetTableKeys(MasterChainTable);
			ScenarioInfoData.MasterChains = new MasterChain[MasterChainNames.Length];
			for (int mc = 0; mc < MasterChainNames.Length; mc++)
			{
				ScenarioInfoData.MasterChains[mc] = new MasterChain();
				ScenarioInfoData.MasterChains[mc].Name = MasterChainNames[mc];

				LuaTable MarkersTable = (LuaTable)MasterChainTabs[mc].RawGet(MasterChain.KEY_MARKERS);
				LuaTable[] MarkersTabs = LuaParser.Read.GetTableTables(MarkersTable);
				string[] MarkersNames = LuaParser.Read.GetTableKeys(MarkersTable);
				ScenarioInfoData.MasterChains[mc].Markers = new Marker[MarkersTabs.Length];
				for(int m = 0; m < MarkersTabs.Length; m++)
				{
					ScenarioInfoData.MasterChains[mc].Markers[m] = new Marker(MarkersNames[m], MarkersTabs[m]);
				}
			}

			// Chains
			LuaTable ChainsTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_CHAINS);
			LuaTable[] ChainTabs = LuaParser.Read.GetTableTables(ChainsTable);
			string[] ChainNames = LuaParser.Read.GetTableKeys(ChainsTable);
			ScenarioInfoData.Chains = new Chain[ChainNames.Length];
			for (int c = 0; c < ChainNames.Length; c++)
			{
				ScenarioInfoData.Chains[c] = new Chain();
				ScenarioInfoData.Chains[c].Markers = LuaParser.Read.StringArrayFromTable(ChainTabs[c], Chain.KEY_MARKERS);

			}


			ScenarioInfoData.next_queue_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTQUEUEID);
			// Orders - leave as empty
			ScenarioInfoData.next_platoon_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTPLATOONID);

			// Platoons
			LuaTable PlatoonsTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_PLATOONS);
			LuaTable[] PlatoonsTabs = LuaParser.Read.GetTableTables(PlatoonsTable);
			string[] PlatoonsNames = LuaParser.Read.GetTableKeys(PlatoonsTable);
			ScenarioInfoData.Platoons = new Platoon[PlatoonsNames.Length];
			for(int p = 0; p < PlatoonsNames.Length; p++)
			{
				ScenarioInfoData.Platoons[p] = new Platoon(PlatoonsNames[p], PlatoonsTabs[p]);
			}


			ScenarioInfoData.next_army_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTARMYID);
			ScenarioInfoData.next_group_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTGROUPID);
			ScenarioInfoData.next_unit_id = LuaParser.Read.IntFromTable(ScenarioInfoTab, Scenario.KEY_NEXTUNITID);

			// Armies
			LuaTable ArmiesTable = (LuaTable)ScenarioInfoTab.RawGet(Scenario.KEY_ARMIES);
			LuaTable[] ArmiesTabs = LuaParser.Read.GetTableTables(ArmiesTable);
			string[] ArmiesNames = LuaParser.Read.GetTableKeys(ArmiesTable);
			ScenarioInfoData.Armies = new Army[ArmiesNames.Length];
			for (int a = 0; a < ArmiesNames.Length; a++)
			{
				ScenarioInfoData.Armies[a] = new Army(ArmiesNames[a], ArmiesTabs[a]);
			}

			return true;
		}
		
		public void Save_SaveLua(string Path)
		{


		}

	}
}
