using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ozone.UI
{
	public class UiVector : MonoBehaviour
	{
		public InputField X;
		public InputField Y;
		public InputField Z;
		public InputField W;

		public bool Normalized = false;

		public UnityEvent OnInputBegin;
		public UnityEvent OnInputFinish;
		public UnityEvent OnValueChanged;

		//System.Action FieldChangedAction;
		bool Loading = false;

		Vector4 LastValue = Vector4.one;
		void UpdateLastValue()
		{
			LastValue.x = FormatFloat(LuaParser.Read.StringToFloat(X.text));
			LastValue.y = FormatFloat(LuaParser.Read.StringToFloat(Y.text));
			if(Z)
				LastValue.z = FormatFloat(LuaParser.Read.StringToFloat(Z.text));
			if(W)
				LastValue.w = FormatFloat(LuaParser.Read.StringToFloat(W.text));
		}

		public Color GetColorValue()
		{
			return new Color(LastValue.x, LastValue.y, LastValue.z, LastValue.w);
		}

		public Vector2 GetVector2Value()
		{
			if (Normalized)
			{
				return LastValue.normalized;
			}
			return LastValue;
		}

		public Vector3 GetVector3Value()
		{
			if (Normalized)
			{
				return LastValue.normalized;
			}
			return LastValue;
		}

		public Vector4 GetVector4Value()
		{
			if (Normalized)
			{
				return LastValue.normalized;
			}
			return LastValue;
		}

		public void SetVectorField(float x, float y, float z = 1f, float w = 1f, bool normalized = false)
		{
			Loading = true;

			X.text = x.ToString();
			Y.text = y.ToString();
			if (Z)
				Z.text = z.ToString();
			if (W)
				W.text = w.ToString();

			Normalized = normalized;

			UpdateLastValue();

			Loading = false;
		}


		public void SetVectorField(Vector4 value, bool normalized = false)
		{
			Loading = true;
			//FieldChangedAction = ChangeAction;

			X.text = value.x.ToString();
			Y.text = value.y.ToString();
			if(Z)
			Z.text = value.z.ToString();
			if(W)
			W.text = value.w.ToString();

			Normalized = normalized;

			LastValue = value;

			UpdateLastValue();

			Loading = false;
		}

		const float FloatSteps = 10000;
		float FormatFloat(float value)
		{
			return Mathf.RoundToInt(value * FloatSteps) / FloatSteps;
		}


		public void InputFieldUpdate()
		{
			if (Loading)
				return;

			Loading = true;

			UpdateLastValue();

			Loading = false;

			//Begin = false;
			OnInputFinish.Invoke();
		}


		//bool UpdatingSlider = false;
		//bool Begin = false;

	}
}