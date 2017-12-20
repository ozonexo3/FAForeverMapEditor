//********************************
// 
// * Procedural skybox from v60 scmap file for FAF Map Editor 
// * Copyright ozonexo3 2017
//
//********************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SkyboxData
{

	//TODO find what are these values are!
	/*
	public short[] BeginValues = new short[32];
	public Vector3[] beginvectors;
	public float[] BeginFloats;
	public int[] BeginInts;
	public Color[] BeginColors;
	*/

	public SkyboxValues Data;

	[System.Serializable]
	public class SkyboxValues
	{
		[Header("Begin")]
		public Vector3 Position = new Vector3(256, 0, 256);
		public Vector3 Scale = new Vector3(-35.45f, 1171.581f, 1256637); // X - Unknown, Y - Scale of sky dome, Z - Always 1.256637
		public int SubdivisionsAxis = 16;
		public int SubdivisionsHeight = 6;
		public float Value3 = 174.5082f; // Unknown
		[ColorUsage(false, true, 0, 2, 0, 2)]
		public Color HorizoColor = new Color(0,0,0,0);
		[ColorUsage(false, true, 0, 2, 0, 2)]
		public Color ZenithColor = new Color(0, 0, 0, 0);
		public float Value6 = 0.1f; // Always 0.1

		[Header("Planet atlas")]
		public string Albedo = "/textures/environment/Decal_test_Albedo001.dds";
		public string Glow = "/textures/environment/Decal_test_Glow001.dds";

		[Header("Planet array")]
		public Planet[] Planets = new Planet[0];

		[Header("Some more data?")]
		public Color32 MidRgbColor; // 3 bytes, always 0 == black?
		public float Mid0 = 1.8f;
		public float Mid1 = 0.97f;
		public float Mid2 = 0.97f;
		public float Mid3 = 0.97f;

		[Header("Clouds")]
		public string Clouds = "/textures/environment/cirrus000.dds";

		[Header("Clouds animation")]
		public int CloudsInt = 4; // Always 4
		public Vector3 Clouds1;
		public Vector3 Clouds2;
		public Vector3 Clouds3;
		public Vector3 Clouds4;
		public Vector3 Clouds5;
		public Vector3 Clouds6;
		public Vector3 Clouds7;

		[System.Serializable]
		public struct Planet
		{
			public Vector3 Position;
			public Vector3 Scale;
			public Vector4 Uv;
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
			SubdivisionsAxis = from.SubdivisionsAxis;
			SubdivisionsHeight = from.SubdivisionsHeight;
			Value3 = from.Value3;
			HorizoColor = from.HorizoColor;
			ZenithColor = from.ZenithColor;
			Value6 = from.Value6;

			Albedo = from.Albedo;
			Glow = from.Glow;

			Planets = new Planet[from.Planets.Length];
			for(int i = 0; i < from.Planets.Length; i++)
			{
				Planets[i] = from.Planets[i];
			}

			Clouds = from.Clouds;

			CloudsInt = from.CloudsInt;

			Clouds1 = from.Clouds1;
			Clouds2 = from.Clouds2;
			Clouds3 = from.Clouds3;
			Clouds4 = from.Clouds4;
			Clouds5 = from.Clouds5;
			Clouds6 = from.Clouds6;
			Clouds7 = from.Clouds7;
		}
	}



	public void Load(BinaryReader Stream)
	{
		Data = new SkyboxValues();

		Data.Position = Stream.ReadVector3();
		Data.Scale = Stream.ReadVector3();
		Data.SubdivisionsAxis = Stream.ReadInt32();
		Data.SubdivisionsHeight = Stream.ReadInt32();
		Data.Value3 = Stream.ReadSingle();
		Vector3 VectorColor = Stream.ReadVector3();
		Data.HorizoColor = new Color(VectorColor.x, VectorColor.y, VectorColor.z);
		VectorColor = Stream.ReadVector3();
		Data.ZenithColor = new Color(VectorColor.x, VectorColor.y, VectorColor.z);
		Data.Value6 = Stream.ReadSingle();

		// Planet and moon textures
		Data.Albedo = Stream.ReadStringNull();
		Data.Glow = Stream.ReadStringNull();

		// Array of Planets/Stars
		int Length = Stream.ReadInt32();
		Data.Planets = new SkyboxValues.Planet[Length];
		for(int i = 0; i < Data.Planets.Length; i++)
		{
			Data.Planets[i] = new SkyboxValues.Planet();
			Data.Planets[i].Position = Stream.ReadVector3();
			Data.Planets[i].Scale = Stream.ReadVector3();
			Data.Planets[i].Uv = Stream.ReadVector4();
		}

		// Mid
		Data.MidRgbColor = new Color32(Stream.ReadByte(), Stream.ReadByte(), Stream.ReadByte(), 0);
		Data.Mid0 = Stream.ReadSingle();
		Data.Mid1 = Stream.ReadSingle();
		Data.Mid2 = Stream.ReadSingle();
		Data.Mid3 = Stream.ReadSingle();


		//Procedural Clouds Texture
		Data.Clouds = Stream.ReadStringNull();

		Data.CloudsInt = Stream.ReadInt32();
		Data.Clouds1 = Stream.ReadVector3();
		Data.Clouds2 = Stream.ReadVector3();
		Data.Clouds3 = Stream.ReadVector3();
		Data.Clouds4 = Stream.ReadVector3();
		Data.Clouds5 = Stream.ReadVector3();
		Data.Clouds6 = Stream.ReadVector3();
		Data.Clouds7 = Stream.ReadVector3();
	}

	public void Save(BinaryWriter Stream)
	{
		Stream.Write(Data.Position);
		Stream.Write(Data.Scale);
		Stream.Write(Data.SubdivisionsAxis);
		Stream.Write(Data.SubdivisionsHeight);
		Stream.Write(Data.Value3);
		Stream.Write(new Vector3(Data.HorizoColor.r, Data.HorizoColor.g, Data.HorizoColor.b));
		Stream.Write(new Vector3(Data.ZenithColor.r, Data.ZenithColor.g, Data.ZenithColor.b));
		Stream.Write(Data.Value6);

		Stream.Write(Data.Albedo, true);
		Stream.Write(Data.Glow, true);

		Stream.Write(Data.Planets.Length);
		for(int i = 0; i < Data.Planets.Length; i++)
		{
			Stream.Write(Data.Planets[i].Position);
			Stream.Write(Data.Planets[i].Scale);
			Stream.Write(Data.Planets[i].Uv);
		}

		Stream.Write(Data.MidRgbColor.r);
		Stream.Write(Data.MidRgbColor.g);
		Stream.Write(Data.MidRgbColor.b);
		Stream.Write(Data.Mid0);
		Stream.Write(Data.Mid1);
		Stream.Write(Data.Mid2);
		Stream.Write(Data.Mid3);

		Stream.Write(Data.Clouds, true);

		Stream.Write(Data.CloudsInt);
		Stream.Write(Data.Clouds1);
		Stream.Write(Data.Clouds2);
		Stream.Write(Data.Clouds3);
		Stream.Write(Data.Clouds4);
		Stream.Write(Data.Clouds5);
		Stream.Write(Data.Clouds6);
		Stream.Write(Data.Clouds7);
	}

}
