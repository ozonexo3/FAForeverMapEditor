using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using System.IO;

public class BrushGenerator : MonoBehaviour {


	// This script generate brush textures for all symmetry options
	// Generates positions and textures used to paint

	public	static	List<Texture2D>			Brushes;
	public static List<string> BrushesNames;


	public static 		Texture2D 			RotatedBrush;
	public static 		Texture2D[] 		PaintImage;
	public static 		Vector3[]			PaintPositions;
	static 				Vector3 			BrushPos;
	public static 		int 				LastSym = 0;


	#region LoadBrushesh

	/// <summary>
	/// Loads all brush textures from structure brush folder
	/// </summary>
	public static void LoadBrushesh(){
		
		string StructurePath = MapLuaParser.StructurePath + "brush";

		if(!Directory.Exists(StructurePath)){
			Debug.LogError("Cant find brush folder");
			return;
		}

		string[] AllBrushFiles = Directory.GetFiles(StructurePath);
		//Debug.Log("Found files: " + AllBrushFiles.Length + ", from path: " + StructurePath);
		Brushes = new List<Texture2D>();
		BrushesNames = new List<string> ();

		for(int i = 0; i < AllBrushFiles.Length; i++){
			byte[] fileData;
			fileData = File.ReadAllBytes(AllBrushFiles[i]);

			Brushes.Add(new Texture2D(512, 512, TextureFormat.RGBA32, false));
			Brushes[Brushes.Count - 1].LoadImage(fileData);
			BrushesNames.Add (AllBrushFiles [i].Replace (StructurePath, "").Replace ("/", "").Replace ("\\", ""));
		}
	}


	#endregion

	#region Symmetry
	// Generate positions - need to be done before paint
	public static void GenerateSymmetry(Vector3 Pos){
		BrushPos = Pos;
		switch(LastSym){
		case 1:
			PaintPositions = new Vector3[2];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetHorizonalSymetry();
			break;
		case 2:
			PaintPositions = new Vector3[2];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetVerticalSymetry();
			break;
		case 3:
			PaintPositions = new Vector3[2];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetHorizontalVerticalSymetry();
			break;
		case 4:
			PaintPositions = new Vector3[4];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetHorizonalSymetry();
			PaintPositions[2] = GetVerticalSymetry();
			PaintPositions[3] = GetHorizontalVerticalSymetry();
			break;
		case 5:
			PaintPositions = new Vector3[2];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetDiagonal1Symetry();
			break;
		case 6:
			PaintPositions = new Vector3[2];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetDiagonal2Symetry();
			break;
		case 7:
			PaintPositions = new Vector3[4];
			PaintPositions[0] = BrushPos;
			PaintPositions[1] = GetDiagonal1Symetry();
			PaintPositions[2] = GetDiagonal2Symetry();
			PaintPositions[3] = GetDiagonal3Symetry();
			break;
		case 8:
			int Count = PlayerPrefs.GetInt("SymmetryAngleCount", 2);
			PaintPositions = new Vector3[Count];
			PaintPositions[0] = BrushPos;
			float angle = 360.0f / (float)Count;
			for(int i = 1; i < Count; i++){
				PaintPositions[i] = GetRotationSymetry(angle * i);
			}
			break;
		default:
			PaintPositions = new Vector3[1];
			PaintPositions[0] = BrushPos;
			break;
		}
	}


	// Generate brush textures for all symmetry - need to be done only once, when changing symmetry
	// It mirrors and rotate brush texture
	// Generating textures is slow and make lag when generating
	// Need to find something to speed it up
	public static void GeneratePaintBrushesh(){
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
			PaintImage = new Texture2D[1];
			PaintImage[0] = RotatedBrush;
			break;
		}
	}
	#endregion



