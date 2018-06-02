//********************************
// 
// * Resource browser
// * Copyright ozonexo3 2017
//
//********************************

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

namespace FAF.MapEditor
{
	public partial class ResourceBrowser : MonoBehaviour
	{

		public static ResourceBrowser Current;

		// Drag data
		public static ResourceObject DragedObject;
		public static int SelectedCategory = 0;

		[Header("UI")]
		public GameObject Prefab_Texture;
		public GameObject Prefab_Decal;
		public GameObject Prefab_Prop;
		public Material PropMaterial;
		public Transform Pivot;
		public ScrollRect SRect;
		public Dropdown EnvType;
		public Dropdown Category;
		public GameObject Loading;
		public Texture2D CursorImage;
		public Texture2D CursorImage_Prop;
		public Texture2D CursorImage_Decal;
		public LayoutGroup Layout;
		public ContentSizeFitter SizeFitter;

		public Texture2D GetCursorImage()
		{
			if (Category.value == 1 || Category.value == 2)
				return CursorImage_Decal;
			else if (Category.value == 3)
				return CursorImage_Prop;
			else
				return CursorImage;
		}

		public static bool IsDecal()
		{
			return Current.Category.value == 1 || Current.Category.value == 2;
		}

		public static bool IsProp()
		{
			return Current.Category.value == 3;
		}

		[Header("Loaded assets")]
		public List<Texture2D> LoadedTextures = new List<Texture2D>();
		public List<string> LoadedPaths = new List<string>();
		public List<GetGamedataFile.PropObject> LoadedProps = new List<GetGamedataFile.PropObject>();

		//Local
		List<string> LoadedEnvPaths = new List<string>();
		const string LocalPath = "env/";
		static string[] CategoryPaths = new string[] { "layers/", "splats/", "decals/", "Props/" };
		string SelectedObject = "";
		bool CustomLoading = false;

		const string CurrentMapPath = "current";
		const string CurrentMapFolderPath = "maps/";
		const int PauseEveryLoadedAsset = 1;


