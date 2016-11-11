using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericInfoPopup : MonoBehaviour {

	public		Text		InfoText;

	public void Show(bool on, string text = ""){
		gameObject.SetActive(on);
		InfoText.text = text;
	}

	public void Hide(){
		gameObject.SetActive(false);
	}

	public void InvokeHide(){
		Invoke("Hide", 4);
	}

}
