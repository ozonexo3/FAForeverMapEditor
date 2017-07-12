using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AreasWindow : MonoBehaviour {

	public		MapLuaParser		Scenario;
	public		Toggle				DefaultArea;
	public		InputField			DefaultX;
	public		InputField			DefaultY;
	public		InputField			DefaultW;
	public		InputField			DefaultH;
	
	void OnEnable () {
		/*
		DefaultArea.isOn = Scenario.ScenarioData.DefaultArea;
		DefaultX.text = Scenario.ScenarioData.Area.x.ToString ();
		DefaultY.text = Scenario.ScenarioData.Area.y.ToString ();
		DefaultW.text = Scenario.ScenarioData.Area.width.ToString ();
		DefaultH.text = Scenario.ScenarioData.Area.height.ToString ();
		*/
	}

	public void ValueChange(){
		/*
		Scenario.ScenarioData.DefaultArea = DefaultArea.isOn;

		Scenario.ScenarioData.Area.x = float.Parse(DefaultX.text);
		Scenario.ScenarioData.Area.y = float.Parse(DefaultY.text);
		Scenario.ScenarioData.Area.width = float.Parse(DefaultW.text);
		Scenario.ScenarioData.Area.height = float.Parse(DefaultH.text);

		Scenario.UpdateArea ();
		*/
	}
}
