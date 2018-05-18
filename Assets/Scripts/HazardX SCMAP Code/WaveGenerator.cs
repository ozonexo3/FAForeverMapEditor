// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: WaveGenerator.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************
using UnityEngine;

[System.Serializable]
public class WaveGenerator
{

    public Vector3 Position;

    public float Rotation;
    public string TextureName;
    public string RampName;

    public Vector3 Velocity;
    public float LifetimeFirst;
    public float LifetimeSecond;
    public float PeriodFirst;
    public float PeriodSecond;
    public float ScaleFirst;

    public float ScaleSecond;
    public float FrameCount;
    public float FrameRateFirst;
    public float FrameRateSecond;

    public float StripCount;
    public void Load(BinaryReader Stream)
    {
        TextureName = Stream.ReadStringNull();
        RampName = Stream.ReadStringNull();

        Position = Stream.ReadVector3();
        Rotation = Stream.ReadSingle();
        Velocity = Stream.ReadVector3();

        LifetimeFirst = Stream.ReadSingle();
        LifetimeSecond = Stream.ReadSingle();
        PeriodFirst = Stream.ReadSingle();
        PeriodSecond = Stream.ReadSingle();
        ScaleFirst = Stream.ReadSingle();
        ScaleSecond = Stream.ReadSingle();

        FrameCount = Stream.ReadSingle();
        FrameRateFirst = Stream.ReadSingle();
        FrameRateSecond = Stream.ReadSingle();
        StripCount = Stream.ReadSingle();
    }

    public static void Skip(BinaryReader Stream)
    {
        Stream.SeekSkipNull();
        Stream.SeekSkipNull();
        Stream.BaseStream.Position += 68;
    }

    public void Save(BinaryWriter Stream)
    {
        Stream.Write(TextureName, true);
        Stream.Write(RampName, true);

        Stream.Write(Position);
        Stream.Write(Rotation);
        Stream.Write(Velocity);

        Stream.Write(LifetimeFirst);
        Stream.Write(LifetimeSecond);
        Stream.Write(PeriodFirst);
        Stream.Write(PeriodSecond);
        Stream.Write(ScaleFirst);
        Stream.Write(ScaleSecond);

        Stream.Write(FrameCount);
        Stream.Write(FrameRateFirst);
        Stream.Write(FrameRateSecond);
        Stream.Write(StripCount);
    }

}