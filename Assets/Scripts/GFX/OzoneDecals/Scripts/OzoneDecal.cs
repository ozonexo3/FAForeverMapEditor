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
		public bool CreationObject;

		public Material Material;

		public float WorldCutoffDistance;
		public float CutOff;
		public float NearCutOff;
		public float OwnerArmy;

		float _CutOffLOD = 50.0f;
		float _NearCutOffLOD = 0.0f;

		Decal _Dec;
		public Decal Dec
		{
			get
			{
				return _Dec;
			}
			set
			{
				_Dec = value;
			}

		}

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
		private Matrix4x4 _localToWorldMatrix;
		public Matrix4x4 localToWorldMatrix
		{
			get
			{
				if (CreationObject)
					return tr.localToWorldMatrix;
				return _localToWorldMatrix;
			}
			set
			{
				_localToWorldMatrix = value;
			}
		}

		void OnEnable()
		{
			tr = transform;
			UpdateMatrix();
			if (!CreationObject && Dec != null)
			DecalsControler.AddDecal(Dec);
		}

		public void UpdateMatrix()
		{
			localToWorldMatrix = tr.localToWorldMatrix;
		}

		private void OnDisable()
		{
			//if (!CreationObject && Dec != null)
			//	DecalsControler.RemoveDecal(Dec);
		}

		private void OnDestroy()
		{
			//if(CreationObject)
			DecalsControler.RemoveDecal(Dec);
			OnBecameInvisible();
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
			UpdateMatrix();
		}

		public static void SnapToGround(Transform tr, GameObject Connected)
		{
			Vector3 Pos = tr.TransformPoint(PivotPointLocal);
			Pos.y = ScmapEditor.Current.Teren.SampleHeight(Pos);
			tr.localPosition = tr.TransformPoint(tr.InverseTransformPoint(Pos) - PivotPointLocal);
			if (Connected != null)
			{
				OzoneDecal OD = Connected.GetComponent<OzoneDecal>();
				if (OD != null)
				{
					OD.tr.localPosition = tr.localPosition;
					OD.UpdateMatrix();
				}
			}
		}


		
		public void Bake()
		{
			_Dec.Type = _Dec.Shared.Type;
			_Dec.Position = ScmapEditor.WorldPosToScmap( GetPivotPoint());
			_Dec.Scale = tr.localScale * 10f;
			_Dec.Rotation = tr.localEulerAngles * Mathf.Deg2Rad;

			_Dec.CutOffLOD = CutOffLOD;
			_Dec.NearCutOffLOD = NearCutOffLOD;
			_Dec.TexPathes = new string[2];
			_Dec.Shared.FixPaths();

			_Dec.Shared.Tex1Path = GetGamedataFile.FixMapsPath(_Dec.Shared.Tex1Path);
			_Dec.Shared.Tex2Path = GetGamedataFile.FixMapsPath(_Dec.Shared.Tex2Path);

			_Dec.TexPathes[0] = _Dec.Shared.Tex1Path;
			_Dec.TexPathes[1] = _Dec.Shared.Tex2Path;
			_Dec.Obj = this;
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

		/*void OnWillRenderObject()
		{
			if(_Dec != null && _Dec.Shared != null)
			OzoneDecalRenderer.AddDecal(this); //, Camera.current
		}*/

		private void OnBecameVisible()
		{
			if (_Dec != null && _Dec.Shared != null)
				OzoneDecalRenderer.AddDecal(this); //, Camera.current
		}

		private void OnBecameInvisible()
		{
			if (_Dec != null && _Dec.Shared != null)
				OzoneDecalRenderer.RemoveDecal(this); //, Camera.current	
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
