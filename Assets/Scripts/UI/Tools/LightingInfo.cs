using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Ozone.UI;
using System.Runtime.InteropServices;
using SFB;

namespace EditMap
{
	public class LightingInfo : MonoBehaviour
	{
		public const float SunMultipiler = 1f;
		public const float BloomMultipiler = 1f;

		public ScmapEditor Scmap;

		public UiTextField RA;
		public UiTextField DA;

		public UiTextField LightMultipiler;

		public UiColor LightColor;
		public UiColor AmbienceColor;
		public UiColor ShadowColor;


		public UiTextField Glow;
		public UiTextField Bloom;

		public UiColor FogColor;
		public UiTextField FogStart;
		public UiTextField FogEnd;

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
			Quaternion CheckRot = Scmap.Sun.transform.rotation;

			float RaHold = CheckRot.eulerAngles.y;
			if (RaHold > 180) RaHold -= 360;
			if (RaHold < -180) RaHold += 360;
			RaHold *= 10;
			RaHold = (int)RaHold;
			RaHold /= 10f;
			RA.SetValue(RaHold);

			float DAHold = CheckRot.eulerAngles.x;
			DAHold *= 10;
			DAHold = (int)DAHold;
			DAHold /= 10f;
			DA.SetValue(DAHold);

			LightMultipiler.SetValue(Scmap.map.LightingMultiplier);

			LightColor.SetColorField(Scmap.map.SunColor.x, Scmap.map.SunColor.y, Scmap.map.SunColor.z); // UpdateColors
			AmbienceColor.SetColorField(Scmap.map.SunAmbience.x, Scmap.map.SunAmbience.y, Scmap.map.SunAmbience.z); // UpdateColors
			ShadowColor.SetColorField(Scmap.map.ShadowFillColor.x, Scmap.map.ShadowFillColor.y, Scmap.map.ShadowFillColor.z); // UpdateColors

			FogColor.SetColorField(Scmap.map.FogColor.x, Scmap.map.FogColor.y, Scmap.map.FogColor.z);
			FogStart.SetValue(Scmap.map.FogStart);
			FogEnd.SetValue(Scmap.map.FogEnd);

			Bloom.SetValue(Scmap.map.Bloom);

			IgnoreUpdate = false;
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

			if (!UndoChange && !SliderDrag && !Slider)
			{
				Debug.Log("Register lighting undo");
				Undo.Current.RegisterLightingChange();
			}

			if (Slider)
			{
				if (!UndoChange)
					SliderDrag = true;

				UpdateMenu(false);
			}
			else
			{
				EndSliderDrag();
				IgnoreUpdate = true;

				Scmap.map.Bloom = Bloom.value;

				RA_Value = RA.intValue;
				DA_Value = RA.intValue;

				IgnoreUpdate = false;
				UpdateLightingData();
			}
		}


		public static int RA_Value = 0;
		public static int DA_Value = 0;
		void UpdateLightingData()
		{
			if (Scmap.map == null) return;

			Scmap.map.LightingMultiplier = LightMultipiler.value;

			Scmap.map.SunColor = LightColor.GetVectorValue();
			Scmap.map.SunAmbience = AmbienceColor.GetVectorValue();
			Scmap.map.ShadowFillColor = ShadowColor.GetVectorValue();

			Scmap.map.FogColor = FogColor.GetVectorValue();
			Scmap.map.FogStart = FogStart.value;
			Scmap.map.FogEnd = FogEnd.value;

			Scmap.UpdateLighting();

			/*
			// Set light
			Scmap.Sun.transform.rotation = Quaternion.Euler(new Vector3(DA.value, -360 + RA.value, 0));

			Scmap.map.SunDirection = Scmap.Sun.transform.rotation * Vector3.back;

			Vector3 SunDIr = new Vector3(-Scmap.map.SunDirection.x, -Scmap.map.SunDirection.y, -Scmap.map.SunDirection.z);
			Scmap.Sun.transform.rotation = Quaternion.LookRotation(SunDIr);
			Scmap.Sun.color = new Color(Scmap.map.SunColor.x, Scmap.map.SunColor.y, Scmap.map.SunColor.z, 1);
			Scmap.Sun.intensity = Scmap.map.LightingMultiplier * SunMultipiler;

			Shader.SetGlobalFloat("_LightingMultiplier", Scmap.map.LightingMultiplier);
			Shader.SetGlobalColor("_SunColor", new Color(Scmap.map.SunColor.x * 0.5f, Scmap.map.SunColor.y * 0.5f, Scmap.map.SunColor.z * 0.5f, 1));
			Shader.SetGlobalColor("_SunAmbience", new Color(Scmap.map.SunAmbience.x * 0.5f, Scmap.map.SunAmbience.y * 0.5f, Scmap.map.SunAmbience.z * 0.5f, 1));
			Shader.SetGlobalColor("_ShadowColor", new Color(Scmap.map.ShadowFillColor.x * 0.5f, Scmap.map.ShadowFillColor.y * 0.5f, Scmap.map.ShadowFillColor.z * 0.5f, 1));
		*/
	}


