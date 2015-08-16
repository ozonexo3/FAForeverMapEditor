using UnityEngine;
using System.Collections;

public class GridCamera : MonoBehaviour {

	public 	Texture		GridTexture;
	public	Material	GridMaterial;

	// Use this for initialization
	void Start () {
		GridTexture.mipMapBias = -0.3f;
	}
	
	// Update is called once per frame
	void Update () {
		GridMaterial.SetFloat("_GridCamDist", transform.localPosition.y / 20);
	}
}
