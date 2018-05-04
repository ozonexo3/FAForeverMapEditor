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
		System.Action<int, int> OnValueChanged;
		public GameObject MultipleValues;

		bool Loading = false;
		public void Set(bool value)
		{
			Loading = true;
			Tog.isOn = value;
			if(MultipleValues)
			MultipleValues.SetActive(false);
			Loading = false;
		}

		public void Set(bool value, string label)
		{
			Label.text = label;
			Set(value);
			if (MultipleValues)
				MultipleValues.SetActive(false);
		}

		int InstanceId;
		int GroupId;
		public void Set(bool value, string label, System.Action<int, int> OnChanged, int GroupId = 0, int InstanceId = 0)
		{
			this.InstanceId = InstanceId;
			this.GroupId = GroupId;
			OnValueChanged = OnChanged;
			Set(value, label);
			if (MultipleValues)
				MultipleValues.SetActive(false);
		}

		public void OnToggleChanged()
		{
			if (Loading)
				return;

			if (MultipleValues)
				MultipleValues.SetActive(false);

			if (OnValueChanged != null)
				OnValueChanged.Invoke(GroupId, InstanceId);
		}


		#region Multiple values testing
		public bool HasOnValue = false;
		public bool HasOffValue = false;

		public void ResetTesting()
		{
			HasOnValue = false;
			HasOffValue = false;
		}

		public void ApplyTesting()
		{
			if(HasOnValue && HasOffValue)
			{
				Set(false);
				MultipleValues.SetActive(true);
			}
			else if (HasOnValue)
			{
				Set(true);
			}
			else if (HasOffValue)
			{
				Set(false);
			}
			else
			{
				Set(false);
			}
		}
		#endregion
	}
}