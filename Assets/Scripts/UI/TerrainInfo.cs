using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using System.IO;

public class TerrainInfo : MonoBehaviour {

	public		CameraControler		KameraKontroler;
	public		Editing				Edit;
	public		ScmapEditor			Map;
	public		Camera				GameplayCamera;
	public		Slider				BrushSizeSlider;
	public		InputField			BrushSize;
	public		Slider				BrushStrengthSlider;
	public		InputField			BrushStrength;

	public		InputField			BrushMini;
	public		InputField			BrushMax;

	public		GameObject			BrushListObject;
	public		Transform			BrushListPivot;
	public		Material			TerrainMaterial;


	public		LayerMask				TerrainMask;
	public		List<Texture2D>			Brushes;
	public		List<Toggle>			BrushToggles;
	public		ToggleGroup				ToogleGroup;

	[Header("State")]
	public bool Invert;
	public bool Smooth;


	void OnEnable(){
		if(!BrusheshLoaded) LoadBrushesh();
		UpdateMenu();
		TerrainMaterial.SetInt("_Brush", 1);
		Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
		TerrainMaterial.SetTexture("_BrushTex", (Texture)Brushes[SelectedFalloff]);
	}

	void OnDisable(){
		TerrainMaterial.SetInt("_Brush", 0);
	}

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

		string[] AllBrushFiles = Directory.GetFiles(StructurePath);
		Debug.Log("Found files: " + AllBrushFiles.Length + ", from path: " + StructurePath);
		Brushes = new List<Texture2D>();
		BrushToggles = new List<Toggle>();

		for(int i = 0; i < AllBrushFiles.Length; i++){
			byte[] fileData;
			fileData = File.ReadAllBytes(AllBrushFiles[i]);

			Brushes.Add(new Texture2D(512, 512));
			Brushes[Brushes.Count - 1].LoadImage(fileData);

			GameObject NewBrush = Instantiate(BrushListObject) as GameObject;
			NewBrush.transform.SetParent(BrushListPivot, false);
			NewBrush.transform.localScale = Vector3.one;
			string ThisName = AllBrushFiles[i].Replace(StructurePath, "").Replace("/", "").Replace("\\", "");
			BrushToggles.Add( NewBrush.GetComponent<BrushListId>().SetBrushList(ThisName, Brushes[Brushes.Count - 1], Brushes.Count - 1) );
			NewBrush.GetComponent<BrushListId>().Controler = this;
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


	void Update () {
		Invert = Input.GetKey(KeyCode.LeftAlt);
		Smooth = Input.GetKey(KeyCode.LeftShift);


		if(Input.GetKeyDown(KeyCode.M)){
			// Change Strength
		}
		else if(Input.GetKeyDown(KeyCode.B)){
			// Change Size
		}
		else{

		}

		if(Edit.MauseOnGameplay){
			if(Input.GetMouseButtonDown(0)){
				if(UpdateBrushPosition(true)){
					Paint();
				}
			}
			else if(Input.GetMouseButton(0)){
				if(UpdateBrushPosition(false)){
					Paint();
				}
			}
			else{
				UpdateBrushPosition(true);
			}
		}
	}
	public float Min = 0;
	public float Max = 512;
	public void UpdateMenu(bool Slider = false){
		if(Slider){
			BrushSize.text = BrushSizeSlider.value.ToString();
			BrushStrength.text = BrushStrengthSlider.value.ToString();
		}
		else{
			BrushSizeSlider.value = float.Parse(BrushSize.text);
			BrushStrengthSlider.value = int.Parse(BrushStrength.text);
		}

		BrushSizeSlider.value = Mathf.Clamp(BrushSizeSlider.value, 0, 100);
		BrushStrengthSlider.value = (int)Mathf.Clamp(BrushStrengthSlider.value, 0, 100);

		BrushSize.text = BrushSizeSlider.value.ToString();
		BrushStrength.text = BrushStrengthSlider.value.ToString();

		Min = int.Parse(BrushMini.text) / 128f;
		Max = int.Parse(BrushMax.text) / 128f;



		TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value );
	}


	int SelectedBrush = 0;
	public void ChangeBrush(int id){
		SelectedBrush = id;
	}

