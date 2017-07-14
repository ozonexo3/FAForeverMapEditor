using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;

public class CameraControler : MonoBehaviour {

	public static			CameraControler		Current;

	public			Undo				History;
	public			MapHelperGui		HUD;
	public			Editing				Edit;
	public			AppMenu				Menu;
	public			GameObject			LoadingPopup;
	public			bool				DragStartedGameplay = false;
	public			bool				DragStartedOverMenu = false;

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


	// Creating Marker;
	public			Transform			MarkerToCreate;
	public			Transform[]			MarkerToCreateSymmetry;

	void Awake(){
		Current = this;
	}

	const float CameraMinOffset = 0.5f;

	public void RestartCam(){
		if(!Terrain.activeTerrain) return;
		Pos = Vector3.zero + Vector3.right * MapSize / 20.0f - Vector3.forward * MapSize / 20.0f;
		Pos.y = Terrain.activeTerrain.SampleHeight(Pos);
		ClampPosY();
		Rot = Vector3.zero;

		zoomIn = 1;

		transform.localPosition = new Vector3(transform.localPosition.x, ZoomCamPos() * MapSize / 7 + CameraMinOffset, transform.localPosition.z);
		Pivot.localRotation = Quaternion.Euler(Rot);
		Pivot.localPosition = Pos;
	}

	public static void FocusCamera(Transform Pivot, float Zoom = 30, float rot = 10)
	{
		float ZoomValue = Zoom - CameraMinOffset;
		ZoomValue /= Current.MapSize / 7f;
		ZoomValue = Mathf.Pow(ZoomValue, 1f / 3f);
		Current.zoomIn = ZoomValue;
		Current.transform.localPosition = new Vector3(Current.transform.localPosition.x, Zoom, Current.transform.localPosition.z);

		Current.Rot = Vector3.right * rot;
		Current.Pivot.localRotation = Quaternion.Euler(Current.Rot);

		Current.Pos = Pivot.position;
		Current.Pivot.localPosition = Current.Pos;
	}

	public void RenderCamera(int resWidth, int resHeight, string path){
		// Set Camera
		Camera.main.orthographic = true;
		Camera.main.orthographicSize = MapSize * 0.05f;
		Pivot.localPosition = new Vector3(MapSize * 0.05f, 0, -MapSize * 0.05f);
		Pivot.rotation = Quaternion.identity;

		// Take Screenshoot
		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		Camera.main.targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
		Camera.main.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		Camera.main.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		byte[] bytes;
		if(path.Contains(".png")) bytes = screenShot.EncodeToPNG();
		else bytes = screenShot.EncodeToJPG();
		System.IO.File.WriteAllBytes(path, bytes);

		// Restart Camera
		Camera.main.orthographic = false;
		RestartCam();
	}

	void Update () {

		if (LoadingPopup.activeSelf){
			if(DragStartedOverMenu) DragStartedOverMenu = false;
			return;
		}
		if (Input.GetMouseButtonDown (0)) {
			DragStartedOverMenu = !Edit.MauseOnGameplay;
		}
		else if (Input.GetMouseButtonUp (0)) {
			DragStartedOverMenu = false;
		}


		// Interaction

		if (HUD.MapLoaded && Edit.MauseOnGameplay)
		{
			CameraMovement();

		}

		/*
		if ((Edit.MauseOnGameplay || DragStartedGameplay) && !DragStartedOverMenu) {
			if (Edit.EditMarkers.CreatingId != 0) {
				if(Input.GetKeyDown(KeyCode.Escape)){
					Edit.EditMarkers.CreatingId = 0;
					Edit.EditMarkers.UpdateCreating();
					return;
				}
				CameraMovement ();
				bool MarkerOK = OnCreateMarker ();
				if (Input.GetMouseButtonDown (0) && MarkerOK) {
					// Create marker
					Edit.Scenario.CreateMarker (Edit.EditMarkers.CreatingId - 1, MarkerToCreate.position, "");
					for (int i = 0; i < MarkerToCreateSymmetry.Length; i++) {
						Edit.Scenario.CreateMarker (Edit.EditMarkers.CreatingId - 1, MarkerToCreateSymmetry [i].position, "");
					}
					Edit.EditMarkers.GenerateAllWorkingElements ();
				}
			} else if (HUD.MapLoaded) {
				CameraMovement ();

				if (Edit.State == Editing.EditStates.MarkersStat) {
					MarkersInteraction ();
				}

				if (Input.GetMouseButtonDown (0)) {
					DragStartedGameplay = Edit.MauseOnGameplay;
					if(AllowDrag()) OnBeginDragFunc ();
				} else if (Input.GetMouseButtonUp (0)) {
					if(AllowDrag()) OnEndDragFunc ();
					DragStartedGameplay = false;
					BeginWithShift = false;
					BeginWithCtrl = false;
					BeginWithAlt = false;
				} else if (Input.GetMouseButton (0)) {
					if(AllowDrag()) OnDragFunc ();
				}
			} else {

			}
		}
		*/
		if (Menu.IsMenuOpen())
			return;
		if (Input.GetKey (KeyCode.LeftControl)) {
			if(Input.GetKeyDown(KeyCode.G)){
				Menu.SlopeToggle.isOn = !Menu.SlopeToggle.isOn;
				MapLuaParser.Current.HeightmapControler.ToogleSlope(Menu.SlopeToggle.isOn);
			}
		}
		else{
			if(Input.GetKeyDown(KeyCode.G)){
				Menu.GridToggle.isOn = !Menu.GridToggle.isOn;
				MapLuaParser.Current.HeightmapControler.ToogleGrid(Menu.GridToggle.isOn);
			}
		}


		if (Input.GetKeyDown (KeyCode.F)) {
			Focus ();
		}
	}

