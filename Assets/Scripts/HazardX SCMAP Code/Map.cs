// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: Map.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
[System.Serializable, PreferBinarySerialization]
#endif
public class Map
{

    // Map
    private const int MAP_MAGIC = 0x1a70614d;

    private const int MAP_VERSION = 2;


    public string Filename;

//    public Bitmap PreviewBitmap;

	public	GetGamedataFile.HeaderClass		PreviewTextHeader;
	public	GetGamedataFile.HeaderClass		TextureMapHeader;
	public	GetGamedataFile.HeaderClass		TextureMap2Header;
	public	GetGamedataFile.HeaderClass		NormalmapHeader;
	public	GetGamedataFile.HeaderClass		WatermapHeader;

	public Texture2D PreviewTex;
	[HideInInspector]
	public short[] HeightmapData = new short[0];
    public short HeightMin;
    public short HeightMax;

    public short HeightDiff;

    public WaterShader Water = new WaterShader();
    //texturemap for the "Strata" layers
	public Texture2D TexturemapTex;
	public Texture2D TexturemapTex2;
	public Texture2D NormalmapTex;
	public Texture2D WatermapTex;
	public Texture2D WaterDataTexture;
	[HideInInspector]
	public byte[] WaterFoamMask = new byte[0];
	[HideInInspector]
	public byte[] WaterFlatnessMask = new byte[0];

	[HideInInspector]
	public byte[] WaterDepthBiasMask = new byte[0];
    public int Width = 0;
    public int Height = 0;

    public string TexPathBackground;
    public string TexPathSkyCubemap;
    public string[] EnvCubemapsName;
    public string[] EnvCubemapsFile;
	public SkyboxData AdditionalSkyboxData = new SkyboxData();

	[HideInInspector]
	public byte[] TerrainTypeData;
	
	[HideInInspector]
    public List<WaveGenerator> WaveGenerators = new List<WaveGenerator>();
    public List<Layer> Layers = new List<Layer>();
    public List<Decal> Decals = new List<Decal>();
    public List<IntegerGroup> DecalGroups = new List<IntegerGroup>();

    public List<Prop> Props = new List<Prop>();
    public int VersionMinor;

    public int VersionMajor;
    public float HeightScale;

    public string TerrainShader;
    public float LightingMultiplier;
    public Vector3 SunDirection;
    public Vector3 SunAmbience;
    public Vector3 SunColor;
    public Vector3 ShadowFillColor;
    public Vector4 SpecularColor;

    public float Bloom;
    public Vector3 FogColor;
    public float FogStart;
    public float FogEnd;
    public int Unknown10;
    public int Unknown11;
    public float Unknown12;

	public byte Unknown15;
	public Single Unknown16;
	public Single Unknown17;

	public short Unknown13;
    public int Unknown7;

    public int Unknown8;
    public float Unknown14;

    //Minimap Cartographic View Colors (Not in Hazard's Original Code)
    public int MinimapContourInterval;
    public Color32 MinimapDeepWaterColor;
    public Color32 MinimapShoreColor;
    public Color32 MinimapLandStartColor;
    public Color32 MinimapLandEndColor;
    public Color32 MinimapContourColor; //Not sure about this one


	//<Merge Conflict>
	public void Initialize()
    {
		PreviewTex = new Texture2D(0, 0);
		TexturemapTex = new Texture2D(0, 0);
		TexturemapTex2 = new Texture2D(0, 0);
		NormalmapTex = new Texture2D(0, 0);
		WatermapTex = new Texture2D(0, 0);
		WaterDataTexture = new Texture2D(0, 0);

		TerrainTypeData = new byte[Height * Width];
//		TerrainTypeData = new List<byte>();
        HeightmapData = new short[(Height + 1) * (Width + 1)];
        WaterDepthBiasMask = new byte[(Height * Width) / 4];
        WaterFlatnessMask = new byte[(Height * Width) / 4];
        WaterFoamMask = new byte[(Height * Width) / 4];
        for (int i = 0; i <= WaterDepthBiasMask.Length - 1; i++)
        {
            WaterDepthBiasMask[i] = 127;
            WaterFlatnessMask[i] = 255;
            WaterFoamMask[i] = 0;
        }

        //Version
        VersionMajor = 2;
        VersionMinor = 56;

        HeightScale = 0.0078125f;


	//Unknown Values

	Unknown8 = 0;
        Unknown10 = -1091567891;
        Unknown11 = 2;
        Unknown12 = 0;
        Unknown13 = 0;
        Unknown14 = 0;
		Unknown15 = 0;

		//Minimap Colors (Default)
		MinimapContourInterval = 24;
        MinimapDeepWaterColor = new Color(71, 140, 181);
		MinimapContourColor = new Color(112, 112, 112);
		MinimapShoreColor = new Color(140, 201, 224);
		MinimapLandStartColor = new Color(117, 99, 107);
		MinimapLandEndColor = new Color(206, 206, 176);
    }


	public Map()
	{

	}

