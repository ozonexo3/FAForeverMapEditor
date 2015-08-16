using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AppMenu : MonoBehaviour {

	public		GameObject			Symmetry;
	public		Editing				EditingMenu;
	public		MapHelperGui		MapHelper;
	public		bool 				MenuOpen = false;
	public		bool				ButtonClicked;

	public		Button[]			Buttons;
	public		GameObject[]		Popups;
	public		Toggle				GridToggle;

	void LateUpdate(){

		if(MenuOpen){
			if(ButtonClicked){
				ButtonClicked = false;
				return;
			}
			if(Input.GetMouseButtonUp(0)){
				foreach(GameObject obj in Popups){
					obj.SetActive(false);
				}
				foreach(Button but in Buttons){
					but.interactable = true;
				}
				MenuOpen = false;
			}
		}
	}

	public void MenuButton(string func){
		if(!MenuOpen) return;
		switch(func){
			case "NewMap":
			MapHelper.Map = false;
			MapHelper.OpenComposition(0);
			break;
		case "Save":
			EditingMenu.ButtonFunction("Save");
			break;
		case "SaveAs":
			EditingMenu.ButtonFunction("Save");
			break;
		case "Symmetry":
			Symmetry.SetActive(true);
			break;
		case "Grid":
			MapHelper.Loader.HeightmapControler.ToogleGrid(GridToggle.isOn);
			break;
		}
	}

	public void OpenMenu(string func){
		if(MenuOpen){

		}

		foreach(GameObject obj in Popups){
			obj.SetActive(false);
		}
		foreach(Button but in Buttons){
			but.interactable = true;
		}

		switch(func){
		case "File":
			Popups[0].SetActive(true);
			Buttons[0].interactable = false;
			break;
		case "Tools":
			Popups[1].SetActive(true);
			Buttons[1].interactable = false;
			break;
		case "Symmetry":
			Popups[2].SetActive(true);
			Buttons[2].interactable = false;
			break;
		}

		MenuOpen = true;
		ButtonClicked = true;
	}
}
