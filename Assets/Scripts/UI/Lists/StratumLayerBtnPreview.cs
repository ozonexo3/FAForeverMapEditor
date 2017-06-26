using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StratumLayerBtnPreview : MonoBehaviour {

	public RawImage Img;

	GameObject LastParent;

	// Use this for initialization
	public void Show (RawImage BtnPrev) {
		Img.texture = BtnPrev.texture;
		Img.material = BtnPrev.material;
		gameObject.SetActive(true);

		transform.localPosition = Vector3.right * transform.localPosition.x + Vector3.up * (transform.parent.InverseTransformPoint(BtnPrev.transform.parent.position).y - 26f);

		LastParent = BtnPrev.transform.parent.gameObject;
	}
	
	// Update is called once per frame
	public void Hide (GameObject Parent) {
		if (LastParent == Parent)
			gameObject.SetActive(false);
	}
}
