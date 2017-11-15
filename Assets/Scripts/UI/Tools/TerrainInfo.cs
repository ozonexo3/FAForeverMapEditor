using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using B83.Image.BMP;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using SFB;

namespace EditMap
{
	public class TerrainInfo : MonoBehaviour
	{

		public GameObject[] Selection;
		public GameObject[] Page;

		public Editing Edit;
		public ScmapEditor Map;
		public Camera GameplayCamera;
		public Slider BrushSizeSlider;
		public InputField BrushSize;
		public Slider BrushStrengthSlider;
		public InputField BrushStrength;
		public Slider BrushRotationSlider;
		public InputField BrushRotation;

		public InputField BrushMini;
		public InputField BrushMax;

		public InputField TerrainSet;
		public InputField TerrainAdd;
		public InputField TerrainScale;
		public Toggle TerrainScale_Height;
		public InputField TerrainScale_HeightValue;

		public GameObject BrushListObject;
		public Transform BrushListPivot;
		public Material TerrainMaterial;


		public LayerMask TerrainMask;
		public List<Toggle> BrushToggles;
		public ToggleGroup ToogleGroup;

		[Header("State")]
		public bool Invert;
		public bool Smooth;


		//PaintWithBrush.BrushData TerrainBrush = new PaintWithBrush.BrushData();

		void OnEnable()
		{
			BrushGenerator.Current.LoadBrushes();

			if (!BrusheshLoaded) LoadBrushes();
			UpdateMenu();
			TerrainMaterial.SetInt("_Brush", 1);
			BrushGenerator.Current.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
			BrushGenerator.Current.Brushes[SelectedFalloff].mipMapBias = -1f;
			BrushGenerator.Current.Brushes[SelectedFalloff].filterMode = FilterMode.Bilinear;
			BrushGenerator.Current.Brushes[SelectedFalloff].anisoLevel = 2;
			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.Brushes[SelectedFalloff]);
		}

		void OnDisable()
		{
			TerrainMaterial.SetInt("_Brush", 0);
		}

		int PreviousPage = 0;
		int CurrentPage = 0;
		public static bool TerrainPageChange = false;
		public void ChangePage(int PageId)
		{
			if (CurrentPage == PageId && Page[CurrentPage].activeSelf && Selection[CurrentPage].activeSelf)
				return;
			TerrainPageChange = true;

			PreviousPage = CurrentPage;
			CurrentPage = PageId;

			for (int i = 0; i < Page.Length; i++)
			{
				Page[i].SetActive(false);
				Selection[i].SetActive(false);
			}

			if(CurrentPage == 0)
			{
				TerrainMaterial.SetInt("_Brush", 1);
			}
			else
			{
				TerrainMaterial.SetInt("_Brush", 0);
			}

			Page[CurrentPage].SetActive(true);
			Selection[CurrentPage].SetActive(true);
			TerrainPageChange = false;
		}


		#region Load all brushesh
		bool BrusheshLoaded = false;
		string StructurePath;
		public void LoadBrushes()
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
				NewBrush.GetComponent<BrushListId>().Controler = this;
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

		#region Update tool
		bool PaintStarted = false;
		bool TerainChanged = false;
		float[,] beginHeights;

