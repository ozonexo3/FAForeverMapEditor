using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selection;

public class PlacementManager : MonoBehaviour {


	public static PlacementManager Current;

	private void Awake()
	{
		Current = this;
	}


	static System.Action<Vector3[], Quaternion[]> CurrentPlaceAction;
	public GameObject PlacementObject;
	public LayerMask RaycastMask;
	public Camera Cam;

	int PlaceAngle = 0;

	public static void BeginPlacement(GameObject Prefab, System.Action<Vector3[], Quaternion[]> PlaceAction)
	{
		Clear();

		Current.PlacementObject = Instantiate(Prefab) as GameObject;
		Current.PlacementObject.SetActive(false);
		CurrentPlaceAction = PlaceAction;
		Current.GenerateSymmetry();
		Current.enabled = true;
	}

	public static void Clear()
	{
		Destroy(Current.PlacementObject);
		for (int i = 0; i < Current.PlacementSymmetry.Length; i++)
			if (Current.PlacementSymmetry[i])
				Destroy(Current.PlacementSymmetry[i]);


		Current.enabled = false;

	}

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
			if (LastSym != PlayerPrefs.GetInt("Symmetry", 0))
				GenerateSymmetry();

			if (!PlacementObject.activeSelf)
			{
				PlacementObject.SetActive(true);
				for (int i = 0; i < PlacementSymmetry.Length; i++)
					PlacementSymmetry[i].SetActive(true);
			}


			// Rotate
			if (Input.GetKeyDown(KeyCode.R))
			{
				PlaceAngle += 90;
				if (PlaceAngle >= 360)
					PlaceAngle = 0;
				PlacementObject.transform.rotation = Quaternion.Euler(Vector3.up * PlaceAngle);
			}


			if (SelectionManager.Current.SnapToGrid)
				PlacementObject.transform.position = ScmapEditor.SnapToGridCenter(hit.point);
			else
				PlacementObject.transform.position = hit.point;

			for (int i = 0; i < PlacementSymmetry.Length; i++)
			{
				Vector3 SymmetryPoint = SymmetryMatrix[i].MultiplyPoint(PlacementObject.transform.position - MapLuaParser.Current.MapCenterPoint) + MapLuaParser.Current.MapCenterPoint;

				PlacementSymmetry[i].transform.position = SymmetryPoint;
				PlacementSymmetry[i].transform.rotation = PlacementObject.transform.rotation * MassMath.QuaternionFromMatrix(SymmetryMatrix[i]);

			}


			// Action
			if (Input.GetMouseButtonDown(0))
			{

				Vector3[] Positions = new Vector3[SymmetryMatrix.Length + 1];
				Quaternion[] Rotations = new Quaternion[SymmetryMatrix.Length + 1];

				Positions[0] = PlacementObject.transform.position;
				Rotations[0] = PlacementObject.transform.rotation;

				for(int i = 0; i < SymmetryMatrix.Length; i++)
				{
					Positions[i + 1] = PlacementSymmetry[i].transform.position;
					Rotations[i + 1] = PlacementSymmetry[i].transform.rotation;
				}

				CurrentPlaceAction(Positions, Rotations);
			}
		}
		else if (PlacementObject.activeSelf)
		{
			PlacementObject.SetActive(false);
			for (int i = 0; i < PlacementSymmetry.Length; i++)
				PlacementSymmetry[i].SetActive(false);
		}
	}


	int LastSym = 0;
	float LastTolerance;
	public Matrix4x4[] SymmetryMatrix;
	public GameObject[] PlacementSymmetry;
	public void GenerateSymmetry()
	{
		LastSym = PlayerPrefs.GetInt("Symmetry", 0);
		LastTolerance = SymmetryWindow.GetTolerance();

		for(int i = 0; i < PlacementSymmetry.Length; i++)
		{
			Destroy(PlacementSymmetry[i]);
		}

		switch (LastSym)
		{
			case 1: // X
				SymmetryMatrix = new Matrix4x4[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
				break;
			case 2: // Z
				SymmetryMatrix = new Matrix4x4[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
				break;
			case 3: // XZ
				SymmetryMatrix = new Matrix4x4[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, -1));
				break;
			case 4: // X Z XZ
				SymmetryMatrix = new Matrix4x4[3];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));

				SymmetryMatrix[1] = new Matrix4x4();
				SymmetryMatrix[1] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

				SymmetryMatrix[2] = new Matrix4x4();
				SymmetryMatrix[2] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, -1));
				break;
			case 5:// Diagonal1
				SymmetryMatrix = new Matrix4x4[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * 90), new Vector3(-1, 1, 1));
				break;
			case 6: // Diagonal 2
				SymmetryMatrix = new Matrix4x4[1];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.down * 90), new Vector3(-1, 1, 1));
				break;
			case 7: // Diagonal 3
				SymmetryMatrix = new Matrix4x4[2];
				SymmetryMatrix[0] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * 90), new Vector3(-1, 1, 1));

				SymmetryMatrix[1] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.down * 90), new Vector3(-1, 1, 1));

				break;
			case 8: // Rotation
				int RotCount = PlayerPrefs.GetInt("SymmetryAngleCount", 2) - 1;
				float angle = 360.0f / (float)(RotCount + 1);
				SymmetryMatrix = new Matrix4x4[RotCount];

				for (int i = 0; i < RotCount; i++)
				{
					SymmetryMatrix[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * (angle * (i + 1))), Vector3.one);
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
		}


	}
}
