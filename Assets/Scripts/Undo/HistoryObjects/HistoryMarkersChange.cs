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

			//WeatherGenerator
			public float cloudCount = 10f;
			public float cloudCountRange = 0f;
			public float cloudEmitterScale = 1f;
			public float cloudEmitterScaleRange = 0.0f;
			public float cloudSpread = 150f;
			public float cloudHeightRange = 15;
			public float spawnChance = 1;
			public string ForceType = "None";
			public float cloudHeight = 180;

			//WeatherDefinition
			public Vector3 WeatherDriftDirection = Vector3.right;
			public string MapStyle = "Tundra";
			public string WeatherType04 = "WhitePatchyClouds";
			public string WeatherType03 = "None";
			public string WeatherType02 = "WhiteThickClouds";
			public string WeatherType01 = "SnowClouds";
			public float WeatherType04Chance = 0.1f;
			public float WeatherType03Chance = 0.3f;
			public float WeatherType02Chance = 0.3f;
			public float WeatherType01Chance = 0.3f;

			public Vector3 Position;
			public Quaternion Orientation;

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

				cloudCount = Marker.cloudCount;
				cloudCountRange = Marker.cloudCountRange;
				cloudEmitterScale = Marker.cloudEmitterScale;
				cloudEmitterScaleRange = Marker.cloudEmitterScaleRange;
				cloudSpread = Marker.cloudSpread;
				cloudHeightRange = Marker.cloudHeightRange;
				spawnChance = Marker.spawnChance;
				ForceType = Marker.ForceType;
				cloudHeight = Marker.cloudHeight;

				WeatherDriftDirection = Marker.WeatherDriftDirection;
				MapStyle = Marker.MapStyle;
				WeatherType04 = Marker.WeatherType04;
				WeatherType03 = Marker.WeatherType03;
				WeatherType02 = Marker.WeatherType02;
				WeatherType01 = Marker.WeatherType01;
				WeatherType04Chance = Marker.WeatherType04Chance;
				WeatherType03Chance = Marker.WeatherType03Chance;
				WeatherType02Chance = Marker.WeatherType02Chance;
				WeatherType01Chance = Marker.WeatherType01Chance;


				if (Marker.MarkerObj && Marker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.CameraInfo)
				{
					Position = Marker.MarkerObj.Tr.position;
					Orientation = Marker.MarkerObj.Tr.localRotation;
				}
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

				if (Marker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.WeatherGenerator)
				{
					Marker.cloudCount = cloudCount;
					Marker.cloudCountRange = cloudCountRange;
					Marker.cloudEmitterScale = cloudEmitterScale;
					Marker.cloudEmitterScaleRange = cloudEmitterScaleRange;
					Marker.cloudSpread = cloudSpread;
					Marker.cloudHeightRange = cloudHeightRange;
					Marker.spawnChance = spawnChance;
					Marker.ForceType = ForceType;
					Marker.cloudHeight = cloudHeight;
				}

				if (Marker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.WeatherDefinition)
				{
					Marker.WeatherDriftDirection = WeatherDriftDirection;
					Marker.MapStyle = MapStyle;
					Marker.WeatherType04 = WeatherType04;
					Marker.WeatherType03 = WeatherType03;
					Marker.WeatherType02 = WeatherType02;
					Marker.WeatherType01 = WeatherType01;
					Marker.WeatherType04Chance = WeatherType04Chance;
					Marker.WeatherType03Chance = WeatherType03Chance;
					Marker.WeatherType02Chance = WeatherType02Chance;
					Marker.WeatherType01Chance = WeatherType01Chance;
				}

				if (Marker.MarkerObj && Marker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.CameraInfo)
				{
					Marker.MarkerObj.Tr.position = Position;
					Marker.MarkerObj.Tr.localRotation = Orientation;
				}
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

			SelectionManager.Current.FinishSelectionChange();


			MarkerSelectionOptions.UpdateOptions();


		}
	}
}