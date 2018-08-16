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
		Quaternion ControlerBeginRot = Quaternion.identity;
		Vector3 ControlerClickPoint = Vector3.zero;
		bool DragStarted = false;

		enum DragTypes
		{
			None, SelectionBox, MoveX, MoveZ, MoveXZ, RotateX, RotateY, ScaleX, ScaleZ, ScaleXYZ
		}


		#region Controler
		public void UpdateControler()
		{
			Controls_Position.SetActive(AllowMove && ChangeControlerType.ControlerId == 0);
			Controls_Up.SetActive(AllowUp && ChangeControlerType.ControlerId == 0);
			Controls_Rotate.SetActive(AllowRotation && ChangeControlerType.ControlerId == 1);
			Controls_RotateX.SetActive(AllowRotationX && ChangeControlerType.ControlerId == 1);
			Controls_Scale.SetActive(AllowScale && ChangeControlerType.ControlerId == 2);

			//FinishSelectionChange();
			ResetControlerPosition();
		}

		#endregion


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

				Ray ray = CameraControler.Current.Cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 1000, ControlerLayers))
				{
					CreateControlerPlane(false);
					Controler_StorePositions();

					ControlerClickPoint = PosOnControler(ray);
					ControlerBegin = Controls.localPosition;
					ControlerBeginRot = Controls.localRotation;

					switch (hit.collider.gameObject.name)
					{
						case "X":
							DragType = DragTypes.MoveX;
							break;
						case "Z":
							DragType = DragTypes.MoveZ;
							break;
						case "XZ":
							DragType = DragTypes.MoveXZ;
							break;
						case "RY":
							DragType = DragTypes.RotateY;
							break;
						case "RX":
							DragType = DragTypes.RotateX;
							break;
						case "SX":
							DragType = DragTypes.ScaleX;
							break;
						case "SZ":
							DragType = DragTypes.ScaleZ;
							break;
						case "SXZ":
							DragType = DragTypes.ScaleXYZ;
							break;
						default:
							DragType = DragTypes.None;
							break;
					}

					if (DragType == DragTypes.RotateY || DragType == DragTypes.RotateX)
						Controler_StoreRotations();
					else if (DragType == DragTypes.ScaleX || DragType == DragTypes.ScaleZ || DragType == DragTypes.ScaleXYZ)
						Controler_StoreScales();

				}
				else
				{
					DragType = DragTypes.SelectionBox;
				}
			}
			else
				DragType = DragTypes.None;
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
			else if (DragType == DragTypes.RotateY)
			{
				ControlerDragRotateY();
			}
			else if (DragType == DragTypes.ScaleX || DragType == DragTypes.ScaleZ || DragType == DragTypes.ScaleXYZ)
			{
				ControlerDragScale();
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
			else if (DragType == DragTypes.MoveX || DragType == DragTypes.MoveZ || DragType == DragTypes.MoveXZ || DragType == DragTypes.RotateY)
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
			Ray ray = CameraControler.Current.Cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 1000, SelectionLayers))
			{
				SelectObject(hit.collider.gameObject);
			}
			else if (Selection.Ids.Count > 0)
			{
				Undo.Current.RegisterSelectionChange();
				Selection.Ids = new List<int>();
				FinishSelectionChange();
			}
		}

		float LastClickTime = 0;
		GameObject LastSelectObject;
		public void SelectObject(GameObject Obj)
		{
			int ObjectId = GetIdOfObject(Obj);

			bool contains = Selection.Ids.Contains(ObjectId);

			if (contains && LastSelectObject == Obj)
			{
				if (Time.time < LastClickTime + 0.2f)
				{
					// Select all of same type
					SelectAllOfType(AffectedTypes[ObjectId]);
					return;
				}
			}
			else if (contains && !IsSelectionRemove() && !IsSelectionAdd())
			{
				if (Time.time < LastClickTime + 0.2f)
					CameraControler.FocusOnObject(Obj);
				LastClickTime = Time.time;

				return;
			}

			LastSelectObject = Obj;
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

		public void SelectAllOfType(int SearchType)
		{
			List<GameObject> ToSelect = new List<GameObject>();
			for(int i = 0; i < AffectedGameObjects.Length; i++)
			{
				if(AffectedGameObjects[i] && AffectedTypes[i] == SearchType)
				{
					ToSelect.Add(AffectedGameObjects[i]);
				}
			}

			if (IsSelectionAdd())
			{
				SelectObjects(ToSelect.ToArray());
			}
			else if (IsSelectionRemove())
			{
				SelectObjectsRemove(ToSelect.ToArray());
			}
			else
			{
				CleanSelection();
				SelectObjects(ToSelect.ToArray());
			}
		}

		public void SelectObjects(GameObject[] ToSelect)
		{
			bool AnyChanged = false;
			for(int i = 0; i < ToSelect.Length; i++)
			{
				int ObjectId = GetIdOfObject(ToSelect[i]);
				if (!Selection.Ids.Contains(ObjectId))
				{
					Selection.Ids.Add(ObjectId);
					AnyChanged = true;
				}
			}

			if(AnyChanged)
				FinishSelectionChange();
		}



		public void SelectObjectsRemove(GameObject[] ToSelect)
		{
			bool AnyChanged = false;
			for (int i = 0; i < ToSelect.Length; i++)
			{
				int ObjectId = GetIdOfObject(ToSelect[i]);
				if (Selection.Ids.Contains(ObjectId))
				{
					Selection.Ids.Remove(ObjectId);
					AnyChanged = true;
				}
			}

			if (AnyChanged)
				FinishSelectionChange();
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


		#region Get Selection
		public GameObject[] GetAllSelectedObjects(bool symmetry = true)
		{
			if (AffectedGameObjects.Length == 0)
				return new GameObject[0];

			HashSet<GameObject> SelectedObjs = new HashSet<GameObject>();


			for(int i = 0; i < Selection.Ids.Count; i++)
			{
				SelectedObjs.Add(AffectedGameObjects[Selection.Ids[i]]);
			}

			for(int s = 0; s < SymetrySelection.Length; s++)
			{
				for(int i = 0; i < SymetrySelection[s].Ids.Count; i++)
				{
					SelectedObjs.Add(AffectedGameObjects[SymetrySelection[s].Ids[i]]);
				}
			}

			GameObject[] ToReturn = new GameObject[SelectedObjs.Count];
			SelectedObjs.CopyTo(ToReturn);
			return ToReturn;
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

		void Controler_StoreRotations()
		{
			Selection.StoreRotations();
			for (int i = 0; i < SymetrySelection.Length; i++)
			{
				SymetrySelection[i].StoreRotations();
			}
		}

		void Controler_StoreScales()
		{
			Selection.StoreScales();
			for (int i = 0; i < SymetrySelection.Length; i++)
			{
				SymetrySelection[i].StoreScales();
			}
		}

		void UndoRegisterMove()
		{
			//Debug.Log(LastControlType);
			if(LastControlType == SelectionControlTypes.Marker || LastControlType == SelectionControlTypes.MarkerChain)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryMarkersMove(), new UndoHistory.HistoryMarkersMove.MarkersMoveHistoryParameter(true));
			}
			else if(LastControlType == SelectionControlTypes.Decal)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsMove(), new UndoHistory.HistoryDecalsMove.DecalsMoveHistoryParameter(true));
			}
			else if(LastControlType == SelectionControlTypes.Units)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryUnitsMove());
			}
			else if (LastControlType == SelectionControlTypes.Props)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryPropsMove(), new UndoHistory.HistoryPropsMove.PropsMoveHistoryParameter(true));
			}
		}

		bool Draged = false;
		void ControlerDrag()
		{
			if (!Draged)
			{
				UndoRegisterMove();
				Draged = true;
			}

			Ray ray = CameraControler.Current.Cam.ScreenPointToRay(Input.mousePosition);
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
			RenderMarkersWarnings.Generate();
			ResetControlerPosition();
			Draged = false;
		}


		int MinAngle = 0;
		void ControlerDragRotateY()
		{
			if (!Draged)
			{
				UndoRegisterMove();
				Draged = true;
			}

			Ray ray = CameraControler.Current.Cam.ScreenPointToRay(Input.mousePosition);
			Vector3 NewPos = PosOnControler(ray);
			//Vector3 Offset = NewPos - ControlerClickPoint;

			float Angle = MassMath.AngleSigned(ControlerClickPoint - Controls.localPosition, NewPos - Controls.localPosition, Vector3.up);
			if (MinAngle > 0)
			{
				Angle += 180;
				Angle = (int)(Angle / MinAngle) * MinAngle;
				Angle -= 180;
			}


			Quaternion Rot = Quaternion.Euler(Vector3.up * Angle);
			Quaternion RotInverse = Quaternion.Euler(Vector3.down * Angle);

			Controls.localRotation = Rot * ControlerBeginRot;


			Selection.OffsetRotation(Controls.position, Rot);
			for (int i = 0; i < SymetrySelection.Length; i++)
			{
				if (SymetrySelection[i].InverseRotation)
					SymetrySelection[i].OffsetRotation(Controls.position, RotInverse);
				else
					SymetrySelection[i].OffsetRotation(Controls.position, Rot);
			}
		}

		void ControlerDragScale()
		{
			if (!Draged)
			{
				UndoRegisterMove();
				Draged = true;
			}


			Ray ray = CameraControler.Current.Cam.ScreenPointToRay(Input.mousePosition);
			Vector3 NewPos = PosOnControler(ray);

			Vector3 ScaledPos = Controls.InverseTransformVector(NewPos - ControlerClickPoint) * 10;

			//Debug.Log("DragScale " + ScaledPos);
			//ScaledPos = Vector3.one + Vector3.one * Mathf.Clamp(Mathf.Max(ScaledPos.x, ScaledPos.z), -0.9f, 100);

			Vector3 Scale = Vector3.one;

			if (DragType == DragTypes.ScaleXYZ || !AllowCustomScale || Selection.Ids.Count > 1)
			{
				Scale += Vector3.one * Mathf.Clamp((ScaledPos.x + ScaledPos.z) / 2f, -0.9f, 100);
			}
			else if (DragType == DragTypes.ScaleX)
			{
				//ScaledPos.y = 1;
				Scale.x += Mathf.Clamp(ScaledPos.x, -0.9f, 100);
				//ScaledPos.z = 1;
			}
			else if (DragType == DragTypes.ScaleZ)
			{
				//ScaledPos.y = 1;
				Scale.z += Mathf.Clamp(ScaledPos.z, -0.9f, 100);
				//ScaledPos.x = 1;
			}

			Selection.OffsetScale(Controls.position, Scale);
			for (int i = 0; i < SymetrySelection.Length; i++)
			{
				SymetrySelection[i].OffsetScale(Controls.position, Scale);
			}
		}

	}
}
