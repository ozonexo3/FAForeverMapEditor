using UnityEngine;
using System.Collections;
using System;
using NLua;

public partial class GetGamedataFile : MonoBehaviour {

	public class PropObject
	{
		public BluePrint BP;
	}

	public class BluePrint
	{
		public string HelpText = "";
		//Display
		public BluePrintLoD[] LODs = new BluePrintLoD[0];
		public float UniformScale = 0.1f;
		public float IconFadeInZoom = 4;

		public int ReclaimEnergyMax;
		public int ReclaimMassMax;
		public float ReclaimTime = 1;

		public float SizeX = 1;
		public float SizeY = 1;
		public float SizeZ = 1;
	}

	public class BluePrintLoD
	{
		public string AlbedoName = "";
		public string NormalsName = "";
		public string ShaderName = "";
		public string Scm = "";
		public float LODCutoff = 1000;

		public Mesh Mesh;
		public Material Mat;
	}


	public static void LoadProp(string scd, string LocalPath)
	{
		byte[] Bytes = LoadBytes(scd, LocalPath);
		if (Bytes.Length == 0)
		{
			Debug.LogError("Prop not exits: " + LocalPath);
			return;
		}
		string BluePrintString = System.Text.Encoding.UTF8.GetString(Bytes);

		Debug.Log(BluePrintString);

		if(BluePrintString.Length == 0)
		{
			Debug.LogError("Loaded blueprint is empty");
			return;
		}

		BluePrintString = BluePrintString.Replace("PropBlueprint {", "PropBlueprint = {");

		// *** Parse Blueprint
		PropObject ToReturn = new PropObject();
		ToReturn.BP = new BluePrint();
		// Create Lua
		Lua BP = new Lua();
		BP.LoadCLRPackage();

		try
		{
			BP.DoString(MapLuaParser.GetLoadedFileFunctions() + BluePrintString);
		}
		catch (NLua.Exceptions.LuaException e)
		{
			Debug.LogError(ParsingStructureData.FormatException(e));
			return;
		}

		// Economy
		if (BP.GetTable("PropBlueprint.Economy") != null)
		{
			LuaTable EconomyTab = BP.GetTable("PropBlueprint.Economy") as LuaTable;

			if(EconomyTab.RawGet("ReclaimEnergyMax") != null)
				ToReturn.BP.ReclaimEnergyMax = int.Parse(EconomyTab.RawGet("ReclaimEnergyMax").ToString());

			if (EconomyTab.RawGet("ReclaimMassMax") != null)
				ToReturn.BP.ReclaimMassMax = int.Parse(EconomyTab.RawGet("ReclaimMassMax").ToString());

			if (EconomyTab.RawGet("ReclaimTime") != null)
				ToReturn.BP.ReclaimTime = MassMath.StringToFloat(EconomyTab.RawGet("ReclaimTime").ToString());
		}

		Debug.Log(ToReturn.BP.ReclaimEnergyMax + ", " + ToReturn.BP.ReclaimMassMax + ", " + ToReturn.BP.ReclaimTime);

		//Size
		if (BP.GetTable("PropBlueprint").RawGet("SizeX") != null)
			ToReturn.BP.SizeX = MassMath.StringToFloat(BP.GetTable("PropBlueprint").RawGet("SizeX").ToString());

		if (BP.GetTable("PropBlueprint").RawGet("SizeY") != null)
			ToReturn.BP.SizeY = MassMath.StringToFloat(BP.GetTable("PropBlueprint").RawGet("SizeY").ToString());

		if (BP.GetTable("PropBlueprint").RawGet("SizeZ") != null)
			ToReturn.BP.SizeY = MassMath.StringToFloat(BP.GetTable("PropBlueprint").RawGet("SizeZ").ToString());


		//Display
		if (BP.GetTable("PropBlueprint.Display") != null)
		{
			if(BP.GetTable("PropBlueprint.Display").RawGet("UniformScale") != null)
				ToReturn.BP.UniformScale = MassMath.StringToFloat(BP.GetTable("PropBlueprint.Display").RawGet("UniformScale").ToString());

			// Mesh
			if (BP.GetTable("PropBlueprint.Display.Mesh") != null)
			{
				if (BP.GetTable("PropBlueprint.Display.Mesh").RawGet("IconFadeInZoom") != null)
					ToReturn.BP.IconFadeInZoom = MassMath.StringToFloat(BP.GetTable("PropBlueprint.Display.Mesh").RawGet("IconFadeInZoom").ToString());

				//Lods
				LuaTable LodTable = BP.GetTable("PropBlueprint.Display.Mesh.LODs");

				LuaTable[] LodTableValues = new LuaTable[LodTable.Keys.Count];
				ToReturn.BP.LODs = new BluePrintLoD[LodTableValues.Length];
				LodTable.Values.CopyTo(LodTableValues, 0);

				for (int i = 0; i < LodTableValues.Length; i++)
				{
					ToReturn.BP.LODs[i] = new BluePrintLoD();

					if (LodTableValues[i].RawGet("AlbedoName") != null)
						ToReturn.BP.LODs[i].AlbedoName = LodTableValues[i].RawGet("AlbedoName").ToString();

					if (LodTableValues[i].RawGet("NormalsName") != null)
						ToReturn.BP.LODs[i].NormalsName = LodTableValues[i].RawGet("NormalsName").ToString();

					if (LodTableValues[i].RawGet("ShaderName") != null)
						ToReturn.BP.LODs[i].ShaderName = LodTableValues[i].RawGet("ShaderName").ToString();

					if (LodTableValues[i].RawGet("LODCutoff") != null)
						ToReturn.BP.LODs[i].LODCutoff = MassMath.StringToFloat(LodTableValues[i].RawGet("LODCutoff").ToString());

					ToReturn.BP.LODs[i].Scm = LocalPath.Replace("prop.bp", "lod" + i.ToString() + ".scm");

					Debug.Log(ToReturn.BP.LODs[i].AlbedoName + ", " + ToReturn.BP.LODs[i].NormalsName + ", " + ToReturn.BP.LODs[i].ShaderName + ", " + ToReturn.BP.LODs[i].LODCutoff + ", " + ToReturn.BP.LODs[i].Scm);

				}
			}
		}

		Debug.Log(ToReturn.BP.UniformScale + ", " + ToReturn.BP.IconFadeInZoom);
		string ParentDirectory = System.IO.Directory.GetParent(LocalPath).FullName;
		string Difference = ParentDirectory.Replace("\\", "/").Replace(LocalPath.Replace("\\", "/"), "");

		Debug.Log(ParentDirectory + " , " + Application.persistentDataPath);

		for (int i = 0; i < ToReturn.BP.LODs.Length; i++)
		{
			ToReturn.BP.LODs[i].Mesh = LoadModel(scd, ToReturn.BP.LODs[i].Scm);

			ToReturn.BP.LODs[i].Mat = new Material(Shader.Find("Standard"));


			if (ToReturn.BP.LODs[i].AlbedoName.Length > 0)
			{
				string LocalDirectory = ParentDirectory;
				while (ToReturn.BP.LODs[i].AlbedoName.StartsWith("../"))
				{
					LocalDirectory = System.IO.Directory.GetParent(LocalDirectory).FullName;
					ToReturn.BP.LODs[i].AlbedoName = ToReturn.BP.LODs[i].AlbedoName.Remove(0, 3);
				}
				ToReturn.BP.LODs[i].AlbedoName = LocalDirectory.Replace("\\", "/").Replace(Difference, "") + "/" + ToReturn.BP.LODs[i].AlbedoName;

				Debug.Log(ToReturn.BP.LODs[i].AlbedoName);

				ToReturn.BP.LODs[i].Mat.SetTexture("_MainTex", LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].AlbedoName, false));


			}

		}
	}


}
