using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FAF.MapEditor
{
	public partial class ResourceBrowser : MonoBehaviour
	{
		int GenerateMapTextureButton(string loadPath, string absolutePath, GameObject Prefab)
		{
			Texture2D LoadedTex;

			string RelativePath = "/" + absolutePath.Replace(MapLuaParser.LoadedMapFolderPath, MapLuaParser.RelativeLoadedMapFolderPath);
			Debug.Log(RelativePath);

			try
			{
				LoadedTex = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.MapScd, RelativePath, false, false);
			}
			catch (System.Exception e)
			{
				LoadedTex = new Texture2D(128, 128);
				Debug.LogError("Can't load DDS texture: " + e);
				return 0;
			}


			string TexPath = "";
			if (RelativePath.EndsWith(".dds"))
				TexPath = RelativePath.Replace(".dds", "");
			else if (RelativePath.EndsWith(".DDS"))
				TexPath = RelativePath.Replace(".DDS", "");


			GameObject NewButton = Instantiate(Prefab) as GameObject;
			NewButton.transform.SetParent(Pivot, false);
			NewButton.GetComponent<ResourceObject>().SetImages(LoadedTex);
			NewButton.GetComponent<ResourceObject>().InstanceId = LoadedTextures.Count;
			NewButton.GetComponent<ResourceObject>().NameField.text = TexPath;
			LoadedTextures.Add(LoadedTex);
			LoadedPaths.Add(RelativePath);

			if (RelativePath.ToLower() == SelectedObject.ToLower())
			{
				LastSelection = NewButton.GetComponent<ResourceObject>().Selected;
				LastSelection.SetActive(true);
				Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			}

			return 1;
		}



		int GenerateMapPropButton(string loadPath, string absolutePath, GameObject Prefab)
		{

			string RelativePath = "/" + absolutePath.Replace(MapLuaParser.LoadedMapFolderPath, MapLuaParser.RelativeLoadedMapFolderPath);
			Debug.Log(RelativePath);

			string PropPath = "";
			if (RelativePath.EndsWith(".bp"))
				PropPath = RelativePath.Replace(".bp", "");
			else if (RelativePath.EndsWith(".BP"))
				PropPath = RelativePath.Replace(".BP", "");

			GetGamedataFile.PropObject LoadedProp = GetGamedataFile.LoadProp(GetGamedataFile.MapScd, RelativePath);

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
			LoadedPaths.Add(RelativePath);
			LoadedProps.Add(LoadedProp);

			if (RelativePath.ToLower() == SelectedObject.ToLower())
			{
				Ro.Selected.SetActive(true);
				Pivot.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 250 * Mathf.FloorToInt(LoadedPaths.Count / 5f);
			}

			return 1;
		}
	}

}
