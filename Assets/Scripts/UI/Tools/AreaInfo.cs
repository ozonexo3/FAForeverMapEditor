using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfo : MonoBehaviour {

	public static AreaInfo Current;

	public Toggle BorderRelative;
	public Toggle AreaHide;
	public Toggle AreaDefault;
	public GameObject AreaPrefab;
	public Transform Pivot;
	public ToggleGroup ToggleGrp;


	List<AreaListObject> Created = new List<AreaListObject>();

	public static void CleanSelection()
	{
		HideArea = false;
		SelectedArea = null;

	}

	private void Awake()
	{
		Current = this;
	}

	private void OnEnable()
	{

		UpdateList();
	}

	private void OnDisable()
	{

	}

	public void SwitchBorderRelative()
	{
		UpdateList();
	}

	public void UpdateList()
	{
		Clean();
		Generate();

		MapLuaParser.Current.UpdateArea();
	}

	void Clean()
	{
		for(int i = 0; i < Created.Count; i++)
		{
			Destroy(Created[i].gameObject);
		}
		Created = new List<AreaListObject>();
	}

	void Generate()
	{

		MapLua.SaveLua.Areas[] Areas = MapLuaParser.Current.SaveLuaFile.Data.areas;

		if (SelectedArea != null)
			AreaHide.isOn = true;

		for (int i = 0; i < Areas.Length; i++)
		{
			GameObject NewArea = Instantiate(AreaPrefab, Pivot.position, Quaternion.identity) as GameObject;
			NewArea.GetComponent<RectTransform>().SetParent(Pivot);

			AreaListObject AreaObject = NewArea.GetComponent<AreaListObject>();

			AreaObject.Controler = this;
			AreaObject.InstanceId = i;
			AreaObject.Name.text = Areas[i].Name;
			AreaObject.SizeX.text = Areas[i].rectangle.x.ToString();
			AreaObject.SizeY.text = Areas[i].rectangle.y.ToString();

			if (BorderRelative.isOn)
			{
				AreaObject.SizeWidth.text = (ScmapEditor.Current.map.Width - Areas[i].rectangle.width).ToString();
				AreaObject.SizeHeight.text = (ScmapEditor.Current.map.Height - Areas[i].rectangle.height).ToString();
			}
			else
			{
				AreaObject.SizeWidth.text = Areas[i].rectangle.width.ToString();
				AreaObject.SizeHeight.text = Areas[i].rectangle.height.ToString();
			}

			AreaObject.Selected.group = ToggleGrp;


			if(Areas[i] == SelectedArea)
			{
				AreaObject.Selected.isOn = true;
			}

			Created.Add(AreaObject);
		}

	}


	#region Selection
	public static bool HideArea = false;
	public static MapLua.SaveLua.Areas SelectedArea;

	public void ToggleSelected()
	{
		HideArea = AreaHide.isOn;

		if (AreaHide.isOn || AreaDefault.isOn)
		{
			SelectedArea = null;
		}
		MapLuaParser.Current.UpdateArea();
	}

	public void SelectArea(int InstanceID)
	{
		SelectedArea = MapLuaParser.Current.SaveLuaFile.Data.areas[InstanceID];
		MapLuaParser.Current.UpdateArea();
	}


	#endregion


	#region UI
	public void OnValuesChange(int instanceID)
	{
		//TODO Undo

		MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].Name = Created[instanceID].Name.text;

		MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.x = LuaParser.Read.StringToFloat( Created[instanceID].SizeX.text);
		MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.y = LuaParser.Read.StringToFloat(Created[instanceID].SizeY.text);

		MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.width = LuaParser.Read.StringToFloat(Created[instanceID].SizeWidth.text);
		MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.height = LuaParser.Read.StringToFloat(Created[instanceID].SizeHeight.text);

		if (BorderRelative.isOn)
		{
			MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.width = ScmapEditor.Current.map.Width - LuaParser.Read.StringToFloat(Created[instanceID].SizeWidth.text);
			MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.height = ScmapEditor.Current.map.Height - LuaParser.Read.StringToFloat(Created[instanceID].SizeHeight.text);
		}
		else
		{
			MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.width = LuaParser.Read.StringToFloat(Created[instanceID].SizeWidth.text);
			MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID].rectangle.height = LuaParser.Read.StringToFloat(Created[instanceID].SizeHeight.text);

		}



		if (SelectedArea == MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID])
		{
			MapLuaParser.Current.UpdateArea();
		}
	}


#endregion

	public void AddNew()
	{

	}


}
