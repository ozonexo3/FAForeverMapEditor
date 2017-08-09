using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace EditMap
{
	public class LightingInfo : MonoBehaviour
	{
		public const float SunMultipiler = 1f;
		public const float BloomMultipiler = 1f;

		public ScmapEditor Scmap;

		public InputField RA;
		public Slider RA_Slider;
		public InputField DA;
		public Slider DA_Slider;

		public InputField LightMultipiler;
		public Slider LightMultipilerSlider;

		public UiColor LightColor;
		public UiColor AmbienceColor;
		public UiColor ShadowColor;

		/*
		public InputField LightColorR;
		public Slider LightColorR_Slider;
		public InputField LightColorG;
		public Slider LightColorG_Slider;
		public InputField LightColorB;
		public Slider LightColorB_Slider;

		public InputField AmbienceColorR;
		public Slider AmbienceColorR_Slider;
		public InputField AmbienceColorG;
		public Slider AmbienceColorG_Slider;
		public InputField AmbienceColorB;
		public Slider AmbienceColorB_Slider;

		public InputField ShadowColorR;
		public Slider ShadowColorR_Slider;
		public InputField ShadowColorG;
		public Slider ShadowColorG_Slider;
		public InputField ShadowColorB;
		public Slider ShadowColorB_Slider;
		*/

		public InputField Glow;
		public Slider Glow_Slider;
		public InputField Bloom;
		public Slider Bloom_Slider;

		// Use this for initialization
		[HideInInspector]
		public bool IgnoreUpdate = true;
		void OnEnable()
		{
			IgnoreUpdate = true;
			if (Scmap.map == null) return;

			LoadValues();
		}



		public void LoadValues()
		{
			//Quaternion CheckRot = Quaternion.LookRotation(Scmap.map.SunDirection);
			Quaternion CheckRot = Scmap.Sun.transform.rotation;

			float RaHold = CheckRot.eulerAngles.y;
			if (RaHold > 180) RaHold -= 360;
			if (RaHold < -180) RaHold += 360;
			RaHold *= 10;
			RaHold = (int)RaHold;
			RaHold /= 10f;
			RA_Slider.value = RaHold;


			//float DAHold = 360 - CheckRot.eulerAngles.x;
			float DAHold = CheckRot.eulerAngles.x;
			DAHold *= 10;
			DAHold = (int)DAHold;
			DAHold /= 10f;
			DA_Slider.value = DAHold;

			LightMultipilerSlider.value = Scmap.map.LightingMultiplier;

			LightColor.SetColorField(Scmap.map.SunColor.x, Scmap.map.SunColor.y, Scmap.map.SunColor.z, UpdateColors);
			AmbienceColor.SetColorField(Scmap.map.SunAmbience.x, Scmap.map.SunAmbience.y, Scmap.map.SunAmbience.z, UpdateColors);
			ShadowColor.SetColorField(Scmap.map.ShadowFillColor.x, Scmap.map.ShadowFillColor.y, Scmap.map.ShadowFillColor.z, UpdateColors);

			/*
			LightColorR_Slider.value = Scmap.map.SunColor.x;
			LightColorG_Slider.value = Scmap.map.SunColor.y;
			LightColorB_Slider.value = Scmap.map.SunColor.z;

			AmbienceColorR_Slider.value = Scmap.map.SunAmbience.x;
			AmbienceColorG_Slider.value = Scmap.map.SunAmbience.y;
			AmbienceColorB_Slider.value = Scmap.map.SunAmbience.z;

			ShadowColorR_Slider.value = Scmap.map.ShadowFillColor.x;
			ShadowColorG_Slider.value = Scmap.map.ShadowFillColor.y;
			ShadowColorB_Slider.value = Scmap.map.ShadowFillColor.z;
			*/

			IgnoreUpdate = false;
			//UpdateMenu(true);
			UndoUpdate();
		}

		bool UndoChange = false;
		public void UndoUpdate()
		{
			UndoChange = true;
			UpdateMenu(true);
			UndoChange = false;

		}
		[HideInInspector]
		public bool SliderDrag = false;
		public void EndSliderDrag()
		{
			SliderDrag = false;
		}

		public void UpdateColors()
		{
			UpdateMenu(true);
		}

		public void UpdateMenu(bool Slider = false)
		{
			if (IgnoreUpdate) return;

			if (!UndoChange && !SliderDrag)
			{
				Debug.Log("Register lighting undo");
				Undo.Current.RegisterLightingChange(Slider);
			}

			if (Slider)
			{
				if (!UndoChange)
					SliderDrag = true;

				LightMultipiler.text = LightMultipilerSlider.value.ToString("n2");

				//Debug.Log( RA_Slider.value.ToString("n1") );
				RA.text = RA_Slider.value.ToString("n1");
				//Debug.Log(RA.text);
				DA.text = DA_Slider.value.ToString("n1");

				/*
				LightColorR.text = LightColorR_Slider.value.ToString("n2");
				LightColorG.text = LightColorG_Slider.value.ToString("n2");
				LightColorB.text = LightColorB_Slider.value.ToString("n2");

				AmbienceColorR.text = AmbienceColorR_Slider.value.ToString("n2");
				AmbienceColorG.text = AmbienceColorG_Slider.value.ToString("n2");
				AmbienceColorB.text = AmbienceColorB_Slider.value.ToString("n2");

				ShadowColorR.text = ShadowColorR_Slider.value.ToString("n2");
				ShadowColorG.text = ShadowColorG_Slider.value.ToString("n2");
				ShadowColorB.text = ShadowColorB_Slider.value.ToString("n2");
				*/

				Bloom.text = Scmap.map.Bloom.ToString("n2");



				//Glow.text = Glow_Slider.value.ToString ();
				//Bloom.text = Bloom_Slider.value.ToString ();
				UpdateMenu(false);
			}
			else
			{
				IgnoreUpdate = true;
				LightMultipilerSlider.value = Mathf.Clamp(float.Parse(LightMultipiler.text), 0, 2);
				LightMultipiler.text = LightMultipilerSlider.value.ToString();

				//Debug.Log(RA.text);
				RA_Slider.value = Mathf.Clamp(float.Parse(RA.text), -180, 180);
				RA.text = RA_Slider.value.ToString();

				DA_Slider.value = Mathf.Clamp(float.Parse(DA.text), 0, 90);
				DA.text = DA_Slider.value.ToString();

				/*
				LightColorR_Slider.value = Mathf.Clamp(float.Parse(LightColorR.text), 0, 2);
				LightColorR.text = LightColorR_Slider.value.ToString("n2");
				LightColorG_Slider.value = Mathf.Clamp(float.Parse(LightColorG.text), 0, 2);
				LightColorG.text = LightColorG_Slider.value.ToString("n2");
				LightColorB_Slider.value = Mathf.Clamp(float.Parse(LightColorB.text), 0, 2);
				LightColorB.text = LightColorB_Slider.value.ToString("n2");

				AmbienceColorR_Slider.value = Mathf.Clamp(float.Parse(AmbienceColorR.text), 0, 2);
				AmbienceColorR.text = AmbienceColorR_Slider.value.ToString("n2");
				AmbienceColorG_Slider.value = Mathf.Clamp(float.Parse(AmbienceColorG.text), 0, 2);
				AmbienceColorG.text = AmbienceColorG_Slider.value.ToString("n2");
				AmbienceColorB_Slider.value = Mathf.Clamp(float.Parse(AmbienceColorB.text), 0, 2);
				AmbienceColorB.text = AmbienceColorB_Slider.value.ToString("n2");

				ShadowColorR_Slider.value = Mathf.Clamp(float.Parse(ShadowColorR.text), 0, 2);
				ShadowColorR.text = ShadowColorR_Slider.value.ToString("n2");
				ShadowColorG_Slider.value = Mathf.Clamp(float.Parse(ShadowColorG.text), 0, 2);
				ShadowColorG.text = ShadowColorG_Slider.value.ToString("n2");
				ShadowColorB_Slider.value = Mathf.Clamp(float.Parse(ShadowColorB.text), 0, 2);
				ShadowColorB.text = ShadowColorB_Slider.value.ToString("n2");
				*/


				//Glow_Slider.value = Mathf.Clamp (float.Parse (Glow.text), 0, 2);
				//Glow.text = Glow_Slider.value.ToString ();

				//Bloom_Slider.value = Mathf.Clamp (float.Parse (Bloom.text), 0, 2);
				Scmap.map.Bloom = Mathf.Clamp(float.Parse(Bloom.text), 0, 2);

				RA_Value = (int)Mathf.Clamp(float.Parse(RA.text), -180, 180);
				DA_Value = (int)Mathf.Clamp(float.Parse(DA.text), 0, 90);

				IgnoreUpdate = false;
				UpdateLightingData();
			}
		}


		public static int RA_Value = 0;
		public static int DA_Value = 0;
		void UpdateLightingData()
		{
			if (Scmap.map == null) return;

			Scmap.map.LightingMultiplier = LightMultipilerSlider.value;

			Scmap.map.SunColor = LightColor.GetVectorValue();
			Scmap.map.SunAmbience = AmbienceColor.GetVectorValue();
			Scmap.map.ShadowFillColor = ShadowColor.GetVectorValue();

			Scmap.UpdateBloom();

			// Set light
			Scmap.Sun.transform.rotation = Quaternion.Euler(new Vector3(DA_Slider.value, -360 + RA_Slider.value, 0));

			Scmap.map.SunDirection = Scmap.Sun.transform.rotation * Vector3.back;

			Vector3 SunDIr = new Vector3(-Scmap.map.SunDirection.x, -Scmap.map.SunDirection.y, -Scmap.map.SunDirection.z);
			Scmap.Sun.transform.rotation = Quaternion.LookRotation(SunDIr);
			Scmap.Sun.color = new Color(Scmap.map.SunColor.x, Scmap.map.SunColor.y, Scmap.map.SunColor.z, 1);
			Scmap.Sun.intensity = Scmap.map.LightingMultiplier * SunMultipiler;

			// Set terrain lighting data
			Scmap.TerrainMaterial.SetFloat("_LightingMultiplier", Scmap.map.LightingMultiplier);
			Scmap.TerrainMaterial.SetColor("_SunColor", new Color(Scmap.map.SunColor.x * 0.5f, Scmap.map.SunColor.y * 0.5f, Scmap.map.SunColor.z * 0.5f, 1));
			Scmap.TerrainMaterial.SetColor("_SunAmbience", new Color(Scmap.map.SunAmbience.x * 0.5f, Scmap.map.SunAmbience.y * 0.5f, Scmap.map.SunAmbience.z * 0.5f, 1));
			Scmap.TerrainMaterial.SetColor("_ShadowColor", new Color(Scmap.map.ShadowFillColor.x * 0.5f, Scmap.map.ShadowFillColor.y * 0.5f, Scmap.map.ShadowFillColor.z * 0.5f, 1));

		}
	}
}
