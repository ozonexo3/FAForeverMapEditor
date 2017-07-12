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

	}
}
