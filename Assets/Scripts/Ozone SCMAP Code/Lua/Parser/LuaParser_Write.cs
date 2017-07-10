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
		#endregion


		public static string PropertiveToLua(string prop)
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
		public const string Coma = "'";
		public const string SetValue = " = ";
		public const string OpenBracket = "{";
		public const string OpenBracketValue = SetValue + OpenBracket;
		public const string EndBracket = "}";
		public const string NextValue = ",";
		public const string EndBracketNext = EndBracket + NextValue;

		const string tab = "    ";
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
