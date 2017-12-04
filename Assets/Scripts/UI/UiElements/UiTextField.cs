using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Ozone.UI
{
	public class UiTextField : MonoBehaviour
	{

		[Header("UI")]
		public InputField	InputFieldUi;
		public Slider		SliderUi;

		[Header("Events")]
		public UnityEvent OnValueChanged;
		public UnityEvent OnEndEdit;

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

		void UpdateSliderValue()
		{
			if (SliderUi)
			{
				if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
					SliderUi.value = int.Parse(InputFieldUi.text);
			}
		}

		public void OnSliderChanged()
		{
			if (ChangingValue)
				return;

			ChangingValue = true;
			InputFieldUi.text = SliderUi.value.ToString();
			ChangingValue = false;

			OnValueChangedInvoke();
		}

		bool ChangingValue = false;
		public void SetValue(string value, bool AllowInvoke = false)
		{
			ChangingValue = !AllowInvoke;
			InputFieldUi.text = value;
			ChangingValue = false;
		}

		public string text
		{
			get
			{
				return InputFieldUi.text;
			}
		}
	}
}
