using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;


public partial struct GetGamedataFile
{

	// Store ZIP Read Stream in memory for faster load
	static Dictionary<string, ZipFile> ScdFiles = new Dictionary<string, ZipFile>();


	static bool Init = false;

	/// <summary>
	/// Loads bytes from *.scd files from game gamedata folder. Returns decompressed bytes of that file.
	/// </summary>
	/// <param name="scd"></param>
	/// <param name="LocalPath"></param>
	/// <returns></returns>
	public static byte[] LoadBytes(string scd, string LocalPath)
	{
		if (!Init)
		{
			ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = 0;
			Init = true;
		}

		if (LocalPath.StartsWith("/maps"))
		{
			return LoadBytes(LocalPath); 
		}


		if (string.IsNullOrEmpty(LocalPath)) return null;

		if (!Directory.Exists(EnvPaths.CurrentGamedataPath))
		{
			Debug.LogError("Gamedata path not exist!");
			return null;
		}


		if (!ScdFiles.ContainsKey(scd))
		{
			FileStream fs = File.OpenRead(EnvPaths.GetGamedataPath() + scd);
			ZipFile NewZipFile = new ZipFile(fs);
			ScdFiles.Add(scd, NewZipFile);
		}

		if (LocalPath.StartsWith("/"))
			LocalPath = LocalPath.Remove(0, 1);

		ZipEntry zipEntry2 = ScdFiles[scd].GetEntry(LocalPath);

		if (zipEntry2 == null)
		{

			int FoundEntry = ScdFiles[scd].FindEntry(LocalPath, true);

			if (FoundEntry >= 0 && FoundEntry < ScdFiles[scd].Count)
				zipEntry2 = ScdFiles[scd][FoundEntry];

			if (zipEntry2 == null)
			{
				//Debug.LogWarning("Zip Entry is empty for: " + LocalPath);
				return null;
			}
		}

		byte[] FinalBytes = new byte[4096]; // 4K is optimum

		Stream s = ScdFiles[scd].GetInputStream(zipEntry2);
		FinalBytes = new byte[zipEntry2.Size];
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
		return System.IO.File.ReadAllBytes(mapPath.Replace("/maps", MapLuaParser.Current.FolderParentPath));
	}
}
