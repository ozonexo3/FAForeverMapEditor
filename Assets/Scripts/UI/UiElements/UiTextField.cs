using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Ozone.UI
{
	public class UiTextField : MonoBehaviour
	{
		[Header("Config")]
		public FieldTypes FieldType;
		public float BeginValue;

		[Header("UI")]
		public InputField	InputFieldUi;
		public Slider		SliderUi;

		[Header("Events")]
		public UnityEvent OnValueChanged;
		public UnityEvent OnEndEdit;

		

		public enum FieldTypes
		{
			Text, Float, Int
		}

		private void OnEnable()
		{
			if (FieldType == FieldTypes.Float)
				SetValue(BeginValue);
			else if (FieldType == FieldTypes.Int)
				SetValue((int)BeginValue);
		}

		#region Events
		public void OnValueChangedInvoke()
		{
			if (ChangingValue)
				return;
			UpdateSliderValue();
			OnValueChanged.Invoke();
		}

		public void OnEndEditInvoke()
		{
			if (ChangingValue)
				return;
			UpdateSliderValue();
			OnEndEdit.Invoke();
		}

		public void OnSliderChanged()
		{
			if (ChangingValue)
				return;

			ChangingValue = true;
			LastValue = SliderUi.value;
			InputFieldUi.text = LastValue.ToString();
			ChangingValue = false;

			OnValueChangedInvoke();
		}
#endregion


		void UpdateSliderValue()
		{
			ChangingValue = true;
			if (SliderUi)
			{
				if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				{
					LastValue = int.Parse(InputFieldUi.text);
					LastValue = Mathf.Clamp(LastValue, SliderUi.minValue, SliderUi.maxValue);
					InputFieldUi.text = LastValue.ToString();
				}
				else if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				{
					LastValue = float.Parse(InputFieldUi.text);
					LastValue = Mathf.Clamp(LastValue, SliderUi.minValue, SliderUi.maxValue);
					InputFieldUi.text = LastValue.ToString();
				}

				SliderUi.value = LastValue;
			}
			ChangingValue = false;
		}

		#region Setters
		bool ChangingValue = false;
		public void SetValue(string value, bool AllowInvoke = false)
		{
			ChangingValue = !AllowInvoke;
			InputFieldUi.text = value;
			ChangingValue = false;
		}

		public void SetValue(float value, bool AllowInvoke = false)
		{
			ChangingValue = !AllowInvoke;
			LastValue = value;
			if (SliderUi)
				SliderUi.value = LastValue;
			InputFieldUi.text = LastValue.ToString();
			ChangingValue = false;
		}

		public void SetValue(int value, bool AllowInvoke = false)
		{
			ChangingValue = !AllowInvoke;
			LastValue = value;
			if (SliderUi)
				SliderUi.value = LastValue;
			InputFieldUi.text = LastValue.ToString();
			ChangingValue = false;
		}
		#endregion

#region Getters
		public string text
		{
			get
			{
				return InputFieldUi.text;
			}
		}

		float LastValue = 0;

		public float value
		{
			get
			{
				return LastValue;
			}
		}

		public int intValue
		{
			get
			{
				return (int)value;
			}
		}
#endregion
	}
}
