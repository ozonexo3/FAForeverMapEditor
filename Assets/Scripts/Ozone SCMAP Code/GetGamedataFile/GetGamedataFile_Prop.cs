using UnityEngine;
using System.Collections;
using System;
using NLua;
using EditMap;

public partial struct GetGamedataFile
{

	const float PropTexturesMipMapBias = 0.0f;

	static LOD[] Lods;

	public class PropObject
	{
		public BluePrint BP;


		public PropGameObject CreatePropGameObject(Vector3 position, Quaternion rotation, Vector3 scale)
		{

			PropGameObject NewProp = GameObject.Instantiate(PropsInfo.Current.PropObjectPrefab, PropsInfo.Current.PropsParent).GetComponent<PropGameObject>();
			NewProp.gameObject.name = BP.Name;

			if (BP.LODs.Length > 0)
			{
				NewProp.Mf.sharedMesh = BP.LODs[0].Mesh;
				NewProp.Mr.sharedMaterial = BP.LODs[0].Mat;

				float DeltaSize = 0.01f;
				if (BP.LODs[0].Mesh != null)
				{
					DeltaSize = Mathf.Max(BP.LODs[0].Mesh.bounds.size.x, BP.LODs[0].Mesh.bounds.size.y);
					DeltaSize = Mathf.Max(DeltaSize, BP.LODs[0].Mesh.bounds.size.z);

					if (DeltaSize < 0.01f)
						DeltaSize = 0.01f;
				}

				Lods = NewProp.Lodg.GetLODs();
				Lods[0].screenRelativeTransitionHeight = Mathf.Lerp(0.018f, 0.20f, Mathf.Pow((DeltaSize - 1.9f) / 190f, 2f));
				NewProp.Lodg.SetLODs(Lods);

				NewProp.Tr.localPosition = position;
				NewProp.Tr.localRotation = rotation;
				scale.x *= BP.LocalScale.x;
				scale.y *= BP.LocalScale.y;
				scale.z *= BP.LocalScale.z;
				NewProp.Tr.localScale = scale;

			}
			else
			{
				Debug.LogError("Prop is empty! " + BP.Path);
			}

			return NewProp;
		}

	}

	public class BluePrint
	{
		public string Path;
		public string Name = "";
		public string HelpText = "";
		//Display
		public BluePrintLoD[] LODs = new BluePrintLoD[0];
		public float UniformScale = 0.1f;
		public float IconFadeInZoom = 4;

		public float ReclaimEnergyMax;
		public float ReclaimMassMax;
		public float ReclaimTime = 1;

		public float SizeX = 1;
		public float SizeY = 1;
		public float SizeZ = 1;

		public Vector3 LocalScale;
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
		public Texture2D Albedo;
		public Texture2D Normal;
	}


	public static PropObject LoadProp(string scd, string LocalPath)
	{
		PropObject ToReturn = new PropObject();

		byte[] Bytes = LoadBytes(scd, LocalPath);
		if (Bytes.Length == 0)
		{
			Debug.LogError("Prop not exits: " + LocalPath);
			return ToReturn;
		}
		string BluePrintString = System.Text.Encoding.UTF8.GetString(Bytes);

		if (BluePrintString.Length == 0)
		{
			Debug.LogError("Loaded blueprint is empty");
			return ToReturn;
		}

		BluePrintString = BluePrintString.Replace("PropBlueprint {", "PropBlueprint = {");

		// *** Parse Blueprint
		ToReturn.BP = new BluePrint();
		// Create Lua
		Lua BP = new Lua();
		BP.LoadCLRPackage();

		string[] PathSplit = LocalPath.Split(("/").ToCharArray());
		ToReturn.BP.Name = PathSplit[PathSplit.Length - 1].Replace(".bp", "");


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
			BP.DoString(MapLuaParser.GetLoadedFileFunctions() + BluePrintString);
		}
		catch (NLua.Exceptions.LuaException e)
		{
			Debug.LogError(ParsingStructureData.FormatException(e) + "\n" + LocalPath);
			return ToReturn;
		}

