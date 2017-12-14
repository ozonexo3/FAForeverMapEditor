using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UiColor : MonoBehaviour
{
	public float Clamp = 2;
	public Image ColorPreview;

	public InputField Red;
	public InputField Green;
	public InputField Blue;

	public Slider RedSlider;
	public Slider GreenSlider;
	public Slider BlueSlider;

	public UnityEvent OnInputFinish;
	public UnityEvent OnValueChanged;

	//System.Action FieldChangedAction;
	bool Loading = false;

	void ClampValues()
	{
		RedSlider.maxValue = Clamp;
		GreenSlider.maxValue = Clamp;
		BlueSlider.maxValue = Clamp;
	}

	public Color GetColorValue()
	{
		return new Color(RedSlider.value, GreenSlider.value, BlueSlider.value, 1);
	}

	public Vector3 GetVectorValue()
	{
		return new Vector3(RedSlider.value, GreenSlider.value, BlueSlider.value);
	}

	public void SetColorField(float R, float G, float B)
	{
		ClampValues();
		Loading = true;
		//if(FieldChangedAction == null)
		//	FieldChangedAction = ChangeAction;

		RedSlider.value = R;
		GreenSlider.value = G;
		BlueSlider.value = B;

		Red.text = RedSlider.value.ToString();
		Green.text = GreenSlider.value.ToString();
		Blue.text = BlueSlider.value.ToString();

		UpdateGfx();

		Loading = false;
	}


	public void SetColorField(Color BeginColor)
	{
		ClampValues();
		Loading = true;
		//FieldChangedAction = ChangeAction;

		RedSlider.value = BeginColor.r;
		GreenSlider.value = BeginColor.g;
		BlueSlider.value = BeginColor.b;

		Red.text = RedSlider.value.ToString();
		Green.text = GreenSlider.value.ToString();
		Blue.text = BlueSlider.value.ToString();

		UpdateGfx();

		Loading = false;
	}


	public void InputFieldUpdate()
	{
		if (Loading)
			return;

		Loading = true;

		RedSlider.value = Mathf.Clamp(LuaParser.Read.StringToFloat(Red.text), 0, Clamp);
		GreenSlider.value = Mathf.Clamp(LuaParser.Read.StringToFloat(Green.text), 0, Clamp);
		BlueSlider.value = Mathf.Clamp(LuaParser.Read.StringToFloat(Blue.text), 0, Clamp);

		Red.text = RedSlider.value.ToString();
		Green.text = GreenSlider.value.ToString();
		Blue.text = BlueSlider.value.ToString();

		Loading = false;

		UpdateGfx();
		//FieldChangedAction();
		OnInputFinish.Invoke();
	}

	public void SliderUpdate(bool Finish)
	{
		if (Loading)
			return;

		Red.text = RedSlider.value.ToString();
		Green.text = GreenSlider.value.ToString();
		Blue.text = BlueSlider.value.ToString();

		UpdateGfx();
		//FieldChangedAction();
		if(Finish)
			OnInputFinish.Invoke();
		else
			OnValueChanged.Invoke();
	}

	void UpdateGfx()
	{
		ColorPreview.color = new Color(RedSlider.value / Clamp, GreenSlider.value / Clamp, BlueSlider.value / Clamp, 1);

	}
}
