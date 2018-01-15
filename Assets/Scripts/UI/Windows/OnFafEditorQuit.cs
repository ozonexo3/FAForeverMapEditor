using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFafEditorQuit : MonoBehaviour {

	static OnFafEditorQuit Current;
	public bool AllowQuit = false;
	public GameObject Popup;

	private void Awake()
	{
		Current = this;
		Application.wantsToQuit += WantsToQuit;
	}

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

	static bool WantsToQuit()
	{
		if (Current == null)
			return true;

		if (MapLuaParser.SavingMapProcess)
		{
			return false;
		}

		if (!string.IsNullOrEmpty(MapLuaParser.Current.FolderName) && !string.IsNullOrEmpty(MapLuaParser.Current.ScenarioFileName) && !Current.AllowQuit)
		{
			if (!Current.Popup.activeSelf)
			{
				Current.Popup.SetActive(true);
				GenericPopup.RemoveAll();

			}
			return false;
		}

		if (Current.AllowQuit && MapLuaParser.SavingMapProcess)
			return false;

		return true;
	}

	/*
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
			{
				Popup.SetActive(true);
				GenericPopup.RemoveAll();

			}
			Application.CancelQuit();
		}

		if(AllowQuit && MapLuaParser.SavingMapProcess)
			Application.CancelQuit();
	}
	*/
}
