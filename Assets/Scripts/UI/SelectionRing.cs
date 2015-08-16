using UnityEngine;
using System.Collections;

public class SelectionRing : MonoBehaviour {

	public		Transform		SelectedObject;
	
	void Update () {
		transform.position = SelectedObject.position;
	}
}
