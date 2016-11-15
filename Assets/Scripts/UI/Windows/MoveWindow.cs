using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MoveWindow : MonoBehaviour {

	public RectTransform Trans;
	Vector2 BeginPos;
	Vector2 MousePos;
	Vector2 DeltaMove;
	public bool IsDrag = false;

	void OnEnable(){
		Trans.anchoredPosition = Vector2.zero;
	}

	public void OnDragBegin(){
		IsDrag = true;
		BeginPos = Trans.anchoredPosition;
		MousePos = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
	}

	public void OnDragEnd(){
		IsDrag = false;
	}

	public void OnDragMove () {
		if (!IsDrag)
			return;

		if (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Screen.width || Input.mousePosition.y >= Screen.height)
			return;

		DeltaMove = (new Vector2 (Input.mousePosition.x, Input.mousePosition.y) - MousePos);

		Trans.anchoredPosition = BeginPos + DeltaMove;
	}
}
