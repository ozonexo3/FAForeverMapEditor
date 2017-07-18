using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	[System.Serializable]
	public class ScenarioLua
	{

		public ScenarioInfo Data = new ScenarioInfo();
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
			public const string KEY_EXTRAARMIES = "ExtraArmies";
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
		public bool Load(string FolderName, string ScenarioFileName)
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

			Data.name = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_NAME);
			Data.description = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_DESCRIPTION);
			Data.type = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_TYPE);
			Data.starts = LuaParser.Read.BoolFromTable(ScenarioInfoTab, ScenarioInfo.KEY_STARTS, true);

			Data.map_version = LuaParser.Read.FloatFromTable(ScenarioInfoTab, ScenarioInfo.KEY_MAPVERSION, 1);

			Data.AdaptiveMap = LuaParser.Read.BoolFromTable(ScenarioInfoTab, ScenarioInfo.KEY_ADAPTIVEMAP, false);

			Data.preview = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_PREVIEW, "");
			Data.save = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_SAVE, "");
			Data.map = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_MAP, "");
			Data.script = LuaParser.Read.StringFromTable(ScenarioInfoTab, ScenarioInfo.KEY_SCRIPT, "");
			Data.Size = LuaParser.Read.IntArrayFromTable((LuaTable)ScenarioInfoTab.RawGet(ScenarioInfo.KEY_SIZE), 512);

			Data.MaxHeight = 128;
			//CamControll.MapSize = Mathf.Max(ScenarioData.Size.x, ScenarioData.Size.y);
			//CamControll.RestartCam();

			List<string> AllArmys = new List<string>();

			LuaTable ConfigurationsTable = LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS) ;


			string[] ConfKeys = LuaParser.Read.GetTableKeys(ConfigurationsTable);
			Data.Configurations = new Configuration[ConfKeys.Length];
			for (int Ct = 0; Ct < Data.Configurations.Length; Ct++)
			{
				Data.Configurations[Ct] = new Configuration();
				Data.Configurations[Ct].name = ConfKeys[Ct];

				// Teams
				LuaTable[] TeamsTables = LuaParser.Read.TableArrayFromTable(
					LuaFile.GetTable(TABLE_SCENARIOINFO +"."+ ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_TEAMS)
					);

				Data.Configurations[Ct].Teams = new Team[TeamsTables.Length];
				for (int T = 0; T < Data.Configurations[Ct].Teams.Length; T++)
				{
					Data.Configurations[Ct].Teams[T] = new Team();
					Data.Configurations[Ct].Teams[T].name = LuaParser.Read.StringFromTable(TeamsTables[T], Team.KEY_NAME, "FFA");
					Data.Configurations[Ct].Teams[T].Armys = LuaParser.Read.StringArrayFromTable(TeamsTables[T], Team.KEY_ARMIES);
					AllArmys.AddRange(Data.Configurations[Ct].Teams[T].Armys);
				}

				// Custom Props
				LuaTable CustomPropsTable = LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_CUSTOMPROPS);

				string[] CustomPropsKeys = LuaParser.Read.GetTableKeys(CustomPropsTable);
				string[] CustomPropsValues = LuaParser.Read.GetTableValues(CustomPropsTable);
				Data.Configurations[Ct].customprops = new CustomProps[CustomPropsKeys.Length];
				for (int cp = 0; cp < CustomPropsKeys.Length; cp++)
				{
					Data.Configurations[Ct].customprops[cp] = new CustomProps();
					Data.Configurations[Ct].customprops[cp].key = CustomPropsKeys[cp];
					Data.Configurations[Ct].customprops[cp].value = CustomPropsValues[cp];
				}

				// Factions
				LuaTable[] FactionsTables = LuaParser.Read.TableArrayFromTable(
					LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_FACTIONS)
					);
				Debug.Log("factions: " + FactionsTables.Length);
				Data.Configurations[Ct].factions = new Factions[FactionsTables.Length];
				for(int i = 0; i < Data.Configurations[Ct].factions.Length; i++)
				{
					Data.Configurations[Ct].factions[i] = new Factions();
					Data.Configurations[Ct].factions[i].Values = LuaParser.Read.StringArrayFromTable(FactionsTables[i]);

				}

			}


			//All NoRushOffsets
			Data.norushradius = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_RADIUS, NoRusnOffset.DefaultRadius);
			Data.NoRushOffsets = new NoRusnOffset[AllArmys.Count];
			for(int i = 0; i < AllArmys.Count; i++)
			{
				Data.NoRushOffsets[i] = new NoRusnOffset();
				Data.NoRushOffsets[i].ARMY = AllArmys[i];

				Data.NoRushOffsets[i].X = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_X + AllArmys[i], 0);
				Data.NoRushOffsets[i].Y = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_Y + AllArmys[i], 0);
			}

			return true;
		}
		#endregion

		#region Save
		public void Save(string Path)
		{
			LuaParser.Creator LuaFile = new LuaParser.Creator();
			LuaFile.AddLine("version = 3");


			LuaFile.OpenTab(TABLE_SCENARIOINFO + LuaParser.Write.OpenBracketValue);
			{
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_NAME, Data.name));
				LuaFile.AddLine(LuaParser.Write.DescriptionToLua(ScenarioInfo.KEY_DESCRIPTION, Data.description.Replace("\n", "\\n")));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_PREVIEW, Data.preview));

				LuaFile.AddLine(LuaParser.Write.FloatToLua(ScenarioInfo.KEY_MAPVERSION, Data.map_version));
				if(Data.AdaptiveMap)
					LuaFile.AddLine(LuaParser.Write.BoolToLua(ScenarioInfo.KEY_ADAPTIVEMAP, Data.AdaptiveMap));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_TYPE, Data.type));
				LuaFile.AddLine(LuaParser.Write.BoolToLua(ScenarioInfo.KEY_STARTS, Data.starts));

				LuaFile.AddLine(LuaParser.Write.IntArrayToLua(ScenarioInfo.KEY_SIZE, Data.Size));

				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_MAP, Data.map));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_SAVE, Data.save));
				LuaFile.AddLine(LuaParser.Write.StringToLua(ScenarioInfo.KEY_SCRIPT, Data.script));
				LuaFile.AddLine(LuaParser.Write.FloatToLua(NoRusnOffset.VALUE_RADIUS, Data.norushradius));

				for(int i = 0; i < Data.NoRushOffsets.Length; i++)
				{
					if(Data.NoRushOffsets[i].X > 0)
						LuaFile.AddLine(LuaParser.Write.FloatToLua(Data.NoRushOffsets[i].GetXKey(), Data.NoRushOffsets[i].X));
					if(Data.NoRushOffsets[i].Y > 0)
						LuaFile.AddLine(LuaParser.Write.FloatToLua(Data.NoRushOffsets[i].GetYKey(), Data.NoRushOffsets[i].Y));
				}


				LuaFile.OpenTab(ScenarioInfo.KEY_CONFIGURATIONS + LuaParser.Write.OpenBracketValue);
				{// Configurations
					for (int Cf = 0; Cf < Data.Configurations.Length; Cf++)
					{
						LuaFile.OpenTab(LuaParser.Write.PropertiveToLua(Data.Configurations[Cf].name) + LuaParser.Write.OpenBracketValue);
						{// Configuration Tab

							LuaFile.OpenTab(Configuration.KEY_TEAMS + LuaParser.Write.OpenBracketValue);
							{//Teams

								for (int T = 0; T < Data.Configurations[Cf].Teams.Length; T++)
								{
									LuaFile.OpenTab(LuaParser.Write.OpenBracket);
									{// Team Tab
										LuaFile.AddLine(LuaParser.Write.StringToLua(Team.KEY_NAME, Data.Configurations[Cf].Teams[T].name));
										LuaFile.AddLine(LuaParser.Write.StringArrayToLua(Team.KEY_ARMIES, Data.Configurations[Cf].Teams[T].Armys, false));

									}
									LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
								}


							}
							LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

							LuaFile.OpenTab(Configuration.KEY_CUSTOMPROPS + LuaParser.Write.OpenBracketValue);
							{ // Custom Props
								for (int i = 0; i < Data.Configurations[Cf].customprops.Length; i++)
								{
									LuaFile.AddLine(
										LuaParser.Write.ValueToLua(LuaParser.Write.PropertiveToLua(Data.Configurations[Cf].customprops[i].key), 
										LuaParser.Write.StringFunction(Data.Configurations[Cf].customprops[i].value), 
										(i < Data.Configurations[Cf].customprops.Length - 1)));

								}
							}
							LuaFile.CloseTab(LuaParser.Write.EndBracketNext);


							if(Data.Configurations[Cf].factions.Length > 0)
							{
								LuaFile.OpenTab(Configuration.KEY_FACTIONS + LuaParser.Write.OpenBracketValue);

								for(int i = 0; i < Data.Configurations[Cf].factions.Length; i++)
								{
									bool NotLast = i + 1 < Data.Configurations[Cf].factions.Length;
									LuaFile.AddLine(LuaParser.Write.StringArrayToLua(Data.Configurations[Cf].factions[i].Values, NotLast));

								}

								LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
							}


						}
						LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
					}
				}
				LuaFile.CloseTab(LuaParser.Write.EndBracketNext);



			}
			LuaFile.CloseTab(LuaParser.Write.EndBracket);


			System.IO.File.WriteAllText(Path, LuaFile.GetFileString());
		}
		#endregion
	}
}
