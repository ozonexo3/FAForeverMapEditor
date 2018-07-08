using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Ozone.UI;
using System.IO;
using B83.Image.BMP;
using System.Text;
using System.Runtime.InteropServices;
using SFB;
using FAF.MapEditor;

namespace EditMap
{
	public partial class StratumInfo : MonoBehaviour
	{

		public StratumSettingsUi StratumSettings;
		public Editing Edit;
		//public ScmapEditor Map;

		public int Selected = 0;
		public GameObject[] Stratum_Selections;
		public bool[] StratumHide = new bool[10];

		public RawImage Stratum_Albedo;
		public RawImage Stratum_Normal;
		public Text Stratum_Albedo_Name;
		public Text Stratum_Normal_Name;

		public UiTextField Stratum_Albedo_Input;
		public UiTextField Stratum_Normal_Input;

		public GameObject Page_Stratum;
		public GameObject Page_StratumSelected;
		public GameObject Page_Paint;
		public GameObject Page_PaintSelected;
		public GameObject Page_Settings;
		public GameObject Page_SettingsSelected;

		// Brush
		[Header("Brush")]
		//public Slider BrushSizeSlider;
		public UiTextField BrushSize;
		//public Slider BrushStrengthSlider;
		public UiTextField BrushStrength;
		//public Slider BrushRotationSlider;
		public UiTextField BrushRotation;

		public UiTextField TargetValue;

		public UiTextField BrushMini;
		public UiTextField BrushMax;

		public UiTextField Scatter;

		public Toggle LinearBrush;
		public Toggle TTerrainXP;

		public LayerMask TerrainMask;
		public List<Toggle> BrushToggles;
		public ToggleGroup ToogleGroup;

		public GameObject BrushListObject;
		public Transform BrushListPivot;
		public Material TerrainMaterial;

		[Header("State")]
		public bool Invert;
		public bool Smooth;

		#region Classes
		[System.Serializable]
		public class StratumSettingsUi
		{
			[Header("Textures")]
			public RawImage Stratum9_Albedo;
			public RawImage Stratum9_Normal;
			public RawImage Stratum8_Albedo;
			public RawImage Stratum8_Normal;
			public RawImage Stratum7_Albedo;
			public RawImage Stratum7_Normal;
			public RawImage Stratum6_Albedo;
			public RawImage Stratum6_Normal;
			public RawImage Stratum5_Albedo;
			public RawImage Stratum5_Normal;
			public RawImage Stratum4_Albedo;
			public RawImage Stratum4_Normal;
			public RawImage Stratum3_Albedo;
			public RawImage Stratum3_Normal;
			public RawImage Stratum2_Albedo;
			public RawImage Stratum2_Normal;
			public RawImage Stratum1_Albedo;
			public RawImage Stratum1_Normal;
			public RawImage Stratum0_Albedo;
			public RawImage Stratum0_Normal;

			[Header("Mask")]
			public RawImage Stratum9_Mask;
			public RawImage Stratum8_Mask;
			public RawImage Stratum7_Mask;
			public RawImage Stratum6_Mask;
			public RawImage Stratum5_Mask;
			public RawImage Stratum4_Mask;
			public RawImage Stratum3_Mask;
			public RawImage Stratum2_Mask;
			public RawImage Stratum1_Mask;
			public RawImage Stratum0_Mask;

			[Header("Mask")]
			public Text Stratum9_Visible;
			public Text Stratum8_Visible;
			public Text Stratum7_Visible;
			public Text Stratum6_Visible;
			public Text Stratum5_Visible;
			public Text Stratum4_Visible;
			public Text Stratum3_Visible;
			public Text Stratum2_Visible;
			public Text Stratum1_Visible;
		}
		#endregion

		void OnEnable()
		{
			BrushGenerator.Current.LoadBrushes();
			ReloadStratums();

			if (Page_Stratum.activeSelf)
			{
				ChangePageToStratum();
			}
			else if (Page_Paint.activeSelf)
			{
				ChangePageToPaint();
			}
			else
			{
				ChangePageToSettings();
			}
		}

		void OnDisable()
		{
			TerrainMaterial.SetFloat("_BrushSize", 0);
		}

		void Start()
		{
			ChangePageToStratum();
			SelectStratum(0);
		}


		bool TerainChanged = false;
		Color[] beginColors;

