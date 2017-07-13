using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Markers;

namespace EditMap
{
	public class NewMarkersInfo : MonoBehaviour
	{



		void OnEnable()
		{
			Selection.SelectionManager.Current.SetAffectedGameObjects(MarkersControler.GetMarkerObjects());
		}


		void OnDisable()
		{
			Selection.SelectionManager.Current.SetAffectedGameObjects(new GameObject[0]);
		}


		public GameObject[] CreateButtonSelections;
		int CreationId;
		public Dropdown AiCreationDropdown;
		public Dropdown SpawnPressetDropdown;
		public void SelectCreateNew(int id)
		{
			for (int i = 0; i < CreateButtonSelections.Length; i++)
				CreateButtonSelections[i].SetActive(false);

			if (id == CreationId)
			{
				CreationId = -1;
			}
			else
			{
				CreationId = id;
				CreateButtonSelections[CreationId].SetActive(true);
			}

			AiCreationDropdown.gameObject.SetActive(CreationId == 4);
			SpawnPressetDropdown.gameObject.SetActive(CreationId == 5);

		}



	}
}
