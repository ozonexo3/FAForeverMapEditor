using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiScaler : MonoBehaviour
{

	public static UiScaler instance { get; private set; }
	public CanvasScaler canvasScaler;

	private void Awake()
	{
		instance = this;
		UpdateUiScale();
	}

	public static void UpdateUiScale()
	{
		float value = Mathf.Clamp(FafEditorSettings.GetUiScale(), 0.25f, 4f);
		instance.canvasScaler.scaleFactor = value;
		instance.canvasScaler.referencePixelsPerUnit = 100f / value;
	}

	public static void TempChangeUiScale(float value)
	{
		value = Mathf.Clamp(value, 0.25f, 4f);

		instance.canvasScaler.scaleFactor = value;
		instance.canvasScaler.referencePixelsPerUnit = 100f / value;
	}
}
