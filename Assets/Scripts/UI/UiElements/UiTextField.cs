using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		[SerializeField] private string FiltredChars;
		//public string Format = "N2";

		[Header("UI")]
		public InputField	InputFieldUi;
		public Slider		SliderUi;

		[Header("Events")]
		public UnityEvent OnBeginChange;
		public UnityEvent OnValueChanged;
		public UnityEvent OnEndEdit;

		
		private string[] _filtredCharsArr;
		private string[] FiltredCharsArr
		{
			get
			{
				if (_filtredCharsArr == null || _filtredCharsArr.Length == 0)
				{
					_filtredCharsArr = FiltredChars.ToCharArray().Select(c => c.ToString()).ToArray();
				}

				return _filtredCharsArr;
			}
		}

		public enum FieldTypes
		{
			Text, Float, Int
		}

		bool HasValue = false;

		private void Awake()
		{
			InputFieldUi.onValueChanged.AddListener(CharFilter);
		}

		private void Start()
		{
			if (HasValue)
				return;
			if (FieldType == FieldTypes.Float)
				SetValue(BeginValue);
			else if (FieldType == FieldTypes.Int)
				SetValue((int)BeginValue);
		}

		private void OnDestroy()
		{
			InputFieldUi.onValueChanged.RemoveListener(CharFilter);
		}

		#region Events
		bool Started = false;
		void InvokeStart()
		{
			if (Started)
				return;

			//Debug.Log("Begin change");
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

		bool SliderStarted = false;
		public void OnSliderChanged()
		{
			if (ChangingValue)
				return;


			InvokeStart();
			SliderStarted = true;
			ChangingValue = true;
			HasValue = true;
			LastValue = SliderUi.value;
			SetTextField();
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				SliderUi.value = int.Parse(InputFieldUi.text);
			if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				LastValue = LuaParser.Read.StringToFloat(InputFieldUi.text);

			//SliderUi.value = LastValue;

			ChangingValue = false;

			OnValueChangedInvoke();
		}

		public void OnSliderFinished()
		{
			if (!SliderStarted)
				return;

			SliderStarted = false;
			ChangingValue = true;
			HasValue = true;
			LastValue = SliderUi.value;
			SetTextField();
			if (InputFieldUi.contentType == InputField.ContentType.IntegerNumber)
				SliderUi.value = int.Parse(InputFieldUi.text);
			if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
				LastValue = LuaParser.Read.StringToFloat(InputFieldUi.text);

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
			{
				LastValue = LuaParser.Read.StringToInt(InputFieldUi.text, (int)LastValue);
			}
			else if (InputFieldUi.contentType == InputField.ContentType.DecimalNumber)
			{
				LastValue = LuaParser.Read.StringToFloat(InputFieldUi.text, LastValue);
			}

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
				InputFieldUi.text = LastValue.ToString("F2").Replace(",", ".");

		}

		public void CharFilter(string value)
		{
			var tmpstr = value;
			foreach (var c in FiltredCharsArr)
			{
				tmpstr = tmpstr.Replace(c, "");
			}

			SetValue(tmpstr);
//			InputFieldUi.text = tmpstr;
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
