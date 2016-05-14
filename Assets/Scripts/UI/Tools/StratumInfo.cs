using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StratumInfo : MonoBehaviour {

	public		StratumSettingsUi		StratumSettings;
	public		ScmapEditor				Scmap;

	public		int						Selected = 0;
	public		GameObject[]			Stratum_Selections;
	public		bool[]					StratumHide = new bool[10];

	public		RawImage				Stratum_Albedo;
	public		RawImage				Stratum_Normal;
	public		Slider					Stratum_Albedo_Slider;
	public		InputField				Stratum_Albedo_Input;
	public		Slider					Stratum_Normal_Slider;
	public		InputField				Stratum_Normal_Input;

	public		GameObject				Page_Stratum;
	public		GameObject				Page_StratumSelected;
	public		GameObject				Page_Paint;
	public		GameObject				Page_PaintSelected;

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
	}


	void OnEnable () {
		ReloadStratums();
	}

	void Start(){
		ChangePageToStratum();
		SelectStratum(0);
	}


	public void ReloadStratums(){
		StratumSettings.Stratum0_Albedo.texture = Scmap.Textures[0].Albedo;
		StratumSettings.Stratum0_Normal.texture = Scmap.Textures[0].Normal;

		StratumSettings.Stratum1_Albedo.texture = Scmap.Textures[1].Albedo;
		StratumSettings.Stratum1_Normal.texture = Scmap.Textures[1].Normal;

		StratumSettings.Stratum2_Albedo.texture = Scmap.Textures[2].Albedo;
		StratumSettings.Stratum2_Normal.texture = Scmap.Textures[2].Normal;

		StratumSettings.Stratum3_Albedo.texture = Scmap.Textures[3].Albedo;
		StratumSettings.Stratum3_Normal.texture = Scmap.Textures[3].Normal;

		StratumSettings.Stratum4_Albedo.texture = Scmap.Textures[4].Albedo;
		StratumSettings.Stratum4_Normal.texture = Scmap.Textures[4].Normal;

		StratumSettings.Stratum5_Albedo.texture = Scmap.Textures[5].Albedo;
		StratumSettings.Stratum5_Normal.texture = Scmap.Textures[5].Normal;

		StratumSettings.Stratum6_Albedo.texture = Scmap.Textures[6].Albedo;
		StratumSettings.Stratum6_Normal.texture = Scmap.Textures[6].Normal;

		StratumSettings.Stratum7_Albedo.texture = Scmap.Textures[7].Albedo;
		StratumSettings.Stratum7_Normal.texture = Scmap.Textures[7].Normal;

		StratumSettings.Stratum8_Albedo.texture = Scmap.Textures[8].Albedo;
		StratumSettings.Stratum8_Normal.texture = Scmap.Textures[8].Normal;

		StratumSettings.Stratum9_Albedo.texture = Scmap.Textures[9].Albedo;
		StratumSettings.Stratum9_Normal.texture = Scmap.Textures[9].Normal;


		StratumSettings.Stratum1_Mask.texture = Scmap.map.TexturemapTex;
		StratumSettings.Stratum2_Mask.texture = Scmap.map.TexturemapTex;
		StratumSettings.Stratum3_Mask.texture = Scmap.map.TexturemapTex;
		StratumSettings.Stratum4_Mask.texture = Scmap.map.TexturemapTex;

		StratumSettings.Stratum5_Mask.texture = Scmap.map.TexturemapTex2;
		StratumSettings.Stratum6_Mask.texture = Scmap.map.TexturemapTex2;
		StratumSettings.Stratum7_Mask.texture = Scmap.map.TexturemapTex2;
		StratumSettings.Stratum8_Mask.texture = Scmap.map.TexturemapTex2;
	}

	public void SelectStratum(int newid){
		Selected = newid;

		foreach(GameObject obj in Stratum_Selections) obj.SetActive(false);

		Stratum_Selections[Selected].SetActive(true);

		Stratum_Albedo.texture = Scmap.Textures[Selected].Albedo;
		Stratum_Normal.texture = Scmap.Textures[Selected].Normal;


		Stratum_Albedo_Slider.value = Scmap.Textures[Selected].AlbedoScale;
		Stratum_Albedo_Input.text = Scmap.Textures[Selected].AlbedoScale.ToString();

		Stratum_Normal_Slider.value = Scmap.Textures[Selected].AlbedoScale;
		Stratum_Normal_Input.text = Scmap.Textures[Selected].AlbedoScale.ToString();
	}

	public void UpdateStratumMenu(bool slider = false){
		if(slider){


			UpdateStratumMenu(false);
		}
		else{


		}
	}

	public void ToggleLayerVisibility(int id){
		StratumHide[id] = !StratumHide[id];
		// TODO Update Terrain Shader To Hide Stratum
	}

	public void ChangePageToStratum(){
		Page_Stratum.SetActive(true);
		Page_StratumSelected.SetActive(true);
		Page_Paint.SetActive(false);
		Page_PaintSelected.SetActive(false);
	}

	public void ChangePageToPaint(){
		Page_Stratum.SetActive(false);
		Page_StratumSelected.SetActive(false);
		Page_Paint.SetActive(true);
		Page_PaintSelected.SetActive(true);
	}
}