		class LightingData{
			public float LightingMultiplier;
			public Vector3 SunDirection;

			public Vector3 SunAmbience;
			public Vector3 SunColor;
			public Vector3 ShadowFillColor;
			public Vector4 SpecularColor;

			public float Bloom;
			public Vector3 FogColor;
			public float FogStart;
			public float FogEnd;

		}

		public void ExportLightingData()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Lighting settings", "scmlighting")
			};

			var path = StandaloneFileBrowser.SaveFilePanel("Export Lighting", EnvPaths.GetMapsPath(), "", extensions);

			if (string.IsNullOrEmpty(path))
				return;

			LightingData Data = new LightingData();
			Data.LightingMultiplier = Scmap.map.LightingMultiplier;
			Data.SunDirection = Scmap.map.SunDirection;

			Data.SunAmbience = Scmap.map.SunAmbience;
			Data.SunColor = Scmap.map.SunColor;
			Data.ShadowFillColor = Scmap.map.ShadowFillColor;
			Data.SpecularColor = Scmap.map.SpecularColor;

			Data.Bloom = Scmap.map.Bloom;
			Data.FogColor = Scmap.map.FogColor;
			Data.FogStart = Scmap.map.FogStart;
			Data.FogEnd = Scmap.map.FogEnd;

			string DataString = JsonUtility.ToJson(Data);
			File.WriteAllText(path, DataString);

		}

		public void ImportLightingData()
		{

			var extensions = new[]
			{
				new ExtensionFilter("Lighting settings", "scmlighting")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import Lighting", EnvPaths.GetMapsPath(), extensions, false);


			if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
				return;

			string data = File.ReadAllText(paths[0]);
			LightingData LightingData = UnityEngine.JsonUtility.FromJson<LightingData>(data);

			Scmap.map.LightingMultiplier = LightingData.LightingMultiplier;
			Scmap.map.SunDirection = LightingData.SunDirection;

			Scmap.map.SunAmbience = LightingData.SunAmbience;
			Scmap.map.SunColor = LightingData.SunColor;
			Scmap.map.ShadowFillColor = LightingData.ShadowFillColor;
			Scmap.map.SpecularColor = LightingData.SpecularColor;

			Scmap.map.Bloom = LightingData.Bloom;
			Scmap.map.FogColor = LightingData.FogColor;
			Scmap.map.FogStart = LightingData.FogStart;
			Scmap.map.FogEnd = LightingData.FogEnd;


			LoadValues();
		}


		public void ExportProceduralSkybox()
		{
			var extensions = new[]
{
				new ExtensionFilter("Procedural skybox", "scmskybox")
			};

			var path = StandaloneFileBrowser.SaveFilePanel("Export skybox", EnvPaths.GetMapsPath(), "", extensions);

			if (string.IsNullOrEmpty(path))
				return;

			string DataString = JsonUtility.ToJson(Scmap.map.AdditionalSkyboxData);
			File.WriteAllText(path, DataString);
		}

		public void ImportProceduralSkybox()
		{
			var extensions = new[]
{
				new ExtensionFilter("Procedural skybox", "scmskybox")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import skybox", EnvPaths.GetMapsPath(), extensions, false);


			if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
				return;

			string data = File.ReadAllText(paths[0]);
			Scmap.map.AdditionalSkyboxData = UnityEngine.JsonUtility.FromJson<SkyboxData>(data);
		}
	}
}
