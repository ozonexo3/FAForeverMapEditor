// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: Decal.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************

using System;
using UnityEngine;

public class Decal
{

    public Vector3 Position;

    public Vector3 Rotation;
    public TerrainDecalType Type;

    public string[] TexPathes = new string[2];
    public Vector3 Scale;
    public float CutOffLOD;
    public float NearCutOffLOD;

    public int OwnerArmy = -1;
    public void Load(BinaryReader Stream)
    {
        int id = Stream.ReadInt32();
        //ID
        Type = (TerrainDecalType)Stream.ReadInt32();
        int TextureCount = Stream.ReadInt32();
        TexPathes = new string[TextureCount];
        for (int i = 0; i <= TextureCount - 1; i++)
        {
            int StrLen = Stream.ReadInt32();
            TexPathes[i] = Stream.ReadString(StrLen);
        }
        Scale = Stream.ReadVector3();
        Position = Stream.ReadVector3();
        Rotation = Stream.ReadVector3();
        CutOffLOD = Stream.ReadSingle();
        NearCutOffLOD = Stream.ReadSingle();
        OwnerArmy = Stream.ReadInt32();
    }

    public void Save(BinaryWriter Stream, int Index)
    {
        Stream.Write(Index);
        Stream.Write(Convert.ToInt32(Type));
        Stream.Write(TexPathes.Length);
        for (int i = 0; i < TexPathes.Length; i++)
        {
            Stream.Write(TexPathes[i].Length);
            Stream.Write(TexPathes[i], false);
        }
        Stream.Write(Scale);
        Stream.Write(Position);
        Stream.Write(Rotation);
        Stream.Write(CutOffLOD);
        Stream.Write(NearCutOffLOD);
        Stream.Write(OwnerArmy);
    }

}