using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EditMap;
using System.Runtime.InteropServices;

public class StratumInfo : MonoBehaviour {

	public		StratumSettingsUi		StratumSettings;
	public		Editing					Edit;
	public		ScmapEditor				Map;

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
	public		GameObject				Page_Settings;
	public		GameObject				Page_SettingsSelected;

	// Brush
	[Header("Brush")]
	public		Slider				BrushSizeSlider;
	public		InputField			BrushSize;
	public		Slider				BrushStrengthSlider;
	public		InputField			BrushStrength;
	public		Slider				BrushRotationSlider;
	public		InputField			BrushRotation;

	public		InputField			BrushMini;
	public		InputField			BrushMax;

	public		LayerMask				TerrainMask;
	public		List<Toggle>			BrushToggles;
	public		ToggleGroup				ToogleGroup;

	public		GameObject			BrushListObject;
	public		Transform			BrushListPivot;
	public		Material			TerrainMaterial;

	[Header("State")]
	public bool Invert;
	public bool Smooth;

	#region Classes
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
	#endregion

	void OnEnable () {
		BrushGenerator.LoadBrushesh ();
		ReloadStratums();

		if (Page_Stratum.activeSelf) {
			ChangePageToStratum ();
		} else if (Page_Paint.activeSelf) {
			ChangePageToPaint ();
		} else {
			ChangePageToSettings ();
		}
	}

	void OnDisable(){
		TerrainMaterial.SetFloat("_BrushSize", 0 );
	}

	void Start(){
		ChangePageToStratum();
		SelectStratum(0);
	}


	bool TerainChanged = false;
	Color[] beginColors;

