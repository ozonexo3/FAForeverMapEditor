using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Markers;
using MapLua;

public class RenderMarkersConnections : MonoBehaviour
{

	public Material AllMaterial;
	public Material SelectedMaterial;

	public List<SaveLua.Marker.MarkerTypes> AllowedMarkers;

	[System.Serializable]
	public class Edge
	{
		public int ConId0;
		public int ConId1;

		public Transform Point0;
		public Transform Point1;
	}

	private void OnEnable()
	{
		GenerateEdges();
	}

	int Count = 0;
	public List<Edge> Edges = new List<Edge>();
	bool generating = false;

	int Mcount = 0;
	void GenerateEdges()
	{
		Edges = new List<Edge>();

		int mc = 0;
		Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
		//bool[] MarkerDone = new bool[Mcount];
		Count = 0;
		for (int m = 0; m < Mcount; m++)
		{
			if(AllowedMarkers.Contains( MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType))
			{
				string[] Names = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].adjacentTo.Split(" ".ToCharArray());
				//MarkerDone[m] = true;

				Transform Tr = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.Tr;

				for (int e = 0; e < Names.Length; e++)
				{
					int ConM = MarkerIdByName(mc, Names[e]);

					if (ConM >= 0 && !EdgeExist(m, ConM))
					{
						//MarkerDone[ConM] = true;

						Edge NewEdge = new Edge();
						NewEdge.Point0 = Tr;
						NewEdge.Point1 = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[ConM].MarkerObj.Tr;

						Edges.Add(NewEdge);
						Count++;
					}
				}
			}
		}
		Count = Edges.Count;
	}

	bool EdgeExist(int p0, int p1)
	{
		for (int e = 0; e < Count; e++)
		{
			if ((Edges[e].ConId0 == p0 || Edges[e].ConId0 == p1) && (Edges[e].ConId1 == p0 || Edges[e].ConId1 == p1))
				return true;
		}
		return false;
	}

	int MarkerIdByName(int mc, string SearchName)
	{
		for (int m = 0; m < Mcount; m++)
		{
			if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.name == SearchName)
				return m;
		}
		return -1;
	}

	public void OnRenderObject()
	{
		if (generating)
			return;
		//CreateLineMaterial();
		// Apply the line material
		AllMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);


		// Draw lines
		GL.Begin(GL.LINES);

		for (int i = 0; i < Count; i++)
		{
			GL.Vertex(Edges[i].Point0.localPosition);
			GL.Vertex(Edges[i].Point1.localPosition);
		}
		GL.End();
		GL.PopMatrix();
	}
}