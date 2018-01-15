using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class BuildWithResources : MonoBehaviour
{
	//[MenuItem("Build/Windows with resources (old)")]
	public static void BuildSettings()
	{
		// Get filename
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

		string[] levels = new string[] { "Assets/MapEditor.unity" };

		BuildPipeline.BuildPlayer(levels, path + "/FAForeverMapEditor.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

		// Copy structure files
		FileUtil.CopyFileOrDirectory("Structure", path + "/FAForeverMapEditor_Data/Structure");

		// Run the game (Process class from System.Diagnostics).
		//Process proc = new Process();
		//proc.StartInfo.FileName = path + "/FAForeverMapEditor.exe";
		//proc.Start();
	}

	[MenuItem("Build/Windows with resources (64bit)")]
	public static void BuildSettings64()
	{
		// Get filename
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

		string[] levels = new string[] { "Assets/MapEditor.unity" };

		BuildPipeline.BuildPlayer(levels, path + "/FAForeverMapEditor.exe", BuildTarget.StandaloneWindows64, BuildOptions.Il2CPP);

		// Copy structure files
		FileUtil.CopyFileOrDirectory("Structure", path + "/FAForeverMapEditor_Data/Structure");

		// Run the game (Process class from System.Diagnostics).
		//Process proc = new Process();
		//proc.StartInfo.FileName = path + "/FAForeverMapEditor.exe";
		//proc.Start();
	}


	/*
	[MenuItem("Build/Do Action")]
	public static void ChangeModelVerts()
	{
		Mesh LoadedMesh = Resources.Load<Mesh>("DecalCube2");

		Vector3[] NewVerts = LoadedMesh.vertices;

		for(int i = 0; i < NewVerts.Length; i++)
		{
			NewVerts[i].y *= 2;
		}

		LoadedMesh.vertices = NewVerts;
	}
	*/

	[MenuItem("Build/Do Action")]
	public static void ChangeModelVerts()
	{
		//ScmapEditor From = UnityEditor.Selection.gameObjects[0].GetComponent<ScmapEditor>();
		//ScmapEditor To = UnityEditor.Selection.gameObjects[1].GetComponent<ScmapEditor>();

		//To.DefaultSkyboxData = From.DefaultSkyboxData;

		string path = EditorUtility.OpenFilePanel("Skybox", "", "");


		if (string.IsNullOrEmpty(path))
			return;

		string data = System.IO.File.ReadAllText(path);
		ScmapEditor To = UnityEditor.Selection.activeGameObject.GetComponent<ScmapEditor>();
		SkyboxData NewSkybox = UnityEngine.JsonUtility.FromJson<SkyboxData>(data);
		To.DefaultSkyboxData = NewSkybox.Data;
		EditorUtility.SetDirty(To);
	}

}