	void LateUpdate(){
		ReflectionCamera.rotation = Quaternion.Euler(Vector3.right * -90);
	}

	float ZoomCamPos(){
		return Mathf.Pow(zoomIn, 3);
	}

	void CameraMovement(){
		if(Input.GetAxis("Mouse ScrollWheel") > 0 && zoomIn > 0){
			zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * 1;
			
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, Mask)){
				Pos += (hit.point - Pos) * Mathf.Lerp(0.22f, 0.12f, ZoomCamPos()) * 1;
				ClampPosY();
			}
			
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0){
			zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * 1;
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
			//float PanSpeed = Mathf.Lerp (2f, 3f, Mathf.Pow( ZoomCamPos (), 0.5f));
			float PanSpeed = 2.5f;
			Pos -= transform.right * (Input.mousePosition.x - prevMausePos.x) * PanSpeed * (transform.localPosition.y * 0.03f + 0.2f) * Time.deltaTime;
			Pos -= (transform.forward + transform.up) * (Input.mousePosition.y - prevMausePos.y) * PanSpeed * (transform.localPosition.y * 0.03f + 0.2f) * Time.deltaTime;
			prevMausePos = Input.mousePosition;
			
			Pos.x = Mathf.Clamp(Pos.x, 0, MapSize / 10.0f);
			Pos.z = Mathf.Clamp(Pos.z, MapSize / -10.0f, 0);
			Pos.y = Terrain.activeTerrain.SampleHeight(Pos);
			ClampPosY();
		}
		
		if(Input.GetKeyDown(KeyCode.Home)){
			RestartCam();
		}
		
		
		transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, ZoomCamPos() * MapSize / 7 + CameraMinOffset, transform.localPosition.z), Time.deltaTime * 20);
		
		Pivot.localRotation = Quaternion.Lerp(Pivot.localRotation, Quaternion.Euler(Rot), Time.deltaTime * 10);
		Pivot.localPosition = Vector3.Lerp(Pivot.localPosition, Pos,  Time.deltaTime * 18);
	}

	void ClampPosY()
	{
		Pos.y = Mathf.Clamp(Pos.y, MapLuaParser.Current.HeightmapControler.WaterLevel.transform.localPosition.y, 2048);
	}
	
	

	void Focus(){
		/*
		if (Edit.State == Editing.EditStates.MarkersStat) {
			if (Edit.EditMarkers.SelectionsRings.Count > 0) {
				Pos = Edit.EditMarkers.SelectedMarker.transform.position;
				ClampPosY();
				//Pivot.position = Edit.EditMarkers.SelectionsRings[0].transform.position;
				float size = Mathf.Max (Edit.EditMarkers.SelectedMarker.GetComponent<MeshRenderer> ().bounds.size.x, Edit.EditMarkers.SelectedMarker.GetComponent<MeshRenderer> ().bounds.size.z);
				//Debug.Log (size + ", " + MapSize);
				zoomIn = size / (MapSize * 0.075f) + 0.21f;
			}
		}
		*/
	}

}
