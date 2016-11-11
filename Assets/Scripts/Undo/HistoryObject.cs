using UnityEngine;
using System.Collections;

namespace UndoHistory{
	public class HistoryObject : MonoBehaviour {

		public		string		UndoCommandName;


		public enum UndoTypes{
			MapInfo, MarkersMove, MarkersSelection, MarkersChange, TerrainHeight
		}


		/// <summary>
		/// Generates Undo
		/// </summary>
		/// <param name="Prefab">Prefab of undo type</param>
		public static HistoryObject GenerateUndo(GameObject Prefab){
			GameObject NewHistoryStep = Instantiate (Prefab) as GameObject;
			NewHistoryStep.name = Prefab.name + "_Undo";
			NewHistoryStep.transform.parent = Undo.Current.transform;
			int ListId = Undo.Current.AddToHistory (NewHistoryStep.GetComponent<HistoryObject> ());
			Undo.Current.CurrentStage = Undo.Current.History.Count;
			return Undo.Current.History [ListId];
		}

		/// <summary>
		/// Generates Redo
		/// </summary>
		/// <param name="Prefab">Prefab of redo type</param>
		public static HistoryObject GenerateRedo(GameObject Prefab){
			GameObject NewHistoryStep = Instantiate (Prefab) as GameObject;
			NewHistoryStep.name = Prefab.name + "_Redo";
			NewHistoryStep.transform.parent = Undo.Current.transform;
			int ListId = Undo.Current.AddToRedoHistory (NewHistoryStep.GetComponent<HistoryObject> ());
			Undo.Current.CurrentStage = Undo.Current.History.Count;
			return Undo.Current.RedoHistory [ListId];
		}



		public virtual void Register(){

		}
			

		public virtual void DoUndo(){

		}

		public virtual void DoRedo(){

		}

	}
}