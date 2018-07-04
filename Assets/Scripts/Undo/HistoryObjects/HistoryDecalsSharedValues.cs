using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UndoHistory;
using EditMap;
using OzoneDecals;

namespace UndoHistory
{
	public class HistoryDecalsSharedValues : HistoryObject
	{

		private DecalsSharedValuesHistoryParameter parameter;
		public class DecalsSharedValuesHistoryParameter : HistoryParameter
		{
			public Decal.DecalSharedSettings RegisterShared;

			public DecalsSharedValuesHistoryParameter(Decal.DecalSharedSettings RegisterShared)
			{
				this.RegisterShared = RegisterShared;
			}
		}

		Decal.DecalSharedSettings Shared;
		public TerrainDecalType Type;
		public string Tex1Path;
		public string Tex2Path;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as DecalsSharedValuesHistoryParameter);
			Shared = parameter.RegisterShared;

			Type = Shared.Type;
			Tex1Path = Shared.Tex1Path;
			Tex2Path = Shared.Tex2Path;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryDecalsSharedValues(), new DecalsSharedValuesHistoryParameter(Shared));
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
		}
	}
}