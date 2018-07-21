using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryWaterSettings : HistoryObject
	{

		public Vector2 ColorLerp;
		public Vector3 SurfaceColor;
		public Vector3 SunColor;
		public float SunStrength;
		public float SunShininess;
		public float SunReflection;
		public float FresnelPower;
		public float FresnelBias;
		public float UnitReflection;
		public float SkyReflection;
		public float RefractionScale;


		public override void Register(HistoryParameter Param)
		{
			ColorLerp = ScmapEditor.Current.map.Water.ColorLerp;
			SurfaceColor = ScmapEditor.Current.map.Water.SurfaceColor;
			SunColor = ScmapEditor.Current.map.Water.SunColor;

			SunStrength = ScmapEditor.Current.map.Water.SunStrength;
			SunShininess = ScmapEditor.Current.map.Water.SunShininess;
			SunReflection = ScmapEditor.Current.map.Water.SunReflection;

			FresnelPower = ScmapEditor.Current.map.Water.FresnelPower;
			FresnelBias = ScmapEditor.Current.map.Water.FresnelBias;

			UnitReflection = ScmapEditor.Current.map.Water.UnitReflection;
			SkyReflection = ScmapEditor.Current.map.Water.SkyReflection;
			RefractionScale = ScmapEditor.Current.map.Water.RefractionScale;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
			{
				Undo.RegisterRedo(new HistoryWaterSettings());
			}
			RedoGenerated = true;
			DoRedo();

		}

		public override void DoRedo()
		{
			if (Undo.Current.EditMenu.State != Editing.EditStates.TerrainStat)
			{
				Undo.Current.EditMenu.State = Editing.EditStates.TerrainStat;
				Undo.Current.EditMenu.ChangeCategory(1);
			}

			Undo.Current.EditMenu.EditTerrain.ChangePage(1);

			ScmapEditor.Current.map.Water.ColorLerp = ColorLerp;
			ScmapEditor.Current.map.Water.SurfaceColor = SurfaceColor;
			ScmapEditor.Current.map.Water.SunColor = SunColor;

			ScmapEditor.Current.map.Water.SunStrength = SunStrength;
			ScmapEditor.Current.map.Water.SunShininess = SunShininess;
			ScmapEditor.Current.map.Water.SunReflection = SunReflection;

			ScmapEditor.Current.map.Water.FresnelPower = FresnelPower;
			ScmapEditor.Current.map.Water.FresnelBias = FresnelBias;

			ScmapEditor.Current.map.Water.UnitReflection = UnitReflection;
			ScmapEditor.Current.map.Water.SkyReflection = SkyReflection;
			ScmapEditor.Current.map.Water.RefractionScale = RefractionScale;

			WaterInfo.Current.ReloadValues(true);
		}
	}
}