using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AppMenu : MonoBehaviour
{
	/// <summary>
	/// Check if file name ends with version controll string
	/// </summary>
	/// <param name="name">Folder name</param>
	/// <returns></returns>
	public static bool HasVersionControll(string name)
	{
		if (name.Length < 7)
			return false;

		string End = name.Remove(0, name.Length - 6);

		if (!End.StartsWith(".v"))
			return false;

		int ParsedValue = 0;
		if (int.TryParse(End.Remove(0, 2), out ParsedValue))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Find version controll value
	/// </summary>
	/// <param name="name">Folder name</param>
	/// <returns></returns>
	public static int GetVersionControll(string name)
	{
		if (name.Length < 7)
			return -1;

		string End = name.Remove(0, name.Length - 6);

		if (!End.StartsWith(".v"))
			return -1;

		int ParsedValue = 0;
		if (int.TryParse(End.Remove(0, 2), out ParsedValue))
		{
			return ParsedValue;
		}
		return -1;
	}

	/// <summary>
	/// Remove version controll from the end
	/// </summary>
	/// <param name="name">Folder name</param>
	/// <returns></returns>
	public static string RemoveVersionControll(string name)
	{
		return name.Remove(name.Length - 6, 6);
	}


	/// <summary>
	/// Ger name without version controll ending
	/// </summary>
	/// <param name="name">Folder name</param>
	/// <returns></returns>
	public static string NonVersionControlledName(string name)
	{
		string toReturn = name.Replace(" ", "_");
		while (HasVersionControll(toReturn))
		{
			toReturn = RemoveVersionControll(toReturn);
		}
		return toReturn;
	}

	/// <summary>
	/// Add or replace version controll with value
	/// </summary>
	/// <param name="name">Folder name</param>
	/// <param name="value">Version number</param>
	/// <returns></returns>
	public static string ForceVersionControllValue(string name, int value)
	{
		name = NonVersionControlledName(name);
		return name + ".v" + value.ToString("0000");

	}
}
