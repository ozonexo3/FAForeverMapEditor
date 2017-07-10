using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MapData : MonoBehaviour
{

	//**************** PROPS
	#region Props
	[System.Serializable]
	public class PropsSettings
	{


	}
	#endregion


	//**************** MARKERS
	#region Markers
	[System.Serializable]
	public class MarkerSettings
	{
		public MarkerGroup[] MarkerGroups;

		public MarkerSettings()
		{
			MarkerGroups = new MarkerGroup[0];

		}

	}

	[System.Serializable]
	public class MarkerGroup
	{
		public string Name;
		public int[] MarkerIds;

	}

	#endregion


	//**************** DECALS
	#region Decals
	[System.Serializable]
	public class DecalSettings
	{
		public DecalGroup[] DecalLayers;

		public DecalSettings()
		{
			DecalLayers = new DecalGroup[0];

		}

	}

	[System.Serializable]
	public class DecalGroup
	{
		public string Name;
		public int[] DecalIds;

	}

	#endregion
}
