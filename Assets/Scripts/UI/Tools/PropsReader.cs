using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PropsReader : MonoBehaviour {

	public static PropsReader Current;

	public static List<PropTypeGroup> AllPropsTypes;
	public	Transform	Pivot;
	public	GameObject	PropGroupObject;
	public	Text		TotalMass;
	public	Text		TotalEnergy;
	public	Text		TotalTime;

	public class PropTypeGroup{
		public		string 		Blueprint = "";
		public string LoadBlueprint = "";
		public		string		HelpText = "";
		public		List<Prop> 	Props = new List<Prop>();
		public		GetGamedataFile.PropObject PropObject;
		public		List<GameObject> PropsInstances = new List<GameObject>();
	}

	void Awake()
	{
		Current = this;
	}

	#region Loading Assets
	public void UnloadProps()
	{
		if(AllPropsTypes != null && AllPropsTypes.Count > 0)
		for (int i = 0; i < AllPropsTypes.Count; i++)
		{
			for (int p = 0; p < AllPropsTypes[i].PropsInstances.Count; p++)
			{
				Destroy(AllPropsTypes[i].PropsInstances[p]);
			}
		}

		AllPropsTypes = new List<PropTypeGroup>();
		TotalMassCount = 0;
		TotalEnergyCount = 0;
		TotalReclaimTime = 0;
	}

	public IEnumerator LoadProps(){
		UnloadProps();

		List<Prop> Props = ScmapEditor.Current.map.Props;

		//Debug.Log("Found props: " + Props.Count);

		const int YieldStep = 100;
		int LoadCounter = YieldStep;

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
				LoadCounter = YieldStep;
				yield return null;
			}

			//TODO store props as instances
			GameObject NewPropGameobject = AllPropsTypes[GroupId].PropObject.CreatePropGameObject(ScmapEditor.MapPosInWorld(Props[i].Position), Quaternion.LookRotation(Props[i].RotationZ, Props[i].RotationY));
			AllPropsTypes[GroupId].PropsInstances.Add(NewPropGameobject);
			LoadCounter--;
			if(LoadCounter <= 0)
			{
				LoadCounter = YieldStep;
				yield return null;
			}

			AllPropsTypes[GroupId].Props.Add(Props[i]);


			TotalMassCount += AllPropsTypes[GroupId].PropObject.BP.ReclaimMassMax;
			TotalEnergyCount += AllPropsTypes[GroupId].PropObject.BP.ReclaimEnergyMax;
			TotalReclaimTime += AllPropsTypes[GroupId].PropObject.BP.ReclaimTime;

			TotalMass.text = TotalMassCount.ToString();
			TotalEnergy.text = TotalEnergyCount.ToString();
			TotalTime.text = TotalReclaimTime.ToString();
		}

		yield return null;

		//Debug.Log("Props types: " + AllPropsTypes.Count);
	}

	#endregion

	void OnDisable(){
		Clean();
	}

	string ParseString;
	float TotalMassCount;
	float TotalEnergyCount;
	float TotalReclaimTime;

	#region Current Reclaims

	public void CalculateReclaim(){
		Clean();

		if(AllPropsTypes.Count == 0){
			Debug.LogWarning("Props count is 0");
			return;
		}

		for(int i = 0; i < AllPropsTypes.Count; i++){

			GameObject NewListObject = Instantiate(PropGroupObject) as GameObject;
			NewListObject.transform.SetParent(Pivot, false);
			NewListObject.transform.localScale = Vector3.one;
			NewListObject.GetComponent<PropData>().SetPropList(i, AllPropsTypes[i].HelpText, AllPropsTypes[i].PropObject.BP.ReclaimMassMax, AllPropsTypes[i].PropObject.BP.ReclaimEnergyMax, AllPropsTypes[i].Props.Count, AllPropsTypes[i].Blueprint);
			TotalMassCount += AllPropsTypes[i].Props.Count * AllPropsTypes[i].PropObject.BP.ReclaimMassMax;
			TotalEnergyCount += AllPropsTypes[i].Props.Count * AllPropsTypes[i].PropObject.BP.ReclaimEnergyMax;
			TotalReclaimTime += AllPropsTypes[i].Props.Count * AllPropsTypes[i].PropObject.BP.ReclaimTime;

			TotalMass.text = TotalMassCount.ToString();
			TotalEnergy.text = TotalEnergyCount.ToString();
			TotalTime.text = TotalReclaimTime.ToString();
		}
	}

	public void Clean()
	{
		if (Pivot.childCount > 0)
		{
			foreach (Transform child in Pivot) Destroy(child.gameObject);
		}
		CleanPaintList();

		TotalMassCount = 0;
		TotalEnergyCount = 0;
		TotalReclaimTime = 0;
		PaintPropObjects = new List<GetGamedataFile.PropObject>();
	}

	public void CleanPaintList()
	{
		if (PaintPropPivot.childCount > 0)
		{
			foreach (Transform child in PaintPropPivot) Destroy(child.gameObject);
		}
	}

	#endregion

	#region UI
	List<GetGamedataFile.PropObject> PaintPropObjects = new List<GetGamedataFile.PropObject>();

	public GameObject PaintPropListObject;
	public Transform PaintPropPivot;
	public StratumLayerBtnPreview Preview;

	public void OpenResorceBrowser()
	{
		ResourceBrowser.Current.LoadPropBlueprint();
	}

	public void DropProp()
	{
		if (!ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
			return;
		if (ResourceBrowser.SelectedCategory == 3)
		{
			//Undo.RegisterStratumChange(Selected);

			if (!PaintPropObjects.Contains(ResourceBrowser.Current.LoadedProps[ResourceBrowser.DragedObject.InstanceId]))
			{
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				PaintPropObjects.Add(ResourceBrowser.Current.LoadedProps[ResourceBrowser.DragedObject.InstanceId]);

				GameObject NewPropListObject = Instantiate(PaintPropListObject, PaintPropPivot) as GameObject;
				NewPropListObject.GetComponent<PropData>().SetPropPaint(PaintPropObjects.Count - 1, ResourceBrowser.Current.LoadedProps[ResourceBrowser.DragedObject.InstanceId].BP.Name);
			}

			//Map.Textures[Selected].Albedo = ResourceBrowser.Current.LoadedTextures[ResourceBrowser.DragedObject.InstanceId];
			//Map.Textures[Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
		}
	}

	public void RemoveProp(int ID)
	{
		CleanPaintList();
		Preview.Hide(PaintPropPivot.GetChild(ID).gameObject);
		PaintPropObjects.RemoveAt(ID);

		for(int i = 0; i < PaintPropObjects.Count; i++)
		{
			GameObject NewPropListObject = Instantiate(PaintPropListObject, PaintPropPivot) as GameObject;
			NewPropListObject.GetComponent<PropData>().SetPropPaint(PaintPropObjects.Count - 1, PaintPropObjects[i].BP.Name);

		}
	}

	public void ShowPreview(int ID, GameObject Parent)
	{
		Preview.Show(PaintPropObjects[ID].BP.LODs[0].Albedo, Parent, 14f);
	}


	#endregion

}
