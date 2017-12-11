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
	public System.Action<ListObject> ClickActionId;
	public System.Action<ListObject> ClickApplyActionId;
	public System.Action<ListObject> ClickCloseActionId;
	public System.Action<ListObject> DragAction;
	public CanvasGroup Cg;

	public void DisableRendering()
	{
		gameObject.SetActive(false);
	}

	public void EnableRendering()
	{
		gameObject.SetActive(true);
	}


	public void SetSelection(int id){
		if(Selected)
			Selected.SetActive(id == 1);
		if(SymmetrySelected)
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
			ClickActionId(this);
		}
	}

	public void ClickedClose()
	{
		if (ClickCloseActionId != null)
			ClickCloseActionId(this);
	}

	public void Apply()
	{
		ClickApplyActionId(this);
	}

#region Drag
	public static ListObject DragBeginId;

	public void DropObject()
	{
		//Debug.Log("Drop");
		DragAction(this);

	}

	public void InitializeDrag()
	{
		DragBeginId = this;
	}

	public void BeginDrag()
	{
		//Debug.Log("BeginDrag");
		if(Cg)
		Cg.alpha = 0.12f;
	}

	public void Drag()
	{
	}

	public void EndDrag()
	{
		//Debug.Log("EndDrag");
		if (Cg)
			Cg.alpha = 1f;
	}
#endregion
}
