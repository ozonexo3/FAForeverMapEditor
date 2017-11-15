using UnityEngine;
using UnityEngine.Rendering;

namespace OzoneDecals
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class OzoneDecal : MonoBehaviour
	{
		public Material Material;
		public bool DrawAlbedo = true;
		public bool DrawNormal = true;

		public float CutOffLOD;
		public float NearCutOffLOD;

		[HideInInspector]
		public Transform tr;

		void OnEnable()
		{
			tr = transform;
		}

		MeshFilter mf;
		MeshRenderer mr;

		private void Start()
		{
			//OzoneDecalRenderer.AddDecal(this, Camera.main);

		}

		void Reset()
		{
			mf = GetComponent<MeshFilter>();
			mf.sharedMesh = Resources.Load<Mesh>("DecalCube");

			mr = GetComponent<MeshRenderer>();
			mr.shadowCastingMode = ShadowCastingMode.Off;
			mr.receiveShadows = false;
			mr.materials = new Material[] { };
			mr.lightProbeUsage = LightProbeUsage.BlendProbes;
			mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}


		void OnWillRenderObject()
		{
			if (Material == null)
				return;

			if (Camera.current == null)
				return;

			OzoneDecalRenderer.AddDecal(this, Camera.current);
		}



#region Editor
		static Color colorSelected = new Color(1, 0.8f, 0.0f, 0.7f);
		static Color colorUnselected = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		static Color colorUnselectedFill = new Color(0, 0, 0, 0.0f);

		void OnDrawGizmos()
		{
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, 0.5f, transform.localScale.z));
			Gizmos.color = colorUnselectedFill;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);

			Gizmos.color = colorUnselected;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(transform.localScale.x, 0.5f, transform.localScale.z));
			Gizmos.color = colorSelected;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
#endregion
}
