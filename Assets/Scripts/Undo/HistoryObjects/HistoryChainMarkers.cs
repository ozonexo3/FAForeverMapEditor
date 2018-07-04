using UnityEngine;
using System.Linq;
using EditMap;

namespace UndoHistory
{
	public class HistoryChainMarkers : HistoryObject
	{


		private ChainMarkersHistoryParameter parameter;
		public class ChainMarkersHistoryParameter : HistoryParameter
		{
			public int LastChainId;

			public ChainMarkersHistoryParameter(int LastChainId)
			{
				this.LastChainId = LastChainId;
			}
		}

		public int ChainId;
		public string Name;
		public MapLua.SaveLua.Marker[] ConnectedMarkers;


		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as ChainMarkersHistoryParameter);
			ChainId = parameter.LastChainId;

			ConnectedMarkers = new MapLua.SaveLua.Marker[MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].ConnectedMarkers.Count];
			MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].ConnectedMarkers.CopyTo(ConnectedMarkers, 0);
			Name = MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].Name;
		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryChainMarkers(), new ChainMarkersHistoryParameter(ChainId));
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{

			//MapLuaParser.Current.SaveLuaFile.Data.Chains = new MapLua.SaveLua.Chain[AllChains.Length];
			//AllChains.CopyTo(MapLuaParser.Current.SaveLuaFile.Data.Chains, 0);

			MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].ConnectedMarkers = ConnectedMarkers.ToList();
			MapLuaParser.Current.SaveLuaFile.Data.Chains[ChainId].Name = Name;

			MarkersInfo.Current.ChainsInfo.CleanMenu();

			//Selection.SelectionManager.Current.SetCustomSettings(false, false, false);
			Selection.SelectionManager.Current.CleanSelection();

			Undo.Current.EditMenu.ChangeCategory(4);
			//NewMarkersInfo.Current.ClearCreateNew();
			MarkersInfo.Current.ChangePage(1);

			MarkersInfo.Current.ChainsInfo.SelectChain(ChainId);

		}
	}
}