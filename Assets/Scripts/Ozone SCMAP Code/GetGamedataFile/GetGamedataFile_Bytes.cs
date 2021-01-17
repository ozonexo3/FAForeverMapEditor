using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using ICSharpCode.SharpZipLib.Zip;

interface IGamedataEntry
{
	string Name();
	byte[] ReadBytes();
}

public partial struct GetGamedataFile
{

	//public const string TexturesScd = "textures.scd";
	//public const string EnvScd = "env.scd";
	public const string MapScd = "maps";
	//public const string UnitsScd = "units.scd";

	// Store ZIP Read Stream in memory for faster load
	//static Dictionary<string, ZipFile> ScdFiles = new Dictionary<string, ZipFile>();

	static bool gamedataLoaded = false;
	static Dictionary<string, IGamedataEntry> gamedata = new Dictionary<string, IGamedataEntry>(32768);


	public struct GamedataZipEntry : IGamedataEntry
	{
		ZipFile zipFile;
		ZipEntry zipEntry;

		public GamedataZipEntry(ZipFile zipFile, ZipEntry zipEntry)
		{
			this.zipFile = zipFile;
			this.zipEntry = zipEntry;
		}

		public string Name()
		{
			return zipEntry.Name;
		}

		public byte[] ReadBytes()
		{
			//byte[] FinalBytes = new byte[4096]; // 4K is optimum
			byte[] FinalBytes = new byte[zipEntry.Size];
			using (Stream s = zipFile.GetInputStream(zipEntry))
			{
				s.Read(FinalBytes, 0, FinalBytes.Length);
				s.Close();
			}
			return FinalBytes;
		}
	}

	public static void LoadGamedata(bool reload = false)
	{
		Initialize();

		if (gamedataLoaded && !reload)
			return;

		gamedata.Clear();

		if (!EnvPaths.GamedataExist)
			return;


		string[] allPaths = EnvPaths.LoadGamedataPaths;

		for(int i = 0; i < allPaths.Length; i++)
		{
			string path = allPaths[i];
			if (!Directory.Exists(path))
			{
				//Debug.LogWarning("Gamedata scd file could not be found!\n" + path);
				continue;
			}

			var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".scd") || s.EndsWith(".nx2"));

