using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class ScriptBatch : MonoBehaviour 
{
	[MenuItem("Build/Windows with resources")]
	public static void BuildSettings ()
	{
		// Get filename
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

		string[] levels = new string[] {"Assets/FAForeverMapEditor.unity"};

		BuildPipeline.BuildPlayer(levels, path + "/FAForeverMapEditor.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

		// Copy structure files
		FileUtil.CopyFileOrDirectory("Structure", path + "/FAForeverMapEditor_Data/Structure");

		// Run the game (Process class from System.Diagnostics).
		//Process proc = new Process();
		//proc.StartInfo.FileName = path + "/FAForeverMapEditor.exe";
		//proc.Start();
	}
}