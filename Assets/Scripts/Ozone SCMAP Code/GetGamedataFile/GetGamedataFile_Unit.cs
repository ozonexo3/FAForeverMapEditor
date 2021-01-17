using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using NLua;
using EditMap;

public partial struct GetGamedataFile
{


	public class UnitBluePrint
	{
		public string CodeName = "";
		public string Path;
		public string Name = "";
		public string HelpText = "";

		public int TypeId;

		// Display
		public BluePrintLoD[] LODs = new BluePrintLoD[0];
		public Termac[] Termacs;
		public bool HasTermac = false;
		public Decal Termac_Albedo;
		public Decal Termac_Normal;

		public bool PhysicsLayerAir;
		public bool PhysicsLayerLand;
		public bool PhysicsLayerOrbit;
		public bool PhysicsLayerSeabed;
		public bool PhysicsLayerSub;
		public bool PhysicsLayerWater;
		public float PhysicsElevation = 0;

		public string[] Categories;
		public string GeneralCategory;
		public string GeneralClassification;

		public Vector3 SelectionSize;
		public Vector3 Size;
		public Vector3 UniformScale;
		public Vector3 RenderScale;
		public Vector2 Footprint = Vector2.zero;

		// Strategic
		public string StrategicIconName;
		public int StrategicIconSortPriority;
		public Texture2D strategicIcon;
		public Material strategicMaterial;

		// Skirt
		public float SkirtOffsetX;
		public float SkirtOffsetZ;
		public Vector3 SkirtSize;
		public float TurnRate;

		// Intel
		public float VisionRadius;

		// Economy
		public float BuildCostEnergy;
		public float BuildCostMass;
		public float BuildTime;

		// Wreckage
		public float Wreckage_EnergyMult;
		public float Wreckage_HealthMult;
		public float Wreckage_MassMult;
		public float Wreckage_ReclaimTimeMultiplier;

		public bool Wreckage_Layer_Air;
		public bool Wreckage_Layer_Land;
		public bool Wreckage_Layer_Seabed;
		public bool Wreckage_Layer_Sub;
		public bool Wreckage_Layer_Water;

		public Weapon[] Weapons;
		public float MinRange;
		public float MaxRange;

		public class Weapon
		{
			public float MinRadius;
			public float MaxRadius;
			public string WeaponCategory = "";
		}

		public void GenerateEmptyLod()
		{
			LODs = new BluePrintLoD[1];
			LODs[0] = new BluePrintLoD();
			LODs[0].Mesh = UnitsInfo.Current.NoUnitMesh;
			LODs[0].Mat = UnitsInfo.Current.NoUnitMaterial;
			LODs[0].LODCutoff = 300;
		}
	}


	public static bool IsUnitSourceLoaded(string UnitCode)
	{
		UnitCode.ToLower();
		string scdPath = "units/" + UnitCode + "/" + UnitCode + "_unit.bp";

		return LoadedUnitObjects.ContainsKey(LocalBlueprintPath(scdPath));
	}


	public static UnitSource LoadUnit(string UnitCode)
	{
		UnitCode.ToLower();
		string scdPath = "units/" + UnitCode + "/" + UnitCode + "_unit.bp";

		return LoadUnitSource(LocalBlueprintPath(scdPath));
	}


	public static Dictionary<string, UnitSource> LoadedUnitObjects = new Dictionary<string, UnitSource>();
	public static string[] LoadedUnitsStrategicDrawOrder = new string[0];

	static void SortStrategicIcons()
	{
		if(LoadedUnitObjects.Count == 0)
		{
			LoadedUnitsStrategicDrawOrder = new string[0];
			return;
		}

		List<string> iconsKeys = new List<string>(64);
		List<int> iconsOrder = new List<int>(64);

		foreach (var unitEntry in LoadedUnitObjects)
		{
			int strategicOrder = unitEntry.Value.BP.StrategicIconSortPriority;
			if (iconsOrder.Count == 0)
			{
				iconsOrder.Add(strategicOrder);
				iconsKeys.Add(unitEntry.Key);
				continue;
			}

			bool found = false;
			for (int i = 0; i < iconsOrder.Count; i++)
			{
				if(iconsOrder[i] >= strategicOrder)
				{
					iconsOrder.Insert(i, strategicOrder);
					iconsKeys.Insert(i, unitEntry.Key);
					found = true;
					break;
				}
			}

			if (!found)
			{
				iconsOrder.Add(strategicOrder);
				iconsKeys.Add(unitEntry.Key);
			}
		}

		LoadedUnitsStrategicDrawOrder = iconsKeys.ToArray();
	}