			foreach(string filePath in files)
			{
				//Debug.Log("Load SCD: " + filePath);
				FileStream fs = File.OpenRead(filePath);
				ZipFile zipFile = new ZipFile(fs);

				foreach(ZipEntry ze in zipFile)
				{
					if (!ze.IsFile)
						continue;

					string entryPath = ze.Name.ToLower().Replace("\\", "/");

					if (entryPath.StartsWith("env/") || entryPath.StartsWith("textures/") || entryPath.StartsWith("units/"))
					{ // Ignore other paths because they will never be used

						if (gamedata.ContainsKey(entryPath))
						{
							gamedata[entryPath] = new GamedataZipEntry(zipFile, ze);
						}
						else
						{
							gamedata.Add(entryPath, new GamedataZipEntry(zipFile, ze));
							//Debug.Log(ze.Name);
						}
					}
				}
			}
		}

		//TODO Mods

		gamedataLoaded = true;

		Debug.Log("Loaded files from gamedata: " + gamedata.Count);
	}

	/*public static ZipFile GetZipFileInstance(string scd)
	{
		Initialize();

		if (!ScdFiles.ContainsKey(scd))
		{
			string ScdPath = EnvPaths.GamedataPath + scd;
			if (!Directory.Exists(Path.GetDirectoryName(ScdPath)) || !File.Exists(ScdPath))
			{
				Debug.LogWarning("Gamedata scd file could not be found!\n" + ScdPath);
				return null;
			}
		
			FileStream fs = File.OpenRead(ScdPath);
			ZipFile NewZipFile = new ZipFile(fs);
			ScdFiles.Add(scd, NewZipFile);
		}

		return ScdFiles[scd];
	}*/


	public static string[] GetFilesInPath(string path)
	{
		path = path.ToLower();
		var files = gamedata.Keys.Where(s => s.StartsWith(path));
		return files.ToArray();
	}


	static bool Init = false;

	static void Initialize()
	{
		if (!Init)
		{
			ZipConstants.DefaultCodePage = 0;
			Init = true;
		}
	}

	/// <summary>
	/// Find and return real path from SCD
	/// </summary>
	/// <param name="scd"></param>
	/// <param name="LocalPath"></param>
	/// <returns></returns>
	public static string FindFile(string localPath)
	{
		localPath = localPath.ToLower();

		if (IsMapPath(localPath))
		{
			return localPath;
		}


		if (string.IsNullOrEmpty(localPath))
		{
			return localPath;
		}

		if (!gamedata.ContainsKey(localPath))
		{
			Debug.LogWarning("Can't load ZipFile");
			return null;
		}

		return gamedata[localPath].Name();
	}


	/// <summary>
	/// Loads bytes from *.scd files from game gamedata folder. Returns decompressed bytes of that file.
	/// </summary>
	/// <param name="scd"></param>
	/// <param name="LocalPath"></param>
	/// <returns></returns>
	public static byte[] LoadBytes(string LocalPath)
	{
		if (IsMapPath(LocalPath))
		{
			return LoadMapBytes(LocalPath); 
		}

		if (string.IsNullOrEmpty(LocalPath)) return null;

		if (!Directory.Exists(EnvPaths.CurrentGamedataPath))
		{
			Debug.LogWarning("Gamedata path not exist!");
			return null;
		}

		if (LocalPath.StartsWith("/"))
			LocalPath = LocalPath.Remove(0, 1);

		if (!gamedata.ContainsKey(LocalPath))
			return null;

		return gamedata[LocalPath].ReadBytes();
	}

	/*static byte[] ReadZipEntryBytes(ZipEntry ze, ZipFile File)
	{
		//byte[] FinalBytes = new byte[4096]; // 4K is optimum
		Stream s = File.GetInputStream(ze);
		byte[] FinalBytes = new byte[ze.Size];
		s.Read(FinalBytes, 0, FinalBytes.Length);
		s.Close();

		return FinalBytes;
	}*/


	/// <summary>
	/// Load Bytes from files in Map folder
	/// </summary>
	/// <param name="mapPath"></param>
	/// <returns></returns>
	public static byte[] LoadMapBytes(string mapPath)
	{
		if (!mapPath.StartsWith("/"))
			mapPath = "/" + mapPath;

		string FilePath = MapAssetSystemPath(mapPath);

		if (!File.Exists(FilePath))
		{

			mapPath = TryRecoverMapAsset(mapPath);
			FilePath = MapAssetSystemPath(mapPath);

			if (!File.Exists(FilePath))
			{
				Debug.LogWarning("File does not exist! " + FilePath);
				return null;
			}
			else
			{
				Debug.Log("File " + FilePath + " restored from old map folder!");
			}
		}

		return File.ReadAllBytes(FilePath);
	}

	#region Map folder path

	public static bool IsMapPath(string mapPath)
	{
		mapPath = mapPath.ToLower();
		if (!mapPath.StartsWith("/"))
			mapPath = "/" + mapPath;

		return mapPath.StartsWith("/maps");
	}

	public static string MapAssetSystemPath(string mapPath)
	{
		if (!mapPath.StartsWith("/"))
			mapPath = "/" + mapPath;

		return mapPath.Replace("/maps", MapLuaParser.Current.FolderParentPath);
	}

	public static string TryRecoverMapAsset(string WrongPath)
	{
		bool StartChar = false;
		if (WrongPath.StartsWith("/"))
		{
			StartChar = true;
			WrongPath = WrongPath.Remove(0, 1);
		}

		WrongPath = WrongPath.Replace("\\", "/");

		string NewPath = "";
		try
		{
			string[] SplitedName = WrongPath.Split('/');
			SplitedName[1] = MapLuaParser.Current.FolderName;


			for(int i = 0; i < SplitedName.Length; i++)
			{
				if (i > 0)
					NewPath += "/";
				NewPath += SplitedName[i];
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(e);
		}
		return (StartChar?"/":"") +  NewPath;
	}

	public static string FixMapsPath(string BlueprintPath)
	{
		if (IsMapPath(BlueprintPath))
		{
			//Debug.Log(MapLuaParser.Current.FolderName);
			if (!BlueprintPath.StartsWith("/maps/" + MapLuaParser.Current.FolderName) && !BlueprintPath.StartsWith("maps/" + MapLuaParser.Current.FolderName))
			{ 
			//if (!System.IO.File.Exists(GetGamedataFile.MapAssetSystemPath(BlueprintPath)))
			//{
				string NewBlueprintPath = TryRecoverMapAsset(BlueprintPath);
				Debug.Log("Before: " + BlueprintPath + ", after: " + NewBlueprintPath);

				if (!string.IsNullOrEmpty(NewBlueprintPath))
				{
					return NewBlueprintPath;
				}
				else
				{
					Debug.LogError("Unable to recover wrong map path: " + BlueprintPath);
				}
			}
		}
		return BlueprintPath;
	}
	#endregion
}
