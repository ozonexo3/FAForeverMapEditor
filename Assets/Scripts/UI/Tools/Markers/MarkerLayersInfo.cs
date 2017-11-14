using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Markers;

namespace EditMap
{
	public class MarkerLayersInfo : MonoBehaviour
	{

		public Toggle BlankActive;
		public Toggle SpawnActive;
		public Toggle ResourcesActive;
		public Toggle CameraActive;


		public Toggle LandNodesActive;
		public Toggle AmphibiousNodesActive;
		public Toggle NavalNodesActive;
		public Toggle AirNodesActive;
		public Toggle ConnectionsActive;

		public Toggle RallyPointActive;
		public Toggle NavyRallyPointActive;

		public Toggle OtherActive;

		public void ValuesChanged()
		{
			MarkersControler.Current.MarkerLayersSettings.Blank = BlankActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.Spawn = SpawnActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.Resource = ResourcesActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.Camera = CameraActive.isOn;

			MarkersControler.Current.MarkerLayersSettings.LandNodes = LandNodesActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.AmphibiousNodes = AmphibiousNodesActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.NavyNodes = NavalNodesActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.AirNodes = AirNodesActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.ConnectedNodes = ConnectionsActive.isOn;

			MarkersControler.Current.MarkerLayersSettings.RallyPoint = RallyPointActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.NavyRallyPoint = NavyRallyPointActive.isOn;


			MarkersControler.Current.MarkerLayersSettings.Other = OtherActive.isOn;

			MarkersControler.UpdateLayers();

			MarkersInfo.Current.MarkerList.UpdateSelection();
		}
	}
}
