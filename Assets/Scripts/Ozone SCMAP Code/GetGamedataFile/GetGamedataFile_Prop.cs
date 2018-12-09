using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NLua;
using EditMap;

public partial struct GetGamedataFile
{

	const float PropTexturesMipMapBias = 0.0f;

	static LOD[] Lods;

	public class PropObject
	{
		public PropBluePrint BP;

		public bool IsTemp = false;

		public PropGameObject CreatePropGameObject(Vector3 position, Quaternion rotation, Vector3 scale, bool AllowFarLod = true)
		{

			if (IsTemp)
			{
				IsTemp = false;
				for(int i = 0; i < BP.LODs.Length; i++)
				{
					if (BP.LODs[i].Albedo != null)
						AddToMemory(BP.LODs[i].AlbedoName, BP.LODs[i].Albedo);
					else if (BP.LODs[i].AlbedoLoaded)
					{
						BP.LODs[i].Albedo = LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, BP.LODs[i].AlbedoName, false, !IsTemp, true);
						BP.LODs[i].Albedo.anisoLevel = 2;
						BP.LODs[i].Mat.SetTexture("_MainTex", BP.LODs[i].Albedo);
					}

					if (BP.LODs[i].Normal != null)
						AddToMemory(BP.LODs[i].NormalsName, BP.LODs[i].Normal);
					else if (BP.LODs[i].NormalLoaded)
					{
						BP.LODs[i].Normal = LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, BP.LODs[i].NormalsName, true, !IsTemp, true);
						BP.LODs[i].NormalLoaded = BP.LODs[i].Normal != null;
						BP.LODs[i].Normal.anisoLevel = 2;
						BP.LODs[i].Mat.SetTexture("_BumpMap", BP.LODs[i].Normal);
					}
				}
			}

			//Reset scale, because it's not supported anyway
			scale = Vector3.one;

			PropGameObject NewProp = GameObject.Instantiate(PropsInfo.Current.PropObjectPrefab, PropsInfo.Current.PropsParent).GetComponent<PropGameObject>();
			NewProp.gameObject.name = BP.Name;

			if (BP.LODs.Length > 0)
			{
				if (BP.LODs[0].Mesh)
				{
					NewProp.Mf.sharedMesh = BP.LODs[0].Mesh;
					NewProp.Mr.sharedMaterial = BP.LODs[0].Mat;
					NewProp.Col.size = NewProp.Mf.sharedMesh.bounds.size;
				}
				bool Lod1Exist = BP.LODs.Length > 1 && BP.LODs[1].Mesh != null;
				if (Lod1Exist)
				{
					NewProp.Mf1.sharedMesh = BP.LODs[1].Mesh;
					NewProp.Mr1.sharedMaterial = BP.LODs[1].Mat;
				}
				else
				{
					NewProp.Mf1.gameObject.SetActive(false);
				}
				bool Lod2Exist = Lod1Exist && AllowFarLod && BP.LODs.Length > 2 && BP.LODs[2].Mesh != null;
				if (Lod2Exist)
				{
					NewProp.Mf2.sharedMesh = BP.LODs[2].Mesh;
					NewProp.Mr2.sharedMaterial = BP.LODs[2].Mat;
				}
				else
				{
					NewProp.Mf2.gameObject.SetActive(false);
				}

				

				scale.x *= BP.LocalScale.x;
				scale.y *= BP.LocalScale.y;
				scale.z *= BP.LocalScale.z;
				NewProp.Tr.localScale = scale;
				Lods = NewProp.Lodg.GetLODs();

				float DeltaSize = 0.01f;
				if (BP.LODs[0].Mesh != null)
				{
					Vector3 bs = BP.LODs[0].Mesh.bounds.size;
					DeltaSize = Mathf.Max(scale.x * bs.x, scale.y * bs.y, scale.z * bs.z);
					Lods[0].screenRelativeTransitionHeight = DeltaSize / DecalsInfo.FrustumHeightAtDistance(BP.LODs[0].LODCutoff * 0.1f);
				}
				if (Lod1Exist)
				{
					Vector3 bs = BP.LODs[1].Mesh.bounds.size;
					DeltaSize = Mathf.Max(scale.x * bs.x, scale.y * bs.y, scale.z * bs.z);
					Lods[1].screenRelativeTransitionHeight = DeltaSize / DecalsInfo.FrustumHeightAtDistance(BP.LODs[1].LODCutoff * 0.1f);
				}
				if (Lod2Exist)
				{
					Vector3 bs = BP.LODs[2].Mesh.bounds.size;
					DeltaSize = Mathf.Max(scale.x * bs.x, scale.y * bs.y, scale.z * bs.z);
					Lods[2].screenRelativeTransitionHeight = DeltaSize / DecalsInfo.FrustumHeightAtDistance(BP.LODs[2].LODCutoff * 0.1f);
				}


				if(!Lod1Exist && !Lod2Exist)
					Lods = new LOD[] { Lods[0] };
				else if(!Lod2Exist)
					Lods = new LOD[] { Lods[0], Lods[1] };

				NewProp.Lodg.SetLODs(Lods);

				NewProp.Tr.localPosition = position;
				NewProp.Tr.localRotation = rotation;
			}
			else
			{
				Debug.LogWarning("Prop is empty! " + BP.Path);
			}

