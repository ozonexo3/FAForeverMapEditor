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

			string data = JsonUtility.ToJson(Wave);

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

		}
		[System.Serializable]
		public class WaveData
		{
			public WaveGenerator[] AllWaves;
		}
	}
}
