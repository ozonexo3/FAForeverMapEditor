using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Selection;
using MapLua;
using Markers;

namespace EditMap
{
	public class MarkerSelectionOptions : MonoBehaviour
	{

		public static MarkerSelectionOptions Current;

		public GameObject None;
		public GameObject Mix;

		public GameObject NameInput;
		public GameObject Connect;
		public GameObject CameraZoom;
		public GameObject SetCamera;
		public GameObject SyncCamera;
		public GameObject ViewCamera;
		public GameObject Size;
		public GameObject Amount;

		public InputField NameField;

		void Awake()
		{
			Current = this;
		}

		void OnEnable()
		{
			Current.UpdateSelectionOptions();
		}

		public static void UpdateOptions()
		{
			if (Current == null)
				return;
			Current.UpdateSelectionOptions();
		}

		bool Loading = false;
		List<GameObject> SelectedGameObjects;
		void UpdateSelectionOptions()
		{
			Loading = true;
			SelectedGameObjects = new List<GameObject>();

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

			int Count = SelectedGameObjects.Count;
			if (Count == 0)
			{
				None.SetActive(true);
				Mix.SetActive(false);
				return;
			}


			None.SetActive(false);
			Mix.SetActive(true);

			bool AllowName = SelectionManager.Current.Selection.Ids.Count == 1;
			if (AllowName)
			{
				NameField.text = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.Name;
			}


			bool AllowConnect = true;
			bool AllowCamera = true;
			bool AllowSize = true;
			bool AllowAmount = true;


			bool[] CheckedTypes = new bool[(int)SaveLua.Marker.MarkerTypes.Count];

			SaveLua.Marker Mr;

			for (int i = 0; i < Count; i++)// Search for allowed 
			{
				Mr = SelectedGameObjects[i].GetComponent<MarkerObject>().Owner;

				if (!CheckedTypes[(int)Mr.MarkerType])
				{
					CheckedTypes[(int)Mr.MarkerType] = true;

					if (AllowConnect)
						AllowConnect = Mr.AllowByType(SaveLua.Marker.KEY_ADJACENTTO);

					if (AllowCamera)
						AllowCamera = Mr.AllowByType(SaveLua.Marker.KEY_CANSETCAMERA);

					if (AllowSize)
						AllowSize = Mr.AllowByType(SaveLua.Marker.KEY_SIZE);

					if (AllowAmount)
						AllowAmount = Mr.AllowByType(SaveLua.Marker.KEY_AMOUNT);

					if (!AllowConnect && !AllowCamera && !AllowSize && !AllowAmount)
						break;
				}
			}

			Mr = null;

			NameInput.SetActive(AllowName);

			Connect.SetActive(AllowConnect);

			CameraZoom.SetActive(AllowCamera);
			SetCamera.SetActive(AllowCamera);
			SyncCamera.SetActive(AllowCamera);
			ViewCamera.SetActive(AllowCamera);

			Size.SetActive(AllowSize);

			Amount.SetActive(AllowAmount);
			Loading = false;
		}

		public void NameChanged()
		{
			if (Loading)
				return;
			for(int i = 0; i < SelectionManager.Current.AffectedGameObjects.Length; i++)
			{
				if (SelectionManager.Current.AffectedGameObjects[i].name == NameField.text)
				{
					Loading = true;
					NameField.text = SelectedGameObjects[0].name;
					Loading = false;
					return;
				}
			}

			SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.Name = NameField.text;
			SelectedGameObjects[0].name = NameField.text;
		}
	}
}