			return NewProp;
		}

	}

	public class PropBluePrint
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

		public HashSet<string> Categories = new HashSet<string>();

		public bool RECLAIMABLE = false;

		public float SizeX = 1;
		public float SizeY = 1;
		public float SizeZ = 1;

		public Vector3 LocalScale;
	}

	public class BluePrintLoD
	{
		public bool AlbedoLoaded = false;
		public bool NormalLoaded = false;

		public string AlbedoName = "";
		public string NormalsName = "";
		public string SpecularName = "";
		public string ShaderName = "";
		public string Scm = "";
		public float LODCutoff = 1000;

		public Mesh Mesh;
		public Material Mat;
		public Texture2D Albedo;
		public Texture2D Normal;
		public Texture2D Specular;
	}

	public static string LocalBlueprintPath(string path)
	{
		if (path.StartsWith("/"))
			path = path.Remove(0, 1);

		return path;
	}

	public static PropObject LoadProp(string scdPath, bool IsTemp = false)
	{
		if (scdPath.StartsWith("maps"))
			scdPath = "/" + scdPath;

		if (scdPath.StartsWith("/maps")) { 
			return LoadProp(GetGamedataFile.MapScd, scdPath, IsTemp);
		}
		else
			return LoadProp(GetGamedataFile.EnvScd, LocalBlueprintPath(scdPath), IsTemp);
	}

	static Dictionary<string, PropObject> LoadedPropObjects = new Dictionary<string, PropObject>();

	public static PropObject LoadProp(string scd, string LocalPath, bool IsTemp = false)
	{
		if (LoadedPropObjects.ContainsKey(LocalPath))
			return LoadedPropObjects[LocalPath];


		PropObject ToReturn = new PropObject();

		ToReturn.IsTemp = IsTemp;

		byte[] Bytes = LoadBytes(scd, LocalPath);
		if (Bytes.Length == 0)
		{
			Debug.LogError("Prop does not exits: " + LocalPath);
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
		ToReturn.BP = new PropBluePrint();
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
			BP.DoString(MapLuaParser.Current.SaveLuaHeader.text + BluePrintString);
		}
		catch (NLua.Exceptions.LuaException e)
		{
			Debug.LogWarning(LuaParser.Read.FormatException(e) + "\n" + LocalPath);
			return ToReturn;
		}

		if (ToReturn.BP.Categories == null)
			ToReturn.BP.Categories = new HashSet<string>();
		else
			ToReturn.BP.Categories.Clear();

		if (BP.GetTable("PropBlueprint.Categories") != null)
		{
			string[] Categories = LuaParser.Read.StringArrayFromTable(BP.GetTable("PropBlueprint.Categories"));
			for(int i = 0; i < Categories.Length; i++)
				ToReturn.BP.Categories.Add(Categories[i]);

			ToReturn.BP.RECLAIMABLE = ToReturn.BP.Categories.Contains("RECLAIMABLE");
		}

		// Economy
		LuaTable EconomyTab = BP.GetTable("PropBlueprint.Economy");
		if (EconomyTab != null)
		{
			if (EconomyTab.RawGet("ReclaimEnergyMax") != null)
				ToReturn.BP.ReclaimEnergyMax = LuaParser.Read.StringToFloat(EconomyTab.RawGet("ReclaimEnergyMax").ToString());

			if (EconomyTab.RawGet("ReclaimMassMax") != null)
				ToReturn.BP.ReclaimMassMax = LuaParser.Read.StringToFloat(EconomyTab.RawGet("ReclaimMassMax").ToString());

			if (EconomyTab.RawGet("ReclaimTime") != null)
				ToReturn.BP.ReclaimTime = LuaParser.Read.StringToFloat(EconomyTab.RawGet("ReclaimTime").ToString());
		}

		//Size
		object SizeX = BP.GetTable("PropBlueprint").RawGet("SizeX");
		if (SizeX != null)
			ToReturn.BP.SizeX = LuaParser.Read.StringToFloat(SizeX.ToString());

		object SizeY = BP.GetTable("PropBlueprint").RawGet("SizeY");
		if (SizeY != null)
			ToReturn.BP.SizeY = LuaParser.Read.StringToFloat(SizeY.ToString());

		object SizeZ = BP.GetTable("PropBlueprint").RawGet("SizeZ");
		if (SizeZ != null)
			ToReturn.BP.SizeY = LuaParser.Read.StringToFloat(SizeZ.ToString());


		//Display
		if (BP.GetTable("PropBlueprint.Display") != null)
		{
			if (BP.GetTable("PropBlueprint.Display").RawGet("UniformScale") != null)
				ToReturn.BP.UniformScale = LuaParser.Read.StringToFloat(BP.GetTable("PropBlueprint.Display").RawGet("UniformScale").ToString());

			// Mesh
			if (BP.GetTable("PropBlueprint.Display.Mesh") != null)
			{
				if (BP.GetTable("PropBlueprint.Display.Mesh").RawGet("IconFadeInZoom") != null)
					ToReturn.BP.IconFadeInZoom = LuaParser.Read.StringToFloat(BP.GetTable("PropBlueprint.Display.Mesh").RawGet("IconFadeInZoom").ToString());

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
					{
						ToReturn.BP.LODs[i].NormalsName = LodTableValues[i].RawGet("NormalsName").ToString();
					}

					if (LodTableValues[i].RawGet("ShaderName") != null)
						ToReturn.BP.LODs[i].ShaderName = LodTableValues[i].RawGet("ShaderName").ToString();

					if (LodTableValues[i].RawGet("LODCutoff") != null)
						ToReturn.BP.LODs[i].LODCutoff = LuaParser.Read.StringToFloat(LodTableValues[i].RawGet("LODCutoff").ToString());

					ToReturn.BP.LODs[i].Scm = LocalPath.Replace("prop.bp", "lod" + i.ToString() + ".scm");
				}
			}
		}

		for (int i = 0; i < ToReturn.BP.LODs.Length; i++)
		{
			ToReturn.BP.LODs[i].Mesh = LoadModel(scd, ToReturn.BP.LODs[i].Scm);

			//ToReturn.BP.LODs[i].Mat = new Material(Shader.Find("Standard (Specular setup)"));
			ToReturn.BP.LODs[i].Mat = new Material(EditMap.PropsInfo.Current.PropMaterial);

			ToReturn.BP.LODs[i].Mat.name = ToReturn.BP.Name + " mat";



			if(i > 0 && (string.IsNullOrEmpty(ToReturn.BP.LODs[i].AlbedoName) || ToReturn.BP.LODs[i].AlbedoName == ToReturn.BP.LODs[0].AlbedoName)
				&& (string.IsNullOrEmpty(ToReturn.BP.LODs[i].NormalsName) || ToReturn.BP.LODs[i].NormalsName == ToReturn.BP.LODs[0].NormalsName))
			{

				ToReturn.BP.LODs[i].Mat = ToReturn.BP.LODs[0].Mat;
				continue;
			}

			if (ToReturn.BP.LODs[i].AlbedoName.Length == 0)
			{
				ToReturn.BP.LODs[i].AlbedoName = LocalPath.Replace("prop.bp", "albedo.dds");
			}
			else
			{
				ToReturn.BP.LODs[i].AlbedoName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].AlbedoName, true);
			}

			ToReturn.BP.LODs[i].Albedo = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].AlbedoName, false, !IsTemp, true);
			ToReturn.BP.LODs[i].AlbedoLoaded = ToReturn.BP.LODs[i].Albedo != null;
			if(ToReturn.BP.LODs[i].AlbedoLoaded)
				ToReturn.BP.LODs[i].Albedo.anisoLevel = 2;
				ToReturn.BP.LODs[i].Mat.SetTexture("_MainTex", ToReturn.BP.LODs[i].Albedo);


			if (ToReturn.BP.LODs[i].ShaderName == "VertexNormal")
			{
				ToReturn.BP.LODs[i].NormalsName = "";
			}
			else if (ToReturn.BP.LODs[i].NormalsName.Length == 0 && i == 0)
			{
				ToReturn.BP.LODs[i].NormalsName = LocalPath.Replace("prop.bp", "normalsTS.dds");
			}
			else
			{
				ToReturn.BP.LODs[i].NormalsName = OffsetRelativePath(LocalPath, ToReturn.BP.LODs[i].NormalsName, true);
			}

			if(ToReturn.BP.LODs[i].ShaderName == "NormalMappedTerrain")
			{
				ToReturn.BP.LODs[i].Mat.SetInt("_GlowAlpha", 1);
			}

			if (!string.IsNullOrEmpty(ToReturn.BP.LODs[i].NormalsName))
			{
				ToReturn.BP.LODs[i].Normal = LoadTexture2DFromGamedata(scd, ToReturn.BP.LODs[i].NormalsName, true, !IsTemp, true);
				ToReturn.BP.LODs[i].NormalLoaded = ToReturn.BP.LODs[i].Normal != null;
				if (ToReturn.BP.LODs[i].NormalLoaded)
				{
					ToReturn.BP.LODs[i].Normal.anisoLevel = 2;
					ToReturn.BP.LODs[i].Mat.SetTexture("_BumpMap", ToReturn.BP.LODs[i].Normal);
				}
			}
		}
		if(ToReturn.BP.LODs.Length <= 0){
			ToReturn.BP.LODs = new BluePrintLoD[1];
			ToReturn.BP.LODs[0] = new BluePrintLoD();
			ToReturn.BP.LODs[0].LODCutoff = 100;
		}

		if (ToReturn.BP.LODs[0].Mesh == null)
		{
			ToReturn.BP.LODs[0].Mesh = PropsInfo.Current.NoPropMesh;
			ToReturn.BP.UniformScale = 2f;
		}

		if (ToReturn.BP.LODs[0].Mat == null)
			ToReturn.BP.LODs[0].Mat = PropsInfo.Current.NoPropMaterial;

		if (ToReturn.BP.UniformScale <= 0.01f)
			ToReturn.BP.UniformScale = 0.01f;

		ToReturn.BP.LocalScale = Vector3.one * (ToReturn.BP.UniformScale * 0.1f);



		LoadedPropObjects.Add(LocalPath, ToReturn);

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
