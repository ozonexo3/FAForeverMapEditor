using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ozone.UI
{
	public class UiToggle : MonoBehaviour
	{

		public Toggle Tog;
		public Text Label;
		System.Action OnValueChanged;

		bool Loading = false;
		public void Set(bool value)
		{
			Loading = true;
			Tog.isOn = value;
			Loading = false;
		}

		public void Set(bool value, string label)
		{
			Label.text = label;
			Set(value);
		}

		public void Set(bool value, string label, System.Action OnChanged)
		{
			OnValueChanged = OnChanged;
			Set(value, label);
		}

		public void OnToggleChanged()
		{
			if (Loading)
				return;

			if(OnValueChanged != null)
				OnValueChanged.Invoke();
		}
	}
}