using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderLineBetweenTransforms : MonoBehaviour {

	// https://docs.unity3d.com/ScriptReference/GL.html

	public Color LineColor;
	public Transform[] Transforms;
	static Material lineMaterial;
	static void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt("_ZWrite", 0);
		}
	}

	// Will be called after all regular rendering is done
	Vector3 Pos;
	public void OnPostRender()
	//public void OnRenderObject()
	{
		if (PreviewTex.IsPreview)
			return;

		CreateLineMaterial();
		// Apply the line material
		lineMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);

		// Draw lines
		GL.Begin(GL.LINES);
		GL.Color(LineColor);
		for (int i = 1; i < Transforms.Length; i++)
		{
			Pos = Transforms[i-1].localPosition;
			GL.Vertex3(Pos.x, Pos.y, Pos.z);

			Pos = Transforms[i].localPosition;
			GL.Vertex3(Pos.x, Pos.y, Pos.z);
		}
		GL.End();
		GL.PopMatrix();
	}
}
