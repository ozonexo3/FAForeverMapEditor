using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EditMap;
using System.IO;

public class TerrainInfo : MonoBehaviour {

	public		CameraControler		KameraKontroler;
	public		Editing				Edit;
	public		ScmapEditor			Map;
	public		MarkersRenderer		Markers;
	public		Camera				GameplayCamera;
	public		Slider				BrushSizeSlider;
	public		InputField			BrushSize;
	public		Slider				BrushStrengthSlider;
	public		InputField			BrushStrength;
	public		Slider				BrushRotationSlider;
	public		InputField			BrushRotation;

	public		InputField			BrushMini;
	public		InputField			BrushMax;

	public		InputField			TerrainSet;
	public		InputField			TerrainAdd;
	public		InputField			TerrainScale;
	public		Toggle				TerrainScale_Height;
	public		InputField			TerrainScale_HeightValue;

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
		Brushes[SelectedFalloff].mipMapBias = -1f;
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
		//Debug.Log("Found files: " + AllBrushFiles.Length + ", from path: " + StructurePath);
		Brushes = new List<Texture2D>();
		BrushToggles = new List<Toggle>();

		for(int i = 0; i < AllBrushFiles.Length; i++){
			byte[] fileData;
			fileData = File.ReadAllBytes(AllBrushFiles[i]);

			Brushes.Add(new Texture2D(512, 512, TextureFormat.RGBA32, false));
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

	bool TerainChanged = false;
	float[,] beginHeights;

	Vector3 BeginMousePos;
	float StrengthBeginValue;
	bool ChangingStrength;
	float SizeBeginValue;
	bool ChangingSize;
	void Update () {
		Invert = Input.GetKey(KeyCode.LeftAlt);
		Smooth = Input.GetKey(KeyCode.LeftShift);

		if(Edit.MauseOnGameplay || ChangingStrength || ChangingSize){
			if(!ChangingSize && (Input.GetKey(KeyCode.M) || ChangingStrength)){
				// Change Strength
				if(Input.GetMouseButtonDown(0)){
					ChangingStrength = true;
					BeginMousePos = Input.mousePosition;
					StrengthBeginValue = BrushStrengthSlider.value;
				}
				else if(Input.GetMouseButtonUp(0)){
					ChangingStrength = false;
				}
				if(ChangingStrength){
					BrushStrengthSlider.value = Mathf.Clamp(StrengthBeginValue - (BeginMousePos.x - Input.mousePosition.x), 0, 100);
					UpdateMenu(true);
					//UpdateBrushPosition(true);

				}
			}
			else if(Input.GetKey(KeyCode.B) || ChangingSize){
				// Change Size
				if(Input.GetMouseButtonDown(0)){
					ChangingSize = true;
					BeginMousePos = Input.mousePosition;
					SizeBeginValue = BrushSizeSlider.value;
				}
				else if(Input.GetMouseButtonUp(0)){
					ChangingSize = false;
				}
				if(ChangingSize){
					BrushSizeSlider.value = Mathf.Clamp(SizeBeginValue - (BeginMousePos.x - Input.mousePosition.x), 1, 256);
					UpdateMenu(true);
					//UpdateBrushPosition(true);

				}
			}
			else{
				if(Input.GetMouseButtonDown(0)){
					if(UpdateBrushPosition(true)){
						SymmetryPaint();
					}
				}
				else if(Input.GetMouseButton(0)){
					if(UpdateBrushPosition(false)){
						SymmetryPaint();
					}
				}
				else{
					UpdateBrushPosition(true);
				}
			}
		}

		if(TerainChanged && Input.GetMouseButtonUp(0)){
			Map.Scenario.History.RegisterTerrainHeightmapChange(beginHeights);
			TerainChanged = false;
		}

		if(PlayerPrefs.GetInt("Symmetry", 0) != LastSym){
			GeneratePaintBrushesh();
		}
	}
	public float Min = 0;
	public float Max = 512;
	int LastRotation = 0;
	public void UpdateMenu(bool Slider = false){
		if(Slider){
			BrushSize.text = BrushSizeSlider.value.ToString();
			BrushStrength.text = BrushStrengthSlider.value.ToString();
			//BrushRotation.text = BrushRotationSlider.value.ToString();
		}
		else{
			BrushSizeSlider.value = float.Parse(BrushSize.text);
			BrushStrengthSlider.value = int.Parse(BrushStrength.text);
			//BrushRotationSlider.value = int.Parse(BrushRotation.text);
		}

		BrushSizeSlider.value = Mathf.Clamp(BrushSizeSlider.value, 1, 256);
		BrushStrengthSlider.value = (int)Mathf.Clamp(BrushStrengthSlider.value, 0, 100);
		//BrushRotationSlider.value = (int)Mathf.Clamp(BrushStrengthSlider.value, -360, 360);

		BrushSize.text = BrushSizeSlider.value.ToString();
		BrushStrength.text = BrushStrengthSlider.value.ToString();
		//BrushRotation.text = BrushRotationSlider.value.ToString();

		Min = int.Parse(BrushMini.text) / 128f;
		Max = int.Parse(BrushMax.text) / 128f;

		if(LastRotation != int.Parse(BrushRotation.text)){
			LastRotation = int.Parse( BrushRotation.text);
			if(LastRotation == 0){
				RotatedBrush = Brushes[SelectedFalloff];
			}
			else{
				RotatedBrush = rotateTexture(Brushes[SelectedFalloff], LastRotation);
			}

			TerrainMaterial.SetTexture("_BrushTex", (Texture)RotatedBrush);
			GeneratePaintBrushesh();
		}
		TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value );
	}

