using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SymmetryWindow : MonoBehaviour {

	public		Toggle[]		Toggles;
	public		Slider			AngleSlider;

	void OnEnable(){
		foreach(Toggle tog in Toggles){
			tog.isOn = false;
		}
		Toggles[PlayerPrefs.GetInt("Symmetry", 0)].isOn = true;
	}

	public void SliderChange(){
		PlayerPrefs.SetInt("SymmetryAngleCount", (int)AngleSlider.value);
	}

	public void Button(string func){
		switch(func){
		case "close":
			gameObject.SetActive(false);
			break;
		case "sym0":
			PlayerPrefs.SetInt("Symmetry", 0);
			break;
		case "sym1":
			PlayerPrefs.SetInt("Symmetry", 1);
			break;
		case "sym2":
			PlayerPrefs.SetInt("Symmetry", 2);
			break;
		case "sym3":
			PlayerPrefs.SetInt("Symmetry", 3);
			break;
		case "sym4":
			PlayerPrefs.SetInt("Symmetry", 4);
			break;
		case "sym5":
			PlayerPrefs.SetInt("Symmetry", 5);
			break;
		case "sym6":
			PlayerPrefs.SetInt("Symmetry", 6);
			break;
		}
	}
}
