using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;
using Ozone.UI;

public class PropData : MonoBehaviour {

	public		int ID;
	public		Text		NameField;
	public		Text		PathField;
	public		Text		MassField;
	public		Text		EnergyField;
	public		Text		Count;
	public		Text		TotalMassField;
	public		Text		TotalEnergyField;

	public UiTextField ScaleMin;
	public UiTextField ScaleMax;

	public UiTextField RotationMin;
	public UiTextField RotationMax;

	public UiTextField Chance;

	public void SetPropList(int _ID, string name, float mass, float energy, int count, string path){
		ID = _ID;
		NameField.text = name;
		MassField.text = (mass).ToString();
		EnergyField.text = (energy).ToString();

		PathField.text = path;

		Count.text = "x" + count;
		TotalMassField.text = (mass * count).ToString();
		TotalEnergyField.text = (energy * count).ToString();
	}

	public void SetPropPaint(int _ID, string name)
	{
		ID = _ID;
		NameField.text = name;
	}

	public void RemoveProp()
	{
		PropsInfo.Current.RemoveProp(ID);
	}

	public void OverPropEnter()
	{
		PropsInfo.Current.ShowPreview(ID, gameObject);
	}

	public void OverPropExit()
	{
		PropsInfo.Current.Preview.Hide(gameObject);
	}

	/*
	public void ClampRotations()
	{
		float RotMin = LuaParser.Read.StringToFloat(RotationMin.text);
		float RotMax = LuaParser.Read.StringToFloat(RotationMax.text);

		RotationMin.text = Mathf.Clamp(RotMin, 0, RotMax).ToString();
		RotationMax.text = Mathf.Clamp(RotMax, RotMin, 360).ToString();
	}
	*/
}
