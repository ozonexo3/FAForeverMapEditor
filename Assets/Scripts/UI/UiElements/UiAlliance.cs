using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Ozone.UI
{
	public class UiAlliance : MonoBehaviour
	{
		public Text Label;
		public Toggle[] Toggles;
		System.Action<int, MapLua.SaveLua.Army.AllianceTypes> OnValueChanged;

		bool Loading = false;
		public void Set(MapLua.SaveLua.Army.AllianceTypes Value)
		{
			Loading = true;

			Toggles[(int)Value].isOn = true;

			Loading = false;
		}

		int ArmyId = 0;
		public void Set(MapLua.SaveLua.Army.AllianceTypes Value, string label, System.Action<int, MapLua.SaveLua.Army.AllianceTypes> ChangeAction, int ArmyId)
		{
			this.ArmyId = ArmyId;
			Label.text = label;
			Set(Value);
			OnValueChanged = ChangeAction;
		}

		public void ToggleValueChanged(int ID)
		{
			if (Loading)
				return;


			if(Toggles[ID].isOn)
				OnValueChanged(ArmyId, value);
		}

		public MapLua.SaveLua.Army.AllianceTypes value
		{
			get
			{
				for(int i = 0; i < Toggles.Length; i++)
				{
					if (Toggles[i].isOn)
						return (MapLua.SaveLua.Army.AllianceTypes)i;

				}

				return MapLua.SaveLua.Army.AllianceTypes.None;
			}
		}
	}
}