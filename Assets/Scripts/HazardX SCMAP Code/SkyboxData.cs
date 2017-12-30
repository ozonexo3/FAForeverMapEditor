//********************************
// 
// * Procedural skybox from v60 scmap file for FAF Map Editor 
// * Copyright ozonexo3 2017
//
//********************************

using System.Collections;
using UnityEngine;

[System.Serializable]
public class SkyboxData
{
	public SkyboxValues Data;

	#region SkyboxData
	[System.Serializable]
	public class SkyboxValues
	{
		[Header("Sky dome")]
		public Vector3 Position = new Vector3(256, 0, 256);
		public float Scale = 2343;
		public float SubtractHeight = 1.256637f;
		//public Vector3 Scale = new Vector3(-35.45f, 1171.581f, 1.256637f); // X - horizonBegin, Y - Scale of sky dome, Z - subtract from vertex position.y
		public int SubdivisionsAxis = 16;
		public int SubdivisionsHeight = 6;

		[Header("Horizon")]
		public float HorizonHeight = 0;
		public float ZenithHeight = 174.5082f; // horizonEnd
		[ColorUsage(false, true, 0, 2, 0, 2)]
		public Color HorizonColor = new Color(0, 0, 0, 0);
		[ColorUsage(false, true, 0, 2, 0, 2)]
		public Color ZenithColor = new Color(0, 0, 0, 0);

		[Header("Decals atlas")]
		public float DecalGlowMultiplier = 0.1f; // Always 0.1
		public string Albedo = "/textures/environment/Decal_test_Albedo001.dds";
		public string Glow = "/textures/environment/Decal_test_Glow001.dds";
		public Planet[] Planets = new Planet[0];

		[Header("Cumulus?")]
		public Color32 MidRgbColor; // 3 bytes, always 0 == black?


		[Header("Cirrus")]
		public float CirrusMultiplier = 1.8f;
		[ColorUsage(false, true, 0, 2, 0, 2)]
		public Color CirrusColor = Color.white;
		public string CirrusTexture = "/textures/environment/cirrus000.dds";
		public Cirrus[] CirrusLayers;

		public float Clouds7; // Always 0

		[System.Serializable]
		public struct Planet
		{
			public Vector3 Position;
			public float Rotation; // Used in shader to rotate vertexes around center
			public Vector2 Scale;
			public Vector4 Uv;
		}

		[System.Serializable]
		public struct Cirrus
		{
			public Vector2 frequency;
			public float Speed;
			public Vector2 Direction;
		}

		public SkyboxValues()
		{

		}

		public SkyboxValues(SkyboxValues from)
		{
			CopyFrom(from);

		}

		public void CopyFrom(SkyboxValues from)
		{
			Position = from.Position;
			Scale = from.Scale;
			SubtractHeight = from.SubtractHeight;
			SubdivisionsAxis = from.SubdivisionsAxis;
			SubdivisionsHeight = from.SubdivisionsHeight;

			HorizonHeight = from.HorizonHeight;
			ZenithHeight = from.ZenithHeight;
			HorizonColor = from.HorizonColor;
			ZenithColor = from.ZenithColor;
			DecalGlowMultiplier = from.DecalGlowMultiplier;

			Albedo = from.Albedo;
			Glow = from.Glow;

			Planets = new Planet[from.Planets.Length];
			for (int i = 0; i < from.Planets.Length; i++)
			{
				Planets[i] = from.Planets[i];
			}

			CirrusMultiplier = from.CirrusMultiplier;
			CirrusColor = from.CirrusColor;

			CirrusTexture = from.CirrusTexture;

			CirrusLayers = from.CirrusLayers;
			Clouds7 = from.Clouds7;

			UpdateSize();
		}

		public void UpdateSize()
		{
			Position = ScmapEditor.WorldPosToScmap( MapLuaParser.Current.MapCenterPoint);
			Scale = Mathf.Max(ScmapEditor.Current.map.Width, ScmapEditor.Current.map.Height) * 2.288245f;
		}
	}
	#endregion

