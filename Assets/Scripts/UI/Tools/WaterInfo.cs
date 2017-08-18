using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EditMap
{
	public class WaterInfo : MonoBehaviour
	{

		public TerrainInfo TerrainMenu;

		public Toggle HasWater;
		public InputField WaterElevation;
		public InputField DepthElevation;
		public InputField AbyssElevation;
		public CanvasGroup WaterSettings;

		public InputField ColorLerpXElevation;
		public InputField ColorLerpYElevation;
		public UiColor WaterColor;
		public UiColor SunColor;

		public InputField SunStrength;
		public InputField SunShininess;
		public InputField SunReflection;

		public InputField FresnelPower;
		public InputField FresnelBias;

		public InputField UnitReflection;
		public InputField SkyReflection;
		public InputField RefractionScale;

		bool Loading = false;
		private void OnEnable()
		{
			Loading = true;
			HasWater.isOn = ScmapEditor.Current.map.Water.HasWater;

			WaterElevation.text = ScmapEditor.Current.map.Water.Elevation.ToString();
			DepthElevation.text = ScmapEditor.Current.map.Water.ElevationDeep.ToString();
			AbyssElevation.text = ScmapEditor.Current.map.Water.ElevationAbyss.ToString();

			ColorLerpXElevation.text = ScmapEditor.Current.map.Water.ColorLerp.x.ToString();
			ColorLerpYElevation.text = ScmapEditor.Current.map.Water.ColorLerp.y.ToString();

			WaterColor.SetColorField(ScmapEditor.Current.map.Water.SurfaceColor.x, ScmapEditor.Current.map.Water.SurfaceColor.y, ScmapEditor.Current.map.Water.SurfaceColor.z, WaterSettingsChanged);
			SunColor.SetColorField(ScmapEditor.Current.map.Water.SunColor.x, ScmapEditor.Current.map.Water.SunColor.y, ScmapEditor.Current.map.Water.SunColor.z, WaterSettingsChanged);

			SunStrength.text = ScmapEditor.Current.map.Water.SunStrength.ToString();
			SunShininess.text = ScmapEditor.Current.map.Water.SunShininess.ToString();
			SunReflection.text = ScmapEditor.Current.map.Water.SunReflection.ToString();

			FresnelPower.text = ScmapEditor.Current.map.Water.FresnelPower.ToString();
			FresnelBias.text = ScmapEditor.Current.map.Water.FresnelBias.ToString();

			UnitReflection.text = ScmapEditor.Current.map.Water.UnitReflection.ToString();
			SkyReflection.text = ScmapEditor.Current.map.Water.SkyReflection.ToString();
			RefractionScale.text = ScmapEditor.Current.map.Water.RefractionScale.ToString();

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

			WaterElevation.text = water.ToString();
			DepthElevation.text = depth.ToString();
			AbyssElevation.text = abyss.ToString();

			ScmapEditor.Current.SetWater();

			TerrainMenu.RegenerateMaps();

			WaterSettings.interactable = HasWater.isOn;
		}

		public void WaterSettingsChanged()
		{
			if (Loading)
				return;
			ScmapEditor.Current.map.Water.ColorLerp.x = float.Parse(ColorLerpXElevation.text);
			ScmapEditor.Current.map.Water.ColorLerp.y = float.Parse(ColorLerpYElevation.text);

			ScmapEditor.Current.map.Water.SurfaceColor = WaterColor.GetVectorValue();
			ScmapEditor.Current.map.Water.SunColor = SunColor.GetVectorValue();

			ScmapEditor.Current.map.Water.SunStrength = float.Parse(SunStrength.text);
			ScmapEditor.Current.map.Water.SunShininess = float.Parse(SunShininess.text);
			ScmapEditor.Current.map.Water.SunReflection = float.Parse(SunReflection.text);

			ScmapEditor.Current.map.Water.FresnelPower = float.Parse(FresnelPower.text);
			ScmapEditor.Current.map.Water.FresnelBias = float.Parse(FresnelBias.text);

			ScmapEditor.Current.map.Water.UnitReflection = float.Parse(UnitReflection.text);
			ScmapEditor.Current.map.Water.SkyReflection = float.Parse(SkyReflection.text);
			ScmapEditor.Current.map.Water.RefractionScale = float.Parse(RefractionScale.text);

			ScmapEditor.Current.SetWater();
		}
	}
}
