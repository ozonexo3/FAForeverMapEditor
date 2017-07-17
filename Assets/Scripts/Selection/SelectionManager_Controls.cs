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
		public LayerMask ControlerLayers;

		bool PointerOnGameplay;
		DragTypes DragType;
		Vector3 BeginMousePos = Vector3.zero;
		Vector3 ControlerBegin = Vector3.zero;
		Vector3 ControlerClickPoint = Vector3.zero;
		bool DragStarted = false;

		enum DragTypes
		{
			None, SelectionBox, MoveX, MoveZ, MoveXZ
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
			if (!Active)
				return;
			if (!Input.GetMouseButtonUp(0) || DragStarted)
				return;
			//Debug.Log("Click");
			ClickOnScreen();
		}

		public void DragInit()
		{
			BeginMousePos = Input.mousePosition;
			if (Input.GetMouseButtonDown(0))
			{

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 1000, ControlerLayers))
				{
					CreateControlerPlane(false);
					Controler_StorePositions();

					ControlerClickPoint = PosOnControler(ray);
					ControlerBegin = Controls.position;

					if (hit.collider.gameObject.name == "X")
						DragType = DragTypes.MoveX;
					else if (hit.collider.gameObject.name == "Z")
						DragType = DragTypes.MoveZ;
					else if (hit.collider.gameObject.name == "XZ")
						DragType = DragTypes.MoveXZ;
					else
						DragType = DragTypes.None;

				}
				else
				{
					DragType = DragTypes.SelectionBox;
				}
			}
			else
				DragType = DragTypes.None;
			//TODO Add Move/Rotate/Scale
		}

		public void DragBegin()
		{
			if (!Active)
				return;
			DragStarted = true;
			if (DragType == DragTypes.SelectionBox)
			{
				//Debug.Log("Drag Begin");

				UpdateSelectionBox();
			}
		}

		public void DragUpdate()
		{
			if (!Active)
				return;
			// UpdateBox
			if (DragType == DragTypes.SelectionBox)
			{
				//Debug.Log("Drag Update");

				UpdateSelectionBox();
			}
			else if (DragType == DragTypes.MoveX || DragType == DragTypes.MoveZ || DragType == DragTypes.MoveXZ)
			{
				ControlerDrag();
			}
		}

		public void DragEnd()
		{
			if (!Active)
				return;
			if (DragType == DragTypes.SelectionBox)
			{
				//Debug.Log("Drag End");

				UpdateSelectionBox(false);
				AddSelectionBoxObjects();
			}
			else if(DragType == DragTypes.MoveX || DragType == DragTypes.MoveZ || DragType == DragTypes.MoveXZ)
			{
				ControlerFinish();
			}
			else
			{

			}
			DragStarted = false;
			DragType = DragTypes.None;

		}
		#endregion


		#region Input
		bool IsSelectionAdd()
		{
			return Input.GetKey(KeyCode.LeftShift);
		}

		bool IsSelectionRemove()
		{
			return Input.GetKey(KeyCode.LeftAlt);
		}
		#endregion

		#region Click On Object

		void ClickOnScreen()
		{
			if (!AllowSelection)
				return;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, SelectionLayers))
			{
				SelectObject(hit.collider.gameObject);
			}
			else if(Selection.Ids.Count > 0)
			{
				Undo.Current.RegisterSelectionChange();
				Selection.Ids = new List<int>();
				FinishSelectionChange();
			}
		}

		float LastClickTime = 0;
		//GameObject LastSelectObject;
		public void SelectObject(GameObject Obj)
		{
			int ObjectId = GetIdOfObject(Obj);

			bool contains = Selection.Ids.Contains(ObjectId);
			
			if( contains&& !IsSelectionRemove() && !IsSelectionAdd())
			{
				if(Time.time < LastClickTime + 0.2f)
					CameraControler.FocusOnObject(Obj);
				LastClickTime = Time.time;

				return;
			}

			//LastSelectObject = Obj;

			LastClickTime = Time.time;


			if (ObjectId >= 0)
			{
				Undo.Current.RegisterSelectionChange();
				if (IsSelectionRemove())
				{
					if (contains)
					{
						Selection.Ids.Remove(ObjectId);
						FinishSelectionChange();
					}
				}
				else if (IsSelectionAdd())
				{
					if (!contains)
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

		public void SelectObjectAdd(GameObject Obj)
		{
			int ObjectId = GetIdOfObject(Obj);
			if (ObjectId >= 0)
			{
				if (!Selection.Ids.Contains(ObjectId))
				{
					Selection.Ids.Add(ObjectId);
					FinishSelectionChange();
				}
			}
		}

		#endregion

		#region Selection Box
		void UpdateSelectionBox(bool On = true)
		{
			if (!AllowSelection)
				On = false;

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
			if (!AllowSelection)
				return;
			//Undo.Current.RegisterSelectionChange();

			Vector3 MouseEndPos = Input.mousePosition;
			Vector3 diference = MouseEndPos - BeginMousePos;
			Rect SelectionBoxArea = new Rect(Mathf.Min(MouseEndPos.x, BeginMousePos.x), Mathf.Min(MouseEndPos.y, BeginMousePos.y), Mathf.Abs(diference.x), Mathf.Abs(diference.y));

			bool AnyChanged = false;

			if (IsSelectionRemove())
			{
				// Add
				for (int i = 0; i < AffectedGameObjects.Length; i++)
				{
					if (AffectedGameObjects[i].activeSelf && SelectionBoxArea.Contains(Cam.WorldToScreenPoint(AffectedGameObjects[i].transform.position)))
					{
						if (Selection.Ids.Contains(i))
						{
							if(!AnyChanged)
								Undo.Current.RegisterSelectionChange();
							AnyChanged = true;
							Selection.Ids.RemoveAt(Selection.Ids.IndexOf(i));
						}
					}
				}
			}
			else {
				if (!IsSelectionAdd() && Selection.Ids.Count > 0)
				{
					if (!AnyChanged)
						Undo.Current.RegisterSelectionChange();
					AnyChanged = true;

					Selection.Ids = new List<int>();

				}

				for (int i = 0; i < AffectedGameObjects.Length; i++)
				{
					if (Selection.Ids.Contains(i))
						continue;

					if (AffectedGameObjects[i].activeSelf && SelectionBoxArea.Contains(Cam.WorldToScreenPoint(AffectedGameObjects[i].transform.position)))
					{
						Selection.Ids.Add(i);
						if (!AnyChanged)
							Undo.Current.RegisterSelectionChange();
						AnyChanged = true;
					}
				}
			}

			if (AnyChanged)
			{
				FinishSelectionChange();
			}


		}
		#endregion


		Plane ControlPlane;

		void CreateControlerPlane(bool up)
		{
			if (up)
			{
				ControlPlane = new Plane(Vector3.up, Controls.position.y);
			}
			else
			{
				ControlPlane = new Plane();
				ControlPlane.SetNormalAndPosition(Vector3.up, Controls.position);
			}
		}

		Vector3 PosOnControler(Ray ray)
		{
			float enter = 0;
			if (ControlPlane.Raycast(ray, out enter))
			{
				return ray.GetPoint(enter);
			}
			return Vector3.zero;
		}

		void Controler_StorePositions()
		{

			Selection.StorePositions();
			for(int i = 0; i < SymetrySelection.Length; i++)
			{
				SymetrySelection[i].StorePositions();
			}
		}


		bool Draged = false;
		void ControlerDrag()
		{
			if (!Draged)
			{
				Undo.Current.RegisterMarkersMove();
				Draged = true;
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 NewPos = PosOnControler(ray);
			Vector3 Offset = NewPos - ControlerClickPoint;

			if (DragType == DragTypes.MoveZ)
				Offset.x = 0;
			if (DragType == DragTypes.MoveX)
				Offset.z = 0;

			if (SnapToGrid)
				Offset = ScmapEditor.SnapToGrid(Offset);

			Controls.position = ControlerBegin + Offset;
			Selection.OffsetPosition(Offset);
			for (int i = 0; i < SymetrySelection.Length; i++)
			{
				SymetrySelection[i].OffsetPosition(Offset);
			}
		}

		void ControlerFinish()
		{
			//TODO Bake Positions
			ResetControlerPosition();
			Draged = false;
		}

	}
}
