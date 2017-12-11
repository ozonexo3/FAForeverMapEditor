using UnityEngine;
using System.Collections.Generic;
 
[RequireComponent(typeof(CanvasRenderer))]
[ExecuteInEditMode]
public class UI3DMesh : MonoBehaviour
{
    public Mesh TheMesh;
    public Material Material;
 
    void ResetData ()
    {
        var cr = GetComponent<CanvasRenderer>();
        cr.SetMesh (TheMesh);
		//TheMesh.bounds.size.magnitude

		cr.materialCount = 1;
 
        cr.SetMaterial (Material, 0);
    }
 
    void OnEnable()
    {
        ResetData();
    }
 
    void OnValidate()
    {
        ResetData();
    }
}