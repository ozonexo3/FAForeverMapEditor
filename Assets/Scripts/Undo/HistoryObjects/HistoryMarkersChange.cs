using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;
using Selection;
using Markers;

namespace UndoHistory
{
	public class HistoryMarkersChange : HistoryObject
	{

		private MarkersChangeHistoryParameter parameter;
		public class MarkersChangeHistoryParameter : HistoryParameter
		{
			public MapLua.SaveLua.Marker[] RegisterMarkers;

			public MarkersChangeHistoryParameter(MapLua.SaveLua.Marker[] RegisterMarkers)
			{
				this.RegisterMarkers = RegisterMarkers;
			}
		}


		public MarkerChange[] Markers;

		[System.Serializable]
		public class MarkerChange
		{
			public MapLua.SaveLua.Marker Marker;
			public bool Many;
			public string Name;
			public float zoom;
			public bool canSetCamera;
			public bool canSyncCamera;
			public float size;
			public float amount;
			public Vector3 offset;
			public float scale;
			public string effectTemplate;

			public void Load(MapLua.SaveLua.Marker RegisterMarker)
			{
				Marker = RegisterMarker;
				Name = Marker.Name;
				zoom = Marker.zoom;
				canSetCamera = Marker.canSetCamera;
				canSyncCamera = Marker.canSyncCamera;
				size = Marker.size;
				amount = Marker.amount;
				offset = Marker.offset;
				scale = Marker.scale;
				effectTemplate = Marker.EffectTemplate;

			}

			public void Redo()
			{
				if (Marker.Name != Name)
				{
					if (MapLua.SaveLua.MarkerExist(Name))
					{
						// Cant Undo, Name already exist
					}
					else
					{
						MapLua.SaveLua.RemoveMarker(Marker.Name);
						Marker.Name = Name;
						Marker.MarkerObj.gameObject.name = Name;
						MapLua.SaveLua.AddNewMarker(Marker);

					}
				}
				Marker.zoom = zoom;
				Marker.canSetCamera = canSetCamera;
				Marker.canSyncCamera = canSyncCamera;
				Marker.size = size;
				Marker.amount = amount;
				Marker.offset = offset;
				Marker.scale = scale;
				Marker.EffectTemplate = effectTemplate;
			}
		}


		public override void Register(HistoryParameter Param)
		{
			UndoCommandName = "Markers change";
			parameter = Param as MarkersChangeHistoryParameter;

			Markers = new MarkerChange[parameter.RegisterMarkers.Length];
			for (int i = 0; i < parameter.RegisterMarkers.Length; i++)
			{
				Markers[i] = new MarkerChange();
				Markers[i].Load(parameter.RegisterMarkers[i]);
			}
		}


		public override void DoUndo()
		{
			MapLua.SaveLua.Marker[] RegisterMarkers = new MapLua.SaveLua.Marker[Markers.Length];
			for (int i = 0; i < Markers.Length; i++)
				RegisterMarkers[i] = Markers[i].Marker;

			Undo.RegisterRedo(new HistoryMarkersChange(), new MarkersChangeHistoryParameter(RegisterMarkers));
			DoRedo();
		}

		public override void DoRedo()
		{

			if (Undo.Current.EditMenu.State != Editing.EditStates.MarkersStat)
			{
				Undo.Current.EditMenu.State = Editing.EditStates.MarkersStat;
				Undo.Current.EditMenu.ChangeCategory(4);
			}

			for (int i = 0; i < Markers.Length; i++)
			{
				Markers[i].Redo();
				if (i == 0)
					SelectionManager.Current.SelectObject(Markers[i].Marker.MarkerObj.gameObject);
				else
					SelectionManager.Current.SelectObjectAdd(Markers[i].Marker.MarkerObj.gameObject);

			}


			MarkerSelectionOptions.UpdateOptions();


		}
	}
}