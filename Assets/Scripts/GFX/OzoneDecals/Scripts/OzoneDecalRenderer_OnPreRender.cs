using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using EditMap;

#pragma warning disable 0162
namespace OzoneDecals
{
	public partial class OzoneDecalRenderer : MonoBehaviour
	{

		public bool UseInstancing = false;

		void OnPreRender()
		{
			if (DecalsInfo.LoadingDecals)
				return;

			if (RenderCamera == null)
				return;

			RenderForCamera(RenderCamera);
		}

		private void RenderForCamera(Camera cam)
		{
#if UNITY_EDITOR
			if (_cubeMesh == null)
				return;
#endif


			if (DecalsInfo.Current)
			{
				int ScreenDecalsCount = _DecalsAlbedo.Count + _Decals.Count;
				DecalsInfo.Current.UpdateScreenCount(ScreenDecalsCount);
			}

			if (_Decals.Count == 0 && _DecalsAlbedo.Count == 0)
				return;

			CreateBuffer(ref _bufferDeferred, cam, _Name, _camEvent);

			//UseInstancing = false;


			_bufferDeferred.Clear();
			DrawDeferredDecals_Albedo(cam);
			DrawDeferredDecals_Normal(cam);


			// Clear 
			var decalEnum = _Decals.GetEnumerator();
			while (decalEnum.MoveNext())
				decalEnum.Current.Value.Clear();

			_DecalsAlbedo.Clear();

		}

		const bool AllowAlbedoInstancing = false;
		private void DrawDeferredDecals_Albedo(Camera cam)
		{
			_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

			var decalListEnum = _DecalsAlbedo.GetEnumerator();
			while (decalListEnum.MoveNext())
			{
				OzoneDecal decal = decalListEnum.Current;

				if (decal != null && decal.Dec.Shared.DrawAlbedo)
				{
					_directBlock.Clear();
					_directBlock.SetFloat("_NearCutOffLOD", decal.NearCutOff);
					_directBlock.SetFloat("_CutOffLOD", decal.CutOff);

					_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, decal.Material, 0, 0, _directBlock);
				}
			}
		}

		public static int CutoffMultiplier = 1;

		private void DrawDeferredDecals_AlbedoInstanced(Camera cam)
		{
			_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

			//var copy1id = Shader.PropertyToID("_CameraGBufferTexture0Copy");
			//_bufferDeferred.GetTemporaryRT(copy1id, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
			//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer0, copy1id);

			//var copy2id = Shader.PropertyToID("_CameraGBufferTexture4Copy");
			//_bufferDeferred.GetTemporaryRT(copy2id, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);

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

					if (decal != null && decal.Dec.Shared.DrawAlbedo)
					{
						if (UseInstancing && AllowAlbedoInstancing)
						{
							_matrices[n] = decal.tr.localToWorldMatrix;
							_NearCutOffLODValues[n] = decal.NearCutOff;
							_CutOffLODValues[n] = decal.CutOff * CutoffMultiplier;
							++n;

							if (n == 1023)
							{
								//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer0, copy1id);
								//_bufferDeferred.Blit(BuiltinRenderTextureType.CameraTarget, copy2id);
								//_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

								_instancedBlock.Clear();
								_instancedBlock.SetFloatArray("_NearCutOffLOD", _NearCutOffLODValues);
								_instancedBlock.SetFloatArray("_CutOffLOD", _CutOffLODValues);
								_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);
								n = 0;
							}
						}
						else
						{
							//if(n == 0)
								//_bufferDeferred.Blit(BuiltinRenderTextureType.CameraTarget, copy2id);

							_directBlock.Clear();
							_directBlock.SetFloat("_NearCutOffLOD", decal.NearCutOff);
							_directBlock.SetFloat("_CutOffLOD", decal.CutOff);

							_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, material, 0, 0, _directBlock);
						}
					}
				}

				if (UseInstancing && n > 0 && AllowAlbedoInstancing)
				{
					//_bufferDeferred.Blit(BuiltinRenderTextureType.CameraTarget, copy2id);
					//_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

					_instancedBlock.Clear();
					_instancedBlock.SetFloatArray("_NearCutOffLOD", _NearCutOffLODValues);
					_instancedBlock.SetFloatArray("_CutOffLOD", _CutOffLODValues);
					_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);
				}
			}
		}

		const float HightQualityBlendingDistance = 15;
		const int HightQualityMaxCount = 0;


		private void DrawDeferredDecals_Normal(Camera cam)
		{

			int hqCount = 0;

			// Specular
			//var copy1id = Shader.PropertyToID("_CameraGBufferTexture1Copy");
			//_bufferDeferred.GetTemporaryRT(copy1id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

			// Normal
			var copy2id = Shader.PropertyToID("_CameraGBufferTexture2Copy");
			_bufferDeferred.GetTemporaryRT(copy2id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

			_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

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

					if (decal != null && decal.Dec.Shared.DrawNormal)
					{
						if (hqCount < HightQualityMaxCount)
						{
							// Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
							//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
							_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

							_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
							_directBlock.Clear();
							_directBlock.SetFloat("_NearCutOffLOD", decal.NearCutOff);
							_directBlock.SetFloat("_CutOffLOD", decal.CutOff);
							_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, material, 0, 1, _directBlock);
							hqCount++;
						}
						else
						{
							if (UseInstancing)
							{
								// Instanced drawing
								_matrices[n] = decal.tr.localToWorldMatrix;
								_CutOffLODValues[n] = decal.CutOff * CutoffMultiplier;
								_NearCutOffLODValues[n] = decal.NearCutOff;
								++n;

								if (n == 1023)
								{
									//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
									//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

									_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
									_instancedBlock.Clear();
									_instancedBlock.SetFloatArray("_NearCutOffLOD", _NearCutOffLODValues);
									_instancedBlock.SetFloatArray("_CutOffLOD", _CutOffLODValues);
									_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _instancedBlock);
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
								_directBlock.Clear();
								_directBlock.SetFloat("_NearCutOffLOD", decal.NearCutOff * CutoffMultiplier);
								_directBlock.SetFloat("_CutOffLOD", decal.CutOff);

								_bufferDeferred.DrawMesh(_cubeMesh, decal.tr.localToWorldMatrix, material, 0, 1, _directBlock);
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
					//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

					_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);

					_instancedBlock.Clear();
					_instancedBlock.SetFloatArray("_NearCutOffLOD", _NearCutOffLODValues);
					_instancedBlock.SetFloatArray("_CutOffLOD", _CutOffLODValues);
					_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _instancedBlock);
				}
#endif
			}
		}


		public static float DecalDist(Transform tr)
		{
			//return tr.localPosition - Current
			return Current._camTr.InverseTransformPoint(tr.position).z;
		}

	}
}
