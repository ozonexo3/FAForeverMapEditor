// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: BinaryWriter.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************
using UnityEngine;


public class BinaryWriter : System.IO.BinaryWriter
{
    private const byte NullByte = 0;
    public BinaryWriter(System.IO.Stream Stream)
        : base(Stream)
    {
    }

    public void Write(Vector2 value)
    {
        this.Write(value.x);
        this.Write(value.y);
    }

    public void Write(Vector3 value)
    {
        this.Write(value.x);
        this.Write(value.y);
        this.Write(value.z);
    }

    public void Write(Vector4 value)
    {
        this.Write(value.x);
        this.Write(value.y);
        this.Write(value.z);
        this.Write(value.w);
    }

    public void Write(string value, bool NullTerminated)
    {
        this.Write(System.Text.Encoding.ASCII.GetBytes(value));
        if (NullTerminated)
            this.Write(NullByte);
    }

    public void Write(short[] value)
    {
        for (int i = 0; i <= value.Length - 1; i++)
        {
            this.Write(value[i]);
        }
    }

    public void Write(int[] value)
    {
        for (int i = 0; i <= value.Length - 1; i++)
        {
            this.Write(value[i]);
        }
    }

}