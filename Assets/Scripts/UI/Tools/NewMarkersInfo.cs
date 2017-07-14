using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Markers;
using Selection;

namespace EditMap
{
	public class NewMarkersInfo : MonoBehaviour
	{

		void OnEnable()
		{
			if (CreationId >= 0)
				SelectCreateNew(CreationId);
			GoToSelection();
		}


		void OnDisable()
		{
			Selection.SelectionManager.Current.SetAffectedGameObjects(new GameObject[0]);
			PlacementManager.Clear();
		}

		void GoToSelection()
		{
			Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects());
			PlacementManager.Clear();
			ChangeControlerType.Current.UpdateButtons();
		}

		void GoToCreation()
		{
			Selection.SelectionManager.Current.SetAffectedGameObjects(new GameObject[0]);
			PlacementManager.BeginPlacement(GetCreationObject(), Place);
			ChangeControlerType.Current.UpdateButtons();
		}


		public GameObject[] CreateButtonSelections;
		int CreationId;
		public Dropdown AiCreationDropdown;
		public Dropdown SpawnPressetDropdown;


		public GameObject MarkerPrefab;
		public GameObject[] MarkerPresets;

		public void SelectCreateNew(int id)
		{
			for (int i = 0; i < CreateButtonSelections.Length; i++)
				CreateButtonSelections[i].SetActive(false);

			if (id == CreationId)
			{
				CreationId = -1;
				GoToSelection();
			}
			else
			{
				CreationId = id;
				CreateButtonSelections[CreationId].SetActive(true);
				GoToCreation();
			}

			AiCreationDropdown.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.gameObject.SetActive(CreationId == 5);
		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations)
		{
			Debug.Log(Positions[0]);
		}


		GameObject GetCreationObject()
		{
			if(CreationId == 5)
			{
				return MarkerPresets[SpawnPressetDropdown.value];
			}
			else
			{

				MapLua.SaveLua.Marker.MarkerTypes Mt = MapLua.SaveLua.Marker.MarkerTypes.Mass;

				if(CreationId == 0)
					Mt = MapLua.SaveLua.Marker.MarkerTypes.BlankMarker;
				else if (CreationId == 1)
					Mt = MapLua.SaveLua.Marker.MarkerTypes.Mass;
				else if (CreationId == 2)
					Mt = MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon;
				else if (CreationId == 3)
					Mt = MapLua.SaveLua.Marker.MarkerTypes.CameraInfo;

				MarkersControler.MarkerPropGraphic Mpg = MarkersControler.GetPropByType(Mt);

				MarkerNew NewMarkerObject = MarkerPrefab.GetComponent<MarkerNew>();
				NewMarkerObject.Mf.sharedMesh = Mpg.SharedMesh;
				NewMarkerObject.Mr.sharedMaterial = Mpg.SharedMaterial;

				return MarkerPrefab;

			}

		}

	}
}
