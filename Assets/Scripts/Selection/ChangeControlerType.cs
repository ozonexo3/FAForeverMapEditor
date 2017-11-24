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

		public void ChangeControler(int id)
		{
			ControlerId = id;
			Selection[0].SetActive(ControlerId == 0);
			Selection[1].SetActive(ControlerId == 1);
			Selection[2].SetActive(ControlerId == 2);
			SelectionManager.Current.UpdateControler();
		}

		public void ToggleSnap()
		{
			if(!SelectionManager.Current.AllowSnapToGrid){
				SelectionManager.Current.SnapToGrid = !SelectionManager.Current.SnapToGrid;
				SnapSelection.SetActive(SelectionManager.Current.SnapToGrid);
			}
		}

		void OnEnable()
		{
			Current = this;
			UpdateButtons();
			//ChangeControler(ControlerId);
			SnapSelection.SetActive(SelectionManager.Current.SnapToGrid);
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
			Rotation.interactable = SelectionManager.Current.AllowRotation;
			Scale.interactable = SelectionManager.Current.AllowScale;

			Snap.interactable = SelectionManager.Current.AllowSnapToGrid;

			if (!SelectionManager.Current.AllowSnapToGrid)
			{
				SelectionManager.Current.SnapToGrid = false;
			}

			if (!SelectionManager.Current.Active)
			{
				Selection[0].SetActive(false);
				Selection[1].SetActive(false);
				Selection[2].SetActive(false);
			}
			else if (ControlerId == 1 && !SelectionManager.Current.AllowRotation)
			{
				ChangeControler(0);
			}
			else if (ControlerId == 2 && !SelectionManager.Current.AllowScale)
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
