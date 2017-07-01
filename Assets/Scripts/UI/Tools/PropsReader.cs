using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PropsReader : MonoBehaviour {

	public static List<PropTypeGroup> AllPropsTypes;
	public	Transform	Pivot;
	public	GameObject	PropGroupObject;
	public	Text		TotalMass;
	public	Text		TotalEnergy;

	public class PropTypeGroup{
		public		string 		Blueprint = "";
		public string LoadBlueprint = "";
		public		string		HelpText = "";
		public		float 		MassReclaim = 0;
		public		float 		EnergyReclaim = 0;
		public		List<Prop> 	Props = new List<Prop>();
		public		GetGamedataFile.PropObject PropObject;
		public	List<GameObject> PropsInstances = new List<GameObject>();
	}

	public static void LoadProps(ScmapEditor HeightmapControler){

		AllPropsTypes = new List<PropTypeGroup>();
		List<Prop> Props = HeightmapControler.map.Props;

		Debug.Log("Found props: " + Props.Count);

		for (int i = 0; i < Props.Count; i++)
		{
			bool NewProp = false;
			int GroupId = 0;
			if (AllPropsTypes.Count == 0) NewProp = true;
			else
			{
				NewProp = true;
				for (int g = 0; g < AllPropsTypes.Count; g++)
				{
					if (Props[i].BlueprintPath == AllPropsTypes[g].Blueprint)
					{
						NewProp = false;
						GroupId = g;
						break;
					}
				}
			}

			if (NewProp)
			{
				GroupId = AllPropsTypes.Count;
				AllPropsTypes.Add(new PropTypeGroup());
				AllPropsTypes[GroupId].Blueprint = Props[i].BlueprintPath;
				AllPropsTypes[GroupId].LoadBlueprint = Props[i].BlueprintPath.Replace("\\", "/");

				if (AllPropsTypes[GroupId].LoadBlueprint.StartsWith("/"))
					AllPropsTypes[GroupId].LoadBlueprint = AllPropsTypes[GroupId].LoadBlueprint.Remove(0, 1);

				AllPropsTypes[GroupId].PropObject = GetGamedataFile.LoadProp("env.scd", AllPropsTypes[GroupId].LoadBlueprint);

			}

			Quaternion PropRotation = Quaternion.LookRotation(Props[i].RotationZ, Props[i].RotationY);

			GameObject NewPropGameobject = AllPropsTypes[GroupId].PropObject.CreatePropGameObject(ScmapEditor.MapPosInWorld(Props[i].Position), PropRotation);
			AllPropsTypes[GroupId].PropsInstances.Add(NewPropGameobject);

			AllPropsTypes[GroupId].Props.Add(Props[i]);
		}

		Debug.Log("Props types: " + AllPropsTypes.Count);
	}

	public void Clean(){
		if(Pivot.childCount > 0){
			foreach(Transform child in Pivot) Destroy(child.gameObject);
		}
		TotalMassCount = 0;
		TotalEnergyCount = 0;
	}

	void OnDisable(){
		Clean();
	}

	string ParseString;
	float TotalMassCount;
	float TotalEnergyCount;
	public void CalculateReclaim(){
		Clean();

		if(AllPropsTypes.Count == 0){
			Debug.LogWarning("Props count is 0");
			return;
		}

		for(int i = 0; i < AllPropsTypes.Count; i++){
			//if(i > 0) return;
			string[] BlueprintData = GamedataBlueprints.GetBlueprint("env.scd", AllPropsTypes[i].Blueprint);
			if (BlueprintData == null)
				continue;
			if(BlueprintData.Length > 0){
				for(int l = 0; l < BlueprintData.Length; l++){
					if(BlueprintData[l].Contains("ReclaimMassMax")){
						ParseString = BlueprintData[l].Replace(" ", "").Replace("ReclaimMassMax", "").Replace("'", "").Replace("=", "").Replace(",", "").Replace("\n", "");
						//Debug.Log(ParseString);
						if(string.IsNullOrEmpty(ParseString)) AllPropsTypes[i].MassReclaim = 0;
						else AllPropsTypes[i].MassReclaim = float.Parse(ParseString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					}
					else if(BlueprintData[l].Contains("ReclaimEnergyMax")){
						ParseString = BlueprintData[l].Replace(" ", "").Replace("ReclaimEnergyMax", "").Replace("'", "").Replace("=", "").Replace(",", "").Replace("\n", "");
						//Debug.Log(ParseString);
						if(string.IsNullOrEmpty(ParseString)) AllPropsTypes[i].EnergyReclaim = 0;
						else AllPropsTypes[i].EnergyReclaim = float.Parse(ParseString, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					}
					else if(BlueprintData[l].Contains("HelpText")){
						ParseString = BlueprintData[l].Replace(" ", "").Replace("HelpText", "").Replace("=", "").Replace("'", "").Replace(",", "").Replace("\n", "");
						//Debug.Log(ParseString);
						AllPropsTypes[i].HelpText = ParseString;
					}
				}
				//Debug.Log(BlueprintData.Length + ", " + AllPropsTypes[i].HelpText + ", " + AllPropsTypes[i].MassReclaim + ", " + AllPropsTypes[i].EnergyReclaim );

				GameObject NewListObject = Instantiate(PropGroupObject) as GameObject;
				NewListObject.transform.SetParent(Pivot, false);
				NewListObject.transform.localScale = Vector3.one;
				NewListObject.GetComponent<PropData>().SetPropList(i, AllPropsTypes[i].HelpText, AllPropsTypes[i].MassReclaim, AllPropsTypes[i].EnergyReclaim, AllPropsTypes[i].Props.Count, AllPropsTypes[i].Blueprint);
				TotalMassCount += AllPropsTypes[i].Props.Count * AllPropsTypes[i].MassReclaim;
				TotalEnergyCount += AllPropsTypes[i].Props.Count * AllPropsTypes[i].EnergyReclaim;
			}
			else{
				AllPropsTypes[i].HelpText = "Warning: Can't find prop in env.scd";
			}

			TotalMass.text = TotalMassCount.ToString();
			TotalEnergy.text = TotalEnergyCount.ToString();


		}

	}
}