	#region Read/Write
	public void Load(BinaryReader Stream)
	{
		Data = new SkyboxValues();

		// Skydome
		Data.Position = Stream.ReadVector3();
		Data.HorizonHeight = Stream.ReadSingle();
		Data.Scale = Stream.ReadSingle();
		Data.SubtractHeight = Stream.ReadSingle();
		Data.SubdivisionsAxis = Stream.ReadInt32();
		Data.SubdivisionsHeight = Stream.ReadInt32();
		Data.ZenithHeight = Stream.ReadSingle();
		Vector3 VectorColor = Stream.ReadVector3();
		Data.HorizonColor = new Color(VectorColor.x, VectorColor.y, VectorColor.z);
		VectorColor = Stream.ReadVector3();
		Data.ZenithColor = new Color(VectorColor.x, VectorColor.y, VectorColor.z);

		// Decals
		Data.DecalGlowMultiplier = Stream.ReadSingle();

		Data.Albedo = Stream.ReadStringNull();
		Data.Glow = Stream.ReadStringNull();

		// Array of Planets/Stars
		int Length = Stream.ReadInt32();
		Data.Planets = new SkyboxValues.Planet[Length];
		for (int i = 0; i < Data.Planets.Length; i++)
		{
			Data.Planets[i] = new SkyboxValues.Planet();
			Data.Planets[i].Position = Stream.ReadVector3();
			Data.Planets[i].Rotation = Stream.ReadSingle();
			Data.Planets[i].Scale = Stream.ReadVector2();
			Data.Planets[i].Uv = Stream.ReadVector4();
		}

		// Mid
		Data.MidRgbColor = new Color32(Stream.ReadByte(), Stream.ReadByte(), Stream.ReadByte(), 0);

		// Cirrus
		Data.CirrusMultiplier = Stream.ReadSingle();
		VectorColor = Stream.ReadVector3();
		Data.CirrusColor = new Color(VectorColor.x, VectorColor.y, VectorColor.z);

		Data.CirrusTexture = Stream.ReadStringNull();

		int CirrusLayerCount = Stream.ReadInt32();
		Data.CirrusLayers = new SkyboxValues.Cirrus[CirrusLayerCount];
		for (int i = 0; i < Data.CirrusLayers.Length; i++)
		{
			Data.CirrusLayers[i] = new SkyboxValues.Cirrus();
			Data.CirrusLayers[i].frequency = Stream.ReadVector2();
			Data.CirrusLayers[i].Speed = Stream.ReadSingle();
			Data.CirrusLayers[i].Direction = Stream.ReadVector2();
		}
		Data.Clouds7 = Stream.ReadSingle();
	}

	public void Save(BinaryWriter Stream)
	{
		// Sky Dome
		Stream.Write(Data.Position);
		Stream.Write(Data.HorizonHeight);
		Stream.Write(Data.Scale);
		Stream.Write(Data.SubtractHeight);
		Stream.Write(Data.SubdivisionsAxis);
		Stream.Write(Data.SubdivisionsHeight);
		Stream.Write(Data.ZenithHeight);
		Stream.Write(new Vector3(Data.HorizonColor.r, Data.HorizonColor.g, Data.HorizonColor.b));
		Stream.Write(new Vector3(Data.ZenithColor.r, Data.ZenithColor.g, Data.ZenithColor.b));

		// Decals
		Stream.Write(Data.DecalGlowMultiplier);
		Stream.Write(Data.Albedo, true);
		Stream.Write(Data.Glow, true);

		Stream.Write(Data.Planets.Length);
		for (int i = 0; i < Data.Planets.Length; i++)
		{
			Stream.Write(Data.Planets[i].Position);
			Stream.Write(Data.Planets[i].Rotation);
			Stream.Write(Data.Planets[i].Scale);
			Stream.Write(Data.Planets[i].Uv);
		}

		// Mid
		Stream.Write(Data.MidRgbColor.r);
		Stream.Write(Data.MidRgbColor.g);
		Stream.Write(Data.MidRgbColor.b);

		// Cirrus
		Stream.Write(Data.CirrusMultiplier);
		Stream.Write(new Vector3(Data.CirrusColor.r, Data.CirrusColor.g, Data.CirrusColor.b));

		Stream.Write(Data.CirrusTexture, true);

		Stream.Write(Data.CirrusLayers.Length);
		for (int i = 0; i < Data.CirrusLayers.Length; i++)
		{
			Stream.Write(Data.CirrusLayers[i].frequency);
			Stream.Write(Data.CirrusLayers[i].Speed);
			Stream.Write(Data.CirrusLayers[i].Direction);
		}

		Stream.Write(Data.Clouds7);
	}
	#endregion
}
