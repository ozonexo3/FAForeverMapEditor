using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderLabel : MonoBehaviour {

	public		Text		Label;
	public		Slider		ValueSlider;

	void OnEnable(){
		UpdateText();
	}

	public void UpdateText(){
		Label.text = ValueSlider.value.ToString();
	}
}