	public static UnitSource CreateEmptyUnit(string LocalPath, UnitSource ToReturn = null)
	{
		string[] PathSplit = LocalPath.Split(("/").ToCharArray());
		GameObject NewUnit = new GameObject(PathSplit[PathSplit.Length - 1].Replace(".bp", ""));
		if (ToReturn == null) {
			ToReturn = NewUnit.AddComponent<UnitSource>();

			// *** Parse Blueprint
			ToReturn.BP = new UnitBluePrint();
			ToReturn.BP.Name = PathSplit[PathSplit.Length - 1].Replace(".bp", "");
			ToReturn.BP.CodeName = ToReturn.BP.Name.Replace("_unit", "");
		}

		ToReturn.BP.TypeId = 0;

		ToReturn.BP.Size = Vector3.one;
		ToReturn.BP.UniformScale = Vector3.one;
		ToReturn.BP.RenderScale = Vector3.one;
		ToReturn.BP.Categories = new string[0];
		ToReturn.BP.GenerateEmptyLod();


		ToReturn.RenderDistances = new float[] { ToReturn.BP.LODs[0].LODCutoff * 0.1f };
		ToReturn.ApplyLods();

		LoadedUnitObjects.Add(LocalPath, ToReturn);
		SortStrategicIcons();

		return ToReturn;
	}


