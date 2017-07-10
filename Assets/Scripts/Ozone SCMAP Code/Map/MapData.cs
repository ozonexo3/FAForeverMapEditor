using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


/// <summary>
/// Used to store editor only settings and to save it in map folder. It allows to recover custom data, that can't be stored inside scmap, scenario or save.
/// </summary>
public partial class MapData : MonoBehaviour {

	public const string FormatExtension = "mapdata";

	public PropsSettings Props = new PropsSettings();
	public DecalSettings Decals = new DecalSettings();
	public MarkerSettings Markers = new MarkerSettings();

	#region Public functions

	/// <summary>
	/// Loads text file and parse it to MapData class.
	/// </summary>
	/// <param name="Path">Path to map folder + map name</param>
	public void Load(string Path)
	{
		if (!File.Exists(ConvertToFilePath(Path)))
		{
			Debug.LogWarning("File not exist! " + ConvertToFilePath(Path));

			this.Props = new PropsSettings();
			this.Decals = new DecalSettings();
			this.Markers = new MarkerSettings();

			return;
		}

		string TextFile = File.ReadAllText(ConvertToFilePath(Path));
		MapData LoadedData = JsonUtility.FromJson<MapData>(TextFile);
		this.Props = LoadedData.Props;
		this.Decals = LoadedData.Decals;
		this.Markers = LoadedData.Markers; 
	}


	/// <summary>
	/// Save class to file.
	/// </summary>
	/// <param name="Path">Path to map folder + map name</param>
	public void Save(string Path)
	{
		string DataString = JsonUtility.ToJson(this);

		File.WriteAllText(ConvertToFilePath(Path), DataString);

	}

	#endregion


	static string ConvertToFilePath(string mapPath)
	{
		return mapPath + "." + FormatExtension;
	}


}
