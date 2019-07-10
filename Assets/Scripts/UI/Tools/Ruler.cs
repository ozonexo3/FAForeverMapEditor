using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ruler : MonoBehaviour
{
	public static Ruler Instance;
	static bool Active;

	public Material lineMaterial;

	private void Awake()
	{
		Instance = this;
	}

	public static void Toggle(bool value)
	{
		Active = value;
	}

	bool Started = false;
	Vector3 Begin;
	Vector3 End;

	void Update()
    {
		//if (!Active)
		//	return;

		if(!Started && Input.GetMouseButtonDown(0) && Selection.SelectionManager.Current.IsPointerOnGameplay())
		{
			if (Active)
			{
				Begin = CameraControler.BufforedGameplayCursorPos;
				//Begin.y = 0;
				Started = true;
			}
		}

		if (!Started)
			return;

		if (Input.GetMouseButtonUp(0))
		{
			Started = false;
		}

		End = CameraControler.BufforedGameplayCursorPos;
		//End.y = 0;

		//Draw();
	}

	void OnRenderObject()
	{
		if (!Started)
			return;

		lineMaterial.SetPass(0);

		GL.PushMatrix();
		GL.MultMatrix(transform.localToWorldMatrix);

		GL.Begin(GL.LINES);

		GL.Color(LabelStyle.normal.textColor);

		GL.Vertex(Begin);
		GL.Vertex(End);

		GL.End();
		GL.PopMatrix();
	}

	private void Draw()
	{
		lineMaterial.SetPass(0);

		GL.PushMatrix();
		GL.MultMatrix(transform.localToWorldMatrix);

		GL.Begin(GL.LINES);

		GL.Color(LabelStyle.normal.textColor);

		GL.Vertex(Begin);
		GL.Vertex(End);

		GL.End();
		GL.PopMatrix();
	}

	void OnGUI()
	{
		if (!Started)
			return;

		// Label
		Camera MainCam = CameraControler.Current.Cam;
		Rect CamRect = MainCam.pixelRect;
		Rect UiRect = new Rect(CamRect.x, CamRect.y + (Screen.height - CamRect.height), CamRect.width, CamRect.height);
		GUI.BeginScrollView(UiRect, Vector2.zero, new Rect(0, (Screen.height - CamRect.height), CamRect.width, CamRect.height), false, false);

		Vector3 Center = (Begin + End) / 2f;

		Vector3 ScBegin = ScmapEditor.WorldPosToScmap(Begin);
		Vector3 ScEnd = ScmapEditor.WorldPosToScmap(End);
		ScBegin.y = 0;
		ScEnd.y = 0;

		float Distance = Vector3.Distance(ScBegin, ScEnd);

		DrawGuiLabel(MainCam, CamRect, Center, Distance.ToString("N2"), LabelStyle);

		GUI.EndScrollView();
	}

	public GUIStyle LabelStyle;

	static void DrawGuiLabel(Camera Cam, Rect CamRect, Vector3 pos, string text, GUIStyle Style)
	{
		Vector3 position = Cam.WorldToScreenPoint(pos);
		if (position.z < 0)
			return;
		Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(position.x - CamRect.x, (Screen.height - position.y), textSize.x, textSize.y), text, Style);
	}
}
