using UnityEngine;

public class KeyboardManager {

	//public static KeyCode BrushStrength = KeyCode.V;
	//public static KeyCode BrushSize = KeyCode.B;

	public static bool BrushStrengthDown(){
		return Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.S);
	}

	public static bool BrushStrengthHold()
	{
		return Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.S);
	}

	public static bool BrushSizeDown()
	{
		return Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.W);
	}

	public static bool BrushSizeHold()
	{
		return Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.W);
	}

	public static bool SwitchTypeNext()
	{
		return Input.GetKeyDown(KeyCode.Tab);
	}

	public static bool SwitchType1()
	{
		return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
	}

	public static bool SwitchType2()
	{
		return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
	}

	public static bool SwitchType3()
	{
		return Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
	}

	public static bool SwitchType4()
	{
		return Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4);
	}

	const float ClickDownOffset = 0.5f;
	const float ClickOffset = 0.1f;

	static float LastClickTime = 0;
	public static bool IncreaseTarget()
	{
		if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			LastClickTime = Time.realtimeSinceStartup + ClickDownOffset;
			return true;
		}
		else if(Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
		{
			if(Time.realtimeSinceStartup > LastClickTime)
			{
				LastClickTime = Time.realtimeSinceStartup + ClickOffset;
				return true;
			}
		}
		return false;
	}

	public static bool DecreaseTarget()
	{
		if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			LastClickTime = Time.realtimeSinceStartup + ClickDownOffset;
			return true;
		}
		else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
		{
			if (Time.realtimeSinceStartup > LastClickTime)
			{
				LastClickTime = Time.realtimeSinceStartup + ClickOffset;
				return true;
			}
		}
		return false;
	}
}