	public void SetTerrainHeight(){
		int h = Map.Teren.terrainData.heightmapHeight;
		int w = Map.Teren.terrainData.heightmapWidth;
		beginHeights = Map.Teren.terrainData.GetHeights(0,0, w, h);
		Map.Scenario.History.RegisterTerrainHeightmapChange(beginHeights);

		float[,] heights = Map.Teren.terrainData.GetHeights(0, 0, Map.Teren.terrainData.heightmapWidth, Map.Teren.terrainData.heightmapHeight);

		for(int i = 0; i < Map.Teren.terrainData.heightmapWidth; i++){
			for(int j = 0; j < Map.Teren.terrainData.heightmapWidth; j++){
				heights[i,j] = int.Parse(TerrainAdd.text) / 128f;
			}
		}
		Map.Teren.terrainData.SetHeights(0, 0, heights);
	}

	public void AddTerrainHeight(){
		int h = Map.Teren.terrainData.heightmapHeight;
		int w = Map.Teren.terrainData.heightmapWidth;
		beginHeights = Map.Teren.terrainData.GetHeights(0,0, w, h);
		Map.Scenario.History.RegisterTerrainHeightmapChange(beginHeights);

		float[,] heights = Map.Teren.terrainData.GetHeights(0, 0, Map.Teren.terrainData.heightmapWidth, Map.Teren.terrainData.heightmapHeight);

		for(int i = 0; i < Map.Teren.terrainData.heightmapWidth; i++){
			for(int j = 0; j < Map.Teren.terrainData.heightmapWidth; j++){
				heights[i,j] += int.Parse(TerrainAdd.text) / 128f;
			}
		}
		Map.Teren.terrainData.SetHeights(0, 0, heights);
	}

