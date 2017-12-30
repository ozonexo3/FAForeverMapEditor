//********************************
// 
// * Procedural skybox mesh generator for FAF Map Editor 
// * Copyright ozonexo3 2017
//
//********************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSkybox : MonoBehaviour {

	public MeshFilter Mf;
	public Material Mat;

	SkyboxData SkyData;
	public void LoadSkybox()
	{
		SkyData = ScmapEditor.Current.map.AdditionalSkyboxData;

		CreateMesh();

		transform.position = ScmapEditor.ScmapPosToWorld(SkyData.Data.Position) + Vector3.up * (SkyData.Data.HorizonHeight * 0.1f);
		transform.localScale = Vector3.one * (SkyData.Data.Scale * 0.1f);

		Mat.SetFloat("_HorizonHeight", SkyData.Data.HorizonHeight * 0.1f);
		Mat.SetColor("_HorizonColor", SkyData.Data.HorizonColor);
		Mat.SetFloat("_ZenithHeight", SkyData.Data.ZenithHeight * 0.1f);
		Mat.SetColor("_ZenithColor", SkyData.Data.ZenithColor);
	}

	void CreateMesh()
	{
		Mesh newMesh = new Mesh();
		List<Vector3> Verts = new List<Vector3>();
		List<Vector2> Uvs = new List<Vector2>();
		List<int> Tris = new List<int>();

		float SphereLerp = 1f - ((SkyData.Data.SubtractHeight * 2) / Mathf.PI);

		Vector3[] Positions = new Vector3[SkyData.Data.SubdivisionsHeight];
		for (int h = 0; h < SkyData.Data.SubdivisionsHeight; h++)
		{
			float Lerp = h / ((float)SkyData.Data.SubdivisionsHeight - 1f);
			Positions[h] = Vector3.Lerp(
				new Vector3(1 - Lerp, 0, 0),
				Quaternion.Euler(Vector3.forward * Lerp * 90) * Vector3.right,
				SphereLerp);


		}


		for (int i = 0; i < SkyData.Data.SubdivisionsAxis; i++)
		{
			float AngleLerp = i / ((float)SkyData.Data.SubdivisionsAxis);
			for (int h = 0; h < SkyData.Data.SubdivisionsHeight; h++)
			{
				float Lerp = h / ((float)SkyData.Data.SubdivisionsHeight - 1f);

				Verts.Add(Quaternion.Euler(Vector3.up * (360 * AngleLerp)) * Positions[h]);
				Vector3 UvOffset = Quaternion.Euler(Vector3.forward * (360 * AngleLerp)) * (Vector3.right * (Lerp * 0.5f));
				Uvs.Add(new Vector2(0.5f + UvOffset.x, 0.5f + UvOffset.y));

				if(i > 0 && h > 0)
				{
					int CountOffset = i * SkyData.Data.SubdivisionsHeight;
					Tris.Add(CountOffset + h);
					Tris.Add(CountOffset + h - 1);
					Tris.Add(CountOffset + h - (SkyData.Data.SubdivisionsHeight + 1));

					Tris.Add(CountOffset + h);
					Tris.Add(CountOffset + h - (SkyData.Data.SubdivisionsHeight + 1));
					Tris.Add(CountOffset + h - (SkyData.Data.SubdivisionsHeight));
				}
			}
		}

		for (int h = 0; h < SkyData.Data.SubdivisionsHeight; h++)
		{
			if (h > 0)
			{
				int CountOffset = (SkyData.Data.SubdivisionsAxis - 1) * SkyData.Data.SubdivisionsHeight;
				Tris.Add(CountOffset + h);
				Tris.Add(CountOffset + h - (CountOffset + 1));
				Tris.Add(CountOffset + h - 1);
				

				Tris.Add(CountOffset + h);
				Tris.Add(CountOffset + h - (CountOffset));
				Tris.Add(CountOffset + h - (CountOffset + 1));
				
			}
		}


		newMesh.SetVertices(Verts);
		newMesh.SetTriangles(Tris, 0);
		newMesh.RecalculateNormals();
		Mf.sharedMesh = newMesh;
	}
}