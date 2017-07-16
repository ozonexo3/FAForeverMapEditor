using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;


public class HistoryStratumChange : HistoryObject {

	public int i = 0;
	public		ScmapEditor.TerrainTexture	Textures;



	public override void Register(){
		Undo.RegisterMarkersDelete = true;

		Textures = new ScmapEditor.TerrainTexture();

		i = Undo.UndoData_StratumId;
			
		Textures.Albedo = Undo.Current.Scmap.Textures [i].Albedo;
		Textures.AlbedoPath = Undo.Current.Scmap.Textures [i].AlbedoPath;
		Textures.AlbedoScale = Undo.Current.Scmap.Textures [i].AlbedoScale;

		Textures.Normal = Undo.Current.Scmap.Textures [i].Normal;
		Textures.NormalPath = Undo.Current.Scmap.Textures [i].NormalPath;
		Textures.NormalScale = Undo.Current.Scmap.Textures [i].NormalScale;

		Textures.Tilling = Undo.Current.Scmap.Textures [i].Tilling;
}


	public override void DoUndo(){
		if (!RedoGenerated)
			HistoryStratumChange.GenerateRedo (Undo.Current.Prefabs.StratumChange).Register();
		RedoGenerated = true;
		DoRedo ();
	}

	public override void DoRedo(){
		if(Undo.Current.EditMenu.State != Editing.EditStates.TexturesStat){
			Undo.Current.EditMenu.State = Editing.EditStates.TexturesStat;
			Undo.Current.EditMenu.ChangeCategory(2);
		}


		Undo.Current.Scmap.Textures [i].Albedo = Textures.Albedo;
		Undo.Current.Scmap.Textures [i].AlbedoPath = Textures.AlbedoPath;
		Undo.Current.Scmap.Textures [i].AlbedoScale = Textures.AlbedoScale;

		Undo.Current.Scmap.Textures [i].Normal = Textures.Normal;
		Undo.Current.Scmap.Textures [i].NormalPath = Textures.NormalPath;
		Undo.Current.Scmap.Textures [i].NormalScale = Textures.NormalScale;

		Undo.Current.Scmap.map.Layers [i].PathTexture = Textures.AlbedoPath;
		Undo.Current.Scmap.map.Layers [i].PathNormalmap = Textures.NormalPath;

		Undo.Current.Scmap.Textures [i].Tilling = Textures.Tilling;

		Undo.Current.Scmap.SetTextures (i);
	}
}
