// ******************************************************************************
//
// * Tables.lua Class - Adaptive maps code made by CookieNoob
// * Can be loaded from LUA and saved as LUA using LuaParser
// * Parsing is done by hand, because I can't find good parser that will convert LUA to Class
// * Copyright ozonexo3 2017
//
// ******************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;
using Markers;

namespace MapLua
{
	[System.Serializable]
	public class TablesLua
	{
		public bool IsLoaded;
		public TablesInfo Data = new TablesInfo();
		Lua LuaFile;


		#region Structure Objects
		[System.Serializable]
		public class TablesInfo
		{
			// Core Marker to Army
			public MexArray[] spawnMexArmy = new MexArray[5];
			public MexArray[] spawnHydroArmy = new MexArray[5];

			public List<TableKey> AllTables;
		}



		[System.Serializable]
		public struct MexArray
		{
			public string[] MexesIds;

			public MexArray(string[] MexesIds)
			{
				this.MexesIds = MexesIds;
			}
		}

		[System.Serializable]
		public struct TableKey
		{
			public string Key;
			public bool OneDimension;
			public MexArray[] Values;
			public bool IsHydro;

			public TableKey(string Key, bool OneDimension)
			{
				this.Key = Key;
				this.OneDimension = OneDimension;
				IsHydro = Key.ToLower().Contains("hydro");

				if (OneDimension)
				{
					Values = new MexArray[1];
					Values[0] = new MexArray(new string[0]);
				}
				else
				{
					Values = new MexArray[0];
				}
			}

			public TableKey(string Key, int count)
			{
				this.Key = Key;
				this.OneDimension = false;
				IsHydro = Key.ToLower().Contains("hydro");

				Values = new MexArray[count];
				for(int i = 0; i < count; i++)
					Values[i] = new MexArray(new string[i]);
			}

		}
		#endregion


		const string KEY_spwnMexArmy = "spwnMexArmy";
		const string KEY_spwnHydroArmy = "spwnHydroArmy";

		public static string ScenarioToTableFileName(string ScenarioFileName)
		{
			return ScenarioFileName.Replace("_scenario.lua", "_tables.lua");
		}

		public static string ScenarioToOptionsFileName(string ScenarioFileName)
		{
			return ScenarioFileName.Replace("_scenario.lua", "_options.lua");
		}

