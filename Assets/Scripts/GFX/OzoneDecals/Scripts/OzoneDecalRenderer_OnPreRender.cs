using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using EditMap;

#pragma warning disable 0162
namespace OzoneDecals
{
	public partial class OzoneDecalRenderer : MonoBehaviour
	{

		static int _NearCutOffLOD = Shader.PropertyToID("_NearCutOffLOD");
		static int _CutOffLOD = Shader.PropertyToID("_CutOffLOD");
		static int copy2id = Shader.PropertyToID("_CameraGBufferTexture2Copy");

		public bool UseInstancing = false;

		public bool DrawAlbedo = false;
		public bool DrawTermacs = false;
		public bool DrawDetails = false;

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

			int DecalsAlbedoCount = _DecalsAlbedo.Count;
			int DecalsCount = _Decals.Count;
			int DecalsTarmacs = _DecalsTarmacs.Count;

			if (DecalsInfo.Current)
			{
				int ScreenDecalsCount = DecalsAlbedoCount + DecalsCount + DecalsTarmacs;
				DecalsInfo.Current.UpdateScreenCount(ScreenDecalsCount);
			}

			//if (DecalsCount == 0 && DecalsAlbedoCount == 0 && DecalsTarmacs == 0)
			//	return;

			CreateBuffer(ref _bufferDeferred, cam, _Name, _camEvent);

			//UseInstancing = false;

			_bufferDeferred.Clear();
			//if(DecalsAlbedoCount > 0 && DrawAlbedo)
			DrawDeferredDecals_Albedo(cam);
			//if(DecalsTarmacs > 0 && DrawTermacs)
			DrawDeferredDecals_Tarmacs(cam);
			//if(DecalsCount > 0 && DrawDetails)
			//DrawDeferredDecals_Normal(cam);
			DrawDeferredDecals_Normals_Sorted(cam);


			// Clear 
			/*
			var decalEnum = _Decals.GetEnumerator();
			while (decalEnum.MoveNext())
				decalEnum.Current.Value.Clear();

			_DecalsAlbedo.Clear();
			_DecalsTarmacs.Clear();
			*/
		}

		const bool AllowAlbedoInstancing = false;
		private void DrawDeferredDecals_Albedo(Camera cam)
		{
			_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

			OzoneDecal decal = null;
			for (int i = 0; i < AlbedoCount; i++)
			{
				if (AlbedoArray[i] == null)
					continue;

				decal = AlbedoArray[i];

				if (decal.Dec.Shared.DrawAlbedo && decal.IsVisible)
				{
					_directBlock.Clear();
					_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
					_directBlock.SetFloat(_CutOffLOD, decal.CutOff);

					_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, decal.Material, 0, 0, _directBlock);
				}
			}

