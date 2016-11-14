using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainMesh : MonoBehaviour {

	public MeshFilter mf;
	public float[,] Heights;

	public void GenerateMesh(){
		int width = Heights.GetLength(0);
		int height = Heights.GetLength(1);

		width = 200;
		height = 200;

		Mesh NewMesh = new Mesh ();
		Vector3[] Verts = new Vector3[width * height];
		Vector2[] Uv = new Vector2[width * height];
		int[] Indices = new int[(width - 1) * (height - 1) * 4];


		int IndicesId = 0;
		int i = 0;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x > 512 || y > 512)
					continue;

				Verts [i] = new Vector3 (x * 0.1f, Heights[y, x] * 16, y * 0.1f);
				Uv [i] = new Vector2 (x / width, y / height);

				if (x < width - 1 && y < height - 1) {
					Indices [IndicesId + 3] = i;
					Indices [IndicesId + 2] = i + width;
					Indices [IndicesId + 1] = i + width + 1;
					Indices [IndicesId + 0] = i + 1;
					IndicesId += 4;
				}
				i ++;

			}
		}
		Debug.Log (Verts.Length);
		NewMesh.vertices = Verts;
		NewMesh.uv = Uv;
		NewMesh.subMeshCount = 1;
		NewMesh.SetIndices (Indices, MeshTopology.Quads, 0);
		NewMesh.RecalculateNormals ();
		NewMesh.MarkDynamic ();

		mf.sharedMesh = NewMesh;
	}
}