		#region Load
		public bool Load(string FolderName, string ScenarioFileName, string FolderParentPath)
		{
			IsLoaded = false;
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

			//string MapPath = EnvPaths.GetMapsPath();

			string loadedFile = "";
			string loc = FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua";
			loc = ScenarioToTableFileName(loc);

			//Debug.Log("Load file:" + loc);

			if (!System.IO.File.Exists(loc))
			{
				//Debug.Log("No Tables file found");
				return false;
			}

			loadedFile = System.IO.File.ReadAllText(loc, encodeType);// .Replace("}\n", "},\n").Replace("} ", "}, ");

			LuaFile = new Lua();
			LuaFile.LoadCLRPackage();
			try
			{
				LuaFile.DoString(MapLuaParser.Current.SaveLuaHeader.text + loadedFile);
			}
			catch (NLua.Exceptions.LuaException e)
			{
				Debug.LogError(LuaParser.Read.FormatException(e), MapLuaParser.Current.gameObject);
				return false;
			}

			string[] Keys = GetAllTableKeys(loadedFile);
			Data.AllTables = new List<TableKey>();

			GetMexArrays(LuaFile.GetTable(KEY_spwnMexArmy), ref Data.spawnMexArmy);
			GetMexArrays(LuaFile.GetTable(KEY_spwnHydroArmy), ref Data.spawnHydroArmy);


			for(int i = 0; i < Data.spawnMexArmy.Length; i++)
				for (int m = 0; m < Data.spawnMexArmy[i].MexesIds.Length; m++)
					Data.spawnMexArmy[i].MexesIds[m] = ConvertNumberString(Data.spawnMexArmy[i].MexesIds[m]);

			for (int i = 0; i < Data.spawnHydroArmy.Length; i++)
				for (int m = 0; m < Data.spawnHydroArmy[i].MexesIds.Length; m++)
					Data.spawnHydroArmy[i].MexesIds[m] = ConvertNumberString(Data.spawnHydroArmy[i].MexesIds[m]);

			//Debug.Log(Keys.Length);
			for (int i = 0; i < Keys.Length; i++)
			{
				LuaTable MarkerTable = LuaFile.GetTable(Keys[i]);
				if (MarkerTable != null)
				{

					//Debug.Log(Keys[i] + ": " + MarkerTable.Values.Count);

					string[] StringValues = LuaParser.Read.StringArrayFromTable(MarkerTable);


					if (StringValues.Length == 0 || StringValues[0] != "table")
					{
						// Change all names with id smaller than 10 to start from 0
						for (int s = 0; s < StringValues.Length; s++)
						{
							StringValues[s] = ConvertNumberString(StringValues[s]);
						}

						TableKey NewTable = new TableKey(Keys[i], true);
						NewTable.Values[0].MexesIds = StringValues;

						Data.AllTables.Add(NewTable);
					}
					else
					{
						TableKey NewTable = new TableKey(Keys[i], false);
						GetMexArrays(MarkerTable, ref NewTable.Values);
						Data.AllTables.Add(NewTable);
					}

				}
			}

			SaveLua.Scenario SaveLuaData = MapLuaParser.Current.SaveLuaFile.Data;
			for (int mc = 0; mc < SaveLuaData.MasterChains.Length; mc++)
			{
				for (int m = 0; m < SaveLuaData.MasterChains[mc].Markers.Count; m++)
				{
					if (SaveLuaData.MasterChains[mc].Markers[m].MarkerType == SaveLua.Marker.MarkerTypes.Mass)
					{ // Mex
						if (!SaveLuaData.MasterChains[mc].Markers[m].Name.ToLower().StartsWith("mass "))
							continue;

						string NameKey = ConvertToTableName(SaveLuaData.MasterChains[mc].Markers[m].Name, MexName);

						//Debug.Log(NameKey);

						SaveLuaData.MasterChains[mc].Markers[m].SpawnWithArmy = new List<int>();
						SaveLuaData.MasterChains[mc].Markers[m].AdaptiveKeys = new List<string>();

						for (int i = 0; i < Data.spawnMexArmy.Length; i++)
						{
							for (int k = 0; k < Data.spawnMexArmy[i].MexesIds.Length; k++)
							{
								if (Data.spawnMexArmy[i].MexesIds[k] == NameKey) {
									SaveLuaData.MasterChains[mc].Markers[m].SpawnWithArmy.Add(i);
								}
							}
						}

						for (int i = 0; i < Data.AllTables.Count; i++)
						{
							if (!Data.AllTables[i].IsHydro)
							{
								if (Data.AllTables[i].OneDimension)
								{
									for (int k = 0; k < Data.AllTables[i].Values[0].MexesIds.Length; k++)
									{
										if (Data.AllTables[i].Values[0].MexesIds[k] == NameKey)
										{
											SaveLuaData.MasterChains[mc].Markers[m].AdaptiveKeys.Add(Data.AllTables[i].Key);
										}
									}
								}
								else
								{
									for (int t = 0; t < Data.AllTables[i].Values.Length; t++)
									{
										for (int k = 0; k < Data.AllTables[i].Values[t].MexesIds.Length; k++)
										{
											if (Data.AllTables[i].Values[t].MexesIds[k] == NameKey)
											{
												SaveLuaData.MasterChains[mc].Markers[m].AdaptiveKeys.Add(Data.AllTables[i].Key + "#" + t);
											}
										}
									}
								}
							}
						}
					}
					else if (SaveLuaData.MasterChains[mc].Markers[m].MarkerType == SaveLua.Marker.MarkerTypes.Hydrocarbon)
					{ // Hydro
						if (!SaveLuaData.MasterChains[mc].Markers[m].Name.ToLower().StartsWith("hydrocarbon "))
							continue;

						string NameKey = ConvertToTableName(SaveLuaData.MasterChains[mc].Markers[m].Name, HydroName);

						SaveLuaData.MasterChains[mc].Markers[m].SpawnWithArmy = new List<int>();
						SaveLuaData.MasterChains[mc].Markers[m].AdaptiveKeys = new List<string>();

						for (int i = 0; i < Data.spawnHydroArmy.Length; i++)
						{
							for (int k = 0; k < Data.spawnHydroArmy[i].MexesIds.Length; k++)
							{
								if (Data.spawnHydroArmy[i].MexesIds[k] == NameKey)
								{
									SaveLuaData.MasterChains[mc].Markers[m].SpawnWithArmy.Add(i);
								}
							}
						}

						for (int i = 0; i < Data.AllTables.Count; i++)
						{
							if (Data.AllTables[i].IsHydro)
							{
								if (Data.AllTables[i].OneDimension)
								{
									for (int k = 0; k < Data.AllTables[i].Values[0].MexesIds.Length; k++)
									{
										if (Data.AllTables[i].Values[0].MexesIds[k] == NameKey)
										{
											SaveLuaData.MasterChains[mc].Markers[m].AdaptiveKeys.Add(Data.AllTables[i].Key);
										}
									}
								}
								else
								{
									for (int t = 0; t < Data.AllTables[i].Values.Length; t++)
									{
										for (int k = 0; k < Data.AllTables[i].Values[t].MexesIds.Length; k++)
										{
											if (Data.AllTables[i].Values[t].MexesIds[k] == NameKey)
											{
												SaveLuaData.MasterChains[mc].Markers[m].AdaptiveKeys.Add(Data.AllTables[i].Key + "#" + t);
											}
										}
									}
								}
							}
						}

					}
				}
			}


			IsLoaded = true;
			return true;
		}

