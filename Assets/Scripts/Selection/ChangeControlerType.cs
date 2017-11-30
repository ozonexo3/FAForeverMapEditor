using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Selection
{
	public class ChangeControlerType : MonoBehaviour
	{

		public static ChangeControlerType Current;
		public static int ControlerId;

		public GameObject[] Selection;
		public GameObject SnapSelection;
		public Button Position;
		public Button Rotation;
		public Button Scale;
		public Button Snap;

		public bool DefaultSnap = true;

		public void ChangeControler(int id)
		{
			ControlerId = id;
			Selection[0].SetActive(ControlerId == 0);
			Selection[1].SetActive(ControlerId == 1);
			Selection[2].SetActive(ControlerId == 2);
			SelectionManager.Current.UpdateControler();
		}

		public static void ChangeCurrentControler(int id)
		{
			if (Current)
				Current.ChangeControler(id);
		}

		public void ToggleSnap()
		{
			if(SelectionManager.AllowSnapToGrid){
				SelectionManager.Current.SnapToGrid = !SelectionManager.Current.SnapToGrid;
				SnapSelection.SetActive(SelectionManager.Current.SnapToGrid);
				DefaultSnap = SelectionManager.Current.SnapToGrid;

			}
		}

		void OnEnable()
		{
			Current = this;
			UpdateButtons();
			//ChangeControler(ControlerId);
			if (SelectionManager.AllowSnapToGrid)
			{
				SelectionManager.Current.SnapToGrid = DefaultSnap;
			}
			SnapSelection.SetActive(SelectionManager.Current.SnapToGrid);

		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				if(Position.interactable)
				ChangeControler(0);
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				if(Rotation.interactable)
				ChangeControler(1);
			}
			else if (Input.GetKeyDown(KeyCode.R))
			{
				if(Scale.interactable)
				ChangeControler(2);
			}
			else if (Input.GetKeyDown(KeyCode.T))
			{
				if (Snap.interactable)
					ToggleSnap();
			}
		}

		public static void UpdateCurrent()
		{
			if (Current == null)
				return;

			Current.UpdateButtons();
		}

		public void UpdateButtons()
		{
			//Rotation.SetActive(SelectionManager.Current.AllowRotation);
			//Scale.SetActive(SelectionManager.Current.AllowScale);

			Position.interactable = SelectionManager.Current.Active;
			Rotation.interactable = SelectionManager.AllowRotation;
			Scale.interactable = SelectionManager.AllowScale;

			Snap.interactable = SelectionManager.AllowSnapToGrid;

			if (!SelectionManager.AllowSnapToGrid)
			{
				SelectionManager.Current.SnapToGrid = false;
			}

			if (!SelectionManager.Current.Active)
			{
				Selection[0].SetActive(false);
				Selection[1].SetActive(false);
				Selection[2].SetActive(false);
			}
			else if (ControlerId == 1 && !SelectionManager.AllowRotation)
			{
				ChangeControler(0);
			}
			else if (ControlerId == 2 && !SelectionManager.AllowScale)
			{
				ChangeControler(0);
			}
			else
			{
				ChangeControler(ControlerId);
			}

		}
	}
}
