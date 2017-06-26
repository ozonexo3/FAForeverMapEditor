using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SkyboxData
{

	//TODO find what are these values are!

	public short[] BeginValues = new short[32];
	public Vector3[] beginvectors;
	public float[] BeginFloats;
	public int[] BeginInts;
	public Color[] BeginColors;

	public string Albedo = "/textures/environment/Decal_test_Albedo001.dds";
	public string Glow = "/textures/environment/Decal_test_Glow001.dds";

	public int Length = 0;
	public byte[] MidBytes = new byte[0];
	public byte[] MidBytesStatic = new byte[0];

	public string Clouds = "/textures/environment/cirrus000.dds";

	public short[] EndValues = new short[44];


	public void Load(BinaryReader Stream)
	{
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


		// Planet and moon textures
		Albedo = Stream.ReadStringNull();
		Glow = Stream.ReadStringNull();


		// This should be settings for planets and moons on skybox

		//Array
		Length = Stream.ReadInt32();
		if(Length > 0)
			MidBytes = Stream.ReadBytes(Length * 40);

		//Total of 19 bytes
		MidBytesStatic = Stream.ReadBytes(19);



		//Procedural Clouds Texture
		Clouds = Stream.ReadStringNull();

		// Find total of 88 bytes
		// Animation settings and coordinates for procedural clouds
		EndValues = new short[44];
		for (int i = 0; i < EndValues.Length; i++)
		{
			EndValues[i] = Stream.ReadInt16();
		}

	}

	public void Save(BinaryWriter Stream)
	{
		for (int i = 0; i < BeginValues.Length; i++)
		{
			Stream.Write(BeginValues[i]);
		}

		Stream.Write(Albedo, true);
		Stream.Write(Glow, true);

		Stream.Write(Length);
		Stream.Write(MidBytes);
		Stream.Write(MidBytesStatic);
		
		Stream.Write(Clouds, true);

		for (int i = 0; i < EndValues.Length; i++)
		{
			Stream.Write(EndValues[i]);
		}

	}
}
