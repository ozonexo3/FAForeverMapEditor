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
		public string Format = "N2";

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

		bool HasValue = false;

		private void Start()
		{
			if (HasValue)
				return;
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
			UpdateSliderValue(true);
			OnEndEdit.Invoke();
		}

		public void OnSliderChanged()
		{
			if (ChangingValue)
				return;

			ChangingValue = true;
			HasValue = true;
			LastValue = SliderUi.value;
			InputFieldUi.text = LastValue.ToString(Format);
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				SliderUi.value = int.Parse(InputFieldUi.text);
			if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				LastValue = float.Parse(InputFieldUi.text);

			//SliderUi.value = LastValue;

			ChangingValue = false;

			OnValueChangedInvoke();
		}
		#endregion


		void UpdateSliderValue(bool ClampText = false)
		{
			ChangingValue = true;
			HasValue = true;
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				LastValue = int.Parse(InputFieldUi.text);
			else if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				LastValue = float.Parse(InputFieldUi.text);

			if (SliderUi)
			{
				LastValue = Mathf.Clamp(LastValue, SliderUi.minValue, SliderUi.maxValue);

				if (ClampText)
				{
					if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
						InputFieldUi.text = LastValue.ToString();
					else if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
						InputFieldUi.text = LastValue.ToString(Format);
				}

				SliderUi.value = LastValue;

			}

			ChangingValue = false;
		}

		#region Setters
		bool ChangingValue = false;
		public void SetValue(string value, bool AllowInvoke = false)
		{
			HasValue = true;
			ChangingValue = !AllowInvoke;
			InputFieldUi.text = value;
			ChangingValue = false;
		}

		public void SetValue(float value, bool AllowInvoke = false)
		{
			HasValue = true;
			ChangingValue = !AllowInvoke;
			LastValue = value;
			if (SliderUi)
			{
				LastValue = Mathf.Clamp(LastValue, SliderUi.minValue, SliderUi.maxValue);
				SliderUi.value = LastValue;
			}
			InputFieldUi.text = LastValue.ToString(Format);
			ChangingValue = false;
		}

		public void SetValue(int value, bool AllowInvoke = false)
		{
			HasValue = true;
			ChangingValue = !AllowInvoke;
			LastValue = value;
			if (SliderUi)
			{
				LastValue = Mathf.Clamp(LastValue, SliderUi.minValue, SliderUi.maxValue);
				SliderUi.value = LastValue;
			}
			InputFieldUi.text = LastValue.ToString(Format);
			ChangingValue = false;
		}
		#endregion

#region Getters
		public string text
		{
			get
			{
				if (!HasValue)
				{

				}
				return InputFieldUi.text;
			}
		}

		float LastValue = 0;

		public float value
		{
			get
			{
				if (!HasValue)
				{
					LastValue = BeginValue;
				}
				return LastValue;
			}
		}

		public int intValue
		{
			get
			{
				if (!HasValue)
				{
					LastValue = (int)BeginValue;
				}
				return (int)LastValue;
			}
		}
#endregion
	}
}
