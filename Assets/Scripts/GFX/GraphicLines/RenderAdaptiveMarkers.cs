using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Markers;
using MapLua;

public class RenderAdaptiveMarkers : MonoBehaviour {

	static RenderAdaptiveMarkers Instance;

	public Material lineMaterial;
	public Color[] ArmyColors;
	public Color LabelColor;

	static HashSet<AdaptiveConnection> AdaptiveConnections = new HashSet<AdaptiveConnection>();
	static HashSet<AdaptiveCustom> AdaptiveCustoms = new HashSet<AdaptiveCustom>();

	private void Awake()
	{
		Instance = this;
	}

	public static void Clear()
	{
		AdaptiveConnections.Clear();
	}

	static Color GetColor(int id)
	{
		if (Instance == null)
			return Color.white;

		while(id >= Instance.ArmyColors.Length)
		{
			id -= Instance.ArmyColors.Length;
		}

		return Instance.ArmyColors[id];
	}
	static bool RenderEnabled = false;
	public static void UpdateAdaptiveLines()
	{
		Clear();

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


			int ArmyCount = Current.SpawnWithArmy.Count;
			for(int ai = 0; ai < ArmyCount; ai++)
			{
				string ArmyMarkerName = "ARMY_" + (Current.SpawnWithArmy[ai] + 1);
				SaveLua.Marker ArmyMarker = SaveLua.GetMarker(ArmyMarkerName);
				if (ArmyMarker != null && ArmyMarker.MarkerObj != null)
				{
					AdaptiveConnections.Add(new AdaptiveConnection(Current.MarkerObj.Tr, ArmyMarker.MarkerObj.Tr, GetColor(Current.SpawnWithArmy[ai])));
				}
				else
					Debug.LogWarning("Cant find marker with name: " + ArmyMarkerName);
			}

			int AdaptiveCount = Current.AdaptiveKeys.Count;
			if (AdaptiveCount > 0)
			{
				string Text = "";
				for (int ai = 0; ai < AdaptiveCount; ai++)
				{
					if (ai > 0)
						Text += "\n";
					Text += Current.AdaptiveKeys[ai];
				}

				AdaptiveCustoms.Add(new AdaptiveCustom(Current.MarkerObj.Tr, Text));

			}
		}

		RenderEnabled = true;
	}


	public static void DisableRenderer()
	{
		RenderEnabled = false;
	}


	public struct AdaptiveConnection
	{
		public Transform Marker;
		public Transform Army;
		public Color ArmyColor;

		public AdaptiveConnection(Transform Marker, Transform Army, Color ArmyColor)
		{
			this.Marker = Marker;
			this.Army = Army;
			this.ArmyColor = ArmyColor;
		}
	}

	public struct AdaptiveCustom
	{
		public Transform Marker;
		public string Text;

		public AdaptiveCustom(Transform Marker, string Text)
		{
			this.Marker = Marker;
			this.Text = Text;
		}
	}

	public void OnRenderObject()
	{
		if (!RenderEnabled || AdaptiveConnections.Count <= 0)
			return;

		lineMaterial.SetPass(0);

		GL.PushMatrix();
		GL.MultMatrix(transform.localToWorldMatrix);

		GL.Begin(GL.LINES);

		HashSet<AdaptiveConnection>.Enumerator ListEnum = AdaptiveConnections.GetEnumerator();
		while (ListEnum.MoveNext())
		{
			AdaptiveConnection Current = ListEnum.Current;

			GL.Color(Current.ArmyColor);
			GL.Vertex(Current.Marker.position);
			GL.Vertex(Current.Army.position);
		}
		ListEnum.Dispose();

		GL.End();
		GL.PopMatrix();

	}

	void OnGUI()
	{
		if (!RenderEnabled || AdaptiveCustoms.Count <= 0)
			return;

		Camera MainCam = CameraControler.Current.Cam;
		Rect CamRect = MainCam.pixelRect;
		Rect UiRect = new Rect(CamRect.x, CamRect.y + (Screen.height - CamRect.height), CamRect.width, CamRect.height);
		GUI.BeginScrollView(UiRect, Vector2.zero, new Rect(0, (Screen.height - CamRect.height), CamRect.width, CamRect.height), false, false);

		//GUI.Label(new Rect(500, 500, 100, 50), "Test");

		Color LastColor = GUI.contentColor;
		GUI.contentColor = LabelColor;
		HashSet<AdaptiveCustom>.Enumerator ListEnum = AdaptiveCustoms.GetEnumerator();
		while (ListEnum.MoveNext())
		{
			AdaptiveCustom Current = ListEnum.Current;

			DrawGuiLabel(MainCam, CamRect, Current.Marker, Current.Text);
		}
		ListEnum.Dispose();
		GUI.contentColor = LastColor;

		GUI.EndScrollView();
	}

	static void DrawGuiLabel(Camera Cam, Rect CamRect, Transform Pivot, string text)
	{
		var position = Cam.WorldToScreenPoint(Pivot.position);
		var textSize = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(position.x - CamRect.x, (Screen.height - position.y), textSize.x, textSize.y), text);
	}
}
