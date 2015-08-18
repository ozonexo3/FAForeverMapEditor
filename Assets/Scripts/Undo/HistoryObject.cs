using UnityEngine;
using System.Collections;

namespace UndoHistory{
	public class HistoryObject : MonoBehaviour {

		public		string		UndoCommandName;
		public		UndoTypes	UndoType;	


		public enum UndoTypes{
			MapInfo, MarkersMove, MarkersSelection, MarkersCreate, MarkersDelete
		}


	}
}