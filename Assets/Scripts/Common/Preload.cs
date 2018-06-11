using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour {

	private void Start()
	{
		MapLuaParser.LoadStructurePaths();
		BrushGenerator.Current.LoadBrushes();

		//Preload heavy gamedata files
		GetGamedataFile.GetZipFileInstance(GetGamedataFile.UnitsScd);
		GetGamedataFile.GetFAFZipFileInstance(GetGamedataFile.UnitsScd);

		SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
	}
}
