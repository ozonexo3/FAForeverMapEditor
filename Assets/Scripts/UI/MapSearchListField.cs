using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapSearchListField : MonoBehaviour {

	public		Text 				ObjectName;
	public		int					Id;
	public		GameObject			Selected;
	public		SearchMapPopup		Controler;

	public void Click(){
		Controler.ClickedField (Id);
	}
}
