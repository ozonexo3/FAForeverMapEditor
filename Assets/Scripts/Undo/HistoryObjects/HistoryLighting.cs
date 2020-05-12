using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryLighting : HistoryObject
	{

		public float RA;
		public float DA;
		public float SunMultipiler;
		public Vector3 SunColor = Vector3.zero;
		public Vector3 AmbientColor = Vector3.zero;
		public Vector3 ShadowColor = Vector3.zero;
		public Vector4 Specular = Vector4.zero;
		public Vector4 Fog = Vector4.zero;
		public float FogStart;
		public float FogEnd;
		public float Bloom;

		public override void Register(HistoryParameter Param)
		{
			LightingInfo LightMenu = Undo.Current.EditMenu.Categorys[3].GetComponent<LightingInfo>();

			RA = LightMenu.RA.value;
			DA = LightMenu.DA.value;
			SunMultipiler = LightMenu.LightMultipiler.value;

			SunColor = LightMenu.LightColor.GetVectorValue();
			AmbientColor = LightMenu.AmbienceColor.GetVectorValue();
			ShadowColor = LightMenu.ShadowColor.GetVectorValue();
			Specular = LightMenu.Specular.GetVector4Value();
			Fog = LightMenu.FogColor.GetVector4Value();
			FogStart = LightMenu.FogStart.value;
			FogEnd = LightMenu.FogEnd.value;
			Bloom = LightMenu.Bloom.value;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryLighting());
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			if (Undo.Current.EditMenu.State != Editing.EditStates.LightingStat) Undo.Current.EditMenu.ButtonFunction("Lighting");

			LightingInfo LightMenu = Undo.Current.EditMenu.Categorys[3].GetComponent<LightingInfo>();

			LightMenu.IgnoreUpdate = true;

			LightMenu.RA.SetValue(RA);
			LightMenu.DA.SetValue(DA);
			LightMenu.LightMultipiler.SetValue(SunMultipiler);

			LightMenu.LightColor.SetColorField(SunColor.x, SunColor.y, SunColor.z);
			LightMenu.AmbienceColor.SetColorField(AmbientColor.x, AmbientColor.y, AmbientColor.z);
			LightMenu.ShadowColor.SetColorField(ShadowColor.x, ShadowColor.y, ShadowColor.z);
			LightMenu.Specular.SetColorField(Specular.x, Specular.y, Specular.z, Specular.w);

			LightMenu.FogColor.SetColorField(Fog.x, Fog.y, Fog.z, Fog.w);
			LightMenu.FogStart.SetValue(FogStart);
			LightMenu.FogEnd.SetValue(FogEnd);
			LightMenu.Bloom.SetValue(Bloom);

			LightMenu.IgnoreUpdate = false;

			LightMenu.UndoUpdate();
		}
	}
}