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
	public		AppMenu				Menu;
	public		Editing				EditMenu;
	public		MapInfo				MapInfoMenu;

	[Header("Config")]
	public		int				MaxHistoryLength;

	//[Header("History")]
	List<HistoryObject> History = new List<HistoryObject>(50);
	List<HistoryObject> RedoHistory = new List<HistoryObject>(50);
	int				CurrentStage;

	void Awake(){
		Current = this;
	}

	void Start(){
		MaxHistoryLength = PlayerPrefs.GetInt(FafEditorSettings.UndoHistory, FafEditorSettings.DefaultUndoHistory);
		Clear();
	}

	public void AddUndoCleanup()
	{
		while(History.Count > CurrentStage)
		{
			//Destroy(History[History.Count - 1]);
			History.RemoveAt(History.Count - 1);
			//Destroy(RedoHistory[0]);
			//RedoHistory.RemoveAt(0);
		}

		int hc = History.Count;
		for (int i = 0; i < hc; i++)
		{
			History[i].RedoGenerated = false;
		}

		RedoHistory.Clear();
	}

	public void Clear()
	{
		if (History == null)
			History = new List<HistoryObject>();

		if (RedoHistory == null)
			RedoHistory = new List<HistoryObject>();

		for (int i = 0; i < History.Count; i++)
		{
			//if(History[i] && History[i].gameObject)
			//	Destroy(History[i].gameObject);
		}
		for (int i = 0; i < RedoHistory.Count; i++)
		{
			//if(RedoHistory[i] && RedoHistory[i].gameObject)
			//	Destroy(RedoHistory[i].gameObject);
		}

		CurrentStage = 0;

		History.Clear();
		RedoHistory.Clear();
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
		//Debug.Log("Undo to " + UndoTo);

		CurrentStage = UndoTo;
	}

	public void DoRedo(){
		// Use RedoCommand
		if(CurrentStage == History.Count){
			Debug.LogWarning("Cant redo: current");
			return;
		}
		int RedoTo = (History.Count - 1) - CurrentStage;

		//Debug.Log("Redo to " + RedoTo);

		RedoHistory [RedoTo].DoRedo ();
		CurrentStage++;
	}


//********************************************* FUNCTIONS
	public int AddToHistory(HistoryObject NewHistoryStep){
		History.Add (NewHistoryStep);
		if (History.Count - 1 >= MaxHistoryLength) {
			//Destroy(History[0].gameObject);
			History.RemoveAt(0);
		}
		return History.Count - 1;
	}

	public int AddToRedoHistory(HistoryObject NewHistoryStep){
		RedoHistory.Add (NewHistoryStep);
		if (RedoHistory.Count - 1 >= MaxHistoryLength) {
			//Destroy(RedoHistory[0].gameObject);
			RedoHistory.RemoveAt(0);
		}
		return RedoHistory.Count - 1;
	}


	public static void RegisterUndo(HistoryObject UndoStep, HistoryObject.HistoryParameter Params = null)
	{
		Current.AddUndoCleanup();
		Current.AddToHistory(UndoStep);
		Current.CurrentStage = Current.History.Count;
		UndoStep.Register(Params);
	}

	public static void RegisterRedo(HistoryObject RedoStep, HistoryObject.HistoryParameter Params = null)
	{
		Current.AddToRedoHistory(RedoStep);
		Current.CurrentStage = Current.History.Count;
		RedoStep.Register(Params);
	}

	//*********************************************  REGISTER UNDO
	public void RegisterSelectionChange()
	{
		// NotImplemented
	}

	public void RegisterSelectionRangeChange()
	{
		// NotImplemented
	}
}