		// Economy
		if (BP.GetTable("PropBlueprint.Economy") != null)
		{
			LuaTable EconomyTab = BP.GetTable("PropBlueprint.Economy") as LuaTable;

			if (EconomyTab != null)
			{
				if (EconomyTab.RawGet("ReclaimEnergyMax") != null)
					ToReturn.BP.ReclaimEnergyMax = MassMath.StringToFloat(EconomyTab.RawGet("ReclaimEnergyMax").ToString());

				if (EconomyTab.RawGet("ReclaimMassMax") != null)
					ToReturn.BP.ReclaimMassMax = MassMath.StringToFloat(EconomyTab.RawGet("ReclaimMassMax").ToString());

				if (EconomyTab.RawGet("ReclaimTime") != null)
					ToReturn.BP.ReclaimTime = MassMath.StringToFloat(EconomyTab.RawGet("ReclaimTime").ToString());
			}
		}

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
			if (BP.GetTable("PropBlueprint.Display").RawGet("UniformScale") != null)
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
				}
			}
		}

		ToReturn.BP.LocalScale = Vector3.one * (ToReturn.BP.UniformScale * 0.1f);

		for (int i = 0; i < ToReturn.BP.LODs.Length; i++)
		{
			ToReturn.BP.LODs[i].Mesh = LoadModel(scd, ToReturn.BP.LODs[i].Scm);

			ToReturn.BP.LODs[i].Mat = new Material(Shader.Find("Standard (Specular setup)"));

			ToReturn.BP.LODs[i].Mat.name = ToReturn.BP.Name + " mat";

			{ // Set AlphaTest standard shader
				ToReturn.BP.LODs[i].Mat.SetFloat("_Mode", 1);
				ToReturn.BP.LODs[i].Mat.SetOverrideTag("RenderType", "TransparentCutout");
				ToReturn.BP.LODs[i].Mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				ToReturn.BP.LODs[i].Mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				ToReturn.BP.LODs[i].Mat.SetInt("_ZWrite", 1);
				ToReturn.BP.LODs[i].Mat.EnableKeyword("_ALPHATEST_ON");
				ToReturn.BP.LODs[i].Mat.DisableKeyword("_ALPHABLEND_ON");
				ToReturn.BP.LODs[i].Mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				ToReturn.BP.LODs[i].Mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

				ToReturn.BP.LODs[i].Mat.SetColor("_SpecColor", Color.black);
				ToReturn.BP.LODs[i].Mat.SetFloat("_Glossiness", 1);
				ToReturn.BP.LODs[i].Mat.enableInstancing = true;
			}

			if (ToReturn.BP.LODs[i].AlbedoName.Length == 0)
			{
				ToReturn.BP.LODs[i].AlbedoName = LocalPath.Replace("prop.bp", "albedo.dds");
			}
			else
			{
				ToReturn.BP.LODs[i].AlbedoName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].AlbedoName, true);
			}

			ToReturn.BP.LODs[i].Albedo = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].AlbedoName, false);
			if(ToReturn.BP.LODs[i].Albedo != null)
				ToReturn.BP.LODs[i].Albedo.mipMapBias = PropTexturesMipMapBias;
			ToReturn.BP.LODs[i].Mat.SetTexture("_MainTex", ToReturn.BP.LODs[i].Albedo);


			if (ToReturn.BP.LODs[i].NormalsName.Length == 0)
			{
				ToReturn.BP.LODs[i].NormalsName = LocalPath.Replace("prop.bp", "normalsTS.dds");
			}
			else
			{
				ToReturn.BP.LODs[i].NormalsName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].NormalsName, true);
			}

			ToReturn.BP.LODs[i].Normal = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].NormalsName, true);
			if (ToReturn.BP.LODs[i].Normal != null)
				ToReturn.BP.LODs[i].Normal.mipMapBias = PropTexturesMipMapBias;
			ToReturn.BP.LODs[i].Mat.SetTexture("_BumpMap", ToReturn.BP.LODs[i].Normal);

		}

		return ToReturn;
	}


static string OffsetRelativePath(string OriginalPath, string offset, bool File = true)
	{
		OriginalPath = OriginalPath.Replace("\\", "/");
		offset = offset.Replace("\\", "/");

		string[] Folders = OriginalPath.Split("/".ToCharArray());

		int Step = Folders.Length;
		if (File && Step > 0)
			Step--;

		while (offset.StartsWith("../"))
		{
			offset = offset.Remove(0, 3);
			Step--;
		}


		string ToReturn = "";

		for (int i = 0; i < Folders.Length; i++)
		{
			if (i >= Step)
				break;
			else
				ToReturn += Folders[i] + "/";
		}

		return ToReturn + offset;

	}


}
