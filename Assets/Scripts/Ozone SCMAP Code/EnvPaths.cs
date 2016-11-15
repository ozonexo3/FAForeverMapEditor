using UnityEngine;
using System.Collections;
using Microsoft.Win32;

public class EnvPaths : MonoBehaviour {

	public static string DefaultMapPath;
	public static string DefaultGamedataPath;

	static RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");

	public static void GenerateDefaultPaths(){
		DefaultMapPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments).Replace ("\\", "/") + "/My Games/Gas Powered Games/Supreme Commander Forged Alliance/Maps/";


		DefaultGamedataPath = FindByDisplayName(regKey, "Supreme Commander: Forged Alliance");
		if (string.IsNullOrEmpty (DefaultGamedataPath))
			DefaultGamedataPath = "gamedata/";
	}


	private static string FindByDisplayName(RegistryKey parentKey, string name)
	{
		string[] nameList = parentKey.GetSubKeyNames();
		for (int i = 0; i < nameList.Length; i++)
		{
			RegistryKey regKey =  parentKey.OpenSubKey(nameList[i]);
			try
			{
				if (regKey.GetValue("DisplayName").ToString() == name)
				{
					return regKey.GetValue("InstallLocation").ToString();
				}
				else{
					Debug.LogWarning(regKey.GetValue("InstallLocation").ToString());
				}
			}
			catch { }
		}
		return "";
	}

}
