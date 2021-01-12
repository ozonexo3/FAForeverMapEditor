using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ozone.UI;
using SFB;
using System.IO;

namespace EditMap
{
	public partial class WaterInfo : MonoBehaviour
	{

		[Header("Waves")]
		public Toggle DrawShorelineToggle;
		public UiTextField ShoreDepth;
		public UiTextField MinWaveAngle;
		public UiTextField MaxWaveAngle;
		public Dropdown WavesDropdown;

		bool waveUiLoaded = false;
		public void LoadWavesUI()
		{
			if (waveUiLoaded)
				return;
			List<Dropdown.OptionData> waveValues = new List<Dropdown.OptionData>();

			for(int i = 0; i < WavesRenderer.Instance.WavePatterns.Length; i++)
			{
				waveValues.Add(new Dropdown.OptionData(WavesRenderer.Instance.WavePatterns[i].name));
			}

			WavesDropdown.ClearOptions();
			WavesDropdown.options = waveValues;
			waveUiLoaded = true;
		}

		const string format = "scmwaves";

		public void ExportWaves()
		{
			var extensions = new[]
{
				new ExtensionFilter("Waves settings", format)
			};

			var path = StandaloneFileBrowser.SaveFilePanel("Export waves", EnvPaths.GetMapsPath(), "", extensions);

			if (string.IsNullOrEmpty(path))
				return;


			Debug.Log(ScmapEditor.Current.map.WaveGenerators.Count);

			WaveData Wave = new WaveData();
			Wave.AllWaves = new WaveGenerator[ScmapEditor.Current.map.WaveGenerators.Count];
			ScmapEditor.Current.map.WaveGenerators.CopyTo(Wave.AllWaves);

			string data = JsonUtility.ToJson(Wave, true);

			data = data.Replace(",", ",\n");
			data = data.Replace("{", "{\n");
			data = data.Replace("}", "\n}");

			File.WriteAllText(path, data);
		}

		public void ImportWaves()
		{

			var extensions = new[]
{
				new ExtensionFilter("Waves settings", format)
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import waves", EnvPaths.GetMapsPath(), extensions, false);


			if (paths.Length <= 0 || string.IsNullOrEmpty(paths[0]))
				return;


			string data = File.ReadAllText(paths[0]);

			ScmapEditor.Current.map.WaveGenerators = JsonUtility.FromJson<List<WaveGenerator>>(data);
			WavesRenderer.ReloadWaves();
		}

		public void ClearWaves()
		{
			Debug.Log("Clear waves count: " + ScmapEditor.Current.map.WaveGenerators.Count);
			ScmapEditor.Current.map.WaveGenerators.Clear();
			WavesRenderer.ReloadWaves();
		}

		public void GenerateWaves()
		{
			WavesRenderer.WavePattern pattern = WavesRenderer.Instance.WavePatterns[WavesDropdown.value];

			Debug.Log("Generate waves: " + pattern.name);

			Vector2 angleRange = Vector2.zero;
			angleRange.x = MinWaveAngle.value;

			while (angleRange.x < 0f)
				angleRange.x += 360f;
			while (angleRange.x > 360f)
				angleRange.x -= 360f;

			MinWaveAngle.SetValue(angleRange.x);

			angleRange.y = Mathf.Clamp(MaxWaveAngle.value, 0f, 180f);
			MaxWaveAngle.SetValue(angleRange.y);

			var waveShorePoints = WavesRenderer.GetShoreDepthPoints(ShoreDepth.value * 0.1f, angleRange);

			Debug.Log(waveShorePoints.Count);

			for(int i = 0; i < waveShorePoints.Count; i++)
			{
				Vector3 forward = Quaternion.Euler(0f, waveShorePoints[i].angle, 0f) * Vector3.forward;
				Vector3 right = Vector3.Cross(forward, Vector3.up);
				Debug.DrawLine(waveShorePoints[i].point, waveShorePoints[i].point + forward * 0.2f, Color.cyan, 10f);
				Debug.DrawLine(waveShorePoints[i].point - right * 0.05f, waveShorePoints[i].point + right * 0.05f, Color.cyan, 10f);

			}

			WavesRenderer.ReloadWaves();
		}

		public void ToggleDrawShoreline()
		{
			WavesRenderer.Instance.DrawShoreLine = DrawShorelineToggle.isOn;
		}

		[System.Serializable]
		public class WaveData
		{
			public WaveGenerator[] AllWaves;
		}
	}
}
