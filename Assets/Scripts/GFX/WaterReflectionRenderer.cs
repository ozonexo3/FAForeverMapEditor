using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FX
{
	[ExecuteInEditMode]
	public class WaterReflectionRenderer : MonoBehaviour
	{

		public Transform MainCameraPivot;
		public Transform MainCameraTr;
		public Camera MainCamera;
		public Camera reflectionCamera;

		public float clipPlaneOffset = 0.07f;
		public int TextureSize = 512;
		public float MinRenderDeltaTime = 0.033333f;

		public Transform WaterLevel;
		public float RenderDistance = 50;

		static Vector3 pos = Vector3.zero;
		static Vector3 normal = Vector3.up;

		private static bool s_InsideWater;
		static RenderTexture RT;

		private void OnEnable()
		{
			if (RT == null || reflectionCamera.targetTexture == null)
			{
				CreateRT();
			}

			reflectionCamera.targetTexture = RT;
			Shader.SetGlobalTexture("_ReflectionTexture", (Texture)reflectionCamera.targetTexture);
			//Debug.Log( Shader.GetGlobalTexture("_ReflectionTexture"));
			reflectionCamera.enabled = false;

		}

		private void Update()
		{
			pos = MainCameraPivot.localPosition;
			pos.y = WaterLevel.localPosition.y;
			//transform.localPosition = pos;

			pos.x = 0;
			pos.z = 0;
		}

		public bool IsDirty = false;

		Vector3 LastReflectionPos = Vector3.zero;

#if !UNITY_EDITOR
		float Timer = 0;
#endif
		void OnWillRenderObject()
		{

			Camera cam = Camera.current;
			if (cam != MainCamera)
				return;

			// Safeguard from recursive water reflections.
			if (s_InsideWater)
			{
				return;
			}

#if !UNITY_EDITOR
			if (Application.isPlaying)
			{
				if (Time.realtimeSinceStartup - Timer >= MinRenderDeltaTime)
					Timer = Time.realtimeSinceStartup;
				else
				{
					return;
				}
			}
#endif


			s_InsideWater = true;
			float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
			Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

			Matrix4x4 reflection = Matrix4x4.zero;
			CalculateReflectionMatrix(ref reflection, reflectionPlane);
			Vector3 oldpos = MainCameraTr.position;
			//oldpos.y = 0;
			Vector3 newpos = reflection.MultiplyPoint(oldpos);

			if (LastReflectionPos.x != newpos.x && LastReflectionPos.z != newpos.z && LastReflectionPos.y != newpos.y)
				return;

			reflectionCamera.worldToCameraMatrix = MainCamera.worldToCameraMatrix * reflection;

			if (reflectionCamera.targetTexture == null || RT.width != TextureSize || RT.height != TextureSize)
			{
				CreateRT();

				reflectionCamera.targetTexture = RT;
				Shader.SetGlobalTexture("_ReflectionTexture", reflectionCamera.targetTexture);

			}

			Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
			reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

			reflectionCamera.cullingMatrix = MainCamera.projectionMatrix * MainCamera.worldToCameraMatrix;

			//float pos = reflectionCamera.transform.position.x;

			//float ReflPosX = reflectionCamera.transform.position.x;
			//reflectionCamera.farClipPlane = ReflPosX - (-RenderDistance);
			//reflectionCamera.nearClipPlane = ReflPosX - RenderDistance;

			reflectionCamera.transform.position = newpos;
			Vector3 euler = MainCameraTr.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

#if !UNITY_EDITOR
			QualitySettings.shadows = ShadowQuality.Disable;

#endif
			//QualitySettings.pixelLightCount = 0;
			GL.invertCulling = true;
			reflectionCamera.Render();
			GL.invertCulling = false;
#if !UNITY_EDITOR
			QualitySettings.shadows = ShadowQuality.All;
#endif

			s_InsideWater = false;
		}

		void CreateRT()
		{
			if (reflectionCamera.targetTexture != null)
				reflectionCamera.targetTexture.Release();

			RT = new RenderTexture(TextureSize, TextureSize, 16, RenderTextureFormat.ARGB32);
			RT.isPowerOfTwo = true;
		}

		Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 offsetPos = pos + normal * clipPlaneOffset;
			Matrix4x4 m = cam.worldToCameraMatrix;
			Vector3 cpos = m.MultiplyPoint(offsetPos);
			Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
		}

		// Calculates reflection matrix around the given plane
		static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
			reflectionMat.m01 = (-2F * plane[0] * plane[1]);
			reflectionMat.m02 = (-2F * plane[0] * plane[2]);
			reflectionMat.m03 = (-2F * plane[3] * plane[0]);

			reflectionMat.m10 = (-2F * plane[1] * plane[0]);
			reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
			reflectionMat.m12 = (-2F * plane[1] * plane[2]);
			reflectionMat.m13 = (-2F * plane[3] * plane[1]);

			reflectionMat.m20 = (-2F * plane[2] * plane[0]);
			reflectionMat.m21 = (-2F * plane[2] * plane[1]);
			reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
			reflectionMat.m23 = (-2F * plane[3] * plane[2]);

			reflectionMat.m30 = 0F;
			reflectionMat.m31 = 0F;
			reflectionMat.m32 = 0F;
			reflectionMat.m33 = 1F;
		}
	}
}