using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OzoneDecals
{
	public partial class OzoneDecalRenderer : MonoBehaviour
	{

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
					cam.AddCommandBuffer(evt, buffer);
				}
			}
		}

	}
}
