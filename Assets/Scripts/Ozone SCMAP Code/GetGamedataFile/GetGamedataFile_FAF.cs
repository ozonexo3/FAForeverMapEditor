using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

public partial struct GetGamedataFile
{


	static Dictionary<string, ZipFile> FAFScdFiles = new Dictionary<string, ZipFile>();
	static Dictionary<string, string[]> FAFNewEntries = new Dictionary<string, string[]>();
	//static Dictionary<string, string[]> FAFNewFolders = new Dictionary<string, string[]>();

	static bool FafNotInstalled = false;

	public static ZipFile GetFAFZipFileInstance(string scd)
	{
		if (!Init)
		{
			FafNotInstalled = !System.IO.Directory.Exists(EnvPaths.FAFGamedataPath);
			ZipConstants.DefaultCodePage = 0;
			Init = true;
		}

		if (FafNotInstalled)
			return null;

		if (!ScdFiles.ContainsKey(scd))
		{
			// Original files not loaded, return
			return null;
		}

		if (!FAFScdFiles.ContainsKey(scd))
		{
			string ScdPath = EnvPaths.FAFGamedataPath + scd.Replace("scd", "nx2");
			if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(ScdPath)) || !System.IO.File.Exists(ScdPath))
			{
				FAFScdFiles.Add(scd, null);
				return null;
			}
			FileStream fs = File.OpenRead(ScdPath);
			ZipFile NewZipFile = new ZipFile(fs);
			FAFScdFiles.Add(scd, NewZipFile);

			LoadNewEntriesPaths(scd);
		}

		return FAFScdFiles[scd];
	}

	public static string[] GetNewFafFiles(string scd)
	{
		if (!FAFScdFiles.ContainsKey(scd))
		{
			Debug.LogWarning("No FAF scd");
			return new string[0];
		}

		return FAFNewEntries[scd];
	}

	/*
	public static string[] GetNewFafFolders(string scd)
	{
		if (!FAFScdFiles.ContainsKey(scd))
			return new string[0];

		return FAFNewFolders[scd];
	}
	*/

	static void LoadNewEntriesPaths(string scd)
	{
		ZipFile Source = ScdFiles[scd];
		ZipFile Faf = FAFScdFiles[scd];

		List<string> NewPaths = new List<string>();
		//List<string> NewFolders = new List<string>();
		bool IsTexturesScd = scd == TexturesScd;
		string Log = "";
		foreach (ZipEntry zipEntry in Faf)
		{
			string name = zipEntry.Name;

			if (!zipEntry.IsFile)
			{
				//if (Source.FindEntry(name, false) < 0)
				//	NewFolders.Add(name);
				continue;
			}


			if (IsTexturesScd)
			{
				if (!name.StartsWith("textures/environment"))
					continue;

			}
			if (Source.FindEntry(name, false) < 0)
			{
				NewPaths.Add(name);
				Log += name + "\n";
			}
		}

		if (NewPaths.Count > 0)
		{
			//Debug.Log("New FAF files: " + scd + ": " + NewPaths.Count);
		}
		FAFNewEntries.Add(scd, NewPaths.ToArray());
		//FAFNewFolders.Add(scd, NewFolders.ToArray());
	}

}
