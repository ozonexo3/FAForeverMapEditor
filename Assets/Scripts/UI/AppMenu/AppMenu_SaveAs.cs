using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using System.Text;
using System.Runtime.InteropServices;
using SFB;
using System.Linq;
using B83.Win32;
using System.IO;

public partial class AppMenu : MonoBehaviour
{

	#region SaveAs
	public void SaveMapAs()
	{
		if (!MapLuaParser.IsMapLoaded)
			return;

		LateUpdate();


		var paths = StandaloneFileBrowser.OpenFolderPanel("Save map as...", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			if (System.IO.Directory.GetDirectories(paths[0]).Length > 0 || System.IO.Directory.GetFiles(paths[0]).Length > 0)
			{
				Debug.LogWarning("Selected directory is not empty! " + paths[0]);
				GenericPopup.ShowPopup(GenericPopup.PopupTypes.Error, "Error", "Selected folder is not empty! Select empty folder.", "OK", null);
				return;
			}

			string[] PathSeparation = paths[0].Replace("\\", "/").Split("/".ToCharArray());


			string ChosenFolderName = PathSeparation[PathSeparation.Length - 1];

			string NewFolderName = ChosenFolderName;
			NewFolderName = NewFolderName.Replace(" ", "_");

			int ReplaceVersion = GetVersionControll(NewFolderName);
			if(ReplaceVersion >= 0)
			{
				Debug.Log("Replace version: " + ReplaceVersion);
				MapLuaParser.Current.ScenarioLuaFile.Data.map_version = ReplaceVersion;
			}
			else
			{
				MapLuaParser.Current.ScenarioLuaFile.Data.map_version = 1;
			}

			NewFolderName = ForceVersionControllValue(NewFolderName, (int)MapLuaParser.Current.ScenarioLuaFile.Data.map_version);
			string FileBeginName = NonVersionControlledName(NewFolderName);


			if(SaveAsNewFolder(NewFolderName, FileBeginName))
			{
				if(ChosenFolderName != NewFolderName)
				{
					string LogText = "Wrong folder name: " + ChosenFolderName + "\n Instead saved as : " + NewFolderName;
					Debug.Log(LogText);
					GenericInfoPopup.ShowInfo(LogText);

				}

			}
		}
	}

	public void SaveAsNewVersion()
	{
		int NextVersion = (int)MapLuaParser.Current.ScenarioLuaFile.Data.map_version + 1;

		GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "Create new version", "Create version " + NextVersion + " of this map?", "Yes", SaveAsNewVersionYes, "No", null);
	}

	public void SaveAsNewVersionYes()
	{
		MapLuaParser.Current.ScenarioLuaFile.Data.map_version++;

		string NewFolderName = ForceVersionControllValue(MapLuaParser.Current.FolderName, (int)MapLuaParser.Current.ScenarioLuaFile.Data.map_version);

		string NewFileName = NonVersionControlledName(NewFolderName);

		if (SaveAsNewFolder(NewFolderName, NewFileName))
		{

		}
		else
		{
			// Error! Revert changes
			MapLuaParser.Current.ScenarioLuaFile.Data.map_version--;
		}

	}

	bool SaveAsNewFolder(string NewFolderName, string FileBeginName = "")
	{
		string SystemPath = MapLuaParser.Current.FolderParentPath + NewFolderName;

		if (!System.IO.Directory.Exists(SystemPath))
		{
			System.IO.Directory.CreateDirectory(SystemPath);
		}
		else if (System.IO.Directory.GetFiles(SystemPath + "/").Length <= 0)
		{

		}
		else
		{
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.Error, "Error", "Next map version folder already exist.\nRemove it or load never version.", "OK", null);

			return false;
		}

		string OldFolderName = MapLuaParser.Current.FolderName;

		MapLuaParser.Current.FolderName = NewFolderName;


		string OldScript = MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath);
		string OldOptionsFile = OldScript.Replace("_script.lua", "_options.lua");

		if (string.IsNullOrEmpty(FileBeginName))
		{
			FileBeginName = MapLuaParser.Current.ScenarioFileName.Replace("_scenario", "");
		}
		else
		{
			MapLuaParser.Current.ScenarioFileName = FileBeginName + "_scenario";
		}

		MapLuaParser.Current.ScenarioLuaFile.Data.map = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + ".scmap";
		MapLuaParser.Current.ScenarioLuaFile.Data.save = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + "_save.lua";
		MapLuaParser.Current.ScenarioLuaFile.Data.script = "/maps/" + MapLuaParser.Current.FolderName + "/" + FileBeginName + "_script.lua";
		MapLuaParser.Current.ScenarioLuaFile.Data.preview = "";

		LoadRecentMaps.MoveLastMaps(MapLuaParser.Current.ScenarioFileName, MapLuaParser.Current.FolderName, MapLuaParser.Current.FolderParentPath);

		string EnvPath = MapLuaParser.Current.FolderParentPath + OldFolderName + "/env";
		if (System.IO.Directory.Exists(EnvPath))
		{
			DirectoryCopy(EnvPath, MapLuaParser.Current.FolderParentPath + NewFolderName + "/env", true);
		}

		MapLuaParser.Current.SaveMap(false);

		string LoadScript = System.IO.File.ReadAllText(OldScript);
		//Replace old folder name to new one
		LoadScript = LoadScript.Replace(OldFolderName + "/", NewFolderName + "/");
		System.IO.File.WriteAllText(MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath), LoadScript);

		//System.IO.File.Copy(OldScript, MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath));

		if (System.IO.File.Exists(OldOptionsFile))
		{ // Found options, copy it
			System.IO.File.Copy(OldOptionsFile, MapLuaParser.Current.ScenarioLuaFile.Data.script.Replace("/maps/", MapLuaParser.Current.FolderParentPath).Replace("_script.lua", "_options.lua"));
		}

		EditingMenu.MapInfoMenu.UpdateFields();
		WindowStateSever.WindowStateSaver.ChangeWindowName(NewFolderName);

		return true;
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);

		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, false);
		}

		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}
	#endregion


}