		Vector3 BeginMousePos;
		float StrengthBeginValue;
		bool ChangingStrength;
		float SizeBeginValue;
		bool ChangingSize;
		void Update()
		{
			if (StratumChangeCheck)
				if (Input.GetMouseButtonUp(0))
					StratumChangeCheck = false;

			if (Page_Paint.activeSelf)
			{
				Invert = Input.GetKey(KeyCode.LeftAlt);
				Smooth = Input.GetKey(KeyCode.LeftShift);



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
						}
						if (ChangingStrength)
						{
							//BrushStrengthSlider.value = Mathf.Clamp(StrengthBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.1f), 0, 100);
							BrushStrength.SetValue(Mathf.Clamp(StrengthBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.1f), 0, 100));
							UpdateStratumMenu(true);
							//UpdateBrushPosition(true);

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
						}
						if (ChangingSize)
						{
							BrushSize.SetValue(Mathf.Clamp(SizeBeginValue - (int)((BeginMousePos.x - Input.mousePosition.x) * 0.4f), 1, 256));
							UpdateStratumMenu(true);
							UpdateBrushPosition(true);

						}
					}
					else
					{
						if (Edit.MauseOnGameplay && Input.GetMouseButtonDown(0))
						{
							if (CameraControler.Current.DragStartedGameplay && UpdateBrushPosition(true))
							{
								SymmetryPaint();
							}
						}
						else if (Input.GetMouseButton(0))
						{
							if (CameraControler.Current.DragStartedGameplay)
							{
								if (UpdateBrushPosition(false))
								{
									SymmetryPaint();
								}
							}
						}
						else
						{
							UpdateBrushPosition(true);
						}
					}
				}

				if (Input.GetMouseButton(0))
				{
				}
				else if (KeyboardManager.SwitchTypeNext())
				{
					LinearBrush.isOn = !LinearBrush.isOn;
				}
				else if (KeyboardManager.SwitchType1())
				{
					LinearBrush.isOn = false;
				}
				else if (KeyboardManager.SwitchType2())
				{
					LinearBrush.isOn = true;
				}

				if (KeyboardManager.IncreaseTarget())
				{
					if (TargetValue.value < 1)
						TargetValue.SetValue(TargetValue.value + 0.05f);
				}
				else if (KeyboardManager.DecreaseTarget())
				{
					if (TargetValue.value > 0)
						TargetValue.SetValue(TargetValue.value - 0.05f);
				}

				if (Input.GetMouseButtonUp(0))
				{
					if (Painting)
					{
						Painting = false;
					}

					if (TerainChanged)
					{
						if (Selected > 0 && Selected < 5)
							Undo.RegisterUndo(new UndoHistory.HistoryStratumPaint(), new UndoHistory.HistoryStratumPaint.StratumPaintHistoryParameter(0, beginColors));
						else if (Selected > 4 && Selected < 9)
							Undo.RegisterUndo(new UndoHistory.HistoryStratumPaint(), new UndoHistory.HistoryStratumPaint.StratumPaintHistoryParameter(1, beginColors));
						TerainChanged = false;
					}
				}

				BrushGenerator.RegeneratePaintBrushIfNeeded();
			}
		}

		#region Stratums
		public void VisibleStratums()
		{
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat0", StratumHide[1]?1:0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat1", StratumHide[2] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat2", StratumHide[3] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat3", StratumHide[4] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat4", StratumHide[5] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat5", StratumHide[6] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat6", StratumHide[7] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat7", StratumHide[8] ? 1 : 0);
			ScmapEditor.Current.TerrainMaterial.SetInt("_HideSplat8", StratumHide[9] ? 1 : 0);


			const string TextVisible = "V";
			const string TextHiden = "H";

			StratumSettings.Stratum1_Visible.text = StratumHide[1] ? TextHiden : TextVisible;
			StratumSettings.Stratum2_Visible.text = StratumHide[2] ? TextHiden : TextVisible;
			StratumSettings.Stratum3_Visible.text = StratumHide[3] ? TextHiden : TextVisible;
			StratumSettings.Stratum4_Visible.text = StratumHide[4] ? TextHiden : TextVisible;
			StratumSettings.Stratum5_Visible.text = StratumHide[5] ? TextHiden : TextVisible;
			StratumSettings.Stratum6_Visible.text = StratumHide[6] ? TextHiden : TextVisible;
			StratumSettings.Stratum7_Visible.text = StratumHide[7] ? TextHiden : TextVisible;
			StratumSettings.Stratum8_Visible.text = StratumHide[8] ? TextHiden : TextVisible;
			StratumSettings.Stratum9_Visible.text = StratumHide[9] ? TextHiden : TextVisible;
		}

		public void ReloadStratums()
		{
			StratumSettings.Stratum0_Albedo.texture = ScmapEditor.Current.Textures[0].Albedo;
			StratumSettings.Stratum0_Normal.texture = ScmapEditor.Current.Textures[0].Normal;

			StratumSettings.Stratum1_Albedo.texture = ScmapEditor.Current.Textures[1].Albedo;
			StratumSettings.Stratum1_Normal.texture = ScmapEditor.Current.Textures[1].Normal;

			StratumSettings.Stratum2_Albedo.texture = ScmapEditor.Current.Textures[2].Albedo;
			StratumSettings.Stratum2_Normal.texture = ScmapEditor.Current.Textures[2].Normal;

			StratumSettings.Stratum3_Albedo.texture = ScmapEditor.Current.Textures[3].Albedo;
			StratumSettings.Stratum3_Normal.texture = ScmapEditor.Current.Textures[3].Normal;

			StratumSettings.Stratum4_Albedo.texture = ScmapEditor.Current.Textures[4].Albedo;
			StratumSettings.Stratum4_Normal.texture = ScmapEditor.Current.Textures[4].Normal;

			StratumSettings.Stratum5_Albedo.texture = ScmapEditor.Current.Textures[5].Albedo;
			StratumSettings.Stratum5_Normal.texture = ScmapEditor.Current.Textures[5].Normal;

			StratumSettings.Stratum6_Albedo.texture = ScmapEditor.Current.Textures[6].Albedo;
			StratumSettings.Stratum6_Normal.texture = ScmapEditor.Current.Textures[6].Normal;

			StratumSettings.Stratum7_Albedo.texture = ScmapEditor.Current.Textures[7].Albedo;
			StratumSettings.Stratum7_Normal.texture = ScmapEditor.Current.Textures[7].Normal;

			StratumSettings.Stratum8_Albedo.texture = ScmapEditor.Current.Textures[8].Albedo;
			StratumSettings.Stratum8_Normal.texture = ScmapEditor.Current.Textures[8].Normal;

			StratumSettings.Stratum9_Albedo.texture = ScmapEditor.Current.Textures[9].Albedo;
			StratumSettings.Stratum9_Normal.texture = ScmapEditor.Current.Textures[9].Normal;


			StratumSettings.Stratum1_Mask.texture = ScmapEditor.Current.map.TexturemapTex;
			StratumSettings.Stratum2_Mask.texture = ScmapEditor.Current.map.TexturemapTex;
			StratumSettings.Stratum3_Mask.texture = ScmapEditor.Current.map.TexturemapTex;
			StratumSettings.Stratum4_Mask.texture = ScmapEditor.Current.map.TexturemapTex;

			StratumSettings.Stratum5_Mask.texture = ScmapEditor.Current.map.TexturemapTex2;
			StratumSettings.Stratum6_Mask.texture = ScmapEditor.Current.map.TexturemapTex2;
			StratumSettings.Stratum7_Mask.texture = ScmapEditor.Current.map.TexturemapTex2;
			StratumSettings.Stratum8_Mask.texture = ScmapEditor.Current.map.TexturemapTex2;
		}

		bool LoadingStratum = false;
		public void SelectStratum(int newid)
		{
			LoadingStratum = true;
			Selected = newid;

			foreach (GameObject obj in Stratum_Selections) obj.SetActive(false);

			Stratum_Selections[Selected].SetActive(true);

			Stratum_Albedo.texture = ScmapEditor.Current.Textures[Selected].Albedo;
			Stratum_Normal.texture = ScmapEditor.Current.Textures[Selected].Normal;

			Stratum_Albedo_Name.text = ScmapEditor.Current.Textures[Selected].AlbedoPath;
			Stratum_Normal_Name.text = ScmapEditor.Current.Textures[Selected].NormalPath;


			Stratum_Albedo_Input.SetValue(ScmapEditor.Current.Textures[Selected].AlbedoScale);

			Stratum_Normal_Input.SetValue(ScmapEditor.Current.Textures[Selected].NormalScale);
			LoadingStratum = false;
		}

		public void ResetVisibility()
		{
			for (int i = 0; i < StratumHide.Length; i++)
				StratumHide[i] = false;

			VisibleStratums();
		}

		public void ToggleLayerVisibility(int id)
		{
			StratumHide[id] = !StratumHide[id];
			// TODO Update Terrain Shader To Hide Stratum

			VisibleStratums();
		}

		#endregion

		#region Update Menu
		public void ChangePageToStratum()
		{
			Page_Stratum.SetActive(true);
			Page_StratumSelected.SetActive(true);
			Page_Paint.SetActive(false);
			Page_PaintSelected.SetActive(false);
			Page_Settings.SetActive(false);
			Page_SettingsSelected.SetActive(false);
			TerrainMaterial.SetFloat("_BrushSize", 0);
		}

		public void ChangePageToPaint()
		{
			Page_Stratum.SetActive(false);
			Page_StratumSelected.SetActive(false);
			Page_Paint.SetActive(true);
			Page_PaintSelected.SetActive(true);
			Page_Settings.SetActive(false);
			Page_SettingsSelected.SetActive(false);

			BrushGenerator.Current.LoadBrushes();

			if (!BrusheshLoaded) LoadBrushesh();
			UpdateStratumMenu();

			TerrainMaterial.SetInt("_Brush", 1);
			BrushGenerator.SetFallof(SelectedFalloff, LastRotation);
			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.Brushes[SelectedFalloff]);

			
		}

		public void ChangePageToSettings()
		{
			Page_Stratum.SetActive(false);
			Page_StratumSelected.SetActive(false);
			Page_Paint.SetActive(false);
			Page_PaintSelected.SetActive(false);
			Page_Settings.SetActive(true);
			Page_SettingsSelected.SetActive(true);
			TerrainMaterial.SetFloat("_BrushSize", 0);
		}


		public float Min = 0;
		public float Max = 512;
		int LastRotation = 0;
		bool StratumChangeCheck = false;

		bool StratumMenuUndoRegistered = false;

		public void UpdateStratumMenu(bool Slider = false)
		{
			if (!gameObject.activeSelf)
				return;

			if (Page_Stratum.activeSelf)
			{
				if (Slider)
				{
					if (!StratumChangeCheck)
					{
						StratumChangeCheck = true;
						if (!LoadingStratum && !StratumMenuUndoRegistered)
						{
							Undo.RegisterUndo(new UndoHistory.HistoryStratumChange(), new UndoHistory.HistoryStratumChange.StratumChangeHistoryParameter(Selected));
							StratumMenuUndoRegistered = true;
						}
					}
					if (!LoadingStratum)
					{
					}
				}
				else
				{
					if (!LoadingStratum && !StratumMenuUndoRegistered)
					{
						Undo.RegisterUndo(new UndoHistory.HistoryStratumChange(), new UndoHistory.HistoryStratumChange.StratumChangeHistoryParameter(Selected));
					}
					StratumMenuUndoRegistered = false;
				}
				if (!LoadingStratum)
				{
					ScmapEditor.Current.Textures[Selected].AlbedoScale = Stratum_Albedo_Input.value;
					ScmapEditor.Current.Textures[Selected].NormalScale = Stratum_Normal_Input.value;
				}

				ScmapEditor.Current.UpdateScales(Selected);

			}
			else if (Page_Paint.activeSelf)
			{
				if (Slider)
				{

				}
				else
				{

				}


				Min = BrushMini.intValue;
				Max = BrushMax.intValue;

				Min = Mathf.Clamp(Min, 0, Max);
				Max = Mathf.Clamp(Max, Min, 90);

				BrushMini.SetValue(Min);
				BrushMax.SetValue(Max);

				if (TargetValue.value < 0)
					TargetValue.SetValue(0);
				else if (TargetValue.value > 1)
					TargetValue.SetValue(1);

				//BrushMini.text = Min.ToString("0");
				//BrushMax.text = Max.ToString("0");

				if (LastRotation != BrushRotation.intValue)
				{
					LastRotation = BrushRotation.intValue;
					if (LastRotation == 0)
					{
						BrushGenerator.Current.RotatedBrush = BrushGenerator.Current.Brushes[SelectedFalloff];
					}
					else
					{
						BrushGenerator.Current.RotatedBrush = BrushGenerator.rotateTexture(BrushGenerator.Current.Brushes[SelectedFalloff], LastRotation);
					}

					TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.RotatedBrush);
					BrushGenerator.RegeneratePaintBrushIfNeeded(true);
				}
				//TerrainMaterial.SetFloat("_BrushSize", BrushSize.value);
			}
		}
		#endregion

		#region Load all brushesh
		bool BrusheshLoaded = false;
		string StructurePath;
		public void LoadBrushesh()
		{
			Clean();


			StructurePath = Application.dataPath + "/Structure/"; ;
#if UNITY_EDITOR
			StructurePath = StructurePath.Replace("Assets", "");
#endif
			StructurePath += "brush";

			if (!Directory.Exists(StructurePath))
			{
				Debug.LogError("Cant find brush folder");
				return;
			}

			BrushToggles = new List<Toggle>();

			for (int i = 0; i < BrushGenerator.Current.Brushes.Count; i++)
			{
				GameObject NewBrush = Instantiate(BrushListObject) as GameObject;
				NewBrush.transform.SetParent(BrushListPivot, false);
				NewBrush.transform.localScale = Vector3.one;
				string ThisName = BrushGenerator.Current.BrushesNames[i];
				BrushToggles.Add(NewBrush.GetComponent<BrushListId>().SetBrushList(ThisName, BrushGenerator.Current.Brushes[i], i));
				NewBrush.GetComponent<BrushListId>().Controler2 = this;
			}

			foreach (Toggle tog in BrushToggles)
			{
				tog.isOn = false;
				tog.group = ToogleGroup;
			}
			BrushToggles[0].isOn = true;
			SelectedFalloff = 0;

			BrusheshLoaded = true;
		}

		void Clean()
		{
			BrusheshLoaded = false;
			foreach (Transform child in BrushListPivot) Destroy(child.gameObject);
		}

		#endregion


		#region Brush Update
		int SelectedBrush = 0;
		public void ChangeBrush(int id)
		{
			SelectedBrush = id;
		}

		int SelectedFalloff = 0;
		public void ChangeFalloff(int id)
		{
			SelectedFalloff = id;

			LastRotation = BrushRotation.intValue;

			BrushGenerator.SetFallof(SelectedFalloff, LastRotation);

			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.RotatedBrush);
			//BrushGenerator.RegeneratePaintBrushIfNeeded(true);
		}


		Vector3 BrushPos;
		Vector3 MouseBeginClick;
		bool UpdateBrushPosition(bool Forced = false)
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

			MouseBeginClick = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 2000, TerrainMask))
			{
				BrushPos = hit.point;
				BrushPos.y = ScmapEditor.Current.Teren.SampleHeight(BrushPos);

				Vector3 tempCoord = ScmapEditor.Current.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
				Vector3 coord = Vector3.zero;
				float SizeX = (int)((BrushSizeValue / SizeXprop) * 100) * 0.01f;
				float SizeZ = (int)((BrushSizeValue / SizeZprop) * 100) * 0.01f;
				coord.x = (tempCoord.x - SizeX * MapLuaParser.GetMapSizeX() * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.x;
				coord.z = (tempCoord.z - SizeZ * MapLuaParser.GetMapSizeY() * 0.0001f) / ScmapEditor.Current.Teren.terrainData.size.z;

				TerrainMaterial.SetFloat("_BrushSize", BrushSizeValue / ((SizeXprop + SizeZprop) / 2f));
				TerrainMaterial.SetFloat("_BrushUvX", coord.x);
				TerrainMaterial.SetFloat("_BrushUvY", coord.z);

				return true;
			}
			return false;
		}
		#endregion
		bool _Painting = false;
		bool Painting
		{
			set
			{
				_Painting = value;
				TerrainMaterial.SetInt("_BrushPainting", _Painting ? (1) : (0));

			}
			get
			{
				return _Painting;
			}
		}
		float ScatterValue = 0;
		static float TargetPaintValue = 1;
		void SymmetryPaint()
		{
			Painting = true;
			float SizeProportion = (float)ScmapEditor.Current.map.TexturemapTex.width / (float)ScmapEditor.Current.map.Width;
			size = (int)(BrushSize.value * SizeProportion);
			ScatterValue = Scatter.value;

			if (LinearBrush.isOn)
				TargetPaintValue = TargetValue.value;
			else
				TargetPaintValue = Mathf.Clamp01((TargetValue.value + 1f) / 2f);

			BrushGenerator.Current.GenerateSymmetry(BrushPos, 0, ScatterValue, size * 0.03f);

			if (Selected == 1 || Selected == 5)
				PaintChannel = 0;
			else if (Selected == 2 || Selected == 6)
				PaintChannel = 1;
			else if (Selected == 3 || Selected == 7)
				PaintChannel = 2;
			else if (Selected == 4 || Selected == 8)
				PaintChannel = 3;

			for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
			{
				Paint(BrushGenerator.Current.PaintPositions[i], i);
			}

			if (Selected > 0 && Selected < 5)
			{
				ScmapEditor.Current.map.TexturemapTex.Apply();
			}
			else if (Selected > 4 && Selected < 9)
			{
				ScmapEditor.Current.map.TexturemapTex2.Apply();
			}
		}


		static Color[] StratumData;
		static int PaintChannel = 0;
		int size = 0;
		void Paint(Vector3 AtPosition, int id = 0)
		{

			int hmWidth = ScmapEditor.Current.map.TexturemapTex.width;
			int hmHeight = ScmapEditor.Current.map.TexturemapTex.height;

			Vector3 tempCoord = ScmapEditor.Current.Teren.gameObject.transform.InverseTransformPoint(AtPosition);
			Vector3 coord = Vector3.zero;
			coord.x = tempCoord.x / ScmapEditor.Current.Teren.terrainData.size.x;
			//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
			coord.z = 1 - tempCoord.z / ScmapEditor.Current.Teren.terrainData.size.z;

			if (coord.x > 1) return;
			if (coord.x < 0) return;
			if (coord.z > 1) return;
			if (coord.z < 0) return;

			// get the position of the terrain heightmap where this game object is
			int posXInTerrain = (int)(coord.x * hmWidth);
			int posYInTerrain = (int)(coord.z * hmHeight);
			// we set an offset so that all the raising terrain is under this game object
			int offset = size / 2;
			// get the heights of the terrain under this game object

			// Horizontal Brush Offsets
			int OffsetLeft = 0;
			if (posXInTerrain - offset < 0) OffsetLeft = Mathf.Abs(posXInTerrain - offset);
			int OffsetRight = 0;
			if (posXInTerrain - offset + size > hmWidth) OffsetRight = posXInTerrain - offset + size - hmWidth;

			// Vertical Brush Offsets
			int OffsetDown = 0;
			if (posYInTerrain - offset < 0) OffsetDown = Mathf.Abs(posYInTerrain - offset);
			int OffsetTop = 0;
			if (posYInTerrain - offset + size > hmHeight) OffsetTop = posYInTerrain - offset + size - hmHeight;

			//float CenterHeight = 0;
			float LocalBrushStrength = Mathf.Pow(BrushStrength.value * 0.01f, 1.5f) * 0.6f;
			float inverted = (Invert ? (-1) : 1);
			LocalBrushStrength *= inverted;
			float SampleBrush = 0;
			//Color BrushValue;
			int x = 0;
			int y = 0;
			int i = 0;
			int j = 0;

			int SizeDown = (size - OffsetDown) - OffsetTop;
			int SizeLeft = (size - OffsetLeft) - OffsetRight;

			if (Selected > 0 && Selected < 5)
			{
				StratumData = ScmapEditor.Current.map.TexturemapTex.GetPixels(posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, SizeLeft, SizeDown);
			}
			else if (Selected > 4 && Selected < 9)
			{
				StratumData = ScmapEditor.Current.map.TexturemapTex2.GetPixels(posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, SizeLeft, SizeDown);
			}
			else
				return;


			int PaintType = PaintChannel;
			if (LinearBrush.isOn)
				PaintType += 10;


			for (i = 0; i < SizeDown; i++)
			{
				for (j = 0; j < SizeLeft; j++)
				{

					if (Min > 0 || Max < 90)
					{
						float angle = Vector3.Angle(Vector3.up, ScmapEditor.Current.Teren.terrainData.GetInterpolatedNormal((posXInTerrain - offset + OffsetLeft + i) / (float)hmWidth, 1 - (posYInTerrain - offset + OffsetDown + j) / (float)hmHeight));
						if ((angle < Min && Min > 0) || (angle > Max && Max < 90))
							continue;
					}

					// Brush strength
					x = BrushGenerator.Current.PaintImageWidths[id] - 
						(int)(((i + OffsetDown) / (float)size) * BrushGenerator.Current.PaintImageWidths[id]);
					y = (int)(((j + OffsetLeft) / (float)size) * BrushGenerator.Current.PaintImageHeights[id]);

					if (x < 0 || y < 0 || x >= BrushGenerator.Current.PaintImageWidths[id] || y >= BrushGenerator.Current.PaintImageHeights[id])
						continue;
					SampleBrush = Mathf.Clamp01(BrushGenerator.Current.Values[id][y + BrushGenerator.Current.PaintImageHeights[id] * x] - 0.0255f);
					SampleBrush = Mathf.GammaToLinearSpace(SampleBrush); // , 0.454545f

					if (SampleBrush >= 0.003f)
					{
						if (Smooth || SelectedBrush == 2)
						{
							//float PixelPower = Mathf.Abs( heights[i,j] - CenterHeight);
							//heights[i,j] = Mathf.Lerp(heights[i,j], CenterHeight, BrushStrengthSlider.value * 0.4f * Mathf.Pow(SambleBrush, 2) * PixelPower);
						}
						else if (SelectedBrush == 3)
						{
							//float PixelPower = heights[i,j] - CenterHeight;
							//heights[i,j] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * BrushStrengthSlider.value * 0.01f * Mathf.Pow(SambleBrush, 2);
						}
						else
						{
							int XY = j + i * SizeLeft;

							switch (PaintType)
							{
								case 0:
										StratumData[XY].r = ToTarget(StratumData[XY].r, SampleBrush * LocalBrushStrength);
									break;
								case 1:
										StratumData[XY].g = ToTarget(StratumData[XY].g, SampleBrush * LocalBrushStrength);
									break;
								case 2:
										StratumData[XY].b = ToTarget(StratumData[XY].b, SampleBrush * LocalBrushStrength);
									break;
								case 3:
										StratumData[XY].a = ToTarget(StratumData[XY].a, SampleBrush * LocalBrushStrength);
									break;
								case 10:
										StratumData[XY].r = ConvertToLinear(StratumData[XY].r, SampleBrush * LocalBrushStrength);
									break;
								case 11:
										StratumData[XY].g = ConvertToLinear(StratumData[XY].g, SampleBrush * LocalBrushStrength);
									break;
								case 12:
										StratumData[XY].b = ConvertToLinear(StratumData[XY].b, SampleBrush * LocalBrushStrength);
									break;
								case 13:
										StratumData[XY].a = ConvertToLinear(StratumData[XY].a, SampleBrush * LocalBrushStrength);
									break;
							}
						}
					}
				}
			}
			// set the new height
			if (!TerainChanged)
			{
				if (Selected > 0 && Selected < 5)
				{
					beginColors = ScmapEditor.Current.map.TexturemapTex.GetPixels();
				}
				else if (Selected > 4 && Selected < 9)
				{
					beginColors = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
				}

				TerainChanged = true;
			}
			if (Selected > 0 && Selected < 5)
			{
				ScmapEditor.Current.map.TexturemapTex.SetPixels(posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop, StratumData);
			}
			else
			{
				ScmapEditor.Current.map.TexturemapTex2.SetPixels(posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop, StratumData);
			}
			//Map.map.TexturemapTex.SetPixels(StratumData);
		}

		static float ToTarget(float source, float addValue)
		{
			if(addValue < 0)
			{
				return Mathf.Clamp(source + addValue, 0, 1);
			}
			else if(source < TargetPaintValue)
			{
				return Mathf.Clamp(source + addValue, 0, TargetPaintValue);
			}
			else
			{
				return Mathf.Clamp(source - addValue, TargetPaintValue, 1);
			}
		}

		static float ConvertToLinear(float value, float addValue)
		{
			value = value * 2f - 1f;
			//value = Mathf.Clamp(value + addValue, -0.05f, 1.05f);
			value = ToTarget(value, addValue);
			value += 1;
			value /= 2f;
			value = Mathf.Clamp01(value);

			return value;
			//return Mathf.Clamp01((Mathf.Clamp01(value * 0.5f + 0.5f) + addValue) * 2f - 1f);
			//return Mathf.Pow(Mathf.Pow(value, 0.454545f) + addValue, 2.2f);
			//return value + Mathf.Pow(addValue, 0.454545f);
			//return value + addValue;
		}

		#region Select Texture
		public void SelectAlbedo()
		{
			if(ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Texture)
				return;

			if (!ResourceBrowser.Current.gameObject.activeSelf)
				return;
			if (ResourceBrowser.SelectedCategory == 0 || ResourceBrowser.SelectedCategory == 1)
			{
				if (ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId].ToLower().Contains("normal"))
				{
					GenericInfoPopup.ShowInfo("Can't assign normal map as albedo!");
					return;
				}

				Undo.RegisterUndo(new UndoHistory.HistoryStratumChange(), new UndoHistory.HistoryStratumChange.StratumChangeHistoryParameter(Selected));
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				ScmapEditor.Current.Textures[Selected].Albedo = ResourceBrowser.Current.LoadedTextures[ResourceBrowser.DragedObject.InstanceId];
				ScmapEditor.Current.Textures[Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];

				//Map.map.Layers [Selected].PathTexture = Map.Textures [Selected].AlbedoPath;

				ResourceBrowser.ClearDrag();
				ScmapEditor.Current.SetTextures(Selected);
				ReloadStratums();
				SelectStratum(Selected);
			}
		}

		public void SelectNormal()
		{
			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Texture)
				return;

			if (!ResourceBrowser.Current.gameObject.activeSelf)
				return;
			if (ResourceBrowser.SelectedCategory == 0 || ResourceBrowser.SelectedCategory == 1)
			{
				if (!ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId].ToLower().Contains("normal"))
				{
					GenericInfoPopup.ShowInfo("Can't assign albedo as normal map!");
					return;
				}

				Undo.RegisterUndo(new UndoHistory.HistoryStratumChange(), new UndoHistory.HistoryStratumChange.StratumChangeHistoryParameter(Selected));
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				//Map.Textures [Selected].Normal = ResourceBrowser.Current.LoadedTextures [ResourceBrowser.DragedObject.InstanceId];
				ScmapEditor.Current.Textures[Selected].NormalPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];

				GetGamedataFile.LoadTextureFromGamedata(GetGamedataFile.EnvScd, ScmapEditor.Current.Textures[Selected].NormalPath, Selected, true);

				//Map.map.Layers [Selected].PathNormalmap = Map.Textures [Selected].NormalPath;

				ResourceBrowser.ClearDrag();
				ScmapEditor.Current.SetTextures(Selected);
				ReloadStratums();
				SelectStratum(Selected);
			}
		}

		public void ClickAlbedo()
		{
			ResourceBrowser.Current.LoadStratumTexture(ScmapEditor.Current.Textures[Selected].AlbedoPath);
		}

		public void ClickNormal()
		{
			ResourceBrowser.Current.LoadStratumTexture(ScmapEditor.Current.Textures[Selected].NormalPath);
		}
		#endregion


