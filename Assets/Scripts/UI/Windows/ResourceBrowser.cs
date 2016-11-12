using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

public class ResourceBrowser : MonoBehaviour {

	public MapLuaParser LuaParser;
	public static ResourceBrowser Current;
	public static ResourceObject DragedObject;
	public static int SelectedCategory = 0;

	public GameObject Prefab_Texture;
	public GameObject Prefab_Decal;
	public GameObject Prefab_Prop;
	public Transform Pivot;
	public ScrollRect SRect;

	public Dropdown EnvType;
	public Dropdown Category;
	public GameObject Loading;

	public static bool Generating = false;

	public List<Texture2D> LoadedTextures = new List<Texture2D>();
	public List<string> LoadedPaths = new List<string>();

	public List<string> LoadedEnvPaths = new List<string>();

	public void Instantiate(){
		Current = this;
		GetGamedataFile.SetPath ();
		ReadAllFolders ();
	}

	void OnEnable () {
	//	StartCoroutine ("GenerateList");
	}

	void Update(){
		if (Input.GetMouseButtonUp (0)) {
			if(DragedObject)
				Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);
		}
	}

	const string LocalPath = "env/";
	static string[] CategoryPaths = new string[]{"layers/", "splats/", "decals/", "Props/"};

	public void ReadAllFolders(){
		EnvType.ClearOptions ();

		LoadedEnvPaths = new List<string> ();
		List<Dropdown.OptionData> NewOptions = new List<Dropdown.OptionData> ();

		if(!Directory.Exists(GetGamedataFile.GameDataPath)){
			Debug.LogError("Gamedata path not exist!");
			return;
		}

		FileStream fs = File.OpenRead(GetGamedataFile.GameDataPath + "env.scd");
		ZipFile zf = new ZipFile(fs);

		foreach (ZipEntry zipEntry in zf) {
			if (!zipEntry.IsDirectory) {
				continue;
			}

			string LocalName = zipEntry.Name.Replace("env/", "");

			if (LocalName.Length == 0)
				continue;

			int ContSeparators = 0;
			char Separator = ("/")[0];
			for (int i = 0; i < LocalName.Length; i++) {
				if (LocalName [i] == Separator) {
					ContSeparators++;
					if (ContSeparators > 1)
						break;
				}
			}
			if (ContSeparators > 1)
				continue;

			LocalName = LocalName.Replace ("/", "");

			LoadedEnvPaths.Add (LocalName);
			Dropdown.OptionData NewOptionInstance = new Dropdown.OptionData (LocalName);
			NewOptions.Add (NewOptionInstance);
		}

		LoadedEnvPaths.Add ("maps/");
		Dropdown.OptionData NewOptionInstance2 = new Dropdown.OptionData ("Map folder" );
		NewOptions.Add (NewOptionInstance2);

		EnvType.AddOptions (NewOptions);

	}

	static string SelectedObject = "";
	bool CustomLoading = false;
	public void LoadStratumTexture(string path){
		CustomLoading = true;
		StopCoroutine ("GenerateList");
		//Debug.Log ("Load browser for: " + path);
		string BeginPath = path;
		//SRect.normalizedPosition = Vector2.zero;
		Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

		path = path.Replace ("env/", "");

		string EnvTypeFolder = "";

		while (EnvTypeFolder.Length < path.Length) {
			if (path [EnvTypeFolder.Length] == "/"[0]) {
				path = path.Replace (EnvTypeFolder + "/", "");
				break;
			}
			EnvTypeFolder += path[EnvTypeFolder.Length];
		}

		for (int i = 0; i < EnvType.options.Count; i++) {
			if (EnvType.options [i].text.ToLower() == EnvTypeFolder.ToLower()) {
				EnvType.value = i;
				break;
			}
		}

		string CategoryFolder = "";
		while (CategoryFolder.Length < path.Length) {
			if (path [CategoryFolder.Length] == "/"[0]) {
				CategoryFolder += "/";
				path = path.Replace (CategoryFolder, "");
				break;
			}
			CategoryFolder += path[CategoryFolder.Length];
		}
			
		for (int i = 0; i < Category.options.Count; i++) {
			if (CategoryPaths[i].ToLower() == CategoryFolder.ToLower()) {
				Category.value = i;
				break;
			}
		}

		SelectedObject = BeginPath;


		gameObject.SetActive (true);
		StopCoroutine ("GenerateList");
		CustomLoading = false;
		StartCoroutine ("GenerateList");
	}

	IEnumerator GenerateList(){
		SelectedCategory = Category.value;
		Clean ();
		Generating = true;
		Loading.SetActive (true);


		if (LoadedEnvPaths [EnvType.value] == "maps/") {



		} else {
			ZipFile zf = null;
			try {
				FileStream fs = File.OpenRead (GetGamedataFile.GameDataPath + "env.scd");
				zf = new ZipFile (fs);

				yield return null;
				int Counter = 0;

				foreach (ZipEntry zipEntry in zf) {
					if (!zipEntry.IsFile) {
						continue;
					}
					string LocalPath = "env/" + EnvType.options [EnvType.value].text + "/" + CategoryPaths [Category.value];
					if (!zipEntry.Name.ToLower ().StartsWith (LocalPath.ToLower ()))
						continue;

					string LocalName = zipEntry.Name.Remove (0, LocalPath.Length);

					switch (Category.value) {
					case 0:
						if (GenerateTextureButton (zipEntry.Name, LocalName, Prefab_Texture))
							yield return null;
						else
							yield return null;
						break;
					case 1:
						if (GenerateTextureButton (zipEntry.Name, LocalName, Prefab_Decal))
							yield return null;
						else
							yield return null;
						break;
					case 2:
						if (GenerateTextureButton (zipEntry.Name, LocalName, Prefab_Decal))
							yield return null;
						else
							yield return null;
						break;
					case 3:
						if (GeneratePropButton (zipEntry.Name, LocalName, Prefab_Prop))
							yield return null;
						break;
					}
					Counter++;
					if (Counter >= 6) {
						Counter = 0;
						yield return null;
					}

				}
			} finally {
				if (zf != null) {
					zf.IsStreamOwner = true; // Makes close also shut the underlying stream
					zf.Close (); // Ensure we release resources
				}
			}
		}
		yield return null;
		Generating = false;
		Loading.SetActive (false);
	}

	bool GenerateTextureButton(string localpath, string LocalName, GameObject Prefab){
		if (!LocalName.EndsWith (".dds"))
			return true;

		Texture2D LoadedTex = GetGamedataFile.LoadTexture2DFromGamedata ("env.scd", localpath, false);
		GameObject NewButton = Instantiate (Prefab) as GameObject;
		NewButton.transform.SetParent (Pivot, false);
		NewButton.GetComponent<RawImage> ().texture = LoadedTex;
		NewButton.GetComponent<ResourceObject> ().Controler = this;
		NewButton.GetComponent<ResourceObject> ().InstanceId = LoadedTextures.Count;
		NewButton.GetComponent<ResourceObject> ().NameField.text = LocalName.Replace(".dds", "");
		LoadedTextures.Add(LoadedTex );
		LoadedPaths.Add (localpath);

		if (localpath.ToLower () == SelectedObject.ToLower ()) {
			NewButton.GetComponent<ResourceObject> ().Selected.SetActive (true);
			//Pivot.localPosition = Vector3.up * (Mathf.Clamp(Pivot.GetComponent<RectTransform>().sizeDelta.y - 350, 0, 1000000) - 350) + Vector3.right * Pivot.localPosition.x;
			Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			//SRect.normalizedPosition = Vector2.up;
		}
		return false;
	}

	bool GeneratePropButton(string localpath, string LocalName, GameObject Prefab){
		//Debug.Log (LocalName);
		if (!LocalName.EndsWith (".bp"))
			return false;

		//Texture2D LoadedTex = GetGamedataFile.LoadTexture2DFromGamedata ("env.scd", localpath, false);
		GameObject NewButton = Instantiate (Prefab) as GameObject;
		NewButton.transform.SetParent (Pivot, false);
		//NewButton.GetComponent<RawImage> ().texture = LoadedTex;
		NewButton.GetComponent<ResourceObject> ().Controler = this;
		NewButton.GetComponent<ResourceObject> ().InstanceId = LoadedPaths.Count;
		NewButton.GetComponent<ResourceObject> ().NameField.text = LocalName.Replace(".blueprint", "");
		//LoadedTextures.Add(LoadedTex );
		LoadedPaths.Add (localpath);

		if (localpath.ToLower () == SelectedObject.ToLower ()) {
			NewButton.GetComponent<ResourceObject> ().Selected.SetActive (true);
			//Pivot.localPosition = Vector3.up * (Mathf.Clamp(Pivot.GetComponent<RectTransform>().sizeDelta.y - 350, 0, 1000000) - 350) + Vector3.right * Pivot.localPosition.x;
			Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			//SRect.normalizedPosition = Vector2.up;
		}
		return true;
	}


	public void Clean(){
		foreach (Transform child in Pivot) {
			Destroy (child.gameObject);
		}

		LoadedTextures = new List<Texture2D>();
		LoadedPaths = new List<string>();
	}

	public void OnDropdownChanged(){
		if (!gameObject.activeSelf || CustomLoading)
			return;
		SelectedObject = "";
		StopCoroutine ("GenerateList");
		Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		StartCoroutine ("GenerateList");

	}

}
