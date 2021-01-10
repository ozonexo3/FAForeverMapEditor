using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Multiline]
	public string text;

	public void OnPointerEnter(PointerEventData eventData)
	{
		TooltipWindow.Show(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TooltipWindow.Hide(this);
	}
}
