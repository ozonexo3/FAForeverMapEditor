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

//*********************************************  REGISTER UNDO
	#region REGISTER UNDO
	public void RegisterMapInfo(){
		HistoryMapInfo.GenerateUndo (Prefabs.MapInfo).Register();
	}

	public void RegisterArmiesChange()
	{
		HistoryArmiesChange.GenerateUndo(Prefabs.ArmiesChange).Register();
	}

	public void RegisterArmyChange(MapLua.ScenarioLua.Army Army)
	{
		HistoryArmyChange.CurrentArmy = Army;
		HistoryArmyChange.GenerateUndo(Prefabs.ArmyChange).Register();
	}

	public void RegisterAreasChange()
	{
		HistoryAreasChange.GenerateUndo(Prefabs.AreasChange).Register();
	}

	public void RegisterAreaChange(MapLua.SaveLua.Areas Area)
	{
		HistoryAreaChange.CurrentArea = Area;
		HistoryAreaChange.GenerateUndo(Prefabs.AreaChange).Register();
	}

	public void RegisterSelectionChange()
	{
		//HistoryMarkersMove.GenerateUndo(Prefabs.SelectionChange).Register();
	}

	public void RegisterSelectionRangeChange()
	{
		//if (HistorySelectionRange.DoingRedo)
		//	return;
		//HistorySelectionRange.GenerateUndo(Prefabs.SelectionRange).Register();
	}

#region Markers
	public void RegisterMarkersAdd()
	{
		HistoryMarkersRemove.GenerateUndo(Prefabs.MarkersRemove).Register();
	}

	public void RegisterMarkersRemove()
	{
		HistoryMarkersRemove.GenerateUndo(Prefabs.MarkersRemove).Register();
	}

	public void RegisterMarkersMove(bool MoveMenu = true){
		HistoryMarkersMove.UndoMenu = MoveMenu;
		HistoryMarkersMove.GenerateUndo (Prefabs.MarkersMove).Register();
	}

	public void RegisterMarkerChange(MapLua.SaveLua.Marker[] AllMarkers){
		HistoryMarkersChange.RegisterMarkers = AllMarkers;
		HistoryMarkersChange.GenerateUndo (Prefabs.MarkersChange).Register();
	}

	public void RegisterChainsChange()
	{
		HistoryMarkersMove.GenerateUndo(Undo.Current.Prefabs.ChainChange).Register();
	}

	public static int LastChainId = 0;
	public void RegisterChainMarkersChange(int ChainId = 0)
	{
		LastChainId = ChainId;
		HistoryMarkersMove.GenerateUndo(Undo.Current.Prefabs.ChainMarkers).Register();
	}
	#endregion

	#region Decals

	public void RegisterDecalsMove(bool MoveMenu = true)
	{
		HistoryDecalsMove.UndoMenu = MoveMenu;
		HistoryDecalsMove.GenerateUndo(Prefabs.DecalsMove).Register();
	}

	public void RegisterDecalsAdd()
	{

	}

	public void RegisterDecalsRemove()
	{

	}

	public void RegisterDecalsChange()
	{

	}
	#endregion

	#region Heightmap
	public static float[,] UndoData_newheights;
	public void RegisterTerrainHeightmapChange(float[,] newheights){
		UndoData_newheights = newheights;
		HistoryTerrainHeight.GenerateUndo (Prefabs.TerrainHeightChange).Register();
	}
	#endregion

#region Stratum
	public static Color[] UndoData_Stratum;
	public static int UndoData_StratumId;
	public void RegisterStratumPaint(Color[] colors, int id){
		UndoData_Stratum = colors;
		UndoData_StratumId = id;
		HistoryStratumPaint.GenerateUndo (Prefabs.StratumPaint).Register();
	}

	public static void RegisterStratumChange(int stratum){
		UndoData_StratumId = stratum;
		HistoryStratumChange.GenerateUndo (Undo.Current.Prefabs.StratumChange).Register();
	}
#endregion

	public static bool Slider = false;
	public void RegisterLightingChange(bool IsSlider = false)
	{
		Slider = IsSlider;
		HistoryLighting.GenerateUndo(Prefabs.LightingChange).Register();
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