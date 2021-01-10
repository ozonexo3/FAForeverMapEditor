using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace EditMap
{
	public partial class WavesRenderer : MonoBehaviour
	{

		[Header("Debug")]
		public Gradient edgeColors;
		public Material shoreLineMaterial;
		public bool DrawShoreLine;
		public Camera RenderCamera;

		public static void GenerateShoreline()
		{
			if (IsShoreLineGenerated)
				return;

			if (ShoreLineTask != null)
			{
				ClearShoreLine();
				return;
			}
			ShoreLineTask = Instance.StartCoroutine(Instance._GenerateShoreline());
		}

		public static void ClearShoreLine()
		{
			IsShoreLineGenerated = false;
			ShoreLines.Clear();
			if (ShoreLineTask != null)
			{
				if (thread != null)
				{
					thread.Abort();
				}
				Instance.StopCoroutine(ShoreLineTask);
			}
		}

		static Coroutine ShoreLineTask;
		static Thread thread;
		public static bool IsShoreLineGenerated { get; private set; } = false;
		public static List<EdgeLoop> ShoreLines { get; private set; } = new List<EdgeLoop>(64);

		public struct Edge
		{
			public Vector3 point0;
			public Vector3 point1;
			public Vector3 normal0;
			//public Vector3 normal1;

			public Edge(Vector3 point0, Vector3 point1, Vector3 normal0) // , Vector3 normal1
			{
				this.point0 = point0;
				this.point1 = point1;

				Vector3 Dir = point0 - point1;

				Vector3 cross = Vector3.Cross(Dir.normalized, Vector3.up);
				//Vector3 normalAvarage = (normal0 + normal1) / 2f;

				if (Vector3.Dot(cross, normal0) < 0)
				{
					cross *= -1f;
				}

				this.normal0 = cross;
				//this.normal1 = normal1;
			}
		}

		public class EdgeLoop
		{
			public List<Edge> Edges = new List<Edge>(2048);
		}

		static float heightmapWidth;
		static float heightmapHeight;
		static float terrainSizeX;
		static float terrainSizeY;
		static float terrainSizeZ;
		static float worldWaterLevel;
		static float waterLevel;
		static float[,] heights;

		static int cutCount = 0;
		static Vector3[] localCutPoint = new Vector3[4];

		static Vector3 lowestPoint;
		static Vector3 highestPoint;
		static Vector3 planeNormal;

		static HashSet<Edge> allEdges = new HashSet<Edge>();

		IEnumerator _GenerateShoreline()
		{
			var ts = System.TimeSpan.FromTicks(System.DateTime.Now.Ticks);

			//Load data
			heights = ScmapEditor.Heights;
			heightmapWidth = ScmapEditor.HeightmapWidth;
			heightmapHeight = ScmapEditor.HeightmapHeight;

			terrainSizeX = ScmapEditor.Current.Teren.terrainData.size.x;
			terrainSizeY = ScmapEditor.Current.Teren.terrainData.size.y;
			terrainSizeZ = ScmapEditor.Current.Teren.terrainData.size.z;
			worldWaterLevel = ScmapEditor.GetWaterLevel();

			waterLevel = worldWaterLevel / terrainSizeY;

			//Clear
			ShoreLines.Clear();
			allEdges.Clear();

			thread = new Thread(ThreadWork);
			thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			thread.IsBackground = false;
			thread.Start();

			while (thread.IsAlive)
			{
				yield return null;
			}
			thread = null;

			//ThreadWork();
			//yield return null;

			//Clear
			heights = null;
			allEdges.Clear();
			ShoreLineTask = null;
			IsShoreLineGenerated = true;

			GenericInfoPopup.ShowInfo("Shoreline generated in " + (System.TimeSpan.FromTicks(System.DateTime.Now.Ticks).TotalSeconds - ts.TotalSeconds).ToString("F4") + "s");
			//Debug.Log("Shoreline generated in time: " + (System.TimeSpan.FromTicks(System.DateTime.Now.Ticks).TotalSeconds - ts.TotalSeconds).ToString("F4"));
		}

		void ThreadWork()
		{
			for (int x = 1; x < heightmapWidth; x++)
			{
				for (int y = 1; y < heightmapHeight; y++)
				{
					int x0 = x - 1;
					int y0 = y - 1;

					float h0 = heights[x0, y0];
					float h1 = heights[x, y0];
					float h2 = heights[x, y];
					float h3 = heights[x0, y];

					bool isOverWater0 = h0 > waterLevel;
					bool isOverWater1 = h1 > waterLevel;
					bool isOverWater2 = h2 > waterLevel;
					bool isOverWater3 = h3 > waterLevel;

					cutCount = 0;

					if (isOverWater0 != isOverWater1)
					{
						GetPoint(h0, h1, x0, y0, x, y0);
					}
					if (isOverWater1 != isOverWater2)
					{
						GetPoint(h1, h2, x, y0, x, y);
					}
					if (isOverWater2 != isOverWater3)
					{
						GetPoint(h2, h3, x, y, x0, y);
					}
					if (isOverWater3 != isOverWater0)
					{
						GetPoint(h3, h0, x0, y, x0, y0);
					}



					if (cutCount > 1) // 1 edge
					{
						planeNormal = lowestPoint - highestPoint;
						planeNormal.y = 0;
						planeNormal.Normalize();
						//Cut detected, create edges
						if ((localCutPoint[0] - localCutPoint[1]).sqrMagnitude > 0.000000015)
						{
							Edge newEdge = new Edge(localCutPoint[0], localCutPoint[1], planeNormal);
							allEdges.Add(newEdge);
						}
					}
					if (cutCount > 3) // 2 edges
					{
						//Cut detected, create edges
						if ((localCutPoint[2] - localCutPoint[3]).sqrMagnitude > 0.000000015)
						{
							Edge newEdge = new Edge(localCutPoint[2], localCutPoint[3], planeNormal);
							allEdges.Add(newEdge);
						}
					}
				}
			}

			//Transfer edges into groups
			var AllEdgesTestEnumerator = allEdges.GetEnumerator();

			while (AllEdgesTestEnumerator.MoveNext())
			{
				Edge fistEdge = AllEdgesTestEnumerator.Current;
				Vector3 point0 = fistEdge.point0;
				Vector3 point1 = fistEdge.point1;
				AllEdgesTestEnumerator.Dispose();

				EdgeLoop newEdgeLoop = new EdgeLoop();
				newEdgeLoop.Edges.Add(fistEdge);
				allEdges.Remove(fistEdge);

				bool found0 = true;
				bool found1 = true;
				bool finished0 = false;
				bool finished1 = false;
				HashSet<Edge>.Enumerator edgeEnumerator;
				Edge foundEdge0 = new Edge();
				Edge foundEdge1 = new Edge();

				while (found0 || found1)
				{
					found0 = false;
					found1 = false;
					edgeEnumerator = allEdges.GetEnumerator();

					while (edgeEnumerator.MoveNext())
					{
						Edge current = edgeEnumerator.Current;
						if (!found0 && !finished0)
						{
							if (FastComparePoints(current.point0, point0))
							{
								newEdgeLoop.Edges.Insert(0, current);
								point0 = current.point1;
								foundEdge0 = current;
								found0 = true;
								if (found1 || finished1)
									break;
								continue;
							}
							else if (FastComparePoints(current.point1, point0))
							{
								newEdgeLoop.Edges.Insert(0, current);
								point0 = current.point0;
								foundEdge0 = current;
								found0 = true;
								if (found1 || finished1)
									break;
								continue;
							}
						}
						if (!found1 && !finished1)
						{
							if (FastComparePoints(current.point0, point1))
							{
								newEdgeLoop.Edges.Add(current);
								point1 = current.point1;
								foundEdge1 = current;
								found1 = true;
								if (found0 || finished0)
									break;
								//break;
							}
							else if (FastComparePoints(current.point1, point1))
							{
								newEdgeLoop.Edges.Add(current);
								point1 = current.point0;
								foundEdge1 = current;
								found1 = true;
								if (found0 || finished0)
									break;
								//break;
							}
						}
					}
					edgeEnumerator.Dispose();

					if (found0)
						allEdges.Remove(foundEdge0);
					else
						finished0 = true;
					if (found1)
						allEdges.Remove(foundEdge1);
					else
						finished1 = true;
				}

				ShoreLines.Add(newEdgeLoop);

				AllEdgesTestEnumerator = allEdges.GetEnumerator();
			}
		}

		static bool FastComparePoints(Vector3 p0, Vector3 p1)
		{
			return Mathf.Abs(p0.x - p1.x) < 0.00015f && Mathf.Abs(p0.z - p1.z) < 0.00015f;
		}

		void GetPoint(float h0, float h1, int x0, int y0, int x1, int y1)
		{
			float p0x = x0 / (heightmapWidth - 1f);
			float p1x = x1 / (heightmapWidth - 1f);
			float p0y = y0 / (heightmapHeight - 1f);
			float p1y = y1 / (heightmapHeight - 1f);

			if (cutCount <= 0)
			{
				lowestPoint.y = 100000f;
				highestPoint.y = -100000f;
			}

			if (h0 < h1)
			{
				float lerp = (waterLevel - h0) / (h1 - h0); // 0 >>> 1
				float worldX = Mathf.Lerp(p0x, p1x, lerp) * terrainSizeZ;
				float worldZ = Mathf.Lerp(p0y, p1y, lerp) * terrainSizeX;
				localCutPoint[cutCount] = new Vector3(worldZ, worldWaterLevel, worldX - terrainSizeZ);

				if (lowestPoint.y > h0 * terrainSizeY)
				{
					lowestPoint = new Vector3(p0y * terrainSizeX, h0 * terrainSizeY, p0x * terrainSizeZ - terrainSizeZ);
				}
				if (highestPoint.y < h1 * terrainSizeY)
				{
					highestPoint = new Vector3(p1y * terrainSizeX, h1 * terrainSizeY, p1x * terrainSizeZ - terrainSizeZ);
				}
				cutCount++;
			}
			else
			{
				float lerp = (waterLevel - h1) / (h0 - h1); // 1 >>> 0
				float worldX = Mathf.Lerp(p1x, p0x, lerp) * terrainSizeZ;
				float worldZ = Mathf.Lerp(p1y, p0y, lerp) * terrainSizeX;
				localCutPoint[cutCount] = new Vector3(worldZ, worldWaterLevel, worldX - terrainSizeZ);

				if (highestPoint.y < h0 * terrainSizeY)
				{
					highestPoint = new Vector3(p0y * terrainSizeX, h0 * terrainSizeY, p0x * terrainSizeZ - terrainSizeZ);
				}
				if (lowestPoint.y > h1 * terrainSizeY)
				{
					lowestPoint = new Vector3(p1y * terrainSizeX, h1 * terrainSizeY, p1x * terrainSizeZ - terrainSizeZ);
				}

				cutCount++;
			}
		}

		private void OnRenderObject()
		{
			if (ShoreLineTask != null || !DrawShoreLine)
				return;

			if (ShoreLines.Count <= 0)
				return;

#if !UNITY_EDITOR
		if (Camera.current != RenderCamera)
			return;
#endif

			GL.PushMatrix();
			GL.MultMatrix(Matrix4x4.identity);
			shoreLineMaterial.SetPass(0);
			for (int i = 0; i < ShoreLines.Count; i++)
			{
				GL.Begin(GL.LINES);

				int edgesCount = ShoreLines[i].Edges.Count;
				//Color drawColor = edgeColors.Evaluate(edgesCount / 512f);
				//Color shorelineColor = Color.cyan;
				for (int e = 0; e < edgesCount; e++)
				{
					Vector3 center = (ShoreLines[i].Edges[e].point0 + ShoreLines[i].Edges[e].point1) / 2f;

					Vector3 viewportPos = RenderCamera.WorldToViewportPoint(center);
					if (viewportPos.x < -0.01f || viewportPos.x > 1.01f || viewportPos.y < -0.01f || viewportPos.y > 1.01f || viewportPos.z < 0)
						continue;

					GL.Vertex(ShoreLines[i].Edges[e].point0);
					GL.Vertex(ShoreLines[i].Edges[e].point1);

					//GL.Vertex(center);
					//GL.Vertex(center + ShoreLines[i].Edges[e].normal0 * 0.3f);

					//GL.Vertex(center);
					//GL.Vertex(center + ShoreLines[i].Edges[e].normal1 * 0.3f);

					//Debug.DrawLine(ShoreLines[i].Edges[e].point0, ShoreLines[i].Edges[e].point1, drawColor);

					/*if(edgesCount <= 4)
					{
						Debug.DrawLine(ShoreLines[i].Edges[e].point0, ShoreLines[i].Edges[e].point0 + Vector3.up, Color.red);
					}*/
				}
				GL.End();
			}

			GL.PopMatrix();
		}
	}
}