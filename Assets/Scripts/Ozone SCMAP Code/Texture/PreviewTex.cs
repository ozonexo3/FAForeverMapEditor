using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewTex : MonoBehaviour {

	public Camera Cam;
	public RenderTexture RT;
	
	public Texture2D RenderPreview() {

		Vector3 CamPos = MapLuaParser.Current.MapCenterPoint;
		float Size = Mathf.Max(ScmapEditor.Current.map.Width, ScmapEditor.Current.map.Height) * 0.1f;
		float distance = Size * (2.0f * Mathf.Tan(0.5f * Cam.fieldOfView * Mathf.Deg2Rad));
		CamPos.y = distance * 2;
		transform.position = CamPos;

		Cam.Render();

		RenderTexture currentActiveRT = RenderTexture.active;

		RenderTexture.active = RT;

		Texture2D PreviewRender = new Texture2D(256, 256, TextureFormat.RGBA32, false);
		PreviewRender.ReadPixels(new Rect(0, 0, PreviewRender.width, PreviewRender.height), 0, 0);
		PreviewRender.Apply();

		//Texture2D Preview = TextureLoader.ConvertToBGRA(PreviewRender);
		//Graphics.ConvertTexture(PreviewRender, Preview);

		//Preview = TextureFlip.FlipTextureVertical(Preview, false);
		//Preview.Compress(true);
		//Preview.Apply();

		RenderTexture.active = currentActiveRT;

		return TextureFlip.FlipTextureVertical(PreviewRender, false);

	}

}
