using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;
using EditMap;

public partial struct GetGamedataFile
{

	public struct UnitDB
	{
		public string CodeName;
		public string Path;
		public string Name;
		public string HelpText;

		public string FactionName;
		public string Icon;
		public string TechLevel;
		public string Category;
		public string[] Categories;

		public HashSet<string> CategoriesHash;

		public UnitDB(string LocalPath)
		{
			CodeName = GetCodenameFromPath(LocalPath);
			Path = LocalPath;
			Name = "";
			HelpText = "";
			FactionName = DefaultFaction;
			Icon = "land";
			Categories = new string[] { "TECH1" };
			CategoriesHash = new HashSet<string>(Categories);
			TechLevel = "TECH1";
			Category = "";
		}

		const string DefaultFaction = "Other";

		public UnitDB(Lua BP, string LocalPath)
		{
			CodeName = GetCodenameFromPath(LocalPath);
			Path = LocalPath;
			object CurrentValue;

			LuaTable InterfaceTab = BP.GetTable("UnitBlueprint.Interface");
			if (InterfaceTab != null)
			{
				CurrentValue = InterfaceTab.RawGet("HelpText");
				if (CurrentValue != null)
					HelpText = CurrentValue.ToString();
				else
					HelpText = "";
			}
			else
				HelpText = "";


			LuaTable GeneralTab = BP.GetTable("UnitBlueprint.General");
			if (GeneralTab != null)
			{
				CurrentValue = GeneralTab.RawGet("UnitName");
				if (CurrentValue != null)
				{
					Name = CurrentValue.ToString();
				}
				else
				{
					if (!string.IsNullOrEmpty(HelpText))
					{
						Name = HelpText;
					}
					else
					{
						Name = CodeName;
					}
				}


				CurrentValue = GeneralTab.RawGet("FactionName");
				if (CurrentValue != null)
					FactionName = CurrentValue.ToString();
				else
					FactionName = DefaultFaction;

				CurrentValue = GeneralTab.RawGet("Icon");
				if (CurrentValue != null)
					Icon = CurrentValue.ToString();
				else
					Icon = "land";

				CurrentValue = GeneralTab.RawGet("Category");
				if (CurrentValue != null)
					Category = CurrentValue.ToString();
				else
					Category = "";

			}
			else
			{
				FactionName = DefaultFaction;
				Icon = "land";
				Category = "";
				Name = CodeName;
			}

			if (Name.Contains(">"))
			{
				string[] SplitedName = Name.Split('>');
				Name = SplitedName[SplitedName.Length - 1];
			}


			LuaTable CategoriesTab = BP.GetTable("UnitBlueprint.Categories");
			if (CategoriesTab != null)
			{
				Categories = LuaParser.Read.GetTableValues(CategoriesTab);

				TechLevel = "TECH1";

				for (int i = 0; i < Categories.Length; i++)
				{
					if(Categories[i].StartsWith("TECH"))
					{
						TechLevel = Categories[i];
						break;
					}
					else if(Categories[i] == "EXPERIMENTAL")
					{
						TechLevel = "TECH4";
						break;
					}
				}
			}
			else
			{
				Categories = new string[] {"TECH1"};
				TechLevel = "TECH1";
			}
			CategoriesHash = new HashSet<string>(Categories);
		}

	}

	static Dictionary<string, UnitDB> LoadedUnitBPPreviews = new Dictionary<string, UnitDB>();
	public static UnitDB LoadUnitBlueprintPreview(string LocalPath)
	{
		if (LoadedUnitBPPreviews.ContainsKey(LocalPath))
			return LoadedUnitBPPreviews[LocalPath];


		byte[] Bytes = LoadBytes(LocalPath);
		if (Bytes == null || Bytes.Length == 0)
		{
			Debug.LogWarning("Unit does not exits: " + LocalPath);
			return new UnitDB(LocalPath);
		}
		string BluePrintString = System.Text.Encoding.UTF8.GetString(Bytes);

		if (BluePrintString.Length == 0)
		{
			Debug.LogWarning("Loaded blueprint is empty");
			return new UnitDB(LocalPath);
		}

		BluePrintString = BluePrintString.Replace("UnitBlueprint {", "UnitBlueprint = {");


		//Fix LUA
		string[] SplitedBlueprint = BluePrintString.Split("\n".ToCharArray());
		string NewBlueprintString = "";
		for (int i = 0; i < SplitedBlueprint.Length; i++)
		{
			if (SplitedBlueprint[i].Length > 0 && !SplitedBlueprint[i].Contains("#"))
			{
				NewBlueprintString += SplitedBlueprint[i] + "\n";
			}
		}
		BluePrintString = NewBlueprintString;

		Lua BP = new Lua();
		BP.LoadCLRPackage();

		try
		{
			BP.DoString(MapLuaParser.Current.SaveLuaHeader.text + BluePrintString);
		}
		catch (NLua.Exceptions.LuaException e)
		{
			Debug.LogWarning(LuaParser.Read.FormatException(e) + "\n" + LocalPath);
			return new UnitDB(LocalPath);
		}
		UnitDB ToReturn = new UnitDB(BP, LocalPath);
		LoadedUnitBPPreviews.Add(LocalPath, ToReturn);

		return ToReturn;
	}


	static string GetCodenameFromPath(string LocalPath)
	{
		string[] PathSplit = LocalPath.Split('/');
		return PathSplit[PathSplit.Length - 1].Replace(".bp", "").Replace("_unit", "");
	}

}