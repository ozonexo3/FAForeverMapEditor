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

			public const string KEY_NAME = "name";
			public const string KEY_DESCRIPTION = "description";
			public const string KEY_TYPE = "type";
			public const string KEY_STARTS = "starts";
			public const string KEY_MAPVERSION = "map_version";
			public const string KEY_ADAPTIVEMAP = "AdaptiveMap";
			public const string KEY_PREVIEW = "preview";
			public const string KEY_SAVE = "save";
			public const string KEY_MAP = "map";
			public const string KEY_SCRIPT = "script";
			public const string KEY_SIZE = "size";
			public const string KEY_CONFIGURATIONS = "Configurations";
		}

		[System.Serializable]
		public class Configuration
		{
			public string name;
			public Team[] Teams = new Team[0];
			public CustomProps[] customprops = new CustomProps[0];
			public Factions[] factions = new Factions[0];

			public const string KEY_TEAMS = "teams";
			public const string KEY_CUSTOMPROPS = "customprops";
			public const string KEY_FACTIONS = "factions";
			// TODO customprops
		}

		[System.Serializable]
		public class Team
		{
			public string name = "";
			public string[] Armys = new string[0];

			public const string KEY_NAME = "name";
			public const string KEY_ARMIES = "armies";

		}

		[System.Serializable]
		public class CustomProps
		{
			public string key;
			public string value;
		}

		[System.Serializable]
		public class Factions
		{
			public string[] Values;
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

			ScenarioInfoData.name = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_NAME);
			ScenarioInfoData.description = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_DESCRIPTION);
			ScenarioInfoData.type = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_TYPE);
			ScenarioInfoData.starts = LuaParser.Read.BoolFromTable(ScenarioInfoTab, ScenarioInfo.KEY_STARTS, true);

			ScenarioInfoData.map_version = LuaParser.Read.FloatFromTable(ScenarioInfoTab, ScenarioInfo.KEY_MAPVERSION, 1);

			ScenarioInfoData.AdaptiveMap = LuaParser.Read.BoolFromTable(ScenarioInfoTab, ScenarioInfo.KEY_ADAPTIVEMAP, false);

			ScenarioInfoData.preview = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_PREVIEW, "");
			ScenarioInfoData.save = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_SAVE, "");
			ScenarioInfoData.map = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_MAP, "");
			ScenarioInfoData.script = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_SCRIPT, "");
			ScenarioInfoData.Size = LuaParser.Read.IntArrayFromTable((LuaTable)ScenarioInfoTab.RawGet(ScenarioInfo.KEY_SIZE), 512);

			ScenarioInfoData.MaxHeight = 128;
			//CamControll.MapSize = Mathf.Max(ScenarioData.Size.x, ScenarioData.Size.y);
			//CamControll.RestartCam();

			List<string> AllArmys = new List<string>();

			LuaTable ConfigurationsTable = LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS) ;


			string[] ConfKeys = LuaParser.Read.GetTableKeys(ConfigurationsTable);
			ScenarioInfoData.Configurations = new Configuration[ConfKeys.Length];
			for (int Ct = 0; Ct < ScenarioInfoData.Configurations.Length; Ct++)
			{
				ScenarioInfoData.Configurations[Ct] = new Configuration();
				ScenarioInfoData.Configurations[Ct].name = ConfKeys[Ct];

				// Teams
				LuaTable[] TeamsTables = LuaParser.Read.TableArrayFromTable(
					LuaFile.GetTable(TABLE_SCENARIOINFO +"."+ ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_TEAMS)
					);

				ScenarioInfoData.Configurations[Ct].Teams = new Team[TeamsTables.Length];
				for (int T = 0; T < ScenarioInfoData.Configurations[Ct].Teams.Length; T++)
				{
					ScenarioInfoData.Configurations[Ct].Teams[T] = new Team();
					ScenarioInfoData.Configurations[Ct].Teams[T].name = LuaParser.Read.StringFromTable(TeamsTables[T], Team.KEY_NAME, "FFA");
					ScenarioInfoData.Configurations[Ct].Teams[T].Armys = LuaParser.Read.StringArrayFromTable(TeamsTables[T], Team.KEY_ARMIES);
					AllArmys.AddRange(ScenarioInfoData.Configurations[Ct].Teams[T].Armys);
				}

				// Custom Props
				LuaTable CustomPropsTable = LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_CUSTOMPROPS);

				string[] CustomPropsKeys = LuaParser.Read.GetTableKeys(CustomPropsTable);
				string[] CustomPropsValues = LuaParser.Read.GetTableValues(CustomPropsTable);
				ScenarioInfoData.Configurations[Ct].customprops = new CustomProps[CustomPropsKeys.Length];
				for (int cp = 0; cp < CustomPropsKeys.Length; cp++)
				{
					ScenarioInfoData.Configurations[Ct].customprops[cp] = new CustomProps();
					ScenarioInfoData.Configurations[Ct].customprops[cp].key = CustomPropsKeys[cp];
					ScenarioInfoData.Configurations[Ct].customprops[cp].value = CustomPropsValues[cp];
				}

				// Factions
				LuaTable[] FactionsTables = LuaParser.Read.TableArrayFromTable(
					LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_FACTIONS)
					);
				Debug.Log("factions: " + FactionsTables.Length);
				ScenarioInfoData.Configurations[Ct].factions = new Factions[FactionsTables.Length];
				for(int i = 0; i < ScenarioInfoData.Configurations[Ct].factions.Length; i++)
				{
					ScenarioInfoData.Configurations[Ct].factions[i] = new Factions();
					ScenarioInfoData.Configurations[Ct].factions[i].Values = LuaParser.Read.StringArrayFromTable(FactionsTables[i]);

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
			LuaParser.Creator LuaFile = new LuaParser.Creator();
			LuaFile.AddLine("version = 3");


			LuaFile.AddLine(TABLE_SCENARIOINFO + LuaParser.Write.OpenBracketValue);
			{
				LuaFile.OpenTab();
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_NAME, ScenarioInfoData.name));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_DESCRIPTION, ScenarioInfoData.description.Replace("\n", "\\n")));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_PREVIEW, ScenarioInfoData.preview));

				LuaFile.AddLine(LuaParser.Write.FloatToLua(ScenarioInfo.KEY_MAPVERSION, ScenarioInfoData.map_version));
				if(ScenarioInfoData.AdaptiveMap)
					LuaFile.AddLine(LuaParser.Write.BoolToLua(ScenarioInfo.KEY_ADAPTIVEMAP, ScenarioInfoData.AdaptiveMap));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_TYPE, ScenarioInfoData.type));
				LuaFile.AddLine(LuaParser.Write.BoolToLua(ScenarioInfo.KEY_STARTS, ScenarioInfoData.starts));

				LuaFile.AddLine(LuaParser.Write.IntArrayToLua(ScenarioInfo.KEY_SIZE, ScenarioInfoData.Size));

				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_MAP, ScenarioInfoData.map));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_SAVE, ScenarioInfoData.save));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_SCRIPT, ScenarioInfoData.script));
				LuaFile.AddLine(LuaParser.Write.FloatToLua(NoRusnOffset.VALUE_RADIUS, ScenarioInfoData.norushradius));

				for(int i = 0; i < ScenarioInfoData.NoRushOffsets.Length; i++)
				{
					if(ScenarioInfoData.NoRushOffsets[i].X > 0)
						LuaFile.AddLine(LuaParser.Write.FloatToLua(ScenarioInfoData.NoRushOffsets[i].GetXKey(), ScenarioInfoData.NoRushOffsets[i].X));
					if(ScenarioInfoData.NoRushOffsets[i].Y > 0)
						LuaFile.AddLine(LuaParser.Write.FloatToLua(ScenarioInfoData.NoRushOffsets[i].GetYKey(), ScenarioInfoData.NoRushOffsets[i].Y));
				}


				LuaFile.AddLine(ScenarioInfo.KEY_CONFIGURATIONS + LuaParser.Write.OpenBracketValue);
				{// Configurations
					LuaFile.OpenTab();
					for (int Cf = 0; Cf < ScenarioInfoData.Configurations.Length; Cf++)
					{
						LuaFile.AddLine(LuaParser.Write.PropertiveToLua(ScenarioInfoData.Configurations[Cf].name) + LuaParser.Write.OpenBracketValue);
						{// Configuration Tab
							LuaFile.OpenTab();

							LuaFile.AddLine(Configuration.KEY_TEAMS + LuaParser.Write.OpenBracketValue);
							{//Teams
								LuaFile.OpenTab();

								for (int T = 0; T < ScenarioInfoData.Configurations[Cf].Teams.Length; T++)
								{
									LuaFile.AddLine(LuaParser.Write.OpenBracket);
									{// Team Tab
										LuaFile.OpenTab();
										LuaFile.AddLine(LuaParser.Write.StringToLua(Team.KEY_NAME, ScenarioInfoData.Configurations[Cf].Teams[T].name));
										LuaFile.AddLine(LuaParser.Write.StringArrayToLua(Team.KEY_ARMIES, ScenarioInfoData.Configurations[Cf].Teams[T].Armys, false));

										LuaFile.CloseTab();
									}
									LuaFile.AddLine(LuaParser.Write.EndBracketNext);
								}


								LuaFile.CloseTab();
							}
							LuaFile.AddLine(LuaParser.Write.EndBracketNext);

							LuaFile.AddLine(Configuration.KEY_CUSTOMPROPS + LuaParser.Write.OpenBracketValue);
							{ // Custom Props
								LuaFile.OpenTab();
								for (int i = 0; i < ScenarioInfoData.Configurations[Cf].customprops.Length; i++)
								{
									LuaFile.AddLine(
										LuaParser.Write.ValueToLua(LuaParser.Write.PropertiveToLua(ScenarioInfoData.Configurations[Cf].customprops[i].key), 
										LuaParser.Write.StringFunction(ScenarioInfoData.Configurations[Cf].customprops[i].value), 
										(i < ScenarioInfoData.Configurations[Cf].customprops.Length - 1)));

								}
								LuaFile.CloseTab();
							}
							LuaFile.AddLine(LuaParser.Write.EndBracketNext);


							if(ScenarioInfoData.Configurations[Cf].factions.Length > 0)
							{
								LuaFile.AddLine(Configuration.KEY_FACTIONS + LuaParser.Write.OpenBracketValue);
								LuaFile.OpenTab();

								for(int i = 0; i < ScenarioInfoData.Configurations[Cf].factions.Length; i++)
								{
									bool NotLast = i + 1 < ScenarioInfoData.Configurations[Cf].factions.Length;
									LuaFile.AddLine(LuaParser.Write.StringArrayToLua(ScenarioInfoData.Configurations[Cf].factions[i].Values, NotLast));

								}

								LuaFile.CloseTab();
								LuaFile.AddLine(LuaParser.Write.EndBracketNext);
							}


							LuaFile.CloseTab();
						}
						LuaFile.AddLine(LuaParser.Write.EndBracketNext);
					}
					LuaFile.CloseTab();
				}
				LuaFile.AddLine(LuaParser.Write.EndBracketNext);



				LuaFile.CloseTab();
			}
			LuaFile.AddLine(LuaParser.Write.EndBracket);


			System.IO.File.WriteAllText(Path, LuaFile.GetFileString());
		}
		#endregion
	}
}
