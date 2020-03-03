using UnityEngine;
using System.Collections;

namespace UndoHistory{
	[System.Serializable]
	public class HistoryObject {

		public		string		UndoCommandName;

		public bool RedoGenerated = false;

		public virtual void Register(HistoryParameter Params)
		{
			UndoCommandName = "History step";
		}

		public virtual void DoUndo(){

		}

		public virtual void DoRedo(){

		}

		public virtual HistoryObject GetNew()
		{
			return new HistoryObject();
		}

		public abstract class HistoryParameter
		{

			protected HistoryParameter()
			{
				
			}
		}

	}
}