#region ColorTransfer
		Color[] GetPixels(int layer)
		{
			if (layer > 4)
			{
				return ScmapEditor.Current.map.TexturemapTex2.GetPixels();
			}
			else
			{
				return ScmapEditor.Current.map.TexturemapTex.GetPixels();
			}
		}

		void SetPixels(int layer, Color[] Colors)
		{
			if (layer > 4)
			{
				ScmapEditor.Current.map.TexturemapTex2.SetPixels(Colors);
				ScmapEditor.Current.map.TexturemapTex2.Apply(false);
			}
			else
			{
				ScmapEditor.Current.map.TexturemapTex.SetPixels(Colors);
				ScmapEditor.Current.map.TexturemapTex.Apply(false);
			}
		}

		float GetChannelByLayer(int layer, Color color)
		{
			if (layer == 1 || layer == 5)
				return color.r;
			else if (layer == 2 || layer == 6)
				return color.g;
			else if (layer == 3 || layer == 7)
				return color.b;
			else if (layer == 4 || layer == 8)
				return color.a;
			else
				return 0;
		}

		void SetChannelByLayer(int layer, ref Color color, float channel)
		{
			if (layer == 1 || layer == 5)
				color.r = channel;
			else if (layer == 2 || layer == 6)
				color.g = channel;
			else if (layer == 3 || layer == 7)
				color.b = channel;
			else if (layer == 4 || layer == 8)
				color.a = channel;
		}

