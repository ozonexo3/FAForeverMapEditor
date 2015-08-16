using UnityEngine;
using System.Collections;

public class SelectionRing : MonoBehaviour {

	public		Transform		SelectedObject;

	public void ForceUpdate(){
		transform.position = SelectedObject.position;
	}

	void Update () {
		transform.position = SelectedObject.position;
	}
}
