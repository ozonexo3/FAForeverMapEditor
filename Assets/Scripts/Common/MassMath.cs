using UnityEngine;
using System.Collections;

public class MassMath : MonoBehaviour {

	public static float EasyInOut(float lerp){
		return 0;
	}

	public static float EasyIn(float lerp){
		return 0;
	}

	public static float EasyOut(float lerp){
		return 0;
	}


	public static float StringToFloat(string value)
	{
		float ToReturn = 0f;

		if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out ToReturn))
		{

		}

		return ToReturn;
	}
}
