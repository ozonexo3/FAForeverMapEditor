using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MapLuaParser : MonoBehaviour
{

	static int More = 1;
	static string[] Args;
	void Start () {
		Args = System.Environment.GetCommandLineArgs();
		if (Args.Length > 0)

			if (Args.Length == 3 && Args[1] == "-setInstalationPath")
			{
				EnvPaths.SetInstalationPath(Args[2]);
				//Debug.Log("Success! Instalation path changed to: " + Args[2]);
			}


		if (Args.Length == 6 + More && Args[1 + More] == "-renderPreviewImage")
		{
			GetGamedataFile.MipmapBias = -0.9f;
			StartCoroutine("RenderImageAndClose");
		}
	}




	public IEnumerator RenderImageAndClose()
	{
		var LoadScmapFile = MapLuaParser.Current.StartCoroutine("ForceLoadMapAtPath", Args[4 + More]);
		yield return LoadScmapFile;


		int Widht = int.Parse(Args[2 + More]);
		int Height = int.Parse(Args[3 + More]);

		CameraControler.Current.RestartCam();

		CameraControler.Current.RenderCamera(Widht, Height, Args[5 + More]);

		Debug.Log("Success! Preview rendered to: " + Args[5 + More]);

		Application.Quit();
	}

}
