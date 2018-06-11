using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Ozone.UI;

public class UnitListObject : MonoBehaviour {

	public Text GroupName;
	public Color NameRoot;
	public Color NameGroup;

	public Text UnitsCount;
	public Transform Pivot;
	public VerticalLayoutGroup Layout;
	public int InstanceId;

	public Image Selection;
	public Color ColorNormal;
	public Color ColorSelected;

	public UiTextField NameInputField;
	public GameObject SelectButton;
	public GameObject RemoveButton;


	public MapLua.SaveLua.Army Owner;
	public MapLua.SaveLua.Army.UnitsGroup Source;
	public System.Action<UnitListObject> AddAction;
	public System.Action<UnitListObject> RemoveAction;
	public System.Action<UnitListObject> SelectAction;

	public void SetTab(int count)
	{
		if (count > 5)
			count = 5;

		Layout.padding.left = 2 + count * 6;
	}


	bool IsRoot = false;
	public void SetGroup(MapLua.SaveLua.Army Owner, MapLua.SaveLua.Army.UnitsGroup Data, bool root)
	{
		this.Owner = Owner;
		IsRoot = root;


		if (IsRoot)
		{
			GroupName.text = Owner.Name;
			NameInputField.SetValue(Owner.Name);
			RemoveButton.SetActive(false);
			UnitsCount.gameObject.SetActive(false);
		}
		else
		{
			GroupName.text = Data.Name;
			NameInputField.SetValue(Data.Name);
			RemoveButton.SetActive(true);
			UnitsCount.gameObject.SetActive(true);
		}

		UnitsCount.text = Data.UnitInstances.Count.ToString();

		OnRenamed();
		Selection.color = ColorNormal;
		GroupName.color = IsRoot ? NameRoot : NameGroup;
	}



	public void UpdateSelection(bool Selected)
	{
		//Selection.gameObject.SetActive(Selected);
		Selection.color = Selected ? ColorSelected : ColorNormal;
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
		SelectAction(this);

		if (IsRoot)
			return;

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
