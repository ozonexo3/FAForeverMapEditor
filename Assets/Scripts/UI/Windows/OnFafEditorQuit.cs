using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFafEditorQuit : MonoBehaviour {

	public bool AllowQuit = false;
	public GameObject Popup;

	public void PressYes()
	{
		AllowQuit = true;
		Popup.SetActive(false);
		MapLuaParser.Current.SaveMap();
		//Application.Quit();
		InvokeRepeating("WaitForSavingDone", 0.1f, 0.03f);
	}

	void WaitForSavingDone()
	{
		if (!MapLuaParser.SavingMapProcess)
		{
			CancelInvoke();
			Application.Quit();
		}
	}

	public void PressNo()
	{
		AllowQuit = true;
		Popup.SetActive(false);
		Application.Quit();
	}

	public void PressCancel()
	{
		Popup.SetActive(false);
	}


	private void OnApplicationQuit()
	{
		if (MapLuaParser.SavingMapProcess)
		{
			Application.CancelQuit();
			return;
		}

		if (!string.IsNullOrEmpty(MapLuaParser.Current.FolderName) && !string.IsNullOrEmpty(MapLuaParser.Current.ScenarioFileName) && !AllowQuit)
		{
			if (!Popup.activeSelf)
				Popup.SetActive(true);
			Application.CancelQuit();
		}

		if(AllowQuit && MapLuaParser.SavingMapProcess)
			Application.CancelQuit();
	}
}
