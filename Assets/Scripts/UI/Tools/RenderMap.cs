using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class RenderMap : MonoBehaviour {

	public InputField Resolution;
	public Toggle RenderMarkers;
	public Toggle RenderIcons;
	public Toggle Png;
	public InputField Path;

	public GameObject Markers;
	public Canvas[] Canvases;

	public void RenderCurrentMap()
	{
		if (MapLuaParser.Current.MapLoaded())
		{
			if (Directory.Exists(Path.text))
			{
				QualitySettings.lodBias = 2000;
				QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
				QualitySettings.shadowDistance = 500;


				for (int i = 0; i < Canvases.Length; i++)
					Canvases[i].enabled = false;
				Markers.SetActive(RenderMarkers.isOn);


				int Res = int.Parse(Resolution.text);

				CameraControler.Current.RestartCam();
				string path = Path.text.Replace("\\", "/");
				if (!path.EndsWith("/"))
					path += "/";

				CameraControler.Current.RenderCamera(Res, Res, path + MapLuaParser.Current.ScenarioFileName.Replace(".lua", "") + "_preview." + ((Png.isOn)?("png"):("jpg")));

				Markers.SetActive(true);
				for (int i = 0; i < Canvases.Length; i++)
					Canvases[i].enabled = true;

				QualitySettings.lodBias = 4;
				QualitySettings.shadowResolution = ShadowResolution.Low;
				QualitySettings.shadowDistance = 23;
			}
			else
			{
				Debug.LogWarning("Path not exist: " + Path.text);
			}
		}



		gameObject.SetActive(false);
		
	}
}
