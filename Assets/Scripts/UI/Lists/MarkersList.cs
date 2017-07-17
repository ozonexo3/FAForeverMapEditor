using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditMap;

public class MarkersList : MonoBehaviour
{

	//public MapLuaParser Scenario;
	//public CameraControler KameraKontroler;
	//public MarkersInfo MarkersMenu;

	public RectTransform Pivot;
	public GameObject ListPrefab;
	public List<ListObject> AllFields = new List<ListObject>();

	public Sprite[] Icons;

	void OnEnable()
	{
		UpdateList();
	}

	bool Generated = false;
	int GeneratedCount = -1;
	void OnDisable()
	{
		if(!MarkersInfo.MarkerPageChange)
			Clean();
	}

	public void UnselectAll()
	{
		foreach (ListObject obj in AllFields)
		{
			obj.Unselect();
		}
	}

	public void UpdateSelection()
	{
		for(int g = 0; g < GeneratedCount; g++)
		{
			int i = AllFields[g].InstanceId;

			if (Selection.SelectionManager.Current.Selection.Ids.Contains(i))
				AllFields[g].SetSelection(1);
			else
			{
				AllFields[g].SetSelection(0);

				if (Selection.SelectionManager.Current.SymetrySelection.Length > 0)
				{
					for (int s = 0; s < Selection.SelectionManager.Current.SymetrySelection.Length; s++)
					{
						if (Selection.SelectionManager.Current.SymetrySelection[s].Ids.Contains(i))
							AllFields[g].SetSelection(2);
					}
				}
			}

		}
	}

	public void UpdateList()
	{
		int mc = 0;
		if (Generated && GeneratedCount == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count)
		{
			UpdateSelection();
			return;

		}
		
		Clean();
		GenerateList();
	}

	public void Clean()
	{
		//Debug.Log("Clean");
		AllFields = new List<ListObject>();

		foreach (RectTransform child in Pivot)
		{
			Destroy(child.gameObject);
		}
		Generated = false;
	}



	void GenerateList()
	{
		//Debug.Log("Regenerate");
		int mc = 0;
		if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length == 0)
			return;

		int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
		int TypesCount = (int)MapLua.SaveLua.Marker.MarkerTypes.Count;

		MapLua.SaveLua.Marker CurrentMarker;
		GameObject newList;
		ListObject NewListObject;
		int t = 0;
		int i = 0;

		List<GameObject> AllObjectsList = new List<GameObject>();
		for (t = 0; t < TypesCount; t++)
		{
			List<GameObject> ObjectToSort = new List<GameObject>();

			for (i = 0; i < Mcount; i++)
			{
				CurrentMarker = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i];
				if ((int)CurrentMarker.MarkerType != t)
					continue; 

				if (CurrentMarker == null || CurrentMarker.MarkerObj == null)
					continue;

				newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
				newList.GetComponent<RectTransform>().SetParent(Pivot);
				newList.name = CurrentMarker.Name;
				ObjectToSort.Add(newList);

				NewListObject = newList.GetComponent<ListObject>();
				AllFields.Add(NewListObject);
				NewListObject.ObjectName.text = CurrentMarker.Name;
				NewListObject.InstanceId = i;
				NewListObject.ListId = 0;
				NewListObject.ConnectedGameObject = CurrentMarker.MarkerObj.gameObject;
				NewListObject.ClickAction = Selection.SelectionManager.Current.SelectObject;

				if (Selection.SelectionManager.Current.Selection.Ids.Contains(i))
					NewListObject.SetSelection(1);
				else
				{
					NewListObject.SetSelection(0);

					if (Selection.SelectionManager.Current.SymetrySelection.Length > 0)
					{
						for (int s = 0; s < Selection.SelectionManager.Current.SymetrySelection.Length; s++)
						{
							if (Selection.SelectionManager.Current.SymetrySelection[s].Ids.Contains(i))
								NewListObject.SetSelection(2);
						}
					}
				}

				NewListObject.Icon.sprite = Markers.MarkersControler.GetIconByType(CurrentMarker.MarkerType);
			}

			AllObjectsList.AddRange(ObjectToSort.OrderBy(go => go.name));
		}

		int ListCount = AllObjectsList.Count;
		for (i = 0; i < ListCount; i++)
		{
			AllObjectsList[i].transform.SetSiblingIndex(i);
		}

		Generated = true;
		GeneratedCount = AllFields.Count;
		//Debug.Log(GeneratedCount + " / " + MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count);
	}
}