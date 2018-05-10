using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderChainLine : MonoBehaviour {

	// https://docs.unity3d.com/ScriptReference/GL.html

	//public Color LineColor;
	//public Transform[] Transforms;
	public Material lineMaterial;

	public MapLua.SaveLua.Chain RenderChain;


	// Will be called after all regular rendering is done
	Vector3 Pos;
	Transform PreviousTransform;

	//public void OnPostRender()
	public void OnRenderObject()
	{
		if (RenderChain == null || RenderChain.ConnectedMarkers == null)
			return;
		if (PreviewTex.IsPreview)
			return;

		//CreateLineMaterial();
		// Apply the line material
		lineMaterial.SetPass(0);

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);


		// Draw lines
		GL.Begin(GL.LINES);
		
		int i = 1;
		int Count = RenderChain.ConnectedMarkers.Count;

		PreviousTransform = null;

		for (i = 0; i < Count; i++)
		{
			if (RenderChain.ConnectedMarkers[i] == null || RenderChain.ConnectedMarkers[i].MarkerObj == null)
				continue;

			if (!PreviousTransform)
			{
				PreviousTransform = RenderChain.ConnectedMarkers[i].MarkerObj.Tr;
				continue;
			}
			//GL.Color(LineColor);
			Pos = PreviousTransform.localPosition;
			GL.Vertex3(Pos.x, Pos.y, Pos.z);

			Pos = RenderChain.ConnectedMarkers[i].MarkerObj.Tr.localPosition;
			GL.Vertex3(Pos.x, Pos.y, Pos.z);
			PreviousTransform = RenderChain.ConnectedMarkers[i].MarkerObj.Tr;

		}
		GL.End();
		GL.PopMatrix();
	}
}
