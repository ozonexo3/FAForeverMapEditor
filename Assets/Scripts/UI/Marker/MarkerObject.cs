using System.Collections;
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
				//X > Height
				//Y > Direction
				rot = new Vector3(rot.y, -rot.x, 0) * Mathf.Rad2Deg;

				//Clamp unproper rotations
				while (rot.x < -180)
					rot.x += 360;
				while (rot.x >= 360)
					rot.x -= 360;
				rot.x = Mathf.Clamp(rot.x, 0, 180);

				// Invert rotation and offset it by 90 degree
				rot.x = 90f - rot.x;

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
					Euler.x += 360f;

				Euler.x = 90f - Euler.x;

				Euler = new Vector3(-Euler.y, Euler.x, 0);
				
				Euler *= Mathf.Deg2Rad;
				Euler.x = Round(Euler.x);
				Euler.y = Round(Euler.y);
				return Euler;
			}
			else
			{
				Vector3 Euler = Rot.eulerAngles;
				Euler.z = 0;
				return Euler * Mathf.Deg2Rad;
			}
		}

		const float Rounding = 1000000f;
		static float Round(float value)
		{
			return Mathf.Round(value * Rounding) / Rounding; 
		}
	}
}
