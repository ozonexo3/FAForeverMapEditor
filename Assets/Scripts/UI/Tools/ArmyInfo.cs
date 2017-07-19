using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MapLua;

public class ArmyInfo : MonoBehaviour {

	public GameObject TeamPrefab;
	public GameObject TeamListPrefab;
	public GameObject ArmyPrefab;

	public Transform TeamsPivot;
	public ListObject ExtraList;
	public TeamListObject ExtraPivot;

	public GameObject ArmyOptions;
	public GameObject TeamsOptions;
	public GameObject ExtraArmies;

	public InputField ArmyNameInput;

	private void Awake()
	{
		ExtraPivot.AddAction = AddArmy;
	}

	private void OnEnable()
	{
		ExtraList.DragAction = DragEnded;
		UpdateList();
	}

	private void OnDisable()
	{
		
	}

#region Generate
	public void UpdateList()
	{
		if(SelectedArmy != null)
		{
			// Show Army
			ArmyOptions.SetActive(true);
			TeamsOptions.SetActive(false);
			ExtraArmies.SetActive(false);

			Clean();
		}
		else
		{
			// Show Teams
			ArmyOptions.SetActive(false);
			TeamsOptions.SetActive(true);
			ExtraArmies.SetActive(true);

			Clean();
			Generate();
		}
	}

	void Clean()
	{

		AllTeamFields = new List<ListObject>();
		AllFields = new List<ListObject>();
		foreach (RectTransform child in TeamsPivot)
		{
			Destroy(child.gameObject);
		}

		foreach (RectTransform child in ExtraPivot.Pivot)
		{
			Destroy(child.gameObject);
		}
	}


	public List<ListObject> AllTeamFields = new List<ListObject>();
	public List<ListObject> AllFields = new List<ListObject>();

	void Generate()
	{

		GameObject newList;

		int c = 0;


		int TeamId = 0;
		for (int t = 0; t < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length; t++)
		{
			//Teams
			newList = Instantiate(TeamPrefab, TeamsPivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(TeamsPivot);
			ListObject TeamListObject = newList.GetComponent<ListObject>();
			TeamListObject.name = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].name;
			TeamListObject.ObjectName.text = TeamListObject.name;
			TeamListObject.InstanceId = TeamId;
			TeamListObject.ClickActionId = SelectTeam;
			TeamListObject.ClickCloseActionId = RemoveTeam;
			TeamListObject.ClickApplyActionId = ApplyTeam;
			TeamListObject.DragAction = DragEnded;
			AllTeamFields.Add(TeamListObject);

			newList = Instantiate(TeamListPrefab, TeamsPivot.position, Quaternion.identity) as GameObject;
			newList.GetComponent<RectTransform>().SetParent(TeamsPivot);
			newList.GetComponent<TeamListObject>().InstanceId = TeamId;
			newList.GetComponent<TeamListObject>().AddAction = AddArmy;


			Transform TeamPivot = newList.GetComponent<TeamListObject>().Pivot;
			TeamId++;


			for (int a = 0; a < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys.Count; a++)
			{

				newList = Instantiate(ArmyPrefab, TeamPivot.position, Quaternion.identity) as GameObject;
				newList.GetComponent<RectTransform>().SetParent(TeamPivot);
				ListObject ArmyListObject = newList.GetComponent<ListObject>();
				ArmyListObject.name = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys[a].Name;

				ArmyListObject.ObjectName.text = ArmyListObject.name;

				if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys[a].Data == null)
				{
					ArmyListObject.SetSelection(1);
					ArmyListObject.ObjectName.text += "\nNo army in Save.lua!";
				}
				else if (!SaveLua.NameExist(ArmyListObject.name))
				{
					ArmyListObject.SetSelection(1);
					ArmyListObject.ObjectName.text += "\nArmy marker not found!";
				}

				ArmyListObject.ListId = t;
				ArmyListObject.InstanceId = a;
				ArmyListObject.ClickActionId = SelectArmy;
				ArmyListObject.ClickCloseActionId = RemoveArmy;
				ArmyListObject.DragAction = DragEndedArmy;
				AllFields.Add(ArmyListObject);

			}


			for (int a = 0; a < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count; a++)
			{

				newList = Instantiate(ArmyPrefab, ExtraPivot.Pivot.position, Quaternion.identity) as GameObject;
				newList.GetComponent<RectTransform>().SetParent(ExtraPivot.Pivot);
				ListObject ArmyListObject = newList.GetComponent<ListObject>();
				ArmyListObject.name = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys[a].Name;
				ArmyListObject.ObjectName.text = ArmyListObject.name;

				if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys[a].Data == null)
				{
					ArmyListObject.SetSelection(1);
					ArmyListObject.ObjectName.text += "\nNo army in Save.lua!";
				}
				/*else if (!SaveLua.NameExist(ArmyListObject.name))
				{
					ArmyListObject.SetSelection(1);
					ArmyListObject.ObjectName.text += "\nArmy marker not found!";
				}*/


				ArmyListObject.ListId = -1;
				ArmyListObject.InstanceId = a;
				ArmyListObject.ClickActionId = SelectArmy;
				ArmyListObject.ClickCloseActionId = RemoveArmy;
				ArmyListObject.DragAction = DragEndedArmy;
				
				AllFields.Add(ArmyListObject);
			}

		}
	}
	#endregion