		#endregion

		public void CreateDefault()
		{
			Data.AllTables = new List<TableKey>();

			Data.AllTables.Add(new TableKey("spwnAdditionalHydro", true));

			Data.AllTables.Add(new TableKey("middlemass", 2));
			Data.AllTables.Add(new TableKey("sidemass", 2));
			Data.AllTables.Add(new TableKey("underwatermass", 2));
			Data.AllTables.Add(new TableKey("islandmass", 2));
			Data.AllTables.Add(new TableKey("backmass", 2));

			Data.AllTables.Add(new TableKey("crazyrushOneMex", true));
			Data.AllTables.Add(new TableKey("extramass", true));
			Data.AllTables.Add(new TableKey("DuplicateListMex", true));

			IsLoaded = true;
		}

		#region Save
		public void Save(string Path)
		{
			if (!IsLoaded)
				return;

			ScenarioLua.ScenarioInfo ScenarioData = MapLuaParser.Current.ScenarioLuaFile.Data;

			SaveLua.Marker[] AllMarkers = MarkersControler.GetMarkers();

			LuaParser.Creator LuaFile = new LuaParser.Creator();

			LuaFile.AddSaveComent("Generated by FAF Map Editor");
			LuaFile.AddComent("Table of which resources belong to which player, it is sorted in such a way that the first line");
			LuaFile.AddComent("corresponds to player one, the second to player 2 and so on...");
			LuaFile.NextLine(6);

			int ArmyId = 0;

			// Count armies
			for (int c = 0; c < ScenarioData.Configurations.Length; c++)
			{
				for (int t = 0; t < ScenarioData.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < ScenarioData.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if (ScenarioData.Configurations[c].Teams[t].Armys[a].Data != null)
						{
							ArmyId++;
						}
					}
				}
			}


