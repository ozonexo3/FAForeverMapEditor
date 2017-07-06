using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;

public class SymmetryWindow : MonoBehaviour {

	public		Editing			EditMenu;
	public		Toggle[]		Toggles;
	public		Slider			AngleSlider;
	public		InputField		ToleranceInput;

	bool Enabling = false;

	void OnEnable(){
		Enabling = true;
		/*foreach(Toggle tog in Toggles){
			tog.isOn = false;
		}*/
		Debug.Log(PlayerPrefs.GetInt("Symmetry", 0));
		Toggles[ PlayerPrefs.GetInt("Symmetry", 0) ].isOn = true;
		AngleSlider.value = PlayerPrefs.GetInt("SymmetryAngleCount", 2);
		ToleranceInput.text =  PlayerPrefs.GetFloat("SymmetryTolerance", 0.4f).ToString();
		Enabling = false;
	}

	public void SliderChange(){
		if(Enabling) return;
		bool AnythingChanged = false;
		if(GetTolerance() != (int)AngleSlider.value){
			PlayerPrefs.SetInt("SymmetryAngleCount", (int)AngleSlider.value);
			AnythingChanged = true;
		}
		if(float.Parse(ToleranceInput.text) != PlayerPrefs.GetFloat("SymmetryTolerance", 0.4f)){
			PlayerPrefs.SetFloat("SymmetryTolerance", float.Parse(ToleranceInput.text));
			EditMenu.MirrorTolerance = float.Parse(ToleranceInput.text);
			AnythingChanged = true;
		}

		if(AnythingChanged) EditMenu.EditMarkers.UpdateSelectionRing();
	}

	public void Button(string func){
		if(Enabling) return;
		Debug.Log("Change symmetry: " + func);
		switch(func){
		case "close":
			gameObject.SetActive(false);
			break;
		case "sym0":
			if(Toggles[0].isOn) PlayerPrefs.SetInt("Symmetry", 0);
			break;
		case "sym1":
			if(Toggles[1].isOn) PlayerPrefs.SetInt("Symmetry", 1);
			break;
		case "sym2":
			if(Toggles[2].isOn) PlayerPrefs.SetInt("Symmetry", 2);
			break;
		case "sym3":
			if(Toggles[3].isOn) PlayerPrefs.SetInt("Symmetry", 3);
			break;
		case "sym4":
			if(Toggles[4].isOn) PlayerPrefs.SetInt("Symmetry", 4);
			break;
		case "sym5":
			if(Toggles[5].isOn) PlayerPrefs.SetInt("Symmetry", 5);
			break;
		case "sym6":
			if(Toggles[6].isOn) PlayerPrefs.SetInt("Symmetry", 6);
			break;
		case "sym7":
			if(Toggles[7].isOn) PlayerPrefs.SetInt("Symmetry", 7);
			break;
		case "sym8":
			if(Toggles[8].isOn) PlayerPrefs.SetInt("Symmetry", 8);
			break;
		}
		PlayerPrefs.Save();

		EditMenu.EditMarkers.UpdateSelectionRing();
	}



	public static float GetTolerance()
	{
		return PlayerPrefs.GetInt("SymmetryAngleCount", 0);
	}
}
