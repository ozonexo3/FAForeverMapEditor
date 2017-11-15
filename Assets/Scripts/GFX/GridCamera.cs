using UnityEngine;
using System.Collections;

public class GridCamera : MonoBehaviour {

	public 	Texture		GridTexture;
	public	Material	GridMaterial;

	// Use this for initialization
	void Start () {
		GridTexture.mipMapBias = -0.3f;
		GridTexture.filterMode = FilterMode.Bilinear;
		GridTexture.anisoLevel = 2;
		InvokeRepeating("UpdateGrid", 0, 0.333f);
	}
	
	// Update is called once per frame
	void UpdateGrid () {
		GridMaterial.SetFloat("_GridCamDist", transform.localPosition.y / 20);
	}
}
