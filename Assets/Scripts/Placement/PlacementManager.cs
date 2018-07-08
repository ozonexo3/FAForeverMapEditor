// ******************************************************************************
// *
// * System for creating new objects. Can handle multiple objects, symmetry, rotation and scale.
// * It take only GameObject and some voids for actions and returns arrays of transforms
// * Copyright ozonexo3 2017
// * 
// ******************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selection;
using FAF.MapEditor;

public class PlacementManager : MonoBehaviour {


	public static PlacementManager Current;

	private void Awake()
	{
		Current = this;
	}


	static System.Action<Vector3[], Quaternion[], Vector3[]> CurrentPlaceAction;
	public static System.Action<GameObject> InstantiateAction;
	public static int MinRotAngle = 90;
	public GameObject PlacementObject;
	public LayerMask RaycastMask;
	public Camera Cam;

	static bool _SnapToWater = false;
	public static bool SnapToWater
	{
		set
		{
			_SnapToWater = value;
		}
		get
		{
			return _SnapToWater;
		}
	}

	//int PlaceAngle = 0;


	static Quaternion OldRot;
	static Vector3 OldScale;
	public static void BeginPlacement(GameObject Prefab, System.Action<Vector3[], Quaternion[], Vector3[]> PlaceAction, bool ResetTransform = true)
	{
		if (!ResetTransform && Current.PlacementObject)
		{
			OldRot = Current.PlacementObject.transform.rotation;
			OldScale = Current.PlacementObject.transform.localScale;
		}
		else
		{
			OldRot = Quaternion.identity;
			OldScale = Vector3.one;
		}

		Clear();

		Current.PlacementObject = Instantiate(Prefab) as GameObject;
		Current.PlacementObject.SetActive(true);
		if (InstantiateAction != null)
			InstantiateAction(Current.PlacementObject);

		Current.PlacementObject.transform.rotation = OldRot;
		Current.PlacementObject.transform.localScale = OldScale;

		Current.PlacementObject.SetActive(false);
		CurrentPlaceAction = PlaceAction;
		Current.GenerateSymmetry();
		Current.enabled = true;
		ChangeControlerType.ChangeCurrentControler(0);
		SymmetryWindow.OnSymmetryChanged += Current.UpdateSymmetry;

	}

	public static void Clear()
	{
		Destroy(Current.PlacementObject);
		for (int i = 0; i < Current.PlacementSymmetry.Length; i++)
			if (Current.PlacementSymmetry[i])
				DestroyImmediate(Current.PlacementSymmetry[i]);

		Current.enabled = false;
		SymmetryWindow.OnSymmetryChanged -= Current.UpdateSymmetry;
	}

