using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StratumInfo : MonoBehaviour {

	public		StratumSettingsUi		StratumSettings;

	[System.Serializable]
	public class StratumSettingsUi{
		[Header("Textures")]
		public		RawImage		Stratum9_Albedo;
		public		RawImage		Stratum9_Normal;
		public		RawImage		Stratum8_Albedo;
		public		RawImage		Stratum8_Normal;
		public		RawImage		Stratum7_Albedo;
		public		RawImage		Stratum7_Normal;
		public		RawImage		Stratum6_Albedo;
		public		RawImage		Stratum6_Normal;
		public		RawImage		Stratum5_Albedo;
		public		RawImage		Stratum5_Normal;
		public		RawImage		Stratum4_Albedo;
		public		RawImage		Stratum4_Normal;
		public		RawImage		Stratum3_Albedo;
		public		RawImage		Stratum3_Normal;
		public		RawImage		Stratum2_Albedo;
		public		RawImage		Stratum2_Normal;
		public		RawImage		Stratum1_Albedo;
		public		RawImage		Stratum1_Normal;
		public		RawImage		Stratum0_Albedo;
		public		RawImage		Stratum0_Normal;

		[Header("Mask")]
		public		RawImage		Stratum9_Mask;
		public		RawImage		Stratum8_Mask;
		public		RawImage		Stratum7_Mask;
		public		RawImage		Stratum6_Mask;
		public		RawImage		Stratum5_Mask;
		public		RawImage		Stratum4_Mask;
		public		RawImage		Stratum3_Mask;
		public		RawImage		Stratum2_Mask;
		public		RawImage		Stratum1_Mask;
		public		RawImage		Stratum0_Mask;

		[Header("Visible")]
		public		bool			Stratum9_Visible = true;
		public		bool			Stratum8_Visible = true;
		public		bool			Stratum7_Visible = true;
		public		bool			Stratum6_Visible = true;
		public		bool			Stratum5_Visible = true;
		public		bool			Stratum4_Visible = true;
		public		bool			Stratum3_Visible = true;
		public		bool			Stratum2_Visible = true;
		public		bool			Stratum1_Visible = true;
		public		bool			Stratum0_Visible = true;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
