using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MapLuaParser : MonoBehaviour
{

	static string[] Args;
	void Start () {
		Args = System.Environment.GetCommandLineArgs();

		/*
		for(int i = 0; i < Args.Length; i++)
		{
			Debug.Log(Args[i]);
		}
		*/

		if (Args.Length > 0)

			if (Args.Length == 3 && Args[1] == "-setInstalationPath")
			{
				EnvPaths.SetInstalationPath(Args[2]);
				//Debug.Log("Success! Instalation path changed to: " + Args[2]);
			}


		//Debug.Log(Args.Length);
		//Debug.Log(Args[0]);
		if (Args.Length >= 6)
		{
			if (Args[1] == "-renderPreviewImage" || Args[1] == "-renderPreviewImageNoProps" || Args[1] == "-renderPreviewImageNoDecals" || Args[1] == "-renderPreviewImageNoPropsDecals")
			{

				GetGamedataFile.MipmapBias = -0.9f;

				bool Props = Args[1] == "-renderPreviewImage" || Args[1] == "-renderPreviewImageNoDecals";
				bool Decals = Args[1] == "-renderPreviewImage" || Args[1] == "-renderPreviewImageNoProps";

				int Widht = int.Parse(Args[2]);
				int Height = int.Parse(Args[3]);
				Debug.Log("Begin coroutine");
				StartCoroutine(RenderImageAndClose(Props, Decals, Widht, Height, Args[4], Args[5]));
			}
		}
	}




	public IEnumerator RenderImageAndClose(bool Props, bool Decals, int Width, int Height, string MapPath, string ImagePath)
	{
		var LoadScmapFile = MapLuaParser.Current.StartCoroutine(ForceLoadMapAtPath(MapPath, Props, Decals));
		yield return LoadScmapFile;

		QualitySettings.lodBias = 100000;
		QualitySettings.shadowDistance = CameraControler.Current.transform.localPosition.y * 1.1f;

		if (Decals)
		{
			OzoneDecals.OzoneDecalRenderer.CutoffMultiplier = 100000;


		}

		CameraControler.Current.RestartCam(true);
		yield return null;
		CameraControler.Current.RenderCamera(Width, Height, ImagePath);
		//ScmapEditor.Current.PreviewRenderer.RenderPreview()

		Debug.Log("Success! Preview rendered to: " + ImagePath);

		OnFafEditorQuit.ForceQuit();
	}

}
