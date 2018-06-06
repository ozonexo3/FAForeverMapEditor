// ******************************************************************************
// *
// * Simple Undo system. Values are stored in Prefabs. 
// * Copyright ozonexo3 2017
// *
// * Registers
// *
// ******************************************************************************

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

public partial class Undo : MonoBehaviour
{
	//*********************************************  REGISTER UNDO
	public void RegisterMapInfo()
	{
		HistoryMapInfo.GenerateUndo(Prefabs.MapInfo).Register();
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

	public void RegisterMarkersMove(bool MoveMenu = true)
	{
		HistoryMarkersMove.UndoMenu = MoveMenu;
		HistoryMarkersMove.GenerateUndo(Prefabs.MarkersMove).Register();
	}

	public void RegisterMarkerChange(MapLua.SaveLua.Marker[] AllMarkers)
	{
		HistoryMarkersChange.RegisterMarkers = AllMarkers;
		HistoryMarkersChange.GenerateUndo(Prefabs.MarkersChange).Register();
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
		HistoryDecalsChange.GenerateUndo(Prefabs.DecalsChange).Register();
	}

	public void RegisterDecalsRemove()
	{
		HistoryDecalsChange.GenerateUndo(Prefabs.DecalsChange).Register();
	}

	public void RegisterDecalsOrderChange()
	{
		HistoryDecalsChange.GenerateUndo(Prefabs.DecalsChange).Register();
	}

	public void RegisterDecalsValuesChange()
	{
		HistoryDecalsValues.GenerateUndo(Prefabs.DecalValues).Register();
	}

	public void RegisterDecalsSharedValuesChange()
	{
		HistoryDecalsSharedValues.RegisterShared = DecalSettings.GetLoaded;
		HistoryDecalsSharedValues.GenerateUndo(Prefabs.DecalSharedValues).Register();
	}
	#endregion

	#region Props
	public void RegisterPropsChange()
	{
		HistoryPropsChange.GenerateUndo(Prefabs.PropsChange).Register();
	}
	#endregion

	#region Heightmap
	public static float[,] UndoData_newheights;
	public void RegisterTerrainHeightmapChange(float[,] newheights)
	{
		UndoData_newheights = newheights;
		HistoryTerrainHeight.GenerateUndo(Prefabs.TerrainHeightChange).Register();
	}
	#endregion

	#region Water
	public void RegisterWaterElevationChange()
	{
		HistoryTerrainHeight.GenerateUndo(Prefabs.TerrainWaterElevationChange).Register();
	}

	public void RegisterWaterSettingsChange()
	{
		HistoryTerrainHeight.GenerateUndo(Prefabs.TerrainWaterSettingsChange).Register();
	}
	#endregion

	#region Stratum
	public static Color[] UndoData_Stratum;
	public static int UndoData_StratumId;
	public void RegisterStratumPaint(Color[] colors, int id)
	{
		UndoData_Stratum = colors;
		UndoData_StratumId = id;
		HistoryStratumPaint.GenerateUndo(Prefabs.StratumPaint).Register();
	}

	public static void RegisterStratumChange(int stratum)
	{
		UndoData_StratumId = stratum;
		HistoryStratumChange.GenerateUndo(Undo.Current.Prefabs.StratumChange).Register();
	}
	#endregion

	public void RegisterLightingChange()
	{
		HistoryLighting.GenerateUndo(Prefabs.LightingChange).Register();
	}

}