//***************************** MATH
	#region Math

	static Vector3 GetHorizonalSymetry(){
		Vector3 MirroredPos = BrushPos - MapLuaParser.Current.MapCenterPoint;
		MirroredPos.x = -MirroredPos.x;
		MirroredPos += MapLuaParser.Current.MapCenterPoint;
		return MirroredPos;
	}

	static Vector3 GetVerticalSymetry(){
		Vector3 MirroredPos = BrushPos - MapLuaParser.Current.MapCenterPoint;
		MirroredPos.z = -MirroredPos.z;
		MirroredPos += MapLuaParser.Current.MapCenterPoint;
		return MirroredPos;
	}

	static Vector3 GetHorizontalVerticalSymetry(){
		Vector3 MirroredPos = BrushPos - MapLuaParser.Current.MapCenterPoint;
		MirroredPos.z = -MirroredPos.z;
		MirroredPos.x = -MirroredPos.x;
		MirroredPos += MapLuaParser.Current.MapCenterPoint;
		return MirroredPos;
	}

	static Vector3 GetDiagonal1Symetry(){
		Vector3 Origin = new Vector3(0, 0, - MapLuaParser.Current.ScenarioData.Size.y / 10f);
		Vector3 Origin2 = new Vector3( MapLuaParser.Current.ScenarioData.Size.y / 10f, 0, 0);
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

	static Vector3 GetDiagonal2Symetry(){
		Vector3 Origin = new Vector3(0, 0, 0);
		Vector3 Origin2 = new Vector3(MapLuaParser.Current.ScenarioData.Size.y / 10f, 0, -MapLuaParser.Current.ScenarioData.Size.y / 10f);
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

	static Vector3 GetDiagonal3Symetry(){
		Vector3 Origin = new Vector3(0, 0, - MapLuaParser.Current.ScenarioData.Size.y / 10f);
		Vector3 Origin2 = new Vector3( MapLuaParser.Current.ScenarioData.Size.y / 10f, 0, 0);
		Vector3 Point = new Vector3( BrushPos.x, 0, BrushPos.z);

		Vector3 PointOfMirror = EditingMarkers.ClosestPointToLine(Origin, Origin2, Point);
		Vector3 FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		float FinalDist = Vector3.Distance(PointOfMirror, Point);
		Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;
		MirroredPos.y = BrushPos.y;



		Origin = new Vector3(0, 0, 0);
		Origin2 = new Vector3(MapLuaParser.Current.ScenarioData.Size.y / 10f, 0, -MapLuaParser.Current.ScenarioData.Size.y / 10f);
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

	static Vector3 GetRotationSymetry(float angle){
		Vector3 MirroredPos = BrushPos - MapLuaParser.Current.MapCenterPoint;
		MirroredPos = EditingMarkers.RotatePointAroundPivot(BrushPos, MapLuaParser.Current.MapCenterPoint, angle);
		return MirroredPos;
	}




	public static Texture2D rotateTexture(Texture2D tex, float angle)
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
		Color[] AllPixels = new Color[w * h];
		for (x = 0; x < tex.width; x++) {
			x2 = x1;
			y2 = y1;
			for ( y = 0; y < tex.height; y++) {
				//rotImage.SetPixel (x1, y1, Color.clear);          

				x2 += dx_x;//rot_x(angle, x1, y1);
				y2 += dx_y;//rot_y(angle, x1, y1);


				AllPixels [x + y * w] = getPixel (tex, x2, y2);
				//rotImage.SetPixel ( (int)Mathf.Floor(y), (int)Mathf.Floor(x), getPixel(tex,x2, y2));
			}

			x1 += dy_x;
			y1 += dy_y;

		}
		rotImage.SetPixels (AllPixels);
		rotImage.Apply();
		return rotImage;
	}

	private static Color getPixel(Texture2D tex, float x, float y)
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

	private static float rot_x (float angle, float x, float y) {
		float cos = Mathf.Cos(angle/180.0f*Mathf.PI);
		float sin = Mathf.Sin(angle/180.0f*Mathf.PI);
		return (x * cos + y * (-sin));
	}
	private static float rot_y (float angle, float x, float y) {
		float cos = Mathf.Cos(angle/180.0f*Mathf.PI);
		float sin = Mathf.Sin(angle/180.0f*Mathf.PI);
		return (x * sin + y * cos);
	}


	static Texture2D MirrorTexture(Texture2D tex, bool horizontal, bool vertical)
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
	#endregion
}
