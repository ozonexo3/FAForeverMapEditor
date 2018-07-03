using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MapLua;
using Ozone.UI;
using Markers;
using Selection;

namespace EditMap
{
	public class AdaptiveInfo : MonoBehaviour
	{

		[Header("UI")]
		public Text MarkerName;
		public GameObject TogglePrefab;
		public GameObject ToggleGroupPrefab;
		public GameObject TitlePrefab;
		public GameObject TitleDescPrefab;
		public Transform ArmyTooglePivot;
		public Transform CustomTablesPivot;

		public HashSet<GameObject> CreatedObjects = new HashSet<GameObject>();

		UiToggle[] ArmyToogles = new UiToggle[0];
		UiToggle[][] CustomToggles = new UiToggle[0][];



		private void OnEnable()
		{
			Generate();
			SelectionManager.Current.SetSelectionChangeAction(UpdateSelection);

			RenderAdaptiveMarkers.UpdateAdaptiveLines();
			UpdateMarkerName();
		}

		private void OnDisable()
		{
			RenderAdaptiveMarkers.DisableRenderer();
		}

		void Clear()
		{
			HashSet<GameObject>.Enumerator ListEnum = CreatedObjects.GetEnumerator();

			while (ListEnum.MoveNext())
			{
				Destroy(ListEnum.Current);
			}

			CreatedObjects.Clear();
		}

		void Generate()
		{
			if (!MapLuaParser.Current.TablesLuaFile.IsLoaded)
				return;

			Clear();


			ScenarioLua.ScenarioInfo ScenarioData = MapLuaParser.Current.ScenarioLuaFile.Data;
			List<UiToggle> ArmyTogglesList = new List<UiToggle>();
			for (int c = 0; c < ScenarioData.Configurations.Length; c++)
			{
				for (int t = 0; t < ScenarioData.Configurations[c].Teams.Length; t++)
				{
					for (int a = 0; a < ScenarioData.Configurations[c].Teams[t].Armys.Count; a++)
					{
						if (ScenarioData.Configurations[c].Teams[t].Armys[a].Data != null)
						{
							GameObject NewToggleObj = Instantiate(TogglePrefab, ArmyTooglePivot.parent);
							CreatedObjects.Add(NewToggleObj);
							NewToggleObj.transform.SetSiblingIndex(ArmyTooglePivot.GetSiblingIndex());
							NewToggleObj.SetActive(true);

							UiToggle NewToggle = NewToggleObj.GetComponent<UiToggle>();
							NewToggle.Set(false, ScenarioData.Configurations[c].Teams[t].Armys[a].Data.Name + " (" + ScenarioData.Configurations[c].Teams[t].name + ")", OnArmyToggleChanged, t, a);
							ArmyTogglesList.Add(NewToggle);
						}
					}
				}
			}
			ArmyToogles = ArmyTogglesList.ToArray();

			TablesLua.TablesInfo TablesData = MapLuaParser.Current.TablesLuaFile.Data;
			CustomToggles = new UiToggle[TablesData.AllTables.Count][];
			for (int i = 0; i < TablesData.AllTables.Count; i++)
			{
				CreateTitle(TablesData.AllTables[i].Key);

				if (TablesData.AllTables[i].OneDimension)
				{
					CustomToggles[i] = new UiToggle[1];

					GameObject NewToggleObj = Instantiate(TogglePrefab, ArmyTooglePivot.parent);
					CreatedObjects.Add(NewToggleObj);
					NewToggleObj.transform.SetSiblingIndex(CustomTablesPivot.GetSiblingIndex());
					NewToggleObj.SetActive(true);

					UiToggle NewToggle = NewToggleObj.GetComponent<UiToggle>();
					NewToggle.Set(false, TablesData.AllTables[i].Key, OnTableToggleChanged, i, 0);
					CustomToggles[i][0] = NewToggle;
				}
				else
				{
					int ToogleGroupCount = 0;
					GameObject LastToggleGroup = Instantiate(ToggleGroupPrefab, ArmyTooglePivot.parent);
					CreatedObjects.Add(LastToggleGroup);
					LastToggleGroup.transform.SetSiblingIndex(CustomTablesPivot.GetSiblingIndex());
					LastToggleGroup.SetActive(true);

					CustomToggles[i] = new UiToggle[TablesData.AllTables[i].Values.Length];

					for (int j = 0; j < TablesData.AllTables[i].Values.Length; j++)
					{
						ToogleGroupCount++;
						if(ToogleGroupCount > 3)
						{
							ToogleGroupCount = 0;
							LastToggleGroup = Instantiate(ToggleGroupPrefab, ArmyTooglePivot.parent);
							CreatedObjects.Add(LastToggleGroup);
							LastToggleGroup.transform.SetSiblingIndex(CustomTablesPivot.GetSiblingIndex());
							LastToggleGroup.SetActive(true);
						}

						GameObject NewToggleObj = Instantiate(TogglePrefab, LastToggleGroup.transform);
						CreatedObjects.Add(NewToggleObj);
						NewToggleObj.transform.SetSiblingIndex(CustomTablesPivot.GetSiblingIndex());
						NewToggleObj.SetActive(true);


						UiToggle NewToggle = NewToggleObj.GetComponent<UiToggle>();
						NewToggle.Set(false, (j + 1).ToString(), OnTableToggleChanged, i, j); // TablesData.AllTables[i].Key + " " + 
						CustomToggles[i][j] = NewToggle;
					}
				}
			}

			UpdateSelection();
		}

