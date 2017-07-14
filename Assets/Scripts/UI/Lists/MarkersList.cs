using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EditMap;

public class MarkersList : MonoBehaviour
{

	public MapLuaParser Scenario;
	public CameraControler KameraKontroler;
	public MarkersInfo MarkersMenu;

	public RectTransform Pivot;
	public GameObject ListPrefab;
	public List<ListObject> AllFields = new List<ListObject>();

	public Sprite[] Icons;

	void OnEnable()
	{
		//GenerateList();
	}

	void OnDisable()
	{
		foreach (RectTransform child in Pivot)
		{
			AllFields = new List<ListObject>();
			Destroy(child.gameObject);
		}
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
		
	}

	public void UpdateList()
	{
		foreach (RectTransform child in Pivot)
		{
			AllFields = new List<ListObject>();
			Destroy(child.gameObject);
		}
		GenerateList();
	}

	void GenerateList()
	{
		int mc = 0;
		if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length == 0)
			return;

		for(int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Length; i++)
		{
			MapLua.SaveLua.Marker CurrentMarker = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[i];
			if (CurrentMarker == null || CurrentMarker.MarkerObj == null)
				continue;

			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			//newList.GetComponent<RectTransform>().localPosition = Vector3.up * -30 * count;
			//newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 30);

			ListObject NewListObject = newList.GetComponent<ListObject>();


			//int a = Scenario.MarkerRend.Armys[i].GetComponent<MarkerData>().InstanceId;
			NewListObject.ObjectName.text = CurrentMarker.Name;
			NewListObject.InstanceId = i;
			NewListObject.ListId = 0;
			NewListObject.ConnectedGameObject = CurrentMarker.MarkerObj.gameObject;


			if(Selection.SelectionManager.Current.Selection.Ids.Contains(i))
				NewListObject.SetSelection(1);
			else
			{
				NewListObject.SetSelection(0);

				if (Selection.SelectionManager.Current.SymetrySelection.Length > 0)
				{
					for(int s = 0; s < Selection.SelectionManager.Current.SymetrySelection.Length; s++)
					{
						if(Selection.SelectionManager.Current.SymetrySelection[s].Ids.Contains(i))
							NewListObject.SetSelection(2);
					}
				}
			}

			NewListObject.Icon.sprite = Markers.MarkersControler.GetIconByType(CurrentMarker.MarkerType);

			/*if (CurrentMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker)
				NewListObject.Icon.sprite = Icons[0];
			else if (CurrentMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.Mass)
				NewListObject.Icon.sprite = Icons[1];
			else if (CurrentMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.Hydrocarbon)
				NewListObject.Icon.sprite = Icons[2];
			else
				NewListObject.Icon.sprite = Icons[3];*/

			AllFields.Add(NewListObject);
		}


		/*
		int count = 0;
		for (int i = 0; i < Scenario.MarkerRend.Armys.Count; i++)
		{
			if (Scenario.ARMY_[i].Hidden)
				continue;
			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			newList.GetComponent<RectTransform>().localPosition = Vector3.up * -30 * count;
			newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 30);
			AllFields.Add(newList.GetComponent<ListObject>());

			int a = Scenario.MarkerRend.Armys[i].GetComponent<MarkerData>().InstanceId;
			AllFields[count].ObjectName.text = Scenario.ARMY_[a].name;
			AllFields[count].Icon.sprite = Icons[0];
			AllFields[count].KameraKontroler = KameraKontroler;
			AllFields[count].InstanceId = i;
			AllFields[count].ListId = 0;
			AllFields[count].ConnectedGameObject = Scenario.MarkerRend.Armys[i];
			if (MarkersMenu.IsSelected(0, i)) AllFields[count].SetSelection(1);
			else if (MarkersMenu.IsSymmetrySelected(0, i)) AllFields[count].SetSelection(2);
			else AllFields[count].SetSelection(0);

			count++;
		}

		for (int i = 0; i < Scenario.Mexes.Count; i++)
		{
			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			newList.GetComponent<RectTransform>().localPosition = Vector3.up * -30 * count;
			newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 30);

			AllFields.Add(newList.GetComponent<ListObject>());
			AllFields[count].ObjectName.text = Scenario.Mexes[i].name;
			AllFields[count].Icon.sprite = Icons[1];
			AllFields[count].KameraKontroler = KameraKontroler;
			AllFields[count].InstanceId = i;
			AllFields[count].ListId = 1;
			AllFields[count].ConnectedGameObject = Scenario.MarkerRend.Mex[i];

			if (MarkersMenu.IsSelected(1, i)) AllFields[count].SetSelection(1);
			else if (MarkersMenu.IsSymmetrySelected(1, i)) AllFields[count].SetSelection(2);
			else AllFields[count].SetSelection(0);

			count++;
		}

		for (int i = 0; i < Scenario.Hydros.Count; i++)
		{
			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			newList.GetComponent<RectTransform>().localPosition = Vector3.up * -30 * count;
			newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 30);
			AllFields.Add(newList.GetComponent<ListObject>());
			AllFields[count].ObjectName.text = Scenario.Hydros[i].name;
			AllFields[count].Icon.sprite = Icons[2];
			AllFields[count].KameraKontroler = KameraKontroler;
			AllFields[count].InstanceId = i;
			AllFields[count].ListId = 2;
			AllFields[count].ConnectedGameObject = Scenario.MarkerRend.Hydro[i];

			if (MarkersMenu.IsSelected(2, i)) AllFields[count].SetSelection(1);
			else if (MarkersMenu.IsSymmetrySelected(2, i)) AllFields[count].SetSelection(2);
			else AllFields[count].SetSelection(0);

			count++;
		}

		for (int i = 0; i < Scenario.SiMarkers.Count; i++)
		{
			GameObject newList = Instantiate(ListPrefab, Pivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(Pivot);
			newList.GetComponent<RectTransform>().localPosition = Vector3.up * -30 * count;
			newList.GetComponent<RectTransform>().sizeDelta = new Vector3(1, 30);
			AllFields.Add(newList.GetComponent<ListObject>());
			AllFields[count].ObjectName.text = Scenario.SiMarkers[i].name;
			AllFields[count].Icon.sprite = Icons[3];
			AllFields[count].KameraKontroler = KameraKontroler;
			AllFields[count].InstanceId = i;
			AllFields[count].ListId = 3;
			AllFields[count].ConnectedGameObject = Scenario.MarkerRend.Ai[i];

			if (MarkersMenu.IsSelected(3, i)) AllFields[count].SetSelection(1);
			else if (MarkersMenu.IsSymmetrySelected(3, i)) AllFields[count].SetSelection(2);
			else AllFields[count].SetSelection(0);
			count++;
		}

		Vector2 PivotRect = Pivot.sizeDelta;
		PivotRect.y = 30 * count;
		Pivot.sizeDelta = PivotRect;
		*/
	}
}