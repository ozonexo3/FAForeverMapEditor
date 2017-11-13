using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using EditMap;

namespace OzoneDecals
{
	public partial class OzoneDecalRenderer : MonoBehaviour
	{

		public bool UseInstancing = false;

		void OnPreRender()
		{
			if (DecalsInfo.LoadingDecals)
				return;


#if UNITY_EDITOR
			if (_cubeMesh == null)
				return;
#endif

			if (_albedoRenderTarget == null || _camera.allowHDR != _camLastKnownHDR)
			{
				_camLastKnownHDR = _camera.allowHDR;
				_albedoRenderTarget = new RenderTargetIdentifier[] { BuiltinRenderTextureType.GBuffer0,
					_camLastKnownHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3 };
			}


			CreateBuffer(ref _bufferDeferred, _camera, _Name, _camEvent);

			UseInstancing = false;
			_bufferDeferred.Clear();
			DrawDeferredDecals_Albedo(_camera);
			DrawDeferredDecals_Normal(_camera);

			// Clear 
			var decalEnum = _Decals.GetEnumerator();
			while (decalEnum.MoveNext())
				decalEnum.Current.Value.Clear();

		}

		private void DrawDeferredDecals_Albedo(Camera cam)
		{
			if (_Decals.Count == 0)
				return;

			_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

			var allDecalEnum = _Decals.GetEnumerator();
			while (allDecalEnum.MoveNext())
			{
				Material material = allDecalEnum.Current.Key;
				HashSet<OzoneDecal> decals = allDecalEnum.Current.Value;
				int decalCount = decals.Count;
				int n = 0;

				var decalListEnum = decals.GetEnumerator();
				while (decalListEnum.MoveNext())
				{
					OzoneDecal decal = decalListEnum.Current;
					if (decal != null && decal.DrawAlbedo)
					{
#if UNITY_5_5_OR_NEWER
						if (UseInstancing)
						{
							_matrices[n] = decal.tr.localToWorldMatrix;
							++n;

							if (n == 1023)
							{
								_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n);
								n = 0;
							}
						}
						else
#endif
						{
							_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, material, 0, 0, _directBlock);
						}
					}
				}

#if UNITY_5_5_OR_NEWER
				if (UseInstancing && n > 0)
				{
					_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n);
				}
#endif
			}
		}

		private void DrawDeferredDecals_Normal(Camera cam)
		{
			if (_Decals.Count == 0)
				return;

			// Specular
			//var copy1id = Shader.PropertyToID("_CameraGBufferTexture1Copy");
			//_bufferDeferred.GetTemporaryRT(copy1id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

			// Normal
			var copy2id = Shader.PropertyToID("_CameraGBufferTexture2Copy");
			_bufferDeferred.GetTemporaryRT(copy2id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

			var allDecalEnum = _Decals.GetEnumerator();
			while (allDecalEnum.MoveNext())
			{
				Material material = allDecalEnum.Current.Key;
				HashSet<OzoneDecal> decals = allDecalEnum.Current.Value;
				int n = 0;

				var decalListEnum = decals.GetEnumerator();
				while (decalListEnum.MoveNext())
				{
					OzoneDecal decal = decalListEnum.Current;
					if (decal != null && decal.DrawNormal)
					{
						/*
						if (false)
						{
							// Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
							//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
							_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

							_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
							_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, material, 0, 1); 
						}
						else
						*/
						{
							if (UseInstancing)
							{
								// Instanced drawing
								_matrices[n] = decal.tr.localToWorldMatrix;
								++n;

								if (n == 1023)
								{
									//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
									_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

									_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
									_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n); 
									n = 0;
								}
							}
							else
							{
								if (n == 0)
								{
									// Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
									//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
									_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);
								}

								_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
								_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, material, 0, 1);
								++n;
							}
						}
					}
				}

#if UNITY_5_5_OR_NEWER
				if (UseInstancing && n > 0)
				{
					// Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
					//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
					_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

					_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);

					_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n);
				}
#endif
			}
		}

	}
}
