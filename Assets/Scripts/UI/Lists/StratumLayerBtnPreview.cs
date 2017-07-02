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

	public void Show(Texture2D Image, GameObject Parent, float offset = 26f)
	{
		Img.texture = Image;
		gameObject.SetActive(true);

		transform.localPosition = Vector3.right * transform.localPosition.x + Vector3.up * (transform.parent.InverseTransformPoint(Parent.transform.position).y - offset);

		LastParent = Parent;

	}
	
	// Update is called once per frame
	public void Hide (GameObject Parent) {
		if (LastParent == Parent)
			gameObject.SetActive(false);
	}
}
