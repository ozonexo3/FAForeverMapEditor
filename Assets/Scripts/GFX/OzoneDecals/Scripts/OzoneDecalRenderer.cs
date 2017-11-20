using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OzoneDecals {
	[ExecuteInEditMode]
	public partial class OzoneDecalRenderer : MonoBehaviour {

		public static OzoneDecalRenderer Current;
		void Awake()
		{
			if (GetComponent<Camera>() == Camera.main)
				Current = this;
		}

		protected const string _Name = "Ozone Decals Rendering";

		protected CommandBuffer _bufferDeferred = null;
		protected Dictionary<Material, HashSet<OzoneDecal>> _Decals;
		protected HashSet<OzoneDecal> _DecalsAlbedo;
		protected List<OzoneDecal> _decalComponent;
		//protected List<MeshFilter> _meshFilterComponent;
		protected const CameraEvent _camEvent = CameraEvent.BeforeReflections;

		protected Camera RenderCamera;

		protected Transform _camTr;
		protected bool _camLastKnownHDR;
		protected static Mesh _cubeMesh = null;

		protected Matrix4x4[] _matrices;
		protected MaterialPropertyBlock _instancedBlock;
		protected MaterialPropertyBlock _directBlock;
		protected RenderTargetIdentifier[] _albedoRenderTarget;
		protected RenderTargetIdentifier[] _normalRenderTarget;
		protected Material _materialLimitToGameObjects;
		protected static Vector4[] _avCoeff = new Vector4[7];

		protected float[] _CutOffLODValues;
		protected float[] _NearCutOffLODValues;

		public Mesh cubeMesh;

		public static float CameraNear;
		public static float CameraFar;

		private void OnEnable()
		{
			RenderCamera = GetComponent<Camera>();
			if (RenderCamera == Camera.main)
			{
				Current = this;

				CameraNear = RenderCamera.nearClipPlane;
				CameraFar = RenderCamera.farClipPlane - CameraNear;
				_camTr = RenderCamera.transform;

			}

			_Decals = new Dictionary<Material, HashSet<OzoneDecal>>();
			_DecalsAlbedo = new HashSet<OzoneDecal>();
			_decalComponent = new List<OzoneDecal>();
			//_meshFilterComponent = new List<MeshFilter>();

			_matrices = new Matrix4x4[1023];
			_CutOffLODValues = new float[1023];
			_NearCutOffLODValues = new float[1023];

			_instancedBlock = new MaterialPropertyBlock();
			_directBlock = new MaterialPropertyBlock();
			

			//_cubeMesh = cubeMesh;
			_cubeMesh = Resources.Load<Mesh>("DecalCube");
			_normalRenderTarget = new RenderTargetIdentifier[] { BuiltinRenderTextureType.GBuffer1, BuiltinRenderTextureType.GBuffer2 };
			_albedoRenderTarget = new RenderTargetIdentifier[] { BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer1 }; // , 
		}

		private void OnDisable()
		{
			if (_bufferDeferred != null)
			{
				GetComponent<Camera>().RemoveCommandBuffer(_camEvent, _bufferDeferred);
				_bufferDeferred = null;
			}
		}

		public static void AddDecal(OzoneDecal d, Camera Cam)
		{
			if (Current == null || Cam == null)
				return;

#if UNITY_EDITOR
			if(Cam != Current.RenderCamera && Cam.name == "SceneCamera")
			{
				// Is Editor
				OzoneDecalRenderer renderer = Cam.GetComponent<OzoneDecalRenderer>();
				if (renderer == null)
					renderer = Cam.gameObject.AddComponent<OzoneDecalRenderer>();

				renderer.AddDecalToRenderer(d);
			}
			else
#endif
			{
				Current.AddDecalToRenderer(d);
			}

		}

		bool HasSpecular;
		bool HasEmission;

		void AddDecalToRenderer(OzoneDecal d)
		{
			HasEmission = HasEmission || d.HasEmission;

			if (d.DrawAlbedo)
			{
				_DecalsAlbedo.Add(d);
			}
			else
			{
				if (!_Decals.ContainsKey(d.Material))
				{
					_Decals.Add(d.Material, new HashSet<OzoneDecal>() { d });
				}
				else
				{
					_Decals[d.Material].Add(d);
				}
			}
		}


		public static void AddAlbedoDecal(OzoneDecal d)
		{
			//Current._DecalsAlbedo.Add(d);
		}

		public static void RemoveAlbedoDecal(OzoneDecal d)
		{
			//Current._DecalsAlbedo.Remove(d);
		}
	}
}