#region Get Army
	public SaveLua.Army GetCurrentArmy()
	{
		if (SelectedArmy == null)
			return null;
		return SelectedArmy.Data;
		/*for (int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.Armies.Length; i++)
			if (MapLuaParser.Current.SaveLuaFile.Data.Armies[i].Name == SelectedArmy)
				return MapLuaParser.Current.SaveLuaFile.Data.Armies[i];
				*/
		//return null;
	}

	/*public static SaveLua.Army ArmyByName(string Name){
		for (int i = 0; i < MapLuaParser.Current.SaveLuaFile.Data.Armies.Length; i++)
			if (MapLuaParser.Current.SaveLuaFile.Data.Armies[i].Name == Name)
				return MapLuaParser.Current.SaveLuaFile.Data.Armies[i];
		return null;
	}*/


	public static bool ArmyExist(string Name)
	{
		int c = 0;

		
		for (int t = 0; t < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length; t++)
		{
			for (int a = 0; a < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys.Count; a++)
			{
				if (Name == MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].Armys[a].Name)
					return true;
			}
		}


		return false;
	}
	#endregion


#region UI
	public void SelectTeam(ListObject id)
	{
		int c = 0;

		AllTeamFields[id.InstanceId].SymmetrySelected.GetComponent<InputField>().text = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[id.InstanceId].name;
		AllTeamFields[id.InstanceId].Selected.SetActive(false);
		AllTeamFields[id.InstanceId].SymmetrySelected.SetActive(true);
	}

	public void ApplyTeam(ListObject id)
	{
		string NewName = AllTeamFields[id.InstanceId].SymmetrySelected.GetComponent<InputField>().text;
		int c = 0;
		bool WrongName = false;
		for (int t = 0; t < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length; t++)
		{
			if(t != id.InstanceId && NewName == MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].name)
			{
				WrongName = true;
				break;
			}

		}

		if (!WrongName)
		{
			//TODO Register Undo
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[id.InstanceId].name = NewName;
			id.ObjectName.text = NewName;
		}

		id.Selected.SetActive(true);
		id.SymmetrySelected.SetActive(false);
	}

	public void RemoveTeam(ListObject id)
	{
		//TODO Register Undo

		int c = 0;
		List<ScenarioLua.Team> TeamsList = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.ToList();
		TeamsList.RemoveAt(id.InstanceId);
		MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams = TeamsList.ToArray();

		UpdateList();
	}

	public void AddTeam()
	{
		//TODO Register Undo
		int c = 0;

		ScenarioLua.Team NewTeam = new ScenarioLua.Team();
		string DefaultName = "FFA";

		bool WrongName = true;
		int OffsetId = 0;
		while (WrongName)
		{
			WrongName = false;
			string TestName = DefaultName;
			if (OffsetId > 0)
				TestName += "_" + OffsetId.ToString();

			for (int t = 0; t < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length; t++)
			{
				if (TestName == MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[t].name)
				{
					WrongName = true;
					OffsetId++;
					break;
				}
			}
		}

		if (OffsetId > 0)
			DefaultName += "_" + OffsetId.ToString();

		NewTeam.name = DefaultName;
		NewTeam.Armys = new List<ScenarioLua.Army>();

		List< ScenarioLua.Team> TeamsList = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.ToList();
		TeamsList.Add(NewTeam);
		MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams = TeamsList.ToArray();

		UpdateList();

	}

	public void AddArmy(TeamListObject TeamObject)
	{
		//TODO Register Undo
		ScenarioLua.Army CreatedArmy;
		//string CreatedName;
		int c = 0;
		if (TeamObject.InstanceId == -1)
		{
			CreatedArmy = new ScenarioLua.Army();
			CreatedArmy.Name = "NewExtraArmy" + (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count + 1);
			CreatedArmy.NoRush = new ScenarioLua.NoRusnOffset();
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Add(CreatedArmy);

			/*
			int foundP = -1;
			for (int p = 0; p < MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops.Length; p++)
			{
				if (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops[p].key == ScenarioLua.ScenarioInfo.KEY_EXTRAARMIES)
				{
					foundP = p;
					break;
				}
			}

			if(foundP < 0)
			{
				List<ScenarioLua.CustomProps> NewCustomProps = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops.ToList();
				ScenarioLua.CustomProps NewCustomProp = new ScenarioLua.CustomProps();
				NewCustomProp.key = ScenarioLua.ScenarioInfo.KEY_EXTRAARMIES;
				NewCustomProp.value = "";
				NewCustomProps.Add(NewCustomProp);

				MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops = NewCustomProps.ToArray();
				foundP = 0;
			}


			List<string> Armies = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops[foundP].value.Split(" ".ToCharArray()).ToList();
			CreatedName = "NewExtraArmy" + (Armies.Count + 1);
			Armies.Add(CreatedName);

			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops[foundP].value = "";
			bool Added = false;
			for (int i = 0; i < Armies.Count; i++)
			{
				if (string.IsNullOrEmpty(Armies[i]))
					continue;
				MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].customprops[foundP].value += (Added?(" "):("")) + Armies[i];
				Added = true;
			}
			*/

		}
		else if (TeamObject.InstanceId < 0 || TeamObject.InstanceId >= MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length)
			return;
		else {

			//MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[TeamObject.InstanceId].Armys.ToList();
			CreatedArmy = new ScenarioLua.Army();
			CreatedArmy.Name = "NewArmy" + (MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[TeamObject.InstanceId].Armys.Count + 1);
			CreatedArmy.NoRush = new ScenarioLua.NoRusnOffset();
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[TeamObject.InstanceId].Armys.Add(CreatedArmy);
			//MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[TeamObject.InstanceId].Armys = Armies;
		}



		UpdateList();
	}

	public void RemoveArmy(ListObject id)
	{

	}

	public void RemoveCurrentArmy()
	{

	}


	#region Army Changes
	public void SelectArmy(ListObject id)
	{
		int c = 0;


		if (id.ListId < -1 || id.InstanceId < 0)
			return;



		if (id.ListId == -1)
		{
			if (id.InstanceId >= MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count)
				return;

				SelectedArmy = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys[id.InstanceId];
		}
		else
		{
			if (id.ListId >= MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams.Length ||
				id.InstanceId >= MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[id.ListId].Armys.Count)
				return;

			SelectedArmy = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[id.ListId].Armys[id.InstanceId];
		}

		ArmyNameInput.text = SelectedArmy.Name;

		UpdateList();
	}


	ScenarioLua.Army SelectedArmy = null;


	public void ArmyNameChange()
	{
		Markers.MarkersControler.UpdateBlankMarkersGraphics();
	}

	public void ReturnFromArmy()
	{
		SelectedArmy = null;
		UpdateList();
	}
	#endregion
	#endregion


	#region Drag Drop
	public void DragEnded(ListObject AtId)
	{
		if (AtId.ListId == ListObject.DragBeginId.ListId)
			return;

		if (AllTeamFields.Contains(ListObject.DragBeginId))
			return;

		int c = 0;


		//TODO Register Undo
		ScenarioLua.Army Army;
		if (ListObject.DragBeginId.ListId >= 0)
		{
			Army = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[ListObject.DragBeginId.ListId].Armys[ListObject.DragBeginId.InstanceId];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[ListObject.DragBeginId.ListId].Armys.RemoveAt(ListObject.DragBeginId.InstanceId);
		}
		else
		{
			Army = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys[ListObject.DragBeginId.InstanceId];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.RemoveAt(ListObject.DragBeginId.InstanceId);
		}

		if (AtId.ListId >= 0)
		{
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[AtId.ListId].Armys.Insert(MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[AtId.ListId].Armys.Count, Army);
			ListObject.DragBeginId.ListId = AtId.ListId;
		}
		else
		{
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Insert(MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Count, Army);
			ListObject.DragBeginId.ListId = AtId.ListId;
		}

		UpdateList();

	}

	public void DragEndedArmy(ListObject AtId)
	{
		int c = 0;

		if (AtId.ListId == ListObject.DragBeginId.ListId && AtId.InstanceId == ListObject.DragBeginId.InstanceId)
			return; // Same

		if (AllTeamFields.Contains(ListObject.DragBeginId))
			return;

		//TODO Register Undo

		ScenarioLua.Army Army;

		if (ListObject.DragBeginId.ListId >= 0)
		{
			Army = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[ListObject.DragBeginId.ListId].Armys[ListObject.DragBeginId.InstanceId];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[ListObject.DragBeginId.ListId].Armys.RemoveAt(ListObject.DragBeginId.InstanceId);
		}
		else
		{
			Army = MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys[ListObject.DragBeginId.InstanceId];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.RemoveAt(ListObject.DragBeginId.InstanceId);
		}

		if(AtId.ListId >= 0)
		{
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].Teams[AtId.ListId].Armys.Insert(AtId.InstanceId, Army);
			ListObject.DragBeginId.ListId = AtId.ListId;
		}
		else
		{
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[c].ExtraArmys.Insert(AtId.InstanceId, Army);
			ListObject.DragBeginId.ListId = AtId.ListId;
		}


		UpdateList();

	}
	#endregion
}




