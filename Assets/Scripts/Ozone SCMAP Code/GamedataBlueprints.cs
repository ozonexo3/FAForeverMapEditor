using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

public class GamedataBlueprints : MonoBehaviour {

	public static string[] GetBlueprint(string scd, string LocalPath){
		Debug.Log( LocalPath );
		string[] ToReturn = new string[0];


		if(!Directory.Exists(GetGamedataFile.GameDataPath)){
			Debug.LogError("Gamedata path not exist!");
			return ToReturn;
		}

		string PatchToLoad = GetGamedataFile.GameDataPath;

		if(!Directory.Exists("temfiles")) Directory.CreateDirectory("temfiles");

		ZipFile zf = null;
		try {
			Debug.Log("Get gamedata scd: " + GetGamedataFile.GameDataPath + scd);
			FileStream fs = File.OpenRead(GetGamedataFile.GameDataPath + scd);
			zf = new ZipFile(fs);



			char[] sep = ("/").ToCharArray();
			LocalPath = LocalPath.Remove(0, 1);
			string[] LocalSepPath = LocalPath.Split(sep);
			string FileName = LocalSepPath[LocalSepPath.Length - 1];
			//string AllFiles = "";

			foreach (ZipEntry zipEntry in zf) {
				if (!zipEntry.IsFile) {
					continue;
				}
				//AllFiles += zipEntry.Name + "\n";

				if(zipEntry.Name.ToLower() == LocalPath.ToLower() || zipEntry.Name == LocalPath.ToLower()){
					//Debug.LogWarning("File found!");

					// Unpack file to temp
					byte[] buffer = new byte[4096]; // 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);
					int size = 4096;
					using (FileStream streamWriter = File.Create("temfiles/" + FileName))
					{
						while (true)
						{
							size = zipStream.Read(buffer, 0, buffer.Length);
							if (size > 0)
							{
								streamWriter.Write(buffer, 0, size);
							}
							else
							{
								break;
							}
						}
					}

					ToReturn = System.IO.File.ReadAllLines("temfiles/" + FileName);

				}
			}
			//Debug.LogWarning(AllFiles);

		} finally {
			if (zf != null) {
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}

		return ToReturn;
	}

}