	Vector3 RotationStartMousePos;
	Vector3 StartRotation;
	Vector3 LastHitPoint;
	Vector3 StartScale;
	void Update () {

		if (!PlacementObject)
		{
			enabled = false;
			return;
		}

		if (!SelectionManager.Current.IsPointerOnGameplay())
			return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1000, RaycastMask))
		{
			if (LastSym != SymmetryWindow.GetSymmetryType())
				GenerateSymmetry();

			if (!PlacementObject.activeSelf)
			{
				PlacementObject.SetActive(true);
				for (int i = 0; i < PlacementSymmetry.Length; i++)
					PlacementSymmetry[i].SetActive(true);
			}


			// Rotate
			bool Rotating = false;
			if (SelectionManager.AllowRotation)
			{
				if (Input.GetKeyDown(KeyCode.E))
				{
					/*if (MinRotAngle > 1)
					{
						PlaceAngle += MinRotAngle;
						if (PlaceAngle >= 360)
							PlaceAngle = 0;
						PlacementObject.transform.rotation = Quaternion.Euler(Vector3.up * PlaceAngle);
					}
					else*/
					{
						RotationStartMousePos = Input.mousePosition;
						StartRotation = PlacementObject.transform.eulerAngles;
					}
				}
				else if (Input.GetKeyUp(KeyCode.E))
				{
					ChangeControlerType.ChangeCurrentControler(0);
				}
				else if (Input.GetKey(KeyCode.E))
				{
					Rotating = true;

					Vector3 NewRot = StartRotation + Vector3.down * ((Input.mousePosition.x - RotationStartMousePos.x) * 0.5f);

					if (MinRotAngle > 1)
					{
						int ClampedRot = (int)(NewRot.y / MinRotAngle);
						NewRot.y = MinRotAngle * ClampedRot;
					}

					PlacementObject.transform.eulerAngles = NewRot;

				}
			}

			//Scale
			bool Scaling = false;
			if (!Rotating && SelectionManager.AllowScale)
			{
				if (Input.GetKeyDown(KeyCode.R))
				{
					RotationStartMousePos = Input.mousePosition;
					StartScale = PlacementObject.transform.localScale;
					LastHitPoint = hit.point;
				}
				else if (Input.GetKeyUp(KeyCode.R))
				{
					ChangeControlerType.ChangeCurrentControler(0);
				}
				else if (Input.GetKey(KeyCode.R))
				{
					Scaling = true;

					Vector3 Scale = StartScale;
					float ScalingScale = (Scale.x + Scale.y + Scale.z) / 3f;
					float ScalePower = (Input.mousePosition.x - RotationStartMousePos.x) * 0.003f * ScalingScale;

					Scale.x = Mathf.Clamp(Scale.x + ScalePower, 0.005f, 100);
					Scale.y = Mathf.Clamp(Scale.y + ScalePower, 0.005f, 100);
					Scale.z = Mathf.Clamp(Scale.z + ScalePower, 0.005f, 100);
					PlacementObject.transform.localScale = Scale;
				}
			}

			if (!Rotating)
			{
				Vector3 Point = hit.point;
				if (Scaling)
					Point = LastHitPoint;

				if (SelectionManager.Current.SnapToGrid)
					PlacementObject.transform.position = ScmapEditor.SnapToGridCenter(Point, true, SnapToWater);
				else if (SnapToWater)
					PlacementObject.transform.position = ScmapEditor.ClampToWater(Point);
				else
					PlacementObject.transform.position = Point;
			}

			UpdateSymmetryObjects();

			// Action
			if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.R) || KeyboardManager.BrushSizeHold() || KeyboardManager.BrushStrengthHold())
			{
			}
			else if (Input.GetMouseButtonDown(0))
			{
				Place();
			}
		}
		else if (PlacementObject.activeSelf)
		{
			PlacementObject.SetActive(false);
			for (int i = 0; i < PlacementSymmetry.Length; i++)
				PlacementSymmetry[i].SetActive(false);
		}
	}

	void UpdateSymmetryObjects()
	{

		Vector3 Euler = PlacementObject.transform.localRotation.eulerAngles;

		for (int i = 0; i < PlacementSymmetry.Length; i++)
		{
			Vector3 SymmetryPoint = SymmetryMatrix[i].MultiplyPoint(PlacementObject.transform.position - MapLuaParser.Current.MapCenterPoint) + MapLuaParser.Current.MapCenterPoint;

			PlacementSymmetry[i].transform.localPosition = SymmetryPoint;
			PlacementSymmetry[i].transform.localRotation = Quaternion.Euler(new Vector3(Euler.x, Euler.y * (InvertRotation[i] ? (-1) : (1)), Euler.z)) * MassMath.QuaternionFromMatrix(SymmetryMatrix[i]);
			PlacementSymmetry[i].transform.localScale = PlacementObject.transform.localScale;
		}
	}

	void Place()
	{
		Vector3[] Positions = new Vector3[SymmetryMatrix.Length + 1];
		Quaternion[] Rotations = new Quaternion[SymmetryMatrix.Length + 1];
		Vector3[] Scales = new Vector3[SymmetryMatrix.Length + 1];

		Positions[0] = PlacementObject.transform.position;
		Rotations[0] = PlacementObject.transform.rotation;
		Scales[0] = PlacementObject.transform.localScale;

		for (int i = 0; i < SymmetryMatrix.Length; i++)
		{
			Positions[i + 1] = PlacementSymmetry[i].transform.position;
			Rotations[i + 1] = PlacementSymmetry[i].transform.rotation;
			Scales[i + 1] = PlacementObject.transform.localScale;
		}

		CurrentPlaceAction(Positions, Rotations, Scales);
	}

	public delegate void DropAction();
	public static event DropAction OnDropOnGameplay;

	public void DropAtGameplay()
	{
		if (ResourceBrowser.DragedObject == null)
			return;

		//string DropPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
		if(OnDropOnGameplay != null)
		OnDropOnGameplay.Invoke();
	}


	public static void PlaceAtPosition(Vector3 Position, GameObject Prefab, System.Action<Vector3[], Quaternion[], Vector3[]> PlaceAction, bool ResetTransform = true)
	{
		BeginPlacement(Prefab, PlaceAction, ResetTransform);

		Current.PlacementObject.transform.localPosition = Position;
		Current.PlacementObject.transform.localRotation = OldRot;
		Current.PlacementObject.transform.localScale = OldScale;

		Current.UpdateSymmetryObjects();

		Current.Place();
		Clear();
	}


	public void UpdateSymmetry()
	{
		GenerateSymmetry();
	}


	int LastSym = 0;
	//float LastTolerance;
	public Matrix4x4[] SymmetryMatrix;
	public bool[] InvertRotation;
	public GameObject[] PlacementSymmetry;
	public void GenerateSymmetry()
	{
		LastSym = SymmetryWindow.GetSymmetryType();
		//LastTolerance = SymmetryWindow.GetTolerance();

		for(int i = 0; i < PlacementSymmetry.Length; i++)
		{
			Destroy(PlacementSymmetry[i]);
		}

		switch (LastSym)
		{
			case 1: // X
				SymmetryMatrix = new Matrix4x4[1];
				InvertRotation = new bool[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
				InvertRotation[0] = true;
				break;
			case 2: // Z
				SymmetryMatrix = new Matrix4x4[1];
				InvertRotation = new bool[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
				InvertRotation[0] = true;
				break;
			case 3: // XZ
				SymmetryMatrix = new Matrix4x4[1];
				InvertRotation = new bool[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, -1));
				InvertRotation[0] = false;
				break;
			case 4: // X Z XZ
				SymmetryMatrix = new Matrix4x4[3];
				InvertRotation = new bool[3];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
				InvertRotation[0] = true;

				SymmetryMatrix[1] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
				InvertRotation[1] = true;

				SymmetryMatrix[2] = SymmetryMatrix[0] * SymmetryMatrix[1];
				InvertRotation[2] = false;
				//SymmetryMatrix[2] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, -1));
				break;
			case 5:// Diagonal1
				SymmetryMatrix = new Matrix4x4[1];
				InvertRotation = new bool[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * 90), new Vector3(-1, 1, 1));
				InvertRotation[0] = true;
				break;
			case 6: // Diagonal 2
				SymmetryMatrix = new Matrix4x4[1];
				InvertRotation = new bool[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.down * 90), new Vector3(-1, 1, 1));
				InvertRotation[0] = true;
				break;
			case 7: // Diagonal 3
				SymmetryMatrix = new Matrix4x4[3];
				InvertRotation = new bool[3];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * 90), new Vector3(-1, 1, 1));
				InvertRotation[0] = true;
				SymmetryMatrix[1] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.down * 90), new Vector3(-1, 1, 1));
				InvertRotation[1] = true;
				SymmetryMatrix[2] = SymmetryMatrix[0] * SymmetryMatrix[1];
				InvertRotation[2] = false;
				break;
			case 8: // Rotation
				int RotCount = SymmetryWindow.GetRotationSym() - 1;
				float angle = 360.0f / (float)(RotCount + 1);
				SymmetryMatrix = new Matrix4x4[RotCount];
				InvertRotation = new bool[RotCount];

				for (int i = 0; i < RotCount; i++)
				{
					SymmetryMatrix[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * (angle * (i + 1))), Vector3.one);
					InvertRotation[i] = false;
				}
				break;
			default:
				SymmetryMatrix = new Matrix4x4[0];
				break;
		}

		PlacementSymmetry = new GameObject[SymmetryMatrix.Length];

		for(int i = 0; i < PlacementSymmetry.Length; i++)
		{
			PlacementSymmetry[i] = Instantiate(PlacementObject) as GameObject;
			if(InstantiateAction != null)
				InstantiateAction(PlacementSymmetry[i]);
		}


	}
}
