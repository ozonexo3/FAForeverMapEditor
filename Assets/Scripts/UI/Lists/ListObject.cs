using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ListObject : MonoBehaviour {

	public		Text 				ObjectName;
	public		Image				Icon;
	public		GameObject			Selected;
	public		GameObject			SymmetrySelected;
	//public		CameraControler		KameraKontroler;
	public		int					InstanceId;
	public		int					ListId;
	public		GameObject			ConnectedGameObject;
	public System.Action<GameObject> ClickAction;
	public System.Action<int> ClickActionId;

	public void SetSelection(int id){
		Selected.SetActive(id == 1);
		SymmetrySelected.SetActive(id == 2);
	}

	public void Unselect(){
		Selected.SetActive(false);
		SymmetrySelected.SetActive(false);
	}

	public void Select(){
		Selected.SetActive(true);
	}

	public void Clicked(){
		//KameraKontroler.MarkerList(ConnectedGameObject);
		if (ConnectedGameObject)
			ClickAction(ConnectedGameObject);
		//Selection.SelectionManager.Current.SelectObject(ConnectedGameObject);
		else
		{
			ClickActionId(InstanceId);

		}
	}
}
