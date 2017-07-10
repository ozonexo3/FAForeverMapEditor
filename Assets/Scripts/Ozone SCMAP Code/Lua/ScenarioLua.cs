using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	[System.Serializable]
	public class ScenarioLua
	{

		public ScenarioInfo ScenarioInfoData = new ScenarioInfo();
		Lua LuaFile;


		#region Structure Objects
		[System.Serializable]
		public class ScenarioInfo
		{
			public string name;
			public string description;
			public string type;
			public bool starts;
			public float map_version;
			public bool AdaptiveMap;
			public string preview;
			public string save;
			public string map;
			public string script;
			public int[] Size = new int[0];
			public float MaxHeight = 128;
			public float norushradius = 40;
			public NoRusnOffset[] NoRushOffsets = new NoRusnOffset[0];
			public Configuration[] Configurations = new Configuration[0];
		}

		[System.Serializable]
		public class Configuration
		{
			public string name;
			public Team[] Teams = new Team[0];
			public CustomProps[] customprops = new CustomProps[0];

			public const string VALUE_TEAMS = "teams";
			public const string VALUE_CUSTOMPROPS = "customprops";
			// TODO customprops
		}

		[System.Serializable]
		public class Team
		{
			public string name = "";
			public string[] Armys = new string[0];

			public const string VALUE_NAME = "name";
			public const string VALUE_ARMIES = "armies";

		}

		[System.Serializable]
		public class CustomProps
		{
			public string key;
			public string value;
		}

		[System.Serializable]
		public class NoRusnOffset
		{
			public string ARMY;
			public float X;
			public float Y;

			public const float DefaultRadius = 40;

			public const string VALUE_RADIUS = "norushradius";
			public const string VALUE_X = "norushoffsetX_";
			public const string VALUE_Y = "norushoffsetY_";

			public string GetXKey()
			{
				return VALUE_X + ARMY;
			}

			public string GetYKey()
			{
				return VALUE_Y + ARMY;
			}
		}

		#endregion

		#region Keys
		const string TABLE_SCENARIOINFO = "ScenarioInfo";
		const string TABLE_SCENARIOINFO_NAME = "name";
		const string TABLE_SCENARIOINFO_DESCRIPTION = "description";
		const string TABLE_SCENARIOINFO_TYPE = "type";
		const string TABLE_SCENARIOINFO_STARTS = "starts";
		const string TABLE_SCENARIOINFO_MAPVERSION = "map_version";
		const string TABLE_SCENARIOINFO_ADAPTIVEMAP = "AdaptiveMap";
		const string TABLE_SCENARIOINFO_PREVIEW = "preview";
		const string TABLE_SCENARIOINFO_SAVE = "save";
		const string TABLE_SCENARIOINFO_MAP = "map";
		const string TABLE_SCENARIOINFO_SCRIPT = "script";
		const string TABLE_SCENARIOINFO_SIZE = "size";
		const string TABLE_CONFIGURATIONS = "Configurations";
		#endregion


		#region Load
		public bool Load_ScenarioLua(string FolderName, string ScenarioFileName)
		{
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

			string MapPath = EnvPaths.GetMapsPath();

			string loadedFile = "";
			Debug.Log("Load file:" + MapPath + FolderName + "/" + ScenarioFileName + ".lua");
			string loc = MapPath + FolderName + "/" + ScenarioFileName + ".lua";
			loadedFile = System.IO.File.ReadAllText(loc, encodeType);

			LuaFile = new Lua();
			LuaFile.LoadCLRPackage();
			try
			{
				LuaFile.DoString(MapLuaParser.GetLoadedFileFunctions() + loadedFile);
			}
			catch (NLua.Exceptions.LuaException e)
			{
				Debug.LogError(ParsingStructureData.FormatException(e), MapLuaParser.Current.gameObject);
				//HelperGui.MapLoaded = false;
				return false;
			}

			// Load Map Prop
			LuaTable ScenarioInfoTab = LuaFile.GetTable(TABLE_SCENARIOINFO);

			ScenarioInfoData.name = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_NAME);
			ScenarioInfoData.description = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_DESCRIPTION);
			ScenarioInfoData.type = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_TYPE);
			ScenarioInfoData.starts = LuaParser.Read.BoolFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_STARTS, true);

			ScenarioInfoData.map_version = LuaParser.Read.FloatFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_MAPVERSION, 1);

			ScenarioInfoData.AdaptiveMap = LuaParser.Read.BoolFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_ADAPTIVEMAP, false);

			ScenarioInfoData.preview = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_PREVIEW, "");
			ScenarioInfoData.save = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_SAVE, "");
			ScenarioInfoData.map = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_MAP, "");
			ScenarioInfoData.script = LuaParser.Read.StringFromTable(ScenarioInfoTab, TABLE_SCENARIOINFO_SCRIPT, "");
			ScenarioInfoData.Size = LuaParser.Read.IntArrayFromTable((LuaTable)ScenarioInfoTab.RawGet(TABLE_SCENARIOINFO_SIZE), 512);

			ScenarioInfoData.MaxHeight = 128;
			//CamControll.MapSize = Mathf.Max(ScenarioData.Size.x, ScenarioData.Size.y);
			//CamControll.RestartCam();

			List<string> AllArmys = new List<string>();

			LuaTable ConfigurationsTable = LuaFile.GetTable(TABLE_SCENARIOINFO + "." + TABLE_CONFIGURATIONS) ;


			string[] ConfKeys = LuaParser.Read.GetTableKeys(ConfigurationsTable);
			ScenarioInfoData.Configurations = new Configuration[ConfKeys.Length];
			for (int Ct = 0; Ct < ScenarioInfoData.Configurations.Length; Ct++)
			{
				ScenarioInfoData.Configurations[Ct] = new Configuration();
				ScenarioInfoData.Configurations[Ct].name = ConfKeys[Ct];

				// Teams
				LuaTable[] TeamsTables = LuaParser.Read.TableArrayFromTable(
					LuaFile.GetTable(TABLE_SCENARIOINFO +"."+ TABLE_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.VALUE_TEAMS)
					);


				ScenarioInfoData.Configurations[Ct].Teams = new Team[TeamsTables.Length];
				for (int T = 0; T < ScenarioInfoData.Configurations[Ct].Teams.Length; T++)
				{
					ScenarioInfoData.Configurations[Ct].Teams[T] = new Team();
					ScenarioInfoData.Configurations[Ct].Teams[T].name = LuaParser.Read.StringFromTable(TeamsTables[T], Team.VALUE_NAME, "FFA");
					ScenarioInfoData.Configurations[Ct].Teams[T].Armys = LuaParser.Read.StringArrayFromTable(TeamsTables[T], Team.VALUE_ARMIES);
					AllArmys.AddRange(ScenarioInfoData.Configurations[Ct].Teams[T].Armys);
				}

				// Custom Props
				LuaTable CustomPropsTable = LuaFile.GetTable(TABLE_SCENARIOINFO + "." + TABLE_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.VALUE_CUSTOMPROPS);

				string[] CustomPropsKeys = LuaParser.Read.GetTableKeys(CustomPropsTable);
				string[] CustomPropsValues = LuaParser.Read.GetTableValues(CustomPropsTable);
				ScenarioInfoData.Configurations[Ct].customprops = new CustomProps[CustomPropsKeys.Length];
				for (int cp = 0; cp < CustomPropsKeys.Length; cp++)
				{
					ScenarioInfoData.Configurations[Ct].customprops[cp] = new CustomProps();
					ScenarioInfoData.Configurations[Ct].customprops[cp].key = CustomPropsKeys[cp];
					ScenarioInfoData.Configurations[Ct].customprops[cp].value = CustomPropsValues[cp];

				}
			}


			//All NoRushOffsets
			ScenarioInfoData.norushradius = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_RADIUS, NoRusnOffset.DefaultRadius);
			ScenarioInfoData.NoRushOffsets = new NoRusnOffset[AllArmys.Count];
			for(int i = 0; i < AllArmys.Count; i++)
			{
				ScenarioInfoData.NoRushOffsets[i] = new NoRusnOffset();
				ScenarioInfoData.NoRushOffsets[i].ARMY = AllArmys[i];

				ScenarioInfoData.NoRushOffsets[i].X = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_X + AllArmys[i], 0);
				ScenarioInfoData.NoRushOffsets[i].Y = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_Y + AllArmys[i], 0);
			}

			return true;
		}
		#endregion

		#region Save
		public void Save_ScenarioLua(string Path)
		{
			string LuaFile = "version = 3";
			int Tabs = 0;

			LuaParser.Write.AddLine(TABLE_SCENARIOINFO + LuaParser.Write.OpenBracketValue, Tabs, ref LuaFile);
			{
				Tabs ++;
				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_NAME, ScenarioInfoData.name), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_DESCRIPTION, ScenarioInfoData.description), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.FloatToLua(TABLE_SCENARIOINFO_MAPVERSION, ScenarioInfoData.map_version), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.BoolToLua(TABLE_SCENARIOINFO_ADAPTIVEMAP, ScenarioInfoData.AdaptiveMap), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_TYPE, ScenarioInfoData.type), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.BoolToLua(TABLE_SCENARIOINFO_STARTS, ScenarioInfoData.starts), Tabs, ref LuaFile);

				LuaParser.Write.AddLine(LuaParser.Write.IntArrayToLua(TABLE_SCENARIOINFO_SIZE, ScenarioInfoData.Size), Tabs, ref LuaFile);

				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_MAP, ScenarioInfoData.map), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_SAVE, ScenarioInfoData.save), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_SCRIPT, ScenarioInfoData.script), Tabs, ref LuaFile);
				LuaParser.Write.AddLine(LuaParser.Write.StringToLua(TABLE_SCENARIOINFO_PREVIEW, ScenarioInfoData.preview), Tabs, ref LuaFile);

				LuaParser.Write.AddLine(LuaParser.Write.FloatToLua(NoRusnOffset.VALUE_RADIUS, ScenarioInfoData.norushradius), Tabs, ref LuaFile);

				for(int i = 0; i < ScenarioInfoData.NoRushOffsets.Length; i++)
				{
					LuaParser.Write.AddLine(LuaParser.Write.FloatToLua(ScenarioInfoData.NoRushOffsets[i].GetXKey(), ScenarioInfoData.NoRushOffsets[i].X), Tabs, ref LuaFile);
					LuaParser.Write.AddLine(LuaParser.Write.FloatToLua(ScenarioInfoData.NoRushOffsets[i].GetYKey(), ScenarioInfoData.NoRushOffsets[i].Y), Tabs, ref LuaFile);
				}


				LuaParser.Write.AddLine(TABLE_CONFIGURATIONS + LuaParser.Write.OpenBracketValue, Tabs, ref LuaFile);
				{// Configurations
					Tabs++;
					for(int Cf = 0; Cf < ScenarioInfoData.Configurations.Length; Cf++)
					{
						LuaParser.Write.AddLine(LuaParser.Write.PropertiveToLua(ScenarioInfoData.Configurations[Cf].name) + LuaParser.Write.OpenBracketValue, Tabs, ref LuaFile);
						{// Configuration Tab
							Tabs++;

							LuaParser.Write.AddLine(Configuration.VALUE_TEAMS + LuaParser.Write.OpenBracketValue, Tabs, ref LuaFile);
							{//Teams
								Tabs++;

								for (int T = 0; T < ScenarioInfoData.Configurations[Cf].Teams.Length; T++)
								{
									LuaParser.Write.AddLine(LuaParser.Write.OpenBracket, Tabs, ref LuaFile);
									{// Team Tab
										Tabs++;
										LuaParser.Write.AddLine(LuaParser.Write.StringToLua(Team.VALUE_NAME, ScenarioInfoData.Configurations[Cf].Teams[T].name), Tabs, ref LuaFile);
										LuaParser.Write.AddLine(LuaParser.Write.StringArrayToLua(Team.VALUE_ARMIES, ScenarioInfoData.Configurations[Cf].Teams[T].Armys, false), Tabs, ref LuaFile);

										Tabs--;
									}
									LuaParser.Write.AddLine(LuaParser.Write.EndBracketNext, Tabs, ref LuaFile);
								}


								Tabs--;
							}
							LuaParser.Write.AddLine(LuaParser.Write.EndBracketNext, Tabs, ref LuaFile);

							LuaParser.Write.AddLine(Configuration.VALUE_CUSTOMPROPS + LuaParser.Write.OpenBracketValue, Tabs, ref LuaFile);
							{ // Custom Props
								Tabs++;
								for (int i = 0; i < ScenarioInfoData.Configurations[Cf].customprops.Length; i++)
								{
									LuaParser.Write.AddLine(
										LuaParser.Write.ValueToLua(LuaParser.Write.PropertiveToLua(ScenarioInfoData.Configurations[Cf].customprops[i].key), 
										LuaParser.Write.StringFunction(ScenarioInfoData.Configurations[Cf].customprops[i].value), 
										(i < ScenarioInfoData.Configurations[Cf].customprops.Length - 1))
										, Tabs, ref LuaFile);

								}
								Tabs--;
							}
							LuaParser.Write.AddLine(LuaParser.Write.EndBracketNext, Tabs, ref LuaFile);

							Tabs--;
						}
						LuaParser.Write.AddLine(LuaParser.Write.EndBracketNext, Tabs, ref LuaFile);
					}
					Tabs--;
				}
				LuaParser.Write.AddLine(LuaParser.Write.EndBracketNext, Tabs, ref LuaFile);



				Tabs--;
			}
			LuaParser.Write.AddLine(LuaParser.Write.EndBracket, Tabs, ref LuaFile);


			System.IO.File.WriteAllText(Path, LuaFile);
		}
		#endregion
	}
}
