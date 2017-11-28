using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

public class ResourceBrowser : MonoBehaviour
{

	public static ResourceBrowser Current;

	// Drag data
	public static ResourceObject DragedObject;
	public static int SelectedCategory = 0;

	[Header("UI")]
	public GameObject Prefab_Texture;
	public GameObject Prefab_Decal;
	public GameObject Prefab_Prop;
	public Transform Pivot;
	public ScrollRect SRect;
	public Dropdown EnvType;
	public Dropdown Category;
	public GameObject Loading;
	public Texture2D CursorImage;


	[Header("Loaded assets")]
	public List<Texture2D> LoadedTextures = new List<Texture2D>();
	public List<string> LoadedPaths = new List<string>();
	public List<GetGamedataFile.PropObject> LoadedProps = new List<GetGamedataFile.PropObject>();

	//Local
	bool Generating = false;
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
		StopCoroutine("GenerateList");
		DontReload = false;
		Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		StartCoroutine("GenerateList");
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

		FileStream fs = File.OpenRead(EnvPaths.GetGamedataPath() + "env.scd");
		ZipFile zf = new ZipFile(fs);

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
		//StopCoroutine("GenerateList");
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

		DontReload = LastCategory == Category.value && LastEnvType == EnvType.value && !Generating;

		if(!DontReload)
			Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


		StopCoroutine("GenerateList");
		CustomLoading = false;
		StartCoroutine("GenerateList");
	}

	public void LoadPropBlueprint()
	{
		if (Category.value != 3)
		{
			Category.value = 3;

			gameObject.SetActive(true);
			StopCoroutine("GenerateList");
			CustomLoading = false;
			StartCoroutine("GenerateList");
		}
		else
			gameObject.SetActive(true);
	}

	public void LoadDecalTexture(string path)
	{

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

	IEnumerator GenerateList()
	{
		SelectedCategory = Category.value;
		if (!DontReload)
			Clean();
		else if (LastSelection)
			LastSelection.SetActive(false);
		Generating = true;
		Loading.SetActive(true);

		int Counter = 0;
		int Id = 0;

		if (LoadedEnvPaths[EnvType.value] == CurrentMapFolderPath)
		{

			yield return null;
			yield return null;

		}
		else if (LoadedEnvPaths[EnvType.value] == CurrentMapPath)
		{
			if (Category.value == 3)
			{
				int Count = EditMap.PropsInfo.AllPropsTypes.Count;
				Debug.Log("Found props: " + Count);

				for (int i = 0; i < Count; i++)
				{
					LoadAtPath(EditMap.PropsInfo.AllPropsTypes[i].LoadBlueprint, EditMap.PropsInfo.AllPropsTypes[i].PropObject.BP.Name);

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
				FileStream fs = File.OpenRead(EnvPaths.GetGamedataPath() + "env.scd");
				zf = new ZipFile(fs);

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
				if (zf != null)
				{
					zf.IsStreamOwner = true; // Makes close also shut the underlying stream
					zf.Close(); // Ensure we release resources
				}
			}
		}
		yield return null;
		Generating = false;
		Loading.SetActive(false);
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

		if (!LocalName.EndsWith(".dds"))
			return true;
		Texture2D LoadedTex;

		try
		{
			LoadedTex = GetGamedataFile.LoadTexture2DFromGamedata("env.scd", localpath, false, false);
		}
		catch (System.Exception e)
		{
			LoadedTex = new Texture2D(128, 128);
			Debug.LogError("Can't load DDS texture: " + e);
		}


		GameObject NewButton = Instantiate(Prefab) as GameObject;
		NewButton.transform.SetParent(Pivot, false);
		//NewButton.GetComponent<RawImage> ().texture = LoadedTex;
		//NewButton.GetComponent<ResourceObject> ().Controler = this;
		NewButton.GetComponent<ResourceObject>().SetImages(LoadedTex);
		NewButton.GetComponent<ResourceObject>().InstanceId = LoadedTextures.Count;
		NewButton.GetComponent<ResourceObject>().NameField.text = LocalName.Replace(".dds", "");
		LoadedTextures.Add(LoadedTex);
		LoadedPaths.Add(localpath);

		if (localpath.ToLower() == SelectedObject.ToLower())
		{
			LastSelection = NewButton.GetComponent<ResourceObject>().Selected;
			LastSelection.SetActive(true);
			//Pivot.localPosition = Vector3.up * (Mathf.Clamp(Pivot.GetComponent<RectTransform>().sizeDelta.y - 350, 0, 1000000) - 350) + Vector3.right * Pivot.localPosition.x;
			Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			//SRect.normalizedPosition = Vector2.up;
		}
		return false;
	}

	bool GeneratePropButton(string localpath, string LocalName, GameObject Prefab)
	{
		if (!localpath.EndsWith(".bp"))
			return false;

		string PropPath = LocalName.Replace(".bp", "");

		GetGamedataFile.PropObject LoadedProp = GetGamedataFile.LoadProp("env.scd", localpath);

		//Texture2D LoadedTex = GetGamedataFile.LoadTexture2DFromGamedata ("env.scd", localpath, false);
		GameObject NewButton = Instantiate(Prefab) as GameObject;
		NewButton.transform.SetParent(Pivot, false);

		if (LoadedProp.BP.LODs.Length > 0 && LoadedProp.BP.LODs[0].Albedo)
			NewButton.GetComponent<RawImage>().texture = LoadedProp.BP.LODs[0].Albedo;

		//NewButton.GetComponent<ResourceObject> ().Controler = this;
		NewButton.GetComponent<ResourceObject>().InstanceId = LoadedPaths.Count;
		NewButton.GetComponent<ResourceObject>().NameField.text = LoadedProp.BP.Name;
		PropPath = PropPath.Replace(LoadedProp.BP.Name, "");
		NewButton.GetComponent<ResourceObject>().CustomTexts[2].text = PropPath;

		NewButton.GetComponent<ResourceObject>().CustomTexts[0].text = LoadedProp.BP.ReclaimMassMax.ToString();
		NewButton.GetComponent<ResourceObject>().CustomTexts[1].text = LoadedProp.BP.ReclaimEnergyMax.ToString();
		//LoadedTextures.Add(LoadedTex );
		LoadedPaths.Add(localpath);
		LoadedProps.Add(LoadedProp);

		if (localpath.ToLower() == SelectedObject.ToLower())
		{
			NewButton.GetComponent<ResourceObject>().Selected.SetActive(true);
			//Pivot.localPosition = Vector3.up * (Mathf.Clamp(Pivot.GetComponent<RectTransform>().sizeDelta.y - 350, 0, 1000000) - 350) + Vector3.right * Pivot.localPosition.x;
			Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			//SRect.normalizedPosition = Vector2.up;
		}
		return true;
	}
	#endregion


}
