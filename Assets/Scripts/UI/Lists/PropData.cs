using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PropData : MonoBehaviour {

	public		Text		NameField;
	public		Text		MassField;
	public		Text		EnergyField;
	public		Text		Count;
	public		Text		TotalMassField;
	public		Text		TotalEnergyField;

	public void SetPropList(string name, float mass, float energy, int count){
		NameField.text = name;
		MassField.text = (mass).ToString();
		EnergyField.text = (energy).ToString();

		Count.text = "x" + count;
		TotalMassField.text = (mass * count).ToString();
		TotalEnergyField.text = (energy * count).ToString();
	}
}
