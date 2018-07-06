using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfo : MonoBehaviour {

	public static AreaInfo Current;

	public Toggle BorderRelative;
	public Toggle Rounding;
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

	public void SwitchPlayableAreaRounding()
	{
		//Rounding.isOn = !Rounding.isOn;

		MapLuaParser.Current.UpdateArea(Rounding.isOn);
	}

	public void UpdateList()
	{
		Clean();
		Generate();

		MapLuaParser.Current.UpdateArea(Rounding.isOn);
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

		bool ToogleFound = false;

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
				ToogleFound = true;
			}

			Created.Add(AreaObject);
		}

		if (SelectedArea != null && !ToogleFound)
			AreaHide.isOn = true;

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
		MapLuaParser.Current.UpdateArea(Rounding.isOn);
	}

	public void SelectArea(int InstanceID)
	{
		SelectedArea = MapLuaParser.Current.SaveLuaFile.Data.areas[InstanceID];
		MapLuaParser.Current.UpdateArea(Rounding.isOn);
	}


	#endregion


	#region UI
	public void OnValuesChange(int instanceID)
	{
		Undo.RegisterUndo(new UndoHistory.HistoryAreaChange(), new UndoHistory.HistoryAreaChange.AreaChangeParam(MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID]));

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
			MapLuaParser.Current.UpdateArea(Rounding.isOn);
		}
	}


#endregion

	public void AddNew()
	{

		Undo.RegisterUndo(new UndoHistory.HistoryAreasChange());

		List<MapLua.SaveLua.Areas> Areas = MapLuaParser.Current.SaveLuaFile.Data.areas.ToList();
		MapLua.SaveLua.Areas NewArea = new MapLua.SaveLua.Areas();

		string DefaultMapArea = "New Area";
		int NextAreaName = 0;
		bool FoundGoodName = false;
		while (!FoundGoodName)
		{
			FoundGoodName = true;
			string TestName = DefaultMapArea + ((NextAreaName > 0) ? (" " + NextAreaName.ToString()) : (""));
			for (int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.areas.Length; i++)
			{
				if(MapLuaParser.Current.SaveLuaFile.Data.areas[i].Name == TestName)
				{
					FoundGoodName = false;
					NextAreaName++;
					break;
				}
			}
		}

		NewArea.Name = DefaultMapArea + ((NextAreaName > 0)?(" " + NextAreaName.ToString()) :(""));
		NewArea.rectangle = new Rect(0, 0, ScmapEditor.Current.map.Width, ScmapEditor.Current.map.Height);
		Areas.Add(NewArea);

		MapLuaParser.Current.SaveLuaFile.Data.areas = Areas.ToArray();
		UpdateList();

	}

	public void Remove(int instanceID)
	{
		Undo.RegisterUndo(new UndoHistory.HistoryAreasChange());

		if (SelectedArea == MapLuaParser.Current.SaveLuaFile.Data.areas[instanceID])
		{
			SelectedArea = null;
			AreaDefault.isOn = true;
		}

		List<MapLua.SaveLua.Areas> Areas = MapLuaParser.Current.SaveLuaFile.Data.areas.ToList();

		Areas.RemoveAt(instanceID);

		MapLuaParser.Current.SaveLuaFile.Data.areas = Areas.ToArray();


		UpdateList();
	}


}
