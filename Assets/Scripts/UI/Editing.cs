using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Editing : MonoBehaviour {

	public		MapLuaParser		Scenario;
	public		CameraControler		KameraKontroler;
	public		GameObject[]		Categorys;
	public		GameObject[]		CategorysSelected;
	public		EditStates			State = EditStates.MapStat;
	public		bool				MauseOnGameplay;
	public		MarkersList			AllMarkersList;


	public		List<GameObject>		Selected = new List<GameObject>();
	public		List<GameObject>		MirrorSelected = new List<GameObject>();
	public		SymmetrySelection[]		SymmetrySelectionList = new SymmetrySelection[0];
	public		Vector3[]				SelectedStartPos;

	public		Transform			HudElements;
	public		Transform			SelectedMarker;
	public		Transform			SelectedReflectionMarker;
	public		List<Transform>		SelectedSymmetryMarkers = new List<Transform>();
	public		Vector3				SelectedMarkerBeginClickPos;
	public		Vector3				SelectedMarkerBeginPos;
	public		GameObject			SelectionSymmetryPrefab;

	public		Texture[]			SelectionSizeTextures;
	public		List<GameObject>	SelectionsRings = new List<GameObject>();
	public		GameObject			RingSelectionPrefab;
	public		InputField			ToolbarTolerance;
	public		float				MirrorTolerance;

	public		Toggle				ToogleMirrorX;
	public		Toggle				ToogleMirrorZ;
	public		Toggle				ToogleMirror90;
	public		Toggle				ToogleMirror270;
	public		Toggle				ToogleMirror180;

	[System.Serializable]
	public class SymmetrySelection{
		public		Vector3					MoveMultiply = Vector3.one;
		public		float					MoveRotation = 0;
		public		List<GameObject>		MirrorSelected = new List<GameObject>();
		public		Vector3[]				SelectedStartPos;
		public		Vector3					SelectedMarkerBeginClickPos;
	}

	public enum EditStates{
		MapStat, TerrainStat, TexturesStat, LightingStat, MarkersStat, DecalsStat, PropsStat, AIStat
	}

	void OnEnable(){
		ChangeCategory(0);
		State = EditStates.MapStat;
		MirrorTolerance = 0.5f;
		ToolbarTolerance.text = MirrorTolerance + "";
	}

	public void ButtonFunction(string func){
		switch(func){
		case "Save":
			Scenario.StartCoroutine("SaveMap");
			break;
		case "Map":
			State = EditStates.MapStat;
			ChangeCategory(0);
			break;
		case "Terrain":
			State = EditStates.TerrainStat;
			ChangeCategory(1);
			break;
		case "Textures":
			State = EditStates.TexturesStat;
			ChangeCategory(2);
			break;
		case "Lighting":
			State = EditStates.LightingStat;
			ChangeCategory(3);
			break;
		case "Markers":
			State = EditStates.MarkersStat;
			ChangeCategory(4);
			KameraKontroler.AllWorkingObjects = new List<GameObject>();
			foreach(MapLuaParser.Army obj in Scenario.ARMY_){
				KameraKontroler.AllWorkingObjects.Add(obj.Mark.gameObject);
			}
			foreach(MapLuaParser.Mex obj in Scenario.Mexes){
				KameraKontroler.AllWorkingObjects.Add(obj.Mark.gameObject);
			}
			foreach(MapLuaParser.Hydro obj in Scenario.Hydros){
				KameraKontroler.AllWorkingObjects.Add(obj.Mark.gameObject);
			}
			foreach(MapLuaParser.Marker obj in Scenario.SiMarkers){
				KameraKontroler.AllWorkingObjects.Add(obj.Mark.gameObject);
			}
			break;
		case "Decals":
			State = EditStates.DecalsStat;
			ChangeCategory(5);
			break;
		case "Props":
			State = EditStates.PropsStat;
			ChangeCategory(6);
			break;
		case "Ai":
			State = EditStates.AIStat;
			ChangeCategory(7);
			break;
		}
	}


	void ChangeCategory(int id = 0){
		foreach(GameObject obj in Categorys){
			obj.SetActive(false);
		}

		foreach(GameObject obj in CategorysSelected){
			obj.SetActive(false);
		}


		CategorysSelected[id].SetActive(true);
		Categorys[id].SetActive(true);
	}

	public void ChangePointerInGameplay(bool on = true){
		MauseOnGameplay = on;
	}

	public void AddToSelection(List<GameObject> add){
		for(int i = 0; i < add.Count; i++){
			bool AlreadyExist = false;
			foreach(GameObject obj in Selected){
				if(obj == add[i]){
					AlreadyExist = true;
					break;
				}
			}
			if(!AlreadyExist) Selected.Add(add[i]);

		}
		UpdateSelectionRing();
	}

	public void RemoveFromSelection(List<GameObject> remove){
		for(int i = 0; i < remove.Count; i++){
			foreach(GameObject obj in Selected){
				if(obj == remove[i]){
					Selected.Remove(obj);
					break;
				}
			}
		}
		UpdateSelectionRing();
	}

	public void ChangeSelectionState(List<GameObject> change){
		for(int i = 0; i < change.Count; i++){
			bool AlreadyExist = false;
			foreach(GameObject obj in Selected){
				if(obj == change[i]){
					AlreadyExist = true;
					Selected.Remove(obj);
					break;
				}
			}
			if(!AlreadyExist) Selected.Add(change[i]);
		}
		UpdateSelectionRing();
	}

	public void CleanSelection(){
		Selected = new List<GameObject>();
		UpdateSelectionRing();

	}

	public void UpdateReflectionOption(int id){
		UpdateSelectionRing();
	}
	public void UpdateTolerance(){
		MirrorTolerance = System.Single.Parse(ToolbarTolerance.text);
		UpdateSelectionRing();
	}

	public void UpdateSelectionRing(){
		MirrorTolerance = 0.3f;

		if(Selected.Count <= 0){
			SelectedMarker.gameObject.SetActive(false);
			if(SelectedSymmetryMarkers.Count > 0){
				foreach(Transform marker in SelectedSymmetryMarkers){
					marker.gameObject.SetActive(false);
				}
			}
			//SelectedReflectionMarker.gameObject.SetActive(false);
			foreach(GameObject child in SelectionsRings){
				Destroy(child.gameObject);
			}
			SelectionsRings = new List<GameObject>();
			SelectedStartPos = new Vector3[0];
			foreach(ListObject list in AllMarkersList.AllFields){
				list.Unselect();
			}
		}
		else{
			SelectedMarker.gameObject.SetActive(true);
			//SymmetrySelectionList = new;
			//MirrorSelected1 = new List<GameObject>();

			foreach(GameObject child in SelectionsRings){
				Destroy(child.gameObject);
			}
			SelectionsRings = new List<GameObject>();

			float MaxX = Selected[0].transform.position.x;
			float MaxY = Selected[0].transform.position.z;

			float MinX = Selected[0].transform.position.x;
			float MinY = Selected[0].transform.position.z;
			float MidHeight = 0;

			foreach(GameObject obj in Selected){
				MaxX = Mathf.Max(MaxX, obj.transform.position.x);
				MaxY = Mathf.Max(MaxY, obj.transform.position.z);
				MinX = Mathf.Min(MinX, obj.transform.position.x);
				MinY = Mathf.Min(MinY, obj.transform.position.z);
				MidHeight += obj.transform.position.y;
			}
			MidHeight /= Selected.Count;

			SelectedMarker.position = new Vector3((MaxX+MinX) /2, MidHeight, (MaxY + MinY) /2);
			SelectedMarker.localScale = Vector3.one * (Mathf.Max(Mathf.Abs(MaxX - MinX), Mathf.Abs(MaxY - MinY)) * 1.1f  +  0.4f);

			if(SelectedMarker.localScale.x < 3){
				SelectedMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[0]);
				if(SelectedSymmetryMarkers.Count > 0){
					foreach(Transform marker in SelectedSymmetryMarkers){
						marker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[0]);
					}
				}
				//SelectedReflectionMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[0]);
			}
			else{
				SelectedMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[1]);
				if(SelectedSymmetryMarkers.Count > 0){
					foreach(Transform marker in SelectedSymmetryMarkers){
						marker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[1]);
					}
				}
				//SelectedReflectionMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[1]);
			}

			SelectedStartPos = new Vector3[Selected.Count];

			for(int i = 0; i < Selected.Count; i++){
				SelectedStartPos[i] = Selected[i].transform.position - SelectedMarker.position;
			}

			int SymmetryCode = PlayerPrefs.GetInt("Symmetry", 0);

			// Generate Symmetry Selection Rings
			if(SymmetryCode == 0){
				RegenerateSymmetryMarkers(0);
			}
			else if(SymmetryCode == 7){
				RegenerateSymmetryMarkers(PlayerPrefs.GetInt("SymmetryAngleCount", 2) - 1);
			}
			else if(SymmetryCode == 4){
				RegenerateSymmetryMarkers(3);
			}
			else{
				RegenerateSymmetryMarkers(1);
			}

			// Do Symmetry Selection
			if( SymmetryCode == 0){}
			else if(SymmetryCode == 1) SelectHorizontal();
			else if(SymmetryCode == 2) SelectVertical();
			else if(SymmetryCode == 3) SelectHorizontalVertical();
			else if(SymmetryCode == 4){
				SelectHorizontal(0);
				SelectVertical(1);
				SelectHorizontalVertical(2);
			}
			else if(SymmetryCode == 5) SelectDiagonal1();
			else if(SymmetryCode == 6) SelectDiagonal2();
			else if(SymmetryCode == 7){
				int Count = PlayerPrefs.GetInt("SymmetryAngleCount", 2);
				float angle = 360.0f / (float)Count;
				for(int i = 0; i < Count - 1; i++){
					SelectRotateByCenter(i, angle + angle * i);
				}
			}

			foreach(ListObject list in AllMarkersList.AllFields){
				list.Unselect();
			}
			foreach(GameObject obj in Selected){
				GameObject newRing = Instantiate(RingSelectionPrefab) as GameObject;
				newRing.GetComponent<SelectionRing>().SelectedObject = obj.transform;
				SelectionsRings.Add(newRing);
				foreach(ListObject list in AllMarkersList.AllFields){
					if(list.ConnectedGameObject == obj){
						list.Select();
						break;
					}
				}
			}
			for(int i = 0; i < SymmetrySelectionList.Length; i++){
				SymmetrySelectionList[i].SelectedStartPos = new Vector3[SymmetrySelectionList[i].MirrorSelected.Count];
				for(int m = 0; m < SymmetrySelectionList[i].MirrorSelected.Count; m++){
					GameObject newRing = Instantiate(RingSelectionPrefab) as GameObject;
					newRing.GetComponent<SelectionRing>().SelectedObject = SymmetrySelectionList[i].MirrorSelected[m].transform;
					newRing.GetComponent<SelectionRing>().ForceUpdate();
					SelectionsRings.Add(newRing);
					SymmetrySelectionList[i].SelectedStartPos[m] = newRing.transform.position - SelectedSymmetryMarkers[i].position;
				}
			}
		}
	}

	void RegenerateSymmetryMarkers(int count = 0){
		if(SelectedSymmetryMarkers.Count == count){
			for(int i = 0; i < SymmetrySelectionList.Length; i++){
				SymmetrySelectionList[i].MirrorSelected = new List<GameObject>();
			}
			return;
		}

		SymmetrySelectionList = new SymmetrySelection[count];
		for(int i = 0; i < count; i++){
			SymmetrySelectionList[i] = new SymmetrySelection();
			SymmetrySelectionList[i].MirrorSelected = new List<GameObject>();
		}

		Debug.Log("Regenerate Symmetry Markers");

		foreach(Transform marker in SelectedSymmetryMarkers){
			DestroyImmediate(marker.gameObject);
		}

		SelectedSymmetryMarkers = new List<Transform>();
		for(int i = 0; i < count; i++){
			GameObject newMarker = Instantiate(SelectionSymmetryPrefab) as GameObject;
			newMarker.transform.parent = HudElements;
			SelectedSymmetryMarkers.Add(newMarker.transform);
		}
	}

	void SelectHorizontal(int id = 0){
		if(SelectedSymmetryMarkers.Count < 0) return;
		SelectedSymmetryMarkers[id].gameObject.SetActive(true);
		SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

		Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
		MirroredMarker.x = -MirroredMarker.x;
		MirroredMarker += Scenario.MapCenterPoint;
		MirroredMarker.y = SelectedMarker.position.y;
		SelectedSymmetryMarkers[id].position = MirroredMarker;
		SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, 1);
		
		foreach(GameObject obj in Selected){
			foreach(GameObject all in KameraKontroler.AllWorkingObjects){
				Vector3 MirroredPos = all.transform.position - Scenario.MapCenterPoint;
				MirroredPos.x = -MirroredPos.x;
				MirroredPos += Scenario.MapCenterPoint;

				//Xdist = Scenario.MapCenterPoint.x - obj.transform.position.x;
				Vector3 SelPos = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
				Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);
				
				if(Vector3.Distance(SelPos, AllPos) < MirrorTolerance){ //MirrorTolerance
					bool AlreadyExist = false;
					foreach(GameObject MirObj in SymmetrySelectionList[id].MirrorSelected){
						if(MirObj == all){
							AlreadyExist = true;
							break;
						}
					}
					if(!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
					continue;
				}
			}
		}
	}

	void SelectVertical(int id = 0){
		if(SelectedSymmetryMarkers.Count < 0) return;
		SelectedSymmetryMarkers[id].gameObject.SetActive(true);
		SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;
		Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
		MirroredMarker.z = -MirroredMarker.z;
		MirroredMarker += Scenario.MapCenterPoint;
		MirroredMarker.y = SelectedMarker.position.y;
		SelectedSymmetryMarkers[id].position = MirroredMarker;
		SymmetrySelectionList[id].MoveMultiply = new Vector3(1, 1, -1);

		foreach(GameObject obj in Selected){
			foreach(GameObject all in KameraKontroler.AllWorkingObjects){
				Vector3 MirroredPos = all.transform.position - Scenario.MapCenterPoint;
				MirroredPos.z = -MirroredPos.z;
				MirroredPos += Scenario.MapCenterPoint;
				
				//Xdist = Scenario.MapCenterPoint.x - obj.transform.position.x;
				Vector3 SelPos = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
				Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);


				if(Vector3.Distance(SelPos, AllPos) < MirrorTolerance){ //MirrorTolerance
					bool AlreadyExist = false;
					foreach(GameObject MirObj in SymmetrySelectionList[id].MirrorSelected){
						if(MirObj == all){
							AlreadyExist = true;
							break;
						}
					}
					if(!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
					continue;
				}
			}
		}
	}

	void SelectHorizontalVertical(int id = 0){
		if(SelectedSymmetryMarkers.Count < 0) return;
		SelectedSymmetryMarkers[id].gameObject.SetActive(true);
		SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

		Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
		MirroredMarker.x = -MirroredMarker.x;
		MirroredMarker.z = -MirroredMarker.z;
		MirroredMarker += Scenario.MapCenterPoint;
		MirroredMarker.y = SelectedMarker.position.y;
		SelectedSymmetryMarkers[id].position = MirroredMarker;
		SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, -1);
		
		foreach(GameObject obj in Selected){
			foreach(GameObject all in KameraKontroler.AllWorkingObjects){
				Vector3 MirroredPos = all.transform.position - Scenario.MapCenterPoint;
				MirroredPos.z = -MirroredPos.z;
				MirroredPos.x = -MirroredPos.x;
				MirroredPos += Scenario.MapCenterPoint;
				
				Vector3 SelPos = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
				Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

				if(Vector3.Distance(SelPos, AllPos) < MirrorTolerance){ //MirrorTolerance
					bool AlreadyExist = false;
					foreach(GameObject MirObj in SymmetrySelectionList[id].MirrorSelected){
						if(MirObj == all){
							AlreadyExist = true;
							break;
						}
					}
					if(!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
					continue;
				}
			}
		}
	}

	void SelectDiagonal1(int id = 0){
		if(SelectedSymmetryMarkers.Count < 0) return;
		SelectedSymmetryMarkers[id].gameObject.SetActive(true);
		SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;
		
		Vector3 Origin = new Vector3(0, 0, -Scenario.ScenarioData.Size.y / 10f);
		Vector3 Origin2 = new Vector3(Scenario.ScenarioData.Size.y / 10f, 0, 0);
		Vector3 Point = new Vector3(SelectedMarker.position.x, 0, SelectedMarker.position.z);

		Vector3 PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
		Vector3 FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		float FinalDist = Vector3.Distance(PointOfMirror, Point);
		Vector3 MirroredMarker = PointOfMirror + FinalDir * FinalDist;
		MirroredMarker.y = SelectedMarker.position.y;

		//SelectedSymmetryMarkers[id].position = Origin;
		SelectedSymmetryMarkers[id].position = MirroredMarker;
		SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, -1);
		
		foreach(GameObject obj in Selected){
			foreach(GameObject all in KameraKontroler.AllWorkingObjects){

				Point = new Vector3(all.transform.position.x, 0, all.transform.position.z);
				PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
				FinalDir = PointOfMirror - Point;
				FinalDir.y = 0;
				FinalDir.Normalize();
				FinalDist = Vector3.Distance(PointOfMirror, Point);
				Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;

				Vector3 SelPos = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
				Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);
				
				if(Vector3.Distance(SelPos, AllPos) < MirrorTolerance){ //MirrorTolerance
					bool AlreadyExist = false;
					foreach(GameObject MirObj in SymmetrySelectionList[id].MirrorSelected){
						if(MirObj == all){
							AlreadyExist = true;
							break;
						}
					}
					if(!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
					continue;
				}
			}
		}
	}

	
	void SelectDiagonal2(int id = 0){
		if(SelectedSymmetryMarkers.Count < 0) return;
		SelectedSymmetryMarkers[id].gameObject.SetActive(true);
		SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;
		
		Vector3 Origin = new Vector3(0, 0, 0);
		Vector3 Origin2 = new Vector3(Scenario.ScenarioData.Size.y / 10f, 0, -Scenario.ScenarioData.Size.y / 10f);
		Vector3 Point = new Vector3(SelectedMarker.position.x, 0, SelectedMarker.position.z);
		
		Vector3 PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
		Vector3 FinalDir = PointOfMirror - Point;
		FinalDir.y = 0;
		FinalDir.Normalize();
		float FinalDist = Vector3.Distance(PointOfMirror, Point);
		Vector3 MirroredMarker = PointOfMirror + FinalDir * FinalDist;
		MirroredMarker.y = SelectedMarker.position.y;
		
		//SelectedSymmetryMarkers[id].position = Origin;
		SelectedSymmetryMarkers[id].position = MirroredMarker;
		SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, -1);
		
		foreach(GameObject obj in Selected){
			foreach(GameObject all in KameraKontroler.AllWorkingObjects){
				
				Point = new Vector3(all.transform.position.x, 0, all.transform.position.z);
				PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
				FinalDir = PointOfMirror - Point;
				FinalDir.y = 0;
				FinalDir.Normalize();
				FinalDist = Vector3.Distance(PointOfMirror, Point);
				Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;
				
				Vector3 SelPos = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
				Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);
				
				if(Vector3.Distance(SelPos, AllPos) < MirrorTolerance){ //MirrorTolerance
					bool AlreadyExist = false;
					foreach(GameObject MirObj in SymmetrySelectionList[id].MirrorSelected){
						if(MirObj == all){
							AlreadyExist = true;
							break;
						}
					}
					if(!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
					continue;
				}
			}
		}
	}

	void SelectRotateByCenter(int id = 0, float angle = 180){
		if(SelectedSymmetryMarkers.Count < 0) return;
		SelectedSymmetryMarkers[id].gameObject.SetActive(true);
		SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;
		
		Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
		MirroredMarker = RotatePointAroundPivot(SelectedMarker.position, Scenario.MapCenterPoint, angle);

		SelectedSymmetryMarkers[id].position = MirroredMarker;
		SymmetrySelectionList[id].MoveMultiply = Vector3.one;
		SymmetrySelectionList[id].MoveRotation = angle;
		
		foreach(GameObject obj in Selected){
			foreach(GameObject all in KameraKontroler.AllWorkingObjects){
				Vector3 MirroredPos = all.transform.position - Scenario.MapCenterPoint;
				//MirroredPos.z = -MirroredPos.z;
				//MirroredPos.x = -MirroredPos.x;
				//MirroredPos += Scenario.MapCenterPoint;
				MirroredPos = RotatePointAroundPivot(all.transform.position, Scenario.MapCenterPoint, angle);
				
				Vector3 SelPos = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
				Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

				if(Vector3.Distance(SelPos, AllPos) < MirrorTolerance){ //MirrorTolerance
					bool AlreadyExist = false;
					foreach(GameObject MirObj in SymmetrySelectionList[id].MirrorSelected){
						if(MirObj == all){
							AlreadyExist = true;
							break;
						}
					}
					if(!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
					continue;
				}
			}
		}
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle){
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler(Vector3.up * angle) * dir;
		point = dir + pivot;
		return point;
	}

	static public Vector3 ClosestPointToLine(Vector3 A, Vector3 B, Vector3 P){
		Vector3 AP = P - A;
		Vector3 AB = B - A;
		float ab2 = AB.x*AB.x + AB.z*AB.z;
		float ap_ab = AP.x*AB.x + AP.z*AB.z;
		float t = ap_ab / ab2;
		Vector3 Closest = A + AB * t;
		return Closest;
	}
}
