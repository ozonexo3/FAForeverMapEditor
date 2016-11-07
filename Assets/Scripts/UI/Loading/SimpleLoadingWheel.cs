using UnityEngine;
using System.Collections;

public class SimpleLoadingWheel : MonoBehaviour {

	void OnEnable(){
		lerp = 0;
		transform.localRotation = Quaternion.identity;
	}

	float lerp = 0;

	void Update () {
		lerp += Time.unscaledDeltaTime * 1.2f;
		if(lerp >= 1)
			lerp --;

		transform.localRotation = Quaternion.Euler(Vector3.forward * -360 * lerp);
	}
}
