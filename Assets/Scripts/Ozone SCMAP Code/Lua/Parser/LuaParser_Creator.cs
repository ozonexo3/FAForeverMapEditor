using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuaParser
{
	public class Creator : MonoBehaviour
	{

		string FileString;

		int tabs = 0;

		public void ForceTab(int count)
		{
			tabs = count;
		}

		public void OpenTab()
		{
			tabs++;
		}

		public void CloseTab()
		{
			tabs--;
		}

		public void AddLine(string line)
		{
			LuaParser.Write.AddLine(line, tabs, ref FileString);
		}


		const int MinimumComentCharacters = 75;
		const string CommentSaveBegin = "--[[";
		const string CommentSaveEnd = "]]--";
		public void AddSaveComent(string coment)
		{
			coment = "  " + coment;

			while (coment.Length < MinimumComentCharacters)
				coment += " ";

			LuaParser.Write.AddLine(CommentSaveBegin + coment + CommentSaveEnd, tabs, ref FileString);
		}

		public string GetFileString()
		{
			return FileString;

		}

		public Creator()
		{
			FileString = "";
			tabs = 0;
		}
	}
}