			LuaFile.AddComent("Line number is 10 + armynumber for the mexes in the table");
			int ArmyCount = ArmyId;
			Data.spawnMexArmy = new MexArray[ArmyCount];
			Data.spawnHydroArmy = new MexArray[ArmyCount];

			ArmyId = 0;
			// Create Mass table
			for (int c = 0; c < ScenarioData.Configurations.Length; c++)
			{
				for (int t = 0; t < ScenarioData.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < ScenarioData.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if (ScenarioData.Configurations[c].Teams[t].Armys[a].Data != null)
						{
							List<string> MarkerArrays = new List<string>();

							for (int m = 0; m < AllMarkers.Length; m++)
							{
								if (AllMarkers[m].MarkerType == SaveLua.Marker.MarkerTypes.Mass && AllMarkers[m].SpawnWithArmy.Contains(ArmyId))
								{
									MarkerArrays.Add(ConvertToTableName(AllMarkers[m].Name, MexName));
								}
							}

							MarkerArrays.Sort();
							Data.spawnMexArmy[ArmyId] = new MexArray(MarkerArrays.ToArray());

							string ValueString = CreateTableValueString(MarkerArrays);

							if (ArmyCount >= 16 && ArmyId == ArmyCount - 1)
								ValueString += LuaParser.Write.EndBracket;
							else
								ValueString += ",";

							if (ArmyId == 0)
								LuaFile.OpenTab("spwnMexArmy" + LuaParser.Write.OpenBracketValue + "\t\t" + ValueString, 5);
							else
								LuaFile.AddLine(ValueString);
							ArmyId++;
						}
					}
				}
			}
			if (ArmyCount < 16)
			{
				for (int a = ArmyId; a < 16; a++)
				{
					if (a == 15)
						LuaFile.AddLine(LuaParser.Write.OpenBracket + LuaParser.Write.EndBracket + LuaParser.Write.EndBracket);
					else
						LuaFile.AddLine(LuaParser.Write.OpenBracket + LuaParser.Write.EndBracketNext);

				}
			}

			LuaFile.CloseTab(5);

			LuaFile.NextLine(3);

			LuaFile.AddComent("Line number is 30 + armynumber for the hydros in the table");


