using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlerScale : MonoBehaviour {

	public Transform CamPos;
	public Transform Pos;

	void LateUpdate() {
		Plane plane = new Plane(CamPos.forward, CamPos.position);
		float dist = plane.GetDistanceToPoint(Pos.position) * 0.5f;
		if (dist < 1)
			dist = 1;

		Pos.localScale = Vector3.one * dist;
	}
}
