// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: WaveTexture.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************


using System.IO;
using UnityEngine;

public class WaveTexture
{
    public string TexPath;
    public Vector2 NormalMovement;

    public float NormalRepeat;
    public void Load(BinaryReader Stream)
    {
        NormalMovement = Stream.ReadVector2();
        TexPath = Stream.ReadStringNull();
    }

    public void Save(BinaryWriter Stream)
    {
        Stream.Write(NormalMovement);
        Stream.Write(TexPath, true);
    }
}