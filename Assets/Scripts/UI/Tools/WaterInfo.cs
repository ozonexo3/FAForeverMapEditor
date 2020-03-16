using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ozone.UI;
using System.IO;
using System.Runtime.InteropServices;
using SFB;

namespace EditMap
{
	public partial class WaterInfo : MonoBehaviour
	{

		public static WaterInfo Current;

		public TerrainInfo TerrainMenu;

		public Toggle HasWater;
		public CanvasGroup WaterSettings;

		public UiTextField WaterElevation;
		public UiTextField DepthElevation;
		public UiTextField AbyssElevation;

		public UiTextField ColorLerpXElevation;
		public UiTextField ColorLerpYElevation;
		public UiColor WaterColor;
		public UiColor SunColor;

		public UiTextField SunStrength;
		public UiTextField SunShininess;
		public UiTextField SunReflection;

		public UiTextField FresnelPower;
		public UiTextField FresnelBias;

		public UiTextField UnitReflection;
		public UiTextField SkyReflection;
		public UiTextField RefractionScale;

		bool Loading = false;
		private void OnEnable()
		{
			Current = this;
			ReloadValues();
		}

		public void ReloadValues(bool Undo = false)
		{
			Loading = true;
			HasWater.isOn = ScmapEditor.Current.map.Water.HasWater;

			WaterElevation.SetValue(ScmapEditor.Current.map.Water.Elevation);
			DepthElevation.SetValue(ScmapEditor.Current.map.Water.ElevationDeep);
			AbyssElevation.SetValue(ScmapEditor.Current.map.Water.ElevationAbyss);

			ColorLerpXElevation.SetValue(ScmapEditor.Current.map.Water.ColorLerp.x);
			ColorLerpYElevation.SetValue(ScmapEditor.Current.map.Water.ColorLerp.y);

			WaterColor.SetColorField(ScmapEditor.Current.map.Water.SurfaceColor.x, ScmapEditor.Current.map.Water.SurfaceColor.y, ScmapEditor.Current.map.Water.SurfaceColor.z); // WaterSettingsChanged
			SunColor.SetColorField(ScmapEditor.Current.map.Water.SunColor.x, ScmapEditor.Current.map.Water.SunColor.y, ScmapEditor.Current.map.Water.SunColor.z); // WaterSettingsChanged

			SunStrength.SetValue(ScmapEditor.Current.map.Water.SunStrength);
			SunShininess.SetValue(ScmapEditor.Current.map.Water.SunShininess);
			SunReflection.SetValue(ScmapEditor.Current.map.Water.SunReflection);

			FresnelPower.SetValue(ScmapEditor.Current.map.Water.FresnelPower);
			FresnelBias.SetValue(ScmapEditor.Current.map.Water.FresnelBias);

			UnitReflection.SetValue(ScmapEditor.Current.map.Water.UnitReflection);
			SkyReflection.SetValue(ScmapEditor.Current.map.Water.SkyReflection);
			RefractionScale.SetValue(ScmapEditor.Current.map.Water.RefractionScale);

			WaterSettings.interactable = HasWater.isOn;

			Loading = false;

			if (Undo)
			{
				UpdateScmap(true);
			}
		}

		void UpdateScmap(bool Maps)
		{
			ScmapEditor.Current.SetWater();

			if (Maps)
			{
				GenerateControlTex.StopAllTasks();
				TerrainMenu.RegenerateMaps();
			}
		}

		bool UndoRegistered = false;
		public void ElevationChangeBegin()
		{
			if (Loading || UndoRegistered)
				return;
			//Undo.Current.RegisterWaterElevationChange();
			//UndoRegistered = true;
			//Debug.Log("Begin");
		}

		public void ElevationChanged()
		{
			if (Loading)
				return;



			float water = WaterElevation.value;
			float depth = DepthElevation.value;
			float abyss = AbyssElevation.value;

			if (water < 1)
				water = 1;
			else if (water > 256)
				water = 256;

			if (depth > water)
				depth = water;
			else if (depth < 0)
				depth = 0;

			if (abyss > depth)
				abyss = depth;
			else if (abyss < 0)
				abyss = 0;

			bool AnyChanged = ScmapEditor.Current.map.Water.HasWater != HasWater.isOn
				|| ScmapEditor.Current.map.Water.Elevation != water
				|| ScmapEditor.Current.map.Water.ElevationDeep != depth
				|| ScmapEditor.Current.map.Water.ElevationAbyss != abyss
				;

			if (!AnyChanged)
			{
				return;
			}

			Undo.RegisterUndo(new UndoHistory.HistoryWaterElevation());
			if (!UndoRegistered)
				ElevationChangeBegin();
			UndoRegistered = false;

			ScmapEditor.Current.map.Water.HasWater = HasWater.isOn;
			ScmapEditor.Current.map.Water.Elevation = water;
			ScmapEditor.Current.map.Water.ElevationDeep = depth;
			ScmapEditor.Current.map.Water.ElevationAbyss = abyss;

			Loading = true;
			WaterElevation.SetValue(water);
			DepthElevation.SetValue(depth);
			AbyssElevation.SetValue(abyss);
			WaterSettings.interactable = HasWater.isOn;
			Loading = false;

			UpdateScmap(true);

		}

