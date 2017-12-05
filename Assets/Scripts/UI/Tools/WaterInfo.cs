using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ozone.UI;

namespace EditMap
{
	public class WaterInfo : MonoBehaviour
	{

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

		public InputField SunStrength;
		public UiTextField SunShininess;
		public InputField SunReflection;

		public UiTextField FresnelPower;
		public UiTextField FresnelBias;

		public UiTextField UnitReflection;
		public UiTextField SkyReflection;
		public UiTextField RefractionScale;

		bool Loading = false;
		private void OnEnable()
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

			SunStrength.text = ScmapEditor.Current.map.Water.SunStrength.ToString();
			SunShininess.SetValue(ScmapEditor.Current.map.Water.SunShininess);
			SunReflection.text = ScmapEditor.Current.map.Water.SunReflection.ToString();

			FresnelPower.SetValue(ScmapEditor.Current.map.Water.FresnelPower);
			FresnelBias.SetValue(ScmapEditor.Current.map.Water.FresnelBias);

			UnitReflection.SetValue(ScmapEditor.Current.map.Water.UnitReflection);
			SkyReflection.SetValue(ScmapEditor.Current.map.Water.SkyReflection);
			RefractionScale.SetValue(ScmapEditor.Current.map.Water.RefractionScale);

			WaterSettings.interactable = HasWater.isOn;

			Loading = false;
		}

		public void ElevationChanged()
		{
			if (Loading)
				return;
			ScmapEditor.Current.map.Water.HasWater = HasWater.isOn;

			float water = float.Parse(WaterElevation.text);
			float depth = float.Parse(DepthElevation.text);
			float abyss = float.Parse(AbyssElevation.text);

			if (water < 1)
				water = 1;
			else if (water > 128)
				water = 128;

			if (depth > water)
				depth = water;
			else if (depth < 0)
				depth = 0;

			if (abyss > depth)
				abyss = depth;
			else if (abyss < 0)
				abyss = 0;


			ScmapEditor.Current.map.Water.Elevation = water;
			ScmapEditor.Current.map.Water.ElevationDeep = depth;
			ScmapEditor.Current.map.Water.ElevationAbyss = abyss;

			WaterElevation.SetValue(water);
			DepthElevation.SetValue(depth);
			AbyssElevation.SetValue(abyss);

			ScmapEditor.Current.SetWater();

			TerrainMenu.RegenerateMaps();

			WaterSettings.interactable = HasWater.isOn;
		}

		public void WaterSettingsChanged()
		{
			if (Loading)
				return;
			ScmapEditor.Current.map.Water.ColorLerp.x = ColorLerpXElevation.value;
			ScmapEditor.Current.map.Water.ColorLerp.y = ColorLerpXElevation.value;

			ScmapEditor.Current.map.Water.SurfaceColor = WaterColor.GetVectorValue();
			ScmapEditor.Current.map.Water.SunColor = SunColor.GetVectorValue();

			ScmapEditor.Current.map.Water.SunStrength = float.Parse(SunStrength.text);
			ScmapEditor.Current.map.Water.SunShininess = SunShininess.value;
			ScmapEditor.Current.map.Water.SunReflection = float.Parse(SunReflection.text);

			ScmapEditor.Current.map.Water.FresnelPower = FresnelPower.value;
			ScmapEditor.Current.map.Water.FresnelBias = FresnelBias.value;

			ScmapEditor.Current.map.Water.UnitReflection = UnitReflection.value;
			ScmapEditor.Current.map.Water.SkyReflection = SkyReflection.value;
			ScmapEditor.Current.map.Water.RefractionScale = RefractionScale.value;

			ScmapEditor.Current.SetWater();
		}
	}
}
