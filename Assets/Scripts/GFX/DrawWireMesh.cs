using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWireMesh : MonoBehaviour {

	public Material lineMaterial;

	const float DownSize = 0.5f;
	const float UpperSize = 0.1f;
	const float Height = 0.5f;

	void OnRenderObject() {

		lineMaterial.SetPass(0);
		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);

		GL.Begin(GL.LINES);

		// Walls
		GL.Vertex3(DownSize, 0, DownSize);
		GL.Vertex3(UpperSize, Height, UpperSize);

		GL.Vertex3(-DownSize, 0, DownSize);
		GL.Vertex3(-UpperSize, Height, UpperSize);

		GL.Vertex3(DownSize, 0, -DownSize);
		GL.Vertex3(UpperSize, Height, -UpperSize);

		GL.Vertex3(-DownSize, 0, -DownSize);
		GL.Vertex3(-UpperSize, Height, -UpperSize);


		// Up Floor
		GL.Vertex3(UpperSize, Height, UpperSize);
		GL.Vertex3(-UpperSize, Height, UpperSize);

		GL.Vertex3(UpperSize, Height, -UpperSize);
		GL.Vertex3(-UpperSize, Height, -UpperSize);

		GL.Vertex3(UpperSize, Height, UpperSize);
		GL.Vertex3(UpperSize, Height, -UpperSize);

		GL.Vertex3(-UpperSize, Height, UpperSize);
		GL.Vertex3(-UpperSize, Height, -UpperSize);

		// DownFloor
		GL.Vertex3(DownSize, 0, DownSize);
		GL.Vertex3(-DownSize, 0, DownSize);

		GL.Vertex3(DownSize, 0, -DownSize);
		GL.Vertex3(-DownSize, 0, -DownSize);

		GL.Vertex3(DownSize, 0, DownSize);
		GL.Vertex3(DownSize, 0, -DownSize);

		GL.Vertex3(-DownSize, 0, DownSize);
		GL.Vertex3(-DownSize, 0, -DownSize);

		GL.End();
		GL.PopMatrix();

	}
}