		public void WaterSettingsChangedBegin()
		{
			if (Loading || UndoRegistered)
				return;
			//Undo.Current.RegisterWaterSettingsChange();
			//UndoRegistered = true;
		}

		public void WaterSettingsChanged(bool Slider)
		{
			if (Loading)
				return;


			bool AnyChanged = ScmapEditor.Current.map.Water.ColorLerp.x != ColorLerpXElevation.value
				|| ScmapEditor.Current.map.Water.ColorLerp.y != ColorLerpYElevation.value
				|| ScmapEditor.Current.map.Water.SurfaceColor != WaterColor.GetVectorValue()
				|| ScmapEditor.Current.map.Water.SunColor != SunColor.GetVectorValue()
				|| ScmapEditor.Current.map.Water.SunStrength != SunStrength.value
				|| ScmapEditor.Current.map.Water.SunShininess != SunShininess.value
				|| ScmapEditor.Current.map.Water.SunReflection != SunReflection.value
				|| ScmapEditor.Current.map.Water.FresnelPower != FresnelPower.value
				|| ScmapEditor.Current.map.Water.FresnelBias != FresnelBias.value
				|| ScmapEditor.Current.map.Water.UnitReflection != UnitReflection.value
				|| ScmapEditor.Current.map.Water.SkyReflection != SkyReflection.value
				|| ScmapEditor.Current.map.Water.RefractionScale != RefractionScale.value
				;

			if (!AnyChanged)
				return;


			if (!UndoRegistered)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryWaterSettings());
				UndoRegistered = true;
				WaterSettingsChangedBegin();

			}

			if(!Slider)
				UndoRegistered = false;




			ScmapEditor.Current.map.Water.ColorLerp.x = ColorLerpXElevation.value;
			ScmapEditor.Current.map.Water.ColorLerp.y = ColorLerpYElevation.value;

			ScmapEditor.Current.map.Water.SurfaceColor = WaterColor.GetVectorValue();
			ScmapEditor.Current.map.Water.SunColor = SunColor.GetVectorValue();

			ScmapEditor.Current.map.Water.SunStrength = SunStrength.value;
			ScmapEditor.Current.map.Water.SunShininess = SunShininess.value;
			ScmapEditor.Current.map.Water.SunReflection = SunReflection.value;

			ScmapEditor.Current.map.Water.FresnelPower = FresnelPower.value;
			ScmapEditor.Current.map.Water.FresnelBias = FresnelBias.value;

			ScmapEditor.Current.map.Water.UnitReflection = UnitReflection.value;
			ScmapEditor.Current.map.Water.SkyReflection = SkyReflection.value;
			ScmapEditor.Current.map.Water.RefractionScale = RefractionScale.value;

			UpdateScmap(false);
		}

		#region Import/Export
		const string ExportPathKey = "WaterExport";
		static string DefaultPath
		{
			get
			{
				return EnvPaths.GetLastPath(ExportPathKey, EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName);
			}
		}
		public void ExportWater()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Water settings", "scmwtr")
			};

			var path = StandaloneFileBrowser.SaveFilePanel("Export water", DefaultPath, "WaterSettings", extensions);

			if (string.IsNullOrEmpty(path))
				return;


			string data = JsonUtility.ToJson(ScmapEditor.Current.map.Water);

			File.WriteAllText(path, data);
			EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(path));
		}

		public void ImportWater()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Water settings", "scmwtr")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import water", DefaultPath, extensions, false);


			if (paths.Length <= 0 || string.IsNullOrEmpty(paths[0]))
				return;


			string data = File.ReadAllText(paths[0]);

			float WaterLevel = ScmapEditor.Current.map.Water.Elevation;
			float AbyssLevel = ScmapEditor.Current.map.Water.ElevationAbyss;
			float DeepLevel = ScmapEditor.Current.map.Water.ElevationDeep;

			ScmapEditor.Current.map.Water = JsonUtility.FromJson<WaterShader>(data);

			ScmapEditor.Current.map.Water.Elevation = WaterLevel;
			ScmapEditor.Current.map.Water.ElevationAbyss = AbyssLevel;
			ScmapEditor.Current.map.Water.ElevationDeep = DeepLevel;

			ReloadValues();

			UpdateScmap(true);
			EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths[0]));

		}
		#endregion


		public void ClearWaves()
		{
			Debug.Log("Clear waves count: " + ScmapEditor.Current.map.WaveGenerators.Count);
			ScmapEditor.Current.map.WaveGenerators.Clear();

		}
	}
}
