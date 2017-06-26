using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapInfo : MonoBehaviour {

	public		MapLuaParser		Scenario;
	public		InputField			Name;
	public		InputField			Desc;
	public		InputField			Version;
	public		Toggle[]			ScriptToggles;
	public		Toggle				SaveAsFa;

	
	void OnEnable() {
		UpdateFields();
	}

	public void UpdateFields(){
		Name.text = Scenario.ScenarioData.MapName;
		Desc.text = Scenario.ScenarioData.MapDesc;
		Version.text = Scenario.ScenarioData.Version + "";
	}

	public void UpdateScriptToggles(int id){
		for(int i = 0; i < ScriptToggles.Length; i++){
			if(i == id) ScriptToggles[i].isOn = true;
			else ScriptToggles[i].isOn = false;
		}
	}

	public void EndFieldEdit(){
		if(HasChanged()) Scenario.History.RegisterMapInfo ();
		Scenario.ScenarioData.MapName = Name.text;
		Scenario.ScenarioData.MapDesc = Desc.text;
		Scenario.ScenarioData.Version = int.Parse(Version.text);
	}

	public void ChangeScript(int id = 0){
		if(Scenario.ScriptId != id) Scenario.History.RegisterMapInfo ();
		Scenario.ScriptId = id;
	}



	bool HasChanged(){
		if(Scenario.ScenarioData.MapName != Name.text) return true;
		if(Scenario.ScenarioData.MapDesc != Desc.text) return true;
		if (Scenario.ScenarioData.Version != int.Parse (Version.text)) return true;
		return false;
	}
}

