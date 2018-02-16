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
		//public string Format = "N2";

		[Header("UI")]
		public InputField	InputFieldUi;
		public Slider		SliderUi;

		[Header("Events")]
		public UnityEvent OnBeginChange;
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
		bool Started = false;
		void InvokeStart()
		{
			if (Started)
				return;

			Debug.Log("Begin change");
			Started = true;
			OnBeginChange.Invoke();
		}

		public void OnValueChangedInvoke()
		{
			if (ChangingValue)
				return;

			InvokeStart();
			UpdateSliderValue();
			OnValueChanged.Invoke();
		}

		public void OnEndEditInvoke()
		{
			if (ChangingValue)
				return;
			UpdateSliderValue(true);
			OnEndEdit.Invoke();
			SetTextField();
			Started = false;
		}

		public void OnSliderChanged()
		{
			if (ChangingValue)
				return;

			InvokeStart();

			ChangingValue = true;
			HasValue = true;
			LastValue = SliderUi.value;
			SetTextField();
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				SliderUi.value = int.Parse(InputFieldUi.text);
			if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				LastValue = float.Parse(InputFieldUi.text);

			//SliderUi.value = LastValue;

			ChangingValue = false;

			OnValueChangedInvoke();
		}

		public void OnSliderFinished()
		{
			ChangingValue = true;
			HasValue = true;
			LastValue = SliderUi.value;
			SetTextField();
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				SliderUi.value = int.Parse(InputFieldUi.text);
			if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				LastValue = float.Parse(InputFieldUi.text);

			//SliderUi.value = LastValue;

			ChangingValue = false;

			OnEndEditInvoke();
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
					SetTextField();
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

			SetTextField();
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
			SetTextField();
			ChangingValue = false;
		}

		void SetTextField()
		{
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				InputFieldUi.text = LastValue.ToString();
			else if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				InputFieldUi.text = LastValue.ToString("F2");
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
