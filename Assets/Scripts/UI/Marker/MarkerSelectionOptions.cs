using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selection;

namespace EditMap
{
	public class MarkerSelectionOptions : MonoBehaviour
	{

		public static MarkerSelectionOptions Current;

		public GameObject NameInput;
		public GameObject Connect;
		public GameObject CameraZoom;
		public GameObject SetCamera;
		public GameObject SyncCamera;
		public GameObject Size;
		public GameObject Amount;

		void Awake()
		{
			Current = this;
		}


		public static void UpdateOptions()
		{
			if (Current == null)
				return;
			Current.UpdateSelectionOptions();
		}


		void UpdateSelectionOptions()
		{
			List<GameObject> SelectedGameObjects = new List<GameObject>();

			for(int i = 0; i < SelectionManager.Current.Selection.Ids.Count; i++)
			{
				SelectedGameObjects.Add(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]]);
			}

			for (int s = 0; s < SelectionManager.Current.SymetrySelection.Length; s++)
			{
				for (int i = 0; i < SelectionManager.Current.Selection.Ids.Count; i++)
				{
					SelectedGameObjects.Add(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]]);
				}
			}




		}
	}
}
