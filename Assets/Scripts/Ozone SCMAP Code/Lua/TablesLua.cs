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

		const string KEY_vanishCoreMexesFor1v1y = "vanishCoreMexesFor1v1";
		const string KEY_vanish1v1Mexes = "vanish1v1Mexes";
		const string KEY_vanish1v1Hydro = "vanish1v1Hydro";
		const string KEY_vanish2v2Hydro = "vanish2v2Hydro";

		const string KEY_middlemex = "middlemex";
		const string KEY_middlemass = "middlemass";
		const string KEY_coremexes = "coremexes";

		const string KEY_spwnAdditionalHydro = "spwnAdditionalHydro";
		const string KEY_sidemass = "sidemass";
		const string KEY_underwatermass = "underwatermass";
		const string KEY_islandmass = "islandmass";
		const string KEY_backmass = "backmass";
		const string KEY_crazyrushOneMex = "crazyrushOneMex";
		const string KEY_DuplicateListMex = "DuplicateListMex";

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

			loadedFile = System.IO.File.ReadAllText(loc, encodeType);

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

			GetMexArrays(LuaFile.GetTable(KEY_spwnMexArmy), ref Data.spawnMexArmy);
			GetMexArrays(LuaFile.GetTable(KEY_spwnHydroArmy), ref Data.spwnHydroArmy);

			Data.vanishCoreMexesFor1v1 = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_vanishCoreMexesFor1v1y));
			Data.vanish1v1Mexes = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_vanish1v1Mexes));
			Data.vanish1v1Hydro = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_vanish1v1Hydro));
			Data.vanish2v2Hydro = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_vanish2v2Hydro));

			GetMexArrays(LuaFile.GetTable(KEY_middlemex), ref Data.middlemex);
			GetMexArrays(LuaFile.GetTable(KEY_middlemass), ref Data.middlemass);
			GetMexArrays(LuaFile.GetTable(KEY_coremexes), ref Data.coremexes);

			Data.spwnAdditionalHydro = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_spwnAdditionalHydro));

			GetMexArrays(LuaFile.GetTable(KEY_sidemass), ref Data.sidemass);
			GetMexArrays(LuaFile.GetTable(KEY_underwatermass), ref Data.underwatermass);
			GetMexArrays(LuaFile.GetTable(KEY_islandmass), ref Data.islandmass);
			GetMexArrays(LuaFile.GetTable(KEY_backmass), ref Data.backmass);

			Data.crazyrushOneMex = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_crazyrushOneMex));
			Data.DuplicateListMex = LuaParser.Read.StringArrayFromTable(LuaFile.GetTable(KEY_DuplicateListMex));

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