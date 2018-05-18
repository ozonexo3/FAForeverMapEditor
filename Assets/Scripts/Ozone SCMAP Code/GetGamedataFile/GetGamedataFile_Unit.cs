using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using NLua;
using EditMap;

public partial struct GetGamedataFile
{

	public class UnitObject : MonoBehaviour
	{
		public UnitBluePrint BP;


		public void CreateUnitObject(Vector3 position, Quaternion rotation)
		{
			GameObject Obj = new GameObject(BP.CodeName);

			Obj.transform.localPosition = ScmapEditor.SnapToTerrain(position);
			Obj.transform.localRotation = rotation;
			Obj.transform.localScale = BP.UniformScale * 0.1f;

			MeshRenderer mr = Obj.AddComponent<MeshRenderer>();
			MeshFilter mf = Obj.AddComponent<MeshFilter>();

			mr.material = BP.LODs[0].Mat;
			mf.mesh = BP.LODs[0].Mesh;
		}
	}

	public class UnitBluePrint
	{
		public string CodeName = "";
		public string Path;
		public string Name = "";
		public string HelpText = "";

		// Display
		public BluePrintLoD[] LODs = new BluePrintLoD[0];

		public string[] Categories;
		public string GeneralCategory;
		public string GeneralClassification;

		public Vector3 SelectionSize;
		public Vector3 Size;
		public Vector3 UniformScale;

		// Strategic
		public string StrategicIconName;
		public int StrategicIconSortPriority;

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
	}


	public static UnitObject LoadUnit(string UnitCode)
	{
		string scdPath = "units/" + UnitCode + "/" + UnitCode + "_unit.bp";

		return LoadUnit(GetGamedataFile.UnitsScd, LocalBlueprintPath(scdPath));
	}


	static Dictionary<string, UnitObject> LoadedUnitObjects = new Dictionary<string, UnitObject>();

	public static UnitObject LoadUnit(string scd, string LocalPath)
	{
		if (LoadedUnitObjects.ContainsKey(LocalPath))
			return LoadedUnitObjects[LocalPath];


		UnitObject ToReturn = new UnitObject();

		byte[] Bytes = LoadBytes(scd, LocalPath);
		if (Bytes.Length == 0)
		{
			Debug.LogError("Unit does not exits: " + LocalPath);
			return ToReturn;
		}
		string BluePrintString = System.Text.Encoding.UTF8.GetString(Bytes);

		if (BluePrintString.Length == 0)
		{
			Debug.LogError("Loaded blueprint is empty");
			return ToReturn;
		}

		BluePrintString = BluePrintString.Replace("UnitBlueprint {", "UnitBlueprint = {");

		// *** Parse Blueprint
		ToReturn.BP = new UnitBluePrint();
		// Create Lua
		Lua BP = new Lua();
		BP.LoadCLRPackage();

		string[] PathSplit = LocalPath.Split(("/").ToCharArray());
		ToReturn.BP.Name = PathSplit[PathSplit.Length - 1].Replace(".bp", "");
		ToReturn.BP.CodeName = ToReturn.BP.Name.Replace("_unit", "");

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
			Debug.LogError(LuaParser.Read.FormatException(e) + "\n" + LocalPath);
			return ToReturn;
		}


		//TODO
		// Load Blueprint Data
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
			ToReturn.BP.StrategicIconName = CurrentValue.ToString();

		CurrentValue = EconomyTab.RawGet("StrategicIconSortPriority");
		if (CurrentValue != null)
			ToReturn.BP.StrategicIconSortPriority = LuaParser.Read.StringToInt(CurrentValue.ToString());




		//Display
		if (BP.GetTable("UnitBlueprint.Display") != null)
		{
			if (BP.GetTable("UnitBlueprint.Display").RawGet("UniformScale") != null)
				ToReturn.BP.UniformScale = Vector3.one * LuaParser.Read.StringToFloat(BP.GetTable("UnitBlueprint.Display").RawGet("UniformScale").ToString());

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
						ToReturn.BP.LODs[i].LODCutoff = LuaParser.Read.StringToFloat(CurrentValue.ToString());


					ToReturn.BP.LODs[i].Scm = LocalPath.Replace("unit.bp", "lod" + i.ToString() + ".scm");
				}
			}
		}

		ToReturn.BP.Size = Vector3.one * 0.1f;

		CurrentValue = UnitBlueprintTable.RawGet("SizeX");
		if (CurrentValue != null)
			ToReturn.BP.Size.x = LuaParser.Read.StringToFloat(CurrentValue.ToString());

		CurrentValue = UnitBlueprintTable.RawGet("SizeY");
		if (CurrentValue != null)
			ToReturn.BP.Size.y = LuaParser.Read.StringToFloat(CurrentValue.ToString());

		CurrentValue = UnitBlueprintTable.RawGet("SizeZ");
		if (CurrentValue != null)
			ToReturn.BP.Size.z = LuaParser.Read.StringToFloat(CurrentValue.ToString());


		for (int i = 0; i < ToReturn.BP.LODs.Length; i++)
		{
			ToReturn.BP.LODs[i].Mesh = LoadModel(scd, ToReturn.BP.LODs[i].Scm);

			//ToReturn.BP.LODs[i].Mat = new Material(Shader.Find("Standard (Specular setup)"));
			ToReturn.BP.LODs[i].Mat = new Material(EditMap.PropsInfo.Current.UnitMaterial);

			ToReturn.BP.LODs[i].Mat.name = ToReturn.BP.CodeName + "_LOD" + i + " mat";

			if (ToReturn.BP.LODs[i].AlbedoName.Length == 0)
			{
				ToReturn.BP.LODs[i].AlbedoName = LocalPath.Replace("unit.bp", "albedo.dds");
			}
			else
			{
				ToReturn.BP.LODs[i].AlbedoName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].AlbedoName, true);
			}

			ToReturn.BP.LODs[i].Albedo = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].AlbedoName, false);
			ToReturn.BP.LODs[i].Albedo.anisoLevel = 2;
			ToReturn.BP.LODs[i].Mat.SetTexture("_MainTex", ToReturn.BP.LODs[i].Albedo);


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
				ToReturn.BP.LODs[i].Normal = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].NormalsName, true);
				ToReturn.BP.LODs[i].Normal.anisoLevel = 2;
				ToReturn.BP.LODs[i].Mat.SetTexture("_BumpMap", ToReturn.BP.LODs[i].Normal);
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
				ToReturn.BP.LODs[i].Specular = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].SpecularName, false);
				ToReturn.BP.LODs[i].Specular.anisoLevel = 2;
				ToReturn.BP.LODs[i].Mat.SetTexture("_SpecTeam", ToReturn.BP.LODs[i].Specular);
			}

		}





			Debug.Log("Unit blueprint loaded: " + ToReturn.BP.CodeName + "\n" + ToReturn.BP.HelpText);
		//Debug.Log(ToReturn.BP.HelpText);
		Debug.Log("StrategicIconName: " + ToReturn.BP.StrategicIconName);
		Debug.Log("BuildTime: " + ToReturn.BP.BuildTime);
		Debug.Log("BuildCostEnergy: " + ToReturn.BP.BuildCostEnergy + "\nBuildCostMass: " + ToReturn.BP.BuildCostMass);

		return ToReturn;
	}

	}
