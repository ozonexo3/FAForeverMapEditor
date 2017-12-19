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
		public Vector3 Position; // Confirmed, always center of the map
		public Vector3 Scale; // Confirmed
		public int Densiti; // Confirmed, Int always 16
		public int HorizontalDensiti; // Confirmed, Int always 6
		public float Value3; // Confirmed
		public Vector3 Value4; // Confirmed
		public Vector3 Value5; // Confirmed
		public float Value6; // Confirmed


		//public byte[] BeginBytes = new byte[0];
		public string Albedo = "/textures/environment/Decal_test_Albedo001.dds";
		public string Glow = "/textures/environment/Decal_test_Glow001.dds";

		public int Length = 0;
		public Planet[] Planets = new Planet[0];

		//public byte[] MidBytesStatic = new byte[0];
		public Color32 MidRgbColor; // 3 bytes, always 0 == black?
		public float Mid0; // Confirmed
		public float Mid1; // Confirmed
		public float Mid2; // Confirmed
		public float Mid3; // Confirmed

		public string Clouds = "/textures/environment/cirrus000.dds";

		public byte[] EndBytes = new byte[0];

		public AllValues EndValue0; // Int
		public AllValues EndValue1; // Float
		public AllValues EndValue2; // Float
		public AllValues EndValue3; // Float
		public AllValues EndValue4; // Float
		public AllValues EndValue5; // Float
		public AllValues EndValue6; // Float
		public AllValues EndValue7; // Float
		public AllValues EndValue8; // Float
		public AllValues EndValue9; // Float
		public AllValues EndValue10; // Float

		[System.Serializable]
		public class Planet
		{
			public byte[] Bytes = new byte[0];
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
			//TODO
		}
	}

	[System.Serializable]
	public struct AllValues{
		public int Int;
		public float Float;
		public Color Color;
		//public short Short1;
		//public short Short2;

		public void LoadFromBytes(ref byte[] Bytes, int StartsWith)
		{
			Int = BytesToInt32(ref Bytes, StartsWith);
			Float = BytesToFloat(ref Bytes, StartsWith);
			Color = BytesToColor(ref Bytes, StartsWith);
			//Short1 = BytesToShort(ref Bytes, StartsWith);
			//Short2 = BytesToShort(ref Bytes, StartsWith + 2);
		}
	}




	public void Load(BinaryReader Stream)
	{
		// It should be some kind of settings for procedural skybox (colors, UV coordinates, rect, scale)
		// It should also contain skybox height (i hope so)

		Data = new SkyboxValues();

		// Float - 4bytes
		// Int32 - 4bytes
		// Short - 2bytes
		// Color RGB - 3 bytes
		// Color RGBA - 4 bytes

		// Sun and sky gradient settins ?
		//Data.BeginBytes = Stream.ReadBytes(64); // 16 x 4 bytes?

		Data.Position = Stream.ReadVector3();
		Data.Scale = Stream.ReadVector3();
		Data.Densiti = Stream.ReadInt32();
		Data.HorizontalDensiti = Stream.ReadInt32();
		Data.Value3 = Stream.ReadSingle();
		Data.Value4 = Stream.ReadVector3();
		Data.Value5 = Stream.ReadVector3();
		Data.Value6 = Stream.ReadSingle();


		// Planet and moon textures
		Data.Albedo = Stream.ReadStringNull();
		Data.Glow = Stream.ReadStringNull();

		// This should be settings for planets and moons on skybox

		//Array of layers
		// Planets, moons
		Data.Length = Stream.ReadInt32();
		Data.Planets = new SkyboxValues.Planet[Data.Length];
		for(int i = 0; i < Data.Planets.Length; i++)
		{
			Data.Planets[i] = new SkyboxValues.Planet();
			Data.Planets[i].Position = Stream.ReadVector3();
			Data.Planets[i].Scale = Stream.ReadVector3();
			Data.Planets[i].Uv = Stream.ReadVector4();
		}


		//Total of 19 bytes
		//Data.MidBytesStatic = Stream.ReadBytes(19); // 4x 4 bytes + RGB (3bytes)?

		Data.MidRgbColor = new Color32(Stream.ReadByte(), Stream.ReadByte(), Stream.ReadByte(), 0);
		Data.Mid0 = Stream.ReadSingle();
		Data.Mid1 = Stream.ReadSingle();
		Data.Mid2 = Stream.ReadSingle();
		Data.Mid3 = Stream.ReadSingle();

		/*
		ByteStep = 0;
		Data.MidValue0 = BytesToColorRGB(ref Data.MidBytesStatic, ByteStep);
		Data.MidValue1.LoadFromBytes(ref Data.MidBytesStatic, ByteStep);
		Data.MidValue2.LoadFromBytes(ref Data.MidBytesStatic, ByteStep);
		Data.MidValue3.LoadFromBytes(ref Data.MidBytesStatic, ByteStep);
		Data.MidValue4.LoadFromBytes(ref Data.MidBytesStatic, ByteStep);
	*/

		

		//Procedural Clouds Texture
		Data.Clouds = Stream.ReadStringNull();

		// Find total of 88 bytes
		// Animation settings and coordinates for procedural clouds
		Data.EndBytes = Stream.ReadBytes(88); // 11 x 8 bytes?
		ByteStep = 0;
		Data.EndValue0.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue1.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue2.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue3.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue4.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue5.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue6.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue7.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue8.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue9.LoadFromBytes(ref Data.EndBytes, ByteStep);
		Data.EndValue10.LoadFromBytes(ref Data.EndBytes, ByteStep);

		Debug.Log("Left: " + (88 - ByteStep));
	}

	public void Save(BinaryWriter Stream)
	{
		//Stream.Write(Data.BeginBytes);
		Stream.Write(Data.Position);
		Stream.Write(Data.Scale);
		Stream.Write(Data.Densiti);
		Stream.Write(Data.HorizontalDensiti);
		Stream.Write(Data.Value3);
		Stream.Write(Data.Value4);
		Stream.Write(Data.Value5);
		Stream.Write(Data.Value6);

		Stream.Write(Data.Albedo, true);
		Stream.Write(Data.Glow, true);

		Stream.Write(Data.Planets.Length);
		for(int i = 0; i < Data.Planets.Length; i++)
		{
			//Stream.Write(Data.Planets[i].Bytes);
			Stream.Write(Data.Planets[i].Position);
			Stream.Write(Data.Planets[i].Scale);
			Stream.Write(Data.Planets[i].Uv);
		}

		//Stream.Write(Data.MidBytesStatic);
		Stream.Write(Data.MidRgbColor.r);
		Stream.Write(Data.MidRgbColor.g);
		Stream.Write(Data.MidRgbColor.b);
		Stream.Write(Data.Mid0);
		Stream.Write(Data.Mid1);
		Stream.Write(Data.Mid2);
		Stream.Write(Data.Mid3);

		Stream.Write(Data.Clouds, true);

		Stream.Write(Data.EndBytes);


	}



