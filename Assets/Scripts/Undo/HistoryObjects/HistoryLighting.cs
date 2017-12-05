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

		RA = LightMenu.RA.value;
		DA = LightMenu.DA.value;
		SunMultipiler = LightMenu.LightMultipiler.value;

		SunColor = LightMenu.LightColor.GetVectorValue();
		AmbientColor = LightMenu.AmbienceColor.GetVectorValue();
		ShadowColor = LightMenu.ShadowColor.GetVectorValue();

	}


	public override void DoUndo(){
		if (!RedoGenerated)
			HistoryLighting.GenerateRedo (Undo.Current.Prefabs.LightingChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){
		if(Undo.Current.EditMenu.State != Editing.EditStates.LightingStat) Undo.Current.EditMenu.ButtonFunction("Lighting");

		LightingInfo LightMenu = Undo.Current.EditMenu.Categorys[3].GetComponent<LightingInfo>();

		LightMenu.IgnoreUpdate = true;

		LightMenu.RA.SetValue(RA);
		LightMenu.DA.SetValue(DA);
		LightMenu.LightMultipiler.SetValue(SunMultipiler);

		LightMenu.LightColor.SetColorField(SunColor.x, SunColor.y, SunColor.z);
		LightMenu.AmbienceColor.SetColorField(AmbientColor.x, AmbientColor.y, AmbientColor.z);
		LightMenu.ShadowColor.SetColorField(ShadowColor.x, ShadowColor.y, ShadowColor.z);


		LightMenu.IgnoreUpdate = false;

		LightMenu.UndoUpdate();
	}
}