		public void UpdateSelection()
		{
			UpdateArmyToggles();
			UpdateCustomToggles();
			UpdateMarkerName();
		}

		void UpdateMarkerName()
		{
			if (SelectionManager.Current.Selection.Ids.Count > 0)
			{
				MarkerName.text = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].GetComponent<MarkerObject>().Owner.Name;
			}
			else
				MarkerName.text = "";
		}

		void UpdateArmyToggles()
		{
			for (int t = 0; t < ArmyToogles.Length; t++)
			{
				ArmyToogles[t].ResetTesting();
			}
			int Count = SelectionManager.Current.Selection.Ids.Count;
			for (int i = 0; i < Count; i++)
			{
				GameObject CurrentObj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]];

				MarkerObject Mo = CurrentObj.GetComponent<MarkerObject>();
				if (Mo == null)
					continue;

				SaveLua.Marker Current = CurrentObj.GetComponent<MarkerObject>().Owner;

				if (Current.MarkerType == SaveLua.Marker.MarkerTypes.Mass || Current.MarkerType == SaveLua.Marker.MarkerTypes.Hydrocarbon)
					for (int t = 0; t < ArmyToogles.Length; t++)
					{
						if (Current.SpawnWithArmy.Contains(t))
							ArmyToogles[t].HasOnValue = true;
						else
							ArmyToogles[t].HasOffValue = true;
					}


			}

			for (int t = 0; t < ArmyToogles.Length; t++)
			{
				ArmyToogles[t].ApplyTesting();
			}
		}

		void UpdateCustomToggles()
		{
			for (int t = 0; t < CustomToggles.Length; t++)
			{
				for(int j = 0; j < CustomToggles[t].Length; j++)
				{

					CustomToggles[t][j].ResetTesting();
				}
			}

			TablesLua.TablesInfo TablesData = MapLuaParser.Current.TablesLuaFile.Data;

			int Count = SelectionManager.Current.Selection.Ids.Count;
			for (int i = 0; i < Count; i++)
			{
				GameObject CurrentObj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]];
				SaveLua.Marker Current = CurrentObj.GetComponent<MarkerObject>().Owner;

				if (Current.MarkerType != SaveLua.Marker.MarkerTypes.Mass && Current.MarkerType != SaveLua.Marker.MarkerTypes.Hydrocarbon)
					continue;

				for (int t = 0; t < CustomToggles.Length; t++)
				{
					if (TablesData.AllTables[t].OneDimension)
					{
						if (Current.AdaptiveKeys.Contains(TablesData.AllTables[t].Key))
							CustomToggles[t][0].HasOnValue = true;
						else
							CustomToggles[t][0].HasOffValue = true;

					}
					else
					{
						for (int j = 0; j < CustomToggles[t].Length; j++)
						{
							// Check if contain 
							if (Current.AdaptiveKeys.Contains(TablesData.AllTables[t].Key + "#" + j))
								CustomToggles[t][j].HasOnValue = true;
							else
								CustomToggles[t][j].HasOffValue = true;
						}
					}
				}

			}


			for (int t = 0; t < CustomToggles.Length; t++)
			{
				for (int j = 0; j < CustomToggles[t].Length; j++)
				{
					CustomToggles[t][j].ApplyTesting();
				}
			}

		}

		public void OnArmyToggleChanged(int GroupId, int ToggleId)
		{
			bool SetTo = false;
			if (ArmyToogles[ToggleId].HasOffValue)
				SetTo = true;

			int Count = SelectionManager.Current.Selection.Ids.Count;
			bool AnyChanged = false;
			for (int i = 0; i < Count; i++)
			{
				GameObject CurrentObj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]];
				SaveLua.Marker Current = CurrentObj.GetComponent<MarkerObject>().Owner;

				if (Current.MarkerType != SaveLua.Marker.MarkerTypes.Mass && Current.MarkerType != SaveLua.Marker.MarkerTypes.Hydrocarbon)
					continue;

				if (SetTo)
				{
					if (!Current.SpawnWithArmy.Contains(ToggleId))
					{
						Current.SpawnWithArmy.Add(ToggleId);
						AnyChanged = true;
					}
				}
				else
				{
					if (Current.SpawnWithArmy.Contains(ToggleId))
					{
						Current.SpawnWithArmy.Remove(ToggleId);
						AnyChanged = true;
					}
				}
			}

			ArmyToogles[ToggleId].HasOnValue = SetTo;
			ArmyToogles[ToggleId].HasOffValue = !SetTo;
			ArmyToogles[ToggleId].ApplyTesting();

			if(AnyChanged)
				RenderAdaptiveMarkers.UpdateAdaptiveLines();
		}


		public void OnTableToggleChanged(int GroupId, int ToggleId)
		{
			TablesLua.TablesInfo TablesData = MapLuaParser.Current.TablesLuaFile.Data;

			
			bool SetTo = false;
			if (CustomToggles[GroupId][ToggleId].HasOffValue)
				SetTo = true;

			string TableKey;
			if (TablesData.AllTables[GroupId].OneDimension)
			{
				TableKey = TablesData.AllTables[GroupId].Key;
			}
			else
			{
				TableKey = TablesData.AllTables[GroupId].Key + "#" + ToggleId;
			}

			int Count = SelectionManager.Current.Selection.Ids.Count;
			bool AnyChanged = false;
			for (int i = 0; i < Count; i++)
			{
				GameObject CurrentObj = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]];
				SaveLua.Marker Current = CurrentObj.GetComponent<MarkerObject>().Owner;

				if (Current.MarkerType != SaveLua.Marker.MarkerTypes.Mass && Current.MarkerType != SaveLua.Marker.MarkerTypes.Hydrocarbon)
					continue;

				if (SetTo)
				{
					if (!Current.AdaptiveKeys.Contains(TableKey))
					{
						Current.AdaptiveKeys.Add(TableKey);
						AnyChanged = true;
					}
				}
				else
				{
					if (Current.AdaptiveKeys.Contains(TableKey))
					{
						Current.AdaptiveKeys.Remove(TableKey);
						AnyChanged = true;
					}
				}
			}


			CustomToggles[GroupId][ToggleId].HasOnValue = SetTo;
			CustomToggles[GroupId][ToggleId].HasOffValue = !SetTo;
			CustomToggles[GroupId][ToggleId].ApplyTesting();

			if(AnyChanged)
				RenderAdaptiveMarkers.UpdateAdaptiveLines();
		}

		void CreateTitle(string name)
		{
			GameObject Prefab = TitlePrefab;
			string Name = FormatTableName(name);
			string Desc = "";

			switch (name)
			{
				case "extraHydros":
					Prefab = TitleDescPrefab;
					Name = "Extra Hydros";
					Desc = "Add extra hydros to the map";
					break;
				case "extraMexes":
					Prefab = TitleDescPrefab;
					Name = "Extra Mexes";
					Desc = "Add extra mexes to the map";
					break;
				case "extraBaseMexes":
					Prefab = TitleDescPrefab;
					Name = "Extra Base Mexes";
					Desc = "add mexes to starting base (further away from coreMexes)";
					break;
				case "forwardCrazyrushMexes":
					Prefab = TitleDescPrefab;
					Name = "forwardCrazyrushMexes";
					Desc = "determine forward crazy rush mexes";
					break;
				case "crazyrushOneMex":
					Prefab = TitleDescPrefab;
					Name = "Crazyrush";
					Desc = "Only use these mexes/resources (refers to spwnMexArmy)";
					break;
				case "DuplicateListMex":
					Prefab = TitleDescPrefab;
					Name = "Duplicate List Mex";
					Desc = "Additional crazyrush mex";
					break;
				case "extramass":
					Prefab = TitleDescPrefab;
					Name = "Extra Mass";
					Desc = "Additional mex in starting locations";
					break;
			}



			GameObject NewTitleObj = Instantiate(Prefab, ArmyTooglePivot.parent);
			CreatedObjects.Add(NewTitleObj);
			NewTitleObj.transform.SetSiblingIndex(CustomTablesPivot.GetSiblingIndex());
			NewTitleObj.SetActive(true);
			NewTitleObj.GetComponent<Text>().text = Name;

			if (!string.IsNullOrEmpty(Desc))
			{
				NewTitleObj.GetComponent<UiTitle>().Subtitle.text = Desc;
			}

		}

		static string FormatTableName(string name)
		{
			name = name.Replace("mass", "Mass");
			name = name.Replace("mex", "Mex");
			name = name.Replace("hydro", "Hydro");

			for(int i = 0; i < name.Length; i++)
			{
				if (char.IsUpper(name[i]))
				{
					name = name.Insert(i, " ");
					i++;
				}
				else if(i == 0)
				{
					char New = char.ToUpper(name[i]);
					name = name.Remove(0, 1);
					name = New + name;
				}
			}
			return name;
		}

		public void CreateTablesFile()
		{
			GenericPopup.ShowPopup(GenericPopup.PopupTypes.TriButton, "Creating Tables", "Replace current script.lua file?\nThis can't be undone.",
				"Yes", ReplaceScript,
				"No", NoScript,
				"Cancel", Cancel
				);
		}

		void ReplaceScript()
		{
			MapLuaParser.Current.SaveScriptLua(1, true);
			NoScript();
		}

		void NoScript()
		{
			if (MapLuaParser.Current.TablesLuaFile.IsLoaded)
				return;

			MapLuaParser.Current.TablesLuaFile.CreateDefault();
			MapLuaParser.Current.SaveTablesLua();
			MapLuaParser.Current.SaveOptionsLua();
			OnEnable();
		}

		void Cancel()
		{

		}
	}
}
