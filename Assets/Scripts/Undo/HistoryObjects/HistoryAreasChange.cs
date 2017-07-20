using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using MapLua;

public class HistoryAreasChange : HistoryObject {

	public SaveLua.Areas[] Areas;

	public override void Register(){
		Areas = new SaveLua.Areas[MapLuaParser.Current.SaveLuaFile.Data.areas.Length];
		MapLuaParser.Current.SaveLuaFile.Data.areas.CopyTo(Areas, 0);
	}


	public override void DoUndo(){
		if (!RedoGenerated)
			HistoryAreasChange.GenerateRedo (Undo.Current.Prefabs.AreasChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){

		MapLuaParser.Current.SaveLuaFile.Data.areas = new SaveLua.Areas[Areas.Length];
		Areas.CopyTo(MapLuaParser.Current.SaveLuaFile.Data.areas, 0);


		Undo.Current.EditMenu.ChangeCategory(0);
		Undo.Current.EditMenu.MapInfoMenu.ChangePage(2);
		AreaInfo.Current.AreaDefault.isOn = true;
		AreaInfo.Current.ToggleSelected();
		AreaInfo.Current.UpdateList();
	}
}
