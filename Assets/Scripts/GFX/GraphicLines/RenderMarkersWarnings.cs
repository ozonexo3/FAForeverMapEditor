using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderMarkersWarnings : MonoBehaviour {

	static RenderAdaptiveMarkers Instance;
	public static HashSet<MarkerWarning> WarningPosition = new HashSet<MarkerWarning>();


	public struct MarkerWarning{
		public Transform Marker;
		public string Log;

		public MarkerWarning(Transform Marker, string Log)
		{
			this.Marker = Marker;
			this.Log = Log;
		}
	}

	public static void Clear()
	{
		WarningPosition.Clear();
		RenderEnabled = false;
	}

	const string AiWarning = "Too close to the edge!";

	const float BorderOffset = 0.8f;

	public static void Generate()
	{
		Clear();

		if (!MapLuaParser.IsMapLoaded)
			return;

		if (MapLuaParser.LoadingMapProcess || MapLuaParser.SavingMapProcess)
			return;

		if (MapLuaParser.Current.ScenarioLuaFile.Data != null && MapLuaParser.Current.ScenarioLuaFile.Data.Size != null && MapLuaParser.Current.ScenarioLuaFile.Data.Size.Length >= 2) { }
		else
			return;

		if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains == null || MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length <= 0 || MapLuaParser.Current.SaveLuaFile.Data.MasterChains[0].Markers == null || MapLuaParser.Current.SaveLuaFile.Data.MasterChains[0].Markers.Count == 0)
			return;

		Vector3 MapMaxPoint = ScmapEditor.ScmapPosToWorld(
			new Vector3(MapLuaParser.Current.ScenarioLuaFile.Data.Size[0], 0, MapLuaParser.Current.ScenarioLuaFile.Data.Size[1])
			);

		int mc = 0;
		int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
		MapLua.SaveLua.Marker Current;
		for (int m = 0; m < Mcount; m++)
		{
			Current = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m];

			if (Current.MarkerObj == null)
				continue;

			if (Current.MarkerType != MapLua.SaveLua.Marker.MarkerTypes.Mass && Current.MarkerType != MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon)
				continue;

			Vector3 LocalPos = Current.MarkerObj.Tr.localPosition;

			if (LocalPos.x < BorderOffset
				|| LocalPos.z > -BorderOffset
				|| LocalPos.x > MapMaxPoint.x - BorderOffset
				|| LocalPos.z < MapMaxPoint.z + BorderOffset
				)
			{
				WarningPosition.Add(new MarkerWarning(Current.MarkerObj.Tr, AiWarning));
			}

		}

		RenderEnabled = WarningPosition.Count > 0;
	}


	static bool RenderEnabled;
	public void OnGUI()
	{
		RenderAdaptiveMarkers.DrawGUIStatic();

		if (!RenderEnabled || WarningPosition.Count <= 0)
			return;

		if (PreviewTex.IsPreview)
			return;

		if (MapLuaParser.LoadingMapProcess || MapLuaParser.SavingMapProcess)
			return;

		Camera MainCam = CameraControler.Current.Cam;
		Rect CamRect = MainCam.pixelRect;
		Rect UiRect = new Rect(CamRect.x, CamRect.y + (Screen.height - CamRect.height), CamRect.width, CamRect.height);
		GUI.BeginScrollView(UiRect, Vector2.zero, new Rect(0, (Screen.height - CamRect.height), CamRect.width, CamRect.height), false, false);

		//GUI.Label(new Rect(500, 500, 100, 50), "Test");

		//Color LastColor = GUI.contentColor;
		//GUI.contentColor = LabelColor;
		HashSet<MarkerWarning>.Enumerator ListEnum = WarningPosition.GetEnumerator();
		while (ListEnum.MoveNext())
		{
			MarkerWarning Current = ListEnum.Current;

			DrawGuiLabel(MainCam, CamRect, Current.Marker, Current.Log, LabelStyle);
		}
		ListEnum.Dispose();
		//GUI.contentColor = LastColor;

		GUI.EndScrollView();

	}
	public GUIStyle LabelStyle;

	static void DrawGuiLabel(Camera Cam, Rect CamRect, Transform Pivot, string text, GUIStyle Style)
	{
		if (Pivot == null)
			return;

		Vector3 position = Cam.WorldToScreenPoint(Pivot.localPosition);
		if (position.z < 0)
			return;
		Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(position.x - CamRect.x, (Screen.height - position.y), textSize.x, textSize.y), text, Style);
	}
}