	public void ExportHeightmap(){
		string Filename = PlayerPrefs.GetString("MapsPath", "maps/") + Map.Scenario.FolderName + "/heightmap.raw";

		int h = Map.Teren.terrainData.heightmapHeight;
		int w = Map.Teren.terrainData.heightmapWidth;

		float[,] data = Map.Teren.terrainData.GetHeights(0, 0, w, h);

		using (BinaryWriter writer = new BinaryWriter(new System.IO.FileStream(Filename, System.IO.FileMode.Create)))
		{
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					uint ThisPixel = (uint)(data[y,x] * 0xFFFF);
					writer.Write(System.BitConverter.GetBytes(System.BitConverter.ToUInt16(System.BitConverter.GetBytes(ThisPixel),0)));
				}
			}
			writer.Close();
		}
	}

	public void ExportWithSizeHeightmap(){

		int scale =  int.Parse(TerrainScale.text);
		scale = Mathf.Clamp(scale, 129, 2049);

		string Filename = PlayerPrefs.GetString("MapsPath", "maps/") + Map.Scenario.FolderName + "/heightmap.raw";

		int h = Map.Teren.terrainData.heightmapWidth;
		int w = Map.Teren.terrainData.heightmapWidth;
		bool ScaleUp = scale > Map.Teren.terrainData.heightmapWidth;

		float[,] data = Map.Teren.terrainData.GetHeights(0, 0, w, h);

		Texture2D ExportAs = new Texture2D(Map.Teren.terrainData.heightmapWidth, Map.Teren.terrainData.heightmapWidth, TextureFormat.RGB24, false);
		Debug.Log(data[128,128]);
		//Debug.Log(data[256,256]);

		float Prop = (float)scale / (float)Map.Teren.terrainData.heightmapWidth;
		float HeightValue = 1;
		HeightValue = float.Parse(TerrainScale_HeightValue.text);
		if(HeightValue < 0) HeightValue = 1;

		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				//Debug.Log(data[y,x]);
				float Value = data[y,x] / (1f / 255f);

				if( TerrainScale_Height.isOn){
							Value *= HeightValue;
				}
				float ColorR = (Mathf.Floor(Value) * (1f / 255f));
				float ColorG = (Value - Mathf.Floor(Value));

				if(x == 128 && y == 128){
					Debug.Log(Value);
					Debug.Log(ColorR +", "+ ColorG);
				}

				ExportAs.SetPixel(h - y - 1, x, new Color(ColorR, ColorG, 0));
			}
		}
		ExportAs.Apply();

		Debug.Log(ExportAs.GetPixel(128, 128).r +", "+ ExportAs.GetPixel(128, 128).g);
		Debug.Log(ExportAs.GetPixel(128, 128).r + ExportAs.GetPixel(128, 128).g * (1f / 255f));

		TextureScale.Bilinear(ExportAs, scale, scale);

		h = scale;
		w = scale;
		Debug.Log(Prop);
		//ExportAs.Resize(scale, scale);
		//ExportAs.Apply();

		using (BinaryWriter writer = new BinaryWriter(new System.IO.FileStream(Filename, System.IO.FileMode.Create)))
		{
			for (int y = 0; y < h; y++)
			{
				float Ylerp = 0;
				for (int x = 0; x < w; x++)
				{
					Color pixel =  ExportAs.GetPixel(y,x);
					float value = (pixel.r + pixel.g * (1f / 255f));
					uint ThisPixel = (uint)( value * 0xFFFF);
					writer.Write(System.BitConverter.GetBytes(System.BitConverter.ToUInt16(System.BitConverter.GetBytes(ThisPixel),0)));
				}
			}
			writer.Close();
		}
		ExportAs = null;

	}

	public void ImportHeightmap(){

		int h = Map.Teren.terrainData.heightmapHeight;
		int w = Map.Teren.terrainData.heightmapWidth;
		beginHeights = Map.Teren.terrainData.GetHeights(0,0, w, h);
		Map.Scenario.History.RegisterTerrainHeightmapChange(beginHeights);

		string Filename = PlayerPrefs.GetString("MapsPath", "maps/") + Map.Scenario.FolderName + "/heightmap.raw";
		if(!File.Exists(Filename)){
			Debug.Log("File not exist: " + Filename);
			return;
		}
			
		float[,] data = new float[h, w];
		using (var file = System.IO.File.OpenRead(  Filename))
		using (var reader = new System.IO.BinaryReader(file))
		{
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					float v = (float)reader.ReadUInt16() / 0xFFFF;
					data[y, x] = v;
				}
			}
		}
		Map.Teren.terrainData.SetHeights(0, 0, data);
	}

	int SelectedBrush = 0;
	public void ChangeBrush(int id){
		SelectedBrush = id;
	}

	int SelectedFalloff = 0;
	public void ChangeFalloff(int id){
		SelectedFalloff = id;
		Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
		Brushes[SelectedFalloff].mipMapBias = -1f;
		LastRotation = int.Parse( BrushRotation.text);
		if(LastRotation == 0){
			RotatedBrush = Brushes[SelectedFalloff];
		}
		else{
			RotatedBrush = rotateTexture(Brushes[SelectedFalloff], LastRotation);
		}
		TerrainMaterial.SetTexture("_BrushTex", (Texture)RotatedBrush);
		GeneratePaintBrushesh();
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


	void SymmetryPaint(){
		Paint(BrushPos, 0);

		int SymmetryCode = PlayerPrefs.GetInt("Symmetry", 0);

		if( SymmetryCode == 0){}
		else if(SymmetryCode == 1) Paint(GetHorizonalSymetry(), 1);
		else if(SymmetryCode == 2) Paint(GetVerticalSymetry(), 1);
		else if(SymmetryCode == 3) Paint(GetHorizontalVerticalSymetry(), 1);
		else if(SymmetryCode == 4){
			Paint(GetHorizonalSymetry(), 1);
			Paint(GetVerticalSymetry(), 2);
			Paint(GetHorizontalVerticalSymetry(), 3);
		}
		else if(SymmetryCode == 5) Paint(GetDiagonal1Symetry(), 1);
		else if(SymmetryCode == 6) Paint(GetDiagonal2Symetry(), 1);
		else if(SymmetryCode == 7){
			Paint(GetDiagonal1Symetry(), 1);
			Paint(GetDiagonal2Symetry(), 2);
			Paint(GetDiagonal3Symetry(), 3);
		}
		else if(SymmetryCode == 8){
			int Count = PlayerPrefs.GetInt("SymmetryAngleCount", 2);
			float angle = 360.0f / (float)Count;
			for(int i = 0; i < Count - 1; i++){
				Paint(GetRotationSymetry(angle + angle * i), i + 1);
			}
		}

	}

	Vector3 GetHorizonalSymetry(){
		Vector3 MirroredPos = BrushPos - Map.Scenario.MapCenterPoint;
		MirroredPos.x = -MirroredPos.x;
		MirroredPos += Map.Scenario.MapCenterPoint;
		return MirroredPos;
	}

	Vector3 GetVerticalSymetry(){
		Vector3 MirroredPos = BrushPos - Map.Scenario.MapCenterPoint;
		MirroredPos.z = -MirroredPos.z;
		MirroredPos += Map.Scenario.MapCenterPoint;
		return MirroredPos;
	}

	Vector3 GetHorizontalVerticalSymetry(){
		Vector3 MirroredPos = BrushPos - Map.Scenario.MapCenterPoint;
		MirroredPos.z = -MirroredPos.z;
		MirroredPos.x = -MirroredPos.x;
		MirroredPos += Map.Scenario.MapCenterPoint;
		return MirroredPos;
	}

	Vector3 GetDiagonal1Symetry(){
		Vector3 Origin = new Vector3(0, 0, - Map.Scenario.ScenarioData.Size.y / 10f);
		Vector3 Origin2 = new Vector3( Map.Scenario.ScenarioData.Size.y / 10f, 0, 0);
		Vector3 Point = new Vector3( BrushPos.x, 0, BrushPos.z);

		Vector3 PointOfMirror = EditingMarkers.ClosestPointToLine(Origin, Origin2, Point);
		Vector3 FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		float FinalDist = Vector3.Distance(PointOfMirror, Point);
		Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;
		MirroredPos.y = BrushPos.y;
		return MirroredPos;
	}

	Vector3 GetDiagonal2Symetry(){
		Vector3 Origin = new Vector3(0, 0, 0);
		Vector3 Origin2 = new Vector3(Map.Scenario.ScenarioData.Size.y / 10f, 0, -Map.Scenario.ScenarioData.Size.y / 10f);
		Vector3 Point = new Vector3( BrushPos.x, 0, BrushPos.z);

		Vector3 PointOfMirror = EditingMarkers.ClosestPointToLine(Origin, Origin2, Point);
		Vector3 FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		float FinalDist = Vector3.Distance(PointOfMirror, Point);
		Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;
		MirroredPos.y = BrushPos.y;
		return MirroredPos;
	}

	Vector3 GetDiagonal3Symetry(){
		Vector3 Origin = new Vector3(0, 0, - Map.Scenario.ScenarioData.Size.y / 10f);
		Vector3 Origin2 = new Vector3( Map.Scenario.ScenarioData.Size.y / 10f, 0, 0);
		Vector3 Point = new Vector3( BrushPos.x, 0, BrushPos.z);

		Vector3 PointOfMirror = EditingMarkers.ClosestPointToLine(Origin, Origin2, Point);
		Vector3 FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		float FinalDist = Vector3.Distance(PointOfMirror, Point);
		Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;
		MirroredPos.y = BrushPos.y;



		Origin = new Vector3(0, 0, 0);
		Origin2 = new Vector3(Map.Scenario.ScenarioData.Size.y / 10f, 0, -Map.Scenario.ScenarioData.Size.y / 10f);
		Point = new Vector3( MirroredPos.x, 0, MirroredPos.z);

		PointOfMirror = EditingMarkers.ClosestPointToLine(Origin, Origin2, Point);
		FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		FinalDist = Vector3.Distance(PointOfMirror, Point);
		MirroredPos = PointOfMirror + FinalDir * FinalDist;
		MirroredPos.y = BrushPos.y;

		return MirroredPos;
	}

	Vector3 GetRotationSymetry(float angle){
		Vector3 MirroredPos = BrushPos - Map.Scenario.MapCenterPoint;
		MirroredPos = EditingMarkers.RotatePointAroundPivot(BrushPos, Map.Scenario.MapCenterPoint, angle);
		return MirroredPos;
	}

	Texture2D RotatedBrush;
	Texture2D[] PaintImage;
	int LastSym = 0;
	void GeneratePaintBrushesh(){
		int SymmetryCode = PlayerPrefs.GetInt("Symmetry", 0);
		LastSym = SymmetryCode;
		switch(SymmetryCode){
		case 1:
			PaintImage = new Texture2D[2];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = MirrorTexture(RotatedBrush, true, false);
			break;
		case 2:
			PaintImage = new Texture2D[2];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = MirrorTexture(RotatedBrush, false, true);
			break;
		case 3:
			PaintImage = new Texture2D[2];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = MirrorTexture(RotatedBrush, true, true);
			break;
		case 4:
			PaintImage = new Texture2D[4];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = MirrorTexture(RotatedBrush, true, false);
			PaintImage[2] = MirrorTexture(RotatedBrush, false, true);
			PaintImage[3] = MirrorTexture(RotatedBrush, true, true);
			break;
		case 5:
			PaintImage = new Texture2D[2];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = rotateTexture(RotatedBrush, 90);
			PaintImage[1] = MirrorTexture(PaintImage[1], false, true);
			break;
		case 6:
			PaintImage = new Texture2D[2];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = rotateTexture(RotatedBrush, 90);
			PaintImage[1] = MirrorTexture(PaintImage[1], true, false);
			break;
		case 7:
			PaintImage = new Texture2D[4];
			PaintImage[0] = RotatedBrush;
			PaintImage[1] = rotateTexture(RotatedBrush, 90);
			PaintImage[1] = MirrorTexture(PaintImage[1], false, true);
			PaintImage[2] = rotateTexture(RotatedBrush, 90);
			PaintImage[2] = MirrorTexture(PaintImage[2], true, false);

			PaintImage[3] = MirrorTexture(RotatedBrush, true, true);
			break;
		case 8:
			int Count = PlayerPrefs.GetInt("SymmetryAngleCount", 2);
			PaintImage = new Texture2D[Count + 1];
			PaintImage[0] = RotatedBrush;
			float angle = 360.0f / (float)Count;
			for(int i = 1; i < Count; i++){
				PaintImage[i] = rotateTexture(RotatedBrush, angle * i);
			}
			break;
		default:
			Debug.Log("load");
			PaintImage = new Texture2D[1];
			PaintImage[0] = RotatedBrush;
			break;
		}
	}


	void Paint(Vector3 AtPosition, int id = 0){
		int hmWidth = Map.Teren.terrainData.heightmapWidth;
		int hmHeight = Map.Teren.terrainData.heightmapHeight;

		Vector3 tempCoord = Map.Teren.gameObject.transform.InverseTransformPoint(AtPosition);
		Vector3 coord  = Vector3.zero;
		coord.x = tempCoord.x / Map.Teren.terrainData.size.x;
		//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
		coord.z = tempCoord.z / Map.Teren.terrainData.size.z;

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
		if(posXInTerrain-offset + size > Map.Teren.terrainData.heightmapWidth) OffsetRight = posXInTerrain-offset + size - Map.Teren.terrainData.heightmapWidth;

		// Vertical Brush Offsets
		int OffsetDown = 0;
		if(posYInTerrain-offset < 0) OffsetDown = Mathf.Abs(posYInTerrain-offset);
		int OffsetTop = 0;
		if(posYInTerrain-offset + size > Map.Teren.terrainData.heightmapWidth) OffsetTop = posYInTerrain-offset + size - Map.Teren.terrainData.heightmapWidth;

		float[,] heights = Map.Teren.terrainData.GetHeights(posXInTerrain-offset + OffsetLeft, posYInTerrain-offset + OffsetDown ,(size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop);
		float CenterHeight = 0;

		if(Smooth || SelectedBrush == 2 || SelectedBrush == 3){
			for (int i=0; i<(size - OffsetDown) - OffsetTop; i++){
				for (int j=0; j<(size - OffsetLeft) - OffsetRight; j++){
					CenterHeight += heights[i,j];
				}
			}
			CenterHeight /= size * size;
		}

		for (int i=0; i<(size - OffsetDown) - OffsetTop; i++){
			for (int j=0; j<(size - OffsetLeft) - OffsetRight; j++){
				// Brush strength
				int x = (int)(((i + OffsetDown) / (float)size) * PaintImage[id].width);
				int y = (int)(((j + OffsetLeft) / (float)size) * PaintImage[id].height);
				Color BrushValue =  PaintImage[id].GetPixel(y, x);
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
		if(!TerainChanged){
			beginHeights = Map.Teren.terrainData.GetHeights(0,0, hmWidth, hmHeight);
			TerainChanged = true;
		}

		Map.Teren.terrainData.SetHeights(posXInTerrain-offset + OffsetLeft, posYInTerrain-offset + OffsetDown,heights);
		Markers.UpdateMarkersHeights();
	}







		Texture2D rotateTexture(Texture2D tex, float angle)
		{
		Texture2D rotImage = new Texture2D(tex.width, tex.height, tex.format, false);
		rotImage.wrapMode = TextureWrapMode.Clamp;
			int  x,y;
			float x1, y1, x2,y2;

			int w = tex.width;
			int h = tex.height;
			float x0 = rot_x (angle, -w/2.0f, -h/2.0f) + w/2.0f;
			float y0 = rot_y (angle, -w/2.0f, -h/2.0f) + h/2.0f;

			float dx_x = rot_x (angle, 1.0f, 0.0f);
			float dx_y = rot_y (angle, 1.0f, 0.0f);
			float dy_x = rot_x (angle, 0.0f, 1.0f);
			float dy_y = rot_y (angle, 0.0f, 1.0f);


			x1 = x0;
			y1 = y0;

			for (x = 0; x < tex.width; x++) {
				x2 = x1;
				y2 = y1;
				for ( y = 0; y < tex.height; y++) {
					//rotImage.SetPixel (x1, y1, Color.clear);          

					x2 += dx_x;//rot_x(angle, x1, y1);
					y2 += dx_y;//rot_y(angle, x1, y1);
					rotImage.SetPixel ( (int)Mathf.Floor(y), (int)Mathf.Floor(x), getPixel(tex,x2, y2));
				}

				x1 += dy_x;
				y1 += dy_y;

			}

			rotImage.Apply();
			return rotImage;
		}

		private Color getPixel(Texture2D tex, float x, float y)
		{
			Color pix;
			int x1 = (int) Mathf.Floor(x);
			int y1 = (int) Mathf.Floor(y);

			if(x1 > tex.width || x1 < 0 ||
				y1 > tex.height || y1 < 0) {
				pix = Color.clear;
			} else {
				pix = tex.GetPixel(x1,y1);
			}

			return pix;
		}

		private float rot_x (float angle, float x, float y) {
			float cos = Mathf.Cos(angle/180.0f*Mathf.PI);
			float sin = Mathf.Sin(angle/180.0f*Mathf.PI);
			return (x * cos + y * (-sin));
		}
		private float rot_y (float angle, float x, float y) {
			float cos = Mathf.Cos(angle/180.0f*Mathf.PI);
			float sin = Mathf.Sin(angle/180.0f*Mathf.PI);
			return (x * sin + y * cos);
		}

		
	Texture2D MirrorTexture(Texture2D tex, bool horizontal, bool vertical)
	{
		Texture2D rotImage = new Texture2D(tex.width, tex.height, tex.format, false);
		rotImage.wrapMode = TextureWrapMode.Clamp;
		int  x,y;
		int x1, y1;
		for (x = 0; x < tex.width; x++) {
			for ( y = 0; y < tex.height; y++) {
				if(horizontal) x1 = tex.width - x - 1;
				else x1 = x;

				if(vertical) y1 = tex.height - y - 1;
				else y1 = y;

				rotImage.SetPixel ( x, y, getPixel(tex,x1, y1));
			}
		}
		rotImage.Apply();
		return rotImage;
	}
}