			var decalListEnum = _DecalsAlbedo.GetEnumerator();
			while (decalListEnum.MoveNext())
			{
				decal = decalListEnum.Current;

				if (decal.Dec.Shared.DrawAlbedo)
				{
					_directBlock.Clear();
					_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
					_directBlock.SetFloat(_CutOffLOD, decal.CutOff);

					_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, decal.Material, 0, 0, _directBlock);
				}
			}
			decalListEnum.Dispose();
		}

		private void DrawDeferredDecals_Tarmacs(Camera cam)
		{
			_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

			var decalListEnum = _DecalsTarmacs.GetEnumerator();
			while (decalListEnum.MoveNext())
			{
				OzoneDecal decal = decalListEnum.Current;

				if (decal.Dec.Shared.DrawAlbedo)
				{
					_directBlock.Clear();
					_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
					_directBlock.SetFloat(_CutOffLOD, decal.CutOff);

					_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, decal.Material, 0, 0, _directBlock);
				}
			}
			decalListEnum.Dispose();
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
							_matrices[n] = decal.localToWorldMatrix;
							_NearCutOffLODValues[n] = decal.NearCutOff;
							_CutOffLODValues[n] = decal.CutOff * CutoffMultiplier;
							++n;

							if (n == 1023)
							{
								//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer0, copy1id);
								//_bufferDeferred.Blit(BuiltinRenderTextureType.CameraTarget, copy2id);
								//_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

								_instancedBlock.Clear();
								_instancedBlock.SetFloatArray(_NearCutOffLOD, _NearCutOffLODValues);
								_instancedBlock.SetFloatArray(_CutOffLOD, _CutOffLODValues);
								_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 0, _matrices, n, _instancedBlock);
								n = 0;
							}
						}
						else
						{
							//if(n == 0)
								//_bufferDeferred.Blit(BuiltinRenderTextureType.CameraTarget, copy2id);

							_directBlock.Clear();
							_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
							_directBlock.SetFloat(_CutOffLOD, decal.CutOff);

							_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, material, 0, 0, _directBlock);
						}
					}
				}

				if (UseInstancing && n > 0 && AllowAlbedoInstancing)
				{
					//_bufferDeferred.Blit(BuiltinRenderTextureType.CameraTarget, copy2id);
					//_bufferDeferred.SetRenderTarget(_albedoRenderTarget, BuiltinRenderTextureType.CameraTarget);

					_instancedBlock.Clear();
					_instancedBlock.SetFloatArray(_NearCutOffLOD, _NearCutOffLODValues);
					_instancedBlock.SetFloatArray(_CutOffLOD, _CutOffLODValues);
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
			//var copy2id = Shader.PropertyToID("_CameraGBufferTexture2Copy");
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

					if (decal.Dec.Shared.DrawNormal)
					{
						if (hqCount < HightQualityMaxCount)
						{
							// Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
							//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
							_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

							_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
							_directBlock.Clear();
							_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
							_directBlock.SetFloat(_CutOffLOD, decal.CutOff);
							_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, material, 0, 1, _directBlock);
							hqCount++;
						}
						else
						{
							if (UseInstancing)
							{
								// Instanced drawing
								_matrices[n] = decal.localToWorldMatrix;
								_CutOffLODValues[n] = decal.CutOff * CutoffMultiplier;
								_NearCutOffLODValues[n] = decal.NearCutOff;
								++n;

								if (n == 1023)
								{
									//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
									//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

									_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);
									_instancedBlock.Clear();
									_instancedBlock.SetFloatArray(_NearCutOffLOD, _NearCutOffLODValues);
									_instancedBlock.SetFloatArray(_CutOffLOD, _CutOffLODValues);
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
								_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff * CutoffMultiplier);
								_directBlock.SetFloat(_CutOffLOD, decal.CutOff);

								_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, material, 0, 1, _directBlock);
								++n;
							}
						}
					}
				}
				decalListEnum.Dispose();

#if UNITY_5_5_OR_NEWER
				if (UseInstancing && n > 0)
				{
					// Create of copy of GBuffer1 (specular / smoothness) and GBuffer 2 (normal)
					//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);
					//_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

					_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);

					_instancedBlock.Clear();
					_instancedBlock.SetFloatArray(_NearCutOffLOD, _NearCutOffLODValues);
					_instancedBlock.SetFloatArray(_CutOffLOD, _CutOffLODValues);
					_bufferDeferred.DrawMeshInstanced(_cubeMesh, 0, material, 1, _matrices, n, _instancedBlock);
				}
#endif
			}
			allDecalEnum.Dispose();
		}


		private void DrawDeferredDecals_Normals_Sorted(Camera cam)
		{
			_bufferDeferred.GetTemporaryRT(copy2id, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
			_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer2, copy2id);

			_bufferDeferred.SetRenderTarget(_normalRenderTarget, BuiltinRenderTextureType.CameraTarget);


			OzoneDecal decal = null;
			for (int i = 0; i < NormalCount; i++)
			{
				if (NormalArray[i] == null)
					continue;

				decal = NormalArray[i];

				if (decal.Dec.Shared.DrawNormal && decal.IsVisible)
				{
					_directBlock.Clear();
					_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
					_directBlock.SetFloat(_CutOffLOD, decal.CutOff);

					_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, decal.Material, 0, 1, _directBlock);
				}
			}

			//Creation
			var decalListEnum = _DecalsNormal.GetEnumerator();
			while (decalListEnum.MoveNext())
			{
				decal = decalListEnum.Current;

				_directBlock.Clear();
				_directBlock.SetFloat(_NearCutOffLOD, decal.NearCutOff);
				_directBlock.SetFloat(_CutOffLOD, decal.CutOff);
				_bufferDeferred.DrawMesh(_cubeMesh, decal.localToWorldMatrix, decal.Material, 0, 1, _directBlock);
			}
			decalListEnum.Dispose();
		}

		public static float DecalDist(Transform tr)
		{
			//return tr.localPosition - Current
			return Current._camTr.InverseTransformPoint(tr.position).z;
		}

	}
}
