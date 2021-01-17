using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using SFB;
using Ozone.UI;

public class FafEditorSettings : MonoBehaviour
{

	public InputField PathField;
	public InputField MapsPathField;
	public InputField BackupPathField;
	public Slider HistorySlider;
	public Undo History;

	public Dropdown PlayAs;
	public Toggle FogOfWar;
	public Toggle Markers2D;
	public Toggle HeightmapClamp;

	public UiTextField UiScale;

	public const int DefaultUndoHistory = 50;
	public const string UndoHistory = "UndoHistrySteps";

	void OnEnable()
	{
		PathField.text = EnvPaths.GetInstalationPath();
		MapsPathField.text = EnvPaths.GetMapsPath();
		BackupPathField.text = EnvPaths.GetBackupPath();

		PlayAs.value = GetFaction();
		FogOfWar.isOn = GetFogOfWar();
		Markers2D.isOn = GetMarkers2D();
		HeightmapClamp.isOn = GetHeightmapClamp();

		UiScale.SetValue(GetUiScale());
		UiScaler.UpdateUiScale();
	}

	private void OnDisable()
	{
		UiScaler.UpdateUiScale();
	}


	public void Open()
	{
		HistorySlider.value = PlayerPrefs.GetInt(UndoHistory, DefaultUndoHistory);
		gameObject.SetActive(true);
	}

	public void Close()
	{
		OnUiScaleChanged();
		gameObject.SetActive(false);
	}

	public void Save()
	{
		if (string.IsNullOrEmpty(PathField.text))
		{
			GenericInfoPopup.ShowInfo("Game installation path can't be empty!");
		}
		else
		{
			EnvPaths.SetInstalationPath(PathField.text);
		}

		if (string.IsNullOrEmpty(MapsPathField.text))
		{
			GenericInfoPopup.ShowInfo("Maps folder path can't be empty!");
		}
		else
		{
			EnvPaths.SetMapsPath(MapsPathField.text);
		}

		EnvPaths.SetBackupPath(BackupPathField.text);

		PlayerPrefs.SetInt(UndoHistory, (int)HistorySlider.value);
		if (History) History.MaxHistoryLength = (int)HistorySlider.value;

		PlayerPrefs.SetInt("PlayMap_Faction", PlayAs.value);
		PlayerPrefs.SetInt("PlayMap_FogOfWar", FogOfWar.isOn ? 1 : 0);

		if (GetMarkers2D() != Markers2D.isOn)
		{
			if (Markers2D.isOn)
			{

			}
			else
			{
				Markers.MarkersControler.ForceResetMarkers2D();
			}
			PlayerPrefs.SetInt("Markers_2D", Markers2D.isOn ? 1 : 0);
		}

		if (GetHeightmapClamp() != HeightmapClamp.isOn)
		{

			PlayerPrefs.SetInt("Heightmap_Clamp", HeightmapClamp.isOn ? 1 : 0);
		}

		PlayerPrefs.SetFloat("UiScale", UiScale.value);

		PlayerPrefs.Save();
		gameObject.SetActive(false);

	}

	[DllImport("user32.dll")]
	private static extern void SaveFileDialog(); //in your case : OpenFileDialog

	public void BrowseGamedataPath()
	{

		var paths = StandaloneFileBrowser.OpenFolderPanel("Select Forged Alliance instalation path", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			PathField.text = paths[0];
		}
	}

	public void BrowseMapPath()
	{

		var paths = StandaloneFileBrowser.OpenFolderPanel("Select 'Maps' folder.", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			MapsPathField.text = paths[0];
		}
	}

	public void BrowseBackupPath()
	{
		var paths = StandaloneFileBrowser.OpenFolderPanel("Select backup 'Maps' folder.", EnvPaths.GetMapsPath(), false);

		if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
		{
			BackupPathField.text = paths[0];
		}
	}


	public void ResetGamedata()
	{
		EnvPaths.GenerateGamedataPath();
		PathField.text = EnvPaths.DefaultGamedataPath;

	}

	public void ResetMap()
	{
		EnvPaths.GenerateMapPath();
		MapsPathField.text = EnvPaths.DefaultMapPath;
	}

	public void ResetBackup()
	{
		BackupPathField.text = EnvPaths.DefaultMapPath.Replace("Maps/", "MapsBackup/");
	}

	public static int GetFaction()
	{
		return PlayerPrefs.GetInt("PlayMap_Faction", 0);
	}

	public static bool GetFogOfWar()
	{
		return PlayerPrefs.GetInt("PlayMap_FogOfWar", 1) == 1;
	}

	public static bool GetMarkers2D()
	{
		return PlayerPrefs.GetInt("Markers_2D", 0) == 1;
	}

	static bool _heightmapClamp;
	public static bool IsHeightmapClamp
	{
		get
		{
			return _heightmapClamp;
		}
	}

	public static bool GetHeightmapClamp()
	{
		_heightmapClamp = PlayerPrefs.GetInt("Heightmap_Clamp", 1) > 0;
		return _heightmapClamp;
	}

	public static float GetUiScale()
	{
		return PlayerPrefs.GetFloat("UiScale", 1f);
	}

	public void OnUiScaleChanged()
	{
		UiScaler.TempChangeUiScale(UiScale.value);
	}
}
