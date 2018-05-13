using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewTex : MonoBehaviour {

	public Camera Cam;
	public RenderTexture RT;
	public Color Empty;

	static bool RenderingPreview = false;
	public static bool IsPreview
	{
		get
		{
			return RenderingPreview;
		}
	}

	public static void ForcePreviewMode(bool on)
	{
		RenderingPreview = on;
	}
	
	public Texture2D RenderPreview(float HeightOffset = 0, int Width = 256, int Height = 256, bool Flip = true) {
		RenderingPreview = true;

		bool Slope = ScmapEditor.Current.Slope;
		bool Grid = ScmapEditor.Current.Grid;

		if (Slope)
		ScmapEditor.Current.ToogleSlope(false);
		if(Grid)
		ScmapEditor.Current.ToogleGrid(false);

		Vector3 CamPos = MapLuaParser.Current.MapCenterPoint;
		float Size = Mathf.Max(ScmapEditor.Current.map.Width, ScmapEditor.Current.map.Height) * 0.1f;
		float distance = Size * (2.0f * Mathf.Tan(0.5f * Cam.fieldOfView * Mathf.Deg2Rad));

		distance = (Size) * 0.5f / Mathf.Tan(Cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

		CamPos.y = distance + HeightOffset;
		transform.position = CamPos;

		

		RenderTexture currentActiveRT = RenderTexture.active;
		RenderTexture.active = Cam.targetTexture;

		float LastLodBias = QualitySettings.lodBias;
		QualitySettings.lodBias = 100000;
		// -->
		Cam.Render();
		// <--
		QualitySettings.lodBias = LastLodBias;


		// Texture
		Texture2D PreviewRender = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

		Color[] Colors = new Color[Width * Height];
		for(int i = 0; i < Colors.Length; i++)
		{
			Colors[i] = Empty;
		}
		PreviewRender.SetPixels(Colors);
		PreviewRender.Apply();
		PreviewRender.ReadPixels(new Rect(0, 0, PreviewRender.width, PreviewRender.height), 0, 0, false);
		PreviewRender.Apply();

		RenderTexture.active = currentActiveRT;

		if(Slope)
		ScmapEditor.Current.ToogleSlope(Slope);
		if(Grid)
		ScmapEditor.Current.ToogleGrid(Grid);

		RenderingPreview = false;

		if (Flip)
			return TextureFlip.FlipTextureVertical(PreviewRender, false);
		else
			return PreviewRender;

	}


}
