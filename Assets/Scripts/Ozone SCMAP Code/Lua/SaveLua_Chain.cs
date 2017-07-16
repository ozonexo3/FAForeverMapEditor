using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapLua
{
	public partial class SaveLua
	{

		[System.Serializable]
		public class Chain
		{
			public string Name;
			public string[] Markers;
			public List<Marker> ConnectedMarkers;
			public const string KEY_MARKERS = "Markers";

			public void ConnectMarkers(List<Marker> SearchMarkers)
			{
				ConnectedMarkers = new List<Marker>();
				int Count = SearchMarkers.Count;
				for (int n = 0; n < Markers.Length; n++)
				{
					for (int i = 0; i < Count; i++)
					{
						if (SearchMarkers[i].Name == Markers[n])
						{
							ConnectedMarkers.Add(SearchMarkers[i]);

							if(!SearchMarkers[i].ConnectedToChains.Contains(this))
								SearchMarkers[i].ConnectedToChains.Add(this);

							break;
						}
					}
				}
			}

			public void BakeMarkers()
			{
				List <string> NewMarkers = new List<string>();

				for (int i = 0; i < ConnectedMarkers.Count; i++)
				{
					if (ConnectedMarkers[i] != null && ConnectedMarkers[i].MarkerObj != null)
						NewMarkers.Add(ConnectedMarkers[i].Name);
				}
				Markers = NewMarkers.ToArray();
			}
		}
	}
}
