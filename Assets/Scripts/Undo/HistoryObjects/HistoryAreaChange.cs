using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using MapLua;

public class HistoryAreaChange : HistoryObject {

	public static SaveLua.Areas CurrentArea;

	SaveLua.Areas Area;
	public string Name;
	public float X;
	public float Y;
	public float Width;
	public float Height;

	public override void Register(){
		Area = CurrentArea;
		Name = Area.Name;
		X = Area.rectangle.x;
		Y = Area.rectangle.y;
		Width = Area.rectangle.width;
		Height = Area.rectangle.height;

	}


	public override void DoUndo(){
		CurrentArea = Area;
		if (!RedoGenerated)
			HistoryAreaChange.GenerateRedo (Undo.Current.Prefabs.AreaChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){

		Area.Name = Name;
		Area.rectangle = new Rect(X, Y, Width, Height);


		Undo.Current.EditMenu.ChangeCategory(0);
		Undo.Current.EditMenu.MapInfoMenu.ChangePage(2);
		AreaInfo.Current.AreaDefault.isOn = true;
		AreaInfo.Current.ToggleSelected();
		AreaInfo.Current.UpdateList();
	}
}
