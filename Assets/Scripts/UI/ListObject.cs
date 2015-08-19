using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ListObject : MonoBehaviour {

	public		Text 				ObjectName;
	public		Image				Icon;
	public		GameObject			Selected;
	public		CameraControler		KameraKontroler;
	public		int					InstanceId;
	public		int					ListId;

	public void Unselect(){
		Selected.SetActive(false);
	}

	public void Select(){
		Selected.SetActive(true);
	}

	public void Clicked(){
		//KameraKontroler.MarkerList(ConnectedGameObject);
	}
}
