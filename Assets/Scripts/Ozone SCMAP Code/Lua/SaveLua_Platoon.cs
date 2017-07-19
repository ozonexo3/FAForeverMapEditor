using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	public partial class SaveLua
	{

		[System.Serializable]
		public class Platoon
		{
			public string Name;
			public string PlatoonName;
			public string PlatoonFunction;
			public PlatoonAction Action;

			[System.Serializable]
			public class PlatoonAction
			{
				public bool Loaded = false;
				public string Unit;
				public int Id;
				public int count;
				public string Action;
				public string Formation;

				public PlatoonAction()
				{
					Loaded = false;
					Unit = "";
					Id = 0;
					count = 0;
					Action = "";
					Formation = "";
				}

				public PlatoonAction(LuaTable Table)
				{
					Loaded = true;
					object[] Objects = LuaParser.Read.GetTableObjects(Table);
					Unit = Objects[0].ToString();
					Id = LuaParser.Read.StringToInt(Objects[1].ToString());
					count = LuaParser.Read.StringToInt(Objects[2].ToString());
					Action = Objects[3].ToString();
					Formation = Objects[4].ToString();
				}
			}

			public Platoon()
			{
			}

			public Platoon(string name, LuaTable Table)
			{
				Name = name;

				object[] Objects = LuaParser.Read.GetTableObjects(Table);
				PlatoonName = Objects[0].ToString();
				PlatoonFunction = Objects[1].ToString();
				//if (Objects.Length > 2)
				//	Action = new PlatoonAction((LuaTable)Objects[2]);
				//else
					Action = new PlatoonAction();

			}
		}

	}
}
