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
		public UiTextField ShoreDensity;
		public Toggle PreventCloseWaves;
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

			int patternPropertyId = Shader.PropertyToID(pattern.parameters[0].texture + pattern.parameters[0].ramp);

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

			List<WavesRenderer.ShoreDepthPoint> waveShoreUsedPoints;
			int spawnedCount = 0;

			float density = ShoreDensity.value * 0.01f;
			if (density < 1)
			{

				int targetSpawnCount = Mathf.RoundToInt(waveShorePoints.Count * density);
				HashSet<Vector3> usedPoints = new HashSet<Vector3>();
				waveShoreUsedPoints = new List<WavesRenderer.ShoreDepthPoint>(targetSpawnCount);

				int MaxSearchStepDensity = Mathf.RoundToInt(Mathf.Lerp(600, 10, density + Mathf.Clamp01((waveShorePoints.Count - 500) / 2000f)));

				if (!PreventCloseWaves.isOn)
					MaxSearchStepDensity = 0;

				while (spawnedCount < targetSpawnCount)
				{
					int MaxStepCount = MaxSearchStepDensity;
					while (true)
					{
						int newId = Random.Range(0, waveShorePoints.Count);
						Vector3 searchPoint = waveShorePoints[newId].point;
						bool toCloseToUsedPoint = false;
						foreach (Vector3 point in usedPoints)
						{
							if ((point - searchPoint).sqrMagnitude <= 0.1f)
							{
								toCloseToUsedPoint = true;
								break;
							}
						}

						MaxStepCount--;


						if (!toCloseToUsedPoint || MaxStepCount < 0)
						{
							usedPoints.Add(searchPoint);
							waveShoreUsedPoints.Add(waveShorePoints[newId]);
							waveShorePoints.RemoveAt(newId);
							break;
						}

					}

					spawnedCount++;
				}

			}
			else
			{
				waveShoreUsedPoints = waveShorePoints;
				spawnedCount = waveShorePoints.Count;
			}


			Debug.Log("Spawned waves count: " + spawnedCount);


			for (int i = 0; i < waveShoreUsedPoints.Count; i++)
			{
				Vector3 forward = Quaternion.Euler(0f, waveShoreUsedPoints[i].angle, 0f) * Vector3.forward;
#if UNITY_EDITOR
				Vector3 right = Vector3.Cross(forward, Vector3.up);
				Debug.DrawLine(waveShoreUsedPoints[i].point, waveShoreUsedPoints[i].point + forward * 0.2f, Color.cyan, 10f);
				Debug.DrawLine(waveShoreUsedPoints[i].point - right * 0.05f, waveShoreUsedPoints[i].point + right * 0.05f, Color.cyan, 10f);
#endif

				WaveGenerator newWave = new WaveGenerator();
				newWave.Position = ScmapEditor.WorldPosToScmap(waveShoreUsedPoints[i].point);
				newWave.Rotation = waveShoreUsedPoints[i].angle * Mathf.Deg2Rad;

				newWave.TextureName = pattern.parameters[0].texture;
				newWave.RampName = pattern.parameters[0].ramp;
				newWave.propertyID = patternPropertyId;

				newWave.PeriodFirst = pattern.parameters[0].period - pattern.parameters[0].periodVariance;
				newWave.PeriodSecond = pattern.parameters[0].period + pattern.parameters[0].periodVariance;
				
				newWave.LifetimeFirst = pattern.parameters[0].lifetime - pattern.parameters[0].lifetimeVariance;
				newWave.LifetimeSecond = pattern.parameters[0].lifetime + pattern.parameters[0].lifetimeVariance;

				newWave.ScaleFirst = pattern.parameters[0].scale.x + Random.Range(-pattern.parameters[0].scaleVariance, pattern.parameters[0].scaleVariance);
				newWave.ScaleSecond = pattern.parameters[0].scale.y + Random.Range(-pattern.parameters[0].scaleVariance, pattern.parameters[0].scaleVariance);

				newWave.Velocity = -forward * (pattern.parameters[0].speed + Random.Range(-pattern.parameters[0].speedVariance, pattern.parameters[0].speedVariance)); // ?? TODO Check if it really works

				newWave.Velocity = new Vector3(newWave.Velocity.x * -1f, newWave.Velocity.y * 1, newWave.Velocity.z * 1);

				newWave.FrameCount = pattern.parameters[0].frameCount;
				newWave.FrameRateFirst = pattern.parameters[0].frameRate - pattern.parameters[0].frameRateVariance;
				newWave.FrameRateSecond = pattern.parameters[0].frameRate + pattern.parameters[0].frameRateVariance;
				newWave.StripCount = pattern.parameters[0].stripCount;

				ScmapEditor.Current.map.WaveGenerators.Add(newWave);
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
