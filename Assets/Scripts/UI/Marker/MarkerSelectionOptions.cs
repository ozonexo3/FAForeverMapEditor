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

		public Text MarkerTypeField;
		public InputField NameField;
		public InputField Camera_Zoom;
		public Toggle Camera_Set;
		public Toggle Camera_Sync;
		public InputField SizeField;
		public InputField AmountField;

		[Header("Config")]
		public Color CheckmarkNormal;
		public Color CheckmarkDifferent;

		void Awake()
		{
			Current = this;
		}

		void OnEnable()
		{
			Current.UpdateSelectionOptions();
		}

		void OnDisable()
		{
			Markers.MarkerConnected.Clear();
		}

		public static void UpdateOptions()
		{
			if (Current == null)
				return;
			Current.UpdateSelectionOptions();
		}

		bool Loading = false;
		List<GameObject> SelectedGameObjects;
		int Count = 0;
		bool AllowConnect = true;
		bool AllowCamera = true;
		bool AllowSize = true;
		bool AllowAmount = true;
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

			Count = SelectedGameObjects.Count;
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
				ReadNameMarker();


			AllowConnect = true;
			AllowCamera = true;
			AllowSize = true;
			AllowAmount = true;


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

			ReadTypeMarker();

			if (AllowCamera)
				ReadCameraMarker();
			if (AllowSize)
				ReadSizeMarker();
			if (AllowAmount)
				ReadAmountMarker();

			if (AllowConnect)
				ReadAdiacent();
			else
				Markers.MarkerConnected.Clear();

			Loading = false;
		}


		#region Read Selected Markers
		const string ValueDifferent = " - ";

		void ReadNameMarker()
		{
			NameField.text = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.Name;
		}

		void ReadTypeMarker()
		{
			SaveLua.Marker.MarkerTypes Mt = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.MarkerType;

			for (int i = 1; i < Count; i++)
			{
				if(Mt != SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.MarkerType)
				{
					MarkerTypeField.text = ValueDifferent + ((Count > 1) ?( " ( " + Count + " )"):(""));
					return;
				}
			}

			MarkerTypeField.text = SaveLua.Marker.MarkerTypeToString(Mt) + ((Count > 1) ? (" ( " + Count + " )") : (""));
		}

		void ReadCameraMarker()
		{
			float Zoom = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.zoom;
			bool CanSetCamera = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.canSetCamera;
			bool CanSyncCamera = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.canSyncCamera;

			bool ZoomDif = false;
			bool SetDif = false;
			bool SyncDif = false;
			if (Count > 1)
			{
				for(int i = 1; i < Count; i++)
				{
					if (!ZoomDif && Zoom != SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.zoom)
						ZoomDif = true;

					if (!SetDif && CanSetCamera != SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.canSetCamera)
						SetDif = true;

					if (!SyncDif && CanSyncCamera != SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.canSyncCamera)
						SyncDif = true;

					if (ZoomDif && SetDif && SyncDif)
						break;
				}
			}


			if (ZoomDif)
			{
				Camera_Zoom.contentType = InputField.ContentType.Standard;
				Camera_Zoom.text = ValueDifferent;
			}
			else
			{
				Camera_Zoom.contentType = InputField.ContentType.DecimalNumber;
				Camera_Zoom.text = Zoom.ToString();
			}

			if (SetDif)
			{
				Camera_Set.graphic.color = CheckmarkDifferent;
				Camera_Set.isOn = true;
			}
			else
			{
				Camera_Set.graphic.color = CheckmarkNormal;
				Camera_Set.isOn = CanSetCamera;
			}

			if (SyncDif)
			{
				Camera_Sync.graphic.color = CheckmarkDifferent;
				Camera_Sync.isOn = true;
			}
			else
			{
				Camera_Sync.graphic.color = CheckmarkNormal;
				Camera_Sync.isOn = CanSetCamera;
			}
		}

		void ReadSizeMarker()
		{
			float Size = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.size;
			bool SizeDif = false;
			if (Count > 1)
			{
				for (int i = 1; i < Count; i++)
				{
					if (Size != SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.size)
					{
						SizeDif = true;
						break;
					}
				}
			}

			if (SizeDif)
			{
				SizeField.contentType = InputField.ContentType.Standard;
				SizeField.text = ValueDifferent;
			}
			else
			{
				SizeField.contentType = InputField.ContentType.DecimalNumber;
				SizeField.text = Size.ToString();
			}
		}

		void ReadAmountMarker()
		{
			float Amount = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.amount;
			bool AmountDif = false;
			if (Count > 1)
			{
				for (int i = 1; i < Count; i++)
				{
					if (Amount != SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.amount)
					{
						AmountDif = true;
						break;
					}
				}
			}

			if (AmountDif)
			{
				AmountField.contentType = InputField.ContentType.Standard;
				AmountField.text = ValueDifferent;
			}
			else
			{
				AmountField.contentType = InputField.ContentType.DecimalNumber;
				AmountField.text = Amount.ToString();
			}
		}


		void ReadAdiacent()
		{
			string AdjacentString = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.adjacentTo;
			string[] Names = AdjacentString.Split(" ".ToCharArray());
			//Debug.Log(AdjacentString + " = " + Names.Length);

			Markers.MarkerConnected.RenderConnection(Names, SelectedGameObjects[0].name);

		}
		#endregion

		#region Set UI
		public void NameChanged()
		{
			if (Loading)
				return;
			Loading = true;
			string NameToChange = NameField.text;

			if (AllowConnect)
				NameToChange.Replace(" ", "");

			for (int i = 0; i < SelectionManager.Current.AffectedGameObjects.Length; i++)
			{
				if (SelectionManager.Current.AffectedGameObjects[i].name == NameToChange)
				{
					Loading = true;
					NameField.text = SelectedGameObjects[0].name;
					Loading = false;
					return;
				}
			}

			SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.Name = NameToChange;
			SelectedGameObjects[0].name = NameToChange;
			NameField.text = NameToChange;

			Loading = false;
		}

		public void CameraZoomChanged()
		{
			if (Loading)
				return;
			Loading = true;
			float ReadValue = LuaParser.Read.StringToFloat(Camera_Zoom.text, -1);

			if(ReadValue >= 0)
			{
				for (int i = 0; i < Count; i++)
				{
					SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.zoom = ReadValue;
				}
				Camera_Zoom.text = ReadValue.ToString();
			}
			else
			{
				if(Camera_Zoom.contentType == InputField.ContentType.DecimalNumber)
				{
					Camera_Zoom.text = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.zoom.ToString();
				}
				else
				{
					Camera_Zoom.text = ValueDifferent;
				}
			}
			Loading = false;
		}

		public void ToggleCameraSet()
		{
			if (Loading)
				return;
			Camera_Set.graphic.color = CheckmarkNormal;

			for (int i = 0; i < Count; i++)
			{
				SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.canSetCamera = Camera_Set.isOn;
			}
		}

		public void ToggleCameraSync()
		{
			if (Loading)
				return;
			Camera_Sync.graphic.color = CheckmarkNormal;

			for (int i = 0; i < Count; i++)
			{
				SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.canSyncCamera = Camera_Sync.isOn;
			}
		}

		public void FocusCameraMarker()
		{
			CameraControler.FocusCamera(SelectedGameObjects[0].transform, SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.zoom * 0.1f, -15);

		}


		public void SizeChanged()
		{
			if (Loading)
				return;
			Loading = true;
			float ReadValue = LuaParser.Read.StringToFloat(SizeField.text, -1);

			if (ReadValue >= 0)
			{
				for (int i = 0; i < Count; i++)
				{
					SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.size = ReadValue;
				}
				SizeField.text = ReadValue.ToString();
			}
			else
			{
				if (SizeField.contentType == InputField.ContentType.DecimalNumber)
				{
					SizeField.text = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.size.ToString();
				}
				else
				{
					SizeField.text = ValueDifferent;
				}
			}
			Loading = false;
		}

		public void AmountChanged()
		{
			if (Loading)
				return;
			Loading = true;
			float ReadValue = LuaParser.Read.StringToFloat(AmountField.text, -1);

			if (ReadValue >= 0)
			{
				for (int i = 0; i < Count; i++)
				{
					SelectedGameObjects[i].GetComponent<MarkerObject>().Owner.amount = ReadValue;
				}
				AmountField.text = ReadValue.ToString();
			}
			else
			{
				if (AmountField.contentType == InputField.ContentType.DecimalNumber)
				{
					AmountField.text = SelectedGameObjects[0].GetComponent<MarkerObject>().Owner.amount.ToString();
				}
				else
				{
					AmountField.text = ValueDifferent;
				}
			}
			Loading = false;
		}


		#endregion
	}
}
