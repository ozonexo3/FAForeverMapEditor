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
		public GameObject PropObjectPrefab;
		public Transform PropsParent;

		[Header("Brush")]
		public Slider BrushSizeSlider;
		public InputField BrushSize;
		public Slider BrushStrengthSlider;
		public InputField BrushStrength;
		public InputField BrushMini;
		public InputField BrushMax;
		public InputField Scatter;
		public Toggle AllowWaterLevel;
		public Toggle SnapToGround;
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
			//public Prop[] Props = new Prop[0];
			public GetGamedataFile.PropObject PropObject;
			public List<PropGameObject> PropsInstances = new List<PropGameObject>();

			public PropTypeGroup()
			{
				PropsInstances = new List<PropGameObject>();
			}

			public PropTypeGroup(GetGamedataFile.PropObject FromPropObject)
			{
				PropObject = FromPropObject;
				Blueprint = PropObject.BP.Path;
				HelpText = PropObject.BP.HelpText;

				PropsInstances = new List<PropGameObject>();
			}

			public Prop[] GenerateSupComProps()
			{
				int count = PropsInstances.Count;
				Prop[] Props = new Prop[count];

				for (int i = 0; i < count; i++)
				{
					Props[i] = new Prop();
					if (!Blueprint.StartsWith("/"))
						Blueprint = "/" + Blueprint;

					Props[i].BlueprintPath = Blueprint;
					Props[i].Position = ScmapEditor.WorldPosToScmap(PropsInstances[i].Tr.position);
					Props[i].RotationX = Vector3.zero;
					Props[i].RotationY = Vector3.zero;
					Props[i].RotationZ = Vector3.zero;
					MassMath.QuaternionToRotationMatrix(PropsInstances[i].Tr.localRotation, ref Props[i].RotationX, ref Props[i].RotationY, ref Props[i].RotationZ);

					Vector3 Scale = PropsInstances[i].Tr.localScale;
					Scale.x /= PropObject.BP.LocalScale.x;
					Scale.y /= PropObject.BP.LocalScale.y;
					Scale.z /= PropObject.BP.LocalScale.z;

					Props[i].Scale = Scale;
				}
				return Props;
			}
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
			TerrainMaterial.SetInt("_Brush", 1);

			BrushGenerator.Current.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
			BrushGenerator.Current.Brushes[SelectedFalloff].mipMapBias = -1f;
			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.Brushes[SelectedFalloff]);
			AllowBrushUpdate = true;
		}

		void OnDisable()
		{
			CleanSettingsList();
			TerrainMaterial.SetInt("_Brush", 0);
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
		public static void UnloadProps()
		{
			if (AllPropsTypes != null && AllPropsTypes.Count > 0)
				for (int i = 0; i < AllPropsTypes.Count; i++)
				{
					for (int p = 0; p < AllPropsTypes[i].PropsInstances.Count; p++)
					{
						Destroy(AllPropsTypes[i].PropsInstances[p].gameObject);
					}
				}

			AllPropsTypes = new List<PropTypeGroup>();
			if (Current)
			{
				Current.TotalMassCount = 0;
				Current.TotalEnergyCount = 0;
				Current.TotalReclaimTime = 0;
			}
		}

		public IEnumerator LoadProps()
		{
			UnloadProps();

			List<Prop> Props = ScmapEditor.Current.map.Props;

			//Debug.Log("Found props: " + Props.Count);

			const int YieldStep = 1000;
			int LoadCounter = YieldStep;
			int Count = Props.Count;

			for (int i = 0; i < Count; i++)
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
				AllPropsTypes[GroupId].PropsInstances.Add(
					AllPropsTypes[GroupId].PropObject.CreatePropGameObject(
						ScmapEditor.ScmapPosToWorld(Props[i].Position),
						MassMath.QuaternionFromRotationMatrix(Props[i].RotationX, Props[i].RotationY, Props[i].RotationZ), 
						Props[i].Scale
						)
					);
				LoadCounter--;
				if (LoadCounter <= 0)
				{
					LoadCounter = YieldStep;
					yield return null;
				}


				TotalMassCount += AllPropsTypes[GroupId].PropObject.BP.ReclaimMassMax;
				TotalEnergyCount += AllPropsTypes[GroupId].PropObject.BP.ReclaimEnergyMax;
				TotalReclaimTime += AllPropsTypes[GroupId].PropObject.BP.ReclaimTime;
			}

			TotalMass.text = TotalMassCount.ToString();
			TotalEnergy.text = TotalEnergyCount.ToString();
			TotalTime.text = TotalReclaimTime.ToString();

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
				NewListObject.GetComponent<PropData>().SetPropList(i, AllPropsTypes[i].PropObject.BP.Name, AllPropsTypes[i].PropObject.BP.ReclaimMassMax, AllPropsTypes[i].PropObject.BP.ReclaimEnergyMax, AllPropsTypes[i].PropsInstances.Count, AllPropsTypes[i].Blueprint);

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
			PaintButtons = new List<PropData>();
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
		List<PropData> PaintButtons = new List<PropData>();

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
					PaintButtons.Add(NewPropListObject.GetComponent<PropData>());
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
			PaintButtons.RemoveAt(ID);

			for (int i = 0; i < PaintPropObjects.Count; i++)
			{
				GameObject NewPropListObject = Instantiate(PaintPropListObject, PaintPropPivot) as GameObject;
				NewPropListObject.GetComponent<PropData>().SetPropPaint(i, PaintPropObjects[i].BP.Name);

			}
		}

		public void ShowPreview(int ID, GameObject Parent)
		{
			Preview.Show(PaintPropObjects[ID].BP.LODs[0].Albedo, Parent, 35f);
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


		bool InforeUpdate = false;
		public void UpdateBrushMenu(bool Slider)
		{
			if (InforeUpdate)
				return;

			if (Slider)
			{
				InforeUpdate = true;
				UpdateBrushPosition(true);

				BrushSize.text =  BrushSizeSlider.value.ToString();
				BrushStrength.text = BrushStrengthSlider.value.ToString();

				InforeUpdate = false;
			}
			else
			{
				InforeUpdate = true;
				
				BrushSizeSlider.value = Mathf.Clamp( MassMath.StringToFloat(BrushSize.text), MinimumBrushSize, MaximumBrushSize);
				BrushStrengthSlider.value = Mathf.Clamp(MassMath.StringToFloat(BrushStrength.text), 0, 100);

				InforeUpdate = false;
				UpdateBrushMenu(true);
			}

		}


		#endregion

		#region Brush Update
		Vector3 BrushPos;
		Vector3 MouseBeginClick;
		Vector3 BeginMousePos;
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
						BrushSizeSlider.value = Mathf.Clamp(SizeBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 4f) * 0.025f, MinimumBrushSize, MaximumBrushSize);
						UpdateBrushMenu(true);
						

					}
				}
				else
				{
					if (Input.GetMouseButtonDown(0))
					{
						BrushGenerator.Current.UpdateSymmetryType();

						if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition(true))
						{
							SymmetryPaint(true);
						}
					}
					else if (Input.GetMouseButton(0))
					{
						if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition(false))
						{
							SymmetryPaint(false);
						}
					}
					else
					{
						UpdateBrushPosition(true);
					}
				}
			}
		}

		const float MinimumRenderBrushSize = 0.1f;
		const float MinimumBrushSize = 0.0f;
		const float MaximumBrushSize = 256;
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
				if (SnapToGround.isOn && BrushSizeSlider.value < 1.5f)
					BrushPos = Vector3.Lerp(ScmapEditor.SnapToSmallGridCenter(BrushPos), BrushPos, (BrushSizeSlider.value - 0.2f) / 1.5f);
				BrushPos.y = ScmapEditor.Current.Teren.SampleHeight(BrushPos);

				Vector3 tempCoord = ScmapEditor.Current.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
				Vector3 coord = Vector3.zero;

				float SizeValue = Mathf.Clamp(BrushSizeSlider.value, MinimumRenderBrushSize * 2, MaximumBrushSize);

				coord.x = (tempCoord.x - SizeValue * MapLuaParser.GetMapSizeX() * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.x; // TODO 0.05 ?? this should be terrain proportion?
																																										  //coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
				coord.z = (tempCoord.z - SizeValue * MapLuaParser.GetMapSizeY() * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.z;

				TerrainMaterial.SetFloat("_BrushSize", SizeValue);
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
		int RandomPropGroup = 0;
		float RandomScale = 1f;
		float StepCount = 100;
		public void SymmetryPaint(bool forced = false)
		{
			int Count = PaintPropObjects.Count;
			if (Count <= 0 && !Invert)
			{
#if UNITY_EDITOR
				Debug.Log("No props selected");

#endif
				return;
			}

			size = BrushSizeSlider.value * MapLuaParser.GetMapSizeX() * 0.0001f;

			//float BrushField = Mathf.PI * (size * size);
			//BrushField /= 16f;

			//Debug.Log(size + ", " + BrushField);

			// Check if paint
			StepCount --;
			if (StepCount >= Mathf.Lerp(BrushStrengthSlider.value, 100, Mathf.Sqrt(size / 7f)) && !forced)
				return;

			StepCount = 100;

			//Real brush size

			if (SnapToGround.isOn)
			{
				BrushPos = ScmapEditor.SnapToSmallGridCenter(BrushPos);

			}


			if (Invert)
			{
				float Tolerance = SymmetryWindow.GetTolerance();

				BrushGenerator.Current.GenerateSymmetry(BrushPos, 0, float.Parse(Scatter.text), size);

				float SearchSize = Mathf.Clamp(size, MinimumRenderBrushSize, MaximumBrushSize);

				// Search props by grid
				int ClosestG = -1;
				int ClosestP = -1;
				SearchClosestProp(BrushGenerator.Current.PaintPositions[0], SearchSize, out ClosestG, out ClosestP);

				if (ClosestG < 0 || ClosestP < 0)
					return; // No props found

				BrushPos = AllPropsTypes[ClosestG].PropsInstances[ClosestP].transform.position;
				BrushGenerator.Current.GenerateSymmetry(BrushPos, 0, 0, 0);

				for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
				{
					if(i == 0)
					{
						Destroy(AllPropsTypes[ClosestG].PropsInstances[ClosestP].gameObject);
						AllPropsTypes[ClosestG].PropsInstances.RemoveAt(ClosestP);
					}
					else
					{
						SearchClosestProp(BrushGenerator.Current.PaintPositions[i], Tolerance, out ClosestG, out ClosestP);
						if (ClosestG >= 0 && ClosestP >= 0)
						{
							Destroy(AllPropsTypes[ClosestG].PropsInstances[ClosestP].gameObject);
							AllPropsTypes[ClosestG].PropsInstances.RemoveAt(ClosestP);
						}
					}
				}

			}
			else
			{

				RandomProp = Random.Range(0, Count);
				RandomScale = Random.Range(MassMath.StringToFloat(PaintButtons[RandomProp].ScaleMin.text), MassMath.StringToFloat(PaintButtons[RandomProp].ScaleMax.text));

				BrushGenerator.Current.GenerateSymmetry(BrushPos, size, float.Parse(Scatter.text), size);

				float RotMin = MassMath.StringToFloat(PaintButtons[RandomProp].RotationMin.text);
				float RotMax = MassMath.StringToFloat(PaintButtons[RandomProp].RotationMax.text);

				BrushGenerator.Current.GenerateRotationSymmetry(Quaternion.Euler(Vector3.up * Random.Range(RotMin, RotMax)));



				// Search group id
				RandomPropGroup = -1;
				for (int i = 0; i < AllPropsTypes.Count; i++)
				{
					if (AllPropsTypes[i].Blueprint == PaintPropObjects[RandomProp].BP.Path)
					{
						RandomPropGroup = i;
						break;
					}
				}
				if(RandomPropGroup < 0) // Create new group
				{
					PropTypeGroup NewGroup = new PropTypeGroup(PaintPropObjects[RandomProp]);
					RandomPropGroup = AllPropsTypes.Count;
					AllPropsTypes.Add(NewGroup);
				}

				//float BrushSlope = ScmapEditor.Current.Teren.
				int Min = int.Parse( BrushMini.text);
				int Max = int.Parse(BrushMax.text);

				if (Min > 0 || Max < 90)
				{

					Vector3 LocalPos = ScmapEditor.Current.Teren.transform.InverseTransformPoint(BrushGenerator.Current.PaintPositions[0]);
					LocalPos.x /= ScmapEditor.Current.Teren.terrainData.size.x;
					LocalPos.z /= ScmapEditor.Current.Teren.terrainData.size.z;

					float angle = Vector3.Angle(Vector3.up, ScmapEditor.Current.Teren.terrainData.GetInterpolatedNormal(LocalPos.x, LocalPos.z));
					if ((angle < Min && Min > 0) || (angle > Max && Max < 90))
						return;
				}

				if (!AllowWaterLevel.isOn && ScmapEditor.Current.Teren.SampleHeight(BrushGenerator.Current.PaintPositions[0]) <= ScmapEditor.Current.WaterLevel.position.y)
					return;

				for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
				{
					Paint(BrushGenerator.Current.PaintPositions[i], BrushGenerator.Current.PaintRotations[i]);
				}
			}
		}

		void Paint(Vector3 AtPosition, Quaternion Rotation)
		{
			AtPosition.y = ScmapEditor.Current.Teren.SampleHeight(AtPosition);

			AllPropsTypes[RandomPropGroup].PropsInstances.Add(PaintPropObjects[RandomProp].CreatePropGameObject(AtPosition, Rotation, Vector3.one * RandomScale));
		}



		void SearchClosestProp(Vector3 Pos, float tolerance, out int ClosestG, out int ClosestP)
		{
			int GroupsCount = AllPropsTypes.Count;
			int PropsCount = 0;

			int g = 0;
			int p = 0;
			float dist = 0;

			ClosestG = -1;
			ClosestP = -1;
			float ClosestDist = 1000000f;

			for (g = 0; g < AllPropsTypes.Count; g++)
			{
				PropsCount = AllPropsTypes[g].PropsInstances.Count;

				for (p = 0; p < PropsCount; p++)
				{
					dist = Vector3.Distance(Pos, AllPropsTypes[g].PropsInstances[p].transform.position);
					if (dist < ClosestDist && dist < tolerance)
					{
						ClosestG = g;
						ClosestP = p;
						ClosestDist = dist;
					}
				}
			}
		}

		#endregion


		public void UpdatePropsHeight()
		{

		}

	}
}