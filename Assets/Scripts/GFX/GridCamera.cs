using UnityEngine;
using System.Collections;

public class GridCamera : MonoBehaviour {

	public 	Texture		GridTexture;
	//public	Material	GridMaterial;
	Transform tr;

	void Start () {
		GridTexture.mipMapBias = -0.3f;
		GridTexture.filterMode = FilterMode.Bilinear;
		GridTexture.anisoLevel = 2;
		tr = transform;
		//InvokeRepeating("UpdateGrid", 0, 0.333f);
		UpdateTimer = 0;
	}

	float LastDist = -10000;
	void UpdateGrid () {
		float dist = tr.localPosition.y / 20;

		if (dist != LastDist) {
			LastDist = dist;
			Shader.SetGlobalFloat("_GridCamDist", dist);
		}
	}

	const float UpdateStep = 0.25f;
	float UpdateTimer = 0;
	public void TryUpdateGrid(bool Forced = false)
	{
		UpdateTimer += Time.unscaledDeltaTime;

		if (UpdateTimer > UpdateStep)
		{
			UpdateGrid();
			UpdateTimer -= UpdateStep;

			if (UpdateTimer > UpdateStep)
				UpdateTimer = 0;
		}
	}
}
