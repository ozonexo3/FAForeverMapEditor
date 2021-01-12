using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditMap
{
	public partial class WavesRenderer : MonoBehaviour
	{

		public struct ShoreDepthPoint
		{
			public Vector3 point;
			public float angle;

			public ShoreDepthPoint(Vector3 point, float angle)
			{
				this.point = point;
				this.angle = angle;
			}
		}

		public static List<ShoreDepthPoint> GetShoreDepthPoints(float depth, Vector2 angleRange)
		{
			List<ShoreDepthPoint> points = new List<ShoreDepthPoint>(1024);

			heights = ScmapEditor.Heights;
			heightmapWidth = ScmapEditor.HeightmapWidth;
			heightmapHeight = ScmapEditor.HeightmapHeight;

			terrainSizeX = ScmapEditor.Current.Teren.terrainData.size.x;
			terrainSizeY = ScmapEditor.Current.Teren.terrainData.size.y;
			terrainSizeZ = ScmapEditor.Current.Teren.terrainData.size.z;

			worldWaterLevel = ScmapEditor.GetWaterLevel();

			waterLevel = worldWaterLevel / terrainSizeY;
			float shoreDepthLevel = (worldWaterLevel - depth) / terrainSizeY;


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

					bool isOverWater0 = h0 > shoreDepthLevel;
					bool isOverWater1 = h1 > shoreDepthLevel;
					bool isOverWater2 = h2 > shoreDepthLevel;
					bool isOverWater3 = h3 > shoreDepthLevel;

					if (isOverWater0 != isOverWater1 || isOverWater1 != isOverWater2 || isOverWater2 != isOverWater3)
					{
						// Shore Detected!

						float p0x = x0 / (heightmapWidth - 1f);
						float p1x = x / (heightmapWidth - 1f);
						float p0y = y0 / (heightmapHeight - 1f);
						float p1y = y / (heightmapHeight - 1f);

						Vector3 point0 = new Vector3(p0y * terrainSizeX, h0 * terrainSizeY, p0x * terrainSizeZ - terrainSizeZ);
						Vector3 point1 = new Vector3(p0y * terrainSizeX, h1 * terrainSizeY, p1x * terrainSizeZ - terrainSizeZ);
						Vector3 point2 = new Vector3(p1y * terrainSizeX, h2 * terrainSizeY, p1x * terrainSizeZ - terrainSizeZ);
						Vector3 point3 = new Vector3(p1y * terrainSizeX, h3 * terrainSizeY, p0x * terrainSizeZ - terrainSizeZ);

						Vector3 center = (point1 + point3) / 2f;
						center.y = worldWaterLevel;

						Vector3 normal0 = Vector3.Cross((point3 - point0).normalized, (point1 - point0).normalized).normalized;
						Vector3 normal1 = Vector3.Cross((point1 - point2).normalized, (point3 - point2).normalized).normalized;
						Vector3 finalNormal = (normal0 + normal1) / 2f;

						float angle = Quaternion.LookRotation(finalNormal.normalized).eulerAngles.y;

						while (angle < 0f)
							angle += 360f;
						while (angle > 360f)
							angle -= 360f;

						

						if (Mathf.Abs(Mathf.DeltaAngle(angle - 180f, angleRange.x)) <= angleRange.y)
							points.Add(new ShoreDepthPoint(center, angle));
					}
				}
			}

			return points;
		}


	}
}
