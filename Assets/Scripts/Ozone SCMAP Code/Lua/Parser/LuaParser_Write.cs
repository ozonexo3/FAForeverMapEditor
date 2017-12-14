// ******************************************************************************
//
// * Lua Writer - Convert values to LUA text lines
// * Copyright ozonexo3 2017
//
// ******************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace LuaParser
{
	public struct Write
	{

		// Functions
		#region Functions

		public static string StringFunction(string value)
		{
			return "STRING( " + Coma + value + Coma + " )";
		}


		public static string VectorFunction(Vector3 value)
		{
			// TODO
			return "VECTOR3( " + Coma + value.ToString() + Coma + " )";
		}

		#endregion

		// Arrays
		#region Arrays
		public static string StringArrayToLua(string key, string[] value, bool NextLine = true)
		{
			string ToReturn = key + OpenBracketValue;
			for (int i = 0; i < value.Length; i++)
			{
				if (i > 0)
					ToReturn += NextValue + " ";
				ToReturn += Coma + value[i] + Coma;

			}
			ToReturn += EndBracket;
			if (NextLine)
				ToReturn += NextValue;
			return ToReturn;
		}

		public static string StringArrayToLua(string[] value, bool NextLine = true)
		{
			string ToReturn = OpenBracket;
			for (int i = 0; i < value.Length; i++)
			{
				if (i > 0)
					ToReturn += NextValue + " ";
				ToReturn += Coma + value[i] + Coma;

			}
			ToReturn += EndBracket;
			if (NextLine)
				ToReturn += NextValue;
			return ToReturn;
		}

		public static string FloatArrayToLua(string key, float[] value, bool NextLine = true)
		{
			string ToReturn = key + OpenBracketValue;
			for (int i = 0; i < value.Length; i++)
			{
				if (i > 0)
					ToReturn += NextValue + " ";
				ToReturn += value[i].ToString(FloatFormat);

			}
			ToReturn += EndBracket;
			if (NextLine)
				ToReturn += NextValue;
			return ToReturn;
		}

		public static string IntArrayToLua(string key, int[] value, bool NextLine = true)
		{
			string ToReturn = key + OpenBracketValue;
			for (int i = 0; i < value.Length; i++)
			{
				if (i > 0)
					ToReturn += NextValue + " ";
				ToReturn += value[i].ToString();

			}
			ToReturn += EndBracket;
			if (NextLine)
				ToReturn += NextValue;
			return ToReturn;
		}

		#endregion


		// Variables
		#region Variables
		public static string ValueToLua(string key, string value, bool NextLine = true)
		{
			return key + SetValue + value + GetNextValue(NextLine);
		}

		public static string StringToLua(string key, string value, bool NextLine = true)
		{
			return key + SetValue + Coma + value + Coma + GetNextValue(NextLine);
		}

		public static string DescriptionToLua(string key, string value, bool NextLine = true)
		{
			return key + SetValue + "\"" + value + "\"" + GetNextValue(NextLine);
		}

		public static string FloatToLua(string key, float value, bool NextLine = true)
		{
			return key + SetValue + value.ToString(FloatFormat) + GetNextValue(NextLine);
		}

		public static string IntToLua(string key, int value, bool NextLine = true)
		{
			return key + SetValue + value.ToString() + GetNextValue(NextLine);
		}

		public static string BoolToLua(string key, bool value, bool NextLine = true)
		{
			return key + SetValue + (value?("true"):("false")) + GetNextValue(NextLine);
		}

		public static string Vector3ToLua(string key, Vector3 value, bool NextLine = true)
		{
			return key + OpenBracketValue + " " + value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + " " + EndBracket + GetNextValue(NextLine);
		}
		#endregion

		// Functions
		#region Functions
		public static string RectangleToLua(string key, Rect value, bool NextLine = true)
		{
			return key + SetValue + "RECTANGLE( " + value.x.ToString() + ", " + value.y.ToString() + ", " + value.width.ToString() + ", " + value.height.ToString() + " )" + GetNextValue(NextLine);
		}

		public static string Vector3ToLuaFunction(string key, Vector3 value, bool NextLine = true)
		{
			return key + SetValue + "VECTOR3( " + value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + " )" + GetNextValue(NextLine);
		}

		public static string StringToLuaFunction(string key, string value, bool NextLine = true)
		{
			return key + SetValue + "STRING( " + Coma + value + Coma + " )" + GetNextValue(NextLine);
		}

		public static string FloatToLuaFunction(string key, float value, bool NextLine = true)
		{
			return key + SetValue + "FLOAT( " + value.ToString(FloatFunctionFormat) +" )" + GetNextValue(NextLine);
		}

		public static string BoolToLuaFunction(string key, bool value, bool NextLine = true)
		{
			return key + SetValue + "BOOLEAN( " + (value ? ("true") : ("false")) + " )" + GetNextValue(NextLine);
		}

		#endregion

		public static string PropertieToLua(string prop)
		{
			return "[" + Coma + prop + Coma + "]";
		}


		// Structure
		#region Structure
		public static void AddLine(string line, int tabs, ref string fileString)
		{
			fileString += ((fileString.Length > 0)?("\n"):("")) + GetTabs(tabs) + line;
		}

		static string GetNextValue(bool NextValueCheck)
		{
			return (NextValueCheck ? (NextValue) : (""));
		}

		public const string FloatFormat = "";
		public const string FloatFunctionFormat = "N6";
		public const string Coma = "'";
		public const string SetValue = " = ";
		public const string OpenBracket = "{";
		public const string OpenBracketValue = SetValue + OpenBracket;
		public const string EndBracket = "}";
		public const string NextValue = ",";
		public const string EndBracketNext = EndBracket + NextValue;

		public const string tab = "    ";
		static string GetTabs(int count)
		{
			string ToReturn = "";
			while(count > 0)
			{
				count--;
				ToReturn += tab;
			}
			return ToReturn;
		}
		#endregion

	}
}
