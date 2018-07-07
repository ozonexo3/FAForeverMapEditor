using UnityEngine;
using SFB;
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
			// If its the same folder, then its the same map
			if (paths[0].Replace("\\", "/") + "/" == MapLuaParser.LoadedMapFolderPath)
			{
				MapLuaParser.Current.SaveMap();
				return;
			}

			string[] PathSeparation = paths[0].Replace("\\", "/").Split("/".ToCharArray());
			string ChosenFolderName = PathSeparation[PathSeparation.Length - 1];
			string NewFolderName = ChosenFolderName;
			NewFolderName = NewFolderName.Replace(" ", "_");
			int ReplaceVersion = GetVersionControll(NewFolderName);
			if (ReplaceVersion <= 0)
				ReplaceVersion = 1; // Map without versioning should always be v1
				//ReplaceVersion = (int)MapLuaParser.Current.ScenarioLuaFile.Data.map_version;
			NewFolderName = ForceVersionControllValue(NewFolderName, ReplaceVersion);

			string ChosenPath = "";

			if (ChosenFolderName != NewFolderName)
			{
				// Combine system path using old separation and new folder name
				ChosenPath = "";
				for (int i = 0; i < PathSeparation.Length - 1; i++)
				{
					ChosenPath += PathSeparation[i] + "/";
				}
				ChosenPath += NewFolderName;

				// If selected folder does not exist, then remove wrong folder, and make a new, proper one
				if (!Directory.Exists(ChosenPath))
				{
					Directory.CreateDirectory(ChosenPath);

					// Remove only when folder is empty
					if (Directory.GetFiles(paths[0]).Length <= 0 && System.IO.Directory.GetFiles(paths[0]).Length <= 0)
					{
						Directory.Delete(paths[0]);
					}
				}
			}
			else
			{
				ChosenPath = paths[0];
			}


			string[] FilePaths = System.IO.Directory.GetFiles(ChosenPath);
			if (System.IO.Directory.GetDirectories(ChosenPath).Length > 0 || System.IO.Directory.GetFiles(ChosenPath).Length > 0)
			{
				Debug.LogWarning("Selected directory is not empty! " + ChosenPath);

				// Check if its a map folder 
				foreach (string path in FilePaths)
				{
					// If contains any of the map files, then it need to be a map
					if (path.EndsWith("_scenario.lua") || path.EndsWith("_save.lua") || path.EndsWith(".scmap"))
					{
						OverwritePath = ChosenPath;
						GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "Folder is not empty!", "Overwrite map " + NewFolderName + "?\nThis can't be undone!", "Overwrite", OverwriteYes, "Cancel", null);
						return;
					}
				}

				// Not a map folder, return error to prevent data lose
				GenericPopup.ShowPopup(GenericPopup.PopupTypes.Error, "Error!", "Selected " + NewFolderName + " folder is not empty and it's not a map folder!", "OK", null);
				return;
			}

			if(SaveAsNewFolder(NewFolderName, NonVersionControlledName(NewFolderName)))
			{
				// Inform user, that map was saved into different folder than he chose
				if (ChosenFolderName != NewFolderName)
				{
					string LogText = "Wrong folder name: " + ChosenFolderName + "\n Instead saved as: " + NewFolderName;
					Debug.Log(LogText);
					GenericInfoPopup.ShowInfo(LogText);
				}
			}
		}
	}

	static string OverwritePath = "";
	void OverwriteYes()
	{
		Debug.Log("Overwrite path: " + OverwritePath);

		// Remove files from folder
		string[] FilePaths = System.IO.Directory.GetFiles(OverwritePath);
		foreach (string path in FilePaths)
		{
			File.Delete(path);
		}

		// Remove directories from folder
		string[] DirPaths = System.IO.Directory.GetDirectories(OverwritePath);
		foreach (string path in DirPaths)
		{
			Directory.Delete(path, true);
		}

		// Now we can save to clean folder
		SaveAsNewFolder(OverwritePath, NonVersionControlledName(OverwritePath));
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
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.Error, "Error", "Trying to save map, but folder " + NewFolderName + " is not empty!\nSave as can be only used for empty folders.", "OK", null);

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