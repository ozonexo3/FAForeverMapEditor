using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapInfo : MonoBehaviour {

	public		MapLuaParser		Scenario;
	public		InputField			Name;
	public		InputField			Desc;
	public		InputField			Version;

	
	void OnEnable () {
		Name.text = Scenario.ScenarioData.MapName;
		Desc.text = Scenario.ScenarioData.MapDesc;
		Version.text = Scenario.ScenarioData.Version + "";
	}
	
	void Update () {

	
	}

	public void EndFieldEdit(){
		Scenario.ScenarioData.MapName = Name.text;
		Scenario.ScenarioData.MapDesc = Desc.text;
		Scenario.ScenarioData.Version = int.Parse(Version.text);
	}

	public void ChangeScript(int id = 0){

		Scenario.ScriptId = id;
	}
}

