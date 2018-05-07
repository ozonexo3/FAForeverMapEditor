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

		public void OpenTab(int tabs = 1)
		{
			if(tabs <= 1)
				TabsString += LuaParser.Write.tab;
			else
				for (int i = 0; i < tabs; i++)
					TabsString += LuaParser.Write.tab;
		}

		public void CloseTab(int tabs = 1)
		{
			if (tabs <= 1)
				TabsString = TabsString.Remove(TabsString.Length - LuaParser.Write.tab.Length);
			else
				for (int i = 0; i < tabs; i++)
					TabsString = TabsString.Remove(TabsString.Length - LuaParser.Write.tab.Length);
		}

		public void OpenTab(string line, int tabs = 1)
		{
			AddLine(line);
			OpenTab(tabs);
		}

		public void CloseTab(string line, int tabs = 1)
		{
			CloseTab(tabs);
			AddLine(line);
		}

		public void AddLine(string line)
		{
			Sb.AppendLine(TabsString + line);
		}
		public void NextLine(int Count = 0)
		{
			if(Count <= 1)
				Sb.AppendLine(TabsString);
			else
			{
				for(int i = 0; i < Count; i++)
					Sb.AppendLine(TabsString);
			}
		}

		const int MinimumComentCharacters = 75;
		const string Comment = "--";
		const string CommentSaveBegin = "--[[";
		const string CommentSaveEnd = "]]--";
		public void AddSaveComent(string coment)
		{
			coment = "  " + coment;

			while (coment.Length < MinimumComentCharacters)
				coment += " ";

			Sb.AppendLine(TabsString + CommentSaveBegin + coment + CommentSaveEnd);
		}

		public void AddComent(string coment)
		{
			Sb.AppendLine(TabsString + Comment + " " + coment);
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