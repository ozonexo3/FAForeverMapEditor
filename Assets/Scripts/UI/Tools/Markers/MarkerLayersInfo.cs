using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Markers;

namespace EditMap
{
	public class MarkerLayersInfo : MonoBehaviour
	{
		public Toggle AllActive;

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

		public Toggle CombatActive;
		public Toggle DefenseActive;
		public Toggle ProtExpActive;
		public Toggle ExpandActive;
		public Toggle OtherActive;

		private void OnEnable()
		{
			IgnoreEvent = true;

			BlankActive.isOn = MarkersControler.Current.MarkerLayersSettings.Blank;
			SpawnActive.isOn = MarkersControler.Current.MarkerLayersSettings.Spawn;
			ResourcesActive.isOn = MarkersControler.Current.MarkerLayersSettings.Resource;
			CameraActive.isOn = MarkersControler.Current.MarkerLayersSettings.Camera;

			LandNodesActive.isOn = MarkersControler.Current.MarkerLayersSettings.LandNodes;
			AmphibiousNodesActive.isOn = MarkersControler.Current.MarkerLayersSettings.AmphibiousNodes;
			NavalNodesActive.isOn = MarkersControler.Current.MarkerLayersSettings.NavyNodes;
			AirNodesActive.isOn = MarkersControler.Current.MarkerLayersSettings.AirNodes;
			ConnectionsActive.isOn = MarkersControler.Current.MarkerLayersSettings.ConnectedNodes;

			CombatActive.isOn = MarkersControler.Current.MarkerLayersSettings.Combat;
			DefenseActive.isOn = MarkersControler.Current.MarkerLayersSettings.Defense;
			ProtExpActive.isOn = MarkersControler.Current.MarkerLayersSettings.ProtExp;
			RallyPointActive.isOn = MarkersControler.Current.MarkerLayersSettings.RallyPoint;
			ExpandActive.isOn = MarkersControler.Current.MarkerLayersSettings.Expand;
			OtherActive.isOn = MarkersControler.Current.MarkerLayersSettings.Other;

			UpdateAllToggle();

			IgnoreEvent = false;
		}

		bool IgnoreEvent = false;
		public void AllChanged()
		{
			if (IgnoreEvent)
				return;
			IgnoreEvent = true;

			BlankActive.isOn = AllActive.isOn;
			SpawnActive.isOn = AllActive.isOn;
			ResourcesActive.isOn = AllActive.isOn;
			CameraActive.isOn = AllActive.isOn;

			LandNodesActive.isOn = AllActive.isOn;
			AmphibiousNodesActive.isOn = AllActive.isOn;
			NavalNodesActive.isOn = AllActive.isOn;
			AirNodesActive.isOn = AllActive.isOn;
			ConnectionsActive.isOn = AllActive.isOn;

			RallyPointActive.isOn = AllActive.isOn;

			CombatActive.isOn = AllActive.isOn;
			DefenseActive.isOn = AllActive.isOn;
			ProtExpActive.isOn = AllActive.isOn;
			ExpandActive.isOn = AllActive.isOn;
			OtherActive.isOn = AllActive.isOn;

			IgnoreEvent = false;

			ValuesChanged();
		}

		public void ValuesChanged()
		{
			if (IgnoreEvent)
				return;

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

			MarkersControler.Current.MarkerLayersSettings.Combat = CombatActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.Defense = DefenseActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.ProtExp = ProtExpActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.Expand = ExpandActive.isOn;
			MarkersControler.Current.MarkerLayersSettings.Other = OtherActive.isOn;

			MarkersControler.Current.MarkerLayersSettings.SavePrefs();

			UpdateAllToggle();

			MarkersControler.UpdateLayers();

			//MarkersInfo.Current.MarkerList.UpdateSelection();
		}

		void UpdateAllToggle()
		{
			IgnoreEvent = true;
			bool AllSet = BlankActive.isOn;
			AllSet &= SpawnActive.isOn;
			AllSet &= ResourcesActive.isOn;
			AllSet &= CameraActive.isOn;

			AllSet &= LandNodesActive.isOn;
			AllSet &= AmphibiousNodesActive.isOn;
			AllSet &= NavalNodesActive.isOn;
			AllSet &= AirNodesActive.isOn;
			AllSet &= ConnectionsActive.isOn;

			AllSet &= RallyPointActive.isOn;

			AllSet &= CombatActive.isOn;
			AllSet &= DefenseActive.isOn;
			AllSet &= ProtExpActive.isOn;
			AllSet &= ExpandActive.isOn;
			AllSet &= OtherActive.isOn;

			AllActive.isOn = AllSet;

			IgnoreEvent = false;
		}
	}
}
