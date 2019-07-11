using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selection;

public class RenderUnitRanges : MonoBehaviour
{

	static RenderUnitRanges Instance;

	private void Awake()
	{
		Instance = this;

		for(int i = 0; i < MaxUnitRanges; i++)
		{
			UnitRanges[i] = new UnitRange();
		}
	}

	const int MaxUnitRanges = 32;
	static UnitRange[] UnitRanges = new UnitRange[MaxUnitRanges];
	static int Count = 0;
	public Material lineMaterial;

	public class UnitRange
	{
		public Transform UnitTr;
		public float Radius;
		public float MinRadius;
	}

	public static void CreateRanges()
	{
		//SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].GetComponent<UnitInstance>()

		Count = SelectionManager.Current.Selection.Ids.Count;
		if (Count > MaxUnitRanges)
			Count = MaxUnitRanges;

		for(int i = 0; i < Count; i++)
		{
			GameObject UIobj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]];
			if (UIobj == null)
				continue;

			UnitInstance UI = UIobj.GetComponent<UnitInstance>();
			if (UI == null)
			{
				UnitRanges[i].UnitTr = null;
				UnitRanges[i].Radius = 0;
			}
			else
			{
				UnitRanges[i].UnitTr = UI.transform;
				UnitRanges[i].Radius = ScmapEditor.ScmapPosToWorld(UI.UnitRenderer.BP.MaxRange);
				UnitRanges[i].MinRadius = ScmapEditor.ScmapPosToWorld(UI.UnitRenderer.BP.MinRange);
			}
		}
	}

	public static void Clear()
	{
		Count = 0;
	}

	public void OnRenderObject()
	{
		if (Count <= 0)
			return;


		lineMaterial.SetPass(0);

		for (int i = 0; i < Count; i++)
		{
			if (UnitRanges[i].UnitTr == null)
				continue;

			if(UnitRanges[i].Radius > 0)
				DrawCircle(UnitRanges[i].UnitTr.position, UnitRanges[i].Radius, Color.red);
			if(UnitRanges[i].MinRadius > 0)
				DrawCircle(UnitRanges[i].UnitTr.position, UnitRanges[i].MinRadius, Color.blue);
		}
	}

	void DrawCircle(Vector3 center, float radius, Color col)
	{
		GL.PushMatrix();
		GL.Begin(GL.LINES);
		GL.Color(col);
		//float degRad = Mathf.PI / 180;
		for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += 0.01f)
		{
			Vector3 ci = (new Vector3(Mathf.Cos(theta) * radius + center.x, center.y, Mathf.Sin(theta) * radius + center.z));
			GL.Vertex3(ci.x, ci.y, ci.z);
		}
		GL.End();
		GL.PopMatrix();
	}
}
