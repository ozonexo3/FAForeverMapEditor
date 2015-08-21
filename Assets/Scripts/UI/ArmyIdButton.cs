using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArmyIdButton : MonoBehaviour {

	public		ArmyListId		Controler;
	public		int				Id;
	public		Text			Name;
	public		Image			Select;

	public void Clicked(){
		Controler.Selected(Id);
	}

}