		#region UI
		void Update()
		{
			if (Input.GetMouseButtonUp(0))
			{
				if (DragedObject)
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}

		public void OnDropdownChanged()
		{
			if (!gameObject.activeSelf || CustomLoading)
				return;
			SelectedObject = "";
			if (IsGenerating)
				StopCoroutine(GeneratingList);
			DontReload = false;
			Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			GeneratingList = StartCoroutine(GenerateList());
		}
		#endregion


		#region Init
		public void Instantiate()
		{
			Current = this;
			ReadAllFolders();
		}

		void ReadAllFolders()
		{
			EnvType.ClearOptions();

			LoadedEnvPaths = new List<string>();
			List<Dropdown.OptionData> NewOptions = new List<Dropdown.OptionData>();

			if (!Directory.Exists(EnvPaths.GetGamedataPath()))
			{
				Debug.LogError("Gamedata path not exist!");
				return;
			}




			ZipFile zf = GetGamedataFile.GetZipFileInstance(GetGamedataFile.EnvScd);

			foreach (ZipEntry zipEntry in zf)
			{
				if (!zipEntry.IsDirectory)
				{
					continue;
				}

				string LocalName = zipEntry.Name.Replace("env/", "");

				if (LocalName.Length == 0)
					continue;

				int ContSeparators = 0;
				char Separator = ("/")[0];
				for (int i = 0; i < LocalName.Length; i++)
				{
					if (LocalName[i] == Separator)
					{
						ContSeparators++;
						if (ContSeparators > 1)
							break;
					}
				}
				if (ContSeparators > 1)
					continue;

				LocalName = LocalName.Replace("/", "");

				LoadedEnvPaths.Add(LocalName);
				Dropdown.OptionData NewOptionInstance = new Dropdown.OptionData(LocalName);
				NewOptions.Add(NewOptionInstance);
			}

			LoadedEnvPaths.Add(CurrentMapFolderPath);
			Dropdown.OptionData NewOptionInstance2 = new Dropdown.OptionData("Map folder");
			NewOptions.Add(NewOptionInstance2);

			LoadedEnvPaths.Add(CurrentMapPath);
			Dropdown.OptionData NewOptionInstance3 = new Dropdown.OptionData("On map");
			NewOptions.Add(NewOptionInstance3);

			EnvType.AddOptions(NewOptions);

		}
		#endregion


		bool DontReload = false;

		public void LoadStratumTexture(string path)
		{
			int LastCategory = Category.value;
			int LastEnvType = EnvType.value;

			CustomLoading = true;
			//Debug.Log ("Load browser for: " + path);
			string BeginPath = path;
			//SRect.normalizedPosition = Vector2.zero;


			path = path.Replace("env/", "");

			string EnvTypeFolder = "";

			while (EnvTypeFolder.Length < path.Length)
			{
				if (path[EnvTypeFolder.Length] == "/"[0])
				{
					path = path.Replace(EnvTypeFolder + "/", "");
					break;
				}
				EnvTypeFolder += path[EnvTypeFolder.Length];
			}

			for (int i = 0; i < EnvType.options.Count; i++)
			{
				if (EnvType.options[i].text.ToLower() == EnvTypeFolder.ToLower())
				{
					EnvType.value = i;
					break;
				}
			}

			string CategoryFolder = "";
			while (CategoryFolder.Length < path.Length)
			{
				if (path[CategoryFolder.Length] == "/"[0])
				{
					CategoryFolder += "/";
					path = path.Replace(CategoryFolder, "");
					break;
				}
				CategoryFolder += path[CategoryFolder.Length];
			}

			for (int i = 0; i < Category.options.Count; i++)
			{
				if (CategoryPaths[i].ToLower() == CategoryFolder.ToLower())
				{
					Category.value = i;
					break;
				}
			}

			SelectedObject = BeginPath;


			gameObject.SetActive(true);

			DontReload = LastCategory == Category.value && LastEnvType == EnvType.value && !IsGenerating;

			if (!DontReload)
				Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


			if (IsGenerating)
				StopCoroutine(GeneratingList);
			CustomLoading = false;
			GeneratingList = StartCoroutine(GenerateList());
		}

		public void LoadPropBlueprint()
		{
			if (Category.value != 3)
			{
				Category.value = 3;

				gameObject.SetActive(true);
				if (IsGenerating)
					StopCoroutine(GeneratingList);
				CustomLoading = false;
				GeneratingList = StartCoroutine(GenerateList());
			}
			else
				gameObject.SetActive(true);
		}

		public void LoadDecalTexture(string path)
		{
			int LastCategory = Category.value;
			int LastEnvType = EnvType.value;

			CustomLoading = true;

			if (string.IsNullOrEmpty(path))
			{
				DontReload = false;
				Category.value = 2;
			}
			else
			{
				//Debug.Log ("Load browser for: " + path);
				string BeginPath = path;
				//SRect.normalizedPosition = Vector2.zero;


				path = path.Replace("env/", "");

				string EnvTypeFolder = "";

				while (EnvTypeFolder.Length < path.Length)
				{
					if (path[EnvTypeFolder.Length] == "/"[0])
					{
						path = path.Replace(EnvTypeFolder + "/", "");
						break;
					}
					EnvTypeFolder += path[EnvTypeFolder.Length];
				}

				for (int i = 0; i < EnvType.options.Count; i++)
				{
					if (EnvType.options[i].text.ToLower() == EnvTypeFolder.ToLower())
					{
						EnvType.value = i;
						break;
					}
				}

				string CategoryFolder = "";
				while (CategoryFolder.Length < path.Length)
				{
					if (path[CategoryFolder.Length] == "/"[0])
					{
						CategoryFolder += "/";
						path = path.Replace(CategoryFolder, "");
						break;
					}
					CategoryFolder += path[CategoryFolder.Length];
				}

				for (int i = 0; i < Category.options.Count; i++)
				{
					if (CategoryPaths[i].ToLower() == CategoryFolder.ToLower())
					{
						Category.value = i;
						break;
					}
				}

				SelectedObject = BeginPath;
				DontReload = LastCategory == Category.value && LastEnvType == EnvType.value && !IsGenerating;

			}

			gameObject.SetActive(true);


			if (!DontReload)
				Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


			if (IsGenerating)
				StopCoroutine(GeneratingList);
			CustomLoading = false;
			GeneratingList = StartCoroutine(GenerateList());
		}

		#region Generate List of Assets

		void Clean()
		{
			foreach (Transform child in Pivot)
			{
				Destroy(child.gameObject);
			}

			LoadedTextures = new List<Texture2D>();
			LoadedPaths = new List<string>();
			LoadedProps = new List<GetGamedataFile.PropObject>();
		}

		bool IsGenerating
		{
			get
			{
				return GeneratingList != null;
			}
		}

		Coroutine GeneratingList;
		IEnumerator GenerateList()
		{
			SelectedCategory = Category.value;
			if (!DontReload)
				Clean();
			else if (LastSelection)
				LastSelection.SetActive(false);
			Loading.SetActive(true);

			int Counter = 0;
			int Id = 0;
			Layout.enabled = true;
			SizeFitter.enabled = true;



			if (LoadedEnvPaths[EnvType.value] == CurrentMapFolderPath)
			{
				if (MapLuaParser.IsMapLoaded)
				{

					string LoadPath = MapLuaParser.LoadedMapFolderPath + "env/" + CategoryPaths[Category.value];
					Debug.Log("Try load assets from: " + LoadPath);
					if (Directory.Exists(LoadPath))
					{

						string[] AllFiles = Directory.GetFiles(LoadPath, "*", SearchOption.AllDirectories);
						Debug.Log("Found " + AllFiles.Length + " files in map folder");

						for (int i = 0; i < AllFiles.Length; i++)
						{
							string path = AllFiles[i];

							// Load Texture
							switch (Category.value)
							{
								case 0:
									if (path.ToLower().EndsWith(".dds"))
										Counter += GenerateMapTextureButton(LoadPath, path, Prefab_Texture);
									break;
								case 1:
									if (path.ToLower().EndsWith(".dds"))
										Counter += GenerateMapTextureButton(LoadPath, path, Prefab_Decal);
									break;
								case 2:
									if (path.ToLower().EndsWith(".dds"))
										Counter += GenerateMapTextureButton(LoadPath, path, Prefab_Decal);
									break;
								case 3:
									if (path.ToLower().EndsWith(".bp"))
										Counter += GenerateMapPropButton(LoadPath, path, Prefab_Prop);
									break;
							}

							if (Counter >= PauseEveryLoadedAsset)
							{
								Counter = 0;
								yield return null;
							}
						}
						yield return null;
						yield return null;
					}
				}



			}
			else if (LoadedEnvPaths[EnvType.value] == CurrentMapPath)
			{
				if (Category.value == 3)
				{
					int Count = EditMap.PropsInfo.AllPropsTypes.Count;
					Debug.Log("Found props: " + Count);

					for (int i = 0; i < Count; i++)
					{
						LoadAtPath(GetGamedataFile.LocalBlueprintPath(EditMap.PropsInfo.AllPropsTypes[i].Blueprint), EditMap.PropsInfo.AllPropsTypes[i].PropObject.BP.Name);

						Counter++;
						if (Counter >= PauseEveryLoadedAsset)
						{
							Counter = 0;
							yield return null;
						}
					}
				}
				else
				{
					yield return null;
					yield return null;
				}

			}
			else
			{
				ZipFile zf = null;
				try
				{
					zf = GetGamedataFile.GetZipFileInstance(GetGamedataFile.EnvScd);

					yield return null;


					foreach (ZipEntry zipEntry in zf)
					{
						if (!zipEntry.IsFile)
						{
							continue;
						}
						string LocalPath = "env/" + EnvType.options[EnvType.value].text + "/" + CategoryPaths[Category.value];
						if (!zipEntry.Name.ToLower().StartsWith(LocalPath.ToLower()))
							continue;


						if (DontReload)
						{
							if (zipEntry.Name.ToLower() == SelectedObject.ToLower())
							{
								LastSelection = Pivot.GetChild(Id).GetComponent<ResourceObject>().Selected;
								LastSelection.SetActive(true);
								Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(Id / 5f);
								break;
							}
						}
						else
						{
							string LocalName = zipEntry.Name.Remove(0, LocalPath.Length);


							LoadAtPath(zipEntry.Name, LocalName);
						}

						Id++;
						Counter++;
						if (Counter >= PauseEveryLoadedAsset)
						{
							Counter = 0;
							yield return null;
						}

					}
				}
				finally
				{
					/*
					if (zf != null)
					{
						zf.IsStreamOwner = true; // Makes close also shut the underlying stream
						zf.Close(); // Ensure we release resources
					}
					*/
				}
			}

			yield return null;
			Layout.enabled = false;
			SizeFitter.enabled = false;

			Loading.SetActive(false);
			GeneratingList = null;
		}

		public void FastFocus()
		{


		}

		void LoadAtPath(string localPath, string LocalName)
		{
			switch (Category.value)
			{
				case 0:
					if (GenerateTextureButton(localPath, LocalName, Prefab_Texture))
					{ }
					break;
				case 1:
					if (GenerateTextureButton(localPath, LocalName, Prefab_Decal))
					{ }
					break;
				case 2:
					if (GenerateTextureButton(localPath, LocalName, Prefab_Decal))
					{ }
					break;
				case 3:
					if (GeneratePropButton(localPath, LocalName, Prefab_Prop))
					{ }
					break;
			}
		}

		#endregion

		#region Buttons
		GameObject LastSelection;
		bool GenerateTextureButton(string localpath, string LocalName, GameObject Prefab)
		{
			if (!LocalName.ToLower().EndsWith(".dds"))
				return true;
			Texture2D LoadedTex;

			try
			{
				LoadedTex = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, localpath, false, false);
			}
			catch (System.Exception e)
			{
				LoadedTex = new Texture2D(128, 128);
				Debug.LogError("Can't load DDS texture: " + e);
			}

			string TexPath = "";
			if (localpath.EndsWith(".dds"))
				TexPath = LocalName.Replace(".dds", "");
			else if (localpath.EndsWith(".DDS"))
				TexPath = LocalName.Replace(".DDS", "");


			GameObject NewButton = Instantiate(Prefab) as GameObject;
			NewButton.transform.SetParent(Pivot, false);
			NewButton.GetComponent<ResourceObject>().SetImages(LoadedTex);
			NewButton.GetComponent<ResourceObject>().InstanceId = LoadedTextures.Count;
			NewButton.GetComponent<ResourceObject>().NameField.text = TexPath;
			LoadedTextures.Add(LoadedTex);
			LoadedPaths.Add(localpath);

			if (localpath.ToLower() == SelectedObject.ToLower())
			{
				LastSelection = NewButton.GetComponent<ResourceObject>().Selected;
				LastSelection.SetActive(true);
				Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			}
			return false;
		}


