// ******************************************************************************
//
// * Lua Reader - Convert LUA strings into values
// * Copyright ozonexo3 2017
//
// ******************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace LuaParser
{
	public struct Read
	{
		// Table
		#region Table
		public static bool ValueExist(LuaTable Table, string key)
		{
			return Table.RawGet(key) != null && Table.RawGet(key).ToString() != "null";
		}

		public static string[] GetTableKeys(LuaTable Table)
		{
			if (Table == null)
				return new string[0];
			string[] ToReturn = new string[Table.Keys.Count];
			Table.Keys.CopyTo(ToReturn, 0);
			return ToReturn;
		}

		public static string[] GetTableValues(LuaTable Table)
		{
			if (Table == null)
				return new string[0];
			string[] ToReturn = new string[Table.Values.Count];
			Table.Values.CopyTo(ToReturn, 0);
			return ToReturn;
		}

		public static object[] GetTableObjects(LuaTable Table)
		{
			if (Table == null)
				return new object[0];
			object[] ToReturn = new object[Table.Values.Count];
			Table.Values.CopyTo(ToReturn, 0);
			return ToReturn;
		}

		public static LuaTable[] GetTableTables(LuaTable Table)
		{
			if (Table == null)
				return new LuaTable[0];
			LuaTable[] ToReturn = new LuaTable[Table.Values.Count];
			Table.Values.CopyTo(ToReturn, 0);
			return ToReturn;
		}
		#endregion


		// Variables
		#region Variables

		public static string StringFromTable(LuaTable Table, string key, string empty = "")
		{
			if (ValueExist(Table, key))
				return Table.RawGet(key).ToString();
			else
				return empty;
		}

		public static float FloatFromTable(LuaTable Table, string key, float empty = 0)
		{
			if (ValueExist(Table, key))
				return StringToFloat(Table.RawGet(key).ToString(), empty);
			else
				return empty;
		}

		public static int IntFromTable(LuaTable Table, string key, int empty = 0)
		{
			if (ValueExist(Table, key))
				return StringToInt(Table.RawGet(key).ToString(), empty);
			else
				return empty;
		}

		public static bool BoolFromTable(LuaTable Table, string key, bool empty = false)
		{
			if (ValueExist(Table, key))
				return StringToBool(Table.RawGet(key).ToString(), empty);
			else
				return empty;
		}

		public static Rect RectFromTable(LuaTable Table, string key)
		{

			if (ValueExist(Table, key))
			{
				LuaTable RectTable = (LuaTable)Table.RawGet(key);
				object[] RectValues = GetTableObjects(RectTable);
				return new Rect(
					StringToFloat(RectValues[0].ToString(), 0),
					StringToFloat(RectValues[1].ToString(), 0),
					StringToFloat(RectValues[2].ToString(), 0),
					StringToFloat(RectValues[3].ToString(), 0)
					);

			}
			else
				return new Rect(0, 0, 0, 0);
		}


		public static Vector3 Vector3FromTable(LuaTable Table, string key)
		{
			if (ValueExist(Table, key))
			{
				LuaTable RectTable = (LuaTable)Table.RawGet(key);
				object[] RectValues = GetTableObjects(RectTable);
				return new Vector3(
					StringToFloat(RectValues[0].ToString(), 0),
					StringToFloat(RectValues[1].ToString(), 0),
					StringToFloat(RectValues[2].ToString(), 0)
					);

			}
			else
				return Vector3.zero;
		}
		#endregion


		// Arrays
		#region Arrays

		public static LuaTable[] TableArrayFromTable(LuaTable Table)
		{
			if (Table == null)
				return new LuaTable[0];

			LuaTable[] ToReturn = new LuaTable[Table.Values.Count];

			for (int i = 0; i < ToReturn.Length; i++)
				ToReturn[i] = (LuaTable)Table[i + 1];

			return ToReturn;
		}

		public static string[] StringArrayFromTable(LuaTable Table, string key)
		{
			return StringArrayFromTable((LuaTable)Table.RawGet(key));
		}

		public static string[] StringArrayFromTable(LuaTable Table)
		{
			string[] ToReturn = new string[Table.Values.Count];

			for (int i = 0; i < ToReturn.Length; i++)
				ToReturn[i] = Table[i + 1].ToString();

			return ToReturn;
		}

		public static float[] FloatArrayFromTable(LuaTable Table, float empty = 0)
		{
			float[] ToReturn = new float[Table.Values.Count];

			for(int i = 0; i < ToReturn.Length; i++)
				ToReturn[i] = StringToFloat(Table[i + 1].ToString(), empty);

			return ToReturn;

		}

		public static int[] IntArrayFromTable(LuaTable Table, int empty = 0)
		{
			int[] ToReturn = new int[Table.Values.Count];

			for (int i = 0; i < ToReturn.Length; i++)
				ToReturn[i] = StringToInt(Table[i + 1].ToString(), empty);

			return ToReturn;

		}
		#endregion



		#region Parse
		public static float StringToFloat(string value, float empty = 0f)
		{
			if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out empty))
			{
				// Success
			}
			return empty;
		}

		public static int StringToInt(string value, int empty = 0)
		{
			if (int.TryParse(value, out empty))
			{
				// Success
			}
			return empty;
		}

		public static bool StringToBool(string value, bool empty = false)
		{
			if (string.IsNullOrEmpty(value))
				return empty;
			return value.ToLower() == "true";
		}

		#endregion



		public static string FormatException(NLua.Exceptions.LuaException e)
		{
			string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
			return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
		}

		public static string GetStructureText(string name)
		{
			return System.IO.File.ReadAllText(MapLuaParser.StructurePath + name, System.Text.Encoding.ASCII);
		}
	}
}
