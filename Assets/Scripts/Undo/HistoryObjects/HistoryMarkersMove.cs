using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UndoHistory;
using EditMap;

namespace UndoHistory
{
	public class HistoryMarkersMove : HistoryObject
	{
		private MarkersMoveHistoryParameter parameter;
		public class MarkersMoveHistoryParameter : HistoryParameter
		{
			public bool UndoMenu;

			public MarkersMoveHistoryParameter(bool UndoMenu)
			{
				this.UndoMenu = UndoMenu;
			}
		}

		// MarkersPos
		Vector3[] MarkersPosSelection;
		Quaternion[] MarkersRotSelection;
		//public static bool UndoMenu;
		public bool UndoToMarkerMenu;

		public override void Register(HistoryParameter Param)
		{
			parameter = (Param as MarkersMoveHistoryParameter);
			int mc = 0;
			UndoToMarkerMenu = parameter.UndoMenu;
			MarkersPosSelection = new Vector3[MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count];
			MarkersRotSelection = new Quaternion[MarkersPosSelection.Length];
			for (int i = 0; i < MarkersPosSelection.Length; i++)
			{
				MarkersPosSelection[i] = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.Tr.localPosition;
				MarkersRotSelection[i] = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.Tr.localRotation;
			}


		}


		public override void DoUndo()
		{
			if (!RedoGenerated)
				Undo.RegisterRedo(new HistoryMarkersMove(), new MarkersMoveHistoryParameter(UndoToMarkerMenu));
			RedoGenerated = true;
			DoRedo();
		}

		public override void DoRedo()
		{

			int mc = 0;
			for (int i = 0; i < MarkersPosSelection.Length; i++)
			{
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.Tr.localPosition = MarkersPosSelection[i];
				MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i].MarkerObj.Tr.localRotation = MarkersRotSelection[i];
			}

			if (UndoToMarkerMenu)
			{
				Undo.Current.EditMenu.ChangeCategory(4);
				NewMarkersInfo.Current.ClearCreateNew();
				MarkersInfo.Current.ChangePage(0);

				NewMarkersInfo.Current.GoToSelection();
			}

		}
	}
}