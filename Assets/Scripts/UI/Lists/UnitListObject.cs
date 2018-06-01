using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UnitListObject : MonoBehaviour {

	public Text GroupName;
	public Transform Pivot;
	public VerticalLayoutGroup Layout;
	public int InstanceId;

	public System.Action<UnitListObject> AddAction;

	public void SetTab(int count)
	{
		if (count > 5)
			count = 5;

		Layout.padding.left = 2 + count * 6;
	}

	public void AddNew()
	{
		AddAction(this);
	}

}