	Vector3 BeginMousePos;
	float StrengthBeginValue;
	bool ChangingStrength;
	float SizeBeginValue;
	bool ChangingSize;
	void Update () {
		if (StratumChangeCheck)
			if (Input.GetMouseButtonUp (0))
				StratumChangeCheck = false;
		
		if (Page_Paint.activeSelf) {
			Invert = Input.GetKey (KeyCode.LeftAlt);
			Smooth = Input.GetKey (KeyCode.LeftShift);



			if (Edit.MauseOnGameplay || ChangingStrength || ChangingSize) {
				if (!ChangingSize && (Input.GetKey (KeyCode.M) || ChangingStrength)) {
					// Change Strength
					if (Input.GetMouseButtonDown (0)) {
						ChangingStrength = true;
						BeginMousePos = Input.mousePosition;
						StrengthBeginValue = BrushStrengthSlider.value;
					} else if (Input.GetMouseButtonUp (0)) {
						ChangingStrength = false;
					}
					if (ChangingStrength) {
						BrushStrengthSlider.value = Mathf.Clamp (StrengthBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.1f), 0, 100);
						UpdateStratumMenu (true);
						//UpdateBrushPosition(true);

					}
				} else if (Input.GetKey (KeyCode.B) || ChangingSize) {
					// Change Size
					if (Input.GetMouseButtonDown (0)) {
						ChangingSize = true;
						BeginMousePos = Input.mousePosition;
						SizeBeginValue = BrushSizeSlider.value;
					} else if (Input.GetMouseButtonUp (0)) {
						ChangingSize = false;
					}
					if (ChangingSize) {
						BrushSizeSlider.value = Mathf.Clamp (SizeBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.4f), 1, 256);
						UpdateStratumMenu (true);
						UpdateBrushPosition (true);

					}
				} else {
						if (Input.GetMouseButtonDown (0)) {
							if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition (true)) {
								SymmetryPaint ();
							}
						} else if (Input.GetMouseButton (0)) {
						if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition (false)) {
								SymmetryPaint ();
							}
						} else {
							UpdateBrushPosition (true);
						}
				}
			}

			if (TerainChanged && Input.GetMouseButtonUp (0)) {
				if(Selected > 0 && Selected < 5)
					MapLuaParser.Current.History.RegisterStratumPaint (beginColors, 0);
				else if(Selected > 4 && Selected < 9)
					MapLuaParser.Current.History.RegisterStratumPaint (beginColors, 1);
				TerainChanged = false;
			}

			if (PlayerPrefs.GetInt ("Symmetry", 0) != BrushGenerator.LastSym) {
				BrushGenerator.GeneratePaintBrushesh ();
			}
		}
	}

	#region Stratums
	public void ReloadStratums(){
		StratumSettings.Stratum0_Albedo.texture = Map.Textures[0].Albedo;
		StratumSettings.Stratum0_Normal.texture = Map.Textures[0].Normal;

		StratumSettings.Stratum1_Albedo.texture = Map.Textures[1].Albedo;
		StratumSettings.Stratum1_Normal.texture = Map.Textures[1].Normal;

		StratumSettings.Stratum2_Albedo.texture = Map.Textures[2].Albedo;
		StratumSettings.Stratum2_Normal.texture = Map.Textures[2].Normal;

		StratumSettings.Stratum3_Albedo.texture = Map.Textures[3].Albedo;
		StratumSettings.Stratum3_Normal.texture = Map.Textures[3].Normal;

		StratumSettings.Stratum4_Albedo.texture = Map.Textures[4].Albedo;
		StratumSettings.Stratum4_Normal.texture = Map.Textures[4].Normal;

		StratumSettings.Stratum5_Albedo.texture = Map.Textures[5].Albedo;
		StratumSettings.Stratum5_Normal.texture = Map.Textures[5].Normal;

		StratumSettings.Stratum6_Albedo.texture = Map.Textures[6].Albedo;
		StratumSettings.Stratum6_Normal.texture = Map.Textures[6].Normal;

		StratumSettings.Stratum7_Albedo.texture = Map.Textures[7].Albedo;
		StratumSettings.Stratum7_Normal.texture = Map.Textures[7].Normal;

		StratumSettings.Stratum8_Albedo.texture = Map.Textures[8].Albedo;
		StratumSettings.Stratum8_Normal.texture = Map.Textures[8].Normal;

		StratumSettings.Stratum9_Albedo.texture = Map.Textures[9].Albedo;
		StratumSettings.Stratum9_Normal.texture = Map.Textures[9].Normal;


		StratumSettings.Stratum1_Mask.texture = Map.map.TexturemapTex;
		StratumSettings.Stratum2_Mask.texture = Map.map.TexturemapTex;
		StratumSettings.Stratum3_Mask.texture = Map.map.TexturemapTex;
		StratumSettings.Stratum4_Mask.texture = Map.map.TexturemapTex;

		StratumSettings.Stratum5_Mask.texture = Map.map.TexturemapTex2;
		StratumSettings.Stratum6_Mask.texture = Map.map.TexturemapTex2;
		StratumSettings.Stratum7_Mask.texture = Map.map.TexturemapTex2;
		StratumSettings.Stratum8_Mask.texture = Map.map.TexturemapTex2;
	}

	bool LoadingStratum = false;
	public void SelectStratum(int newid){
		LoadingStratum = true;
		Selected = newid;

		foreach(GameObject obj in Stratum_Selections) obj.SetActive(false);

		Stratum_Selections[Selected].SetActive(true);

		Stratum_Albedo.texture = Map.Textures[Selected].Albedo;
		Stratum_Normal.texture = Map.Textures[Selected].Normal;


		Stratum_Albedo_Slider.value = Map.Textures[Selected].AlbedoScale;
		Stratum_Albedo_Input.text = Map.Textures[Selected].AlbedoScale.ToString();

		Stratum_Normal_Slider.value = Map.Textures[Selected].NormalScale;
		Stratum_Normal_Input.text = Map.Textures[Selected].NormalScale.ToString();
		LoadingStratum = false;
	}


	public void ToggleLayerVisibility(int id){
		StratumHide[id] = !StratumHide[id];
		// TODO Update Terrain Shader To Hide Stratum
	}

	#endregion

	#region Update Menu
	public void ChangePageToStratum(){
		Page_Stratum.SetActive(true);
		Page_StratumSelected.SetActive(true);
		Page_Paint.SetActive(false);
		Page_PaintSelected.SetActive(false);
		Page_Settings.SetActive(false);
		Page_SettingsSelected.SetActive(false);
		TerrainMaterial.SetFloat("_BrushSize", 0 );
	}

	public void ChangePageToPaint(){
		Page_Stratum.SetActive(false);
		Page_StratumSelected.SetActive(false);
		Page_Paint.SetActive(true);
		Page_PaintSelected.SetActive(true);
		Page_Settings.SetActive(false);
		Page_SettingsSelected.SetActive(false);

		BrushGenerator.LoadBrushesh ();

		if(!BrusheshLoaded) LoadBrushesh();
		UpdateStratumMenu();
		TerrainMaterial.SetInt("_Brush", 1);
		BrushGenerator.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
		BrushGenerator.Brushes[SelectedFalloff].mipMapBias = -1f;
		TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Brushes[SelectedFalloff]);
	}

	public void ChangePageToSettings(){
		Page_Stratum.SetActive(false);
		Page_StratumSelected.SetActive(false);
		Page_Paint.SetActive(false);
		Page_PaintSelected.SetActive(false);
		Page_Settings.SetActive(true);
		Page_SettingsSelected.SetActive(true);
		TerrainMaterial.SetFloat("_BrushSize", 0 );
	}


	public float Min = 0;
	public float Max = 512;
	int LastRotation = 0;
	bool StratumChangeCheck = false;

	public void UpdateStratumMenu(bool Slider = false){
		if (!gameObject.activeSelf)
			return;

		if (Page_Stratum.activeSelf) {
			if (Slider) {
				if (!StratumChangeCheck) {
					StratumChangeCheck = true;
					if(!LoadingStratum )
						Undo.RegisterStratumChange (Selected);
				}
				if (!LoadingStratum) {
					Stratum_Albedo_Input.text = Stratum_Albedo_Slider.value.ToString ();
					Stratum_Normal_Input.text = Stratum_Normal_Slider.value.ToString ();
				}
			} else {
				if (!LoadingStratum) {
					Undo.RegisterStratumChange (Selected);
					Stratum_Albedo_Slider.value = float.Parse (Stratum_Albedo_Input.text);
					Stratum_Normal_Slider.value = float.Parse (Stratum_Normal_Input.text);
				}
			}
			if (!LoadingStratum) {
				Map.Textures [Selected].AlbedoScale = Stratum_Albedo_Slider.value;
				Map.Textures [Selected].NormalScale = Stratum_Normal_Slider.value;
			}

			//Map.map.Layers [Selected].ScaleTexture = Map.Textures [Selected].AlbedoScale;
			//Map.map.Layers [Selected].ScaleNormalmap = Map.Textures [Selected].NormalScale;

			Map.UpdateScales (Selected);

		} else if(Page_Paint.activeSelf) {
			if (Slider) {
				BrushSize.text = BrushSizeSlider.value.ToString ();
				BrushStrength.text = BrushStrengthSlider.value.ToString ();
				//BrushRotation.text = BrushRotationSlider.value.ToString();
			} else {
				BrushSizeSlider.value = float.Parse (BrushSize.text);
				BrushStrengthSlider.value = int.Parse (BrushStrength.text);
				//BrushRotationSlider.value = int.Parse(BrushRotation.text);
			}

			BrushSizeSlider.value = Mathf.Clamp (BrushSizeSlider.value, 1, 256);
			BrushStrengthSlider.value = (int)Mathf.Clamp (BrushStrengthSlider.value, 0, 100);
			//BrushRotationSlider.value = (int)Mathf.Clamp(BrushStrengthSlider.value, -360, 360);

			BrushSize.text = BrushSizeSlider.value.ToString ();
			BrushStrength.text = BrushStrengthSlider.value.ToString ();
			//BrushRotation.text = BrushRotationSlider.value.ToString();

			Min = int.Parse (BrushMini.text);
			Max = int.Parse (BrushMax.text);

			Min = Mathf.Clamp (Min, 0, Max);
			Max = Mathf.Clamp (Max, Min, 90);

			BrushMini.text = Min.ToString ("0");
			BrushMax.text = Max.ToString ("0");

			if (LastRotation != int.Parse (BrushRotation.text)) {
				LastRotation = int.Parse (BrushRotation.text);
				if (LastRotation == 0) {
					BrushGenerator.RotatedBrush = BrushGenerator.Brushes [SelectedFalloff];
				} else {
					BrushGenerator.RotatedBrush = BrushGenerator.rotateTexture (BrushGenerator.Brushes [SelectedFalloff], LastRotation);
				}

				TerrainMaterial.SetTexture ("_BrushTex", (Texture)BrushGenerator.RotatedBrush);
				BrushGenerator.GeneratePaintBrushesh ();
			}
			TerrainMaterial.SetFloat ("_BrushSize", BrushSizeSlider.value);
		}
	}
	#endregion

	#region Load all brushesh
	bool BrusheshLoaded = false;
	string StructurePath;
	public void LoadBrushesh(){
		Clean();


		StructurePath = Application.dataPath + "/Structure/";;
		#if UNITY_EDITOR
		StructurePath = StructurePath.Replace("Assets", "");
		#endif
		StructurePath += "brush";

		if(!Directory.Exists(StructurePath)){
			Debug.LogError("Cant find brush folder");
			return;
		}

		BrushToggles = new List<Toggle>();

		for(int i = 0; i < BrushGenerator.Brushes.Count; i++){
			GameObject NewBrush = Instantiate(BrushListObject) as GameObject;
			NewBrush.transform.SetParent(BrushListPivot, false);
			NewBrush.transform.localScale = Vector3.one;
			string ThisName = BrushGenerator.BrushesNames[i];
			BrushToggles.Add( NewBrush.GetComponent<BrushListId>().SetBrushList(ThisName, BrushGenerator.Brushes[i], i ));
			NewBrush.GetComponent<BrushListId>().Controler2 = this;
		}

		foreach(Toggle tog in BrushToggles){
			tog.isOn = false;
			tog.group = ToogleGroup;
		}
		BrushToggles[0].isOn = true;
		SelectedFalloff = 0;

		BrusheshLoaded = true;
	}

	void Clean(){
		BrusheshLoaded = false;
		foreach(Transform child in BrushListPivot) Destroy(child.gameObject);
	}

	#endregion


	#region Brush Update
	int SelectedBrush = 0;
	public void ChangeBrush(int id){
		SelectedBrush = id;
	}

	int SelectedFalloff = 0;
	public void ChangeFalloff(int id){
		SelectedFalloff = id;
		BrushGenerator.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
		BrushGenerator.Brushes[SelectedFalloff].mipMapBias = -1f;
		LastRotation = int.Parse( BrushRotation.text);
		if(LastRotation == 0){
			BrushGenerator.RotatedBrush = BrushGenerator.Brushes[SelectedFalloff];
		}
		else{
			BrushGenerator.RotatedBrush = BrushGenerator.rotateTexture(BrushGenerator.Brushes[SelectedFalloff], LastRotation);
		}
		TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.RotatedBrush);
		BrushGenerator.GeneratePaintBrushesh();
	}


	Vector3 BrushPos;
	Vector3 MouseBeginClick;
	bool UpdateBrushPosition(bool Forced = false){
		//Debug.Log(Vector3.Distance(MouseBeginClick, Input.mousePosition));
		if(Forced || Vector3.Distance(MouseBeginClick, Input.mousePosition) > 1){}
		else{
			return false;
		}


		MouseBeginClick = Input.mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 2000, TerrainMask)){
			BrushPos = hit.point;
			BrushPos.y = Map.Teren.SampleHeight(BrushPos);

			Vector3 tempCoord = Map.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
			Vector3 coord  = Vector3.zero;
			coord.x = (tempCoord.x -  (int)BrushSizeSlider.value * MapLuaParser.Current.ScenarioData.Size.x * 0.0001f) / Map.Teren.terrainData.size.x; // TODO 0.05 ?? this should be terrain proportion?
			//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
			coord.z = (tempCoord.z -  (int)BrushSizeSlider.value * MapLuaParser.Current.ScenarioData.Size.y * 0.0001f) / Map.Teren.terrainData.size.z;

			TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value );
			TerrainMaterial.SetFloat("_BrushUvX", coord.x );
			TerrainMaterial.SetFloat("_BrushUvY", coord.z );

			return true;
		}
		return false;
	}
	#endregion


	void SymmetryPaint(){
		BrushGenerator.GenerateSymmetry(BrushPos);

		if (Selected == 1 || Selected == 5)
			PaintChannel = 0;
		else if (Selected == 2 || Selected == 6)
			PaintChannel = 1;
		else if (Selected == 3 || Selected == 7)
			PaintChannel = 2;
		else if (Selected == 4 || Selected == 8)
			PaintChannel = 3;

		for(int i = 0; i < BrushGenerator.PaintPositions.Length; i++){
			Paint(BrushGenerator.PaintPositions[i], i);

		}

		if (Selected > 0 && Selected < 5) {
			Map.map.TexturemapTex.Apply ();
		} else if (Selected > 4 && Selected < 9) {
			Map.map.TexturemapTex2.Apply ();
		}
	}
		

	static int StratumTexSampleHeight = 0;
	static Color[] StratumData;
	static int PaintChannel = 0;
	void Paint(Vector3 AtPosition, int id = 0){
		int hmWidth = Map.map.TexturemapTex.width;
		int hmHeight = Map.map.TexturemapTex.height;

		Vector3 tempCoord = Map.Teren.gameObject.transform.InverseTransformPoint(AtPosition);
		Vector3 coord  = Vector3.zero;
		coord.x = tempCoord.x / Map.Teren.terrainData.size.x;
		//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
		coord.z = 1 - tempCoord.z / Map.Teren.terrainData.size.z;

		if(coord.x > 1) return;
		if(coord.x < 0) return;
		if(coord.z > 1) return;
		if(coord.z < 0) return;

		// get the position of the terrain heightmap where this game object is
		int posXInTerrain = (int) (coord.x * hmWidth); 
		int posYInTerrain = (int) (coord.z * hmHeight);
		// we set an offset so that all the raising terrain is under this game object
		int size = (int)BrushSizeSlider.value;
		int offset = size / 2;
		// get the heights of the terrain under this game object

		// Horizontal Brush Offsets
		int OffsetLeft = 0;
		if(posXInTerrain-offset < 0) OffsetLeft = Mathf.Abs(posXInTerrain-offset);
		int OffsetRight = 0;
		if(posXInTerrain-offset + size > hmWidth) OffsetRight = posXInTerrain-offset + size - hmWidth;

		// Vertical Brush Offsets
		int OffsetDown = 0;
		if(posYInTerrain-offset < 0) OffsetDown = Mathf.Abs(posYInTerrain-offset);
		int OffsetTop = 0;
		if(posYInTerrain-offset + size > hmHeight) OffsetTop = posYInTerrain-offset + size - hmHeight;

		float CenterHeight = 0;
		float BrushStrength = Mathf.Pow( BrushStrengthSlider.value * 0.01f, 1);
		float inverted = (Invert ? (-1) : 1);
		float SambleBrush = 0;
		Color BrushValue;
		int x = 0;
		int y = 0;
		int i = 0;
		int j = 0;

		if (Selected > 0 && Selected < 5) {
			StratumData = Map.map.TexturemapTex.GetPixels (posXInTerrain-offset + OffsetLeft, posYInTerrain-offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop);
		} else if (Selected > 4 && Selected < 9) {
			StratumData = Map.map.TexturemapTex2.GetPixels (posXInTerrain-offset + OffsetLeft, posYInTerrain-offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop);
		} else
			return;


		StratumTexSampleHeight = (size - OffsetDown) - OffsetTop;

		for (i = 0; i < (size - OffsetDown) - OffsetTop; i++){
			for (j = 0; j < (size - OffsetLeft) - OffsetRight; j++){
				float angle = Vector3.Angle(Vector3.up, Map.Teren.terrainData.GetInterpolatedNormal ((posXInTerrain - offset + OffsetLeft + i) / (float)hmWidth, 1 - (posYInTerrain - offset + OffsetDown + j) / (float)hmHeight));
				if ((angle < Min && Min > 0) || (angle > Max && Max < 90))
					continue;

				// Brush strength
				x = (int)(((i + OffsetDown) / (float)size) * BrushGenerator.PaintImage[id].width);
				y = (int)(((j + OffsetLeft) / (float)size) * BrushGenerator.PaintImage[id].height);
				BrushValue =  BrushGenerator.PaintImage[id].GetPixel(y, x);
				SambleBrush = BrushValue.r;



				if(SambleBrush >= 0.02f) {
					if(Smooth || SelectedBrush == 2){
						//float PixelPower = Mathf.Abs( heights[i,j] - CenterHeight);
						//heights[i,j] = Mathf.Lerp(heights[i,j], CenterHeight, BrushStrengthSlider.value * 0.4f * Mathf.Pow(SambleBrush, 2) * PixelPower);
					}
					else if(SelectedBrush == 3){
						//float PixelPower = heights[i,j] - CenterHeight;
						//heights[i,j] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * BrushStrengthSlider.value * 0.01f * Mathf.Pow(SambleBrush, 2);
					}
					else{
						switch (PaintChannel) {
						case 0:
							StratumData[XyToColorId(i,j)].r += SambleBrush * BrushStrength * inverted;
							break;
						case 1:
							StratumData[XyToColorId(i,j)].g += SambleBrush * BrushStrength * inverted;
							break;
						case 2:
							StratumData[XyToColorId(i,j)].b += SambleBrush * BrushStrength * inverted;
							break;
						case 3:
							StratumData[XyToColorId(i,j)].a += SambleBrush * BrushStrength * inverted;
							break;
						}
						//heights[i,j] += SambleBrush * BrushStrengthSlider.value * 0.0002f * (Invert?(-1):1);
					}

					//heights[i,j] = Mathf.Clamp(heights[i,j], Min, Max);
				}
			}
		}
		// set the new height
		if(!TerainChanged){
			if (Selected > 0 && Selected < 5) {
				beginColors = Map.map.TexturemapTex.GetPixels ();
			} else if (Selected > 4 && Selected < 9) {
				beginColors = Map.map.TexturemapTex2.GetPixels ();
			}

			TerainChanged = true;
		}
		if (Selected > 0 && Selected < 5) {
			Map.map.TexturemapTex.SetPixels (posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop, StratumData);
		} else {
			Map.map.TexturemapTex2.SetPixels (posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop, StratumData);
		}
		//Map.map.TexturemapTex.SetPixels(StratumData);
	}

	static int XyToColorId(int x, int y){
		return x + y * StratumTexSampleHeight;
	}

	public void SelectAlbedo(){
		if (!ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
			return;
		if (ResourceBrowser.SelectedCategory == 0 || ResourceBrowser.SelectedCategory == 1) {
			Undo.RegisterStratumChange (Selected);
			Debug.Log (ResourceBrowser.Current.LoadedPaths [ResourceBrowser.DragedObject.InstanceId]);

			Map.Textures [Selected].Albedo = ResourceBrowser.Current.LoadedTextures [ResourceBrowser.DragedObject.InstanceId];
			Map.Textures [Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths [ResourceBrowser.DragedObject.InstanceId];

			//Map.map.Layers [Selected].PathTexture = Map.Textures [Selected].AlbedoPath;


			Map.SetTextures (Selected);
			ReloadStratums ();
			SelectStratum (Selected);
		}
	}

	public void SelectNormal(){
		if (!ResourceBrowser.Current.gameObject.activeSelf)
			return;
		if (ResourceBrowser.SelectedCategory == 0 || ResourceBrowser.SelectedCategory == 1) {

			Undo.RegisterStratumChange (Selected);
			Debug.Log (ResourceBrowser.Current.LoadedPaths [ResourceBrowser.DragedObject.InstanceId]);

			//Map.Textures [Selected].Normal = ResourceBrowser.Current.LoadedTextures [ResourceBrowser.DragedObject.InstanceId];
			Map.Textures [Selected].NormalPath = ResourceBrowser.Current.LoadedPaths [ResourceBrowser.DragedObject.InstanceId];

			Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[Selected].NormalPath, Selected, true);

			//Map.map.Layers [Selected].PathNormalmap = Map.Textures [Selected].NormalPath;

			Map.SetTextures (Selected);
			ReloadStratums ();
			SelectStratum (Selected);
		}
	}

	public void ClickAlbedo(){
		Map.ResBrowser.LoadStratumTexture (Map.Textures [Selected].AlbedoPath);
	}

	public void ClickNormal(){
		Map.ResBrowser.LoadStratumTexture (Map.Textures [Selected].NormalPath);
	}

	public void ExportStratum(){
		System.Windows.Forms.SaveFileDialog FolderDialog = new System.Windows.Forms.SaveFileDialog ();

		FolderDialog.Filter = "scmstratum files (*.scmsl)|*.scmsl|All files (*.*)|*.*"  ;
		FolderDialog.FilterIndex = 0 ;
		FolderDialog.RestoreDirectory = true ;
		FolderDialog.InitialDirectory = EnvPaths.GetMapsPath();

		if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			Debug.Log( FolderDialog.FileName );

			string data = UnityEngine.JsonUtility.ToJson (Map.Textures [Selected]);

			File.WriteAllText (FolderDialog.FileName, data);
		}
	}

	public void ImportStratum(){
		System.Windows.Forms.OpenFileDialog FolderDialog = new System.Windows.Forms.OpenFileDialog ();

		//FolderDialog.DefaultExt = "scmstratum";
		//FolderDialog.AddExtension = true;
		FolderDialog.Filter = "scmstratum files (*.scmsl)|*.scmsl|All files (*.*)|*.*"  ;
		FolderDialog.FilterIndex = 0 ;
		FolderDialog.RestoreDirectory = true ;
		FolderDialog.InitialDirectory = EnvPaths.GetMapsPath();
		//FolderDialog.

		if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			Debug.Log( FolderDialog.FileName );

			string data = File.ReadAllText (FolderDialog.FileName);

			ScmapEditor.TerrainTexture NewTexture = UnityEngine.JsonUtility.FromJson<ScmapEditor.TerrainTexture> (data);

			Map.Textures [Selected] = NewTexture;

			Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[Selected].AlbedoPath, Selected, false);
			Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[Selected].NormalPath, Selected, true);

			Map.SetTextures (Selected);

			ReloadStratums ();
		}
	}

	class StratumTemplate{
		public ScmapEditor.TerrainTexture Stratum0;
		public ScmapEditor.TerrainTexture Stratum1;
		public ScmapEditor.TerrainTexture Stratum2;
		public ScmapEditor.TerrainTexture Stratum3;
		public ScmapEditor.TerrainTexture Stratum4;
		public ScmapEditor.TerrainTexture Stratum5;
		public ScmapEditor.TerrainTexture Stratum6;
		public ScmapEditor.TerrainTexture Stratum7;
		public ScmapEditor.TerrainTexture Stratum8;
		public ScmapEditor.TerrainTexture Stratum9;
	}

	public void ExportStratumTemplate(){
		System.Windows.Forms.SaveFileDialog FolderDialog = new System.Windows.Forms.SaveFileDialog ();

		FolderDialog.Filter = "scmstratum files (*.scmst)|*.scmst|All files (*.*)|*.*"  ;
		FolderDialog.FilterIndex = 0 ;
		FolderDialog.RestoreDirectory = true ;
		FolderDialog.InitialDirectory = EnvPaths.GetMapsPath();

		if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			Debug.Log( FolderDialog.FileName );

			StratumTemplate NewTemplate = new StratumTemplate ();
			NewTemplate.Stratum0 = Map.Textures [0];
			NewTemplate.Stratum1 = Map.Textures [1];
			NewTemplate.Stratum2 = Map.Textures [2];
			NewTemplate.Stratum3 = Map.Textures [3];
			NewTemplate.Stratum4 = Map.Textures [4];
			NewTemplate.Stratum5 = Map.Textures [5];
			NewTemplate.Stratum6 = Map.Textures [6];
			NewTemplate.Stratum7 = Map.Textures [7];
			NewTemplate.Stratum8 = Map.Textures [8];
			NewTemplate.Stratum9 = Map.Textures [9];

			string data = UnityEngine.JsonUtility.ToJson (NewTemplate);

			File.WriteAllText (FolderDialog.FileName, data);
		}
	}

	public void ImportStratumTemplate(){
		System.Windows.Forms.OpenFileDialog FolderDialog = new System.Windows.Forms.OpenFileDialog ();

		FolderDialog.Filter = "scmstratum files (*.scmst)|*.scmst|All files (*.*)|*.*"  ;
		FolderDialog.FilterIndex = 0 ;
		FolderDialog.RestoreDirectory = true ;

		if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			Debug.Log( FolderDialog.FileName );

			string data = File.ReadAllText (FolderDialog.FileName);

			StratumTemplate NewTemplate = UnityEngine.JsonUtility.FromJson<StratumTemplate> (data);

			Map.Textures [0] = NewTemplate.Stratum0;
			Map.Textures [1] = NewTemplate.Stratum1;
			Map.Textures [2] = NewTemplate.Stratum2;
			Map.Textures [3] = NewTemplate.Stratum3;
			Map.Textures [4] = NewTemplate.Stratum4;
			Map.Textures [5] = NewTemplate.Stratum5;
			Map.Textures [6] = NewTemplate.Stratum6;
			Map.Textures [7] = NewTemplate.Stratum7;
			Map.Textures [8] = NewTemplate.Stratum8;
			Map.Textures [9] = NewTemplate.Stratum9;

			//Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[Selected].AlbedoPath, Selected, false);
			//Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[Selected].NormalPath, Selected, true);

			for (int i = 0; i < Map.Textures.Length; i++) {
				Map.Textures[i].AlbedoPath = Map.map.Layers[i].PathTexture;
				Map.Textures[i].NormalPath = Map.map.Layers[i].PathNormalmap;
				if(Map.Textures[i].AlbedoPath.StartsWith("/")){
					Map.Textures[i].AlbedoPath = Map.Textures[i].AlbedoPath.Remove(0, 1);
				}
				if(Map.Textures[i].NormalPath.StartsWith("/")){
					Map.Textures[i].NormalPath = Map.Textures[i].NormalPath.Remove(0, 1);
				}
				Map.Textures[i].AlbedoScale = Map.map.Layers[i].ScaleTexture;
				Map.Textures[i].NormalScale = Map.map.Layers[i].ScaleNormalmap;

				Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[i].AlbedoPath, i, false);
				Map.Gamedata.LoadTextureFromGamedata("env.scd", Map.Textures[i].NormalPath, i, true);

				Map.SetTextures (i);
			}

			ReloadStratums ();
		}
	}
}
