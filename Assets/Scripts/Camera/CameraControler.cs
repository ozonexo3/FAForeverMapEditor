using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using Selection;

public partial class CameraControler : MonoBehaviour {

	public static			CameraControler		Current;

	public Camera Cam;
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

	float MaxY = 100;
	public void RestartCam(){
		if(!Terrain.activeTerrain) return;
		Pos = Vector3.zero + Vector3.right * MapSize / 20.0f - Vector3.forward * MapSize / 20.0f;
		Pos.y = Terrain.activeTerrain.SampleHeight(Pos);
		ClampPosY();
		Rot = Vector3.zero;

		zoomIn = 1;
		MaxY = ZoomCamPos() * MapSize / 7 + CameraMinOffset;
		transform.localPosition = new Vector3(transform.localPosition.x, ZoomCamPos() * MapSize / 7 + CameraMinOffset, transform.localPosition.z);
		Pivot.localRotation = Quaternion.Euler(Rot);
		Pivot.localPosition = Pos;

		RealCamPos = transform.localPosition;

		LastLocalCamPos = transform.localPosition;
		TargetLocalCamPos = transform.localPosition;
		PanOffsetX = Vector3.zero;
		PanOffsetZ = Vector3.zero;

		//TargetWorldPos = new Vector3(MapSize * 0.05f, 100, MapSize * -0.05f);
		//CamWorldPos = TargetWorldPos;
		//transform.position = CamWorldPos;
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
		return Current.zoomIn * Current.MapSize / 7f + CameraMinOffset;
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
		Cam.Render();
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

		if (HUD.MapLoaded)
		{
			UberCameraMovement();
		}

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
		/*
		float WaterHeight = ScmapEditor.GetWaterLevel();
		Vector3 ReflPos = transform.position;
		ReflPos.y = ReflPos.y - WaterHeight;
		ReflPos.y = WaterHeight - ReflPos.y;

		ReflectionCamera.position = ReflPos;
		Vector3 Forward = transform.forward;
		Vector3 LookDir = Vector3.Reflect(Forward, new Vector3(Forward.x, 0, Forward.z).normalized);
		LookDir *= -1;
		//LookDir.y *= -1;
		ReflectionCamera.rotation = Quaternion.LookRotation(LookDir, Vector3.up);
		*/
	}

	float ZoomCamPos(){
		return Mathf.Pow(zoomIn, 3);
	}

	Vector3 TargetWorldPos = Vector3.zero;
	Vector3 CamWorldPos = Vector3.zero;
	Vector3 RotatePoint = Vector3.zero;
	void NewCameraMovement()
	{
		if (Edit.MauseOnGameplay)
		{


			Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, Mask))
			{
				if (Input.GetAxis("Mouse ScrollWheel") > 0)
				{
					//zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.25f * 1;

					TargetWorldPos += ray.direction * 9.0f;

					RotatePoint = hit.point;
					Pivot.position = RotatePoint;

					/*
					if (Cam.transform.InverseTransformPoint(RotatePoint).z < Cam.transform.InverseTransformPoint(TargetWorldPos).z)
					{
						TargetWorldPos = RotatePoint;
					}
					*/

					//Pos += (hit.point - Pos) * Mathf.Lerp(0.22f, 0.12f, ZoomCamPos()) * 1.14f;
					//ClampPosY();
				}
				else if (Input.GetAxis("Mouse ScrollWheel") < 0)
				{
					//zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * 1;
					TargetWorldPos -= Cam.transform.forward * 9.0f;
				}



				Vector3 GameplayCursorPos = ScmapEditor.WorldPosToScmap(hit.point);
				GameplayCursorPos.y = hit.point.y * 10;
				GameplayCursorPos.z = ScmapEditor.Current.map.Height - GameplayCursorPos.z;
				string X = GameplayCursorPos.x.ToString("N2");
				string Y = GameplayCursorPos.y.ToString("N2");
				string Z = GameplayCursorPos.z.ToString("N2");

				X = X.PadRight(8);
				Y = Y.PadRight(8);
				Z = Z.PadRight(8);

				CursorInfo.text = "x: " + X + "\ty: " + Y + "\tz: " + Z;
			}
			else
			{
				CursorInfo.text = "x: --------  \ty: --------  \tz: --------  ";
			}

