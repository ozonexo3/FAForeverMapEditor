using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadRecentMaps : MonoBehaviour {

	public AppMenu Menu;
	public GameObject ListPrefab;
	public Transform Pivot;
	public Transform HorLine;

	private void OnEnable()
	{
		Generate();
	}

	private void OnDisable()
	{
		Clean();
	}

	void Clean()
	{
		foreach (GameObject child in GeneratedChilds)
			Destroy(child);

		GeneratedChilds.Clear();
	}

	HashSet<GameObject> GeneratedChilds = new HashSet<GameObject>();

	void Generate()
	{
		for(int i = 0; i < 10; i++)
		{
			string ScenarioFileName = PlayerPrefs.GetString(ScenarioFile + i, "");

			if (string.IsNullOrEmpty(ScenarioFileName))
			{

			}
			else
			{
				GameObject NewListObj = Instantiate(ListPrefab, Pivot) as GameObject;
				ListObject Component = NewListObj.GetComponent<ListObject>();
				Component.ClickActionId = ClickRecentMap;
				Component.ObjectName.text = PlayerPrefs.GetString(FolderPath + i, "");
				Component.InstanceId = i;
				GeneratedChilds.Add(NewListObj);
			}
		}
		if(HorLine)
			HorLine.SetAsLastSibling();
	}

	public static int RecentMapSelected = 0;
	public void ClickRecentMap(ListObject id)
	{
		RecentMapSelected = id.InstanceId;
		Menu.OpenRecentMap();

		/*
		MapLuaParser.Current.ScenarioFileName = PlayerPrefs.GetString(ScenarioFile + id.InstanceId, "");
		MapLuaParser.Current.FolderName = PlayerPrefs.GetString(FolderPath + id.InstanceId, "");
		MapLuaParser.Current.FolderParentPath = PlayerPrefs.GetString(ParentPath + id.InstanceId, "");

		//HelperGui.ButtonFunction("LoadMap");
		MapLuaParser.Current.LoadFile();
		*/
	}






	public const string ScenarioFile = "RecentScenario_";
	public const string FolderPath = "RecentFolder_";
	public const string ParentPath = "RecentParent_";

	const int RecentCount = 10;
	public static void MoveLastMaps(string scenario = "", string folder = "", string parent = "")
	{

		if (scenario == PlayerPrefs.GetString(ScenarioFile + 0, "") && folder == PlayerPrefs.GetString(FolderPath + 0, "")) return;

		string[] NewScenario = new string[RecentCount];
		string[] NewFolder = new string[RecentCount];
		string[] NewParent = new string[RecentCount];

		int offset = 0;
		NewScenario[0] = scenario;
		NewFolder[0] = folder;
		NewParent[0] = parent;

		for (int i = 1; i < RecentCount; i++)
		{
			while (PlayerPrefs.GetString(ScenarioFile + (i - 1 + offset), "") == scenario && scenario != "" 
				&& PlayerPrefs.GetString(FolderPath + (i - 1 + offset), "") == folder && folder != ""
				&& offset < RecentCount)
			{
				offset++;
			}
			NewScenario[i] = PlayerPrefs.GetString(ScenarioFile + (i - 1 + offset), "");
			NewFolder[i] = PlayerPrefs.GetString(FolderPath + (i - 1 + offset), "");
			NewParent[i] = PlayerPrefs.GetString(ParentPath + (i - 1 + offset), "");
		}

		for (int i = 0; i < RecentCount; i++)
		{

			PlayerPrefs.SetString(ScenarioFile + i, NewScenario[i]);
			PlayerPrefs.SetString(FolderPath + i, NewFolder[i]);
			PlayerPrefs.SetString(ParentPath + i, NewParent[i]);
		}
	}
}
