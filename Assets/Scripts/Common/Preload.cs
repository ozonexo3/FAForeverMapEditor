using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour {

	private void Start()
	{
		BrushGenerator.Current.LoadBrushes();

		SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
	}
}
