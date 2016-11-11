using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;


public class HistoryStratumPaint : HistoryObject {

	int Id;
	Color[] Colors;

	public override void Register(){
		Undo.RegisterMarkersDelete = true;

		Colors = new Color[Undo.UndoData_Stratum.Length];
		Undo.UndoData_Stratum.CopyTo (Colors, 0);
		Id = Undo.UndoData_StratumId;
	}


	public override void DoUndo(){
		//Undo.UndoData_newheights = Undo.Current.Scmap.Teren.terrainData.GetHeights(0, 0, Undo.Current.Scmap.Teren.terrainData.heightmapWidth, Undo.Current.Scmap.Teren.terrainData.heightmapHeight);
		if (Id == 1) {
			Undo.UndoData_StratumId = Id;
			Undo.UndoData_Stratum = Undo.Current.Scmap.map.TexturemapTex2.GetPixels ();
		} else {
			Undo.UndoData_StratumId = Id;
			Undo.UndoData_Stratum = Undo.Current.Scmap.map.TexturemapTex.GetPixels ();
		}

		HistoryStratumPaint.GenerateRedo (Undo.Current.Prefabs.StratumPaint).Register();
		DoRedo ();
	}

	public override void DoRedo(){
		if(Undo.Current.EditMenu.State != Editing.EditStates.TexturesStat){
			Undo.Current.EditMenu.State = Editing.EditStates.TexturesStat;
			Undo.Current.EditMenu.ChangeCategory(2);
		}

		if (Id == 1) {
			Undo.Current.Scmap.map.TexturemapTex2.SetPixels (Colors);
			Undo.Current.Scmap.map.TexturemapTex2.Apply ();
		} else {
			Undo.Current.Scmap.map.TexturemapTex.SetPixels (Colors);
			Undo.Current.Scmap.map.TexturemapTex.Apply ();
		}

	}
}
