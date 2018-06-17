using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

using System.IO;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;


public partial struct GetGamedataFile
{

	public const string TexturesScd = "textures.scd";
	public const string EnvScd = "env.scd";
	public const string MapScd = "maps";
	public const string UnitsScd = "units.scd";

	// Store ZIP Read Stream in memory for faster load
	static Dictionary<string, ZipFile> ScdFiles = new Dictionary<string, ZipFile>();

	public static ZipFile GetZipFileInstance(string scd)
	{
		if (!Init)
		{
			FafNotInstalled = !System.IO.Directory.Exists(EnvPaths.FAFGamedataPath);
			ZipConstants.DefaultCodePage = 0;
			Init = true;
		}

		if (!ScdFiles.ContainsKey(scd))
		{
			FileStream fs = File.OpenRead(EnvPaths.GamedataPath + scd);
			ZipFile NewZipFile = new ZipFile(fs);
			ScdFiles.Add(scd, NewZipFile);
		}

		return ScdFiles[scd];
	}

	static bool Init = false;

	/// <summary>
	/// Loads bytes from *.scd files from game gamedata folder. Returns decompressed bytes of that file.
	/// </summary>
	/// <param name="scd"></param>
	/// <param name="LocalPath"></param>
	/// <returns></returns>
	public static byte[] LoadBytes(string scd, string LocalPath)
	{
		if (IsMapPath(LocalPath))
		{
			return LoadBytes(LocalPath); 
		}


		if (string.IsNullOrEmpty(LocalPath)) return null;

		if (!Directory.Exists(EnvPaths.CurrentGamedataPath))
		{
			Debug.LogError("Gamedata path not exist!");
			return null;
		}


		// Get ZipFile
		ZipFile ZipFileInstance = GetZipFileInstance(scd);
		if(ZipFileInstance == null)
		{
			Debug.LogError("Can't load ZipFile");
			return null;
		}


		if (LocalPath.StartsWith("/"))
			LocalPath = LocalPath.Remove(0, 1);


		ZipEntry zipEntry2 = ZipFileInstance.GetEntry(LocalPath);

		// FafEntry
		ZipFile FafZipFileInstance = GetFAFZipFileInstance(scd);
		if (FafZipFileInstance != null)
		{
			// Found nx2 file, try load it instead
			ZipEntry zipEntryFAF = FafZipFileInstance.GetEntry(LocalPath);

			if (zipEntryFAF == null)
			{
				int FoundEntry = FafZipFileInstance.FindEntry(LocalPath, true);

				if (FoundEntry >= 0 && FoundEntry < FafZipFileInstance.Count)
					zipEntryFAF = FafZipFileInstance[FoundEntry];
			}

			if (zipEntryFAF != null && zipEntryFAF.IsFile)
			{
				byte[] FafBytes = ReadZipEntryBytes(ref zipEntryFAF, ref FafZipFileInstance);
				if (FafBytes != null && FafBytes.Length > 0)
					return FafBytes;
			}
		}

		if (zipEntry2 == null)
		{

			int FoundEntry = ZipFileInstance.FindEntry(LocalPath, true);

			if (FoundEntry >= 0 && FoundEntry < ZipFileInstance.Count)
				zipEntry2 = ZipFileInstance[FoundEntry];

			if (zipEntry2 == null)
			{
				//Debug.LogWarning("Zip Entry is empty for: " + LocalPath);
				return null;
			}
		}

		return ReadZipEntryBytes(ref zipEntry2, ref ZipFileInstance);
	}

	static byte[] ReadZipEntryBytes(ref ZipEntry ze, ref ZipFile File)
	{
		byte[] FinalBytes = new byte[4096]; // 4K is optimum

		Stream s = File.GetInputStream(ze);
		FinalBytes = new byte[ze.Size];
		s.Read(FinalBytes, 0, FinalBytes.Length);
		s.Close();

		return FinalBytes;

	}


	/// <summary>
	/// Load Bytes from files in Map folder
	/// </summary>
	/// <param name="mapPath"></param>
	/// <returns></returns>
	public static byte[] LoadBytes(string mapPath)
	{
		if (!mapPath.StartsWith("/"))
			mapPath = "/" + mapPath;

		string FilePath = MapAssetSystemPath(mapPath);

		if (!System.IO.File.Exists(FilePath))
		{

			mapPath = TryRecoverMapAsset(mapPath);
			FilePath = MapAssetSystemPath(mapPath);

			if (!System.IO.File.Exists(FilePath))
			{
				Debug.LogError("File does not exist! " + FilePath);
				return null;
			}
			else
			{
				Debug.Log("File " + FilePath + " restored from old map folder!");
			}
		}

		return System.IO.File.ReadAllBytes(FilePath);
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
		if (GetGamedataFile.IsMapPath(BlueprintPath))
		{
			//Debug.Log(MapLuaParser.Current.FolderName);
			if (!BlueprintPath.StartsWith("/maps/" + MapLuaParser.Current.FolderName) && !BlueprintPath.StartsWith("maps/" + MapLuaParser.Current.FolderName))
			{ 
			//if (!System.IO.File.Exists(GetGamedataFile.MapAssetSystemPath(BlueprintPath)))
			//{
				string NewBlueprintPath = GetGamedataFile.TryRecoverMapAsset(BlueprintPath);
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