	const double HeightResize = 128.0 * 256.0; //512 * 40;
	public Map(int _Width, int _Height, int InitialHeight, bool _Water, int WaterLevel, int DepthLevel, int AbyssLevel)
	{
		Width = _Width;
		Height = _Height;
		
		PreviewTex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
		TexturemapTex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
		TexturemapTex2 = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
		NormalmapTex = new Texture2D(Width, Height, TextureFormat.DXT5, false);
		WatermapTex = new Texture2D(Width, Height, TextureFormat.DXT5, false);
		UncompressedWatermapTex = new Texture2D(WatermapTex.width, WatermapTex.height, TextureFormat.RGBA32, false);
		UncompressedNormalmapTex = new Texture2D(NormalmapTex.width, NormalmapTex.height, TextureFormat.RGBA32, false);
		WaterDataTexture = new Texture2D(Width, Height, TextureFormat.RGB24, false);

		Color SplatTextureColor = new Color(0, 0, 0, 0);
		Color[] Pixels = new Color[Width * Height];
		for (int i = 0; i < Pixels.Length; i++)
			Pixels[i] = SplatTextureColor;

		TexturemapTex.SetPixels(Pixels);
		TexturemapTex2.SetPixels(Pixels);

		TexturemapTex.Apply();
		TexturemapTex2.Apply();


		Color Bump = new Color(0, 0, 1, 1);
		for (int i = 0; i < Pixels.Length; i++)
			Pixels[i] = Bump;

		UncompressedNormalmapTex.SetPixels(Pixels);



		PreviewTextHeader = DefaultScmapHeaders.Current.PreviewTextHeader;
		TextureMapHeader = DefaultScmapHeaders.Current.TextureMapHeader;
		TextureMap2Header = DefaultScmapHeaders.Current.TextureMap2Header;
		NormalmapHeader = DefaultScmapHeaders.Current.NormalmapHeader;
		WatermapHeader = DefaultScmapHeaders.Current.WatermapHeader;

		TerrainTypeData = new byte[Height * Width];
//		TerrainTypeData = new List<byte>();
		HeightmapData = new short[(Height + 1) * (Width + 1)];
		WaterDepthBiasMask = new byte[(Height * Width) / 4];
		WaterFlatnessMask = new byte[(Height * Width) / 4];
		WaterFoamMask = new byte[(Height * Width) / 4];
		for (int i = 0; i < WaterDepthBiasMask.Length; i++)
		{
			WaterDepthBiasMask[i] = 127;
			WaterFlatnessMask[i] = 255;
			WaterFoamMask[i] = 0;
		}


		float HeightConversion = InitialHeight / 25.6f;
		HeightConversion /= 10f;


		short InitialHeightValue = (short)((HeightConversion * HeightResize) + 0.5f);
		Debug.Log(InitialHeightValue);

		for (int i = 0; i < HeightmapData.Length; i++)
		{
			HeightmapData[i] = InitialHeightValue;
		}


		//Version
		VersionMajor = 2;
		VersionMinor = 56;

		HeightScale = 0.0078125f;

		TexPathBackground = "/textures/environment/blackbackground.dds";
		TexPathSkyCubemap = "/textures/environment/skycube_evergreen01a.dds";
		EnvCubemapsName = new string[3];
		EnvCubemapsFile = new string[3];
		EnvCubemapsName[0] = "<aeon>";
		EnvCubemapsName[1] = "<default>";
		EnvCubemapsName[2] = "<seraphim>";
		EnvCubemapsFile[0] = "/textures/environment/envcube_aeon_evergreen.dds";
		EnvCubemapsFile[1] = "/textures/environment/envcube_evergreen01a.dds";
		EnvCubemapsFile[2] = "/textures/environment/envcube_seraphim_evergreen.dds";

		Bloom = 0.03f;// 0.145f;

		TerrainShader = "TTerrain";
		LightingMultiplier = 1.54f;
		SunDirection = new Vector3(0.616f, 0.559f, 0.55473f).normalized;
		SunAmbience = Vector3.zero;
		SunColor = new Vector3(1.38f, 1.29f, 1.14f);
		ShadowFillColor = new Vector3(0.54f, 0.54f, 0.7f);
		SpecularColor = new Vector4(0.31f, 0, 0, 0);


		FogColor = new Vector3(0.37f, 0.49f, 0.45f);
		FogStart = 0;
		FogEnd = 750;
		Unknown10 = -1091567891;
		Unknown11 = 2;
		Unknown12 = 0;
		Unknown7 = 13153;
		Unknown8 = 4;
		Unknown14 = -8.3f;
		Unknown15 = 0;

		WaveGenerators = new List<WaveGenerator>();
		Layers = new List<Layer>();

		{
			// 0
			Layer NewLayer = new Layer();
			NewLayer.PathTexture = "/env/evergreen2/layers/eg_gravel005_albedo.dds";
			NewLayer.PathNormalmap = "/env/tundra/layers/tund_sandlight_normal.dds";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 8.75f;
			Layers.Add(NewLayer);

			// 1
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 2
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 3
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 4
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 5
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 6
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 7
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 8
			NewLayer = new Layer();
			NewLayer.PathTexture = "";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 4;
			NewLayer.ScaleNormalmap = 4;
			Layers.Add(NewLayer);

			// 9
			NewLayer = new Layer();
			NewLayer.PathTexture = "/env/evergreen/layers/macrotexture000_albedo.dds";
			NewLayer.PathNormalmap = "";
			NewLayer.ScaleTexture = 128;
			NewLayer.ScaleNormalmap = 1;
			Layers.Add(NewLayer);
		}


		Decals = new List<Decal>();
		DecalGroups = new List<IntegerGroup>();

		Water = new WaterShader();
		Water.Defaults();

		Water.HasWater = _Water;
		Water.Elevation = WaterLevel;
		Water.ElevationDeep = DepthLevel;
		Water.ElevationAbyss = AbyssLevel;
		//Unknown Values

		Unknown8 = 0;
		Unknown10 = -1091567891;
		Unknown11 = 2;
		Unknown12 = 0;
		Unknown13 = 0;
		Unknown14 = 0;
		Unknown15 = 0;

		//Minimap Colors (Default)
		MinimapContourInterval = 24;
		MinimapDeepWaterColor = new Color(71, 140, 181);
		MinimapContourColor = new Color(112, 112, 112);
		MinimapShoreColor = new Color(140, 201, 224);
		MinimapLandStartColor = new Color(117, 99, 107);
		MinimapLandEndColor = new Color(206, 206, 176);
	}



