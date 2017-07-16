using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace LuaParser
{
	public class Creator
	{

		StringBuilder Sb;
		//string FileString;

		//int tabs = 0;
		string TabsString = "";

		public void OpenTab()
		{
			TabsString += LuaParser.Write.tab;
		}

		public void CloseTab()
		{
			TabsString = TabsString.Remove(TabsString.Length - LuaParser.Write.tab.Length);
		}

		public void OpenTab(string line)
		{
			AddLine(line);
			OpenTab();
		}

		public void CloseTab(string line)
		{
			CloseTab();
			AddLine(line);
		}

		public void AddLine(string line)
		{
			Sb.AppendLine(TabsString + line);
			//LuaParser.Write.AddLine(line, tabs, ref FileString);
		}


		const int MinimumComentCharacters = 75;
		const string CommentSaveBegin = "--[[";
		const string CommentSaveEnd = "]]--";
		public void AddSaveComent(string coment)
		{
			coment = "  " + coment;

			while (coment.Length < MinimumComentCharacters)
				coment += " ";

			Sb.AppendLine(TabsString + CommentSaveBegin + coment + CommentSaveEnd);
			//LuaParser.Write.AddLine(CommentSaveBegin + coment + CommentSaveEnd, tabs, ref FileString);
		}

		public string GetFileString()
		{
			return Sb.ToString();
		}

		public Creator()
		{
			Sb = new StringBuilder(2048);
			//FileString = "";
			//tabs = 0;
		}
	}
}