#region ByteConverter

	static int ByteStep = 0;

	const int IntByteCount = 4;
	static int BytesToInt32(ref byte[] Bytes, int startsFrom = 0)
	{
		ByteStep = startsFrom + IntByteCount;
		return System.BitConverter.ToInt32(Bytes, startsFrom);
	}

	const int FloatByteCount = 4;
	static float BytesToFloat(ref byte[] Bytes, int startsFrom = 0)
	{
		ByteStep = startsFrom + FloatByteCount;
		return System.BitConverter.ToSingle(Bytes, startsFrom);
	}

	const int ShortByteCount =2;
	static short BytesToShort(ref byte[] Bytes, int startsFrom = 0)
	{
		ByteStep = startsFrom + ShortByteCount;
		return System.BitConverter.ToInt16(Bytes, startsFrom);
	}

	const int ColorByteCount = 4;
	static Color BytesToColor(ref byte[] Bytes, int startsFrom = 0)
	{
		ByteStep = startsFrom + ColorByteCount;
		return new Color32(Bytes[startsFrom], Bytes[startsFrom + 1], Bytes[startsFrom + 2], Bytes[startsFrom + 3]);
	}

	const int ColorRGBByteCount = 3;
	static Color BytesToColorRGB(ref byte[] Bytes, int startsFrom = 0)
	{
		ByteStep = startsFrom + ColorRGBByteCount;
		return new Color32(Bytes[startsFrom], Bytes[startsFrom + 1], Bytes[startsFrom + 2], 0);
	}

	const int VectorByteCount = 12;
	static Vector3 BytesToVector(ref byte[] Bytes, int startsFrom = 0)
	{
		return new Vector3(
			BytesToFloat(ref Bytes, startsFrom + 0),
			BytesToFloat(ref Bytes, startsFrom + 4),
			BytesToFloat(ref Bytes, startsFrom + 8)
			);
	}
#endregion
}
