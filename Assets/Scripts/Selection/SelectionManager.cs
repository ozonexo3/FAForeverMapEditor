using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		public static SelectionManager Current;

		[Header("Core")]
		public Camera Cam;
		public RectTransform SelBox;
		public int DisableLayer;
		public int UsedLayer;
		public bool SnapToGrid;
		public bool SnapToWater = true;

		public GameObject[] AffectedGameObjects;
		public bool Active = false;


		void Awake()
		{
			Current = this;
		}

		void Update()
		{
			if (!Active)
				return;

			if (AllowRemove && Input.GetKeyDown(KeyCode.Delete))
			{
				DestroySelectedObjects();
			}

		}


		public enum SelectionControlTypes
		{
			None, Last, Marker, MarkerChain, Decal, Units
		}

		SelectionControlTypes LastControlType;
		void SetSelectionType(SelectionControlTypes SelectionControlType)
		{
			if (LastControlType == SelectionControlType)
				return;

			FinishSelectionChange();

			switch (SelectionControlType)
			{
				case SelectionControlTypes.Marker:
					AllowMove = true;
					AllowUp = false;
					AllowRotation = false;
					AllowRotationX = false;
					AllowScale = false;
					AllowSnapToGrid = true;
					AllowSelection = true;
					AllowSymmetry = false;
					AllowRemove = false;
					SelPrefab = 0;
					break;
				case SelectionControlTypes.MarkerChain:
					AllowMove = false;
					AllowUp = false;
					AllowRotation = false;
					AllowRotationX = false;
					AllowScale = false;
					AllowSnapToGrid = true;
					AllowSelection = true;
					AllowSymmetry = false;
					AllowRemove = false;
					SelPrefab = 0;
					break;
				case SelectionControlTypes.Decal:
					AllowMove = true;
					AllowUp = false;
					AllowRotation = true;
					AllowRotationX = true;
					AllowScale = true;
					AllowSnapToGrid = false;
					AllowSelection = true;
					AllowSymmetry = false;
					AllowRemove = false;
					SelPrefab = 1;
					break;
				default:
					AllowMove = false;
					AllowUp = false;
					AllowRotation = false;
					AllowRotationX = false;
					AllowScale = false;
					AllowSnapToGrid = true;
					AllowSelection = false;
					AllowSymmetry = false;
					AllowRemove = false;
					SelPrefab = 0;
					break;
			}
			LastControlType = SelectionControlType;

		}

		public bool AllowSelection = true;
		public bool AllowSymmetry = true;
		public bool AllowRemove = true;
		public bool AllowSnapToGrid = true;
		public bool AllowMove;
		public bool AllowUp;
		public bool AllowRotation;
		public bool AllowRotationX;
		public bool AllowScale;
		int SelPrefab = 0;


		public void ClearAffectedGameObjects()
		{
			Undo.Current.RegisterSelectionRangeChange();
			SetAffectedGameObjects(new GameObject[0], SelectionControlTypes.None);
		}


		/*
		public void SetCustomSettings(bool _Selection = true, bool _Symmetry = true, bool _Remove = true)
		{
			if (AllowSymmetry != _Selection)
				FinishSelectionChange();
			else if(AllowSymmetry != _Symmetry)
				FinishSelectionChange();
			else if (AllowRemove != _Remove)
				FinishSelectionChange();

			AllowSelection = _Selection;
			AllowSymmetry = _Symmetry;
			AllowRemove = _Remove;
		}
		*/

		public void SetAffectedGameObjects(GameObject[] GameObjects, SelectionControlTypes SelectionControlType)
		{
			if(SelectionControlType != SelectionControlTypes.Last)
				SetSelectionType(SelectionControlType);

			ChangeControlerType.UpdateCurrent();

			if (AffectedGameObjects.Length > 0)
			{
				// Clean
				for(int i = 0; i < AffectedGameObjects.Length; i++)
				{
					if(AffectedGameObjects[i])
						AffectedGameObjects[i].layer = DisableLayer;
				}
			}

			AffectedGameObjects = GameObjects;

			if (AffectedGameObjects.Length > 0)
			{
				// activate settings
				for (int i = 0; i < AffectedGameObjects.Length; i++)
				{
					if (AffectedGameObjects[i])
						AffectedGameObjects[i].layer = UsedLayer;
				}
			}


			Active = AffectedGameObjects.Length > 0;

			CleanIfInactive();
		}




		public void CleanSelection()
		{
			Selection.Ids = new List<int>();
			FinishSelectionChange();
		}

		void CleanIfInactive()
		{
			if (!Active)
			{
				AffectedGameObjects = new GameObject[0];
				CleanSelection();
			}
		}


		#region Action Events
		private System.Action<List<GameObject>, bool> RemoveAction;
		public void SetRemoveAction(System.Action<List<GameObject>, bool> Action)
		{
			RemoveAction = Action;
		}


		void DestroySelectedObjects()
		{
			int Count = Selection.Ids.Count;
			if (Count > 0)
			{
				List<GameObject> SelectedObjectsList = new List<GameObject>();
				for(int i = 0; i < Count; i++)
				{
					SelectedObjectsList.Add(AffectedGameObjects[Selection.Ids[i]]);
				}


				for(int s = 0; s < SymetrySelection.Length; s++)
				{
					Count = SymetrySelection[s].Ids.Count;

					for (int i = 0; i < Count; i++)
					{
						SelectedObjectsList.Add(AffectedGameObjects[SymetrySelection[s].Ids[i]]);
					}
				}

				RemoveAction(SelectedObjectsList, true);

				Active = AffectedGameObjects.Length > 0;

				CleanIfInactive();
			}
		}

		private System.Action SelectionChangeAction;
		public void SetSelectionChangeAction(System.Action Action)
		{
			SelectionChangeAction = Action;
		}

		#endregion
	}
}
