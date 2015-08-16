// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: DecalGroup.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************


public class IntegerGroup
{
    public int ID;
    public string Name;

    public int[] Data = new int[0];
    public void Load(BinaryReader Stream)
    {
        ID = Stream.ReadInt32();
        Name = Stream.ReadStringNull();
        int Length = Stream.ReadInt32();
        Data = Stream.ReadInt32Array(Length);
    }

    public void Save(BinaryWriter Stream)
    {
        Stream.Write(ID);
        if (string.IsNullOrEmpty(Name))
            Name = "Group_" + ID;
        Stream.Write(Name, true);
        Stream.Write(Data.Length);
        Stream.Write(Data);
    }

}