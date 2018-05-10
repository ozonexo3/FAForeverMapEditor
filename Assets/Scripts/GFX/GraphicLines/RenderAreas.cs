using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderAreas : MonoBehaviour {

	public Camera RenderCamera;
	public Material Mat;

	public void OnRenderObject()
	{
		if (PreviewTex.IsPreview)
			return;

		if (Camera.current != RenderCamera || ScmapEditor.Current == null || ScmapEditor.Current.Teren == null)
			return;

		if (MapLuaParser.Current.SaveLuaFile.Data.areas.Length == 0)
			return;
		float Height = ScmapEditor.GetWaterLevel();

		GL.PushMatrix();
		// Set transformation matrix for drawing to
		// match our transform
		GL.MultMatrix(transform.localToWorldMatrix);

		Mat.SetPass(0);

		for (int a = 0; a < MapLuaParser.Current.SaveLuaFile.Data.areas.Length; a++)
		{
			if (MapLuaParser.Current.SaveLuaFile.Data.areas[a] == AreaInfo.SelectedArea)
				continue;

			GL.Begin(GL.LINES);

			Vector3 Pos0 = ScmapEditor.ScmapPosToWorld(new Vector3(MapLuaParser.Current.SaveLuaFile.Data.areas[a].rectangle.x, 0, MapLuaParser.Current.SaveLuaFile.Data.areas[a].rectangle.y));
			Pos0.y = ScmapEditor.Current.Teren.SampleHeight(Pos0);
			Vector3 Pos1 = ScmapEditor.ScmapPosToWorld(new Vector3(MapLuaParser.Current.SaveLuaFile.Data.areas[a].rectangle.width, 10, MapLuaParser.Current.SaveLuaFile.Data.areas[a].rectangle.height));
			Pos1.y = ScmapEditor.Current.Teren.SampleHeight(Pos1);

			Pos0.y = Mathf.Lerp(Pos0.y, Pos1.y, 0.5f);
			if (Pos0.y < Height)
				Pos0.y = Height;
			Pos1.y = Pos0.y;

			GL.Vertex(Pos0);
			GL.Vertex3(Pos0.x, Pos0.y, Pos1.z);

			GL.Vertex3(Pos0.x, Pos0.y, Pos1.z);
			GL.Vertex(Pos1);

			GL.Vertex(Pos1);
			GL.Vertex3(Pos1.x, Pos0.y, Pos0.z);

			GL.Vertex3(Pos1.x, Pos0.y, Pos0.z);
			GL.Vertex(Pos0);

			GL.End();
		}

		GL.PopMatrix();

	}


}