	public static UnitSource LoadUnitSource(string LocalPath)
	{
		if (LoadedUnitObjects.ContainsKey(LocalPath))
			return LoadedUnitObjects[LocalPath];


		//UnitObject ToReturn = new UnitObject();
		byte[] Bytes = LoadBytes(LocalPath);
		if (Bytes == null || Bytes.Length == 0)
		{
			Debug.LogWarning("Unit does not exits: " + LocalPath);
			return CreateEmptyUnit(LocalPath);
		}
		string BluePrintString = System.Text.Encoding.UTF8.GetString(Bytes);

		if (BluePrintString.Length == 0)
		{
			Debug.LogWarning("Loaded blueprint is empty");
			return CreateEmptyUnit(LocalPath);
		}

		BluePrintString = BluePrintString.Replace("UnitBlueprint {", "UnitBlueprint = {");

		string[] PathSplit = LocalPath.Split('/');
		GameObject NewUnit = new GameObject(PathSplit[PathSplit.Length - 1].Replace(".bp", ""));
		UnitSource ToReturn = NewUnit.AddComponent<UnitSource>();

		// *** Parse Blueprint
		ToReturn.BP = new UnitBluePrint();
		// Create Lua
		Lua BP = new Lua();
		BP.LoadCLRPackage();

		ToReturn.BP.Name = PathSplit[PathSplit.Length - 1].Replace(".bp", "");
		ToReturn.BP.CodeName = ToReturn.BP.Name.Replace("_unit", "");


		ToReturn.BP.TypeId = ToReturn.BP.CodeName.GetHashCode();

		//Debug.Log("Load Unit : " + ToReturn.BP.CodeName);

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

		ToReturn.BP.Path = LocalPath;

		try
		{
			BP.DoString(MapLuaParser.Current.SaveLuaHeader.text + BluePrintString);
		}
		catch (NLua.Exceptions.LuaException e)
		{
			Debug.LogWarning(LuaParser.Read.FormatException(e) + "\n" + LocalPath);
			return CreateEmptyUnit(LocalPath, ToReturn);
		}


		//TODO
		object CurrentValue = null;
		LuaTable UnitBlueprintTable = BP.GetTable("UnitBlueprint");

		LuaTable InterfaceTab = BP.GetTable("UnitBlueprint.Interface");
		if(InterfaceTab != null)
			ToReturn.BP.HelpText = InterfaceTab.RawGet("HelpText").ToString();

		LuaTable CategoriesTab = BP.GetTable("UnitBlueprint.Categories");
		if (CategoriesTab != null)
			ToReturn.BP.Categories = LuaParser.Read.GetTableValues(CategoriesTab);
		else
			ToReturn.BP.Categories = new string[0];

		LuaTable EconomyTab = BP.GetTable("UnitBlueprint.Economy");
		if (EconomyTab != null)
		{
			CurrentValue = EconomyTab.RawGet("BuildCostEnergy");
			if (CurrentValue != null)
				ToReturn.BP.BuildCostEnergy = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = EconomyTab.RawGet("BuildCostMass");
			if (CurrentValue != null)
				ToReturn.BP.BuildCostMass = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = EconomyTab.RawGet("BuildTime");
			if (CurrentValue != null)
				ToReturn.BP.BuildTime = LuaParser.Read.StringToFloat(CurrentValue.ToString());
		}

		CurrentValue = UnitBlueprintTable.RawGet("StrategicIconName");
		if (CurrentValue != null)
		{
			ToReturn.BP.StrategicIconName = CurrentValue.ToString();

			ToReturn.BP.strategicIcon = GetGamedataFile.LoadTexture2D("textures/ui/common/game/strategicicons/" +  ToReturn.BP.StrategicIconName + "_rest.dds", false, true, true);
			if(ToReturn.BP.strategicIcon != null)
			{
				ToReturn.BP.strategicIcon.filterMode = FilterMode.Point;
			}
			ToReturn.BP.strategicMaterial = new Material(UnitsInfo.Current.StrategicIcon);
			ToReturn.BP.strategicMaterial.SetTexture(SHADER_MainTex, ToReturn.BP.strategicIcon);
		}

		CurrentValue = UnitBlueprintTable.RawGet("StrategicIconSortPriority");
		if (CurrentValue != null)
			ToReturn.BP.StrategicIconSortPriority = LuaParser.Read.StringToInt(CurrentValue.ToString());

		LuaTable PhisicsLayersTab = BP.GetTable("UnitBlueprint.Physics.BuildOnLayerCaps");
		if (PhisicsLayersTab != null)
		{
			CurrentValue = PhisicsLayersTab.RawGet("LAYER_Air");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsLayerAir = LuaParser.Read.StringToBool(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("LAYER_Land");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsLayerLand = LuaParser.Read.StringToBool(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("LAYER_Orbit");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsLayerOrbit = LuaParser.Read.StringToBool(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("LAYER_Seabed");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsLayerSeabed = LuaParser.Read.StringToBool(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("LAYER_Sub");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsLayerSub = LuaParser.Read.StringToBool(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("LAYER_Water");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsLayerWater = LuaParser.Read.StringToBool(CurrentValue.ToString());
			
		}

		float BiggestLodDistance = 100;
		//Display
		if (BP.GetTable("UnitBlueprint.Display") != null)
		{
			if (BP.GetTable("UnitBlueprint.Display").RawGet("UniformScale") != null)
				ToReturn.BP.UniformScale = Vector3.one * LuaParser.Read.StringToFloat(BP.GetTable("UnitBlueprint.Display").RawGet("UniformScale").ToString());

			ToReturn.BP.RenderScale = ToReturn.BP.UniformScale * 0.1f;

			// Mesh
			if (BP.GetTable("UnitBlueprint.Display.Mesh") != null)
			{

				//Lods
				LuaTable LodTable = BP.GetTable("UnitBlueprint.Display.Mesh.LODs");

				LuaTable[] LodTableValues = new LuaTable[LodTable.Keys.Count];
				ToReturn.BP.LODs = new BluePrintLoD[LodTableValues.Length];
				LodTable.Values.CopyTo(LodTableValues, 0);

				for (int i = 0; i < LodTableValues.Length; i++)
				{
					ToReturn.BP.LODs[i] = new BluePrintLoD();

					CurrentValue = LodTableValues[i].RawGet("AlbedoName");
					if (CurrentValue != null)
						ToReturn.BP.LODs[i].AlbedoName = CurrentValue.ToString();

					CurrentValue = LodTableValues[i].RawGet("NormalsName");
					if (CurrentValue != null)
						ToReturn.BP.LODs[i].NormalsName = CurrentValue.ToString();

					CurrentValue = LodTableValues[i].RawGet("SpecularName");
					if (CurrentValue != null)
						ToReturn.BP.LODs[i].SpecularName = CurrentValue.ToString();

					CurrentValue = LodTableValues[i].RawGet("ShaderName");
					if (CurrentValue != null)
						ToReturn.BP.LODs[i].ShaderName = CurrentValue.ToString();

					CurrentValue = LodTableValues[i].RawGet("LODCutoff");
					if (CurrentValue != null)
					{
						ToReturn.BP.LODs[i].LODCutoff = LuaParser.Read.StringToFloat(CurrentValue.ToString());
						BiggestLodDistance = ToReturn.BP.LODs[i].LODCutoff;
					}


					ToReturn.BP.LODs[i].Scm = LocalPath.Replace("unit.bp", "lod" + i.ToString() + ".scm");
				}
			}

			//Termacs

			LuaTable TermacsTable = BP.GetTable("UnitBlueprint.Display.Tarmacs");
			if (TermacsTable != null)
			{
				LuaTable[] AllTermacs = LuaParser.Read.TableArrayFromTable(TermacsTable);
				ToReturn.BP.Termacs = new Termac[AllTermacs.Length];

				for (int t = 0; t < ToReturn.BP.Termacs.Length; t++)
				{
					ToReturn.BP.Termacs[t] = new Termac(AllTermacs[t]);

				}

				LoadTermacs(ToReturn.BP);
			}
			else
				ToReturn.BP.Termacs = new Termac[0];
		}

		ToReturn.BP.Size = Vector3.one * 0.1f;

		ToReturn.BP.MinRange = 0;
		ToReturn.BP.MaxRange = 0;
		LuaTable WeaponsTable = BP.GetTable("UnitBlueprint.Weapon");
		if(WeaponsTable != null)
		{
			LuaTable[] AllWeapons = LuaParser.Read.TableArrayFromTable(WeaponsTable);
			ToReturn.BP.Weapons = new UnitBluePrint.Weapon[AllWeapons.Length];
			for(int i = 0; i < ToReturn.BP.Weapons.Length; i++)
			{
				ToReturn.BP.Weapons[i] = new UnitBluePrint.Weapon();

				if (AllWeapons[i] == null)
					continue;

				CurrentValue = AllWeapons[i].RawGet("WeaponCategory");
				if (CurrentValue != null)
					ToReturn.BP.Weapons[i].WeaponCategory = CurrentValue.ToString();
				else
					ToReturn.BP.Weapons[i].WeaponCategory = "";

				CurrentValue = AllWeapons[i].RawGet("MinRadius");
				if (CurrentValue != null)
					ToReturn.BP.Weapons[i].MinRadius = LuaParser.Read.StringToFloat(CurrentValue.ToString());
				else
					ToReturn.BP.Weapons[i].MinRadius = 0;

				CurrentValue = AllWeapons[i].RawGet("MaxRadius");
				if (CurrentValue != null)
					ToReturn.BP.Weapons[i].MaxRadius = LuaParser.Read.StringToFloat(CurrentValue.ToString());
				else
					ToReturn.BP.Weapons[i].MaxRadius = 0;

				ToReturn.BP.MinRange = Mathf.Max(ToReturn.BP.MinRange, ToReturn.BP.Weapons[i].MinRadius);
				ToReturn.BP.MaxRange = Mathf.Max(ToReturn.BP.MaxRange, ToReturn.BP.Weapons[i].MaxRadius);
			}
		}

		LuaTable WreckageTable = BP.GetTable("UnitBlueprint.Wreckage");
		if (WreckageTable != null)
		{
			CurrentValue = WreckageTable.RawGet("MassMult");
			if (CurrentValue != null)
				ToReturn.BP.Wreckage_MassMult = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				ToReturn.BP.Wreckage_MassMult = 1;

			CurrentValue = WreckageTable.RawGet("EnergyMult");
			if (CurrentValue != null)
				ToReturn.BP.Wreckage_EnergyMult = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				ToReturn.BP.Wreckage_EnergyMult = 1;

			CurrentValue = WreckageTable.RawGet("HealthMult");
			if (CurrentValue != null)
				ToReturn.BP.Wreckage_HealthMult = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				ToReturn.BP.Wreckage_HealthMult = 1;

			CurrentValue = WreckageTable.RawGet("ReclaimTimeMultiplier");
			if (CurrentValue != null)
				ToReturn.BP.Wreckage_ReclaimTimeMultiplier = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				ToReturn.BP.Wreckage_ReclaimTimeMultiplier = 1;

		}

		LuaTable IntelTable = BP.GetTable("UnitBlueprint.Intel");
		if (IntelTable != null)
		{
			CurrentValue = IntelTable.RawGet("VisionRadius");
			if (CurrentValue != null)
				ToReturn.BP.VisionRadius = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				ToReturn.BP.VisionRadius = 1;
		}

		CurrentValue = UnitBlueprintTable.RawGet("SizeX");
		if (CurrentValue != null)
			ToReturn.BP.Size.x = LuaParser.Read.StringToFloat(CurrentValue.ToString());

		CurrentValue = UnitBlueprintTable.RawGet("SizeY");
		if (CurrentValue != null)
			ToReturn.BP.Size.y = LuaParser.Read.StringToFloat(CurrentValue.ToString());

		CurrentValue = UnitBlueprintTable.RawGet("SizeZ");
		if (CurrentValue != null)
			ToReturn.BP.Size.z = LuaParser.Read.StringToFloat(CurrentValue.ToString());


		ToReturn.BP.SelectionSize = ToReturn.BP.Size;

		CurrentValue = UnitBlueprintTable.RawGet("SelectionSizeX");
		if (CurrentValue != null)
			ToReturn.BP.SelectionSize.x = LuaParser.Read.StringToFloat(CurrentValue.ToString());
		CurrentValue = UnitBlueprintTable.RawGet("SelectionSizeZ");
		if (CurrentValue != null)
			ToReturn.BP.SelectionSize.z = LuaParser.Read.StringToFloat(CurrentValue.ToString());
		CurrentValue = UnitBlueprintTable.RawGet("SelectionThickness");
		if (CurrentValue != null)
			ToReturn.BP.SelectionSize.y = LuaParser.Read.StringToFloat(CurrentValue.ToString());

		ToReturn.BP.SkirtSize = ToReturn.BP.SelectionSize;
		ToReturn.BP.SkirtSize.y = 1;

		PhisicsLayersTab = BP.GetTable("UnitBlueprint.Physics");
		if (PhisicsLayersTab != null)
		{
			CurrentValue = PhisicsLayersTab.RawGet("Elevation");
			if (CurrentValue != null)
				ToReturn.BP.PhysicsElevation = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("SkirtOffsetX");
			if (CurrentValue != null)
				ToReturn.BP.SkirtOffsetX = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("SkirtOffsetZ");
			if (CurrentValue != null)
				ToReturn.BP.SkirtOffsetZ = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("SkirtSizeX");
			if (CurrentValue != null)
				ToReturn.BP.SkirtSize.x = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("SkirtSizeZ");
			if (CurrentValue != null)
				ToReturn.BP.SkirtSize.z = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = PhisicsLayersTab.RawGet("TurnRate");
			if (CurrentValue != null)
				ToReturn.BP.TurnRate = LuaParser.Read.StringToFloat(CurrentValue.ToString());
		}

		ToReturn.BP.SkirtSize *= 0.1f;

		LuaTable FootprintTab = BP.GetTable("UnitBlueprint.Footprint");
		if (FootprintTab != null)
		{
			CurrentValue = FootprintTab.RawGet("SizeX");
			if (CurrentValue != null)
				ToReturn.BP.Footprint.x = LuaParser.Read.StringToFloat(CurrentValue.ToString());

			CurrentValue = FootprintTab.RawGet("SizeZ");
			if (CurrentValue != null)
				ToReturn.BP.Footprint.y = LuaParser.Read.StringToFloat(CurrentValue.ToString());
		}


		if (ToReturn.BP.LODs.Length == 0)
		{
			ToReturn.BP.GenerateEmptyLod();
		}
		else
		{
			for (int i = 0; i < ToReturn.BP.LODs.Length; i++)
			{
				if (i > 0)
					continue;

				ToReturn.BP.LODs[i].Mesh = LoadModel(ToReturn.BP.LODs[i].Scm);
				if (ToReturn.BP.LODs[i].Mesh == null)
					ToReturn.BP.LODs[i].Mesh = UnitsInfo.Current.NoUnitMesh;

				//ToReturn.BP.LODs[i].Mat = new Material(Shader.Find("Standard (Specular setup)"));
				ToReturn.BP.LODs[i].Mat = new Material(UnitsInfo.Current.UnitMaterial);

				ToReturn.BP.LODs[i].Mat.name = ToReturn.BP.CodeName + "_LOD" + i + " mat";

				if (ToReturn.BP.LODs[i].AlbedoName.Length == 0)
				{
					ToReturn.BP.LODs[i].AlbedoName = LocalPath.Replace("unit.bp", "albedo.dds");
				}
				else
				{
					ToReturn.BP.LODs[i].AlbedoName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].AlbedoName, true);
				}

				ToReturn.BP.LODs[i].Albedo = LoadTexture2D(ToReturn.BP.LODs[i].AlbedoName, false, true, true);
				ToReturn.BP.LODs[i].Albedo.anisoLevel = 2;
				ToReturn.BP.LODs[i].Mat.SetTexture(SHADER_MainTex, ToReturn.BP.LODs[i].Albedo);


				if (ToReturn.BP.LODs[i].NormalsName.Length == 0)
				{
					ToReturn.BP.LODs[i].NormalsName = LocalPath.Replace("unit.bp", "NormalsTS.dds");
				}
				else
				{
					ToReturn.BP.LODs[i].NormalsName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].NormalsName, true);
				}

				if (!string.IsNullOrEmpty(ToReturn.BP.LODs[i].NormalsName))
				{
					ToReturn.BP.LODs[i].Normal = LoadTexture2D(ToReturn.BP.LODs[i].NormalsName, true, true, true);
					ToReturn.BP.LODs[i].Normal.anisoLevel = 2;
					ToReturn.BP.LODs[i].Mat.SetTexture(SHADER_BumpMap, ToReturn.BP.LODs[i].Normal);
				}


				if (ToReturn.BP.LODs[i].SpecularName.Length == 0)
				{
					ToReturn.BP.LODs[i].SpecularName = LocalPath.Replace("unit.bp", "SpecTeam.dds");
				}
				else
				{
					ToReturn.BP.LODs[i].SpecularName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].SpecularName, true);
				}

				if (!string.IsNullOrEmpty(ToReturn.BP.LODs[i].SpecularName))
				{
					ToReturn.BP.LODs[i].Specular = LoadTexture2D(ToReturn.BP.LODs[i].SpecularName, false, true, true);
					ToReturn.BP.LODs[i].Specular.anisoLevel = 2;
					ToReturn.BP.LODs[i].Mat.SetTexture(SHADER_SpecTeam, ToReturn.BP.LODs[i].Specular);
				}
			}
		}

		ToReturn.RenderDistances = new float[] { BiggestLodDistance * 0.1f };
		ToReturn.ApplyLods();

		//	Debug.Log("Unit blueprint loaded: " + ToReturn.BP.CodeName + "\n" + ToReturn.BP.HelpText);
		//Debug.Log(ToReturn.BP.HelpText);
		//Debug.Log("StrategicIconName: " + ToReturn.BP.StrategicIconName);
		//Debug.Log("BuildTime: " + ToReturn.BP.BuildTime);
		//Debug.Log("BuildCostEnergy: " + ToReturn.BP.BuildCostEnergy + "\nBuildCostMass: " + ToReturn.BP.BuildCostMass);
		LoadedUnitObjects.Add(LocalPath, ToReturn);
		SortStrategicIcons();

		BP.Dispose();

		return ToReturn;
	}

	}
