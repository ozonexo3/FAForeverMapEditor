using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Markers {
	public class Marker2D : MonoBehaviour {

		public float Scale = 1;

		void OnEnable()
		{
			SampleScale();
			MarkersControler.Marker2DComponents.Add(this);
		}

		void OnDisable()
		{
			MarkersControler.Marker2DComponents.Remove(this);
		}

		public void SampleScale()
		{
			Bounds mrb = GetComponent<MeshFilter>().sharedMesh.bounds;

			Scale = Mathf.Lerp(1 / Mathf.Max(mrb.size.x, mrb.size.y, mrb.size.z, 0.01f), 1f, 0.5f);
		}

		public void UpdateScale()
		{
			Plane plane = new Plane(CameraControler.CamForward, CameraControler.CamPos);
			float dist = plane.GetDistanceToPoint(transform.localPosition) * 0.5f * 0.05f * Scale;
			if (dist < 1)
				dist = 1;

			transform.localScale = Vector3.one * dist;
		}
	}
}
