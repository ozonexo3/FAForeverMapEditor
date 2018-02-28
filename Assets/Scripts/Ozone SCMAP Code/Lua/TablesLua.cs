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

namespace MapLua
{
	[System.Serializable]
	public class TablesLua
	{
		public TablesInfo Data = new TablesInfo();
		Lua LuaFile;


		#region Structure Objects
		[System.Serializable]
		public class TablesInfo
		{
			public MexArray[] spawnMexArmy = new MexArray[5];

			public MexArray[] spwnHydroArmy = new MexArray[5];

			public string[] vanishCoreMexesFor1v1;
			public string[] vanish1v1Mexes;
			public string[] vanish1v1Hydro;
			public string[] vanish2v2Hydro;

			public MexArray[] middlemex;
			public MexArray[] middlemass;
			public MexArray[] coremexes;

			public string[] spwnAdditionalHydro;

			public MexArray[] sidemass;
			public MexArray[] underwatermass;
			public MexArray[] islandmass;
			public MexArray[] backmass;

			public string[] crazyrushOneMex;
			public string[] DuplicateListMex;
		}



		[System.Serializable]
		public class MexArray
		{
			public string[] MexesIds;

			public MexArray(string[] Keys)
			{
				MexesIds = Keys;
			}
		}
		#endregion

		const string KEY_spwnMexArmy = "spwnMexArmy";
		const string KEY_spwnHydroArmy = "spwnHydroArmy";

		public bool Load(string FolderName, string ScenarioFileName, string FolderParentPath)
		{
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

			//string MapPath = EnvPaths.GetMapsPath();

			string loadedFile = "";
			string loc = FolderParentPath + FolderName + "/" + ScenarioFileName + ".lua";
			loc = loc.Replace("_scenario.lua", "_tables.lua");

			Debug.Log("Load file:" + loc);

			if (!System.IO.File.Exists(loc))
			{
				Debug.Log("No Tables file found");
				return false;
			}

			loadedFile = System.IO.File.ReadAllText(loc, encodeType).Replace("}\n", "},\n").Replace("} ", "}, ");
			loadedFile = "Main = {\n" + loadedFile + "\n}";

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



			LuaTable Main = LuaFile.GetTable("Main");

			var Enum = Main.Keys.GetEnumerator();
			while (Enum.MoveNext())
			{
				string Key = Enum.Current as string;
				Debug.Log(Key);
			}


			Debug.LogWarning("---");



			string[] MainKeys = LuaParser.Read.GetTableKeys(Main);

			for(int i = 0; i < MainKeys.Length; i++)
			{
				LuaTable Table = Main.RawGet(MainKeys[i]) as LuaTable;
				Debug.Log(MainKeys[i] + " : " + Table.Keys.Count);

				if(MainKeys[i] == KEY_spwnMexArmy)
					GetMexArrays(Main.RawGet(KEY_spwnMexArmy) as LuaTable, ref Data.spawnMexArmy);
			}
			

			//GetMexArrays(Main.GetTable(KEY_spwnMexArmy), ref Data.spawnMexArmy);
			//GetMexArrays(Main.GetTable(KEY_spwnHydroArmy), ref Data.spwnHydroArmy);

			return true;
		}

		void GetMexArrays(LuaTable Table, ref MexArray[] Array)
		{
			LuaTable[] Tabs = LuaParser.Read.GetTableTables(Table);
			Array = new MexArray[Tabs.Length];
			for (int i = 0; i < Array.Length; i++)
				Array[i] = new MexArray(LuaParser.Read.StringArrayFromTable(Tabs[i]));
		}
	}
}