using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		public SelectedObjects Selection = new SelectedObjects();
		public SelectedObjects[] SymetrySelection;

		[System.Serializable]
		public class SelectedObjects
		{
			public List<int> Ids = new List<int>();
			public List<Vector3> Empty = new List<Vector3>();
			public GameObject[] SelectionRings = new GameObject[0];
			public Vector3[] Positions;
			public Quaternion[] Rotations;
			public Matrix4x4 SymmetryMatrix;

			public void LoadSymetryIds()
			{
				Ids = new List<int>();
				Empty = new List<Vector3>();

				int i = 0;
				int o = 0;
				int count = Current.Selection.Ids.Count;
				int ID = 0;
				Vector3 SearchPos = Vector3.zero;
				float Dist = 0;
				int ClosestO;
				float ClosestDist;

				for (i = 0; i < count; i++)
				{
					ID = Current.Selection.Ids[i];
					SearchPos = SymmetryMatrix.MultiplyPoint(Current.AffectedGameObjects[ID].transform.localPosition - MapLuaParser.Current.MapCenterPoint) + MapLuaParser.Current.MapCenterPoint;
					ClosestO = -1;
					ClosestDist = 1000000;

					for (o = 0; o < Current.AffectedGameObjects.Length; o++)
					{
						if (o == ID)
							continue;
						Dist = Vector3.Distance(Current.AffectedGameObjects[o].transform.localPosition, SearchPos);
						if (Dist <= Current.LastTolerance && Dist < ClosestDist)
						{
							ClosestO = o;
							ClosestDist = Dist;
						}
					}

					if (ClosestO >= 0)
						Ids.Add(ClosestO);
					else
						Empty.Add(SearchPos);
				}
			}

			public void StorePositions()
			{
				int count = Ids.Count;
				Positions = new Vector3[count + Empty.Count];
				for(int i = 0; i < count; i++)
				{
					Positions[i] = Current.AffectedGameObjects[Ids[i]].transform.localPosition;
				}

				for(int i = 0; i < Empty.Count; i++)
				{
					Positions[i + count] = Empty[i];

				}
			}

			public void StoreRotations()
			{
				int count = Ids.Count;
				Rotations = new Quaternion[count + Empty.Count];
				for (int i = 0; i < count; i++)
				{
					Rotations[i] = Current.AffectedGameObjects[Ids[i]].transform.localRotation;
				}

				for (int i = 0; i < Empty.Count; i++)
				{
					//Rotations[i + count] = Empty[i];
				}
			}

			public void OffsetPosition(Vector3 Offset)
			{
				Offset = SymmetryMatrix.MultiplyPoint(Offset);
				int count = Ids.Count;
				if (Current.SnapToGrid)
				{
					for (int i = 0; i < Positions.Length; i++)
					{
						if (i >= count)
						{
							SelectionRings[i].transform.localPosition = ScmapEditor.SnapToGridCenter(Positions[i] + Offset, true, Current.SnapToWater);
						}
						else
						{
							Current.AffectedGameObjects[Ids[i]].transform.localPosition = ScmapEditor.SnapToGridCenter(Positions[i] + Offset, true, Current.SnapToWater);
							SelectionRings[i].transform.localPosition = Current.AffectedGameObjects[Ids[i]].transform.localPosition;
						}
					}
				}
				else
				{
					for (int i = 0; i < Positions.Length; i++)
					{
						Current.AffectedGameObjects[Ids[i]].transform.localPosition = Positions[i] + Offset;
						SelectionRings[i].transform.localPosition = Current.AffectedGameObjects[Ids[i]].transform.localPosition;
					}
				}
			}

			public void OffsetRotation(Quaternion Offset, Vector3[] PositionOffsets)
			{

			}
		}

		public int GetIdOfObject(GameObject Obj)
		{
			for(int i = 0; i < AffectedGameObjects.Length; i++)
			{
				if (AffectedGameObjects[i] == Obj)
					return i;
			}
			return -1;
		}


		public void FinishSelectionChange()
		{
			Selection.SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
			UpdateSelectionRing();
			if(AllowSymmetry)
				GenerateSymmetry();
			EditMap.MarkerSelectionOptions.UpdateOptions();

			if(SelectionChangeAction != null)
				SelectionChangeAction();
		}

		int LastSym = 0;
		float LastTolerance;
		void GenerateSymmetry()
		{
			LastSym = PlayerPrefs.GetInt("Symmetry", 0);
			LastTolerance = SymmetryWindow.GetTolerance();


			switch (LastSym)
			{
				case 1: // X
					SymetrySelection = new SelectedObjects[1];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);
					break;
				case 2: // Z
					SymetrySelection = new SelectedObjects[1];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);
					break;
				case 3: // XZ
					SymetrySelection = new SelectedObjects[1];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, -1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);
					break;
				case 4: // X Z XZ
					SymetrySelection = new SelectedObjects[3];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);

					SymetrySelection[1] = new SelectedObjects();
					SymetrySelection[1].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
					SymetrySelection[1].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[1]);

					SymetrySelection[2] = new SelectedObjects();
					SymetrySelection[2].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, -1));
					SymetrySelection[2].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[2]);
					break;
				case 5:// Diagonal1
					SymetrySelection = new SelectedObjects[1];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * 90), new Vector3(-1, 1, 1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);
					break;
				case 6: // Diagonal 2
					SymetrySelection = new SelectedObjects[1];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.down * 90), new Vector3(-1, 1, 1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);
					break;
				case 7: // Diagonal 3
					SymetrySelection = new SelectedObjects[2];
					SymetrySelection[0] = new SelectedObjects();
					SymetrySelection[0].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * 90), new Vector3(-1, 1, 1));
					SymetrySelection[0].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[0]);

					SymetrySelection[1] = new SelectedObjects();
					SymetrySelection[1].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.down * 90), new Vector3(-1, 1, 1));
					SymetrySelection[1].LoadSymetryIds();
					GenerateSymmetrySelectionRing(SymetrySelection[1]);

					break;
				case 8: // Rotation
					int RotCount = PlayerPrefs.GetInt("SymmetryAngleCount", 2) - 1;
					float angle = 360.0f / (float)(RotCount + 1);
					SymetrySelection = new SelectedObjects[RotCount];

					for (int i = 0; i < RotCount; i++)
					{
						SymetrySelection[i] = new SelectedObjects();
						SymetrySelection[i].SymmetryMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.up * (angle * (i + 1))), Vector3.one);
						SymetrySelection[i].LoadSymetryIds();
						GenerateSymmetrySelectionRing(SymetrySelection[i]);
					}
					break;
				default:
					SymetrySelection = new SelectedObjects[0];
					break;
			}
		}

	}
}