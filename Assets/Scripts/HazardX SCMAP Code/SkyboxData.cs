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
		public byte[] BeginBytes = new byte[0];
		public string Albedo = "/textures/environment/Decal_test_Albedo001.dds";
		public string Glow = "/textures/environment/Decal_test_Glow001.dds";

		public int Length = 0;
		public byte[] MidBytes = new byte[0];
		public byte[] MidBytesStatic = new byte[0];

		public string Clouds = "/textures/environment/cirrus000.dds";

		public byte[] EndBytes = new byte[0];
	}


	public void Load(BinaryReader Stream)
	{
		/*
		beginvectors = new Vector3[5];
		BeginFloats = new float[10];
		BeginInts = new int[8];
		BeginColors = new Color[5];

		// Find total of 64 bytes
		// It should be some kind of settings for procedural skybox (colors, coordinates, scale)

		beginvectors[0] = Stream.ReadVector3(); // Good Value - 12 bytes, always (256, 0, 256)
		BeginFloats[0] = Stream.ReadSingle(); // ? Good value ? - 4 bytes, coordinate??

		BeginInts[0] = Stream.ReadInt32(); // 4 bytes, RGBA Color? always pink
		BeginColors[0] = Map.Int32ToColor(BeginInts[0], false);

		BeginInts[1] = Stream.ReadInt32(); // 4 bytes, RGBA Color? always green
		BeginColors[1] = Map.Int32ToColor(BeginInts[1], false);

		BeginInts[2] = Stream.ReadInt32(); // 4 bytes - always 16
		BeginColors[2] = Map.Int32ToColor(BeginInts[2], false);
		BeginInts[3] = Stream.ReadInt32(); // 4 bytes - always 6
		BeginColors[3] = Map.Int32ToColor(BeginInts[3], false);

		//Coordinates? Maybe colors
		BeginFloats[1] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[2] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[3] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[4] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[5] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[6] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[7] = Stream.ReadSingle(); // 4 bytes
		BeginFloats[8] = Stream.ReadSingle(); // 4 bytes
		*/

		Data = new SkyboxValues();

		Data.BeginBytes = Stream.ReadBytes(64);

		// Planet and moon textures
		Data.Albedo = Stream.ReadStringNull();
		Data.Glow = Stream.ReadStringNull();


		// This should be settings for planets and moons on skybox

		//Array
		Data.Length = Stream.ReadInt32();
		if(Data.Length > 0)
			Data.MidBytes = Stream.ReadBytes(Data.Length * 40);

		//Total of 19 bytes
		Data.MidBytesStatic = Stream.ReadBytes(19);



		//Procedural Clouds Texture
		Data.Clouds = Stream.ReadStringNull();

		// Find total of 88 bytes
		// Animation settings and coordinates for procedural clouds
		Data.EndBytes = Stream.ReadBytes(88);

	}

	public void Save(BinaryWriter Stream)
	{
		Stream.Write(Data.BeginBytes);

		Stream.Write(Data.Albedo, true);
		Stream.Write(Data.Glow, true);

		Stream.Write(Data.Length);
		Stream.Write(Data.MidBytes);
		Stream.Write(Data.MidBytesStatic);
		
		Stream.Write(Data.Clouds, true);

		Stream.Write(Data.EndBytes);
	}
}
