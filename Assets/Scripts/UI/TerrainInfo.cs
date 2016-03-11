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
			Debug.Log(Vector3.Distance(MouseBeginClick, Input.mousePosition));
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

		float[,] heights = Map.Teren.terrainData.GetHeights(posXInTerrain-offset,posYInTerrain-offset,size,size);
		// we set each sample of the terrain in the size to the desired height
		for (int i=0; i<size; i++){
			for (int j=0; j<size; j++){
				// Brush strength
				Color BrushValue =  Brushes[SelectedFalloff].GetPixel((int)((i / (float)size) * Brushes[0].width), (int)((j / (float)size) * Brushes[0].height));
				float SambleBrush = BrushValue.r;
				heights[i,j] += SambleBrush * BrushStrengthSlider.value * 0.001f * (Invert?(-1):1);
			}
		}
		// set the new height
		Map.Teren.terrainData.SetHeights(posXInTerrain-offset,posYInTerrain-offset,heights);
	}
}
