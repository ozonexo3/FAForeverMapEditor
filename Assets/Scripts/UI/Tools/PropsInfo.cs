using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace EditMap
{
	public class PropsInfo : MonoBehaviour
	{

		public static PropsInfo Current;

		[Header("Connections")]
		public Editing Edit;
		public static List<PropTypeGroup> AllPropsTypes;
		public Transform Pivot;
		public GameObject PropGroupObject;
		public Text TotalMass;
		public Text TotalEnergy;
		public Text TotalTime;
		public GameObject[] Tabs;
		public GameObject[] TabSelected;

		[Header("Brush")]
		public Slider BrushSizeSlider;
		public InputField BrushSize;
		public Slider BrushStrengthSlider;
		public InputField BrushStrength;
		public InputField BrushMini;
		public InputField BrushMax;
		public InputField Scatter;
		public LayerMask TerrainMask;
		public Material TerrainMaterial;


		float TotalMassCount = 0;
		float TotalEnergyCount = 0;
		float TotalReclaimTime = 0;

		const int SelectedFalloff = 0;


		[Header("State")]
		public bool Invert;

		#region Classes
		public class PropTypeGroup
		{
			public string Blueprint = "";
			public string LoadBlueprint = "";
			public string HelpText = "";
			public List<Prop> Props = new List<Prop>();
			public GetGamedataFile.PropObject PropObject;
			public List<GameObject> PropsInstances = new List<GameObject>();
		}
		#endregion

		void Awake()
		{
			Current = this;
			ShowTab(0);
		}


		void OnEnable()
		{
			BrushGenerator.Current.LoadBrushes();
			BrushGenerator.Current.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
			BrushGenerator.Current.Brushes[SelectedFalloff].mipMapBias = -1f;
			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.Brushes[SelectedFalloff]);
			AllowBrushUpdate = true;
		}

		void OnDisable()
		{
			CleanSettingsList();
			TerrainMaterial.SetFloat("_BrushSize", 0);
		}

		void Update()
		{
			if (Tabs[0].activeSelf && AllowBrushUpdate)
			{
				BrushUpdate();
			}
		}

		#region Loading Assets
		public void UnloadProps()
		{
			if (AllPropsTypes != null && AllPropsTypes.Count > 0)
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

		public IEnumerator LoadProps()
		{
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
				if (LoadCounter <= 0)
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


		#region Current Reclaims

		public void ShowReclaimGroups()
		{
			CleanSettingsList();

			if (AllPropsTypes.Count == 0)
			{
				Debug.LogWarning("Props count is 0");
				return;
			}

			for (int i = 0; i < AllPropsTypes.Count; i++)
			{

				GameObject NewListObject = Instantiate(PropGroupObject) as GameObject;
				NewListObject.transform.SetParent(Pivot, false);
				NewListObject.transform.localScale = Vector3.one;
				NewListObject.GetComponent<PropData>().SetPropList(i, AllPropsTypes[i].PropObject.BP.Name, AllPropsTypes[i].PropObject.BP.ReclaimMassMax, AllPropsTypes[i].PropObject.BP.ReclaimEnergyMax, AllPropsTypes[i].Props.Count, AllPropsTypes[i].Blueprint);

				/*
				TotalMassCount += AllPropsTypes[i].Props.Count * AllPropsTypes[i].PropObject.BP.ReclaimMassMax;
				TotalEnergyCount += AllPropsTypes[i].Props.Count * AllPropsTypes[i].PropObject.BP.ReclaimEnergyMax;
				TotalReclaimTime += AllPropsTypes[i].Props.Count * AllPropsTypes[i].PropObject.BP.ReclaimTime;

				TotalMass.text = TotalMassCount.ToString();
				TotalEnergy.text = TotalEnergyCount.ToString();
				TotalTime.text = TotalReclaimTime.ToString();*/
			}
		}

		public void Clean()
		{
			CleanSettingsList();
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

		public void CleanSettingsList()
		{
			if (Pivot.childCount > 0)
			{
				foreach (Transform child in Pivot) Destroy(child.gameObject);
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
					//Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

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

			for (int i = 0; i < PaintPropObjects.Count; i++)
			{
				GameObject NewPropListObject = Instantiate(PaintPropListObject, PaintPropPivot) as GameObject;
				NewPropListObject.GetComponent<PropData>().SetPropPaint(i, PaintPropObjects[i].BP.Name);

			}
		}

		public void ShowPreview(int ID, GameObject Parent)
		{
			Preview.Show(PaintPropObjects[ID].BP.LODs[0].Albedo, Parent, 14f);
		}

		public void ShowTab(int id)
		{
			for (int i = 0; i < Tabs.Length; i++)
			{
				Tabs[i].SetActive(i == id);
				TabSelected[i].SetActive(i == id);
			}


			if (id == 1)
				ShowReclaimGroups();
		}

		public void UpdateBrushMenu(bool Slider)
		{


		}


		#endregion

		#region Brush Update
		Vector3 BrushPos;
		Vector3 MouseBeginClick;
		Vector3 BeginMousePos;
		bool PropsChanged = false;
		public bool AllowBrushUpdate = false;
		float StrengthBeginValue;
		bool ChangingStrength;
		float SizeBeginValue;
		bool ChangingSize;

		void BrushUpdate()
		{
			Invert = Input.GetKey(KeyCode.LeftAlt);

			if (Edit.MauseOnGameplay || ChangingStrength || ChangingSize)
			{
				if (!ChangingSize && (Input.GetKey(KeyCode.M) || ChangingStrength))
				{
					// Change Strength
					if (Input.GetMouseButtonDown(0))
					{
						ChangingStrength = true;
						BeginMousePos = Input.mousePosition;
						StrengthBeginValue = BrushStrengthSlider.value;
					}
					else if (Input.GetMouseButtonUp(0))
					{
						ChangingStrength = false;
					}
					if (ChangingStrength)
					{
						BrushStrengthSlider.value = Mathf.Clamp(StrengthBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.1f), 0, 100);
						UpdateBrushMenu(true);

					}
				}
				else if (Input.GetKey(KeyCode.B) || ChangingSize)
				{
					// Change Size
					if (Input.GetMouseButtonDown(0))
					{
						ChangingSize = true;
						BeginMousePos = Input.mousePosition;
						SizeBeginValue = BrushSizeSlider.value;
					}
					else if (Input.GetMouseButtonUp(0))
					{
						ChangingSize = false;
					}
					if (ChangingSize)
					{
						BrushSizeSlider.value = Mathf.Clamp(SizeBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.4f), 1, 256);
						UpdateBrushMenu(true);
						UpdateBrushPosition(true);

					}
				}
				else
				{
					if (Input.GetMouseButtonDown(0))
					{
						BrushGenerator.Current.UpdateSymmetryType();

						if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition(true))
						{
							SymmetryPaint();
						}
					}
					else if (Input.GetMouseButton(0))
					{
						if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition(false))
						{
							SymmetryPaint();
						}
					}
					else
					{
						UpdateBrushPosition(true);
					}
				}
			}
		}

		bool UpdateBrushPosition(bool Forced = false)
		{
			//Debug.Log(Vector3.Distance(MouseBeginClick, Input.mousePosition));
			if (Forced || Vector3.Distance(MouseBeginClick, Input.mousePosition) > 1) { }
			else
			{
				return false;
			}


			MouseBeginClick = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 2000, TerrainMask))
			{
				BrushPos = hit.point;
				BrushPos.y = ScmapEditor.Current.Teren.SampleHeight(BrushPos);

				Vector3 tempCoord = ScmapEditor.Current.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
				Vector3 coord = Vector3.zero;
				coord.x = (tempCoord.x - (int)BrushSizeSlider.value * MapLuaParser.Current.ScenarioData.Size.x * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.x; // TODO 0.05 ?? this should be terrain proportion?
																																										  //coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
				coord.z = (tempCoord.z - (int)BrushSizeSlider.value * MapLuaParser.Current.ScenarioData.Size.y * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.z;

				TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value);
				TerrainMaterial.SetFloat("_BrushUvX", coord.x);
				TerrainMaterial.SetFloat("_BrushUvY", coord.z);

				return true;
			}
			return false;
		}

		#endregion

		#region Painting
		float size = 0;
		int RandomProp = 0;
		public void SymmetryPaint()
		{
			int Count = PaintPropObjects.Count;

			if (Count <= 0)
				return;
			if (Random.Range(0, 100) > BrushStrengthSlider.value)
				return;

			size = BrushSizeSlider.value * 0.03f;
			BrushGenerator.Current.GenerateSymmetry(BrushPos, size, float.Parse(Scatter.text), size);
			BrushGenerator.Current.GenerateRotationSymmetry(Quaternion.Euler(Vector3.up * Random.Range(0, 360)));

			RandomProp = Random.Range(0, Count);

			for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
			{
				if (Invert)
				{ }
				else
					Paint(BrushGenerator.Current.PaintPositions[i], BrushGenerator.Current.PaintRotations[i]);
			}
		}

		void Paint(Vector3 AtPosition, Quaternion Rotation)
		{

			AtPosition.y = ScmapEditor.Current.Teren.SampleHeight(AtPosition);

			PaintPropObjects[RandomProp].CreatePropGameObject(AtPosition, Rotation);


		}

		#endregion
	}
}