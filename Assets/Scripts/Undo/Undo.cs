using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public class Undo : MonoBehaviour {

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
		MaxHistoryLength = PlayerPrefs.GetInt("UndoHistry", 5);
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
		int RedoTo = History.Count - 1 - CurrentStage;


		RedoHistory [RedoTo].DoRedo ();
		CurrentStage++;
	}

//*********************************************  REGISTER UNDO
	#region REGISTER UNDO
	public void RegisterMapInfo(){
		HistoryMapInfo.GenerateUndo (Prefabs.MapInfo).Register();
	}

	public void RegisterMarkersMove(){
		HistoryMarkersMove.GenerateUndo (Prefabs.MarkersMove).Register();
	}

	public void RegisterMarkerSelection(){
		if(RegisterMarkersDelete){
			RegisterMarkersDelete = false;
			return;
		}
		HistoryMarkersSelection.GenerateUndo (Prefabs.MarkersSelection).Register();
	}


	public void RegisterMarkerChange(){
		HistoryMarkersChange.GenerateUndo (Prefabs.MarkersChange).Register();
	}

	public static float[,] UndoData_newheights;
	public void RegisterTerrainHeightmapChange(float[,] newheights){
		UndoData_newheights = newheights;
		HistoryTerrainHeight.GenerateUndo (Prefabs.TerrainHeightChange).Register();
	}

	public static Color[] UndoData_Stratum;
	public static int UndoData_StratumId;
	public void RegisterStratumPaint(Color[] colors, int id){
		UndoData_Stratum = colors;
		UndoData_StratumId = id;
		HistoryStratumPaint.GenerateUndo (Prefabs.StratumPaint).Register();
	}

	#endregion
		

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