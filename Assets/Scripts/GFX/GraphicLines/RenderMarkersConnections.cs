using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Markers;
using MapLua;

public class RenderMarkersConnections : MonoBehaviour
{
	public Camera RenderCamera;
	public static RenderMarkersConnections Current;

	public ConnectionLayer[] Layers;

	[System.Serializable]
	public class ConnectionLayer
	{
		public bool Enabled;
		public Material Mat;
		public List<SaveLua.Marker.MarkerTypes> AllowedMarkers;
		public List<Edge> Edges = new List<Edge>();
		public int Count = 0;
	}

	[System.Serializable]
	public class Edge
	{
		public int ConId0;
		public int ConId1;

		public Transform Point0;
		public Transform Point1;
	}


	private void Awake()
	{
		Current = this;
	}

	private void OnEnable()
	{
		//UpdateConnections();

	}



	public void UpdateConnections()
	{
		RenderMarkersWarnings.Generate();

		if (generating)
		{
			Buffor = true;
			return;
		}

		Layers[0].Enabled = MarkersControler.Current.MarkerLayersSettings.LandNodes;
		Layers[1].Enabled = MarkersControler.Current.MarkerLayersSettings.NavyNodes;
		Layers[2].Enabled = MarkersControler.Current.MarkerLayersSettings.AirNodes;
		Layers[3].Enabled = MarkersControler.Current.MarkerLayersSettings.AmphibiousNodes;

		if(MapLuaParser.IsMapLoaded && MapLuaParser.Current.SaveLuaFile.Data.MasterChains != null && MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length > 0 && MapLuaParser.Current.SaveLuaFile.Data.MasterChains[0].Markers != null)
			StartCoroutine(GenerateEdges());
	}

	bool Buffor;
	bool generating = false;

	int Mcount = 0;
	IEnumerator GenerateEdges()
	{
		generating = true;

		int mc = 0;
		Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;

		for (int l = 0; l < Layers.Length; l++)
		{
			if (!Layers[l].Enabled)
				continue;

			Layers[l].Edges = new List<Edge>();

			Layers[l].Count = 0;
			for (int m = 0; m < Mcount; m++)
			{
				if (Layers[l].AllowedMarkers.Contains(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerType))
				{

					//string[] Names = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].adjacentTo.Split(" ".ToCharArray());
					Transform Tr = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.Tr;

					for (int e = 0; e < MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].AdjacentToMarker.Count; e++)
					{
						//int ConM = MarkerIdByName(mc, Names[e]);
						int ConM = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.IndexOf(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].AdjacentToMarker[e]);

						if (ConM >= 0 && !EdgeExist(l, m, ConM))
						{

							Edge NewEdge = new Edge();
							NewEdge.Point0 = Tr;
							NewEdge.Point1 = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[ConM].MarkerObj.Tr;

							NewEdge.ConId0 = m;
							NewEdge.ConId1 = ConM;

							Layers[l].Edges.Add(NewEdge);
							Layers[l].Count++;
						}
					}
				}
			}
			//yield return null;
		}

		yield return null;
		generating = false;
		if (Buffor)
		{
			Buffor = false;
			UpdateConnections();
		}

	}

	bool EdgeExist(int l, int p0, int p1)
	{
		for (int e = 0; e < Layers[l].Count; e++)
		{
			if ((Layers[l].Edges[e].ConId0 == p0 || Layers[l].Edges[e].ConId0 == p1) && (Layers[l].Edges[e].ConId1 == p0 || Layers[l].Edges[e].ConId1 == p1))
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
		if (PreviewTex.IsPreview)
			return;

		if (!MarkersControler.Current.MarkerLayersSettings.ConnectedNodes)
			return;

		if (Camera.current != RenderCamera)
			return;

		//CreateLineMaterial();
		// Apply the line material


		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);

		for (int l = 0; l < Layers.Length; l++)
		{
			if (!Layers[l].Enabled)
				continue;

			Layers[l].Mat.SetPass(0);



			// Draw lines
			GL.Begin(GL.LINES);

			for (int i = 0; i < Layers[l].Count; i++)
			{
				GL.Vertex(Layers[l].Edges[i].Point0.localPosition);
				GL.Vertex(Layers[l].Edges[i].Point1.localPosition);
			}
			GL.End();
		}

		GL.PopMatrix();

	}
}