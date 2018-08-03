using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapLua;

namespace Markers
{
	public class MarkerObject : SelectionObject
	{
		public SaveLua.Marker Owner;


		public static Quaternion ScmapRotToMarkerRot(Vector3 rot, SaveLua.Marker.MarkerTypes Type)
		{
			if (Type == SaveLua.Marker.MarkerTypes.CameraInfo)
			{
				rot = new Vector3(rot.y, -rot.x, 0) * Mathf.Rad2Deg;
				Debug.Log(rot);
				return Quaternion.Euler(rot);
			}
			else
			{
				return Quaternion.Euler(rot * Mathf.Rad2Deg);
			}
		}

		public static Vector3 MarkerRotToScmapRot(Quaternion Rot, SaveLua.Marker.MarkerTypes Type)
		{
			if (Type == SaveLua.Marker.MarkerTypes.CameraInfo)
			{
				Vector3 Euler = Rot.eulerAngles;
				Euler.z = 0;

				while (Euler.x < 0)
					Euler.x += 360;

				Euler = new Vector3(-Euler.y, Euler.x, 0);
				Debug.Log(Euler);
				return Euler * Mathf.Deg2Rad;
			}
			else
			{
				Vector3 Euler = Rot.eulerAngles;
				Euler.z = 0;
				return Euler * Mathf.Deg2Rad;
			}
		}
	}
}
