using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Ozone.UI;

public class UnitListObject : MonoBehaviour {

	public Text GroupName;
	public Text UnitsCount;
	public Transform Pivot;
	public VerticalLayoutGroup Layout;
	public int InstanceId;

	public GameObject Selection;

	public UiTextField NameInputField;
	public GameObject SelectButton;
	public GameObject RemoveButton;


	public System.Action<UnitListObject> AddAction;
	public System.Action<UnitListObject> RemoveAction;
	public System.Action<UnitListObject> SelectAction;

	public void SetTab(int count)
	{
		if (count > 5)
			count = 5;

		Layout.padding.left = 2 + count * 6;
	}

	public void SetGroup(string Name, bool root)
	{
		GroupName.text = Name;
		NameInputField.SetValue(Name);


		if (root)
		{
			RemoveButton.SetActive(false);

		}
		else
		{
			RemoveButton.SetActive(true);
		}
		Selection.SetActive(false);
	}

	public void UpdateSelection(bool Selected)
	{
		Selection.gameObject.SetActive(Selected);
	}

	public void UpdateValues(int UnitCounts)
	{
		UnitsCount.text = "Count: " + UnitCounts;
	}

	public void AddNew()
	{
		AddAction(this);
	}

	public void RemoveGroup()
	{
		RemoveAction(this);
	}

	const float DoubleClickTime = 0.3f;
	float LastClickTime = 0;
	public void OnTitleClick()
	{
		if(Time.realtimeSinceStartup - LastClickTime < DoubleClickTime)
		{
			SelectButton.SetActive(false);
			GroupName.gameObject.SetActive(false);
			NameInputField.gameObject.SetActive(true);
			NameInputField.InputFieldUi.ActivateInputField();
		}

		LastClickTime = Time.realtimeSinceStartup;
	}
	public void OnRenamed()
	{
		SelectButton.SetActive(false);
		NameInputField.gameObject.SetActive(false);
		GroupName.gameObject.SetActive(true);
		SelectButton.SetActive(true);
	}



}
