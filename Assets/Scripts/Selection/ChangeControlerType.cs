using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Selection
{
	public class ChangeControlerType : MonoBehaviour
	{

		public static ChangeControlerType Current;
		static int ControlerId;

		public GameObject[] Selection;
		public GameObject SnapSelection;

		public Button Rotation;
		public Button Scale;

		public void ChangeControler(int id)
		{
			ControlerId = id;
			Selection[0].SetActive(ControlerId == 0);
			Selection[1].SetActive(ControlerId == 1);
			Selection[2].SetActive(ControlerId == 2);
		}

		public void ToggleSnap()
		{
			SelectionManager.Current.SnapToGrid = !SelectionManager.Current.SnapToGrid;
			SnapSelection.SetActive(SelectionManager.Current.SnapToGrid);
		}

		void OnEnable()
		{
			Current = this;
			UpdateButtons();
			ChangeControler(ControlerId);
			SnapSelection.SetActive(SelectionManager.Current.SnapToGrid);
		}

		void UpdateButtons()
		{
			//Rotation.SetActive(SelectionManager.Current.AllowRotation);
			//Scale.SetActive(SelectionManager.Current.AllowScale);

			Rotation.interactable = SelectionManager.Current.AllowRotation;
			Scale.interactable = SelectionManager.Current.AllowScale;

			if (ControlerId == 1 && !SelectionManager.Current.AllowRotation)
			{
				ChangeControler(0);
			}
			else if (ControlerId == 2 && !SelectionManager.Current.AllowScale)
			{
				ChangeControler(0);
			}

		}
	}
}
