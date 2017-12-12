using UnityEngine;
using UnityEngine.Rendering;
using EditMap;

namespace OzoneDecals
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class OzoneDecal : MonoBehaviour
	{
		//public Decal Component;
		public Decal.DecalSharedSettings Shared;
		public bool CreationObject;

		public Material Material;

		public float WorldCutoffDistance;
		public float CutOff;
		public float NearCutOff;

		float _CutOffLOD = 50.0f;
		float _NearCutOffLOD = 0.0f;

		public float CutOffLOD
		{
			get
			{
				return _CutOffLOD;
			}
			set
			{
				_CutOffLOD = value;

				LOD[] Old = lg.GetLODs();
				Old[0].screenRelativeTransitionHeight = tr.localScale.z / DecalsInfo.FrustumHeightAtDistance(Mathf.Max(_CutOffLOD, 1) * 0.102f);
				lg.SetLODs(Old);

				WorldCutoffDistance = _CutOffLOD * 0.1f;
				CutOff = (_CutOffLOD - OzoneDecalRenderer.CameraNear) / OzoneDecalRenderer.CameraFar;
				CutOff *= 0.1f;
			}
		}

		public float NearCutOffLOD
		{
			get
			{
				return _NearCutOffLOD;
			}
			set
			{
				_NearCutOffLOD = value;
				NearCutOff = (_NearCutOffLOD) / OzoneDecalRenderer.CameraFar;
				NearCutOff *= 0.1f;
			}
		}

		[HideInInspector]
		public Transform tr;

		void OnEnable()
		{
			tr = transform;
			if(!CreationObject)
			DecalsControler.AddDecal(this);
		}

		private void OnDisable()
		{
			if (!CreationObject)
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
			Component.Type = Shared.Type;
			Component.Position = ScmapEditor.WorldPosToScmap( GetPivotPoint());
			Component.Scale = tr.localScale * 10f;
			Component.Rotation = tr.localEulerAngles * Mathf.Deg2Rad;

			Component.CutOffLOD = CutOffLOD;
			Component.NearCutOffLOD = NearCutOffLOD;
			Component.TexPathes = new string[2];
			Component.TexPathes[0] = Shared.Tex1Path;
			Component.TexPathes[1] = Shared.Tex2Path;

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
			OzoneDecalRenderer.AddDecal(this, Camera.current);
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
