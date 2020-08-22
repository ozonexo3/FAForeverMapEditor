using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Ozone.UI;
using Selection;
using SFB;
using System.IO;
using FAF.MapEditor;

namespace EditMap
{
	public partial class PropsInfo : MonoBehaviour
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

		[Header("Editing")]
		public Toggle FreeRotation;

		[Header("Brush")]
		//public Slider BrushSizeSlider;
		public UiTextField BrushSize;
		//public Slider BrushStrengthSlider;
		public UiTextField BrushStrength;
		public AnimationCurve StrengthToField;
		public UiTextField BrushMini;
		public UiTextField BrushMax;
		public UiTextField Scatter;
		public Toggle AllowWaterLevel;
		public Toggle SnapToGround;
		public LayerMask TerrainMask;
		public Material TerrainMaterial;
		public Material PropMaterial;
		public Material UnitMaterial;
		public Mesh NoPropMesh;
		public Material NoPropMaterial;

		public static float TotalMassCount { get; private set; } = 0;
		public static float TotalEnergyCount { get; private set; } = 0;
		public static float TotalReclaimTime { get; private set; } = 0;

		const int SelectedFalloff = 0;


		[Header("State")]
		public bool Invert;

		#region Classes
		public class PropTypeGroup
		{
			public string Blueprint = "";
			public string LoadBlueprint
			{
				get
				{
					return GetGamedataFile.LocalBlueprintPath(Blueprint);
				}
			}

			//public string LoadBlueprint = "";
			public string HelpText = "";
			//public Prop[] Props = new Prop[0];
			public GetGamedataFile.PropObject PropObject;
			public HashSet<Prop> PropsInstances = new HashSet<Prop>();

			public void SetNewInstances(HashSet<Prop> NewProps)
			{
				HashSet<PropGameObject> ToRemove = new HashSet<PropGameObject>();

				foreach (Prop OldInstance in PropsInstances)
				{
					if (!NewProps.Contains(OldInstance) && OldInstance.Obj)
					{
						ToRemove.Add(OldInstance.Obj);
					}
				}

				foreach (Prop NewInstance in NewProps)
				{
					if (!NewInstance.Obj) // !PropsInstances.Contains(NewInstance)
					{
						NewInstance.Group = this;
						NewInstance.CreateObject();
					}
				}

				foreach (PropGameObject PropObj in ToRemove)
				{
					Destroy(PropObj.gameObject);
				}

				PropsInstances.Clear();
				PropsInstances = new HashSet<Prop>(NewProps);
			}

			public PropTypeGroup()
			{
				PropsInstances = new HashSet<Prop>();
			}

			public PropTypeGroup(GetGamedataFile.PropObject FromPropObject)
			{
				PropObject = FromPropObject;
				Blueprint = PropObject.BP.Path;
				HelpText = PropObject.BP.HelpText;

				PropsInstances = new HashSet<Prop>();
			}

			public Prop[] GenerateSupComProps()
			{
				int count = PropsInstances.Count;
				Prop[] Props = new Prop[count];

				int i = 0;
				foreach (Prop PropInstance in PropsInstances)
				{
					PropInstance.Bake();

					Props[i] = PropInstance;
					i++;
				}
				return Props;
			}
		}
		#endregion

		void Awake()
		{
			Current = this;
			ShowTab(CurrentTab);
		}


		void OnEnable()
		{
			PlacementManager.OnDropOnGameplay += DropAtGameplay;
			SelectionManager.Current.DisableLayer = 16;
			SelectionManager.Current.SetRemoveAction(DestroyProps);
			SelectionManager.Current.SetSelectionChangeAction(SelectProp);
			SelectionManager.Current.SetCustomSnapAction(SnapAction);
			SelectionManager.Current.SetCopyActionAction(CopyAction);
			SelectionManager.Current.SetPasteActionAction(PasteAction);

			ShowTab(CurrentTab);

			ReloadPropStats();

			MapLuaParser.Current.UpdateArea();
		}

		void OnDisable()
		{
			SelectionManager.Current.ClearAffectedGameObjects();
			PlacementManager.Clear();
			DisableBrush();
			CleanSettingsList();

			UndoRegistered = false;
		}

