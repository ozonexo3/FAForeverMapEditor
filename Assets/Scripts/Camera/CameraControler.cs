using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using Selection;

public partial class CameraControler : MonoBehaviour {

	public static			CameraControler		Current;

	public Camera Cam;
	public Camera[] OtherCams;
	public			Undo				History;
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
	Vector3 DeltaRot;
	Vector3 LastRot;
					Vector2 			prevMausePos = Vector2.zero;

	public			LayerMask			Mask;
	public LayerMask MaskCursor;

	public			Transform			ReflectionCamera;
	public			Text				CursorInfo;

	void Awake(){
		Current = this;
	}

	const float CameraMinOffset = 0.5f;

	//float MaxY = 100;
	public void RestartCam(bool NoRect = false){
		if(!Terrain.activeTerrain) return;
		Pos = Vector3.zero + Vector3.right * MapSize / 20.0f - Vector3.forward * MapSize / 20.0f;
		Pos.y = Terrain.activeTerrain.SampleHeight(Pos);
		ClampPosY();
		Rot = Vector3.zero;

		zoomIn = 1;
		//MaxY = ZoomCamPos() * MapSize / 7 + CameraMinOffset;
		transform.localPosition = new Vector3(transform.localPosition.x, ZoomCamPos() * MapSize / 7 + CameraMinOffset, transform.localPosition.z);
		Pivot.localRotation = Quaternion.Euler(Rot);
		Pivot.localPosition = Pos;

		LastLocalCamPos = transform.localPosition;
		TargetLocalCamPos = transform.localPosition;
		PanOffsetX = Vector3.zero;
		PanOffsetZ = Vector3.zero;

		UpdateRect(NoRect);
	}

	public void UpdateRect(bool NoRect = false)
	{
		float RemoveCamPropHeight = 30f / (float)Screen.height;
		float RemoveCamPropWidth = 309f / (float)Screen.width;

		if (NoRect)
		{
			RemoveCamPropHeight = 0;
			RemoveCamPropWidth = 0;
		}



		Cam.rect = new Rect(RemoveCamPropWidth, 0, 1 - RemoveCamPropWidth, 1 - RemoveCamPropHeight);

		LastWidth = Screen.width;
		LastHeight = Screen.height;

		for(int i = 0; i < OtherCams.Length; i++)
		{
			OtherCams[i].rect = Cam.rect;
		}
	}

	int LastWidth = 0;
	int LastHeight;
	void CheckScreenChange()
	{
		if(Screen.width != LastWidth || Screen.height != LastHeight)
		{
			
			UpdateRect();
		}
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

	public static float GetCurrentZoom()
	{
		//return Current.zoomIn * Current.MapSize / 7f + CameraMinOffset;
		return Current.transform.localPosition.y * 10;
	}

	public void RenderCamera(int resWidth, int resHeight, string path){
		// Set Camera
		Cam.orthographic = true;
		Cam.orthographicSize = MapSize * 0.05f;
		Pivot.localPosition = new Vector3(MapSize * 0.05f, 0, -MapSize * 0.05f);
		Pivot.rotation = Quaternion.identity;

		// Take Screenshoot
		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		Cam.targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

		PreviewTex.ForcePreviewMode(true);
		Cam.Render();
		PreviewTex.ForcePreviewMode(false);

		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		Cam.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		byte[] bytes;
		if(path.Contains(".png")) bytes = screenShot.EncodeToPNG();
		else bytes = screenShot.EncodeToJPG();
		System.IO.File.WriteAllBytes(path, bytes);

		// Restart Camera
		Cam.orthographic = false;
		RestartCam();
	}

	void Update () {

		CheckScreenChange();

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
		DragStartedGameplay = !DragStartedOverMenu;

		// Interaction

		if (MapLuaParser.IsMapLoaded)
		{
			UberCameraMovement();
		}

		if (Menu.IsMenuOpen())
			return;
		if (Input.GetKey (KeyCode.LeftControl)) {
			if(Input.GetKeyDown(KeyCode.G) && !IsInputFieldFocused())
			{
				Menu.SlopeToggle.isOn = !Menu.SlopeToggle.isOn;
				MapLuaParser.Current.HeightmapControler.ToogleSlope(Menu.SlopeToggle.isOn);
			}
		}
		else{
			if(Input.GetKeyDown(KeyCode.G) && !IsInputFieldFocused())
			{
				Menu.ToogleCurrentGrid();
			}
		}


		if (Input.GetKeyDown (KeyCode.F)) {
			Focus ();
		}
	}

	public static bool IsInputFieldFocused()
	{
		GameObject obj = EventSystem.current.currentSelectedGameObject;
		return (obj != null && obj.GetComponent<InputField>() != null && obj.GetComponent<InputField>().isFocused);
	}



	float ZoomCamPos(){
		return Mathf.Pow(zoomIn, 3);
	}


	void ClampPosY()
	{
		if(MapLuaParser.Current.HeightmapControler.WaterLevel.gameObject.activeSelf)
			Pos.y = Mathf.Clamp(Pos.y, MapLuaParser.Current.HeightmapControler.WaterLevel.transform.localPosition.y, 2048);
	}


#region Focus
	void Focus(){
		int count = SelectionManager.Current.Selection.Ids.Count;
		if (SelectionManager.Current.Active && count > 0)
		{
			Bounds FocusBound = new Bounds(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].transform.localPosition, ((count > 1)?( Vector3.zero):(Vector3.one)));

			for (int i = 1; i < count; i++)
			{
				FocusBound.Encapsulate(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].transform.localPosition);
			}

			Pos = FocusBound.center;
			ClampPosY();
			float size = Mathf.Max(FocusBound.size.x, FocusBound.size.z) * 2;
			zoomIn = size / (MapSize * 0.075f) + 0.21f;
		}

	}



	public static void FocusOnObject(GameObject Obj)
	{
		Bounds FocusBound = new Bounds(Obj.transform.localPosition, Vector3.one);

		Current.Pos = FocusBound.center;
		Current.ClampPosY();
		float size = Mathf.Max(FocusBound.size.x, FocusBound.size.z) * 2;
		Current.zoomIn = size / (Current.MapSize * 0.075f) + 0.21f;
	}
#endregion
}
