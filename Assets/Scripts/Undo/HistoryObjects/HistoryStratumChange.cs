using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;

namespace UndoHistory
{
	public class HistoryStratumChange : HistoryObject
	{


		private StratumChangeHistoryParameter parameter;
		public class StratumChangeHistoryParameter : HistoryParameter
		{
			public int StratumId;

			public StratumChangeHistoryParameter(int StratumId)
			{
				this.StratumId = StratumId;
			}
		}

		public int i = 0;
		public ScmapEditor.TerrainTexture Textures;



		public override void Register(HistoryParameter Param)
		{
			UndoCommandName = "Stratum change";
			parameter = (Param as StratumChangeHistoryParameter);

			Textures = new ScmapEditor.TerrainTexture();

			i = parameter.StratumId;

			Textures.Albedo = ScmapEditor.Current.Textures[i].Albedo;
			Textures.AlbedoPath = ScmapEditor.Current.Textures[i].AlbedoPath;
			Textures.AlbedoScale = ScmapEditor.Current.Textures[i].AlbedoScale;

			Textures.Normal = ScmapEditor.Current.Textures[i].Normal;
			Textures.NormalPath = ScmapEditor.Current.Textures[i].NormalPath;
			Textures.NormalScale = ScmapEditor.Current.Textures[i].NormalScale;

			Textures.Tilling = ScmapEditor.Current.Textures[i].Tilling;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryStratumChange(), new StratumChangeHistoryParameter(parameter.StratumId));
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{
			Undo.Current.EditMenu.SetState(Editing.EditStates.TexturesStat);

			ScmapEditor.Current.Textures[i].Albedo = Textures.Albedo;
			ScmapEditor.Current.Textures[i].AlbedoPath = Textures.AlbedoPath;
			ScmapEditor.Current.Textures[i].AlbedoScale = Textures.AlbedoScale;

			ScmapEditor.Current.Textures[i].Normal = Textures.Normal;
			ScmapEditor.Current.Textures[i].NormalPath = Textures.NormalPath;
			ScmapEditor.Current.Textures[i].NormalScale = Textures.NormalScale;

			ScmapEditor.Current.map.Layers[i].PathTexture = Textures.AlbedoPath;
			ScmapEditor.Current.map.Layers[i].PathNormalmap = Textures.NormalPath;

			ScmapEditor.Current.Textures[i].Tilling = Textures.Tilling;

			if(ScmapEditor.Current.Textures[i].Albedo == null)
			{
				GetGamedataFile.LoadTextureFromGamedata(Textures.AlbedoPath, i, false);
			}

			if (ScmapEditor.Current.Textures[i].Normal == null)
			{
				GetGamedataFile.LoadTextureFromGamedata(Textures.NormalPath, i, true);
			}

			ScmapEditor.Current.SetTextures(i);

			Undo.Current.EditMenu.EditStratum.ReloadStratums();
			Undo.Current.EditMenu.EditStratum.SelectStratum(i);


		}
	}
}