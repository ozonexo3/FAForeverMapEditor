// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: Decal.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************

using System;
using UnityEngine;
using System.Collections.Generic;

public class Decal
{

	public TerrainDecalType Type;

	public Vector3 Position;
    public Vector3 Rotation;
	public Vector3 Scale;

    public string[] TexPathes = new string[2];
    public float CutOffLOD = 1f;
    public float NearCutOffLOD = 0.9f;

    public int OwnerArmy = -1;

	public DecalSharedSettings Shared;


	public class DecalSharedSettings
	{
		public Material SharedMaterial;
		public List<int> Ids;

		public DecalSharedSettings()
		{
			SharedMaterial = null;
			Ids = new List<int>();
		}
	}

    public void Load(BinaryReader Stream)
    {
        int id = Stream.ReadInt32();
		if(id > 0){}
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

	public bool Compare(Decal other, bool compareTransform = false)
	{
		if (compareTransform)
		{
			if (Position.x != other.Position.x || Position.y != other.Position.y || Position.z != other.Position.z)
				return false;
			if (Rotation.x != other.Rotation.x || Rotation.y != other.Rotation.y || Rotation.z != other.Rotation.z)
				return false;
			if (Scale.x != other.Scale.x || Scale.y != other.Scale.y || Scale.z != other.Scale.z)
				return false;
		}

		if (Type != other.Type)
			return false;
		if (CutOffLOD != other.CutOffLOD)
			return false;
		if (NearCutOffLOD != other.NearCutOffLOD)
			return false;
		if (OwnerArmy != other.OwnerArmy)
			return false;

		if (TexPathes.Length != other.TexPathes.Length)
			return false;

		for (int i = 0; i < TexPathes.Length; i++)
			if (TexPathes[i] != other.TexPathes[i])
				return false;

		return true;
	}

}