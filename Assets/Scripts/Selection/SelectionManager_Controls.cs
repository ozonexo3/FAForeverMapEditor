using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{


		bool PointerOnGameplay;
		bool Drag = false;
		Vector3 BeginMousePos = Vector3.zero;

		public bool IsPointerOnGameplay()
		{
			return PointerOnGameplay;
		}

		public void PointerEnter()
		{
			PointerOnGameplay = true;
		}

		public void PointerExit()
		{
			PointerOnGameplay = false;
		}

		public void Click()
		{
			Debug.Log("Click");

		}

		public void DragInit()
		{
			BeginMousePos = Input.mousePosition;
		}

		public void DragBegin()
		{
			Drag = true;
			Debug.Log("Drag Begin");
			SelBox.gameObject.SetActive(true);
			UpdateSelectionBox();
		}

		public void DragUpdate()
		{
			// UpdateBox
			Debug.Log("Drag Update");
			UpdateSelectionBox();
		}

		public void DragEnd()
		{
			Drag = false;
			Debug.Log("Drag End");
			SelBox.gameObject.SetActive(false);
		}




		void UpdateSelectionBox()
		{
			Vector2 diference = Input.mousePosition - BeginMousePos;

			SelBox.sizeDelta = new Vector2(Mathf.Abs(diference.x), Mathf.Abs(diference.y));

			if (diference.x < 0 && diference.y < 0)
			{
				SelBox.anchoredPosition = Input.mousePosition;
			}
			else if (diference.x < 0)
			{
				SelBox.anchoredPosition = new Vector2(Input.mousePosition.x, BeginMousePos.y);
			}
			else if (diference.y < 0)
			{
				SelBox.anchoredPosition = new Vector2(BeginMousePos.x, Input.mousePosition.y);
			}
			else {
				SelBox.sizeDelta = diference;
				SelBox.anchoredPosition = BeginMousePos;
			}
		}


	}
}