			if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(2))
			{
				prevMausePos = Input.mousePosition;
			}

			if (Input.GetKey(KeyCode.Space))
			{
				Rot.y += (Input.mousePosition.x - prevMausePos.x) * 12 * Time.deltaTime;
				Rot.x -= (Input.mousePosition.y - prevMausePos.y) * 12 * Time.deltaTime;
				Rot.x = Mathf.Clamp(Rot.x, -80, 0);
				prevMausePos = Input.mousePosition;
			}
			if (Input.GetMouseButton(2))
			{
				//float PanSpeed = Mathf.Lerp (2f, 3f, Mathf.Pow( ZoomCamPos (), 0.5f));
				float PanSpeed = 2.5f;
				TargetWorldPos -= transform.right * (Input.mousePosition.x - prevMausePos.x) * PanSpeed * (transform.localPosition.y * 0.03f + 0.2f) * Time.deltaTime;
				TargetWorldPos -= (transform.forward + transform.up) * (Input.mousePosition.y - prevMausePos.y) * PanSpeed * (transform.localPosition.y * 0.03f + 0.2f) * Time.deltaTime;
				prevMausePos = Input.mousePosition;

				TargetWorldPos.x = Mathf.Clamp(TargetWorldPos.x, 0, MapSize / 10.0f);
				TargetWorldPos.z = Mathf.Clamp(TargetWorldPos.z, MapSize / -10.0f, 0);
				TargetWorldPos.y = Terrain.activeTerrain.SampleHeight(TargetWorldPos);
				ClampPosY();
			}

		}

		//Pivot.localRotation = Quaternion.Lerp(Pivot.localRotation, Quaternion.Euler(Rot), Time.deltaTime * 10);


		//transform.rotation = Quaternion.Euler(Vector3.right * 90);

		TargetWorldPos -= RotatePoint;
		transform.rotation = Quaternion.Euler(Rot) * Quaternion.Euler(Vector3.right * 90);
		DeltaRot = Rot - LastRot;
		TargetWorldPos = Quaternion.Euler(DeltaRot) * TargetWorldPos;
		TargetWorldPos += RotatePoint;
		CamWorldPos = Vector3.Lerp(CamWorldPos, TargetWorldPos, Time.unscaledDeltaTime * 8 * DeltaRot.magnitude);
		LastRot = Rot;
		//Rot = Vector3.zero;
		/*
		if (Rot.x > 0)
			transform.RotateAround(RotatePoint, Vector3.right, Rot.x);
		else if(Rot.x < 0)
			transform.RotateAround(RotatePoint, Vector3.left, -Rot.x);

		if (Rot.y > 0)
			transform.RotateAround(RotatePoint, Vector3.right, Rot.y);
		else if (Rot.y < 0)
			transform.RotateAround(RotatePoint, Vector3.left, -Rot.y);
*/

		CamWorldPos = Vector3.Lerp(CamWorldPos, TargetWorldPos, Time.unscaledDeltaTime * 8);
		transform.position = CamWorldPos;

		
	}

	float MoveDirection;
	Vector3 TransformDirection;
	Vector3 RealCamPos;
	bool LastForward = false;

	Vector3 DebugDrawPoint1 = Vector3.zero;
	Vector3 DebugDrawPoint2 = Vector3.zero;
	void CameraMovement(){
		Debug.DrawLine(DebugDrawPoint1, DebugDrawPoint2);

		if (Edit.MauseOnGameplay)
		{
			Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, Mask))
			{
				/*
				if (Input.GetAxis("Mouse ScrollWheel") > 0 && zoomIn > 0)
				{
				zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.25f * 1;
				
					Pos += (hit.point - Pos) * Mathf.Lerp(0.22f, 0.12f, ZoomCamPos()) * 1.14f;
					ClampPosY();
				}
				else if (Input.GetAxis("Mouse ScrollWheel") < 0)
				{
					zoomIn -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * 1;
				}
				*/

				if (Input.GetAxis("Mouse ScrollWheel") < 0 && transform.localPosition.y < MaxY)
				{
					MoveDirection += Input.GetAxis("Mouse ScrollWheel") * 1;
					//TransformDirection = ray.direction;
					if (!LastForward)
						TransformDirection = Vector3.zero;
					RealCamPos += Pivot.InverseTransformDirection(transform.forward) * Input.GetAxis("Mouse ScrollWheel") * CamSpeed();
					LastForward = true;
				}
				else if (Input.GetAxis("Mouse ScrollWheel") > 0 && transform.localPosition.y > 1)
				{
					MoveDirection += Input.GetAxis("Mouse ScrollWheel") * 1;
					if (LastForward)
						TransformDirection = Vector3.zero;


					Vector3 Ray = Pivot.InverseTransformVector(ray.direction);

					/*
					float CamChange = Mathf.Lerp(1, 1.5f, (Rot.x / -90));
					Ray.z *= CamChange;
					Ray.x *= Mathf.Lerp(1, 0.85f, (Rot.x / -90));
					Ray.Normalize();
					*/

					RealCamPos += Ray * Input.GetAxis("Mouse ScrollWheel") * CamSpeed();

					

					DebugDrawPoint1 = transform.position;
					DebugDrawPoint2 = transform.position + ray.direction * 100;
					
					LastForward = false;
				}
				
			}


			if (Physics.Raycast(ray, out hit, 1000, MaskCursor))
			{
				Vector3 GameplayCursorPos = ScmapEditor.WorldPosToScmap(hit.point);
				GameplayCursorPos.y = hit.point.y * 10;
				GameplayCursorPos.z = ScmapEditor.Current.map.Height - GameplayCursorPos.z;
				string X = GameplayCursorPos.x.ToString("N2");
				string Y = GameplayCursorPos.y.ToString("N2");
				string Z = GameplayCursorPos.z.ToString("N2");

				X = X.PadRight(8);
				Y = Y.PadRight(8);
				Z = Z.PadRight(8);

				CursorInfo.text = "x: " + X + "\ty: " + Y + "\tz: " + Z;
			}
			else
			{
				CursorInfo.text = "x: --------  \ty: --------  \tz: --------  ";
			}

			zoomIn = Mathf.Clamp01(zoomIn);

			if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(2))
			{
				prevMausePos = Input.mousePosition;
			}

			if (Input.GetKey(KeyCode.Space))
			{
				Rot.y += (Input.mousePosition.x - prevMausePos.x) * 12 * Time.deltaTime;
				Rot.x -= (Input.mousePosition.y - prevMausePos.y) * 12 * Time.deltaTime;
				Rot.x = Mathf.Clamp(Rot.x, -80, 0);
				prevMausePos = Input.mousePosition;
			}
			if (Input.GetMouseButton(2))
			{
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

			if (Input.GetKeyDown(KeyCode.Home))
			{
				RestartCam();
			}
		}


		/*
		if (TransformDirection.sqrMagnitude > 0)
		{


			RealCamPos += TransformDirection * CamSpeed();
			

			TransformDirection = Vector3.Lerp(TransformDirection, Vector3.zero, Time.deltaTime * 10);
			if (TransformDirection.magnitude < 0.01f)
				TransformDirection = Vector3.zero;
		}*/


		float Mag = Vector3.Distance(transform.localPosition, RealCamPos);
		if(Mag < 0.02f)
		{
			RealCamPos = transform.localPosition;
			Mag = 0;
		}
		if (Mag > 0)
		{
			
			if (RealCamPos.y < 1 || RealCamPos.y > MaxY)
			{
				RealCamPos.y = Mathf.Clamp(transform.localPosition.y, 1, MaxY);
				RealCamPos.x = transform.localPosition.x;
				RealCamPos.z = transform.localPosition.z;
			}

			transform.localPosition = Vector3.Lerp(transform.localPosition, RealCamPos, Time.deltaTime * 14);

			Ray ray = new Ray(transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, Mask))
			{
				Vector3 PreviousPos = Pivot.transform.position;
				Pos = hit.point;
				Pivot.transform.position = hit.point;

				Vector3 Offset = transform.InverseTransformVector(hit.point - PreviousPos);
				Offset.x = 0;
				Offset.z = 0;
				Offset = transform.InverseTransformVector(Offset);
				//Debug.Log(Offset);
				RealCamPos.y -= Offset.y;
				transform.localPosition -= Vector3.up * Offset.y;

				if (transform.localPosition.y < 1 || transform.localPosition.y > MaxY)
				{
					transform.localPosition = new Vector3(0, Mathf.Clamp(transform.localPosition.y, 1, MaxY), 0);
					RealCamPos = transform.localPosition;
				}
				else
				{
					//	transform.localPosition = new Vector3(0, Mathf.Clamp(transform.localPosition.y, 1, MaxY), 0);

					Vector3 LocalPos = transform.localPosition;
					LocalPos.z -= transform.localPosition.z;
					RealCamPos.z -= transform.localPosition.z;

					LocalPos.x -= transform.localPosition.x;
					RealCamPos.x -= transform.localPosition.x;
					transform.localPosition = LocalPos;
				}
			}
		}
		/*
		else if (MoveDirection < 0)
		{
			MoveDirection += Time.deltaTime;
			if (MoveDirection > 0)
				MoveDirection = 0;

			transform.localPosition -= Vector3.up * MoveDirection * Time.deltaTime * CamSpeed();
		}
		else if (MoveDirection > 0)
		{
			MoveDirection -= Time.deltaTime;
			if (MoveDirection < 0)
				MoveDirection = 0;

			transform.position += TransformDirection * MoveDirection * Time.deltaTime * CamSpeed();

			Ray ray = Cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, Mask))
			{
				Pos = hit.point;
				Pivot.transform.position = hit.point;
				transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
			}
		}
		*/
		else
		{
			Pivot.localRotation = Quaternion.Lerp(Pivot.localRotation, Quaternion.Euler(Rot), Time.deltaTime * 10);
			
		}

		Pivot.localPosition = Vector3.Lerp(Pivot.localPosition, Pos, Time.deltaTime * 18);

		//transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, ZoomCamPos() * MapSize / 7 + CameraMinOffset, transform.localPosition.z), Time.deltaTime * 20);
	}

	float CamSpeed()
	{
		//return Mathf.Lerp(150, 3000, Mathf.Pow(transform.localPosition.y / 150, 1.2f));
		return Mathf.Lerp(15f, 300, Mathf.Pow(transform.localPosition.y / 150, 1.2f));
		//return Mathf.Lerp(120, 3000, transform.localPosition.y / 150f);
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