	int SelectedFalloff = 0;
	public void ChangeFalloff(int id){
		SelectedFalloff = id;
		Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
		TerrainMaterial.SetTexture("_BrushTex", (Texture)Brushes[SelectedFalloff]);
	}


	Vector3 BrushPos;
	Vector3 MouseBeginClick;
	bool UpdateBrushPosition(bool Forced = false){
		//Debug.Log(Vector3.Distance(MouseBeginClick, Input.mousePosition));
		if(Forced || Vector3.Distance(MouseBeginClick, Input.mousePosition) > 5){}
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
			coord.x = (tempCoord.x -  (int)BrushSizeSlider.value * 0.05f) / Map.Teren.terrainData.size.x;
			//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
			coord.z = (tempCoord.z -  (int)BrushSizeSlider.value * 0.05f) / Map.Teren.terrainData.size.z;

			TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value );
			TerrainMaterial.SetFloat("_BrushUvX", coord.x );
			TerrainMaterial.SetFloat("_BrushUvY", coord.z );

			return true;
		}
		return false;
	}


	void Paint(){
		int hmWidth = Map.Teren.terrainData.heightmapWidth;
		int hmHeight = Map.Teren.terrainData.heightmapHeight;

		Vector3 tempCoord = Map.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
		Vector3 coord  = Vector3.zero;
		coord.x = tempCoord.x / Map.Teren.terrainData.size.x;
		//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
		coord.z = tempCoord.z / Map.Teren.terrainData.size.z;
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
		if(posXInTerrain-offset + size > Map.Teren.terrainData.heightmapWidth) OffsetRight = posXInTerrain-offset + size - Map.Teren.terrainData.heightmapWidth;

		// Vertical Brush Offsets
		int OffsetDown = 0;
		if(posYInTerrain-offset < 0) OffsetDown = Mathf.Abs(posYInTerrain-offset);
		int OffsetTop = 0;
		if(posYInTerrain-offset + size > Map.Teren.terrainData.heightmapWidth) OffsetTop = posYInTerrain-offset + size - Map.Teren.terrainData.heightmapWidth;

		float[,] heights = Map.Teren.terrainData.GetHeights(posXInTerrain-offset + OffsetLeft, posYInTerrain-offset + OffsetDown ,(size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop);
		float CenterHeight = 0;

		if(Smooth || SelectedBrush == 2 || SelectedBrush == 3){
			for (int i=0; i<size; i++){
				for (int j=0; j<size; j++){
					CenterHeight += heights[i,j];
				}
			}
			CenterHeight /= size * size;
		}

		for (int i=0; i<(size - OffsetDown) - OffsetTop; i++){
			for (int j=0; j<(size - OffsetLeft) - OffsetRight; j++){
				// Brush strength
				Color BrushValue =  Brushes[SelectedFalloff].GetPixel((int)(((i + OffsetDown) / (float)size) * Brushes[SelectedFalloff].width), (int)(((j + OffsetLeft) / (float)size) * Brushes[SelectedFalloff].height));
				float SambleBrush = BrushValue.r;
				if(SambleBrush >= 0.02f) {
					if(Smooth || SelectedBrush == 2){
						float PixelPower = Mathf.Abs( heights[i,j] - CenterHeight);
						heights[i,j] = Mathf.Lerp(heights[i,j], CenterHeight, BrushStrengthSlider.value * 0.4f * Mathf.Pow(SambleBrush, 2) * PixelPower);
					}
					else if(SelectedBrush == 3){
						float PixelPower = heights[i,j] - CenterHeight;
						heights[i,j] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * BrushStrengthSlider.value * 0.01f * Mathf.Pow(SambleBrush, 2);
					}
					else{
						heights[i,j] += SambleBrush * BrushStrengthSlider.value * 0.0002f * (Invert?(-1):1);
					}

					heights[i,j] = Mathf.Clamp(heights[i,j], Min, Max);
				}
			}
		}
		// set the new height
		Map.Teren.terrainData.SetHeights(posXInTerrain-offset + OffsetLeft,posYInTerrain-offset + OffsetDown,heights);
	}
}
