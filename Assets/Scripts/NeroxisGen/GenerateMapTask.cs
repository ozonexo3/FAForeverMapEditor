using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

namespace neroxis
{
	public class MapGeneratorSettings
	{
		public string mapName = "";
		public string folderPath = "";
		public int width = -1;
		public long seed = -1;
		public int spawnCount = -1;
		public int teams = -1;
		public string biome = "";
		public string symmetry = "";
		public int mexCount = -1;
		public float mexDensity = -1;
		public float reclaimDensity = -1;
	}

	public class GenerateMapTask : MonoBehaviour
	{

		#region Public values
		public static string[] GetAllBiomes()
		{
			string[] allBiomes = new string[biomes.Biomes.BIOMES_LIST.size()];

			for (int i = 0; i < allBiomes.Length; i++)
			{
				allBiomes[i] = biomes.Biomes.BIOMES_LIST.get(i).ToString();
			}

			return allBiomes;
		}

		public static float[,] GetGeneratedHeightmap()
		{
			if (generatedMap == null)
				return null;

			var heightmap = generatedMap.getHeightmap().getData();

			int width = heightmap.getWidth();
			int height = heightmap.getHeight();

			float[,] data = new float[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					data[x, y] = heightmap.getSampleFloat(x, y, 0) / (65536f);
				}
			}

			return data;
		}
		#endregion

		#region Private values

		private static map.SCMap generatedMap;
		private static System.Action OnFinish;

		#endregion


		#region Public Functions

		private static MapGeneratorSettings generatorSettings;

		private static string[] ComputeArguments()
		{
			List<string> arguments = new List<string>();

			arguments.Add("--no-hash");
			//arguments.Add("--debug");

			if(generatorSettings != null)
			{
				if (!string.IsNullOrEmpty(generatorSettings.mapName))
				{
					arguments.Add("--map-name " + generatorSettings.mapName.Replace(" ", "_"));
				}
				if (!string.IsNullOrEmpty(generatorSettings.folderPath))
				{
					arguments.Add("--folder-path " + generatorSettings.folderPath);
				}
				if (!string.IsNullOrEmpty(generatorSettings.biome))
				{
					arguments.Add("--biome " + generatorSettings.biome);
				}
				if (!string.IsNullOrEmpty(generatorSettings.symmetry))
				{
					arguments.Add("--symmetry " + generatorSettings.symmetry);
				}

				if (generatorSettings.width >= 0)
				{
					arguments.Add("--map-size " + generatorSettings.width);
				}
				if (generatorSettings.seed >= 0)
				{
					arguments.Add("--seed " + generatorSettings.seed);
				}
				if (generatorSettings.spawnCount >= 0)
				{
					arguments.Add("--spawn-count " + generatorSettings.spawnCount);
				}
				if (generatorSettings.teams >= 0)
				{
					arguments.Add("--num-teams " + generatorSettings.teams);
				}

				if (generatorSettings.mexCount >= 0)
				{
					arguments.Add("--mex-count " + generatorSettings.mexCount);
				}
				if (generatorSettings.mexDensity >= 0)
				{
					arguments.Add("--mex-density " + generatorSettings.mexDensity);
				}
				if (generatorSettings.reclaimDensity >= 0)
				{
					arguments.Add("--reclaim-density " + generatorSettings.reclaimDensity);
				}
			}

			string debugString = "";
			for(int i = 0; i < arguments.Count; i++)
			{
				debugString += "\n" + arguments[i];
			}
			Debug.Log("Arguments:" + debugString);

			return arguments.ToArray();
		}

		public static void GenerateMap(string path, int size, int biome)
		{

		}

		public static void GenerateSCMP(MapGeneratorSettings settings, System.Action FinishAction)
		{
			if (threadSCMP != null)
			{
				Debug.LogError("Previous thread is still working!");
				return;
			}

			OnFinish = FinishAction;
			generatorSettings = settings;

			threadListener = instance.StartCoroutine(instance.StartThreadSCMP());
		}
		#endregion

		public static GenerateMapTask instance { get; private set; }
		public GameObject window;
		public Text progressText;

		private void Awake()
		{
			instance = this;
		}

		#region Thread
		static void StartThread()
		{

		}

		IEnumerator StartThreadSCMP()
		{
			threadSCMP = new Thread(ThreadSCMP) { Name = "Thread SCMP" };
			threadSCMP.Priority = System.Threading.ThreadPriority.Highest;

			instance.window.SetActive(true);
			instance.progressText.text = "Generating Neroxis SCMP data...\nThis can take ~1-2min";

			ErrorCode = "";
			generatedMap = null;
			ts = System.TimeSpan.FromTicks(System.DateTime.Now.Ticks);
			ThreadStartTime = ts.TotalSeconds;

			threadSCMP.Start();

			int lastSec = 0;

			while (threadSCMP.IsAlive)
			{
				yield return null;

				int currentSec = Mathf.FloorToInt((float)(System.TimeSpan.FromTicks(System.DateTime.Now.Ticks).TotalSeconds - ThreadStartTime));

				if(currentSec > lastSec)
				{
					instance.progressText.text = "Generating Neroxis SCMP data... (" + currentSec + "s)\nThis can take ~1-2min";
				}

				if (!string.IsNullOrEmpty(ErrorCode))
				{
					Debug.LogError(ErrorCode);
					break;
				}
			}

			instance.window.SetActive(false);
			threadSCMP = null;
			OnFinish?.Invoke();
		}

		static Coroutine threadListener;
		static Thread threadSCMP;
		static System.TimeSpan ts;
		static double ThreadStartTime = 0;
		static string ErrorCode = "";

		public static void ThreadSCMP()
		{
			try
			{
				generator.MapGenerator generator = new generator.MapGenerator();
				generator.interpretArguments(ComputeArguments());
				generatedMap = generator.generate();

				//Finish
				Debug.Log(generatedMap.getHeightmap().getWidth() + " x " + generatedMap.getHeightmap().getHeight());
			}
			catch (System.Exception e)
			{
				ErrorCode = e.ToString();
			}

			ts = System.TimeSpan.FromTicks(System.DateTime.Now.Ticks);
			float CurrentTime = (float)(ts.TotalSeconds - ThreadStartTime);
			Debug.Log("Thread finished in time: " + CurrentTime + "s");
		}
		#endregion
	}
}