		Vector3 BeginMousePos;
		float StrengthBeginValue;
		bool ChangingStrength;
		float SizeBeginValue;
		bool ChangingSize;
		void Update()
		{
			Invert = Input.GetKey(KeyCode.LeftAlt);
			Smooth = Input.GetKey(KeyCode.LeftShift);

			if (CurrentPage != 0)
				return;

			if (PaintStarted && Input.GetMouseButtonUp(0))
			{
				Map.Teren.ApplyDelayedHeightmapModification();
			}

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
						BrushStrengthSlider.value = Mathf.Clamp(StrengthBeginValue - (BeginMousePos.x - Input.mousePosition.x), 0, 100);
						UpdateMenu(true);
						//UpdateBrushPosition(true);

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
						BrushSizeSlider.value = Mathf.Clamp(SizeBeginValue - (BeginMousePos.x - Input.mousePosition.x), 1, 256);
						UpdateMenu(true);
						UpdateBrushPosition(true);

					}
				}
				else
				{
					if (Input.GetMouseButtonDown(0))
					{
						if (UpdateBrushPosition(true))
						{
							ScmapEditor.Current.TerrainMaterial.SetFloat("_GeneratingNormal", 1);
							PaintStarted = true;
							SymmetryPaint();
						}
					}
					else if (Input.GetMouseButton(0))
					{
						if (UpdateBrushPosition(false))
						{
							SymmetryPaint();
							PropsRenderer.Current.UpdatePropsHeights();
						}
					}
					else
					{
						UpdateBrushPosition(true);
					}

					if (Painting && Input.GetMouseButtonUp(0))
					{
						RegenerateMaps();
						PropsRenderer.Current.UpdatePropsHeights();
					}
				}
			}

			if (TerainChanged && Input.GetMouseButtonUp(0))
			{
				MapLuaParser.Current.History.RegisterTerrainHeightmapChange(beginHeights);
				TerainChanged = false;
			}

			if (PlayerPrefs.GetInt("Symmetry", 0) != BrushGenerator.Current.LastSym)
			{
				BrushGenerator.Current.GeneratePaintBrushesh();
			}
		}
		public float Min = 0;
		public float Max = 512;
		int LastRotation = 0;
		public void UpdateMenu(bool Slider = false)
		{
			if (Slider)
			{
				BrushSize.text = BrushSizeSlider.value.ToString();
				BrushStrength.text = BrushStrengthSlider.value.ToString();
				//BrushRotation.text = BrushRotationSlider.value.ToString();
			}
			else
			{
				BrushSizeSlider.value = float.Parse(BrushSize.text);
				BrushStrengthSlider.value = int.Parse(BrushStrength.text);
				//BrushRotationSlider.value = int.Parse(BrushRotation.text);
			}

			BrushSizeSlider.value = Mathf.Clamp(BrushSizeSlider.value, 1, 256);
			BrushStrengthSlider.value = (int)Mathf.Clamp(BrushStrengthSlider.value, 0, 100);
			//BrushRotationSlider.value = (int)Mathf.Clamp(BrushStrengthSlider.value, -360, 360);

			BrushSize.text = BrushSizeSlider.value.ToString();
			BrushStrength.text = BrushStrengthSlider.value.ToString();
			//BrushRotation.text = BrushRotationSlider.value.ToString();

			//float TerrainHeight = ScmapEditor.Current.Data.size.y;

			Min = int.Parse(BrushMini.text) / ScmapEditor.Current.Data.size.y;
			Max = int.Parse(BrushMax.text) / ScmapEditor.Current.Data.size.y;

			Min /= 10f;
			Max /= 10f;

			if (LastRotation != int.Parse(BrushRotation.text))
			{
				LastRotation = int.Parse(BrushRotation.text);
				if (LastRotation == 0)
				{
					BrushGenerator.Current.RotatedBrush = BrushGenerator.Current.Brushes[SelectedFalloff];
				}
				else
				{
					BrushGenerator.Current.RotatedBrush = BrushGenerator.rotateTexture(BrushGenerator.Current.Brushes[SelectedFalloff], LastRotation);
				}

				TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.RotatedBrush);
				BrushGenerator.Current.GeneratePaintBrushesh();
			}
			TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value);
		}
		#endregion

		#region Set Heightmap

		public void SetTerrainHeight()
		{
			int h = Map.Teren.terrainData.heightmapHeight;
			int w = Map.Teren.terrainData.heightmapWidth;
			beginHeights = Map.Teren.terrainData.GetHeights(0, 0, w, h);
			MapLuaParser.Current.History.RegisterTerrainHeightmapChange(beginHeights);

			float[,] heights = Map.Teren.terrainData.GetHeights(0, 0, Map.Teren.terrainData.heightmapWidth, Map.Teren.terrainData.heightmapHeight);

			for (int i = 0; i < Map.Teren.terrainData.heightmapWidth; i++)
			{
				for (int j = 0; j < Map.Teren.terrainData.heightmapWidth; j++)
				{
					heights[i, j] = int.Parse(TerrainAdd.text) / 128f;
				}
			}
			Map.Teren.terrainData.SetHeights(0, 0, heights);
			RegenerateMaps();
		}

		public void AddTerrainHeight()
		{
			int h = Map.Teren.terrainData.heightmapHeight;
			int w = Map.Teren.terrainData.heightmapWidth;
			beginHeights = Map.Teren.terrainData.GetHeights(0, 0, w, h);
			MapLuaParser.Current.History.RegisterTerrainHeightmapChange(beginHeights);

			float[,] heights = Map.Teren.terrainData.GetHeights(0, 0, Map.Teren.terrainData.heightmapWidth, Map.Teren.terrainData.heightmapHeight);

			for (int i = 0; i < Map.Teren.terrainData.heightmapWidth; i++)
			{
				for (int j = 0; j < Map.Teren.terrainData.heightmapWidth; j++)
				{
					heights[i, j] += int.Parse(TerrainAdd.text) / 128f;
				}
			}
			Map.Teren.terrainData.SetHeights(0, 0, heights);
			RegenerateMaps();
		}

		public const uint HeightConversion = 256 * (256 + 64); //0xFFFF

		public void ExportHeightmap()
		{
			string Filename = EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName + "/heightmap.raw";
			//string Filename = EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName + "/heightmap.raw";

			var extensions = new[]
			{
				new ExtensionFilter("Heightmap", new string[]{"raw" })
				//new ExtensionFilter("Stratum mask", "raw, bmp")
			};

			var paths = StandaloneFileBrowser.SaveFilePanel("Import stratum mask", EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName, "heightmap", extensions);


			if (paths == null || string.IsNullOrEmpty(paths))
				return;
			

			int h = Map.Teren.terrainData.heightmapHeight;
			int w = Map.Teren.terrainData.heightmapWidth;

			float[,] data = Map.Teren.terrainData.GetHeights(0, 0, w, h);

			using (BinaryWriter writer = new BinaryWriter(new System.IO.FileStream(paths, System.IO.FileMode.Create)))
			{
				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						uint ThisPixel = (uint)(data[h - (y + 1), x] * HeightConversion);
						writer.Write(System.BitConverter.GetBytes(System.BitConverter.ToUInt16(System.BitConverter.GetBytes(ThisPixel), 0)));
					}
				}
				writer.Close();
			}
		}

		public void ExportWithSizeHeightmap()
		{

			int scale = int.Parse(TerrainScale.text);
			scale = Mathf.Clamp(scale, 129, 2049);

			string Filename = EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName + "/heightmap.raw";

			var extensions = new[]
{
				new ExtensionFilter("Heightmap", new string[]{"raw" })
				//new ExtensionFilter("Stratum mask", "raw, bmp")
			};

			var paths = StandaloneFileBrowser.SaveFilePanel("Import stratum mask", EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName, "heightmap", extensions);


			if (paths == null || string.IsNullOrEmpty(paths))
				return;

			int h = Map.Teren.terrainData.heightmapWidth;
			int w = Map.Teren.terrainData.heightmapWidth;

			float[,] data = Map.Teren.terrainData.GetHeights(0, 0, w, h);

			Texture2D ExportAs = new Texture2D(Map.Teren.terrainData.heightmapWidth, Map.Teren.terrainData.heightmapWidth, TextureFormat.RGB24, false);

			float HeightValue = 1;
			HeightValue = float.Parse(TerrainScale_HeightValue.text);
			if (HeightValue < 0) HeightValue = 1;

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					float Value = data[y, x] / HeightConversion;

					if (TerrainScale_Height.isOn)
					{
						Value *= HeightValue;
					}
					float ColorR = (Mathf.Floor(Value) * (1f / 255f));
					float ColorG = (Value - Mathf.Floor(Value));

					ExportAs.SetPixel(h - (y + 1), x, new Color(ColorR, ColorG, 0));
				}
			}
			ExportAs.Apply();

			Debug.Log(ExportAs.GetPixel(128, 128).r + ", " + ExportAs.GetPixel(128, 128).g);
			Debug.Log(ExportAs.GetPixel(128, 128).r + ExportAs.GetPixel(128, 128).g * (1f / 255f));

			TextureScale.Bilinear(ExportAs, scale, scale);

			h = scale;
			w = scale;

			using (BinaryWriter writer = new BinaryWriter(new System.IO.FileStream(paths, System.IO.FileMode.Create)))
			{
				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						Color pixel = ExportAs.GetPixel(h - (y + 1), x);
						float value = (pixel.r + pixel.g * (1f / 255f));
						uint ThisPixel = (uint)(value * HeightConversion);
						writer.Write(System.BitConverter.GetBytes(System.BitConverter.ToUInt16(System.BitConverter.GetBytes(ThisPixel), 0)));
					}
				}
				writer.Close();
			}
			ExportAs = null;

		}

		public void ImportHeightmap()
		{
			string Filename = EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName + "/heightmap.raw";
			//string Filename = EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName + "/heightmap.raw";

			var extensions = new[]
			{
				new ExtensionFilter("Heightmap", new string[]{"raw", "bmp" })
				//new ExtensionFilter("Stratum mask", "raw, bmp")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Import stratum mask", EnvPaths.GetMapsPath() + MapLuaParser.Current.FolderName, extensions, false);


			if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
				return;


			int h = Map.Teren.terrainData.heightmapHeight;
			int w = Map.Teren.terrainData.heightmapWidth;
			beginHeights = Map.Teren.terrainData.GetHeights(0, 0, w, h);
			MapLuaParser.Current.History.RegisterTerrainHeightmapChange(beginHeights);


			float[,] data = new float[h, w];
			float[,] old = Map.Teren.terrainData.GetHeights(0, 0, w, h);

			if (paths[0].ToLower().EndsWith("bmp"))
			{
				BMPLoader loader = new BMPLoader();
				BMPImage img = loader.LoadBMP(paths[0]);
				Debug.Log(img.info.compressionMethod + ", " + img.info.nBitsPerPixel + ", " + img.rMask + ", " + img.imageData[0].r);
				Texture2D ImportedImage = img.ToTexture2D();


				if (ImportedImage.width != h || ImportedImage.height != w)
				{
					Debug.Log("Wrong size");
					//ImportedImage.Resize(Map.map.TexturemapTex.width, Map.map.TexturemapTex.height);
					TextureScale.Bilinear(ImportedImage, h, w);
					ImportedImage.Apply(false);
				}

				//ImportedImage = TextureFlip.FlipTextureVertical(ImportedImage, false);
				Color[] ImportedColors = ImportedImage.GetPixels();

				Debug.Log(((float)ImportedColors[128 + 128 * w].r / old[128, 128]) * 100000f);
				Debug.Log(((256 * 256) / (float)HeightConversion) * 100);

				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						//data[y, x] = ((float)ImportedColors[x + y * w].r * 128f) / (float)(256 * (256 + 64));
						data[y, x] = (float)ImportedColors[x + y * w].r / 0.567f; // 0.58
					}
				}
			}
			else
			{

				using (var file = System.IO.File.OpenRead(paths[0]))
				using (var reader = new System.IO.BinaryReader(file))
				{
					for (int y = 0; y < h; y++)
					{
						for (int x = 0; x < w; x++)
						{
							float v = (float)reader.ReadUInt16() / (float)HeightConversion;
							data[h - (y + 1), x] = v;
						}
					}
				}
			}
			Map.Teren.terrainData.SetHeights(0, 0, data);
			RegenerateMaps();
		}
		#endregion

		public void RegenerateMaps()
		{
			GenerateControlTex.GenerateWater();
			GenerateControlTex.GenerateNormal();
			if (ScmapEditor.Current.Slope)
				GenerateControlTex.Current.GenerateSlopeTexture();
		}

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
			BrushGenerator.Current.Brushes[SelectedFalloff].wrapMode = TextureWrapMode.Clamp;
			BrushGenerator.Current.Brushes[SelectedFalloff].mipMapBias = -1f;
			BrushGenerator.Current.Brushes[SelectedFalloff].filterMode = FilterMode.Bilinear;
			BrushGenerator.Current.Brushes[SelectedFalloff].anisoLevel = 2;
			LastRotation = int.Parse(BrushRotation.text);
			if (LastRotation == 0)
			{
				BrushGenerator.Current.RotatedBrush = BrushGenerator.Current.Brushes[SelectedFalloff];
			}
			else
			{
				BrushGenerator.Current.RotatedBrush = BrushGenerator.rotateTexture(BrushGenerator.Current.Brushes[SelectedFalloff], LastRotation);
			}
			TerrainMaterial.SetTexture("_BrushTex", (Texture)BrushGenerator.Current.RotatedBrush);
			BrushGenerator.Current.GeneratePaintBrushesh();
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


			MouseBeginClick = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 2000, TerrainMask))
			{
				BrushPos = hit.point;
				BrushPos.y = Map.Teren.SampleHeight(BrushPos);

				Vector3 tempCoord = Map.Teren.gameObject.transform.InverseTransformPoint(BrushPos);
				Vector3 coord = Vector3.zero;
				coord.x = (tempCoord.x - (int)BrushSizeSlider.value * MapLuaParser.GetMapSizeX() * 0.0001f) / Map.Teren.terrainData.size.x; // TODO 0.05 ?? this should be terrain proportion?
																																						  //coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
				coord.z = (tempCoord.z - (int)BrushSizeSlider.value * MapLuaParser.GetMapSizeY() * 0.0001f) / Map.Teren.terrainData.size.z;

				TerrainMaterial.SetFloat("_BrushSize", BrushSizeSlider.value);
				TerrainMaterial.SetFloat("_BrushUvX", coord.x);
				TerrainMaterial.SetFloat("_BrushUvY", coord.z);

				return true;
			}
			return false;
		}
		#endregion


		#region Painting
		bool Painting = false;
		void SymmetryPaint()
		{
			Painting = true;
			BrushGenerator.Current.GenerateSymmetry(BrushPos);

			for (int i = 0; i < BrushGenerator.Current.PaintPositions.Length; i++)
			{
				Paint(BrushGenerator.Current.PaintPositions[i], i);

			}
		}

		void Paint(Vector3 AtPosition, int id = 0)
		{
			int BrushPaintType = 0;
			if (Smooth || SelectedBrush == 2)
				BrushPaintType = 1; // Smooth
			else if (SelectedBrush == 3)
				BrushPaintType = 2;

			int hmWidth = Map.Teren.terrainData.heightmapWidth;
			int hmHeight = Map.Teren.terrainData.heightmapHeight;

			Vector3 tempCoord = Map.Teren.gameObject.transform.InverseTransformPoint(AtPosition);
			Vector3 coord = Vector3.zero;
			coord.x = tempCoord.x / Map.Teren.terrainData.size.x;
			//coord.y = tempCoord.y / Map.Teren.terrainData.size.y;
			coord.z = tempCoord.z / Map.Teren.terrainData.size.z;

			if (coord.x > 1) return;
			if (coord.x < 0) return;
			if (coord.z > 1) return;
			if (coord.z < 0) return;

			// get the position of the terrain heightmap where this game object is
			int posXInTerrain = (int)(coord.x * hmWidth);
			int posYInTerrain = (int)(coord.z * hmHeight);
			// we set an offset so that all the raising terrain is under this game object
			int size = (int)BrushSizeSlider.value;

			if (BrushPaintType == 2 && size < 8)
				size = 8;


			int offset = size / 2;
			// get the heights of the terrain under this game object

			// Horizontal Brush Offsets
			int OffsetLeft = 0;
			if (posXInTerrain - offset < 0) OffsetLeft = Mathf.Abs(posXInTerrain - offset);
			int OffsetRight = 0;
			if (posXInTerrain - offset + size > Map.Teren.terrainData.heightmapWidth) OffsetRight = posXInTerrain - offset + size - Map.Teren.terrainData.heightmapWidth;

			// Vertical Brush Offsets
			int OffsetDown = 0;
			if (posYInTerrain - offset < 0) OffsetDown = Mathf.Abs(posYInTerrain - offset);
			int OffsetTop = 0;
			if (posYInTerrain - offset + size > Map.Teren.terrainData.heightmapWidth) OffsetTop = posYInTerrain - offset + size - Map.Teren.terrainData.heightmapWidth;

			float[,] heights = Map.Teren.terrainData.GetHeights(posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, (size - OffsetLeft) - OffsetRight, (size - OffsetDown) - OffsetTop);
			float CenterHeight = 0;

			int i = 0;
			int j = 0;

			if (SelectedBrush == 3)
			{
				for (i = 0; i < (size - OffsetDown) - OffsetTop; i++)
				{
					for (j = 0; j < (size - OffsetLeft) - OffsetRight; j++)
					{
						CenterHeight += heights[i, j];
					}
				}
				CenterHeight /= size * size;
			}


			int SizeDown = (size - OffsetDown) - OffsetTop;
			int SizeLeft = (size - OffsetLeft) - OffsetRight;
			int x = 0;
			int y = 0;
			Color BrushValue = Color.white;
			float SambleBrush = 0;
			float PixelPower = 0;



			for (i = 0; i < SizeDown; i++)
			{
				for (j = 0; j < SizeLeft; j++)
				{
					// Brush strength
					x = (int)(((i + OffsetDown) / (float)size) * BrushGenerator.Current.PaintImage[id].width);
					y = (int)(((j + OffsetLeft) / (float)size) * BrushGenerator.Current.PaintImage[id].height);
					BrushValue = BrushGenerator.Current.PaintImage[id].GetPixel(y, x);
					SambleBrush = BrushValue.r;
					if (SambleBrush >= 0.02f)
					{
						switch (BrushPaintType)
						{
							case 1:
								CenterHeight = GetNearValues(ref heights, i, j);

								PixelPower = Mathf.Pow(Mathf.Abs(heights[i, j] - CenterHeight), 0.5f) * 10 * Mathf.Clamp01(size / 10f);
								//PixelPower = 10;

								heights[i, j] = Mathf.Lerp(heights[i, j], CenterHeight, BrushStrengthSlider.value * 0.4f * Mathf.Pow(SambleBrush, 2) * PixelPower);
								break;
							case 2:
								PixelPower = heights[i, j] - CenterHeight;
								heights[i, j] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * BrushStrengthSlider.value * 0.01f * Mathf.Pow(SambleBrush, 2);
								break;
							default:
								heights[i, j] += SambleBrush * BrushStrengthSlider.value * 0.0002f * (Invert ? (-1) : 1);
								break;
						}


						/*if (Smooth || SelectedBrush == 2)
						{
							PixelPower = Mathf.Abs(heights[i, j] - CenterHeight);
							heights[i, j] = Mathf.Lerp(heights[i, j], CenterHeight, BrushStrengthSlider.value * 0.4f * Mathf.Pow(SambleBrush, 2) * PixelPower);
						}
						else if (SelectedBrush == 3)
						{
							PixelPower = heights[i, j] - CenterHeight;
							heights[i, j] += Mathf.Lerp(PixelPower, 0, PixelPower * 10) * BrushStrengthSlider.value * 0.01f * Mathf.Pow(SambleBrush, 2);
						}
						else
						{
							heights[i, j] += SambleBrush * BrushStrengthSlider.value * 0.0002f * (Invert ? (-1) : 1);
						}*/

						heights[i, j] = Mathf.Clamp(heights[i, j], Min, Max);
					}
				}
			}

			// set the new height
			if (!TerainChanged)
			{
				beginHeights = Map.Teren.terrainData.GetHeights(0, 0, hmWidth, hmHeight);

				TerainChanged = true;
			}

			Map.Teren.terrainData.SetHeightsDelayLOD(posXInTerrain - offset + OffsetLeft, posYInTerrain - offset + OffsetDown, heights);

			Markers.MarkersControler.UpdateMarkersHeights();
		}

		float GetNearValues(ref float[,] heigths, int x, int y, int range = 1)
		{
			float ToReturn = 0f;
			int Count = 0;

			if (x > 0)
			{
				ToReturn += heigths[x - 1, y];
				Count++;
			}
			if (x < heigths.GetLength(0) - 1)
			{
				ToReturn += heigths[x + 1, y];
				Count++;
			}

			if (y > 0)
			{
				ToReturn += heigths[x, y - 1];
				Count++;
			}
			if (y < heigths.GetLength(1) - 1)
			{
				ToReturn += heigths[x, y + 1];
				Count++;
			}

			return ToReturn / Count;
		}
		#endregion
	}
}