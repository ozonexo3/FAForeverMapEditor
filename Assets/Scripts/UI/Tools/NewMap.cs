using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Ozone.UI;

namespace EditMap
{

	public class NewMap : MonoBehaviour
	{

		public UiTextField Name;
		public UiTextField Desc;
		public Dropdown TextureSet;
		public Dropdown MapType;
		public Dropdown Width;
		public Dropdown Height;
		public UiTextField InitialHeight;

		public Toggle Water;
		public UiTextField WaterElv;
		public UiTextField DepthElevation;
		public UiTextField AbyssElevation;

		private void OnEnable()
		{
			Clean();
		}

		public void Clean()
		{
			Name.SetValue("");
			Desc.SetValue("");
			MapType.value = 0;
			Width.value = 3;
			Height.value = 3;
			InitialHeight.SetValue(16);
		}

		public void ToggleWater()
		{
			//WaterElv.interactable = Water.isOn;
			//DepthElevation.interactable = Water.isOn;
			//AbyssElevation.interactable = Water.isOn;
			WaterElv.gameObject.SetActive(Water.isOn);
			DepthElevation.gameObject.SetActive(Water.isOn);
			AbyssElevation.gameObject.SetActive(Water.isOn);
		}

		public void WaterChange()
		{
			float water = WaterElv.value;
			float depth = DepthElevation.value;
			float abyss = AbyssElevation.value;

			if (water < 0)
				water = 0;
			else if (water > 128)
				water = 128;

			if (depth > water)
				depth = water;
			else if (depth < 0)
				depth = 0;

			if (abyss > depth)
				abyss = depth;
			else if (abyss < 0)
				abyss = 0;

			WaterElv.SetValue(water);
			DepthElevation.SetValue(depth);
			AbyssElevation.SetValue(abyss);
		}



		public void CreateMap()
		{
			if (string.IsNullOrEmpty(Name.text))
			{
				// Show Error
				return;
			}

			string MapPath = EnvPaths.GetMapsPath();
			string Error = "";
			if (!System.IO.Directory.Exists(MapPath))
			{
				Error = "Maps folder not exist: " + MapPath;
				Debug.LogError(Error);

				return;
			}

			string FolderName = Name.text.Replace(" ", "_") + ".v0001";

			string path = MapPath + FolderName;
			Debug.Log(path);
			if (Directory.Exists(path))
			{
				Error = "Map folder already exist: " + path;
				Debug.LogError(Error);

				return;
			}

			ScmapEditor.Current.UnloadMap();

			//string FolderName = Name.text.Replace(" ", "_");
			string FileName = Name.text.Replace(" ", "_");

			Debug.Log(FolderName);

			MapLuaParser.Current.FolderName = FolderName;
			MapLuaParser.Current.ScenarioFileName = FileName + "_scenario";
			MapLuaParser.Current.FolderParentPath = MapPath;

			Directory.CreateDirectory(MapPath + MapLuaParser.Current.FolderName);

			int NewMapWidth = SizeByValue(Width.value);
			int NewMapHeight = SizeByValue(Height.value);

			int NewMapSize = NewMapWidth;
			if (NewMapHeight > NewMapSize)
				NewMapSize = NewMapHeight;

			MapLuaParser.Current.ScenarioLuaFile.Data = new MapLua.ScenarioLua.ScenarioInfo();
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations = new MapLua.ScenarioLua.Configuration[1];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0] = new MapLua.ScenarioLua.Configuration();
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].name = "standard";
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].Teams = new MapLua.ScenarioLua.Team[0];
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].ExtraArmys = new List<MapLua.ScenarioLua.Army>();
			MapLuaParser.Current.ScenarioLuaFile.Data.Configurations[0].factions = new MapLua.ScenarioLua.Factions[0];

			MapLuaParser.Current.ScenarioLuaFile.Data.name = Name.text;
			MapLuaParser.Current.ScenarioLuaFile.Data.description = Desc.text;
			MapLuaParser.Current.ScenarioLuaFile.Data.Size = new int[2];
			MapLuaParser.Current.ScenarioLuaFile.Data.type = MapType.options[MapType.value].text.ToLower();
			MapLuaParser.Current.ScenarioLuaFile.Data.Size[0] = NewMapSize;
			MapLuaParser.Current.ScenarioLuaFile.Data.Size[1] = NewMapSize;
			MapLuaParser.Current.ScenarioLuaFile.Data.save = "/maps/" + FolderName  + "/" + FileName + "_save.lua";
			MapLuaParser.Current.ScenarioLuaFile.Data.script = "/maps/" + FolderName + "/" + FileName + "_script.lua";
			MapLuaParser.Current.ScenarioLuaFile.Data.map = "/maps/" + FolderName + "/" + FileName + ".scmap";
			MapLuaParser.Current.ScenarioLuaFile.Data.map_version = 1;
			MapLuaParser.Current.ScenarioLuaFile.Data.starts = true;


			MapLuaParser.Current.SaveLuaFile.Data = new MapLua.SaveLua.Scenario();

			MapLuaParser.Current.SaveLuaFile.Data.MasterChains[0].Markers = new List<MapLua.SaveLua.Marker>();

			MapLuaParser.Current.SaveLuaFile.Data.areas = new MapLua.SaveLua.Areas[1];
			MapLuaParser.Current.SaveLuaFile.Data.areas[0] = new MapLua.SaveLua.Areas();
			MapLuaParser.Current.SaveLuaFile.Data.areas[0].Name = "AREA_1";
			if (NewMapWidth == NewMapHeight)
				MapLuaParser.Current.SaveLuaFile.Data.areas[0].rectangle = new Rect(0, 0, NewMapWidth, NewMapHeight);
			else
			{
				if(NewMapWidth > NewMapHeight) // Horizontal
				{
					int Offset = (NewMapWidth - NewMapHeight) / 2;
					MapLuaParser.Current.SaveLuaFile.Data.areas[0].rectangle = new Rect(0, Offset, NewMapWidth, NewMapHeight + Offset);
				}
				else // Vertical
				{
					int Offset = (NewMapHeight - NewMapWidth) / 2;
					MapLuaParser.Current.SaveLuaFile.Data.areas[0].rectangle = new Rect(Offset, 0, NewMapWidth + Offset, NewMapHeight);
				}
			}


			ScmapEditor.Current.map = new Map(MapLuaParser.Current.ScenarioLuaFile.Data.Size[0], MapLuaParser.Current.ScenarioLuaFile.Data.Size[1], InitialHeight.intValue, 
				Water.isOn, WaterElv.intValue, DepthElevation.intValue, AbyssElevation.intValue);



			MapLuaParser.Current.SaveMap(false);
			MapLuaParser.Current.SaveScriptLua(1);
			Debug.Log(FolderName);
			MapLuaParser.Current.LoadFile();
			Debug.Log(FolderName);
			gameObject.SetActive(false);
		}




		int SizeByValue(int value)
		{
			switch (value)
			{
				case 0:
					return 64;
				case 1:
					return 128;
				case 2:
					return 256;
				case 3:
					return 512;
				case 4:
					return 1024;
				case 5:
					return 2048;
				case 6:
					return 4096;

			}
			return 512;
		}
	}
}
