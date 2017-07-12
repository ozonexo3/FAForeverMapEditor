using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		[Header("Controls")]
		public LayerMask SelectionLayers;

		bool PointerOnGameplay;
		DragTypes DragType;
		Vector3 BeginMousePos = Vector3.zero;


		enum DragTypes
		{
			None, SelectionBox, Move, Rotate, Scale
		}


		#region Events
		public bool IsPointerOnGameplay()
		{
			return PointerOnGameplay;
		}

		public void PointerEnter()
		{
			PointerOnGameplay = true;
		}

		public void PointerExit()
		{
			PointerOnGameplay = false;
		}

		public void Click()
		{
			Debug.Log("Click");
			ClickOnScreen();
		}

		public void DragInit()
		{
			BeginMousePos = Input.mousePosition;
			DragType = DragTypes.SelectionBox;
			//TODO Add Move/Rotate/Scale
		}

		public void DragBegin()
		{
			Debug.Log("Drag Begin");
			if(DragType == DragTypes.SelectionBox)
			{
				UpdateSelectionBox();
			}
		}

		public void DragUpdate()
		{
			// UpdateBox
			Debug.Log("Drag Update");
			if (DragType == DragTypes.SelectionBox)
			{
				UpdateSelectionBox();
			}
		}

		public void DragEnd()
		{
			DragType = DragTypes.None;
			Debug.Log("Drag End");
			if (DragType == DragTypes.SelectionBox)
			{
				UpdateSelectionBox(false);
				AddSelectionBoxObjects();
			}
		}
		#endregion


		#region Input
		bool IsSelectionAdd()
		{
			return Input.GetKeyDown(KeyCode.LeftShift);
		}

		bool IsSelectionRemove()
		{
			return Input.GetKeyDown(KeyCode.LeftAlt);
		}
		#endregion

		#region Click On Object

		void ClickOnScreen()
		{

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, SelectionLayers))
			{
				int ObjectId = GetIdOfObject(hit.collider.gameObject);

				if(ObjectId >= 0)
				{
					if (IsSelectionRemove())
					{
						if (!Selection.Ids.Contains(ObjectId))
						{
							Selection.Ids.Remove(ObjectId);
							FinishSelectionChange();
						}
					}
					else if (IsSelectionAdd())
					{
						if (!Selection.Ids.Contains(ObjectId))
						{
							Selection.Ids.Add(ObjectId);
							FinishSelectionChange();
						}
					}
					else
					{
						if (Selection.Ids.Count == 1 && Selection.Ids[0] == ObjectId)
						{ }
						else {
							Selection.Ids = new List<int>();
							Selection.Ids.Add(ObjectId);
							FinishSelectionChange();
						}
					}

				}
			}
		}

		#endregion

		#region Selection Box
		void UpdateSelectionBox(bool On = true)
		{
			if (On)
			{
				Vector2 diference = Input.mousePosition - BeginMousePos;

				SelBox.sizeDelta = new Vector2(Mathf.Abs(diference.x), Mathf.Abs(diference.y));

				if (diference.x < 0 && diference.y < 0)
				{
					SelBox.anchoredPosition = Input.mousePosition;
				}
				else if (diference.x < 0)
				{
					SelBox.anchoredPosition = new Vector2(Input.mousePosition.x, BeginMousePos.y);
				}
				else if (diference.y < 0)
				{
					SelBox.anchoredPosition = new Vector2(BeginMousePos.x, Input.mousePosition.y);
				}
				else {
					SelBox.sizeDelta = diference;
					SelBox.anchoredPosition = BeginMousePos;
				}
			}

			SelBox.gameObject.SetActive(On);
		}


		void AddSelectionBoxObjects()
		{
			Vector3 MouseEndPos = Input.mousePosition;
			Vector3 diference = MouseEndPos - BeginMousePos;
			Rect SelectionBoxArea = new Rect(Mathf.Min(MouseEndPos.x, BeginMousePos.x), Mathf.Min(MouseEndPos.y, BeginMousePos.y), Mathf.Abs(diference.x), Mathf.Abs(diference.y));

			if (IsSelectionRemove())
			{
				// Add
				for (int i = 0; i < AfectedGameObjects.Length; i++)
				{
					if (Selection.Ids.Contains(i))
					{
						Selection.Ids.RemoveAt(Selection.Ids.IndexOf(i));
					}
				}
			}
			else {
				if (!IsSelectionAdd())
					Selection.Ids = new List<int>();

				for (int i = 0; i < AfectedGameObjects.Length; i++)
				{
					if (Selection.Ids.Contains(i))
						continue;

					if (SelectionBoxArea.Contains(GetComponent<Camera>().WorldToScreenPoint(AfectedGameObjects[i].transform.position)))
					{
						Selection.Ids.Add(i);
					}
				}
			}
		}
		#endregion


	}
}
