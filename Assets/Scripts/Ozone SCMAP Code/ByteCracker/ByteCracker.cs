using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByteCracker : MonoBehaviour {

	[System.Serializable]
	public struct Bytes4
	{
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

	void ExampleUse()
	{

		byte[] SomeBytes = new byte[20];

		Bytes4 Value0 = new Bytes4();
		Bytes4 Value1 = new Bytes4();
		Bytes4 Value2 = new Bytes4();

		ByteStep = 0;
		Value0.LoadFromBytes(ref SomeBytes, ByteStep);
		Value1.LoadFromBytes(ref SomeBytes, ByteStep);
		Value2.LoadFromBytes(ref SomeBytes, ByteStep);

		Debug.Log("Left: " + (SomeBytes.Length - ByteStep));


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

	const int ShortByteCount = 2;
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
