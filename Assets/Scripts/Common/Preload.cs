using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WindowStateSever;

public class Preload : MonoBehaviour {

	public WindowStateSaverMonoBehaviour WSS;

	private void Start()
	{
		WSS.Init();
		StartCoroutine(PreloadEditor());
	}

	IEnumerator PreloadEditor()
	{
		yield return null;
		MapLuaParser.LoadStructurePaths();
		yield return null;
		BrushGenerator.Current.LoadBrushes();
		yield return null;

		//Preload heavy gamedata files
		GetGamedataFile.GetZipFileInstance(GetGamedataFile.UnitsScd);
		yield return null;
		yield return null;
		GetGamedataFile.GetFAFZipFileInstance(GetGamedataFile.UnitsScd);
		yield return null;

		yield return null;
		yield return null;

		SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

	}
}
