using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace EditMap
{
	public class MapInfo : MonoBehaviour
	{

		public MapLuaParser Scenario;
		public InputField Name;
		public InputField Desc;
		public InputField Version;
		public Toggle[] ScriptToggles;
		public Toggle SaveAsFa;


		void OnEnable()
		{
			UpdateFields();
		}

		public void UpdateFields()
		{
			Name.text = Scenario.ScenarioLuaFile.Data.name;
			Desc.text = Scenario.ScenarioLuaFile.Data.description;
			Version.text = Scenario.ScenarioLuaFile.Data.map_version.ToString();
		}

		public void UpdateScriptToggles(int id)
		{
			for (int i = 0; i < ScriptToggles.Length; i++)
			{
				if (i == id) ScriptToggles[i].isOn = true;
				else ScriptToggles[i].isOn = false;
			}
		}

		public void EndFieldEdit()
		{
			if (HasChanged()) Scenario.History.RegisterMapInfo();
			Scenario.ScenarioLuaFile.Data.name = Name.text;
			Scenario.ScenarioLuaFile.Data.description = Desc.text;
			Scenario.ScenarioLuaFile.Data.map_version = float.Parse(Version.text);
		}

		public void ChangeScript(int id = 0)
		{
			if (Scenario.ScriptId != id) Scenario.History.RegisterMapInfo();
			Scenario.ScriptId = id;
		}



		bool HasChanged()
		{
			if (Scenario.ScenarioLuaFile.Data.name != Name.text) return true;
			if (Scenario.ScenarioLuaFile.Data.description != Desc.text) return true;
			if (Scenario.ScenarioLuaFile.Data.map_version != float.Parse(Version.text)) return true;
			return false;
		}
	}
}
