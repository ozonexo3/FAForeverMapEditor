using UnityEngine;
using System.Collections;
using System;

using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;


public partial class GetGamedataFile : MonoBehaviour
{

	/// <summary>
	/// Loads bytes from *.scd files from game gamedata folder. Returns decompressed bytes of that file.
	/// </summary>
	/// <param name="scd"></param>
	/// <param name="LocalPath"></param>
	/// <returns></returns>
	public static byte[] LoadBytes(string scd, string LocalPath){
		if (string.IsNullOrEmpty(LocalPath)) return null;

		if (!Directory.Exists(EnvPaths.GetGamedataPath()))
		{
			Debug.LogError("Gamedata path not exist!");
			return null;
		}
		ZipFile zf = null;
		byte[] FinalBytes = new byte[0];

		try
		{
			FileStream fs = File.OpenRead(EnvPaths.GetGamedataPath() + scd);
			zf = new ZipFile(fs);


			ZipEntry zipEntry2 = zf.GetEntry(LocalPath);
			if (zipEntry2 == null)
			{
				Debug.LogWarning("Zip Entry is empty for: " + LocalPath);
				return null;
			}

			FinalBytes = new byte[4096]; // 4K is optimum

			if (zipEntry2 != null)
			{
				Stream s = zf.GetInputStream(zipEntry2);
				FinalBytes = new byte[zipEntry2.Size];
				s.Read(FinalBytes, 0, FinalBytes.Length);
			}
		}
		finally
		{
			if (zf != null)
			{
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}
		return FinalBytes;
	}

}
