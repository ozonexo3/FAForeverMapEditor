using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlerScale : MonoBehaviour {

	public Transform CamPos;
	public Transform Pos;
	public float Dist = 0;

	void LateUpdate() {
		//Dist = Vector3.Distance(CamPos.position, Pos.position);
		//Pos.localScale = Vector3.one * Mathf.Lerp(1f, 20f, Mathf.Pow(Dist / 200f - 0,5f, 0.5f));

		Plane plane = new Plane(CamPos.forward, CamPos.position);
		float dist = plane.GetDistanceToPoint(Pos.position);
		if (dist < 2)
			dist = 2;

		Pos.localScale = Vector3.one * dist * 0.5f;
	}
}
