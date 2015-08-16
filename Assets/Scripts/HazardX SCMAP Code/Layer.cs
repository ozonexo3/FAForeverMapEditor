// ***************************************************************************************
// * SCMAP Loader
// * Copyright Unknown
// * Filename: Layer.cs
// * Source: http://www.hazardx.com/details.php?file=82
// ***************************************************************************************


public class Layer
{

    public string PathTexture;
    public string PathNormalmap;
    public float ScaleTexture;

    public float ScaleNormalmap;
    public Layer()
    {
        this.PathTexture = "";
        this.PathNormalmap = "";
        this.ScaleTexture = 1f;
        this.ScaleNormalmap = 1f;
    }

    public void Load(BinaryReader Stream)
    {
        PathTexture = Stream.ReadStringNull();
        PathNormalmap = Stream.ReadStringNull();
        ScaleTexture = Stream.ReadSingle();
        ScaleNormalmap = Stream.ReadSingle();
    }
    public void LoadAlbedo(BinaryReader Stream)
    {
        PathTexture = Stream.ReadStringNull();
        ScaleTexture = Stream.ReadSingle();
    }
    public void LoadNormal(BinaryReader Stream)
    {
        PathNormalmap = Stream.ReadStringNull();
        ScaleNormalmap = Stream.ReadSingle();
    }

    public void Save(BinaryWriter Stream)
    {
        Stream.Write(PathTexture, true);
        Stream.Write(PathNormalmap, true);
        Stream.Write(ScaleTexture);
        Stream.Write(ScaleNormalmap);
    }
    public void SaveAlbedo(BinaryWriter Stream)
    {
        string Path = PathTexture.Replace("\\", "/");
        if (!string.IsNullOrEmpty(Path) & !Path.StartsWith("/"))
            Path = "/" + Path;
        Stream.Write(Path, true);
        Stream.Write(ScaleTexture);
    }
    public void SaveNormal(BinaryWriter Stream)
    {
        string Path = PathNormalmap.Replace("\\", "/");
        if (!string.IsNullOrEmpty(Path) & !Path.StartsWith("/"))
            Path = "/" + Path;
        Stream.Write(Path, true);
        Stream.Write(ScaleNormalmap);
    }

}