		bool GeneratePropButton(string localpath, string LocalName, GameObject Prefab)
		{
			if (!localpath.ToLower().EndsWith(".bp"))
				return false;

			string PropPath = "";
			if (localpath.EndsWith(".bp"))
				PropPath = LocalName.Replace(".bp", "");
			else if (localpath.EndsWith(".BP"))
				PropPath = LocalName.Replace(".BP", "");

			GetGamedataFile.PropObject LoadedProp = null;

			if (localpath.ToLower().StartsWith("maps"))
			{
				localpath = "/" + localpath;
				LoadedProp = GetGamedataFile.LoadProp(GetGamedataFile.MapScd, localpath);
			}
			else 
				LoadedProp = GetGamedataFile.LoadProp(GetGamedataFile.EnvScd, localpath);

			GameObject NewButton = Instantiate(Prefab) as GameObject;
			NewButton.transform.SetParent(Pivot, false);

			if (LoadedProp.BP.LODs.Length > 0 && LoadedProp.BP.LODs[0].Albedo)
			{
				NewButton.GetComponent<RawImage>().texture = LoadedProp.BP.LODs[0].Albedo;

			}

			ResourceObject Ro = NewButton.GetComponent<ResourceObject>();

			Ro.InstanceId = LoadedPaths.Count;
			Ro.NameField.text = LoadedProp.BP.Name;
			PropPath = PropPath.Replace(LoadedProp.BP.Name, "");
			Ro.CustomTexts[2].text = PropPath;

			Ro.CustomTexts[0].text = LoadedProp.BP.ReclaimMassMax.ToString();
			Ro.CustomTexts[1].text = LoadedProp.BP.ReclaimEnergyMax.ToString();
			LoadedPaths.Add(localpath);
			LoadedProps.Add(LoadedProp);

			if (localpath.ToLower() == SelectedObject.ToLower())
			{
				Ro.Selected.SetActive(true);
				Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			}
			return true;
		}
		#endregion


	}
}