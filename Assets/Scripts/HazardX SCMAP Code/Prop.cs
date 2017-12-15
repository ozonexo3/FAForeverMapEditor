// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: Prop.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************
using UnityEngine;


public partial class Prop
{

    public Vector3 Position;
    public string BlueprintPath;
    public Vector3 RotationX;
    public Vector3 RotationY;
    public Vector3 RotationZ;
	public Vector3 Scale;

    private static Vector3 V1 = new Vector3(1f, 1f, 1f);
    public void Load(BinaryReader Stream)
    {
        BlueprintPath = Stream.ReadStringNull();
        Position = Stream.ReadVector3();
        RotationX = Stream.ReadVector3();
        RotationY = Stream.ReadVector3();
        RotationZ = Stream.ReadVector3();
		Scale = Stream.ReadVector3();
        // scale (unused)
    }

    public void Save(BinaryWriter Stream)
    {
        Stream.Write(BlueprintPath, true);
        Stream.Write(Position);
        Stream.Write(RotationX);
        Stream.Write(RotationY);
        Stream.Write(RotationZ);
        Stream.Write(V1);
        // scale (unused)
    }

}