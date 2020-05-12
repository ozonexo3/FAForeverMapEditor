using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderChainLine : MonoBehaviour {

	// https://docs.unity3d.com/ScriptReference/GL.html

	//public Color LineColor;
	//public Transform[] Transforms;
	public Material lineMaterial;
	public Material lineSelectedMaterial;

	public float LineWidth = 2f;

	//public static MapLua.SaveLua.Chain RenderChain;
	public static bool DisplayAll = false;
	public static int SelectedChain = -1;

	// Will be called after all regular rendering is done
	Vector3 Pos;
	Transform PreviousTransform;

	//public void OnPostRender()
	public void OnRenderObject()
	{
		//if (RenderChain == null || RenderChain.ConnectedMarkers == null)
		//	return;
		if (!DisplayAll && SelectedChain < 0)
			return;

		if (PreviewTex.IsPreview || !MapLuaParser.IsMapLoaded)
			return;

		GL.PushMatrix();
		GL.MultMatrix(Matrix4x4.identity);

		if (DisplayAll)
		{
			lineMaterial.SetPass(0);
			for (int l = 0; l < MapLuaParser.Current.SaveLuaFile.Data.Chains.Length; l++)
			{
				MapLua.SaveLua.Chain RenderChain = MapLuaParser.Current.SaveLuaFile.Data.Chains[l];

				if (l == SelectedChain)
				{
					continue;
					//lineSelectedMaterial.SetPass(0);
				}



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
			}
		}

		if(SelectedChain >= 0 && SelectedChain < MapLuaParser.Current.SaveLuaFile.Data.Chains.Length)
		{

			MapLua.SaveLua.Chain RenderChain = MapLuaParser.Current.SaveLuaFile.Data.Chains[SelectedChain];
			Camera cam = CameraControler.Current.Cam;
			float nearClip = cam.nearClipPlane + 0.001f;

			lineSelectedMaterial.SetPass(0);

			GL.Begin(GL.QUADS);

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

				Vector3 posPrev = PreviousTransform.localPosition;
				Vector3 posCurrent = RenderChain.ConnectedMarkers[i].MarkerObj.Tr.localPosition;

				/*Vector3 perpendicular = (new Vector3(posCurrent.z, posPrev.x, nearClip) -
									 new Vector3(posPrev.z, posCurrent.x, nearClip)).normalized * LineWidth;*/


				Vector3 v1 = cam.WorldToScreenPoint(posPrev);
				Vector3 v2 = cam.WorldToScreenPoint(posCurrent);

				if (v1.z < 0)
					v1.z *= -1;
				if (v2.z < 0)
					v2.z *= -1;

				//v1.z = nearClip;
				//v2.z = nearClip;

				//Vector3 perpendicular = Vector3.Cross((v2 - v1).normalized, Vector3.forward) * LineWidth;

				/*Vector3 perpendicular = (new Vector3(v2.y, v1.x, nearClip) -
									 new Vector3(v1.y, v2.x, nearClip)).normalized * LineWidth;*/


				Plane plane = new Plane(CameraControler.CamForward, CameraControler.CamPos);
				float dist1 = 2f * (posPrev - CameraControler.CamPos).magnitude * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * LineWidth * (1f / Screen.height);
				float dist2 = 2f * (posCurrent - CameraControler.CamPos).magnitude * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * LineWidth * (1f / Screen.height);

				/*GL.Vertex(cam.ScreenToWorldPoint(v1 + perpendicular));
				GL.Vertex(cam.ScreenToWorldPoint(v1 - perpendicular));
				GL.Vertex(cam.ScreenToWorldPoint(v2 - perpendicular));
				GL.Vertex(cam.ScreenToWorldPoint(v2 + perpendicular));*/

				Vector3 perpendicular = Vector3.Cross((posCurrent - posPrev).normalized, cam.transform.forward).normalized;

				GL.Vertex(posPrev + perpendicular * dist1);
				GL.Vertex(posPrev - perpendicular * dist1);
				GL.Vertex(posCurrent - perpendicular * dist2);
				GL.Vertex(posCurrent + perpendicular * dist2);

				PreviousTransform = RenderChain.ConnectedMarkers[i].MarkerObj.Tr;

			}

			GL.End();

		}



		GL.PopMatrix();
	}
}
