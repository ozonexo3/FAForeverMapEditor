using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControler : MonoBehaviour {


	Vector3 LastLocalCamPos;
	Vector3 TargetLocalCamPos;

	Vector3 PanOffsetX = Vector3.zero;
	Vector3 PanOffsetZ = Vector3.zero;

	const float ScrollStep = 0.033f;
	const float MinDistance = 0.5f;
	static float MaxDistance = 1100;
	static float MaxRaycastDistance = 1500;

	const float SmoothZoom = 14;
	const float SmoothPan = 16;
	const float SmoothRot = 14;
	void UberCameraMovement()
	{
		if (Edit.MauseOnGameplay)
		{
			Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			int MouseScrollSteps = (int)(Input.GetAxis("Mouse ScrollWheel") / ScrollStep);

			while (MouseScrollSteps != 0)
			{
				if (MouseScrollSteps > 0)
				{ // In
					if (transform.localPosition.y > MinDistance && Physics.Raycast(ray, out hit, MaxRaycastDistance, Mask))
					{
						Vector3 Ray = Pivot.InverseTransformDirection(ray.direction);
						TargetLocalCamPos += Ray * (ScrollStep * SampleCamSpeed());

						float Distance = Vector3.Distance(Pivot.TransformPoint(TargetLocalCamPos), hit.point);
						if (Distance < MinDistance)
						{
							TargetLocalCamPos = Pivot.InverseTransformPoint(hit.point - ray.direction * MinDistance);
						}

					}
					MouseScrollSteps--;
				}
				else if (MouseScrollSteps < 0)
				{ // Out
					ray = new Ray(Cam.transform.position, Cam.transform.forward);
					if (transform.localPosition.y < MaxDistance && Physics.Raycast(ray, out hit, MaxDistance, Mask))
					{
						Vector3 Ray = Pivot.InverseTransformDirection(ray.direction);
						TargetLocalCamPos -= Ray * (ScrollStep * SampleCamSpeed());
					}
					MouseScrollSteps++;
				}
			}
			/*
			if (Input.GetAxis("Mouse ScrollWheel") < 0 && transform.localPosition.y < MaxY)
			{
				ray = new Ray(Cam.transform.position, Cam.transform.forward);
				if (Physics.Raycast(ray, out hit, 1000, Mask))
				{
					Vector3 Ray = Pivot.InverseTransformDirection(ray.direction);
					TargetLocalCamPos += Ray  * Input.GetAxis("Mouse ScrollWheel") * CamSpeed();
				}
			}
			else if (Input.GetAxis("Mouse ScrollWheel") > 0 && transform.localPosition.y > 1)
			{
				if (Physics.Raycast(ray, out hit, 1000, Mask))
				{
					Vector3 Ray = Pivot.InverseTransformDirection(ray.direction);
					TargetLocalCamPos += Ray * Input.GetAxis("Mouse ScrollWheel") * CamSpeed();
				}
			}
			*/
		}

		//LastLocalCamPos = Vector3.Lerp(LastLocalCamPos, TargetLocalCamPos, Time.unscaledDeltaTime * SmoothZoom);
		Vector3 Velocity = Vector3.zero;
		LastLocalCamPos = Vector3.SmoothDamp(LastLocalCamPos, TargetLocalCamPos, ref Velocity, 0.043f, 10000, Time.unscaledDeltaTime);
		transform.localPosition = LastLocalCamPos;


		CursorUiPos();

		if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(2))
		{
			prevMausePos = Input.mousePosition;
		}

		if (Input.GetKey(KeyCode.Space))
		{
			Rot.y += (Input.mousePosition.x - prevMausePos.x) * 12 * Time.unscaledDeltaTime;
			Rot.x -= (Input.mousePosition.y - prevMausePos.y) * 12 * Time.unscaledDeltaTime;
			Rot.x = Mathf.Clamp(Rot.x, -80, 0);
			prevMausePos = Input.mousePosition;
		}
		if (Input.GetMouseButton(2))
		{
			float LocalPosToPanSpeed = (transform.localPosition.y * 0.08f + 0.1f) * 13;
			PanOffsetX -= transform.right * (Input.mousePosition.x - prevMausePos.x) * LocalPosToPanSpeed * Time.unscaledDeltaTime;
			Vector3 up = transform.up;
			up.y = 0;
			up.Normalize();
			PanOffsetZ -= (up) * (Input.mousePosition.y - prevMausePos.y) * LocalPosToPanSpeed * Time.unscaledDeltaTime;
			PanOffsetZ.y = 0;
			prevMausePos = Input.mousePosition;
		}

		Vector3 PivotPos = transform.position;
		PivotPos += PanOffsetX * Time.unscaledDeltaTime;

		Ray ray3 = new Ray(PivotPos, Cam.transform.forward);
		RaycastHit hit3;
		if (Physics.Raycast(ray3, out hit3, MaxRaycastDistance, Mask))
		{
			LastLocalCamPos += Pivot.InverseTransformVector(PanOffsetX) * Time.unscaledDeltaTime;
			TargetLocalCamPos += Pivot.InverseTransformVector(PanOffsetX) * Time.unscaledDeltaTime;
			transform.position = PivotPos;
		}

		PivotPos = transform.position;
		PivotPos += PanOffsetZ * Time.unscaledDeltaTime;
		ray3 = new Ray(PivotPos, Cam.transform.forward);
		if (Physics.Raycast(ray3, out hit3, MaxRaycastDistance, Mask))
		{
			LastLocalCamPos += Pivot.InverseTransformVector(PanOffsetZ) * Time.unscaledDeltaTime;
			TargetLocalCamPos += Pivot.InverseTransformVector(PanOffsetZ) * Time.unscaledDeltaTime;
			transform.position = PivotPos;
		}


		PanOffsetX = Vector3.Lerp(PanOffsetX, Vector3.zero, Time.unscaledDeltaTime * SmoothPan);
		PanOffsetZ = Vector3.Lerp(PanOffsetZ, Vector3.zero, Time.unscaledDeltaTime * SmoothPan);

		if (Input.GetKeyDown(KeyCode.Home))
		{
			RestartCam();

			if (transform.localPosition.y < MinDistance)
			{
				Vector3 LocalPos = transform.localPosition;
				LocalPos.y = MinDistance;
				transform.localPosition = LocalPos;
			}
			else if(transform.localPosition.y > MaxDistance)
			{
				Vector3 LocalPos = transform.localPosition;
				LocalPos.y = MaxDistance;
				transform.localPosition = LocalPos;
			}
		}

		Ray ray2 = new Ray(transform.position, Cam.transform.forward);
		RaycastHit hit2;
		if (Physics.Raycast(ray2, out hit2, MaxRaycastDistance, Mask))
		{
			SetNewPivotPos(hit2.point);
			//ClampPosY();
		}
		else
		{

		}

		Pivot.localRotation = Quaternion.Lerp(Pivot.localRotation, Quaternion.Euler(Rot), Time.unscaledDeltaTime * SmoothRot);

	}

	void SetNewPivotPos(Vector3 Pos)
	{
		Vector3 WorldPos = transform.position;
		Pivot.position = Pos;
		Vector3 Offset = Pivot.InverseTransformVector(transform.position - WorldPos);
		transform.position = WorldPos;

		LastLocalCamPos -= Offset;
		TargetLocalCamPos -= Offset;
	}


	public static Vector3 BufforedGameplayCursorPos
	{
		get
		{
			return Current.GameplayCursorPos;
		}
	}

	Vector3 GameplayCursorPos;
	void CursorUiPos()
	{

		Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, MaxRaycastDistance, MaskCursor))
		{
			GameplayCursorPos = hit.point;
			Vector3 GameplayCursorPosScm = ScmapEditor.WorldPosToScmap(GameplayCursorPos);
			GameplayCursorPosScm.y = hit.point.y * 10;
			GameplayCursorPosScm.z = ScmapEditor.Current.map.Height - GameplayCursorPosScm.z;
			string X = GameplayCursorPosScm.x.ToString("N2");
			string Y = GameplayCursorPosScm.y.ToString("N2");
			string Z = GameplayCursorPosScm.z.ToString("N2");

			X = X.PadRight(8);
			Y = Y.PadRight(8);
			Z = Z.PadRight(8);

			CursorInfo.text = "x: " + X + "\ty: " + Y + "\tz: " + Z;
		}
		else
		{
			CursorInfo.text = "x: --------  \ty: --------  \tz: --------  ";
		}
	}

	float SampleCamSpeed()
	{
		Ray ray = new Ray(Pivot.TransformPoint(TargetLocalCamPos), transform.forward);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, MaxRaycastDistance, Mask))
		{
			//return Mathf.Lerp(15f, 300, Mathf.Pow(hit.distance / 150, 1.2f)) * 1.2f;
			//return Mathf.Pow(hit.distance / 1000f, 2f) * 1000f;
			//float Value = Mathf.Lerp(15f, 2048, hit.distance / 2048f);
			float Value = hit.distance * 2.6f;
			if (Value < 5)
				Value = 5;
			return Value;
		}
		return 0;
	}
}
