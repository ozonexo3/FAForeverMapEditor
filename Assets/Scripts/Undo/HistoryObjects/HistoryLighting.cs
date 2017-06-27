using UnityEngine;
using System.Collections;
using UndoHistory;
using EditMap;

public class HistoryLighting : HistoryObject {

	public float RA;
	public float DA;
	public float SunMultipiler;
	public Vector3 SunColor = Vector3.zero;
	public Vector3 AmbientColor = Vector3.zero;
	public Vector3 ShadowColor = Vector3.zero;

	public override void Register(){
		LightingInfo LightMenu = Undo.Current.EditMenu.Categorys[3].GetComponent<LightingInfo>();

		if (Undo.Slider)
		{
			RA = Mathf.Clamp(float.Parse(LightMenu.RA.text), -180, 180);
			DA = Mathf.Clamp(float.Parse(LightMenu.DA.text), 0, 90);
			SunMultipiler = Mathf.Clamp(float.Parse(LightMenu.LightMultipiler.text), 0, 2);


			SunColor.x = Mathf.Clamp(float.Parse(LightMenu.LightColorR.text), 0, 2);
			SunColor.y = Mathf.Clamp(float.Parse(LightMenu.LightColorG.text), 0, 2);
			SunColor.z = Mathf.Clamp(float.Parse(LightMenu.LightColorB.text), 0, 2);

			AmbientColor.x = Mathf.Clamp(float.Parse(LightMenu.AmbienceColorR.text), 0, 2);
			AmbientColor.y = Mathf.Clamp(float.Parse(LightMenu.AmbienceColorG.text), 0, 2);
			AmbientColor.z = Mathf.Clamp(float.Parse(LightMenu.AmbienceColorB.text), 0, 2);

			ShadowColor.x = Mathf.Clamp(float.Parse(LightMenu.ShadowColorR.text), 0, 2);
			ShadowColor.y = Mathf.Clamp(float.Parse(LightMenu.ShadowColorG.text), 0, 2);
			ShadowColor.z = Mathf.Clamp(float.Parse(LightMenu.ShadowColorB.text), 0, 2);

		}
		else
		{
			RA = LightMenu.RA_Slider.value;
			DA = LightMenu.DA_Slider.value;
			SunMultipiler = LightMenu.LightMultipilerSlider.value;

			SunColor.x = LightMenu.LightColorR_Slider.value;
			SunColor.y = LightMenu.LightColorG_Slider.value;
			SunColor.z = LightMenu.LightColorB_Slider.value;

			AmbientColor.x = LightMenu.AmbienceColorR_Slider.value;
			AmbientColor.y = LightMenu.AmbienceColorG_Slider.value;
			AmbientColor.z = LightMenu.AmbienceColorB_Slider.value;

			ShadowColor.x = LightMenu.ShadowColorR_Slider.value;
			ShadowColor.y = LightMenu.ShadowColorG_Slider.value;
			ShadowColor.z = LightMenu.ShadowColorB_Slider.value;
		}

	}


	public override void DoUndo(){
		HistoryLighting.GenerateRedo (Undo.Current.Prefabs.LightingChange).Register();
		DoRedo ();
	}

	public override void DoRedo(){
		if(Undo.Current.EditMenu.State != Editing.EditStates.LightingStat) Undo.Current.EditMenu.ButtonFunction("Lighting");

		LightingInfo LightMenu = Undo.Current.EditMenu.Categorys[3].GetComponent<LightingInfo>();

		LightMenu.IgnoreUpdate = true;

		LightMenu.RA_Slider.value = RA;
		LightMenu.DA_Slider.value = DA;
		LightMenu.LightMultipilerSlider.value = SunMultipiler;

		LightMenu.LightColorR_Slider.value = SunColor.x;
		LightMenu.LightColorG_Slider.value = SunColor.y;
		LightMenu.LightColorB_Slider.value = SunColor.z;

		LightMenu.AmbienceColorR_Slider.value = AmbientColor.x;
		LightMenu.AmbienceColorG_Slider.value = AmbientColor.y;
		LightMenu.AmbienceColorB_Slider.value = AmbientColor.z;

		LightMenu.ShadowColorR_Slider.value = ShadowColor.x;
		LightMenu.ShadowColorG_Slider.value = ShadowColor.y;
		LightMenu.ShadowColorB_Slider.value = ShadowColor.z;

		LightMenu.IgnoreUpdate = false;

		LightMenu.UndoUpdate();
	}
}
