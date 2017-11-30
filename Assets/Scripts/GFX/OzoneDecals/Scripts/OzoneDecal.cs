using UnityEngine;
using UnityEngine.Rendering;

namespace OzoneDecals
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class OzoneDecal : MonoBehaviour
	{
		//public Decal Component;
		public Decal.DecalSharedSettings Shared;

		public Material Material;
		//public bool DrawAlbedo = false;
		//public bool DrawNormal = false;
		//public bool HighQualityBlending;
		//public bool HasEmission;
		//public bool HasSpecular;

		//public float CutOffLOD;
		//public float NearCutOffLOD;
		//public float WorldCutoffDistance;
		//public float FrustumSize;

#if UNITY_EDITOR
		public string Text0Path = "";
		public string Text1Path = "";
#endif

		[HideInInspector]
		public float LastDistance = 0;

		[HideInInspector]
		public Transform tr;

		void OnEnable()
		{
			tr = transform;
			DecalsControler.AddDecal(this);
		}

		private void OnDisable()
		{
			DecalsControler.RemoveDecal(this);
		}

		MeshFilter mf;
		MeshRenderer mr;
		public LODGroup lg;


		
		static Vector3 PivotPointLocal = new Vector3(-0.5f, 0, 0.5f);

		public Vector3 GetPivotPoint()
		{
			return tr.TransformPoint(PivotPointLocal);
		}

		public void MovePivotPoint(Vector3 Pos)
		{
			tr.localPosition = tr.TransformPoint(tr.InverseTransformPoint(Pos) - PivotPointLocal);
		}

		public static void SnapToGround(Transform tr)
		{
			Vector3 Pos = tr.TransformPoint(PivotPointLocal);
			Pos.y = ScmapEditor.Current.Teren.SampleHeight(Pos);
			tr.localPosition = tr.TransformPoint(tr.InverseTransformPoint(Pos) - PivotPointLocal);
		}

		public Decal Bake()
		{
			Decal Component = new Decal();

			//TODO
			Component.Position = ScmapEditor.WorldPosToScmap( GetPivotPoint());
			Component.Scale = tr.localScale * 10f;
			Component.Rotation = tr.localEulerAngles * Mathf.Deg2Rad;

			//Component.CutOffLOD = WorldCutoffDistance * 10;
			return Component;
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

		public bool IsVisible;

		void OnWillRenderObject()
		{
			//if (Material == null)
			//	return;

			//if (Camera.current == null)
			//	return;

			//LastDistance = OzoneDecalRenderer.DecalDist(tr);

			//if (LastDistance > WorldCutoffDistance * WorldCutoffDistance)
			//	return;

			OzoneDecalRenderer.AddDecal(this, Camera.current);
			/*
			if (DrawAlbedo)
			{
				IsVisible = true;


			}
			else
			{

			}
			*/
		}



#region Editor
		static Color colorSelected = new Color(1, 0.8f, 0.0f, 0.7f);
		static Color colorUnselected = new Color(0.7f, 0.7f, 0.7f, 0.6f);
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
