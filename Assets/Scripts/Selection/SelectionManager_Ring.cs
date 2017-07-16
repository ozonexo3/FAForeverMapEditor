using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		[Header("Rings")]
		public Transform Controls;
		public GameObject Controls_Up;
		public GameObject Controls_Rotate;
		public GameObject Controls_Scale;
		public GameObject SmallRing;
		public GameObject SmallRingSymmetry;

		List<GameObject> SelectionRings = new List<GameObject>();

		private void UpdateSelectionRing()
		{
			foreach (GameObject obj in SelectionRings)
				Destroy(obj);
			SelectionRings = new List<GameObject>();

			int count = Selection.Ids.Count;
			if (count == 0)
			{
				Controls.gameObject.SetActive(false);
			}
			else
			{
				Controls.gameObject.SetActive(AllowMove || AllowUp);

				Bounds NewBounds = new Bounds();

				Selection.SelectionRings = new GameObject[count];
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

					Selection.SelectionRings[i] = Instantiate(SmallRing, transform) as GameObject;
					SelectionRings.Add(Selection.SelectionRings[i]);
					Selection.SelectionRings[i].transform.localPosition = AffectedGameObjects[ID].transform.localPosition;
					Selection.SelectionRings[i].SetActive(true);

					MeshRenderer Mr = AffectedGameObjects[ID].GetComponent<MeshRenderer>();
					if (Mr)
						Selection.SelectionRings[i].transform.localScale = new Vector3(Mr.bounds.size.x + 0.2f, 1, Mr.bounds.size.z + 0.2f);
				}

				Controls.localPosition = NewBounds.center;
				//float Size = Mathf.Clamp(Mathf.Max(NewBounds.size.x, NewBounds.size.z), 0.2f, 10000);
				//Ring.localScale = new Vector3(Size, 1, Size);
			}
		}

		private void GenerateSymmetrySelectionRing(SelectedObjects Sel)
		{
			int count = Sel.Ids.Count;
			Sel.SelectionRings = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				int ID = Sel.Ids[i];

				Sel.SelectionRings[i] = Instantiate(SmallRingSymmetry, transform) as GameObject;
				SelectionRings.Add(Sel.SelectionRings[i]);
				Sel.SelectionRings[i].transform.localPosition = AffectedGameObjects[ID].transform.localPosition;
				Sel.SelectionRings[i].SetActive(true);

				MeshRenderer Mr = AffectedGameObjects[ID].GetComponent<MeshRenderer>();
				if (Mr)
					Sel.SelectionRings[i].transform.localScale = new Vector3(Mr.bounds.size.x + 0.2f, 1, Mr.bounds.size.z + 0.2f);
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

		}
		
	}
}
