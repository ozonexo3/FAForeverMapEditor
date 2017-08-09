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
			//public NoRusnOffset[] NoRushOffsets = new NoRusnOffset[0];
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
			public List<Army> ExtraArmys = new List<Army>();
			public Factions[] factions = new Factions[0];

			public void ArmysFromCustomProps(LuaTable Table)
			{
				string[] Names = new string[0];
				for(int i = 0; i < customprops.Length; i++)
				{
					if(customprops[i].key == ScenarioInfo.KEY_EXTRAARMIES)
					{
						Names = customprops[i].value.Split(" ".ToCharArray());
						break;
					}
				}

				for(int i = 0; i < Names.Length; i++)
				{
					Army NewArmy = new Army();
					NewArmy.Name = Names[i];
					NewArmy.NoRush = new NoRusnOffset();
					NewArmy.NoRush.X = LuaParser.Read.FloatFromTable(Table, NoRusnOffset.VALUE_X + NewArmy.Name, 0);
					NewArmy.NoRush.Y = LuaParser.Read.FloatFromTable(Table, NoRusnOffset.VALUE_Y + NewArmy.Name, 0);

					ExtraArmys.Add(NewArmy);
				}
			}

			public const string KEY_TEAMS = "teams";
			public const string KEY_CUSTOMPROPS = "customprops";
			public const string KEY_FACTIONS = "factions";
			// TODO customprops
		}

		[System.Serializable]
		public class Team
		{
			public string name = "";
			//public string[] Armys = new string[0];
			public List<Army> Armys = new List<Army>();

			public void ArmysFromStringArray(string[] Array, LuaTable Table)
			{
				Armys = new List<Army>();
				for (int i = 0; i < Array.Length; i++)
				{
					Army NewArmy = new Army();
					NewArmy.Name = Array[i];
					NewArmy.NoRush = new NoRusnOffset();
					NewArmy.NoRush.X = LuaParser.Read.FloatFromTable(Table, NoRusnOffset.VALUE_X + NewArmy.Name, 0);
					NewArmy.NoRush.Y = LuaParser.Read.FloatFromTable(Table, NoRusnOffset.VALUE_Y + NewArmy.Name, 0);

					Armys.Add(NewArmy);
				}
			}

			public string[] GetArmys()
			{
				string[] ToReturn = new string[Armys.Count];
				for (int i = 0; i < ToReturn.Length; i++)
				{
					ToReturn[i] = Armys[i].Name;
				}
				return ToReturn;
			}

			public const string KEY_NAME = "name";
			public const string KEY_ARMIES = "armies";

		}

		[System.Serializable]
		public class Army
		{
			public string Name;
			public SaveLua.Army Data;
			public NoRusnOffset NoRush;
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
			public float X;
			public float Y;

			public const float DefaultRadius = 40;

			public const string VALUE_RADIUS = "norushradius";
			public const string VALUE_X = "norushoffsetX_";
			public const string VALUE_Y = "norushoffsetY_";

			public string GetXKey(string ARMY)
			{
				return VALUE_X + ARMY;
			}

			public string GetYKey(string ARMY)
			{
				return VALUE_Y + ARMY;
			}
		}

		#endregion

		#region Keys
		const string TABLE_SCENARIOINFO = "ScenarioInfo";

		#endregion

