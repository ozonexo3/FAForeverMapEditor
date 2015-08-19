using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorVersion : MonoBehaviour {

	void Start () {
		GetComponent<Text>().text = "v0.407";
	}

}