			ArmyId = 0;
			// Create Hydro table
			for (int c = 0; c < ScenarioData.Configurations.Length; c++)
			{
				for (int t = 0; t < ScenarioData.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < ScenarioData.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if (ScenarioData.Configurations[c].Teams[t].Armys[a].Data != null)
						{
							List<string> MarkerArrays = new List<string>();

							for (int m = 0; m < AllMarkers.Length; m++)
							{
								if (AllMarkers[m].MarkerType == SaveLua.Marker.MarkerTypes.Hydrocarbon && AllMarkers[m].SpawnWithArmy.Contains(ArmyId))
								{
									MarkerArrays.Add(ConvertToTableName(AllMarkers[m].Name, HydroName));
								}
							}
							MarkerArrays.Sort();
							Data.spawnHydroArmy[ArmyId] = new MexArray(MarkerArrays.ToArray());

							string ValueString = CreateTableValueString(MarkerArrays);

							if (ArmyCount >= 16 && ArmyId == ArmyCount - 1)
								ValueString += LuaParser.Write.EndBracket;
							else
								ValueString += ",";

							if (ArmyId == 0)
								LuaFile.OpenTab("spwnHydroArmy" + LuaParser.Write.OpenBracketValue + "\t" + ValueString, 5);
							else
								LuaFile.AddLine(ValueString);
							ArmyId++;
						}
					}
				}
			}

			if(ArmyCount < 16)
			{
				for(int a = ArmyId; a < 16; a++)
				{
					if(a == 15)
						LuaFile.AddLine(LuaParser.Write.OpenBracket + LuaParser.Write.EndBracket + LuaParser.Write.EndBracket);
					else
						LuaFile.AddLine(LuaParser.Write.OpenBracket + LuaParser.Write.EndBracketNext);

				}
			}

			LuaFile.CloseTab(5);

			LuaFile.NextLine(2);
			int TablesCount = Data.AllTables.Count;
			for (int t = 0; t < TablesCount; t++)
			{
				string TableString = Data.AllTables[t].Key + LuaParser.Write.SetValue;


				if (!Data.AllTables[t].OneDimension)
					TableString += LuaParser.Write.OpenBracket;

				for (int v = 0; v < Data.AllTables[t].Values.Length; v++)
				{
					string TableValueKey = Data.AllTables[t].Key;
					if (!Data.AllTables[t].OneDimension)
						TableValueKey += "#" + (v);

					List<string> NewMexes = new List<string>();

					for (int m = 0; m < AllMarkers.Length; m++)
					{
						if (Data.AllTables[t].IsHydro && AllMarkers[m].MarkerType == SaveLua.Marker.MarkerTypes.Hydrocarbon)
						{ // Hydro
							if (AllMarkers[m].AdaptiveKeys.Contains(TableValueKey))
								NewMexes.Add(ConvertToTableName(AllMarkers[m].Name, HydroName));
						}
						else
						{ // Mass
							if (AllMarkers[m].AdaptiveKeys.Contains(TableValueKey))
								NewMexes.Add(ConvertToTableName(AllMarkers[m].Name, MexName));
						}
					}

					NewMexes.Sort();

					Data.AllTables[t].Values[v] = new MexArray(NewMexes.ToArray());
					if (v > 0)
						TableString += LuaParser.Write.NextValue;

					TableString += CreateTableValueString(NewMexes);

				}

				if (!Data.AllTables[t].OneDimension)
					TableString += LuaParser.Write.EndBracket;



				LuaFile.AddLine(TableString);
				LuaFile.NextLine();
			}

			System.IO.File.WriteAllText(Path, LuaFile.GetFileString());
		}

		const int MexName = 5;
		const int HydroName = 12;

		static string ConvertToTableName(string name, int begin)
		{
			name = name.Remove(0, begin);

			name = ConvertNumberString(name);

			return name;
		}

		static string ConvertNumberString(string name)
		{
			int StringInt = 0;
			if (int.TryParse(name, out StringInt))
			{
				if (StringInt < 10)
					name = "0" + StringInt;
			}
			return name;
		}

		const string TableStringSeparator = ",";
		const string TableStringValue = "'";
		string CreateTableValueString(List<string> Values)
		{
			string TableString = "{";
			int Count = Values.Count;
			for (int s = 0; s < Count; s++)
			{
				int Result;
				bool Parse = int.TryParse(Values[s], out Result);
				if (Parse)
				{
					if (s > 0)
						TableString += TableStringSeparator + Values[s];
					else
						TableString += Values[s];
				}
				else
				{
					if (s > 0)
						TableString += TableStringSeparator + TableStringValue + Values[s] + TableStringValue;
					else
						TableString += TableStringValue + Values[s] + TableStringValue;
				}
			}

			TableString += "}";
			return TableString;
		}


		#endregion

			void GetMexArrays(LuaTable Table, ref MexArray[] Array)
		{
			LuaTable[] Tabs = LuaParser.Read.TableArrayFromTable(Table);
			Array = new MexArray[Tabs.Length];
			for (int i = 0; i < Array.Length; i++)
				Array[i] = new MexArray(LuaParser.Read.StringArrayFromTable(Tabs[i]));
		}

		string[] GetAllTableKeys(string file)
		{
			List<string> Keys = new List<string>();
			file = file.Replace(" ", "");
			string[] Lines = file.Split("\n".ToCharArray());
			for(int l = 0; l < Lines.Length; l++)
			{
				if (Lines[l].Contains("="))
				{
					string value = Lines[l].Split("=".ToCharArray())[0];
					if (value == KEY_spwnMexArmy || value == KEY_spwnHydroArmy)
						continue;
					Keys.Add(value);
				}
			}


			return Keys.ToArray();
		}
	}
}