	public int HeightmapId(int x, int y)
	{
		return (x + y * (Width + 1));
	}

    public short GetHeight(int x, int y)
    {
        return HeightmapData[HeightmapId(x, y)];
    }
    public void SetHeight(int x, int y, short value)
    {
		//Debug.Log("change value " + value);
    	HeightmapData[HeightmapId(x, y)] = value;
    }

    public byte GetTerrainTypeValue(int x, int y)
    {
        return TerrainTypeData[(y * Width) + x];
    }
    public void SetTerrainTypeValue(int x, int y, byte value)
    {
        TerrainTypeData[(y * Width) + x] = value;
    }


    public void Clear()
    {
    //    if (PreviewBitmap != null) { PreviewBitmap.Dispose(); PreviewBitmap = null; }
        if (PreviewTex != null) { PreviewTex = null; }
        if (TexturemapTex != null) { TexturemapTex = null; }
        if (TexturemapTex2 != null) { TexturemapTex2 = null; }
        if (NormalmapTex != null) { NormalmapTex = null; }

        HeightmapData = new short[0];

        Width = 0;
        Height = 0;

        TexPathBackground = "";
        TexPathSkyCubemap = "";
        EnvCubemapsName = new string[0];
        EnvCubemapsFile = new string[0];

        Water.Clear();

        WaterFoamMask = new byte[0];
        WaterFlatnessMask = new byte[0];
        WaterDepthBiasMask = new byte[0];

        WaveGenerators.Clear();
        Layers.Clear();
        Decals.Clear();
        DecalGroups.Clear();
        Props.Clear();
    }