#region Armys
		public bool AddDataToArmy(SaveLua.Army ArmyData)
		{
			for(int c = 0; c < Data.Configurations.Length; c++)
			{
				for(int t = 0; t < Data.Configurations[c].Teams.Length; t++)
				{
					for(int a = 0; a < Data.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if(Data.Configurations[c].Teams[t].Armys[a].Name == ArmyData.Name)
						{
							Data.Configurations[c].Teams[t].Armys[a].Data = ArmyData;
							return true;
						}

					}

				}

				for (int a = 0; a < Data.Configurations[c].ExtraArmys.Count; a++)
				{
					if (Data.Configurations[c].ExtraArmys[a].Name == ArmyData.Name)
					{
						Data.Configurations[c].ExtraArmys[a].Data = ArmyData;
						return true;
					}
				}
			}

			//No army found - Force new army
			if(Data.Configurations.Length > 0 && Data.Configurations[0].Teams.Length > 0)
			{
				Army NewArmy = new Army();
				NewArmy.Name = ArmyData.Name;
				NewArmy.NoRush = new NoRusnOffset();
				NewArmy.Data = ArmyData;

				Data.Configurations[0].Teams[0].Armys.Add(NewArmy);
				return true;

			}

			return false; // Unable to add new army
		}

		public void CheckForEmptyArmy()
		{

			for (int c = 0; c < Data.Configurations.Length; c++)
			{
				for (int t = 0; t < Data.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < Data.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if (Data.Configurations[c].Teams[t].Armys[a].Data == null)
						{
							Debug.Log("Fix army: " + Data.Configurations[c].Teams[t].Armys[a].Name);
							Data.Configurations[c].Teams[t].Armys[a].Data = new SaveLua.Army();
							Data.Configurations[c].Teams[t].Armys[a].Data.Name = Data.Configurations[c].Teams[t].Armys[a].Name;
						}

					}

				}

				for (int a = 0; a < Data.Configurations[c].ExtraArmys.Count; a++)
				{
					if (Data.Configurations[c].ExtraArmys[a].Data == null)
					{
						Debug.Log("Fix army: " + Data.Configurations[c].ExtraArmys[a].Name);
						Data.Configurations[c].ExtraArmys[a].Data = new SaveLua.Army();
						Data.Configurations[c].ExtraArmys[a].Data.Name = Data.Configurations[c].ExtraArmys[a].Name;

					}
				}
			}
		}

		public void SaveArmys(LuaParser.Creator LuaFile)
		{

			for (int c = 0; c < Data.Configurations.Length; c++)
			{
				for (int t = 0; t < Data.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < Data.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if (Data.Configurations[c].Teams[t].Armys[a] != null)
						{

							Data.Configurations[c].Teams[t].Armys[a].Data.SaveArmy(LuaFile, Data.Configurations[c].Teams[t].Armys[a].Name);

						}
					}

				}

				for (int a = 0; a < Data.Configurations[c].ExtraArmys.Count; a++)
				{
					if (Data.Configurations[c].ExtraArmys[a].Data != null)
					{
						Data.Configurations[c].ExtraArmys[a].Data.SaveArmy(LuaFile, Data.Configurations[c].ExtraArmys[a].Name);
					}
				}
			}

			/*for(int a = 0; a < Data.Armies.Length; a++)
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
			}*/

		}


		#endregion

		#region Load
		public bool Load(string FolderName, string ScenarioFileName, string FolderParentPath)
		{
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

			//string MapPath = EnvPaths.GetMapsPath();

			string loadedFile = "";
			Debug.Log("Load file:" + FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua");
			string loc = FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua";
			loadedFile = System.IO.File.ReadAllText(loc, encodeType);

			LuaFile = new Lua();
			LuaFile.LoadCLRPackage();
			try
			{
				LuaFile.DoString(MapLuaParser.GetLoadedFileFunctions() + loadedFile);
			}
			catch (NLua.Exceptions.LuaException e)
			{
				Debug.LogError(LuaParser.Read.FormatException(e), MapLuaParser.Current.gameObject);
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
					//Data.Configurations[Ct].Teams[T].Armys = LuaParser.Read.StringArrayFromTable(TeamsTables[T], Team.KEY_ARMIES);
					string[] ArmyArray = LuaParser.Read.StringArrayFromTable(TeamsTables[T], Team.KEY_ARMIES);
					Data.Configurations[Ct].Teams[T].ArmysFromStringArray(ArmyArray, ScenarioInfoTab);
					AllArmys.AddRange(ArmyArray);
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

				Data.Configurations[Ct].ArmysFromCustomProps(ScenarioInfoTab);

				// Factions
				LuaTable[] FactionsTables = LuaParser.Read.TableArrayFromTable(
					LuaFile.GetTable(TABLE_SCENARIOINFO + "." + ScenarioInfo.KEY_CONFIGURATIONS + "." + ConfKeys[Ct] + "." + Configuration.KEY_FACTIONS)
					);
				Data.Configurations[Ct].factions = new Factions[FactionsTables.Length];

				for(int i = 0; i < Data.Configurations[Ct].factions.Length; i++)
				{
					Data.Configurations[Ct].factions[i] = new Factions();
					Data.Configurations[Ct].factions[i].Values = LuaParser.Read.StringArrayFromTable(FactionsTables[i]);
				}

			}


			//All NoRushOffsets
			Data.norushradius = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_RADIUS, NoRusnOffset.DefaultRadius);
			/*
			Data.NoRushOffsets = new NoRusnOffset[AllArmys.Count];
			for(int i = 0; i < AllArmys.Count; i++)
			{
				Data.NoRushOffsets[i] = new NoRusnOffset();
				Data.NoRushOffsets[i].ARMY = AllArmys[i];

				Data.NoRushOffsets[i].X = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_X + AllArmys[i], 0);
				Data.NoRushOffsets[i].Y = LuaParser.Read.FloatFromTable(ScenarioInfoTab, NoRusnOffset.VALUE_Y + AllArmys[i], 0);
			}
			*/

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


				for(int c = 0; c < Data.Configurations.Length; c++)
				{
					for(int t = 0; t < Data.Configurations[c].Teams.Length; t++)
					{
						for(int a = 0; a < Data.Configurations[c].Teams[t].Armys.Count; a++)
						{
							if (Data.Configurations[c].Teams[t].Armys[a].NoRush.X > 0)
								LuaFile.AddLine(LuaParser.Write.FloatToLua(Data.Configurations[c].Teams[t].Armys[a].NoRush.GetXKey(Data.Configurations[c].Teams[t].Armys[a].Name), Data.Configurations[c].Teams[t].Armys[a].NoRush.X));
							if (Data.Configurations[c].Teams[t].Armys[a].NoRush.Y > 0)
								LuaFile.AddLine(LuaParser.Write.FloatToLua(Data.Configurations[c].Teams[t].Armys[a].NoRush.GetYKey(Data.Configurations[c].Teams[t].Armys[a].Name), Data.Configurations[c].Teams[t].Armys[a].NoRush.Y));
						}
					}
				}

				/*
				for(int i = 0; i < Data.NoRushOffsets.Length; i++)
				{
					if(Data.NoRushOffsets[i].X > 0)
						LuaFile.AddLine(LuaParser.Write.FloatToLua(Data.NoRushOffsets[i].GetXKey(), Data.NoRushOffsets[i].X));
					if(Data.NoRushOffsets[i].Y > 0)
						LuaFile.AddLine(LuaParser.Write.FloatToLua(Data.NoRushOffsets[i].GetYKey(), Data.NoRushOffsets[i].Y));
				}
				*/

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
										LuaFile.AddLine(LuaParser.Write.StringArrayToLua(Team.KEY_ARMIES, Data.Configurations[Cf].Teams[T].GetArmys(), false));

									}
									LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
								}


							}
							LuaFile.CloseTab(LuaParser.Write.EndBracketNext);

							LuaFile.OpenTab(Configuration.KEY_CUSTOMPROPS + LuaParser.Write.OpenBracketValue);
							{ // Custom Props
								int ExtraCount = Data.Configurations[Cf].ExtraArmys.Count;
								if (ExtraCount > 0)
								{
									string ExtraArmyString = "";
									for(int i = 0; i < ExtraCount; i++)
									{
										if (i > 0)
											ExtraArmyString += " ";
										ExtraArmyString += Data.Configurations[Cf].ExtraArmys[i].Name;

									}

									LuaFile.AddLine(
										LuaParser.Write.ValueToLua(LuaParser.Write.PropertiveToLua(ScenarioInfo.KEY_EXTRAARMIES),
										LuaParser.Write.StringFunction(ExtraArmyString),
										(Data.Configurations[Cf].customprops.Length > 0)
										));
								}


								for (int i = 0; i < Data.Configurations[Cf].customprops.Length; i++)
								{
									if (Data.Configurations[Cf].customprops[i].key == ScenarioInfo.KEY_EXTRAARMIES)
										continue;
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
