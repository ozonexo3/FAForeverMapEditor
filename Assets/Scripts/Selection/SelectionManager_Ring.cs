using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		[Header("Rings")]
		public Transform Controls;
		public GameObject Controls_Position;
		public GameObject Controls_Up;
		public GameObject Controls_Rotate;
		public GameObject Controls_RotateX;
		public GameObject Controls_Scale;

		public RingPrefab[] RingPrefabs;

		[System.Serializable]
		public class RingPrefab
		{
			public GameObject SmallRing;
			public GameObject SmallRingSymmetry;
			public GameObject SmallRingEmpty;
		}



		List<GameObject> SelectionRings = new List<GameObject>();

		private void UpdateSelectionRing()
		{
			foreach (GameObject obj in SelectionRings)
				Destroy(obj);
			SelectionRings = new List<GameObject>();

			int count = Selection.Ids.Count;
			if (count == 0 || AffectedGameObjects.Length == 0)
			{
				Controls.gameObject.SetActive(false);
			}
			else
			{
				Controls.gameObject.SetActive(true);

				Bounds NewBounds = new Bounds();

				Selection.SelectionRings = new GameObject[count];
				for (int i = 0; i < count; i++)
				{
					int ID = Selection.Ids[i];

					if (AffectedGameObjects[ID] == null)
						continue;

					if (i == 0)
					{
						NewBounds.center = AffectedGameObjects[ID].transform.localPosition;
						NewBounds.size = Vector3.zero;
					}
					else
					{
						NewBounds.Encapsulate(AffectedGameObjects[ID].transform.localPosition);
					}

					Selection.SelectionRings[i] = Instantiate(RingPrefabs[SelPrefab].SmallRing, transform) as GameObject;
					SelectionRings.Add(Selection.SelectionRings[i]);
					Selection.SelectionRings[i].transform.localPosition = AffectedGameObjects[ID].transform.localPosition;
					Selection.SelectionRings[i].SetActive(true);

					if(LastControlType == SelectionControlTypes.Decal)
					{
						Selection.SelectionRings[i].transform.localScale = AffectedGameObjects[ID].transform.localScale;
						Selection.SelectionRings[i].transform.localRotation = AffectedGameObjects[ID].transform.localRotation;
					}
					else
					{
						MeshRenderer Mr = AffectedGameObjects[ID].GetComponent<MeshRenderer>();
						if (Mr)
						{
							float ScaleMax = Mathf.Max(Mr.bounds.size.x, Mr.bounds.size.z) + 0.2f;
							Selection.SelectionRings[i].transform.localScale = new Vector3(ScaleMax, 1, ScaleMax);
						}
					}

				}

				Controls.localPosition = NewBounds.center;
				if (RotateControls() && count == 1)
					Controls.localRotation = AffectedGameObjects[Selection.Ids[0]].transform.localRotation;
				else
					Controls.localRotation = Quaternion.identity;
				//float Size = Mathf.Clamp(Mathf.Max(NewBounds.size.x, NewBounds.size.z), 0.2f, 10000);
				//Ring.localScale = new Vector3(Size, 1, Size);
			}
		}

		bool RotateControls()
		{
			return (ChangeControlerType.ControlerId == 1 || ChangeControlerType.ControlerId == 2) &&
					LastControlType == SelectionControlTypes.Decal;
		}

		private void GenerateSymmetrySelectionRing(SelectedObjects Sel)
		{
			int count = Sel.Ids.Count;
			int count2 = Sel.Empty.Count;
			Sel.SelectionRings = new GameObject[count + count2];
			for (int i = 0; i < count; i++)
			{
				int ID = Sel.Ids[i];

				Sel.SelectionRings[i] = Instantiate(RingPrefabs[SelPrefab].SmallRingSymmetry, transform) as GameObject;
				SelectionRings.Add(Sel.SelectionRings[i]);
				Sel.SelectionRings[i].transform.localPosition = AffectedGameObjects[ID].transform.localPosition;
				Sel.SelectionRings[i].SetActive(true);

				if (LastControlType == SelectionControlTypes.Decal)
				{
					Sel.SelectionRings[i].transform.localScale = AffectedGameObjects[ID].transform.localScale;
					Sel.SelectionRings[i].transform.localRotation = AffectedGameObjects[ID].transform.localRotation;
				}
				else
				{
					MeshRenderer Mr = AffectedGameObjects[ID].GetComponent<MeshRenderer>();
					if (Mr)
						Sel.SelectionRings[i].transform.localScale = new Vector3(Mr.bounds.size.x + 0.2f, 1, Mr.bounds.size.z + 0.2f);
				}
			}

			for(int e = 0; e < count2; e++)
			{
				int i = e + count;

				Sel.SelectionRings[i] = Instantiate(RingPrefabs[SelPrefab].SmallRingEmpty, transform) as GameObject;
				SelectionRings.Add(Sel.SelectionRings[i]);
				Sel.SelectionRings[i].transform.localPosition = Sel.Empty[e];
				Sel.SelectionRings[i].SetActive(true);
			}
		}

		private void ResetControlerPosition()
		{

			int count = Selection.Ids.Count;
			Bounds NewBounds = new Bounds();
			for (int i = 0; i < count; i++)
			{
				int ID = Selection.Ids[i];

				if (i == 0)
				{
					NewBounds.center = AffectedGameObjects[ID].transform.localPosition;
					NewBounds.size = Vector3.zero;
				}
				else
				{
					NewBounds.Encapsulate(AffectedGameObjects[ID].transform.localPosition);
				}
			}

			Controls.localPosition = NewBounds.center;
			if (RotateControls() && count == 1)
				Controls.localRotation = AffectedGameObjects[Selection.Ids[0]].transform.localRotation;
			else
				Controls.localRotation = Quaternion.identity;
		}
		
	}
}
