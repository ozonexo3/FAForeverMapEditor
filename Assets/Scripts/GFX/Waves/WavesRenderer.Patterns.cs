using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;
using System.IO;

namespace EditMap
{
	public partial class WavesRenderer : MonoBehaviour
	{

		public WavePattern[] WavePatterns = new WavePattern[0];

		[System.Serializable]
		public class WavePattern
		{
			public string name;
			public string preview;
			public Parameters[] parameters;

			[System.Serializable]
			public class Parameters
			{
				public string texture;
				public string ramp;
				public Vector3 position;
				public float period;
				public float periodVariance;
				public float speed;
				public float speedVariance;
				public float lifetime;
				public float lifetimeVariance;
				public Vector2 scale;
				public float scaleVariance;
				public Vector3 velocityDelta;
				public int frameCount;
				public float frameRate;
				public float frameRateVariance;
				public int stripCount;
			}
		}

		const string KEY_patterns = "patterns";

		void LoadWavePatterns()
		{
			Lua LuaFile;
			System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

			string path = MapLuaParser.GetDataPath() + "/Structure/Waves/wavepatterns.lua";

			if (!Directory.Exists(Path.GetDirectoryName(path)))
			{
				Debug.LogError("Cant find Waves folder");
				return;
			}
			if (!File.Exists(path))
			{
				Debug.LogError("Cant find Waves file");
				return;
			}

			string loadedFile = File.ReadAllText(path, encodeType);

			LuaFile = new Lua();
			LuaFile.LoadCLRPackage();
			try
			{
				LuaFile.DoString(MapLuaParser.Current.SaveLuaHeader.text + loadedFile);
			}
			catch (NLua.Exceptions.LuaException e)
			{
				Debug.LogError(LuaParser.Read.FormatException(e), MapLuaParser.Current.gameObject);
				return;
			}

			var patternTable = LuaFile.GetTable(KEY_patterns);
			LuaTable[] wvTabs = LuaParser.Read.GetTableTables(patternTable);

			WavePatterns = new WavePattern[wvTabs.Length];

			for (int i = 0; i < WavePatterns.Length; i++)
			{
				WavePattern pattern = new WavePattern();

				pattern.name = LuaParser.Read.StringFromTable(wvTabs[i], "name", "pattern" + i);
				pattern.preview = LuaParser.Read.StringFromTable(wvTabs[i], "preview", "/editor/tools/water/nopreview.bmp");

				LuaTable parametersTable = (LuaTable)wvTabs[i].RawGet("parameters");

				LuaTable[] parameters = LuaParser.Read.GetTableTables(parametersTable);
				pattern.parameters = new WavePattern.Parameters[parameters.Length];
				for(int p = 0; p < parameters.Length; p++)
				{
					pattern.parameters[p] = new WavePattern.Parameters();
					pattern.parameters[p].texture = LuaParser.Read.StringFromTable(parameters[p], "texture", "/env/common/decals/shoreline/wavetest.dds");
					pattern.parameters[p].ramp = LuaParser.Read.StringFromTable(parameters[p], "ramp", "/env/common/decals/shoreline/waveramptest.dds");
					pattern.parameters[p].position = LuaParser.Read.Vector3FromTable(parameters[p], "position");
					pattern.parameters[p].period = LuaParser.Read.FloatFromTable(parameters[p], "period");
					pattern.parameters[p].periodVariance = LuaParser.Read.FloatFromTable(parameters[p], "periodVariance");
					pattern.parameters[p].speed = LuaParser.Read.FloatFromTable(parameters[p], "speed");
					pattern.parameters[p].speedVariance = LuaParser.Read.FloatFromTable(parameters[p], "speedVariance");
					pattern.parameters[p].lifetime = LuaParser.Read.FloatFromTable(parameters[p], "lifetime");
					pattern.parameters[p].lifetimeVariance = LuaParser.Read.FloatFromTable(parameters[p], "lifetimeVariance");
					pattern.parameters[p].scale = LuaParser.Read.Vector2FromTable(parameters[p], "scale");
					pattern.parameters[p].scaleVariance = LuaParser.Read.FloatFromTable(parameters[p], "scaleVariance");
					pattern.parameters[p].velocityDelta = LuaParser.Read.Vector3FromTable(parameters[p], "velocityDelta");
					pattern.parameters[p].frameCount = LuaParser.Read.IntFromTable(parameters[p], "frameCount");
					pattern.parameters[p].frameRate = LuaParser.Read.FloatFromTable(parameters[p], "frameRate");
					pattern.parameters[p].frameRateVariance = LuaParser.Read.FloatFromTable(parameters[p], "frameRateVariance");
					pattern.parameters[p].stripCount = LuaParser.Read.IntFromTable(parameters[p], "stripCount");
				}

				WavePatterns[i] = pattern;
			}


		}

	}
}
