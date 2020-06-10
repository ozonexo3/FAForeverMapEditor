using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EditMap;
using UnityEngine.UI;

public class MarkersList : MonoBehaviour
{

	//public MapLuaParser Scenario;
	//public CameraControler KameraKontroler;
	//public MarkersInfo MarkersMenu;

	public RectTransform Pivot;
	public GameObject ListPrefab;
	public List<ListObject> AllFields = new List<ListObject>();
	public VerticalLayoutGroup Layout;
	public ContentSizeFitter SizeFitter;

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
		else if (Generating)
		{
			StopGenerating();
		}
	}

	public void UnselectAll()
	{
		foreach (ListObject obj in AllFields)
		{
			obj.Unselect();
		}
	}

	bool UpdateSelectionAfterGenerate = false;
	public void UpdateSelection()
	{
		if (Generating)
		{
			UpdateSelectionAfterGenerate = true;
			return;

		}
		//bool Focused = false;
		for (int g = 0; g < GeneratedCount; g++)
		{
			int i = AllFields[g].InstanceId;

			if (AllFields[g].ConnectedGameObject == null)
				continue;

			if (AllFields[g].gameObject.activeSelf != AllFields[g].ConnectedGameObject.activeSelf)
				AllFields[g].gameObject.SetActive(AllFields[g].ConnectedGameObject.activeSelf);

			if (Selection.SelectionManager.Current.Selection.Ids.Contains(i))
			{
				AllFields[g].SetSelection(1);
				if (!IgnoreFocusList && i == Selection.SelectionManager.Current.Selection.Ids[Selection.SelectionManager.Current.Selection.Ids.Count - 1])
				{
					// Focus on
					Vector3 Pos = AllFields[g].GetComponent<RectTransform>().localPosition;
					Pos.y = 0 - Pos.y;
					Pos.y -= 120;
					Pos.x = 0;
					Pivot.localPosition = Pos;

				}
			}
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

	public void UpdateList(bool forced = false)
	{
		int mc = 0;
		if (forced) { }
		else if (Generated && GeneratedCount == MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count)
		{
			UpdateSelection();
			return;
		}

		StopGenerating();

		Clean();
		GenerateList();
	}

	public void Clean()
	{
		StopGenerating();
		AllFields = new List<ListObject>(2048);

		foreach (RectTransform child in Pivot)
		{
			Destroy(child.gameObject);
		}
		Generated = false;
	}

	bool IgnoreFocusList = false;
	public void SelectListGameobject(GameObject Connected)
	{
		if (Generating)
			return;
		IgnoreFocusList = true;
		Selection.SelectionManager.Current.SelectObject(Connected);
		IgnoreFocusList = false;
	}


	bool Buffor = false;
	bool Generating
	{
		get
		{
			return GeneratingEnum != null;
		}

	}
	Coroutine GeneratingEnum;
	void GenerateList()
	{
		if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains.Length == 0)
			return;

		if (!gameObject.activeInHierarchy)
			return;

		if (Generating)
		{
			Buffor = true;
		}
		else
		{
			GeneratingEnum = StartCoroutine(GenerateingList());
		}
	}

	void StopGenerating()
	{
		if (Generating)
		{
			StopCoroutine(GeneratingEnum);
			GeneratingEnum = null;
		}
	}

	IEnumerator GenerateingList()
	{
		//Debug.Log("Regenerate");
		int mc = 0;
		//const int PauseEvery = 64;
		//int GenerateCounter = 0;

		int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;
		int TypesCount = (int)MapLua.SaveLua.Marker.MarkerTypes.Count;

		MapLua.SaveLua.Marker CurrentMarker;
		GameObject newList;
		ListObject NewListObject;
		int t = 0;
		int i = 0;

		float Realtime = Time.realtimeSinceStartup;
		//const float MaxAllowedOverhead = 0.03334f;
		const float MaxAllowedOverhead = 0.3f;


		List<GameObject> AllObjectsList = new List<GameObject>(1024);
		for (t = 0; t < TypesCount; t++)
		{
			List<GameObject> ObjectToSort = new List<GameObject>(1024);

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
				NewListObject.ClickAction = SelectListGameobject;

				if (!NewListObject.ConnectedGameObject.activeSelf)
					newList.SetActive(false);

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
				if (CurrentMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker && ArmyInfo.ArmyExist(CurrentMarker.Name))
					NewListObject.Icon.sprite = Markers.MarkersControler.Current.SpawnGraphic.Icon;
				else
					NewListObject.Icon.sprite = Markers.MarkersControler.GetIconByType(CurrentMarker.MarkerType);

				/*GenerateCounter++;
				if (GenerateCounter > PauseEvery)
				{
					GenerateCounter = 0;
					yield return null;

				}*/
				if (Time.realtimeSinceStartup - Realtime > MaxAllowedOverhead)
				{
					yield return null;
					Realtime = Time.realtimeSinceStartup;
				}
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


		Layout.enabled = true;
		SizeFitter.enabled = true;

		yield return null;

		Layout.enabled = false;
		SizeFitter.enabled = false;
		GeneratingEnum = null;
		if (Buffor)
		{
			Buffor = false;
			UpdateSelectionAfterGenerate = false;
			GenerateList();
		}
		else if (UpdateSelectionAfterGenerate)
		{
			UpdateSelectionAfterGenerate = false;
			UpdateSelection();
		}

	}

	public void OnScroll()
	{
		Pivot.localPosition = new Vector3(0, (int)Pivot.localPosition.y, 0);
	}
}