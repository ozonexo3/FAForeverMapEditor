//********************************
// 
// * Units browser
// * Copyright ozonexo3 2018
//
//********************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ozone.UI;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

namespace FAF.MapEditor
{
	public class UnitBrowser : MonoBehaviour
	{

		public static UnitBrowser Current;

		// Drag data

		public FafEditorSettings Preferences;

		[Header("UI")]
		public GameObject Prefab;
		public Transform Pivot;
		public GameObject Loading;
		public Texture2D CursorImage;
		public LayoutGroup Layout;
		public ContentSizeFitter SizeFitter;
		public UiTextField Search;
		public Dropdown Faction;

		#region Init
		public void Instantiate()
		{
			Current = this;
			ReadAllUnits();
		}
		#endregion

		public void ShowBrowser()
		{
			gameObject.SetActive(true);
		}

		private void OnEnable()
		{
			if (!Initialised)
			{
				ReadAllUnits();
			}

			if (!Initialised)
			{
				gameObject.SetActive(false);
				return;
			}

			if (!IsGenerated && !IsGenerating)
				GenerateUnits();
			else
				SortUnits();
		}

		void Update()
		{
			if (Input.GetMouseButtonUp(0))
			{
				if (ResourceBrowser.DragedObject)
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}

		public static GetGamedataFile.UnitDB GetDragUnit()
		{
			return Current.LoadedUnits[ResourceBrowser.DragedObject.InstanceId];
		}

		public static Texture GetDragUnitIcon()
		{
			return Current.LoadedButtons[ResourceBrowser.DragedObject.InstanceId].RawImages[1].texture;
		}

		//Icons
		public static Dictionary<string, Texture2D> IconBackgrounds = new Dictionary<string, Texture2D>();

		public List<string> FoundUnits = new List<string>();
		public List<GetGamedataFile.UnitDB> LoadedUnits = new List<GetGamedataFile.UnitDB>();
		List<ResourceObject> LoadedButtons = new List<ResourceObject>();

		bool Initialised = false;
		void ReadAllUnits()
		{
			FoundUnits.Clear();
			IconBackgrounds.Clear();

			ZipFile zf = GetGamedataFile.GetZipFileInstance(GetGamedataFile.UnitsScd);
			ZipFile FAF_zf = GetGamedataFile.GetFAFZipFileInstance(GetGamedataFile.UnitsScd);

			if (zf == null)
			{
				Preferences.Open();
				GenericInfoPopup.ShowInfo("Gamedata path not exist!");
				return;
			}

			IconBackgrounds.Add("land", GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.TexturesScd, "/textures/ui/common/icons/units/land_up.dds"));
			IconBackgrounds.Add("amph", GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.TexturesScd, "/textures/ui/common/icons/units/amph_up.dds"));
			IconBackgrounds.Add("sea", GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.TexturesScd, "/textures/ui/common/icons/units/sea_up.dds"));
			IconBackgrounds.Add("air", GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.TexturesScd, "/textures/ui/common/icons/units/air_up.dds"));

			foreach (ZipEntry zipEntry in zf)
			{
				if (zipEntry.IsDirectory)
				{
					continue;
				}

				string LocalName = zipEntry.Name;
				if (LocalName.ToLower().EndsWith("mesh.bp"))
					continue;

				if (LocalName.ToLower().EndsWith(".bp") && LocalName.Split('/').Length <= 3)
				{
					FoundUnits.Add(LocalName);
				}
			}
			int Count = FoundUnits.Count;

			if (FAF_zf != null)
			{
				string[] NewFiles = GetGamedataFile.GetNewFafFiles(GetGamedataFile.UnitsScd);

				for (int i = 0; i < NewFiles.Length; i++)
				{
					string LocalName = NewFiles[i];
					if (LocalName.ToLower().EndsWith("mesh.bp"))
						continue;

					if (LocalName.ToLower().EndsWith(".bp") && LocalName.Split('/').Length <= 3)
					{
						FoundUnits.Add(LocalName);
					}
				}
			}

			Debug.Log("Found " + FoundUnits.Count + " units ( FAF: " + (FoundUnits.Count - Count) + ")");

			FoundUnits.Sort();
			Initialised = true;
		}


		public void GenerateUnits()
		{
			GeneratingList = StartCoroutine(GenerateList());
		}

		bool IsGenerating
		{
			get
			{
				return GeneratingList != null;
			}
		}

		public static string SortDescription(GetGamedataFile.UnitDB UnitDB)
		{
			return UnitDB.Category + "\n" + UnitDB.TechLevel;
		}

		bool IsGenerated = false;
		Coroutine GeneratingList;
		IEnumerator GenerateList()
		{
			Loading.SetActive(true);
			Layout.enabled = true;
			SizeFitter.enabled = true;

			int GenerateCount = 0;

			for(int i = 0; i < FoundUnits.Count; i++)
			{

				GetGamedataFile.UnitDB UnitDB = GetGamedataFile.LoadUnitBlueprintPreview(FoundUnits[i]);

				GameObject NewButton = Instantiate(Prefab) as GameObject;
				NewButton.transform.SetParent(Pivot, false);
				ResourceObject NewResObject = NewButton.GetComponent<ResourceObject>();
				NewResObject.RawImages[0].texture = IconBackgrounds[UnitDB.Icon];
				NewResObject.RawImages[1].texture = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.TexturesScd, "/textures/ui/common/icons/units/" + UnitDB.CodeName + "_icon.dds");
				if (NewResObject.RawImages[1].texture == Texture2D.whiteTexture)
					NewResObject.RawImages[1].enabled = false;
				NewResObject.InstanceId = i;
				NewResObject.NameField.text = UnitDB.CodeName;
				NewResObject.CustomTexts[0].text = SortDescription(UnitDB);
				NewResObject.CustomTexts[1].text = UnitDB.Name;
				NewButton.SetActive(CheckSorting(UnitDB));

				LoadedButtons.Add(NewResObject);
				LoadedUnits.Add(UnitDB);

				GenerateCount++;

				if (GenerateCount >= 7)
				{
					GenerateCount = 0;
					yield return null;
				}

			}

			IsGenerated = true;

			yield return null;
			Loading.SetActive(false);
			Layout.enabled = false;
			SizeFitter.enabled = false;
			GeneratingList = null;
		}


		public void SortUnits()
		{
			Layout.enabled = true;
			SizeFitter.enabled = true;

			int count = LoadedUnits.Count;
			for(int i = 0; i < count; i++)
			{
				LoadedButtons[i].gameObject.SetActive(CheckSorting(LoadedUnits[i]));
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(Pivot.GetComponent<RectTransform>());

			Layout.enabled = false;
			SizeFitter.enabled = false;
		}


		#region Sorting

		string[] SearchTags = new string[0];
		public void OnSearchChanged()
		{
			SearchTags = Search.text.Split(' ');
			SortUnits();
		}

		bool CheckSorting(GetGamedataFile.UnitDB UnitDB)
		{
			bool FactionCorrect = false;
			switch (Faction.value)
			{
				case 0:
					FactionCorrect = true;
					break;
				case 1:
					FactionCorrect = UnitDB.FactionName == "UEF";
					break;
				case 2:
					FactionCorrect = UnitDB.FactionName == "Aeon";
					break;
				case 3:
					FactionCorrect = UnitDB.FactionName == "Cybran";
					break;
				case 4:
					FactionCorrect = UnitDB.FactionName == "Seraphim";
					break;
				case 5:
					FactionCorrect = UnitDB.FactionName != "UEF" && UnitDB.FactionName != "Aeon" && UnitDB.FactionName != "Cybran" && UnitDB.FactionName != "Seraphim";
					break;
			}

			if (!FactionCorrect)
				return false;


			// UnitDB categories
			// https://github.com/FAForever/UnitDB/blob/master/res/scripts/functions.php#L15
			//

			for (int i = 0; i < SearchTags.Length; i++)
			{
				string SearchValue = SearchTags[i].ToUpper();
				switch (SearchValue)
				{
					// Category
					case "COMMAND":
						if (UnitDB.Category != "Command")
							return false;
						break;
					case "DEFENSE":
						if (UnitDB.Category != "Defense")
							return false;
						break;
					case "STRATEGIC":
						if (UnitDB.Category != "Strategic")
							return false;
						break;
					case "UTILITY":
						if (UnitDB.Category != "Utility")
							return false;
						break;
					case "SHIP":
						if (UnitDB.Category != "Ship")
							return false;
						break;
					case "ECONOMY":
						if (UnitDB.Category != "Economy")
							return false;
						break;
					case "INTELLIGENCE":
						if (UnitDB.Category != "Intelligence")
							return false;
						break;
						//TechLevel
					case "T1":
					case "TECH1":
						if (UnitDB.TechLevel != "TECH1")
							return false;
						break;
					case "T2":
					case "TECH2":
						if (UnitDB.TechLevel != "TECH2")
							return false;
						break;
					case "T3":
					case "TECH3":
						if (UnitDB.TechLevel != "TECH3")
							return false;
						break;
					case "T4":
					case "TECH4":
						if (UnitDB.TechLevel != "EXPERIMENTAL")
							return false;
						break;
						// Tags
					case "EXPERIMENTAL":
					case "EXP":
						if (!UnitDB.CategoriesHash.Contains("EXPERIMENTAL"))
							return false;
						break;
					case "LAND":
						if (!UnitDB.CategoriesHash.Contains("LAND"))
							return false;
						break;
					case "AIR":
						if (!UnitDB.CategoriesHash.Contains("AIR"))
							return false;
						break;
					case "NAVAL":
						if (!UnitDB.CategoriesHash.Contains("NAVAL"))
							return false;
						break;
					case "CIVILIAN":
						if (!UnitDB.CategoriesHash.Contains("CIVILIAN"))
							return false;
						break;
					case "MOBILE":
						if (!UnitDB.CategoriesHash.Contains("MOBILE"))
							return false;
						break;
					case "STRUCTURE":
					case "BUILDING":
						if (!UnitDB.CategoriesHash.Contains("STRUCTURE"))
							return false;
						break;
					case "CONSTRUCTION":
						if (!UnitDB.CategoriesHash.Contains("CONSTRUCTION"))
							return false;
						break;
					case "FACTORY":
						if (!UnitDB.CategoriesHash.Contains("FACTORY"))
							return false;
						break;
					case "PRODUCTSC1":
					case "SC1":
						if (!UnitDB.CategoriesHash.Contains("PRODUCTSC1"))
							return false;
						break;
					case "BUILTBYTIER1ENGINEER":
						if (!UnitDB.CategoriesHash.Contains("BUILTBYTIER1ENGINEER"))
							return false;
						break;
					case "BUILTBYTIER2ENGINEER":
						if (!UnitDB.CategoriesHash.Contains("BUILTBYTIER2ENGINEER"))
							return false;
						break;
					case "BUILTBYTIER3ENGINEER":
						if (!UnitDB.CategoriesHash.Contains("BUILTBYTIER3ENGINEER"))
							return false;
						break;
					case "BUILTBYCOMMANDER":
						if (!UnitDB.CategoriesHash.Contains("BUILTBYTIER3ENGINEER"))
							return false;
						break;
					case "DIRECTFIRE":
						if (!UnitDB.CategoriesHash.Contains("DIRECTFIRE"))
							return false;
						break;
					case "INDIRECTFIRE":
						if (!UnitDB.CategoriesHash.Contains("INDIRECTFIRE"))
							return false;
						break;
					case "ARTILLERY":
						if (!UnitDB.CategoriesHash.Contains("ARTILLERY"))
							return false;
						break;
					case "ANTIAIR":
						if (!UnitDB.CategoriesHash.Contains("ANTIAIR"))
							return false;
						break;
					case "ANTISUB":
						if (!UnitDB.CategoriesHash.Contains("ANTISUB"))
							return false;
						break;
					case "RADAR":
						if (!UnitDB.CategoriesHash.Contains("RADAR"))
							return false;
						break;
					case "SONAR":
						if (!UnitDB.CategoriesHash.Contains("SONAR"))
							return false;
						break;
					case "PLAYABLE":
					case "MULTIPLAYER":
						if (UnitDB.CategoriesHash.Contains("OPERATION") || UnitDB.CategoriesHash.Contains("CIVILIAN") || UnitDB.CategoriesHash.Contains("INSIGNIFICANTUNIT"))
							return false;
						break;
					case "Wall":
						if (!UnitDB.CategoriesHash.Contains("WALL") && !UnitDB.CategoriesHash.Contains("AIRSTAGINGPLATFORM") && !UnitDB.CategoriesHash.Contains("ORBITALSYSTEM"))
							return false;
						break;
				}

			}

			return FactionCorrect;
		}
		#endregion


	}
}