    #region " Load/Save Functions "
	byte[] PreviewData = new byte[0];
	int PreviewImageLength;
    public bool Load(string Filename)
    {
        if (string.IsNullOrEmpty(Filename))
            return false;
        if (!System.IO.File.Exists(Filename))
            return false;

        this.Filename = Filename;

        Clear();

        System.IO.FileStream fs = new System.IO.FileStream(Filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        BinaryReader Stream = new BinaryReader(fs);

		PreviewTex = new Texture2D(0, 0);
		TexturemapTex = new Texture2D(0, 0);
		TexturemapTex2 = new Texture2D(0, 0);
		NormalmapTex = new Texture2D(0, 0);
		WatermapTex = new Texture2D(0, 0);
		WaterDataTexture = new Texture2D(0, 0);

        byte[] TexturemapData = new byte[0];
        byte[] TexturemapData2 = new byte[0];
        byte[] NormalmapData = new byte[0];
        byte[] WatermapData = new byte[0];


        int Count = 0;

        BinaryReader _with1 = Stream;
        //# Header Section #
        if (_with1.ReadInt32() == MAP_MAGIC)
        {
            VersionMajor = _with1.ReadInt32();
            //? always 2
            Unknown10 = _with1.ReadInt32();
            //? always EDFE EFBE
            Unknown11 = _with1.ReadInt32();
			//? always 2
			Unknown16 = _with1.ReadSingle();
			//Map Width (in float)
			Unknown17 = _with1.ReadSingle();
            //Map Height (in float)
            Unknown12 = _with1.ReadSingle();
            //? always 0
            Unknown13 = _with1.ReadInt16();
            //? always 0
			PreviewImageLength = _with1.ReadInt32();
			PreviewData = _with1.ReadBytes(PreviewImageLength);

            VersionMinor = _with1.ReadInt32();
            if (VersionMinor <= 0)
                VersionMinor = 56;
			

            //# Heightmap Section #
            Width = _with1.ReadInt32();
            Height = _with1.ReadInt32();

            HeightScale = _with1.ReadSingle();
            //Height Scale, usually 1/128
            HeightmapData = _with1.ReadInt16Array((Height + 1) * (Width + 1));//TODO: Current saving method gets a memory overload on trying to reload the map here.
																			  //heightmap dimension is always 1 more than texture dimension!

			if (VersionMinor >= 56)
				Unknown15 = _with1.ReadByte();
			else
				Unknown15 = 0;
			//Always 0?

			//# Texture Definition Section #
			TerrainShader = _with1.ReadStringNull();
            //Terrain Shader, usually "TTerrain"
            TexPathBackground = _with1.ReadStringNull();
            TexPathSkyCubemap = _with1.ReadStringNull();

            if (VersionMinor >= 56)
            {
                Count = _with1.ReadInt32();
                //always 1?
                EnvCubemapsName = new string[Count];
                EnvCubemapsFile = new string[Count];
                for (int i = 0; i <= Count - 1; i++)
                {
                    EnvCubemapsName[i] = _with1.ReadStringNull();
                    EnvCubemapsFile[i] = _with1.ReadStringNull();
                }
            }
            else
            {
                EnvCubemapsName = new string[2];
                EnvCubemapsName[0] = "<default>";
                EnvCubemapsFile = new string[2];
                EnvCubemapsFile[0] = _with1.ReadStringNull();
            }

            LightingMultiplier = _with1.ReadSingle();
            SunDirection = _with1.ReadVector3();
            SunAmbience = _with1.ReadVector3();
            SunColor = _with1.ReadVector3();
            ShadowFillColor = _with1.ReadVector3();
            SpecularColor = _with1.ReadVector4();
            Bloom = _with1.ReadSingle();

            FogColor = _with1.ReadVector3();
            FogStart = _with1.ReadSingle();
            FogEnd = _with1.ReadSingle();


            Water.Load(Stream);

            Count = _with1.ReadInt32();
            WaveGenerators.Clear();
            for (int i = 0; i <= Count - 1; i++)
            {
                WaveGenerator WaveGen = new WaveGenerator();
                WaveGen.Load(Stream);
                WaveGenerators.Add(WaveGen);
            }

            if (VersionMinor < 56)
            {
                _with1.ReadStringNull();
                // always "No Tileset"
                Count = _with1.ReadInt32();
                //always 6
                for (int i = 0; i <= 4; i++)
                {
                    Layer Layer = new Layer();
                    Layer.Load(Stream);
                    Layers.Add(Layer);
                }
                for (int i = 5; i <= 8; i++)
                {
                    Layers.Add(new Layer());
                }
                for (int i = 9; i <= 9; i++)
                {
                    Layer Layer = new Layer();
                    Layer.Load(Stream);
                    Layers.Add(Layer);
                }
            }
            else
			{
                MinimapContourInterval = _with1.ReadInt32();

				int argb = _with1.ReadInt32();
				MinimapDeepWaterColor = Int32ToColor(argb);
				/*int r = (argb)&0xFF;
				int g = (argb>>8)&0xFF;
				int b = (argb>>16)&0xFF;
				int a = (argb>>24)&0xFF;
                MinimapDeepWaterColor = new Color(r,g,b,a);*/
				int argb2 = _with1.ReadInt32();
				/*int r2 = (argb2)&0xFF;
				int g2 = (argb2>>8)&0xFF;
				int b2 = (argb2>>16)&0xFF;
				int a2 = (argb2>>24)&0xFF;
				MinimapContourColor = new Color(r2,g2,b2,a2);*/
				MinimapContourColor = Int32ToColor(argb2);
				int argb3 = _with1.ReadInt32();
				/*int r3 = (argb3)&0xFF;
				int g3 = (argb3>>8)&0xFF;
				int b3 = (argb3>>16)&0xFF;
				int a3 = (argb3>>24)&0xFF;
				MinimapShoreColor = new Color(r3,g3,b3,a3);*/
				MinimapShoreColor = Int32ToColor(argb3);
				int argb4 = _with1.ReadInt32();
				/*int r4 = (argb4)&0xFF;
				int g4 = (argb4>>8)&0xFF;
				int b4 = (argb4>>16)&0xFF;
				int a4 = (argb4>>24)&0xFF;
				MinimapLandStartColor = new Color(r4,g4,b4,a4);*/
				MinimapLandStartColor = Int32ToColor(argb4);
				int argb5 = _with1.ReadInt32();
				/*int r5 = (argb5)&0xFF;
				int g5 = (argb5>>8)&0xFF;
				int b5 = (argb5>>16)&0xFF;
				int a5 = (argb5>>24)&0xFF;
				MinimapLandEndColor = new Color(r5,g5,b5,a5);*/
				MinimapLandEndColor = Int32ToColor(argb5);

				if (VersionMinor > 56)
                {
                    Unknown14 = _with1.ReadSingle(); //Not sure what this is.
                }
                Count = 10;
                for (int i = 0; i <= Count - 1; i++)
                {
                    Layer Layer = new Layer();
                    Layer.LoadAlbedo(Stream);
                    Layers.Add(Layer);
                }
                for (int i = 0; i <= Count - 2; i++)
                {
                    Layers[i].LoadNormal(Stream);
                }
            }
            Unknown7 = _with1.ReadInt32();
            //?
            Unknown8 = _with1.ReadInt32();
            //?

            int DecalCount = _with1.ReadInt32();
			//Debug.Log(DecalCount);
            for (int i = 0; i < DecalCount; i++)
            {
                Decal Feature = new Decal();
                Feature.Load(Stream);
                Decals.Add(Feature);
            }

            int GroupCount = _with1.ReadInt32();
            for (int i = 0; i <= GroupCount - 1; i++)
            {
                IntegerGroup Group = new IntegerGroup();
                Group.Load(Stream);
                DecalGroups.Add(Group);
            }

            _with1.ReadInt32();
            //Width again
            _with1.ReadInt32();
            //Height again

            int Length = 0;
            int NormalmapCount = _with1.ReadInt32();
            //always 1
            for (int i = 0; i < NormalmapCount; i++)
            {
                Length = _with1.ReadInt32();
                if (i == 0)
                {
                    NormalmapData = _with1.ReadBytes(Length);
                }
                else
                {
                    _with1.BaseStream.Position += Length;
                    // just to make sure that it doesn't crash if it is not just 1 normalmap for some reason
                }
            }


            if (VersionMinor < 56)
                _with1.ReadInt32();
            //always 1
            Length = _with1.ReadInt32();
            TexturemapData = _with1.ReadBytes(Length);

            if (VersionMinor >= 56)
            {
                Length = _with1.ReadInt32();
                TexturemapData2 = _with1.ReadBytes(Length);
            }


            //Watermap
            _with1.ReadInt32();
            //always 1
            Length = _with1.ReadInt32();
            WatermapData = _with1.ReadBytes(Length);

            int HalfSize = (Width / 2) * (Height / 2);
            WaterFoamMask = _with1.ReadBytes(HalfSize);
            WaterFlatnessMask = _with1.ReadBytes(HalfSize);
            WaterDepthBiasMask = _with1.ReadBytes(HalfSize);

			WaterDataTexture = new Texture2D (Width / 2, Height / 2, TextureFormat.RGB24, false);
			Color32[] NewColors = new Color32[HalfSize];
			for (int i = 0; i < HalfSize; i++) {
				NewColors [i] = new Color32 (WaterFoamMask [i], WaterFlatnessMask [i], WaterDepthBiasMask [i], 0);
			}
			WaterDataTexture.SetPixels32 (NewColors);
			WaterDataTexture.Apply ();

            TerrainTypeData = _with1.ReadBytes(Width * Height);
//            TerrainTypeData = _with1.ReadBytes(Width * Height).ToList();

            if (VersionMinor <= 52)
                _with1.ReadInt16();


			//Debug.Log("Scmap file version: " + VersionMinor);

			if (VersionMinor >= 60)
			{
				AdditionalSkyboxData.Load(_with1);
			}
			else
			{
				ScmapEditor.Current.LoadDefaultSkybox();
			}


			try{
	            int PropCount = _with1.ReadInt32();
				//Debug.Log ("PropCount: " + PropCount + ", v" + VersionMinor );
				for (int i = 0; i < PropCount; i++)
	            {
	                Prop Prop = new Prop();
	                Prop.Load(Stream);
	                Props.Add(Prop);
					//Debug.Log(Prop.BlueprintPath);
	            }
			}
			catch{
				Debug.LogError ("Loading props crashed");
			}

				

        }
        _with1.Close();
        fs.Close();
        fs.Dispose();

		PreviewTextHeader = GetGamedataFile.GetDdsFormat(PreviewData);
		PreviewTextHeader.Format = TextureFormat.BGRA32;
		PreviewTex = TextureLoader.ConvertToRGBA(TextureLoader.LoadTextureDXT(PreviewData, PreviewTextHeader));

		TextureMapHeader = GetGamedataFile.GetDdsFormat(TexturemapData);
		TextureMapHeader.Format = TextureFormat.BGRA32;
		//TexturemapTex = TextureLoader.LoadTextureDXT(TexturemapData, TextureMapHeader);
		TexturemapTex = TextureLoader.ConvertToRGBA (TextureLoader.LoadTextureDXT (TexturemapData, TextureMapHeader));
		TexturemapData = new byte[0];

        if (TexturemapData2.Length > 0)
        {
			TextureMap2Header = GetGamedataFile.GetDdsFormat(TexturemapData2);
			TextureMap2Header.Format = TextureFormat.BGRA32;
			TexturemapTex2 = TextureLoader.ConvertToRGBA (TextureLoader.LoadTextureDXT(TexturemapData2, TextureMap2Header));
			TexturemapData2 = new byte[0];
        }
        else
        {
			TextureMap2Header = TextureMapHeader;
			TexturemapTex2 = new Texture2D(Width/2,Height/2);
			Color[] Pixels = TexturemapTex2.GetPixels();
			for(int p = 0; p < Pixels.Length; p++){
				Pixels[p] = new Color(0,0,0,0);
			}
			TexturemapTex2.SetPixels(Pixels);
			TexturemapTex2.Apply();
        }
		TexturemapTex.wrapMode = TextureWrapMode.Clamp;
		TexturemapTex2.wrapMode = TextureWrapMode.Clamp;

		NormalmapHeader = GetGamedataFile.GetDdsFormat(NormalmapData);
		NormalmapHeader.Format = TextureFormat.DXT5;
		NormalmapTex = TextureLoader.LoadTextureDXT(NormalmapData, NormalmapHeader);
		NormalmapData = new byte[0];

		UncompressedNormalmapTex = new Texture2D(NormalmapTex.width, NormalmapTex.height, TextureFormat.RGBA32, false);
		UncompressedNormalmapTex.SetPixels(NormalmapTex.GetPixels());
		UncompressedNormalmapTex.wrapMode = TextureWrapMode.Clamp;
		UncompressedNormalmapTex.Apply();

		WatermapHeader = GetGamedataFile.GetDdsFormat(WatermapData);
		WatermapHeader.Format = TextureFormat.DXT5;
		WatermapTex = TextureLoader.LoadTextureDXT(WatermapData, WatermapHeader);
		WatermapData = new byte[0];

		UncompressedWatermapTex = new Texture2D (WatermapTex.width, WatermapTex.height, TextureFormat.RGBA32, false);
		UncompressedWatermapTex.SetPixels (WatermapTex.GetPixels ());
		UncompressedWatermapTex.wrapMode = TextureWrapMode.Clamp;

		UncompressedWatermapTex.Apply ();

		return true;
    }

	public Texture2D UncompressedWatermapTex;
	public Texture2D UncompressedNormalmapTex;

	public void SaveMapInformation(string Filename, int randomSeed)
    {
        System.IO.StreamWriter fs = new System.IO.StreamWriter(Filename, false);
        fs.WriteLine("FA Map Information");
        fs.WriteLine("----------------------------------------------------------------");
        fs.WriteLine("    Random Number Seed: " + randomSeed);
        fs.WriteLine("    Dimensions: " + this.Width + "x" + this.Height);
        fs.WriteLine("    Map Data Version: " + this.VersionMajor + "." + this.VersionMinor);
        fs.WriteLine("    Height Scale: " + this.HeightScale);
        fs.WriteLine("    Decal Count: " + this.Decals.Count);
        fs.WriteLine("    Prop Count: " + this.Props.Count);
        fs.WriteLine("    Wave Generator Count: " + this.WaveGenerators.Count);

        fs.WriteLine("");
        fs.WriteLine("    Lighting");
        fs.WriteLine("        Terrain Shader: " + this.TerrainShader);
        fs.WriteLine("        Background: " + this.TexPathBackground);
        fs.WriteLine("        Sky Cube: " + this.TexPathSkyCubemap);
        fs.WriteLine("        Enviroment Lookup Textures");
        for (int i = 0; i <= this.EnvCubemapsFile.Length - 1; i++)
        {
            fs.WriteLine("            Texture " + (i + 1).ToString());
            fs.WriteLine("                Texture Label: " + this.EnvCubemapsName[i]);
            fs.WriteLine("                Texture Path: " + this.EnvCubemapsFile[i]);
            fs.WriteLine("");
        }
        fs.WriteLine("        Light Direction: RA=" + (180 / Math.PI) * Math.Acos(this.SunDirection.z) + " Dec=" + (180 / Math.PI) * Math.Atan2(this.SunDirection.y, this.SunDirection.x) + " Vector=" + this.SunDirection.ToString());
        fs.WriteLine("        Multiplier: " + this.LightingMultiplier);
        fs.WriteLine("        Light Color: R=" + this.SunColor.x + " G=" + this.SunColor.y + " B=" + this.SunColor.z);
        fs.WriteLine("        Ambient Light Color: R=" + this.SunAmbience.x + " G=" + this.SunAmbience.y + " B=" + this.SunAmbience.z);
        fs.WriteLine("        Shadow Color: R=" + this.ShadowFillColor.x + " G=" + this.ShadowFillColor.y + " B=" + this.ShadowFillColor.z);
        fs.WriteLine("        Specular: ");
        fs.WriteLine("        Glow: ");
        fs.WriteLine("        Bloom: " + this.Bloom);
        fs.WriteLine("        Fog Color: R=" + this.FogColor.x + " G=" + this.FogColor.y + " B=" + this.FogColor.z);
        fs.WriteLine("        Fog Start: " + this.FogStart);
        fs.WriteLine("        Fog End: " + this.FogEnd);
        fs.WriteLine("    Water");
        fs.WriteLine("        Enabled: " + this.Water.HasWater);
        fs.WriteLine("        Surface Elevation: " + this.Water.Elevation);
        fs.WriteLine("        Deep Elevation: " + this.Water.ElevationDeep);
        fs.WriteLine("        Abyss Elevation: " + this.Water.ElevationAbyss);
        fs.WriteLine("        Reflected Sun Color: R=" + this.Water.SunColor.x + " G=" + this.Water.SunColor.y + " B=" + this.Water.SunColor.z);
        fs.WriteLine("        Water Surface Color: R=" + this.Water.SurfaceColor.x + " G=" + this.Water.SurfaceColor.y + " B=" + this.Water.SurfaceColor.z);
        fs.WriteLine("        Color Lerp: Max=" + this.Water.ColorLerp.y + " Min=" + this.Water.ColorLerp.x);
        fs.WriteLine("        Sun Reflection: " + this.Water.SunReflection);
        fs.WriteLine("        Sky Reflection: " + this.Water.SkyReflection);
        fs.WriteLine("        Unit Reflection: " + this.Water.UnitReflection);
        fs.WriteLine("        Refraction: " + this.Water.RefractionScale);
        fs.WriteLine("        Sun Shininess: " + this.Water.SunShininess);
        fs.WriteLine("        Sun Strength: " + this.Water.SunStrength);
        fs.WriteLine("        Sun Glow: " + this.Water.SunGlow);
        fs.WriteLine("        Fresnel Bias: " + this.Water.FresnelBias);
        fs.WriteLine("        Fresnel Power: " + this.Water.FresnelPower);
        fs.WriteLine("        Texture-Environment: " + this.Water.TexPathCubemap);
        fs.WriteLine("        Texture-Water Ramp: " + this.Water.TexPathWaterRamp);
        fs.WriteLine("        Wave Normals");
        for (int i = 0; i <= this.Water.WaveTextures.Length - 1; i++)
        {
            fs.WriteLine("            Wave Normal " + (i + 1).ToString());
            fs.WriteLine("                Texture: " + this.Water.WaveTextures[i].TexPath);
            fs.WriteLine("                Direction Vector: X=" + this.Water.WaveTextures[i].NormalMovement.x + " " + this.Water.WaveTextures[i].NormalMovement.y);
            fs.WriteLine("                Frequency: " + this.Water.WaveTextures[i].NormalRepeat);
            fs.WriteLine("");
        }

        fs.WriteLine("    Stratum");
        for (int i = 0; i <= this.Layers.Count - 1; i++)
        {
            Layer stratum = this.Layers[i];
            fs.WriteLine("        Stratum " + (i + 1).ToString());
            fs.WriteLine("                Texture Path: " + stratum.PathTexture);
            fs.WriteLine("                Texture Scale: " + stratum.ScaleTexture);
            fs.WriteLine("                Normal Map Path: " + stratum.PathNormalmap);
            fs.WriteLine("                Normal Map Scale: " + stratum.ScaleNormalmap);
            fs.WriteLine("");
        }

        fs.WriteLine("    Unknown Settings");
        fs.WriteLine("        Unknown Value 7:" + this.Unknown7);
        fs.WriteLine("        Unknown Value 8:" + this.Unknown8);
        fs.WriteLine("        Unknown Value 10:" + this.Unknown10);
        fs.WriteLine("        Unknown Value 11:" + this.Unknown11);
        fs.WriteLine("        Unknown Value 12:" + this.Unknown12);
        fs.WriteLine("        Unknown Value 13:" + this.Unknown13);
        fs.WriteLine("        Unknown Value 14:" + this.Unknown14);

        fs.Close();
    }
    public bool Save(string Filename, int MapFileVersion)
    {
        if (!string.IsNullOrEmpty(Filename) & Filename != this.Filename)
        {
            this.Filename = Filename;
        }
        else
        {
            Filename = this.Filename;
        }
        if (MapFileVersion <= 0)
            MapFileVersion = VersionMinor;
        if (MapFileVersion <= 0)
            MapFileVersion = 56;
        if (MapFileVersion != VersionMinor)
            VersionMinor = MapFileVersion;

		Debug.Log("Save file version: " + VersionMinor);

        System.IO.FileStream fs = new System.IO.FileStream(Filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        BinaryWriter Stream = new BinaryWriter(fs);

        var _with2 = Stream;
        //# Header Section #
        _with2.Write(MAP_MAGIC);
        _with2.Write(MAP_VERSION);

        _with2.Write(Unknown10);
        //? always EDFE EFBE
        _with2.Write(Unknown11);
        //? always 2
        _with2.Write((float)Width);
        //Map Width (in float)
        _with2.Write((float)Height);
        //Map Height (in float)
        _with2.Write(Unknown12);
        //? always 0
        _with2.Write(Unknown13);
		//? always 0
		//byte[] SaveData = new byte[0];

		//SaveData = PreviewTex.GetRawTextureData();
		//_with2.Write(SaveData.Length);
		//_with2.Write(SaveData);
		//Debug.Log(PreviewTex.GetRawTextureData().Length);
		TextureLoader.GetHeaderBGRA(PreviewTex, ref PreviewTextHeader);
		SaveTexture(_with2, TextureLoader.ConvertToBGRA(PreviewTex), PreviewTextHeader);
		//_with2.Write(PreviewImageLength);
		//_with2.Write(PreviewData);
		//Debug.Log( _with2.BaseStream.Length );

        //# Heightmap Section #
        _with2.Write(MapFileVersion);
        _with2.Write(Width);
        _with2.Write(Height);
        _with2.Write(HeightScale);
        //Height Scale, usually 1/128
        _with2.Write(HeightmapData);

		if (MapFileVersion >= 56)
		{
			//_with2.Write(Convert.ToByte(0));
			_with2.Write(Unknown15);
		}
        //Always 0?

        //# Texture Definition Section #
        _with2.Write(TerrainShader, true);
        //usually "TTerrain"
        _with2.Write(TexPathBackground, true);
        _with2.Write(TexPathSkyCubemap, true);
        if (VersionMinor >= 56)
        {
            _with2.Write(EnvCubemapsName.Length);
            for (int i = 0; i < EnvCubemapsName.Length; i++)
            {
                _with2.Write(EnvCubemapsName[i], true);
                _with2.Write(EnvCubemapsFile[i], true);
            }
        }
        else
        {
            if (EnvCubemapsFile.Length >= 1)
            {
                _with2.Write(EnvCubemapsFile[0], true);
            }
            else
            {
                _with2.Write(Convert.ToByte(0));
            }
        }

        _with2.Write(LightingMultiplier);
        _with2.Write(SunDirection);
        _with2.Write(SunAmbience);
        _with2.Write(SunColor);
        _with2.Write(ShadowFillColor);
        _with2.Write(SpecularColor);
        _with2.Write(Bloom);

        _with2.Write(FogColor);
        _with2.Write(FogStart);
        _with2.Write(FogEnd);

        Water.Save(_with2);

        _with2.Write(WaveGenerators.Count);
        for (int i = 0; i < WaveGenerators.Count; i++)
        {
            WaveGenerators[i].Save(_with2);
        }

        if (VersionMinor < 56)
        {
            _with2.Write("No Tileset", true);

            _with2.Write(6);
            for (int i = 0; i <= 4; i++)
            {
                Layers[i].Save(_with2);
            }
            Layers[Layers.Count - 1].Save(_with2);
        }
        else
        {
            _with2.Write(MinimapContourInterval);
			int color = 0;
			color |= MinimapDeepWaterColor.a << 24;
			color |= MinimapDeepWaterColor.r << 16;
			color |= MinimapDeepWaterColor.g << 8;
			color |= MinimapDeepWaterColor.b;
            _with2.Write(color);

			int color2 = 0;
			color2 |= MinimapContourColor.a << 24;
			color2 |= MinimapContourColor.r << 16;
			color2 |= MinimapContourColor.g << 8;
			color2 |= MinimapContourColor.b;
			_with2.Write(color2);

			int color3 = 0;
			color3 |= MinimapShoreColor.a << 24;
			color3 |= MinimapShoreColor.r << 16;
			color3 |= MinimapShoreColor.g << 8;
			color3 |= MinimapShoreColor.b;
			_with2.Write(color3);

			int color4 = 0;
			color4 |= MinimapLandStartColor.a << 24;
			color4 |= MinimapLandStartColor.r << 16;
			color4 |= MinimapLandStartColor.g << 8;
			color4 |= MinimapLandStartColor.b;
			_with2.Write(color4);

			int color5 = 0;
			color5 |= MinimapLandEndColor.a << 24;
			color5 |= MinimapLandEndColor.r << 16;
			color5 |= MinimapLandEndColor.g << 8;
			color5 |= MinimapLandEndColor.b;
			_with2.Write(color5);

            if (VersionMinor > 56)
            {
                _with2.Write(Unknown14);
            }

            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].SaveAlbedo(_with2);
            }
            for (int i = 0; i < Layers.Count - 1; i++)
            {
                Layers[i].SaveNormal(_with2);
            }
        }

        _with2.Write(Unknown7);
        //?
        _with2.Write(Unknown8);
		//?

		Decals = DecalsControler.GetAllDecals();

		_with2.Write(Decals.Count);
        for (int i = 0; i < Decals.Count; i++)
        {
            Decals[i].Save(_with2, i);
        }

        _with2.Write(DecalGroups.Count);
        for (int i = 0; i < DecalGroups.Count; i++)
        {
            DecalGroups[i].Save(_with2);
        }

        _with2.Write(Width);
        //Width again
        _with2.Write(Height);
        //Height again

        _with2.Write(1);

		TextureLoader.GetHeaderDxt5(NormalmapTex, ref NormalmapHeader);
		SaveTexture(_with2, NormalmapTex, NormalmapHeader);
        //Format.Dxt5

        if (VersionMinor < 56)
            _with2.Write(1);

		TextureLoader.GetHeaderBGRA(TexturemapTex, ref TextureMapHeader);
		SaveTexture(_with2, TextureLoader.ConvertToBGRA(TexturemapTex), TextureMapHeader);

        if (VersionMinor >= 56)
        {
			TextureLoader.GetHeaderBGRA(TexturemapTex2, ref TextureMap2Header);
			SaveTexture(_with2, TextureLoader.ConvertToBGRA(TexturemapTex2), TextureMap2Header);
        }

        _with2.Write(1);
		TextureLoader.GetHeaderDxt5(WatermapTex, ref WatermapHeader);
		SaveTexture(_with2, WatermapTex, WatermapHeader);

        _with2.Write(WaterFoamMask);
        _with2.Write(WaterFlatnessMask);
        _with2.Write(WaterDepthBiasMask);

        _with2.Write(TerrainTypeData.ToArray());

        if (MapFileVersion <= 52)
            _with2.Write(Convert.ToInt16(0));


		if(VersionMinor >= 60)
		{
			AdditionalSkyboxData.Save(_with2);
		}

        _with2.Write(Props.Count);
        for (int i = 0; i <= Props.Count - 1; i++)
        {
            Props[i].Save(_with2);
        }

        _with2.Close();
        fs.Close();
        fs.Dispose();
        return true;
    }

