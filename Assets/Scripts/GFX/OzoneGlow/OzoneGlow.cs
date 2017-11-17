using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class OzoneGlow : MonoBehaviour {

	protected CommandBuffer _bufferDeferred = null;
	protected Camera RenderCamera;
	protected const string _Name = "Ozone Glow";

	protected const CameraEvent _camEvent = CameraEvent.AfterImageEffectsOpaque;
	protected RenderTargetIdentifier[] _renderTarget;
	public Mesh _mesh;
	public Material _mat;

	private void OnEnable()
	{
		RenderCamera = GetComponent<Camera>();

		_renderTarget = new RenderTargetIdentifier[] { BuiltinRenderTextureType.CameraTarget };

	}

	private void OnDisable()
	{
		if (_bufferDeferred != null)
		{
			GetComponent<Camera>().RemoveCommandBuffer(_camEvent, _bufferDeferred);
			_bufferDeferred = null;
		}
	}


	private static void CreateBuffer(ref CommandBuffer buffer, Camera cam, string name, CameraEvent evt)
	{
		if (buffer == null)
		{
			// See if the camera already has a command buffer to avoid duplicates
			foreach (CommandBuffer existingCommandBuffer in cam.GetCommandBuffers(evt))
			{
				if (existingCommandBuffer.name == name)
				{
					buffer = existingCommandBuffer;
					break;
				}
			}

			// Not found? Create a new command buffer
			if (buffer == null)
			{
				buffer = new CommandBuffer();
				buffer.name = name;
				//buffer.
				cam.AddCommandBuffer(evt, buffer);
			}
		}
	}


	void OnPreRender()
	{
		if (RenderCamera == null || _mesh == null || _mat == null)
			return;

		CreateBuffer(ref _bufferDeferred, RenderCamera, _Name, _camEvent);
		_bufferDeferred.Clear();

		_bufferDeferred.SetRenderTarget(_renderTarget, BuiltinRenderTextureType.CameraTarget);

		var copy1id = Shader.PropertyToID("_CameraGBufferTexture0Copy");
		_bufferDeferred.GetTemporaryRT(copy1id, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
		_bufferDeferred.Blit(BuiltinRenderTextureType.GBuffer1, copy1id);

		_bufferDeferred.DrawMesh(_mesh, RenderCamera.transform.localToWorldMatrix, _mat, 0, 0);
	}
}