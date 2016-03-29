using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightingInfo : MonoBehaviour {

	public		ScmapEditor			Scmap;

	public		InputField			RA;
	public		InputField			DA;

	public		InputField			LightMultipiler;
	public		Slider				LightMultipilerSlider;

	public		InputField			LightColorR;
	public		Slider 				LightColorR_Slider;
	public		InputField			LightColorG;
	public		Slider 				LightColorG_Slider;
	public		InputField			LightColorB;
	public		Slider 				LightColorB_Slider;

	public		InputField			AmbienceColorR;
	public		Slider 				AmbienceColorR_Slider;
	public		InputField			AmbienceColorG;
	public		Slider 				AmbienceColorG_Slider;
	public		InputField			AmbienceColorB;
	public		Slider 				AmbienceColorB_Slider;

	public		InputField			ShadowColorR;
	public		Slider 				ShadowColorR_Slider;
	public		InputField			ShadowColorG;
	public		Slider 				ShadowColorG_Slider;
	public		InputField			ShadowColorB;
	public		Slider 				ShadowColorB_Slider;

	public		InputField			Glow;
	public		Slider 				Glow_Slider;
	public		InputField			Bloom;
	public		Slider 				Bloom_Slider;

	// Use this for initialization
	void OnEnable () {
	
	}
	
	public void UpdateMenu(bool Slider = false){
		if(Slider){
			LightMultipiler.text = LightMultipilerSlider.value.ToString();

			LightColorR.text = LightColorR_Slider.value.ToString();
			LightColorG.text = LightColorG_Slider.value.ToString();
			LightColorB.text = LightColorB_Slider.value.ToString();

			AmbienceColorR.text = AmbienceColorR_Slider.value.ToString();
			AmbienceColorG.text = AmbienceColorG_Slider.value.ToString();
			AmbienceColorB.text = AmbienceColorB_Slider.value.ToString();

			ShadowColorR.text = ShadowColorR_Slider.value.ToString();
			ShadowColorG.text = ShadowColorG_Slider.value.ToString();
			ShadowColorB.text = ShadowColorB_Slider.value.ToString();

			Glow.text = Glow_Slider.value.ToString ();
			Bloom.text = Bloom_Slider.value.ToString ();

			UpdateMenu (false);
		}
		else{
			LightMultipilerSlider.value = Mathf.Clamp (float.Parse (LightMultipiler.text), 0, 2);
			LightMultipiler.text = LightMultipilerSlider.value.ToString();

			LightColorR_Slider.value = Mathf.Clamp (float.Parse (LightColorR.text), 0, 2);
			LightColorR.text = LightColorR_Slider.value.ToString();
			LightColorG_Slider.value = Mathf.Clamp (float.Parse (LightColorG.text), 0, 2);
			LightColorG.text = LightColorG_Slider.value.ToString();
			LightColorB_Slider.value = Mathf.Clamp (float.Parse (LightColorB.text), 0, 2);
			LightColorB.text = LightColorB_Slider.value.ToString();

			AmbienceColorR_Slider.value = Mathf.Clamp (float.Parse (AmbienceColorR.text), 0, 2);
			AmbienceColorR.text = AmbienceColorR_Slider.value.ToString();
			AmbienceColorG_Slider.value = Mathf.Clamp (float.Parse (AmbienceColorG.text), 0, 2);
			AmbienceColorG.text = AmbienceColorG_Slider.value.ToString();
			AmbienceColorB_Slider.value = Mathf.Clamp (float.Parse (AmbienceColorB.text), 0, 2);
			AmbienceColorB.text = AmbienceColorB_Slider.value.ToString();

			ShadowColorR_Slider.value = Mathf.Clamp (float.Parse (ShadowColorR.text), 0, 2);
			ShadowColorR.text = ShadowColorR_Slider.value.ToString();
			ShadowColorG_Slider.value = Mathf.Clamp (float.Parse (ShadowColorG.text), 0, 2);
			ShadowColorG.text = ShadowColorG_Slider.value.ToString();
			ShadowColorB_Slider.value = Mathf.Clamp (float.Parse (ShadowColorB.text), 0, 2);
			ShadowColorB.text = ShadowColorB_Slider.value.ToString();

			Glow_Slider.value = Mathf.Clamp (float.Parse (Glow.text), 0, 2);
			Glow.text = Glow_Slider.value.ToString ();

			Bloom_Slider.value = Mathf.Clamp (float.Parse (Bloom.text), 0, 2);
			Bloom.text = Bloom_Slider.value.ToString ();

			RA_Value =  (int)Mathf.Clamp( float.Parse (RA.text), -180, 180);
			DA_Value =  (int)Mathf.Clamp( float.Parse (DA.text), 0, 90);

			UpdateLightingData ();
		}
	}


	public static int RA_Value = 0;
	public static int DA_Value = 0;
	void UpdateLightingData(){
		// Set light
		Scmap.map.SunDirection = Quaternion.Euler(new Vector3( RA_Value, DA_Value, 0)) * Vector3.forward;

		Vector3 SunDIr = new Vector3(-Scmap.map.SunDirection.x, -Scmap.map.SunDirection.y, -Scmap.map.SunDirection.z);
		Scmap.Sun.transform.rotation = Quaternion.LookRotation( SunDIr);
		Scmap.Sun.color = new Color(Scmap.map.SunColor.x, Scmap.map.SunColor.y , Scmap.map.SunColor.z, 1) ;
		Scmap.Sun.intensity = Scmap.map.LightingMultiplier * 1.0f;

		// Set terrain lighting data
		Scmap.TerrainMaterial.SetFloat("_LightingMultiplier", Scmap.map.LightingMultiplier);
		Scmap.TerrainMaterial.SetColor("_SunColor",  new Color(Scmap.map.SunColor.x * 0.5f, Scmap.map.SunColor.y * 0.5f, Scmap.map.SunColor.z * 0.5f, 1));
		Scmap.TerrainMaterial.SetColor("_SunAmbience",  new Color(Scmap.map.SunAmbience.x * 0.5f, Scmap.map.SunAmbience.y * 0.5f, Scmap.map.SunAmbience.z * 0.5f, 1));
		Scmap.TerrainMaterial.SetColor("_ShadowColor",  new Color(Scmap.map.ShadowFillColor.x * 0.5f, Scmap.map.ShadowFillColor.y * 0.5f, Scmap.map.ShadowFillColor.z * 0.5f, 1));

	}
}