    public static int GetMapVersion(string Filename)
    {
        if (string.IsNullOrEmpty(Filename))
            return 0;
        if (!System.IO.File.Exists(Filename))
            return 0;

        System.IO.FileStream fs = new System.IO.FileStream(Filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        BinaryReader Stream = new BinaryReader(fs);

        int MapFileVersion = 0;

        var _with3 = Stream;
        //# Header Section #
        if (_with3.ReadInt32() == MAP_MAGIC)
        {
            fs.Position += 26;
            int ImageLength = _with3.ReadInt32();
            fs.Position += ImageLength;

            //# Heightmap Section #
            MapFileVersion = _with3.ReadInt32();
        }

        _with3.Close();
        fs.Close();
        fs.Dispose();

        return MapFileVersion;
    }

	private void SaveTexture(BinaryWriter Stream, Texture2D texture, GetGamedataFile.HeaderClass header)
    {
		byte[] texArray = TextureLoader.SaveTextureDDS(texture, header);
		//Debug.Log("L2: " + texArray.Length);
		Stream.Write(texArray.Length);
		Stream.Write(texArray,0,texArray.Length);
    }

	public static Color Int32ToColor(int data, bool floatRange = true)
	{
		int r = (data) & 0xFF;
		int g = (data >> 8) & 0xFF;
		int b = (data >> 16) & 0xFF;
		int a = (data >> 24) & 0xFF;
		if(floatRange)
			return new Color(r, g, b, a);
		else
			return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

    /*private void CopyStream(System.IO.Stream Source, System.IO.Stream Target)
    {
        byte[] buffer = new byte[2049];
        int read = 0;
        do
        {
            read = Source.Read(buffer, 0, buffer.Length);
            if (read > 0)
                Target.Write(buffer, 0, read);
        } while ((read > 0));
    }*/

    #endregion

}