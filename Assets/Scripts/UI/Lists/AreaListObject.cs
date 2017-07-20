using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaListObject : MonoBehaviour {

	public Toggle Selected;

	public int InstanceId;
	public InputField Name;
	public InputField SizeX;
	public InputField SizeY;
	public InputField SizeWidth;
	public InputField SizeHeight;

	public AreaInfo Controler;


	public void OnNameChanged()
	{
		Controler.OnValuesChange(InstanceId);
	}

	public void OnToggle()
	{
		if (Selected.isOn)
		{
			Controler.SelectArea(InstanceId);
		}
	}

	public void OnRemove()
	{

	}
}
