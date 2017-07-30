using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace EditMap
{

	public class NewMap : MonoBehaviour
	{

		public InputField Name;
		public InputField Desc;
		public Dropdown MapType;
		public Dropdown Width;
		public Dropdown Height;
		public InputField InitialHeight;

		public Toggle Water;
		public InputField WaterElv;
		public InputField DepthElevation;
		public InputField AbyssElevation;

		private void OnEnable()
		{
			Clean();
		}

		public void Clean()
		{
			Name.text = "";
			Desc.text = "";
			MapType.value = 0;
			Width.value = 3;
			Height.value = 3;
			InitialHeight.text = "64";

		}



		public void ToggleWater()
		{
			WaterElv.interactable = Water.isOn;
			DepthElevation.interactable = Water.isOn;
			AbyssElevation.interactable = Water.isOn;
		}

		public void WaterChange()
		{
			int water = int.Parse(WaterElv.text);
			int depth = int.Parse(DepthElevation.text);
			int abyss = int.Parse(AbyssElevation.text);

			if (water < 1)
				water = 1;
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

			WaterElv.text = water.ToString();
			DepthElevation.text = depth.ToString();
			AbyssElevation.text = abyss.ToString();
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

			string path = MapPath + Name.text;
			Debug.Log(path);
			if (Directory.Exists(path))
			{
				Error = "Map folder already exist: " + path;
				Debug.LogError(Error);

				return;
			}

			ScmapEditor.Current.UnloadMap();

			string FolderName = Name.text.Replace(" ", "_");
			string FileName = FolderName.ToLower();

			Debug.Log(FolderName);

			MapLuaParser.Current.FolderName = FolderName;
			MapLuaParser.Current.ScenarioFileName = FileName + "_scenario";
			MapLuaParser.Current.FolderParentPath = MapPath;

			Directory.CreateDirectory(MapPath + MapLuaParser.Current.FolderName);

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
			MapLuaParser.Current.ScenarioLuaFile.Data.Size[0] = SizeByValue(Width.value);
			MapLuaParser.Current.ScenarioLuaFile.Data.Size[1] = SizeByValue(Height.value);
			MapLuaParser.Current.ScenarioLuaFile.Data.save = "/maps/" + FolderName  + "/" + FileName + "_save.lua";
			MapLuaParser.Current.ScenarioLuaFile.Data.script = "/maps/" + FolderName + "/" + FileName + "_script.lua";
			MapLuaParser.Current.ScenarioLuaFile.Data.map = "/maps/" + FolderName + "/" + FileName + ".scmap";
			MapLuaParser.Current.ScenarioLuaFile.Data.map_version = 1;
			MapLuaParser.Current.ScenarioLuaFile.Data.starts = true;


			MapLuaParser.Current.SaveLuaFile.Data = new MapLua.SaveLua.Scenario();

			MapLuaParser.Current.SaveLuaFile.Data.MasterChains[0].Markers = new List<MapLua.SaveLua.Marker>();

			ScmapEditor.Current.map = new Map(MapLuaParser.Current.ScenarioLuaFile.Data.Size[0], MapLuaParser.Current.ScenarioLuaFile.Data.Size[1], int.Parse(InitialHeight.text), 
				Water.isOn, int.Parse(WaterElv.text), int.Parse(DepthElevation.text), int.Parse(AbyssElevation.text));



			MapLuaParser.Current.SaveMap(false);
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
