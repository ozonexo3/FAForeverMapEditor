using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;
using Ozone.UI;

public class SymmetryWindow : MonoBehaviour {

	public		Editing			EditMenu;
	public		Toggle[]		Toggles;
	public		UiTextField			AngleSlider;
	public UiTextField ToleranceInput;

	bool Enabling = false;


	public delegate void SymmetryChange();
	public static event SymmetryChange OnSymmetryChanged;

	void OnEnable(){
		Enabling = true;
		/*foreach(Toggle tog in Toggles){
			tog.isOn = false;
		}*/
		Toggles[ PlayerPrefs.GetInt("Symmetry", 0) ].isOn = true;
		AngleSlider.SetValue(SymmetryWindow.GetRotationSym());
		ToleranceInput.SetValue(GetTolerance());
		Enabling = false;
	}

	public void ToleranceChange()
	{
		if (ToleranceInput.value != GetTolerance())
		{
			PlayerPrefs.SetFloat("SymmetryTolerance", ToleranceInput.value);
			InvokeChange();

		}
	}

	public void SliderChange(){

		if(AngleSlider.intValue != SymmetryWindow.GetRotationSym())
		{
			int value = AngleSlider.intValue;
			if (value < 2)
				value = 2;
			else if (value > 16)
				value = 16;

			PlayerPrefs.SetInt("SymmetryAngleCount", value);
			InvokeChange();

		}
	}

	void InvokeChange()
	{
		if(OnSymmetryChanged != null)
			OnSymmetryChanged.Invoke();
	}

	public void Button(string func){
		if(Enabling) return;
		//Debug.Log("Change symmetry: " + func);
		switch(func){
		case "close":
			gameObject.SetActive(false);
			break;
		case "sym0":
			if(Toggles[0].isOn) SaveSymmetry(0);
			break;
		case "sym1":
			if(Toggles[1].isOn) SaveSymmetry(1);
			break;
		case "sym2":
			if(Toggles[2].isOn) SaveSymmetry(2);
			break;
		case "sym3":
			if(Toggles[3].isOn) SaveSymmetry(3);
			break;
		case "sym4":
			if(Toggles[4].isOn) SaveSymmetry(4);
			break;
		case "sym5":
			if(Toggles[5].isOn) SaveSymmetry(5);
			break;
		case "sym6":
			if(Toggles[6].isOn) SaveSymmetry(6);
			break;
		case "sym7":
			if(Toggles[7].isOn) SaveSymmetry(7);
			break;
		case "sym8":
			if(Toggles[8].isOn) SaveSymmetry(8);
			break;
		}
	}

	void SaveSymmetry(int id)
	{
		if (GetSymmetryType() == id)
			return;

		PlayerPrefs.SetInt("Symmetry", id);
		PlayerPrefs.Save();
		InvokeChange();
	}


	public static float GetTolerance()
	{
		return PlayerPrefs.GetFloat("SymmetryTolerance", 0.4f);
	}

	public static int GetSymmetryType()
	{
		return PlayerPrefs.GetInt("Symmetry", 0);
	}

	public static int GetRotationSym()
	{
		int count = PlayerPrefs.GetInt("SymmetryAngleCount", 2);

		if (count < 2)
		{
			PlayerPrefs.SetInt("SymmetryAngleCount", 2);
			return 2;
		}
		return count;
	}
}