#endregion


		#region Import/Export


		public void ClearStratumMask()
		{
			if (Selected == 0 || Selected == 9)
				return;


			Color[] StratumData;
			if (Selected > 4)
			{
				StratumData = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
				beginColors = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
			}
			else
			{
				StratumData = ScmapEditor.Current.map.TexturemapTex.GetPixels();
				beginColors = ScmapEditor.Current.map.TexturemapTex.GetPixels();
			}

			if (Selected > 0 && Selected < 5)
				Undo.RegisterUndo(new UndoHistory.HistoryStratumPaint(), new UndoHistory.HistoryStratumPaint.StratumPaintHistoryParameter(0, beginColors));
			else if (Selected > 4 && Selected < 9)
				Undo.RegisterUndo(new UndoHistory.HistoryStratumPaint(), new UndoHistory.HistoryStratumPaint.StratumPaintHistoryParameter(1, beginColors));


			for (int i = 0; i < StratumData.Length; i++)
			{
				if (Selected == 1 || Selected == 5)
					StratumData[i].r = 0;
				else if (Selected == 2 || Selected == 6)
					StratumData[i].g = 0;
				else if (Selected == 3 || Selected == 7)
					StratumData[i].b = 0;
				else if (Selected == 4 || Selected == 8)
					StratumData[i].a = 0;
			}


			if (Selected > 4)
			{
				ScmapEditor.Current.map.TexturemapTex2.SetPixels(StratumData);
				ScmapEditor.Current.map.TexturemapTex2.Apply(false);
			}
			else
			{
				ScmapEditor.Current.map.TexturemapTex.SetPixels(StratumData);
				ScmapEditor.Current.map.TexturemapTex.Apply(false);
			}

			GenericInfoPopup.ShowInfo("Cleared stratum mask for layer " + Selected);
		}

		public void ImportStratumMask()
		{
			if (Selected == 0 || Selected == 9)
				return;

			var extensions = new[]
			{
				new ExtensionFilter("Stratum mask", new string[]{"bmp", "raw" })
				//new ExtensionFilter("Stratum mask", "raw, bmp")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import stratum mask", DefaultPath, extensions, false);


			if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
			{
				if (paths[0].ToLower().EndsWith("bmp"))
				{
					BMPLoader loader = new BMPLoader();
					BMPImage img = loader.LoadBMP(paths[0]);

					Color[] StratumData;
					if (Selected > 4)
					{
						StratumData = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
						beginColors = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
					}
					else
					{
						StratumData = ScmapEditor.Current.map.TexturemapTex.GetPixels();
						beginColors = ScmapEditor.Current.map.TexturemapTex.GetPixels();
					}

					if (Selected > 0 && Selected < 5)
						Undo.RegisterUndo(new UndoHistory.HistoryStratumPaint(), new UndoHistory.HistoryStratumPaint.StratumPaintHistoryParameter(0, beginColors));
					else if (Selected > 4 && Selected < 9)
						Undo.RegisterUndo(new UndoHistory.HistoryStratumPaint(), new UndoHistory.HistoryStratumPaint.StratumPaintHistoryParameter(1, beginColors));

					Texture2D ImportedImage = img.ToTexture2D();
					if(ImportedImage.width != ScmapEditor.Current.map.TexturemapTex.width || ImportedImage.height != ScmapEditor.Current.map.TexturemapTex.height)
					{
						//ImportedImage.Resize(Map.map.TexturemapTex.width, Map.map.TexturemapTex.height);
						ImportedImage = TextureScale.Bilinear(ImportedImage, ScmapEditor.Current.map.TexturemapTex.width, ScmapEditor.Current.map.TexturemapTex.height);
						//ImportedImage.Apply(false);
					}

					ImportedImage = TextureFlip.FlipTextureVertical(ImportedImage, false);

					Color[] ImportedColors = ImportedImage.GetPixels();

					for (int i = 0; i < StratumData.Length; i++)
					{
						if (Selected == 1 || Selected == 5)
							StratumData[i].r = ImportedColors[i].r;
						else if (Selected == 2 || Selected == 6)
							StratumData[i].g = ImportedColors[i].r;
						else if (Selected == 3 || Selected == 7)
							StratumData[i].b = ImportedColors[i].r;
						else if (Selected == 4 || Selected == 8)
							StratumData[i].a = ImportedColors[i].r;
					}


					if (Selected > 4)
					{
						ScmapEditor.Current.map.TexturemapTex2.SetPixels(StratumData);
						ScmapEditor.Current.map.TexturemapTex2.Apply(false);
					}
					else
					{
						ScmapEditor.Current.map.TexturemapTex.SetPixels(StratumData);
						ScmapEditor.Current.map.TexturemapTex.Apply(false);
					}
					GenericInfoPopup.ShowInfo("Stratim mask import success!\n" + System.IO.Path.GetFileName(paths[0]));

				}
				else if(paths[0].ToLower().EndsWith("raw"))
				{
					int h = ScmapEditor.Current.map.TexturemapTex.width;
					int w = ScmapEditor.Current.map.TexturemapTex.height;
					int i = 0;

					Color[] data;

					if (Selected > 4)
						data = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
					else
						data = ScmapEditor.Current.map.TexturemapTex.GetPixels();


					//byte[,] data = new byte[h, w];
					using (var file = System.IO.File.OpenRead(paths[0]))
					using (var reader = new System.IO.BinaryReader(file))
					{
						for (int y = 0; y < h; y++)
						{
							for (int x = 0; x < w; x++)
							{
								i = x + y * w;

								//data[y, x] = reader.ReadByte();

								if (Selected == 1 || Selected == 5)
									data[i].r = reader.ReadByte() / 255f;
								else if (Selected == 2 || Selected == 6)
									data[i].g = reader.ReadByte() / 255f;
								else if (Selected == 3 || Selected == 7)
									data[i].b = reader.ReadByte() / 255f;
								else if (Selected == 4 || Selected == 8)
									data[i].a = reader.ReadByte() / 255f;

							}
						}
					}

					if (Selected > 4)
					{
						ScmapEditor.Current.map.TexturemapTex2.SetPixels(data);
						ScmapEditor.Current.map.TexturemapTex2.Apply(false);
					}
					else
					{
						ScmapEditor.Current.map.TexturemapTex.SetPixels(data);
						ScmapEditor.Current.map.TexturemapTex.Apply(false);
					}


					GenericInfoPopup.ShowInfo("Stratim mask import success!\n" + System.IO.Path.GetFileName(paths[0]));

				}
				else
				{
					// Wrong file type
					GenericInfoPopup.ShowInfo("Wrong file type!");

				}

				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths[0]));

				ScmapEditor.Current.SetTextures(Selected);

				ReloadStratums();

			}
		}

		const string ExportPathKey = "TexturesStratumMaskExport";
		static string DefaultPath
		{
			get
			{
				return EnvPaths.GetLastPath(ExportPathKey, EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName);
			}
		}

		public void ExportStratumMask()
		{
			if (Selected <= 0 || Selected > 8)
				return;


			var extensions = new[]
			{
				new ExtensionFilter("Stratum mask", "raw")
			};

			var path = StandaloneFileBrowser.SaveFilePanel("Export stratum mask", DefaultPath, "stratum_" + Selected, extensions);

			if (!string.IsNullOrEmpty(path))
			{

				int h = ScmapEditor.Current.map.TexturemapTex.width;
				int w = ScmapEditor.Current.map.TexturemapTex.height;
				int x = 0;
				int y = 0;
				int i = 0;

				//float[,] data = Map.Teren.terrainData.GetHeights(0, 0, w, h);
				Color[] data;

				if (Selected > 4)
					data = ScmapEditor.Current.map.TexturemapTex2.GetPixels();
				else
					data = ScmapEditor.Current.map.TexturemapTex.GetPixels();

				using (BinaryWriter writer = new BinaryWriter(new System.IO.FileStream(path, System.IO.FileMode.Create)))
				{
					for (y = 0; y < h; y++)
					{
						for (x = 0; x < w; x++)
						{
							i = x + y * w;

							//uint ThisPixel = (uint)(data[y, x] * 0xFFFF);
							byte ThisPixel = 0;

							if (Selected == 1 || Selected == 5)
								ThisPixel = (byte)(data[i].r * 255);
							else if (Selected == 2 || Selected == 6)
								ThisPixel = (byte)(data[i].g * 255);
							else if (Selected == 3 || Selected == 7)
								ThisPixel = (byte)(data[i].b * 255);
							else if (Selected == 4 || Selected == 8)
								ThisPixel = (byte)(data[i].a * 255);

							

							if (Selected == 1 || Selected == 5)
								writer.Write(ThisPixel);
							else if (Selected == 2 || Selected == 6)
								writer.Write(ThisPixel);
							else if (Selected == 3 || Selected == 7)
								writer.Write(ThisPixel);
							else if (Selected == 4 || Selected == 8)
								writer.Write(ThisPixel);
						}
					}
					writer.Close();
				}
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(path));
				GenericInfoPopup.ShowInfo("Stratum mask export success!\n" + System.IO.Path.GetFileName(path));
			}

		}


		public void ExportStratum()
		{

			

			var extensions = new[]
{
				new ExtensionFilter("Stratum setting file", "scmsl")
			};

			var paths = StandaloneFileBrowser.SaveFilePanel("Import stratum mask", DefaultPath, "", extensions);


			if(!string.IsNullOrEmpty(paths))
			{
				string data = UnityEngine.JsonUtility.ToJson(ScmapEditor.Current.Textures[Selected]);

				File.WriteAllText(paths, data);

				GenericInfoPopup.ShowInfo("Stratum settings export success!\n" + System.IO.Path.GetFileName(paths));
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths));
			}
		}

		public void ImportStratum()
		{

			var extensions = new[]
{
				new ExtensionFilter("Stratum setting file", "scmsl")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import stratum mask", DefaultPath, extensions, false);

			if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
			{
				string data = File.ReadAllText(paths[0]);

				ScmapEditor.TerrainTexture NewTexture = UnityEngine.JsonUtility.FromJson<ScmapEditor.TerrainTexture>(data);

				ScmapEditor.Current.Textures[Selected] = NewTexture;

				GetGamedataFile.LoadTextureFromGamedata(GetGamedataFile.EnvScd, ScmapEditor.Current.Textures[Selected].AlbedoPath, Selected, false);
				GetGamedataFile.LoadTextureFromGamedata(GetGamedataFile.EnvScd, ScmapEditor.Current.Textures[Selected].NormalPath, Selected, true);

				ScmapEditor.Current.SetTextures(Selected);

				ReloadStratums();

				GenericInfoPopup.ShowInfo("Stratum settings import success!\n" + System.IO.Path.GetFileName(paths[0]));
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths[0]));
			}
		}

		class StratumTemplate
		{
			public bool TTerrainXP = false;
			public ScmapEditor.TerrainTexture Stratum0;
			public ScmapEditor.TerrainTexture Stratum1;
			public ScmapEditor.TerrainTexture Stratum2;
			public ScmapEditor.TerrainTexture Stratum3;
			public ScmapEditor.TerrainTexture Stratum4;
			public ScmapEditor.TerrainTexture Stratum5;
			public ScmapEditor.TerrainTexture Stratum6;
			public ScmapEditor.TerrainTexture Stratum7;
			public ScmapEditor.TerrainTexture Stratum8;
			public ScmapEditor.TerrainTexture Stratum9;
		}

		public void ExportStratumTemplate()
		{
			var extensions = new[]
{
				new ExtensionFilter("Stratum template", "scmst")
			};

			var paths = StandaloneFileBrowser.SaveFilePanel("Export stratum mask", DefaultPath, "", extensions);


			if (!string.IsNullOrEmpty(paths))
			{

				StratumTemplate NewTemplate = new StratumTemplate();
				NewTemplate.TTerrainXP = TTerrainXP.isOn;
				NewTemplate.Stratum0 = ScmapEditor.Current.Textures[0];
				NewTemplate.Stratum1 = ScmapEditor.Current.Textures[1];
				NewTemplate.Stratum2 = ScmapEditor.Current.Textures[2];
				NewTemplate.Stratum3 = ScmapEditor.Current.Textures[3];
				NewTemplate.Stratum4 = ScmapEditor.Current.Textures[4];
				NewTemplate.Stratum5 = ScmapEditor.Current.Textures[5];
				NewTemplate.Stratum6 = ScmapEditor.Current.Textures[6];
				NewTemplate.Stratum7 = ScmapEditor.Current.Textures[7];
				NewTemplate.Stratum8 = ScmapEditor.Current.Textures[8];
				NewTemplate.Stratum9 = ScmapEditor.Current.Textures[9];

				string data = UnityEngine.JsonUtility.ToJson(NewTemplate);

				File.WriteAllText(paths, data);
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths));
			}
		}

		public void ImportStratumTemplate()
		{

			var extensions = new[]
{
				new ExtensionFilter("Stratum setting file", "scmst")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import stratum mask", DefaultPath, extensions, false);

			if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
			{
				string data = File.ReadAllText(paths[0]);

				StratumTemplate NewTemplate = UnityEngine.JsonUtility.FromJson<StratumTemplate>(data);

				TTerrainXP.isOn = NewTemplate.TTerrainXP;

				ScmapEditor.Current.Textures[0] = NewTemplate.Stratum0;
				ScmapEditor.Current.Textures[1] = NewTemplate.Stratum1;
				ScmapEditor.Current.Textures[2] = NewTemplate.Stratum2;
				ScmapEditor.Current.Textures[3] = NewTemplate.Stratum3;
				ScmapEditor.Current.Textures[4] = NewTemplate.Stratum4;
				ScmapEditor.Current.Textures[5] = NewTemplate.Stratum5;
				ScmapEditor.Current.Textures[6] = NewTemplate.Stratum6;
				ScmapEditor.Current.Textures[7] = NewTemplate.Stratum7;
				ScmapEditor.Current.Textures[8] = NewTemplate.Stratum8;
				ScmapEditor.Current.Textures[9] = NewTemplate.Stratum9;



				ScmapEditor.Current.LoadStratumScdTextures(false);

				ScmapEditor.Current.SetTextures();
				EnvPaths.SetLastPath(ExportPathKey, System.IO.Path.GetDirectoryName(paths[0]));

				ReloadStratums();
			}
		}
		#endregion
	}
}