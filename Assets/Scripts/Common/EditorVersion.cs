using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorVersion : MonoBehaviour {

	public const string EditorBuildVersion = "v0.513 HF3 Alpha";

	void Start () {
		GetComponent<Text>().text = EditorBuildVersion;
	}

}