		void DisableBrush()
		{
			TerrainMaterial.SetInt("_Brush", 0);
			TerrainMaterial.SetFloat("_BrushSize", 0);
		}

		GameObject[] AllObjects;
		void GoToSelection()
		{
			DisableBrush();

			PlacementManager.Clear();
			SelectionManager.Current.CleanSelection();

			int[] AllTypes;
			AllObjects = AllPropGameObjects(out AllTypes);
			SelectionManager.Current.SetAffectedGameObjects(AllObjects, SelectionManager.SelectionControlTypes.Props);
			SelectionManager.Current.SetAffectedTypes(AllTypes);

			AllowBrushUpdate = false;

			//OnChangeFreeRotation();

			if (ChangeControlerType.Current)
				ChangeControlerType.Current.UpdateButtons();
		}

		void GoToPainting()
		{
			PlacementManager.Clear();
			SelectionManager.Current.CleanSelection();
			SelectionManager.Current.SetAffectedGameObjects(new GameObject[0], SelectionManager.SelectionControlTypes.None);

			BrushGenerator.Current.LoadBrushes();
			TerrainMaterial.SetInt("_Brush", 1);

			BrushGenerator.Current.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
			BrushGenerator.Current.Brushes[SelectedFalloff].mipMapBias = -1f;
			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.Brushes[SelectedFalloff]);
			AllowBrushUpdate = true;
			UndoRegistered = false;
		}

		void Update()
		{
			if (Tabs[1].activeSelf && AllowBrushUpdate)
			{
				BrushUpdate();
			}
		}

		#region Loading Assets
		public static void UnloadProps()
		{
			PropsRenderer.StopPropsUpdate();

			if (AllPropsTypes != null && AllPropsTypes.Count > 0)
				for (int i = 0; i < AllPropsTypes.Count; i++)
				{
					foreach (Prop PropInstance in AllPropsTypes[i].PropsInstances)
					{
						Destroy(PropInstance.Obj.gameObject);
					}
				}

			AllPropsTypes = new List<PropTypeGroup>();
			if (Current)
			{
				TotalMassCount = 0;
				TotalEnergyCount = 0;
				TotalReclaimTime = 0;
			}
		}

		int GetPropType(string blueprint)
		{
			int AllPropsTypesCount = AllPropsTypes.Count;
			if (AllPropsTypesCount == 0)
			{

			}
			else
			{
				for (int g = 0; g < AllPropsTypesCount; g++)
				{
					if (blueprint == AllPropsTypes[g].Blueprint)
					{
						return g;
					}
				}
			}

			AllPropsTypes.Add(new PropTypeGroup());
			AllPropsTypes[AllPropsTypesCount].Blueprint = blueprint;
			AllPropsTypes[AllPropsTypesCount].PropObject = GetGamedataFile.LoadProp(blueprint);
			return AllPropsTypesCount;
		}

		public bool LoadingProps;
		public int LoadedCount = 0;
		public IEnumerator LoadProps()
		{
			LoadingProps = true;
			UnloadProps();

			List<Prop> Props = ScmapEditor.Current.map.Props;

			//Debug.Log("Found props: " + Props.Count);

			const int YieldStep = 1000;
			int LoadCounter = YieldStep;
			int Count = Props.Count;
			LoadedCount = 0;

			bool AllowFarLod = Count < 10000;

			for (int i = 0; i < Count; i++)
			{
				int GroupId = GetPropType(Props[i].BlueprintPath);

				Props[i].GroupId = GroupId;
				Props[i].CreateObject(AllowFarLod);

				AllPropsTypes[GroupId].PropsInstances.Add(Props[i]);

				LoadedCount++;
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

			UpdatePropStats();

			yield return null;
			LoadingProps = false;
		}

		public void ReloadPropStats()
		{
			TotalMassCount = 0;
			TotalEnergyCount = 0;
			TotalReclaimTime = 0;

			int AllPropsTypesCount = AllPropsTypes.Count;
			for (int i = 0; i < AllPropsTypesCount; i++)
			{
				int InstancesCount = AllPropsTypes[i].PropsInstances.Count;
				TotalMassCount += AllPropsTypes[i].PropObject.BP.ReclaimMassMax * InstancesCount;
				TotalEnergyCount += AllPropsTypes[i].PropObject.BP.ReclaimEnergyMax * InstancesCount;
				TotalReclaimTime += AllPropsTypes[i].PropObject.BP.ReclaimTime * InstancesCount;
			}
			UpdatePropStats();
		}

		void UpdatePropStats()
		{
			TotalMass.text = TotalMassCount.ToString("N");
			TotalEnergy.text = TotalEnergyCount.ToString("N");
			TotalTime.text = TotalReclaimTime.ToString("N");
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
			if (ResourceBrowser.DragedObject && ResourceBrowser.DragedObject.ContentType == ResourceObject.ContentTypes.Prop)
			{

				if (!ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
					return;
				if (ResourceBrowser.SelectedCategory == 3)
				{
					LoadProp(ResourceBrowser.Current.LoadedProps[ResourceBrowser.DragedObject.InstanceId]);
					ResourceBrowser.ClearDrag();
				}
			}
		}

		bool LoadProp(GetGamedataFile.PropObject PropObj)
		{
			if (!PaintPropObjects.Contains(PropObj))
			{
				PaintPropObjects.Add(PropObj);

				GameObject NewPropListObject = Instantiate(PaintPropListObject, PaintPropPivot) as GameObject;
				PropData pb = NewPropListObject.GetComponent<PropData>();
				pb.SetPropPaint(PaintPropObjects.Count - 1, PropObj.BP.Name);
				PaintButtons.Add(pb);
				return true;
			}
			return false;
		}

		public void RemoveProp(int ID)
		{
			//CleanPaintList();
			Preview.Hide(PaintPropPivot.GetChild(ID).gameObject);
			Destroy(PaintButtons[ID].gameObject);
			PaintPropObjects.RemoveAt(ID);
			PaintButtons.RemoveAt(ID);

			for (int i = 0; i < PaintPropObjects.Count; i++)
			{
				//GameObject NewPropListObject = Instantiate(PaintPropListObject, PaintPropPivot) as GameObject;
				//NewPropListObject.GetComponent<PropData>().SetPropPaint(i, PaintPropObjects[i].BP.Name);

				PaintButtons[i].SetPropPaint(i, PaintPropObjects[i].BP.Name);
			}
		}

		public void ShowPreview(int ID, GameObject Parent)
		{
			Preview.Show(PaintPropObjects[ID].BP.LODs[0].Albedo, Parent, 35f);
		}

		public int CurrentTab { get; private set; } = 0;

		public void ShowTab(int id)
		{
			for (int i = 0; i < Tabs.Length; i++)
			{
				Tabs[i].SetActive(i == id);
				TabSelected[i].SetActive(i == id);
			}

			CurrentTab = id;

			if (id == 2)
				ShowReclaimGroups();

			if (id == 1)
				GoToPainting();
			else
				GoToSelection();
		}


		bool InforeUpdate = false;
		public void UpdateBrushMenu(bool Slider)
		{
			if (InforeUpdate)
				return;

			UpdateBrushPosition(true);
		}

		public void OnChangeFreeRotation()
		{
			int Angle = FreeRotation.isOn ? (0) : (45);
			SelectionManager.Current.ChangeMinAngle(Angle);
			PlacementManager.MinRotAngle = Angle;
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
				if (!ChangingSize && (KeyboardManager.BrushStrengthHold() || ChangingStrength))
				{
					// Change Strength
					if (Input.GetMouseButtonDown(0))
					{
						ChangingStrength = true;
						BeginMousePos = Input.mousePosition;
						StrengthBeginValue = BrushStrength.value;
					}
					else if (Input.GetMouseButtonUp(0))
					{
						ChangingStrength = false;
						UndoRegistered = false;
					}
					if (ChangingStrength)
					{
						BrushStrength.SetValue(Mathf.Clamp(StrengthBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.1f), 0, 100));
						UpdateBrushMenu(true);

					}
				}
				else if (KeyboardManager.BrushSizeHold() || ChangingSize)
				{
					// Change Size
					if (Input.GetMouseButtonDown(0))
					{
						ChangingSize = true;
						BeginMousePos = Input.mousePosition;
						SizeBeginValue = BrushSize.value;
					}
					else if (Input.GetMouseButtonUp(0))
					{
						ChangingSize = false;
						UndoRegistered = false;
					}
					if (ChangingSize)
					{
						BrushSize.SetValue(Mathf.Clamp(SizeBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 4f) * 0.075f, MinimumBrushSize, MaximumBrushSize));
						//BrushSize.SetValue(Mathf.Clamp(SizeBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.4f), 0, 256));
						UpdateBrushPosition(true, true, true);
					}
				}
				else
				{
					if (Edit.MauseOnGameplay && Input.GetMouseButtonDown(0))
					{
						BrushGenerator.Current.UpdateSymmetryType();

						if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition(true))
						{
							SymmetryPaint(true);
						}
					}
					else if (Input.GetMouseButton(0))
					{
						if (CameraControler.Current.DragStartedGameplay)
						{
							if (UpdateBrushPosition(false))
							{
								SymmetryPaint(false);
							}
						}
					}
					else if (Input.GetMouseButtonUp(0))
					{
						if (Painting)
						{
							UpdatePropStats();
							Painting = false;
						}

						UndoRegistered = false;
						UpdateBrushPosition(true);
					}
					else
					{
						UpdateBrushPosition(true);
					}
				}
			}
		}

		bool UndoRegistered = false;

		const float MinimumRenderBrushSize = 0.1f;
		const float MinimumBrushSize = 0.0f;
		const float MaximumBrushSize = 256;
		bool UpdateBrushPosition(bool Forced = false, bool Size = true, bool Position = true)
		{
			//Debug.Log(Vector3.Distance(MouseBeginClick, Input.mousePosition));
			if (Forced || Vector3.Distance(MouseBeginClick, Input.mousePosition) > 1) { }
			else
			{
				return false;
			}

			float SizeXprop = MapLuaParser.GetMapSizeX() / 512f;
			float SizeZprop = MapLuaParser.GetMapSizeY() / 512f;
			float BrushSizeValue = BrushSize.value;
			if (BrushSizeValue < 0.2f)
				BrushSizeValue = 0.2f;

			if (Size)
				TerrainMaterial.SetFloat("_BrushSize", BrushSizeValue / ((SizeXprop + SizeZprop) / 2f));


			MouseBeginClick = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Position && Physics.Raycast(ray, out hit, 2000, TerrainMask))
			{
				BrushPos = hit.point;
				if (SnapToGround.isOn && BrushSize.value < 1.5f)
				{
					//BrushPos = Vector3.Lerp(ScmapEditor.SnapToSmallGridCenter(BrushPos), BrushPos, (BrushSize.value - 0.2f) / 1.5f);
					BrushPos = ScmapEditor.SnapToSmallGrid(BrushPos + new Vector3(0.025f, 0, -0.025f));
				}

				BrushPos.y = ScmapEditor.Current.Teren.SampleHeight(BrushPos);

				Vector3 tempCoord = ScmapEditor.Current.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
				Vector3 coord = Vector3.zero;
				float SizeX = (int)((BrushSizeValue / SizeXprop) * 100) * 0.01f;
				float SizeZ = (int)((BrushSizeValue / SizeZprop) * 100) * 0.01f;
				coord.x = (tempCoord.x - SizeX * MapLuaParser.GetMapSizeX() * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.x;
				coord.z = (tempCoord.z - SizeZ * MapLuaParser.GetMapSizeY() * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.z;

				TerrainMaterial.SetFloat("_BrushUvX", coord.x);
				TerrainMaterial.SetFloat("_BrushUvY", coord.z);

				return true;
			}
			return false;
		}

		#endregion

		#region Painting
		bool _Painting = false;
		bool Painting
		{
			set
			{
				TerrainMaterial.SetInt("_BrushPainting", _Painting ? (1) : (0));
				_Painting = value;
			}
			get
			{
				return _Painting;
			}
		}

		float size = 0;
		int RandomProp = 0;
		int RandomPropGroup = 0;
		//float RandomScale = 1f;
		float StepCount = 100;
		public void SymmetryPaint(bool forced = false)
		{
			Painting = true;
			int Count = PaintPropObjects.Count;
			if (Count <= 0 && !Invert)
			{
				return;
			}
			size = BrushSize.value * 0.05f;

			float BrushField = Mathf.PI * Mathf.Pow(size, 2);

			StepCount += BrushField * StrengthToField.Evaluate(BrushStrength.value);

			if (forced)
				StepCount = 101;

			while (StepCount > 100)
			{
				StepCount -= 100;
				DoPaintSymmetryPaint();
			}
		}

		void DoPaintSymmetryPaint()
		{
			if (Invert)
			{
				float Tolerance = SymmetryWindow.GetTolerance();

				BrushGenerator.Current.GenerateSymmetry(BrushPos, 0, Scatter.value, size);

				float SearchSize = Mathf.Clamp(size, MinimumRenderBrushSize, MaximumBrushSize);

				PropGameObject ClosestInstance = SearchClosestProp(BrushGenerator.Current.PaintPositions[0], SearchSize);

				if (ClosestInstance == null)
					return; // No props found

				BrushPos = ClosestInstance.transform.position;
				BrushGenerator.Current.GenerateSymmetry(BrushPos, 0, 0, 0);

				for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
				{
					if (i == 0)
					{
						RegisterUndo();


						TotalMassCount -= ClosestInstance.Connected.Group.PropObject.BP.ReclaimMassMax;
						TotalEnergyCount -= ClosestInstance.Connected.Group.PropObject.BP.ReclaimEnergyMax;
						TotalReclaimTime -= ClosestInstance.Connected.Group.PropObject.BP.ReclaimTime;

						ClosestInstance.Connected.Group.PropsInstances.Remove(ClosestInstance.Connected);
						Destroy(ClosestInstance.gameObject);
					}
					else
					{
						PropGameObject TestObj = SearchClosestProp(BrushGenerator.Current.PaintPositions[i], Tolerance);
						if (TestObj != null)
						{
							TotalMassCount -= TestObj.Connected.Group.PropObject.BP.ReclaimMassMax;
							TotalEnergyCount -= TestObj.Connected.Group.PropObject.BP.ReclaimEnergyMax;
							TotalReclaimTime -= TestObj.Connected.Group.PropObject.BP.ReclaimTime;

							TestObj.Connected.Group.PropsInstances.Remove(TestObj.Connected);
							Destroy(TestObj.gameObject);

						}
					}
				}

			}
			else
			{

				RandomProp = GetRandomProp();

				BrushGenerator.Current.GenerateSymmetry(BrushPos, size, Scatter.value, size);

				float RotMin = PaintButtons[RandomProp].RotationMin.intValue;
				float RotMax = PaintButtons[RandomProp].RotationMax.intValue;

				BrushGenerator.Current.GenerateRotationSymmetry(Quaternion.Euler(Vector3.up * Random.Range(RotMin, RotMax)));



				// Search group id
				RandomPropGroup = -1;
				for (int i = 0; i < AllPropsTypes.Count; i++)
				{
					if (AllPropsTypes[i].LoadBlueprint == PaintPropObjects[RandomProp].BP.Path)
					{
						RandomPropGroup = i;
						break;
					}
				}
				if (RandomPropGroup < 0) // Create new group
				{
					PropTypeGroup NewGroup = new PropTypeGroup(PaintPropObjects[RandomProp]);
					RandomPropGroup = AllPropsTypes.Count;
					AllPropsTypes.Add(NewGroup);
				}

				//float BrushSlope = ScmapEditor.Current.Teren.
				int Min = BrushMini.intValue;
				int Max = BrushMax.intValue;

				if (Min > 0 || Max < 90)
				{

					Vector3 LocalPos = ScmapEditor.Current.Teren.transform.InverseTransformPoint(BrushGenerator.Current.PaintPositions[0]);
					LocalPos.x /= ScmapEditor.Current.Teren.terrainData.size.x;
					LocalPos.z /= ScmapEditor.Current.Teren.terrainData.size.z;

					float angle = Vector3.Angle(Vector3.up, ScmapEditor.Current.Teren.terrainData.GetInterpolatedNormal(LocalPos.x, LocalPos.z));
					if ((angle < Min && Min > 0) || (angle > Max && Max < 90))
						return;
				}

				if (!AllowWaterLevel.isOn && ScmapEditor.Current.map.Water.HasWater)
					if (ScmapEditor.Current.Teren.SampleHeight(BrushGenerator.Current.PaintPositions[0]) <= ScmapEditor.Current.WaterLevel.position.y)
						return;

				for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
				{
					Paint(BrushGenerator.Current.PaintPositions[i], BrushGenerator.Current.PaintRotations[i]);
				}
			}
		}

		int GetRandomProp()
		{
			int Count = PaintPropObjects.Count;
			int TotalValue = 0;
			for (int i = 0; i < Count; i++)
			{
				TotalValue += PaintButtons[i].Chance.intValue;
			}

			int RandomInt = Random.Range(0, TotalValue);


			TotalValue = 0;
			for (int i = 0; i < Count; i++)
			{
				TotalValue += PaintButtons[i].Chance.intValue;

				if (RandomInt < TotalValue)
					return i;
			}
			return Count - 1;


			//return Random.Range(0, PaintPropObjects.Count);
		}

		void RegisterUndo()
		{
			if (UndoRegistered)
				return;
			UndoRegistered = true;
			Undo.RegisterUndo(new UndoHistory.HistoryPropsChange());
		}

		PropGameObject Paint(Vector3 AtPosition, Quaternion Rotation)
		{
			if (!MapLuaParser.IsInArea(AtPosition))
				return null;

			RegisterUndo();

			AtPosition.y = ScmapEditor.Current.Teren.SampleHeight(AtPosition);

			Prop NewProp = new Prop();
			NewProp.GroupId = RandomPropGroup;
			NewProp.CreateObject(AtPosition, Rotation, Vector3.one);

			AllPropsTypes[RandomPropGroup].PropsInstances.Add(NewProp);

			TotalMassCount += AllPropsTypes[RandomPropGroup].PropObject.BP.ReclaimMassMax;
			TotalEnergyCount += AllPropsTypes[RandomPropGroup].PropObject.BP.ReclaimEnergyMax;
			TotalReclaimTime += AllPropsTypes[RandomPropGroup].PropObject.BP.ReclaimTime;

			return NewProp.Obj;
		}

		PropGameObject SearchClosestProp(Vector3 Pos, float tolerance) //, out int ClosestP
		{
			int GroupsCount = AllPropsTypes.Count;
			int g = 0;
			//int p = 0;
			float dist = 0;

			//int ClosestG = -1;
			//ClosestP = -1;
			float ClosestDist = 1000000f;
			PropGameObject ToReturn = null;

			for (g = 0; g < AllPropsTypes.Count; g++)
			{
				foreach (Prop PropInstance in AllPropsTypes[g].PropsInstances)
				{
					dist = Vector3.Distance(Pos, PropInstance.Obj.Tr.localPosition);
					if (dist < ClosestDist && dist < tolerance)
					{
						//ClosestG = g;
						ToReturn = PropInstance.Obj;
						//ClosestP = p;
						ClosestDist = dist;
					}
				}
			}
			return ToReturn;
		}

		public GameObject[] AllPropGameObjects(out int[] PropTypes)
		{
			int TotalCount = 0;
			int GroupsCount = AllPropsTypes.Count;
			int g = 0;
			for (g = 0; g < AllPropsTypes.Count; g++)
			{
				TotalCount += AllPropsTypes[g].PropsInstances.Count;
			}

			GameObject[] AllGameobjects = new GameObject[TotalCount];
			PropTypes = new int[TotalCount];
			int i = 0;
			for (g = 0; g < AllPropsTypes.Count; g++)
			{
				foreach (Prop PropInstance in AllPropsTypes[g].PropsInstances)
				{
					AllGameobjects[i] = PropInstance.Obj.gameObject;
					i++;
				}
			}

			return AllGameobjects;
		}

		#endregion

		#region Import/Export
		const string ExportPathKey = "PropsSetExport";
		static string DefaultPath
		{
			get
			{
				return EnvPaths.GetLastPath(ExportPathKey, EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName);
			}
		}

		[System.Serializable]
		public class PaintButtonsSet{

			public PaintProp[] PaintProps;

			[System.Serializable]
			public class PaintProp
			{
				public string Blueprint;
				public float ScaleMin;
				public float ScaleMax;
				public int RotationMin;
				public int RotationMax;
				public int Chance;
			}
		}

		public void ImportPropsSet()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Props paint set", "proppaintset")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import props paint set", DefaultPath, extensions, false);


			if (paths.Length <= 0 || string.IsNullOrEmpty(paths[0]))
				return;

			string data = File.ReadAllText(paths[0]);

			PaintButtonsSet PaintSet = JsonUtility.FromJson<PaintButtonsSet>(data);



			while(PaintButtons.Count > 0)
			{
				RemoveProp(0);
			}



			//bool[] Exist = new bool[PaintPropObjects.Count];

			for(int i = 0; i < PaintSet.PaintProps.Length; i++)
			{
				//bool Found = false;
				int o = 0;
				/*
				for(o = 0; i < PaintPropObjects.Count; o++)
				{
					if(PaintPropObjects[i].BP.Path == PaintSet.PaintProps[i].Blueprint)
					{
						if (o < Exist.Length)
							Exist[o] = true;
						Found = true;
						break;
					}
				}*/

				//if (!Found)
				{
					// Load
					if (!LoadProp(GetGamedataFile.LoadProp(PaintSet.PaintProps[i].Blueprint)))
					{
						Debug.LogWarning("Can't load prop at path: " + PaintSet.PaintProps[i].Blueprint);
						continue;
					}

					o = PaintButtons.Count - 1;
				}

				PaintButtons[o].ScaleMin.SetValue(PaintSet.PaintProps[i].ScaleMin);
				PaintButtons[o].ScaleMax.SetValue(PaintSet.PaintProps[i].ScaleMax);

				PaintButtons[o].RotationMin.SetValue(PaintSet.PaintProps[i].RotationMin);
				PaintButtons[o].RotationMax.SetValue(PaintSet.PaintProps[i].RotationMax);

				PaintButtons[o].Chance.SetValue(PaintSet.PaintProps[i].Chance);
			}

			/*
			for(int i = Exist.Length - 1; i >= 0; i--)
			{
				if (!Exist[i])
				{
					RemoveProp(i);
				}
			}
			*/

			EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths[0]));
		}

		public void ExportPropsSet()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Props paint set", "proppaintset")
			};

			var path = StandaloneFileBrowser.SaveFilePanel("Export props paint set", DefaultPath, "", extensions);

			if (string.IsNullOrEmpty(path))
				return;

			PaintButtonsSet PaintSet = new PaintButtonsSet();
			PaintSet.PaintProps = new PaintButtonsSet.PaintProp[PaintButtons.Count];
			
			for(int i = 0; i < PaintSet.PaintProps.Length; i++)
			{
				if (PaintPropObjects[i].BP == null)
					Debug.Log("Prop object is empty!");

				PaintSet.PaintProps[i] = new PaintButtonsSet.PaintProp();

				PaintSet.PaintProps[i].Blueprint = PaintPropObjects[i].BP.Path;
				PaintSet.PaintProps[i].ScaleMin = PaintButtons[RandomProp].ScaleMin.value;
				PaintSet.PaintProps[i].ScaleMax = PaintButtons[RandomProp].ScaleMin.value;
				PaintSet.PaintProps[i].RotationMin = PaintButtons[RandomProp].RotationMin.intValue;
				PaintSet.PaintProps[i].RotationMax = PaintButtons[RandomProp].RotationMax.intValue;
				PaintSet.PaintProps[i].Chance = PaintButtons[RandomProp].Chance.intValue;
			}



			string data = JsonUtility.ToJson(PaintSet, true);

			File.WriteAllText(path, data);
			EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(path));
		}
		#endregion

		public void RemoveAllProps()
		{
			UndoRegistered = false;
			RegisterUndo();



			int GroupsCount = AllPropsTypes.Count;
			int g = 0;
			
			for (g = 0; g < AllPropsTypes.Count; g++)
			{
				foreach (Prop PropInstance in AllPropsTypes[g].PropsInstances)
				{
					Destroy(PropInstance.Obj.gameObject);
				}
				AllPropsTypes[g].PropsInstances.Clear();
			}


			TotalMassCount = 0;
			TotalEnergyCount = 0;
			TotalReclaimTime = 0;

			UpdatePropStats();
			Painting = false;

			UndoRegistered = false;
			UpdateBrushPosition(true);
		}

	}
}