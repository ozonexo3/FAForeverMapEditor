using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;

public class TerrainInfo : MonoBehaviour {

	public		CameraControler		KameraKontroler;
	public		Editing				Edit;
	public		ScmapEditor			Map;
	public		Camera				GameplayCamera;
	public		Slider				BrushSizeSlider;
	public		InputField			BrushSize;
	public		Slider				BrushStrengthSlider;
	public		InputField			BrushStrength;
	public		LayerMask			TerrainMask;
	public		Texture2D[]			Brushes;

	[Header("State")]
	public bool Invert;
	public bool Smooth;

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
	}


	int SelectedBrush = 0;
	public void ChangeBrush(int id){
		SelectedBrush = id;
	}

	int SelectedFalloff = 0;
	public void ChangeFalloff(int id){
		SelectedFalloff = id;
	}


	Vector3 BrushPos;
	Vector3 MouseBeginClick;
	bool UpdateBrushPosition(bool Forced = false){
		//Debug.Log(Vector3.Distance(MouseBeginClick, Input.mousePosition));
		if(Forced || Vector3.Distance(MouseBeginClick, Input.mousePosition) > 10){}
		else{
			return false;
		}


		MouseBeginClick = Input.mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 2000, TerrainMask)){
			BrushPos = hit.point;
			BrushPos.y = Map.Teren.SampleHeight(BrushPos);
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
				Color BrushValue =  Brushes[SelectedFalloff].GetPixel((int)(((i + OffsetDown) / (float)size) * Brushes[0].width), (int)(((j + OffsetLeft) / (float)size) * Brushes[0].height));
				float SambleBrush = BrushValue.r;
				if(Smooth || SelectedBrush == 2){
					float PixelPower = Mathf.Abs( heights[i,j] - CenterHeight);
					heights[i,j] = Mathf.Lerp(heights[i,j], CenterHeight, BrushStrengthSlider.value * 0.1f * Mathf.Pow(SambleBrush, 2) * PixelPower);
				}
				else if(SelectedBrush == 3){
					float PixelPower = heights[i,j] - CenterHeight;
					heights[i,j] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * BrushStrengthSlider.value * 0.01f * Mathf.Pow(SambleBrush, 2);
				}
				else{
					heights[i,j] += SambleBrush * BrushStrengthSlider.value * 0.001f * (Invert?(-1):1);
				}
			}
		}
		// set the new height
		Map.Teren.terrainData.SetHeights(posXInTerrain-offset + OffsetLeft,posYInTerrain-offset + OffsetDown,heights);
	}
}
