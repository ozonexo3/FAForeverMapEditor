// ******************************************************************************
//
// * Creator allows for simple LUA file creation. Couldn't find good LUA>Unity parser so I do everything by hand
// * Unity parse in Json and has great build in tools for converting Class > Json > Class
// * Text files exported from editor (stratum, layers, settings etc.) are in Json
// * Copyright ozonexo3 2017
//
// ******************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace LuaParser
{
	public class Creator
	{

		StringBuilder Sb;
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
		}

		public string GetFileString()
		{
			return Sb.ToString();
		}

		public Creator()
		{
			Sb = new StringBuilder(2048);
		}
	}
}