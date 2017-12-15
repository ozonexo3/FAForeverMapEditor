using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using OzoneDecals;

public class HistoryDecalsSharedValues : HistoryObject
{

	public static Decal.DecalSharedSettings RegisterShared;
	Decal.DecalSharedSettings Shared;
	public TerrainDecalType Type;
	public string Tex1Path;
	public string Tex2Path;

	public override void Register()
	{
		Shared = RegisterShared;// DecalSettings.GetLoaded;

		Type = Shared.Type;
		Tex1Path = Shared.Tex1Path;
		Tex2Path = Shared.Tex2Path;
	}


	public override void DoUndo()
	{
		RegisterShared = Shared;

		if (!RedoGenerated)
			HistoryDecalsSharedValues.GenerateRedo(Undo.Current.Prefabs.DecalSharedValues).Register();
		RedoGenerated = true;
		DoRedo();
	}

	public override void DoRedo()
	{
		Shared.Type = Type;
		Shared.Tex1Path = Tex1Path;
		Shared.Tex2Path = Tex2Path;

		Shared.UpdateMaterial();

		Undo.Current.EditMenu.ChangeCategory(5);
		DecalsInfo.Current.GoToSelection();
		DecalsInfo.Current.DecalSettingsUi.Load(Shared);

		//DecalsInfo.Current.GoToSelection();
		//Selection.SelectionManager.Current.FinishSelectionChange();

	}
}
