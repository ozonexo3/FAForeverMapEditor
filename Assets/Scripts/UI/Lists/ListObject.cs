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
	public System.Action<int> DragAction;
	public CanvasGroup Cg;

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

	public static int DragBeginId = -1;

	public void DropObject()
	{
		//Debug.Log("Drop");
		DragAction(InstanceId);

	}

	public void InitializeDrag()
	{
		DragBeginId = InstanceId;
	}

	public void BeginDrag()
	{
		//Debug.Log("BeginDrag");
		Cg.alpha = 0.12f;
	}

	public void Drag()
	{
	}

	public void EndDrag()
	{
		//Debug.Log("EndDrag");
		Cg.alpha = 1f;
	}
}
