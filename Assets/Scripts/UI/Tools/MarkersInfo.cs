using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EditMap
{
	public class MarkersInfo : MonoBehaviour
	{

		public GameObject SettingMarker;
		public GameObject SettingAI;
		public GameObject SettingNonArmy;
		public GameObject SettingArmy;
		public GameObject SettingWarning;

		#region Classes
		[Header("Objects")]
		public MapLuaParser Scenario;
		public CameraControler KameraKontroler;
		public Editing EditMenu;
		public List<WorkingElement> AllWorkingElements;


		[Header("Creating")]
		public int CreatingId;
		public Image[] ButtonSelected;
		public GameObject[] CreatingPrefabs;

		[Header("Selection")]
		public List<WorkingElement> Selected = new List<WorkingElement>();
		public Vector3[] SelectedStartPos;
		public Transform SelectedMarker;
		public Vector3 SelectedMarkerBeginClickPos;
		public Vector3 SelectedMarkerBeginPos;
		public Texture[] SelectionSizeTextures;
		public MarkersList AllMarkersList;


		[Header("Symmetry Selection")]
		public GameObject SelectionSymmetryPrefab;
		public List<WorkingElement> MirrorSelected = new List<WorkingElement>();
		public SymmetrySelection[] SymmetrySelectionList = new SymmetrySelection[0];
		public List<Transform> SelectedSymmetryMarkers = new List<Transform>();

		[Header("Selection Markers")]
		public List<GameObject> SelectionsRings = new List<GameObject>();
		public GameObject RingSelectionPrefab;



		[System.Serializable]
		public class SymmetrySelection
		{
			public Vector3 MoveMultiply = Vector3.one;
			public float MoveRotation = 0;
			public bool Diagonal;
			public List<WorkingElement> MirrorSelected = new List<WorkingElement>();
			public Vector3[] SelectedStartPos;
			public Vector3 SelectedMarkerBeginClickPos;
		}

		[System.Serializable]
		public class WorkingElement
		{
			public int InstanceId;
			public int ListId;
			public int SelectionState;
		}
		#endregion


		void OnDisable()
		{
			RemoveCreating();
		}

		#region MarkerSettings
		bool IgnoreSettingsInput = false;
		public void OnInputFinished()
		{
			if (IgnoreSettingsInput)
				return;



		}

		public void SettingsForSelected()
		{

		}

		#endregion

		#region Creating
		public void ButtonFunction(string func)
		{
			switch (func)
			{
				case "Army":
					if (CreatingId == 1) CreatingId = 0;
					else CreatingId = 1;
					UpdateCreating();
					break;
				case "Mass":
					if (CreatingId == 2) CreatingId = 0;
					else CreatingId = 2;
					UpdateCreating();
					break;
				case "Hydro":
					if (CreatingId == 3) CreatingId = 0;
					else CreatingId = 3;
					UpdateCreating();
					break;
				case "AI":
					if (CreatingId == 4) CreatingId = 0;
					else CreatingId = 4;
					UpdateCreating();
					break;
			}
		}

		public void RemoveCreating()
		{
			if (CreatingId > 0)
			{
				CreatingId = 0;
				UpdateCreating();
			}
		}

		public void UpdateCreating()
		{
			if (KameraKontroler.MarkerToCreate)
			{
				Destroy(KameraKontroler.MarkerToCreate.gameObject);
				foreach (Transform obj in KameraKontroler.MarkerToCreateSymmetry)
				{
					Destroy(obj.gameObject);
				}
			}
			int SymmetryCode = PlayerPrefs.GetInt("Symmetry", 0);
			int SymmetryCount = 0;
			if (SymmetryCode == 0) SymmetryCount = 0;
			else if (SymmetryCode == 7) SymmetryCount = PlayerPrefs.GetInt("SymmetryAngleCount", 2) - 1;
			else if (SymmetryCode == 4) SymmetryCount = 3;
			else SymmetryCount = 1;


			if (CreatingId == 0)
			{
				SymmetryCount = 0;
				ButtonSelected[0].gameObject.SetActive(false);
				ButtonSelected[1].gameObject.SetActive(false);
				ButtonSelected[2].gameObject.SetActive(false);
				ButtonSelected[3].gameObject.SetActive(false);
			}
			else if (CreatingId == 1)
			{
				ButtonSelected[0].gameObject.SetActive(true);
				ButtonSelected[1].gameObject.SetActive(false);
				ButtonSelected[2].gameObject.SetActive(false);
				ButtonSelected[3].gameObject.SetActive(false);
				GameObject ObjectToCreate = Instantiate(CreatingPrefabs[0]);
				KameraKontroler.MarkerToCreate = ObjectToCreate.transform;
			}
			else if (CreatingId == 2)
			{
				ButtonSelected[0].gameObject.SetActive(false);
				ButtonSelected[1].gameObject.SetActive(true);
				ButtonSelected[2].gameObject.SetActive(false);
				ButtonSelected[3].gameObject.SetActive(false);
				GameObject ObjectToCreate = Instantiate(CreatingPrefabs[1]);
				KameraKontroler.MarkerToCreate = ObjectToCreate.transform;
			}
			else if (CreatingId == 3)
			{
				ButtonSelected[0].gameObject.SetActive(false);
				ButtonSelected[1].gameObject.SetActive(false);
				ButtonSelected[2].gameObject.SetActive(true);
				ButtonSelected[3].gameObject.SetActive(false);
				GameObject ObjectToCreate = Instantiate(CreatingPrefabs[2]);
				KameraKontroler.MarkerToCreate = ObjectToCreate.transform;
			}
			else if (CreatingId == 4)
			{
				ButtonSelected[0].gameObject.SetActive(false);
				ButtonSelected[1].gameObject.SetActive(false);
				ButtonSelected[2].gameObject.SetActive(false);
				ButtonSelected[3].gameObject.SetActive(true);
				GameObject ObjectToCreate = Instantiate(CreatingPrefabs[3]);
				KameraKontroler.MarkerToCreate = ObjectToCreate.transform;
			}

			KameraKontroler.MarkerToCreateSymmetry = new Transform[SymmetryCount];
			for (int i = 0; i < SymmetryCount; i++)
			{
				GameObject ObjectToCreate = Instantiate(KameraKontroler.MarkerToCreate.gameObject);
				KameraKontroler.MarkerToCreateSymmetry[i] = ObjectToCreate.transform;
			}
		}
		#endregion

		#region Generate window
		public void GenerateAllWorkingElements()
		{
			AllWorkingElements = new List<WorkingElement>();

			for (int i = 0; i < Scenario.ARMY_.Count; i++)
			{
				if (Scenario.ARMY_[i].Hidden)
					continue;
				WorkingElement NewElement = new WorkingElement();
				NewElement.InstanceId = i;
				NewElement.ListId = 0;
				NewElement.SelectionState = 0;
				AllWorkingElements.Add(NewElement);
			}
			for (int i = 0; i < Scenario.Mexes.Count; i++)
			{
				WorkingElement NewElement = new WorkingElement();
				NewElement.InstanceId = i;
				NewElement.ListId = 1;
				NewElement.SelectionState = 0;
				AllWorkingElements.Add(NewElement);
			}
			for (int i = 0; i < Scenario.Hydros.Count; i++)
			{
				WorkingElement NewElement = new WorkingElement();
				NewElement.InstanceId = i;
				NewElement.ListId = 2;
				NewElement.SelectionState = 0;
				AllWorkingElements.Add(NewElement);
			}
			for (int i = 0; i < Scenario.SiMarkers.Count; i++)
			{
				WorkingElement NewElement = new WorkingElement();
				NewElement.InstanceId = i;
				NewElement.ListId = 3;
				NewElement.SelectionState = 0;
				AllWorkingElements.Add(NewElement);
			}
		}

		public void ClearWorkingElements()
		{
			CleanSelection();
			AllWorkingElements = new List<WorkingElement>();
		}
		#endregion

		#region Selection
		public void AddToSelection(List<GameObject> add)
		{
			if (add.Count > 0)
			{
				Scenario.History.RegisterMarkerSelection();
			}
			for (int i = 0; i < add.Count; i++)
			{
				bool AlreadyExist = false;
				foreach (WorkingElement obj in Selected)
				{
					if (obj.ListId == add[i].GetComponent<MarkerData>().ListId && obj.InstanceId == add[i].GetComponent<MarkerData>().InstanceId)
					{
						AlreadyExist = true;
						break;
					}
				}
				if (!AlreadyExist)
				{
					WorkingElement ToAdd = new WorkingElement();
					ToAdd.InstanceId = add[i].GetComponent<MarkerData>().InstanceId;
					ToAdd.ListId = add[i].GetComponent<MarkerData>().ListId;
					Selected.Add(ToAdd);
				}
			}
			UpdateSelectionRing();
		}

		public void RemoveFromSelection(List<GameObject> remove)
		{
			if (remove.Count > 0) Scenario.History.RegisterMarkerSelection();
			for (int i = 0; i < remove.Count; i++)
			{
				foreach (WorkingElement obj in Selected)
				{
					if (obj.ListId == remove[i].GetComponent<MarkerData>().ListId && obj.InstanceId == remove[i].GetComponent<MarkerData>().InstanceId)
					{
						Selected.Remove(obj);
						break;
					}
				}
			}
			UpdateSelectionRing();
		}

		public void ChangeSelectionState(List<GameObject> change)
		{
			if (change.Count > 0) Scenario.History.RegisterMarkerSelection();
			for (int i = 0; i < change.Count; i++)
			{
				bool AlreadyExist = false;
				foreach (WorkingElement obj in Selected)
				{
					if (obj.ListId == change[i].GetComponent<MarkerData>().ListId && obj.InstanceId == change[i].GetComponent<MarkerData>().InstanceId)
					{
						AlreadyExist = true;
						Selected.Remove(obj);
						break;
					}
				}
				if (!AlreadyExist)
				{
					WorkingElement ToAdd = new WorkingElement();
					ToAdd.InstanceId = change[i].GetComponent<MarkerData>().InstanceId;
					ToAdd.ListId = change[i].GetComponent<MarkerData>().ListId;
					Selected.Add(ToAdd);
				}
			}
			UpdateSelectionRing();
		}

		public void CleanSelection()
		{
			AllMarkersList.UnselectAll();
			if (Selected.Count > 0) Scenario.History.RegisterMarkerSelection();
			Selected = new List<WorkingElement>();
			UpdateSelectionRing();
		}

		public bool IsSelected(int listId, int instanceId)
		{
			foreach (WorkingElement obj in Selected)
			{
				if (obj.InstanceId == instanceId && obj.ListId == listId) return true;
			}
			return false;
		}

		public bool IsSymmetrySelected(int listId, int instanceId)
		{
			for (int i = 0; i < AllWorkingElements.Count; i++)
			{
				if (AllWorkingElements[i].ListId == listId && AllWorkingElements[i].InstanceId == instanceId)
				{
					if (AllWorkingElements[i].SelectionState == 2) return true;
					return false;
				}
			}
			return false;
		}
		#endregion

		#region Selection Rings
		public void UpdateSelectionRing()
		{
			//EditMenu.MirrorTolerance = 0.3f;
			foreach (WorkingElement all in AllWorkingElements)
			{
				all.SelectionState = 0;
			}
			if (Selected.Count <= 0)
			{
				SelectedMarker.gameObject.SetActive(false);
				if (SelectedSymmetryMarkers.Count > 0)
				{
					foreach (Transform marker in SelectedSymmetryMarkers)
					{
						marker.gameObject.SetActive(false);
					}
				}
				foreach (GameObject child in SelectionsRings)
				{
					Destroy(child.gameObject);
				}
				SelectionsRings = new List<GameObject>();
				SelectedStartPos = new Vector3[0];
			}
			else
			{
				SelectedMarker.gameObject.SetActive(true);

				foreach (GameObject child in SelectionsRings)
				{
					Destroy(child.gameObject);
				}
				SelectionsRings = new List<GameObject>();

				float MaxX = Scenario.GetPosOfMarker(Selected[0]).x;
				float MaxY = Scenario.GetPosOfMarker(Selected[0]).z;

				float MinX = Scenario.GetPosOfMarker(Selected[0]).x;
				float MinY = Scenario.GetPosOfMarker(Selected[0]).z;

				float MidHeight = 0;

				foreach (WorkingElement obj in Selected)
				{
					MaxX = Mathf.Max(MaxX, Scenario.GetPosOfMarker(obj).x);
					MaxY = Mathf.Max(MaxY, Scenario.GetPosOfMarker(obj).z);
					MinX = Mathf.Min(MinX, Scenario.GetPosOfMarker(obj).x);
					MinY = Mathf.Min(MinY, Scenario.GetPosOfMarker(obj).z);
					MidHeight += Scenario.GetPosOfMarker(obj).y;
				}
				MidHeight /= Selected.Count;

				SelectedMarker.position = new Vector3((MaxX + MinX) / 2, MidHeight, (MaxY + MinY) / 2);
				SelectedMarker.localScale = Vector3.one * (Mathf.Max(Mathf.Abs(MaxX - MinX), Mathf.Abs(MaxY - MinY)) * 1.1f + 0.4f);

				if (SelectedMarker.localScale.x < 3)
				{
					SelectedMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[0]);
					if (SelectedSymmetryMarkers.Count > 0)
					{
						foreach (Transform marker in SelectedSymmetryMarkers)
						{
							marker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[0]);
						}
					}
					//SelectedReflectionMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[0]);
				}
				else
				{
					SelectedMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[1]);
					if (SelectedSymmetryMarkers.Count > 0)
					{
						foreach (Transform marker in SelectedSymmetryMarkers)
						{
							marker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[1]);
						}
					}
					//SelectedReflectionMarker.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", SelectionSizeTextures[1]);
				}

				SelectedStartPos = new Vector3[Selected.Count];

				for (int i = 0; i < Selected.Count; i++)
				{
					SelectedStartPos[i] = Scenario.GetPosOfMarker(Selected[i]) - SelectedMarker.position;
				}

				int SymmetryCode = PlayerPrefs.GetInt("Symmetry", 0);

				// Generate Symmetry Selection Rings
				if (SymmetryCode == 0)
				{
					RegenerateSymmetryMarkers(0);
				}
				else if (SymmetryCode == 7)
				{
					RegenerateSymmetryMarkers(PlayerPrefs.GetInt("SymmetryAngleCount", 2) - 1);
				}
				else if (SymmetryCode == 4)
				{
					RegenerateSymmetryMarkers(3);
				}
				else
				{
					RegenerateSymmetryMarkers(1);
				}

				// Do Symmetry Selection
				if (SymmetryCode == 0) { }
				else if (SymmetryCode == 1) SelectHorizontal();
				else if (SymmetryCode == 2) SelectVertical();
				else if (SymmetryCode == 3) SelectHorizontalVertical();
				else if (SymmetryCode == 4)
				{
					SelectHorizontal(0);
					SelectVertical(1);
					SelectHorizontalVertical(2);
				}
				else if (SymmetryCode == 5) SelectDiagonal1();
				else if (SymmetryCode == 6) SelectDiagonal2();
				else if (SymmetryCode == 7)
				{
					SelectDiagonal1(0);
					SelectDiagonal2(1);
				}
				else if (SymmetryCode == 8)
				{
					int Count = PlayerPrefs.GetInt("SymmetryAngleCount", 2);
					float angle = 360.0f / (float)Count;
					for (int i = 0; i < Count - 1; i++)
					{
						SelectRotateByCenter(i, angle + angle * i);
					}
				}

				foreach (WorkingElement obj in Selected)
				{
					GameObject newRing = Instantiate(RingSelectionPrefab) as GameObject;
					newRing.GetComponent<SelectionRing>().SelectedObject = Scenario.GetMarkerRenderer(obj).transform;
					SelectionsRings.Add(newRing);
				}
				for (int i = 0; i < SymmetrySelectionList.Length; i++)
				{
					SymmetrySelectionList[i].SelectedStartPos = new Vector3[SymmetrySelectionList[i].MirrorSelected.Count];
					for (int m = 0; m < SymmetrySelectionList[i].MirrorSelected.Count; m++)
					{
						GameObject newRing = Instantiate(RingSelectionPrefab) as GameObject;
						newRing.GetComponent<SelectionRing>().SelectedObject = Scenario.GetMarkerRenderer(SymmetrySelectionList[i].MirrorSelected[m]).transform;
						newRing.GetComponent<SelectionRing>().ForceUpdate();
						SelectionsRings.Add(newRing);
						SymmetrySelectionList[i].SelectedStartPos[m] = newRing.transform.position - SelectedSymmetryMarkers[i].position;
					}
				}
			}
			AllMarkersList.UpdateSelection();
			SettingsForSelected();
		}
		#endregion

		#region Symmetry
		void RegenerateSymmetryMarkers(int count = 0)
		{
			if (SelectedSymmetryMarkers.Count == count)
			{
				for (int i = 0; i < SymmetrySelectionList.Length; i++)
				{
					SymmetrySelectionList[i].MirrorSelected = new List<WorkingElement>();
				}
				return;
			}

			SymmetrySelectionList = new SymmetrySelection[count];
			for (int i = 0; i < count; i++)
			{
				SymmetrySelectionList[i] = new SymmetrySelection();
				SymmetrySelectionList[i].MirrorSelected = new List<WorkingElement>();
			}

			Debug.Log("Regenerate Symmetry Markers");

			foreach (Transform marker in SelectedSymmetryMarkers)
			{
				DestroyImmediate(marker.gameObject);
			}

			SelectedSymmetryMarkers = new List<Transform>();
			for (int i = 0; i < count; i++)
			{
				GameObject newMarker = Instantiate(SelectionSymmetryPrefab) as GameObject;
				newMarker.transform.parent = EditMenu.HudElements;
				SelectedSymmetryMarkers.Add(newMarker.transform);
			}
		}

		void SelectHorizontal(int id = 0)
		{
			if (SelectedSymmetryMarkers.Count < 0) return;
			SelectedSymmetryMarkers[id].gameObject.SetActive(true);
			SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

			Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
			MirroredMarker.x = -MirroredMarker.x;
			MirroredMarker += Scenario.MapCenterPoint;
			MirroredMarker.y = SelectedMarker.position.y;
			SelectedSymmetryMarkers[id].position = MirroredMarker;
			SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, 1);

			foreach (WorkingElement obj in Selected)
			{
				foreach (WorkingElement all in AllWorkingElements)
				{
					Vector3 MirroredPos = Scenario.GetPosOfMarker(all) - Scenario.MapCenterPoint;
					MirroredPos.x = -MirroredPos.x;
					MirroredPos += Scenario.MapCenterPoint;

					//Xdist = Scenario.MapCenterPoint.x - obj.transform.position.x;
					Vector3 SelPos = new Vector3(Scenario.GetPosOfMarker(obj).x, 0, Scenario.GetPosOfMarker(obj).z);
					Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

					if (Vector3.Distance(SelPos, AllPos) < EditMenu.MirrorTolerance)
					{ //MirrorTolerance
						all.SelectionState = 2;
						bool AlreadyExist = false;
						foreach (WorkingElement MirObj in SymmetrySelectionList[id].MirrorSelected)
						{
							if (MirObj.ListId == all.ListId && MirObj.InstanceId == all.InstanceId)
							{
								AlreadyExist = true;
								break;
							}
						}
						if (!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
						continue;
					}
				}
			}
		}

		void SelectVertical(int id = 0)
		{
			if (SelectedSymmetryMarkers.Count < 0) return;
			SelectedSymmetryMarkers[id].gameObject.SetActive(true);
			SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;
			Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
			MirroredMarker.z = -MirroredMarker.z;
			MirroredMarker += Scenario.MapCenterPoint;
			MirroredMarker.y = SelectedMarker.position.y;
			SelectedSymmetryMarkers[id].position = MirroredMarker;
			SymmetrySelectionList[id].MoveMultiply = new Vector3(1, 1, -1);

			foreach (WorkingElement obj in Selected)
			{
				foreach (WorkingElement all in AllWorkingElements)
				{
					Vector3 MirroredPos = Scenario.GetPosOfMarker(all) - Scenario.MapCenterPoint;
					MirroredPos.z = -MirroredPos.z;
					MirroredPos += Scenario.MapCenterPoint;

					//Xdist = Scenario.MapCenterPoint.x - obj.transform.position.x;
					Vector3 SelPos = new Vector3(Scenario.GetPosOfMarker(obj).x, 0, Scenario.GetPosOfMarker(obj).z);
					Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);


					if (Vector3.Distance(SelPos, AllPos) < EditMenu.MirrorTolerance)
					{ //MirrorTolerance
						all.SelectionState = 2;
						bool AlreadyExist = false;
						foreach (WorkingElement MirObj in SymmetrySelectionList[id].MirrorSelected)
						{
							if (MirObj.InstanceId == all.InstanceId && MirObj.ListId == all.ListId)
							{
								AlreadyExist = true;
								break;
							}
						}
						if (!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
						continue;
					}
				}
			}
		}

		void SelectHorizontalVertical(int id = 0)
		{
			if (SelectedSymmetryMarkers.Count < 0) return;
			SelectedSymmetryMarkers[id].gameObject.SetActive(true);
			SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

			Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
			MirroredMarker.x = -MirroredMarker.x;
			MirroredMarker.z = -MirroredMarker.z;
			MirroredMarker += Scenario.MapCenterPoint;
			MirroredMarker.y = SelectedMarker.position.y;
			SelectedSymmetryMarkers[id].position = MirroredMarker;
			SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, -1);

			foreach (WorkingElement obj in Selected)
			{
				foreach (WorkingElement all in AllWorkingElements)
				{
					Vector3 MirroredPos = Scenario.GetPosOfMarker(all) - Scenario.MapCenterPoint;
					MirroredPos.z = -MirroredPos.z;
					MirroredPos.x = -MirroredPos.x;
					MirroredPos += Scenario.MapCenterPoint;

					Vector3 SelPos = new Vector3(Scenario.GetPosOfMarker(obj).x, 0, Scenario.GetPosOfMarker(obj).z);
					Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

					if (Vector3.Distance(SelPos, AllPos) < EditMenu.MirrorTolerance)
					{ //MirrorTolerance
						all.SelectionState = 2;
						bool AlreadyExist = false;
						foreach (WorkingElement MirObj in SymmetrySelectionList[id].MirrorSelected)
						{
							if (MirObj.InstanceId == all.InstanceId && MirObj.ListId == all.ListId)
							{
								AlreadyExist = true;
								break;
							}
						}
						if (!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
						continue;
					}
				}
			}
		}

		void SelectDiagonal1(int id = 0)
		{
			if (SelectedSymmetryMarkers.Count < 0) return;
			SelectedSymmetryMarkers[id].gameObject.SetActive(true);
			SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

			Vector3 Origin = new Vector3(0, 0, -Scenario.ScenarioData.Size.y / 10f);
			Vector3 Origin2 = new Vector3(Scenario.ScenarioData.Size.y / 10f, 0, 0);
			Vector3 Point = new Vector3(SelectedMarker.position.x, 0, SelectedMarker.position.z);

			Vector3 PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
			Vector3 FinalDir = PointOfMirror - Point;
			FinalDir.y = 0;
			FinalDir.Normalize();
			float FinalDist = Vector3.Distance(PointOfMirror, Point);
			Vector3 MirroredMarker = PointOfMirror + FinalDir * FinalDist;
			MirroredMarker.y = SelectedMarker.position.y;

			//SelectedSymmetryMarkers[id].position = Origin;
			SelectedSymmetryMarkers[id].position = MirroredMarker;
			SymmetrySelectionList[id].MoveMultiply = new Vector3(1, 1, 1);
			SymmetrySelectionList[id].Diagonal = true;

			foreach (WorkingElement obj in Selected)
			{
				foreach (WorkingElement all in AllWorkingElements)
				{

					Point = new Vector3(Scenario.GetPosOfMarker(all).x, 0, Scenario.GetPosOfMarker(all).z);
					PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
					FinalDir = PointOfMirror - Point;
					FinalDir.y = 0;
					FinalDir.Normalize();
					FinalDist = Vector3.Distance(PointOfMirror, Point);
					Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;

					Vector3 SelPos = new Vector3(Scenario.GetPosOfMarker(obj).x, 0, Scenario.GetPosOfMarker(obj).z);
					Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

					if (Vector3.Distance(SelPos, AllPos) < EditMenu.MirrorTolerance)
					{ //MirrorTolerance
						all.SelectionState = 2;
						bool AlreadyExist = false;
						foreach (WorkingElement MirObj in SymmetrySelectionList[id].MirrorSelected)
						{
							if (MirObj.InstanceId == all.InstanceId && MirObj.ListId == all.ListId)
							{
								AlreadyExist = true;
								break;
							}
						}
						if (!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
						continue;
					}
				}
			}
		}


		void SelectDiagonal2(int id = 0)
		{
			if (SelectedSymmetryMarkers.Count < 0) return;
			SelectedSymmetryMarkers[id].gameObject.SetActive(true);
			SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

			Vector3 Origin = new Vector3(0, 0, 0);
			Vector3 Origin2 = new Vector3(Scenario.ScenarioData.Size.y / 10f, 0, -Scenario.ScenarioData.Size.y / 10f);
			Vector3 Point = new Vector3(SelectedMarker.position.x, 0, SelectedMarker.position.z);

			Vector3 PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
			Vector3 FinalDir = PointOfMirror - Point;
			FinalDir.y = 0;
			FinalDir.Normalize();
			float FinalDist = Vector3.Distance(PointOfMirror, Point);
			Vector3 MirroredMarker = PointOfMirror + FinalDir * FinalDist;
			MirroredMarker.y = SelectedMarker.position.y;

			//SelectedSymmetryMarkers[id].position = Origin;
			SelectedSymmetryMarkers[id].position = MirroredMarker;
			SymmetrySelectionList[id].MoveMultiply = new Vector3(-1, 1, -1);
			SymmetrySelectionList[id].Diagonal = true;

			foreach (WorkingElement obj in Selected)
			{
				foreach (WorkingElement all in AllWorkingElements)
				{

					Point = new Vector3(Scenario.GetPosOfMarker(all).x, 0, Scenario.GetPosOfMarker(all).z);
					PointOfMirror = ClosestPointToLine(Origin, Origin2, Point);
					FinalDir = PointOfMirror - Point;
					FinalDir.y = 0;
					FinalDir.Normalize();
					FinalDist = Vector3.Distance(PointOfMirror, Point);
					Vector3 MirroredPos = PointOfMirror + FinalDir * FinalDist;

					Vector3 SelPos = new Vector3(Scenario.GetPosOfMarker(obj).x, 0, Scenario.GetPosOfMarker(obj).z);
					Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

					if (Vector3.Distance(SelPos, AllPos) < EditMenu.MirrorTolerance)
					{ //MirrorTolerance
						all.SelectionState = 2;
						bool AlreadyExist = false;
						foreach (WorkingElement MirObj in SymmetrySelectionList[id].MirrorSelected)
						{
							if (MirObj.InstanceId == all.InstanceId && MirObj.ListId == all.ListId)
							{
								AlreadyExist = true;
								break;
							}
						}
						if (!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
						continue;
					}
				}
			}
		}

		void SelectRotateByCenter(int id = 0, float angle = 180)
		{
			if (SelectedSymmetryMarkers.Count < 0) return;
			SelectedSymmetryMarkers[id].gameObject.SetActive(true);
			SelectedSymmetryMarkers[id].localScale = SelectedMarker.localScale;

			Vector3 MirroredMarker = SelectedMarker.position - Scenario.MapCenterPoint;
			MirroredMarker = RotatePointAroundPivot(SelectedMarker.position, Scenario.MapCenterPoint, angle);

			SelectedSymmetryMarkers[id].position = MirroredMarker;
			SymmetrySelectionList[id].MoveMultiply = Vector3.one;
			SymmetrySelectionList[id].MoveRotation = angle;

			foreach (WorkingElement obj in Selected)
			{
				foreach (WorkingElement all in AllWorkingElements)
				{
					Vector3 MirroredPos = Scenario.GetPosOfMarker(all) - Scenario.MapCenterPoint;
					MirroredPos = RotatePointAroundPivot(Scenario.GetPosOfMarker(all), Scenario.MapCenterPoint, angle);

					Vector3 SelPos = new Vector3(Scenario.GetPosOfMarker(obj).x, 0, Scenario.GetPosOfMarker(obj).z);
					Vector3 AllPos = new Vector3(MirroredPos.x, 0, MirroredPos.z);

					if (Vector3.Distance(SelPos, AllPos) < EditMenu.MirrorTolerance)
					{ //MirrorTolerance
						all.SelectionState = 2;
						bool AlreadyExist = false;
						foreach (WorkingElement MirObj in SymmetrySelectionList[id].MirrorSelected)
						{
							if (MirObj.InstanceId == all.InstanceId && MirObj.ListId == all.ListId)
							{
								AlreadyExist = true;
								break;
							}
						}
						if (!AlreadyExist) SymmetrySelectionList[id].MirrorSelected.Add(all);
						continue;
					}
				}
			}
		}

		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
		{
			Vector3 dir = point - pivot;
			dir = Quaternion.Euler(Vector3.up * angle) * dir;
			point = dir + pivot;
			return point;
		}

		public static Vector3 ClosestPointToLine(Vector3 A, Vector3 B, Vector3 P)
		{
			Vector3 AP = P - A;
			Vector3 AB = B - A;
			float ab2 = AB.x * AB.x + AB.z * AB.z;
			float ap_ab = AP.x * AB.x + AP.z * AB.z;
			float t = ap_ab / ab2;
			Vector3 Closest = A + AB * t;
			return Closest;
		}
		#endregion

	}
}
