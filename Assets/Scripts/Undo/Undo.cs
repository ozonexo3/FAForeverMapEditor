// ******************************************************************************
//
// * Simple Undo system. Values are stored in Prefabs. 
// * Copyright ozonexo3 2017
//
// ******************************************************************************

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public partial class Undo : MonoBehaviour {

	public static Undo Current;

	[Header("Classes")]
	public		MapLuaParser		Scenario;
	public		ScmapEditor			Scmap;
	public		AppMenu				Menu;
	public		Editing				EditMenu;
	public		MapInfo				MapInfoMenu;

	[Header("Config")]
	public		int				MaxHistoryLength;
	public		UndoPrefabs		Prefabs;

	[Header("History")]
	public		List<HistoryObject>		History;
	public		List<HistoryObject>		RedoHistory;
	public		int				CurrentStage;
	public		bool			CurrentSaved = false;

	public static	bool		RegisterMarkersDelete = false;

	void Awake(){
		Current = this;
	}

	void Start(){
		MaxHistoryLength = PlayerPrefs.GetInt(FafEditorSettings.UndoHistory, FafEditorSettings.DefaultUndoHistory);
	}

	public void AddUndoCleanup()
	{
		while(History.Count > CurrentStage)
		{
			Destroy(History[History.Count - 1].gameObject);
			History.RemoveAt(History.Count - 1);
			Destroy(RedoHistory[0].gameObject);
			RedoHistory.RemoveAt(0);
		}
	}

	// keys
	void Update(){
		if(Input.GetKey(KeyCode.LeftControl)){
			if(Input.GetKey(KeyCode.LeftShift)){
				if(Input.GetKeyDown(KeyCode.Z)){
					DoRedo();
				}
			}
			else if(Input.GetKeyDown(KeyCode.Z)){
				DoUndo();
			}
			else if(Input.GetKeyDown(KeyCode.Y)){
				DoRedo();
			}
		}
	}

	public void DoUndo(){
		// Add current stage of same type as "Undo" command
		int UndoTo = CurrentStage - 1;

		if(UndoTo < 0){
			Debug.LogWarning("Cant undo: last");
			return;
		}
			
		History [UndoTo].DoUndo ();

		CurrentStage = UndoTo;
	}

	public void DoRedo(){
		// Use RedoCommand
		if(CurrentStage == History.Count){
			Debug.LogWarning("Cant redo: current");
			return;
		}
		int RedoTo = (History.Count - 1) - CurrentStage;

		Debug.Log("Redo to " + RedoTo);

		RedoHistory [RedoTo].DoRedo ();
		CurrentStage++;
	}


//********************************************* FUNCTIONS
	public int AddToHistory(HistoryObject NewHistoryStep){
		History.Add (NewHistoryStep);
		if (History.Count - 1 >= MaxHistoryLength) {
			Destroy(History[0].gameObject);
			History.RemoveAt(0);
		}
		return History.Count - 1;
	}

	public int AddToRedoHistory(HistoryObject NewHistoryStep){
		RedoHistory.Add (NewHistoryStep);
		if (RedoHistory.Count - 1 >= MaxHistoryLength) {
			Destroy(RedoHistory[0].gameObject);
			RedoHistory.RemoveAt(0);
		}
		return RedoHistory.Count - 1;
	}
}