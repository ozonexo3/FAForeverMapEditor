using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CameraControler : MonoBehaviour {

	public			Undo				History;
	public			MapLuaParser		MapControler;
	public			MapHelperGui		HUD;
	public			Editing				Edit;
	public			AppMenu				Menu;
	public			GameObject			LoadingPopup;

	public			Transform			Pivot;
	public			float				MapSize;
	public 			float 				zoomIn = 1;
	public 			Vector3				Pos = Vector3.zero;
	public 			Vector3				Rot = Vector3.zero;
					Vector2 			prevMausePos = Vector2.zero;
	private			Vector2				BeginMarkerPos;
	private			Vector3				ClickSnapDif;
	private			bool				MoveMarker;

	public			LayerMask			MarkerMask;
	public			LayerMask			Mask;
	public			LayerMask			ControlerMask;

	public			Transform			SelectedMarker;
	public			RectTransform		SelectionBoxImage;
	private			bool				SelectionBox;
	private			Vector2				MouseBeginClick;
	public			bool				BeginWithShift;
	public			bool				BeginWithCtrl;
	public			bool				BeginWithAlt;
	public			bool				ControlerBegin;
	public			bool				ControlerDrag;
	public			Transform			DebugCube;

	public			Transform			ReflectionCamera;

	public			Vector3				TerrainClickPos;

	public			List<GameObject>	AllWorkingObjects = new List<GameObject>();

	public void RestartCam(){
		Pos = Vector3.zero + Vector3.right * MapSize / 20.0f - Vector3.forward * MapSize / 20.0f;
		Pos.y = Terrain.activeTerrain.SampleHeight(Pos);
		Rot = Vector3.zero;
		zoomIn = 1;
	}

	void Update () {
		if(LoadingPopup.activeSelf) return;

		// Interaction
		if(Edit.MauseOnGameplay){
			if(HUD.MapLoaded){
				CameraMovement();

				if(Edit.State == Editing.EditStates.MarkersStat){
					MarkersInteraction();
				}

				if(Input.GetMouseButtonDown(0)){
					OnBeginDragFunc();
				}
				else if(Input.GetMouseButtonUp(0)){
					OnEndDragFunc();

					BeginWithShift = false;
					BeginWithCtrl = false;
					BeginWithAlt = false;
				}
				else if(Input.GetMouseButton(0)){
					OnDragFunc();
				}
			}
			else{

			}
		}

		if(Menu.MenuOpen) return;
		if(Input.GetKeyDown(KeyCode.G)){
			Menu.GridToggle.isOn = !Menu.GridToggle.isOn;
			Menu.MapHelper.Loader.HeightmapControler.ToogleGrid(Menu.GridToggle.isOn);
		}
	}

	void LateUpdate(){
		//ReflectionCamera.localRotation = Quaternion.Lerp(ReflectionCamera.localRotation, Quaternion.Euler(new Vector3(Mathf.Lerp(-90, 50, Rot.x / -80.0f), 0, 0)), Time.deltaTime * 10);
		ReflectionCamera.rotation = Quaternion.Euler(Vector3.right * -90);
	}

	float ZoomCamPos(){
		return Mathf.Pow(zoomIn, 3);
	}

	void CameraMovement(){
		if(Input.GetAxis("Mouse ScrollWheel") > 0 && zoomIn > 0){
			zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * (MapSize/1024);
			
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, Mask)){
				Pos += (hit.point - Pos) * Mathf.Lerp(0.22f, 0.12f, ZoomCamPos()) * (MapSize/1024);
			}
			
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0){
			zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * (MapSize/1024);
		}
		
		zoomIn = Mathf.Clamp01(zoomIn);
		
		
		if(Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(2)){
			prevMausePos = Input.mousePosition;
		}
		
		if(Input.GetKey(KeyCode.Space)){
			Rot.y += (Input.mousePosition.x - prevMausePos.x) * 12 * Time.deltaTime;
			Rot.x -= (Input.mousePosition.y - prevMausePos.y) * 12 * Time.deltaTime;
			Rot.x = Mathf.Clamp(Rot.x, -80, 0);
			prevMausePos = Input.mousePosition;
		}
		if(Input.GetMouseButton(2)){
			Pos -= transform.right * (Input.mousePosition.x - prevMausePos.x) * 2.0f * (transform.localPosition.y * 0.03f + 0.2f) * Time.deltaTime;
			Pos -= (transform.forward + transform.up) * (Input.mousePosition.y - prevMausePos.y) * 2.0f * (transform.localPosition.y * 0.03f + 0.2f) * Time.deltaTime;
			prevMausePos = Input.mousePosition;
			
			Pos.x = Mathf.Clamp(Pos.x, 0, MapSize / 10.0f);
			Pos.z = Mathf.Clamp(Pos.z, MapSize / -10.0f, 0);
			Pos.y = Terrain.activeTerrain.SampleHeight(Pos);
			
		}
		
		if(Input.GetKeyDown(KeyCode.Keypad0)){
			RestartCam();
		}
		
		
		transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, ZoomCamPos() * MapSize / 7 + 2, transform.localPosition.z), Time.deltaTime * 20);
		
		Pivot.localRotation = Quaternion.Lerp(Pivot.localRotation, Quaternion.Euler(Rot), Time.deltaTime * 10);
		Pivot.localPosition = Vector3.Lerp(Pivot.localPosition, Pos,  Time.deltaTime * 18);
	}

	void MarkersInteraction(){
		if(Input.GetMouseButtonDown(0)){
			MouseBeginClick = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, ControlerMask)){
				Debug.Log("Clicked on controler");
				ControlerBegin = true;
				RaycastHit hit2;
				if (Physics.Raycast(ray, out hit2, 1000, Mask)){
					TerrainClickPos = hit2.point;

					Vector3 HitPointSnaped = hit2.point;
					HitPointSnaped.x *= 10;
					HitPointSnaped.x = (int)(HitPointSnaped.x + 0.0f);
					HitPointSnaped.x /= 10.0f;
					
					HitPointSnaped.z *= 10;
					HitPointSnaped.z = (int)(HitPointSnaped.z + 0.0f);
					HitPointSnaped.z /= 10.0f;
					
					HitPointSnaped.x -= 0.05f;
					HitPointSnaped.z -= 0.05f;

					ClickSnapDif = HitPointSnaped - hit2.point;
					Vector3 HitPointSnaped2 = hit2.point - ClickSnapDif;
					HitPointSnaped2.x *= 10;
					HitPointSnaped2.x = (int)(HitPointSnaped2.x + 0.0f);
					HitPointSnaped2.x /= 10.0f;
					
					HitPointSnaped2.z *= 10;
					HitPointSnaped2.z = (int)(HitPointSnaped2.z + 0.0f);
					HitPointSnaped2.z /= 10.0f;
					
					HitPointSnaped2.x -= 0.05f;
					HitPointSnaped2.z -= 0.05f;

					Edit.SelectedMarkerBeginClickPos = Edit.SelectedMarker.position - HitPointSnaped2;
					Edit.SelectedMarkerBeginPos = Edit.SelectedMarker.position;
					for(int i = 0; i < Edit.SymmetrySelectionList.Length; i++){
						Edit.SymmetrySelectionList[i].SelectedMarkerBeginClickPos = Edit.SelectedSymmetryMarkers[i].position;
					}
				}

			}
			else if (Physics.Raycast(ray, out hit, 1000, MarkerMask)){
				SelectedMarker = hit.collider.gameObject.transform;
				BeginMarkerPos = Input.mousePosition;
				List<GameObject> ClickedObjects = new List<GameObject>();
				ClickedObjects.Add(hit.collider.gameObject);
				if(Input.GetKey(KeyCode.LeftAlt)){
					Edit.RemoveFromSelection(ClickedObjects);
				}
				else if(Input.GetKey(KeyCode.LeftControl)){
					Edit.AddToSelection(ClickedObjects);
				}
				else if(Input.GetKey(KeyCode.LeftShift)){
					Edit.ChangeSelectionState(ClickedObjects);
				}
				else{
					Edit.CleanSelection();
					Edit.AddToSelection(ClickedObjects);
				}
			}
			else{
				if(Input.GetKey(KeyCode.LeftAlt)){
					BeginWithAlt = true;
				}
				else if(Input.GetKey(KeyCode.LeftControl)){
					BeginWithCtrl = true;
				}
				else if(Input.GetKey(KeyCode.LeftShift)){
					BeginWithShift = true;
				}
				else{
					Edit.CleanSelection();
				}
			}
			
		}
		if(Input.GetMouseButtonUp(0)){
			SelectedMarker = null;
			MoveMarker = false;
		}
		if(Input.GetMouseButton(0)){
			if(SelectedMarker){
				if(SelectionBox){


				}

				if(MoveMarker){
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 1000, Mask)){
						/*Vector3 SnapMarkerPos = hit.point;

						SnapMarkerPos.x *= 5;
						SnapMarkerPos.x = (int)(SnapMarkerPos.x - 0.5f);
						SnapMarkerPos.x /= 5;

						SnapMarkerPos.z *= 5;
						SnapMarkerPos.z = (int)(SnapMarkerPos.z - 0.5f);
						SnapMarkerPos.z /= 5;
						
						
						SelectedMarker.transform.position = SnapMarkerPos;*/
						
					}	
				}
				else{
					if(Vector2.Distance(Input.mousePosition, BeginMarkerPos) > 5){
						MoveMarker = true;
						SelectionBox = true;
					}
				}
			}
		}
	}

	public void MarkerList(GameObject obj){
		List<GameObject> ClickedObjects = new List<GameObject>();
		ClickedObjects.Add(obj);

		if(Input.GetKey(KeyCode.LeftAlt)){
			Edit.RemoveFromSelection(ClickedObjects);
		}
		else if(Input.GetKey(KeyCode.LeftControl)){
			Edit.AddToSelection(ClickedObjects);
		}
		else if(Input.GetKey(KeyCode.LeftShift)){
			Edit.ChangeSelectionState(ClickedObjects);
		}
		else{
			Edit.CleanSelection();
			Edit.AddToSelection(ClickedObjects);
		}
	}

	public	void OnBeginDragFunc(){
		SelectionBox = false;
		MouseBeginClick = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		/*if(ControlerDrag){
			History.RegisterMarkersMove();
		}*/

		OnDragFunc();
	}

	public	void OnEndDragFunc(){
		ControlerBegin = false;

		if(ControlerDrag){
			ControlerDrag = false;

		}
		else if(SelectionBox){
			SelectionBoxImage.gameObject.SetActive(false);
			SelectionBox = false;
			Vector2 MouseEndPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector2 diference = MouseEndPos - MouseBeginClick;
			Rect SelectionBoxArea = new Rect(Mathf.Min(MouseEndPos.x, MouseBeginClick.x) , Mathf.Min(MouseEndPos.y, MouseBeginClick.y), Mathf.Abs(diference.x), Mathf.Abs(diference.y));

			List<GameObject> ClickedObjects = new List<GameObject>();

			foreach(GameObject obj in AllWorkingObjects){
				if(SelectionBoxArea.Contains(GetComponent<Camera>().WorldToScreenPoint(obj.transform.position))){
					ClickedObjects.Add(obj);
				}
			}
			if(BeginWithAlt){
				Edit.RemoveFromSelection(ClickedObjects);
			}
			else if(BeginWithCtrl){
				Edit.AddToSelection(ClickedObjects);
			}
			else if(BeginWithShift){
				Edit.ChangeSelectionState(ClickedObjects);
			}
			else{
				Edit.AddToSelection(ClickedObjects);
			}
		}
	}

	public	void OnDragFunc(){
		Vector2 MouseEndPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		Vector2 diference = MouseEndPos - MouseBeginClick;

		if(ControlerDrag){

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit2;
			if (Physics.Raycast(ray, out hit2, 1000, Mask)){
				Vector3 OffsetPos = hit2.point - TerrainClickPos;
				OffsetPos.y = 0;

				Vector3 HitPointSnaped = hit2.point - ClickSnapDif;
				HitPointSnaped.x *= 10;
				HitPointSnaped.x = (int)(HitPointSnaped.x + 0.0f);
				HitPointSnaped.x /= 10.0f;
				
				HitPointSnaped.z *= 10;
				HitPointSnaped.z = (int)(HitPointSnaped.z + 0.0f);
				HitPointSnaped.z /= 10.0f;
				
				HitPointSnaped.x -= 0.05f;
				HitPointSnaped.z -= 0.05f;
				
				HitPointSnaped.y = Terrain.activeTerrain.SampleHeight(HitPointSnaped);
				DebugCube.position = HitPointSnaped;

				Edit.SelectedMarker.position = HitPointSnaped + Edit.SelectedMarkerBeginClickPos;

				
				for(int i = 0; i < Edit.Selected.Count; i++){
					Edit.Selected[i].transform.position = Edit.SelectedMarker.position + Edit.SelectedStartPos[i];
				}


				Vector3 MovedOffset = Edit.SelectedMarker.position - Edit.SelectedMarkerBeginPos;
				for(int i = 0; i < Edit.SymmetrySelectionList.Length; i++){
					Vector3 ThisMovedOffset = Quaternion.Euler(Vector3.up * Edit.SymmetrySelectionList[i].MoveRotation) * MovedOffset;
					Vector3 localMoveOffset = new Vector3(ThisMovedOffset.x * Edit.SymmetrySelectionList[i].MoveMultiply.x, ThisMovedOffset.y, ThisMovedOffset.z * Edit.SymmetrySelectionList[i].MoveMultiply.z);
					Edit.SelectedSymmetryMarkers[i].position = Edit.SymmetrySelectionList[i].SelectedMarkerBeginClickPos + localMoveOffset;

					for(int s = 0; s < Edit.SymmetrySelectionList[i].MirrorSelected.Count; s++){
						Edit.SymmetrySelectionList[i].MirrorSelected[s].transform.position = Edit.SelectedSymmetryMarkers[i].position + Edit.SymmetrySelectionList[i].SelectedStartPos[s];
					}
				}


			}

		}
		else if(!SelectionBox){
			if(diference.magnitude > 5 && !ControlerBegin){
				SelectionBox = true;
				SelectionBoxImage.gameObject.SetActive(true);
			}
			else if(diference.magnitude > 5 &&  ControlerBegin){

				ControlerDrag = true;
				History.RegisterMarkersMove();
			}
			else return;

		}

		SelectionBoxImage.sizeDelta = new Vector2(Mathf.Abs(diference.x), Mathf.Abs(diference.y));

		if(diference.x < 0 && diference.y < 0){
			SelectionBoxImage.anchoredPosition = MouseEndPos;
		}
		else if(diference.x < 0){
			SelectionBoxImage.anchoredPosition = new Vector2(MouseEndPos.x, MouseBeginClick.y);
		}
		else if(diference.y < 0){
			SelectionBoxImage.anchoredPosition = new Vector2(MouseBeginClick.x, MouseEndPos.y);
		}
		else{
			SelectionBoxImage.sizeDelta = diference;
			SelectionBoxImage.anchoredPosition = MouseBeginClick;
		}

	}
}
