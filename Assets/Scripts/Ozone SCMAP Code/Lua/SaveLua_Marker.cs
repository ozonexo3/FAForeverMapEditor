using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;

namespace MapLua
{
	public partial class SaveLua
	{

		#region Marker
		//[System.Serializable]
		public class Marker
		{
			public string Name = "";
			public MarkerTypes MarkerType;
			public Markers.MarkerObject MarkerObj;
			public List<Chain> ConnectedToChains;

			public float size = 1f;
			public bool resource = false;
			public float amount = 100;
			public string color = "ff808080";
			public string type = "";
			public string prop = "";
			public Vector3 orientation = Vector3.zero;
			public Vector3 position = Vector3.zero;
			public string editorIcon = "";
			public bool hint;

			// Path
			public string graph = "";
			public string adjacentTo = "";
			public List<Marker> AdjacentToMarker = new List<Marker>();

			// Camera
			public float zoom = 30;
			public bool canSetCamera = true;
			public bool canSyncCamera = true;

			// Effect
			public Vector3 offset = Vector3.zero;
			public string EffectTemplate = "";
			public float scale = 1;

			//WeatherGenerator
			public float cloudCount = 1;
			public float cloudEmitterScale = 10;
			public float cloudEmitterScaleRange = 0.2f;
			public float cloudCountRange = 7;
			public float cloudSpread = 250f;
			public float cloudHeightRange = 15;
			public float spawnChance = 1;
			public string ForceType = "None";
			public float cloudHeight = 180;

			//WeatherDefinition
			public Vector3 WeatherDriftDirection = Vector3.zero;
			public string MapStyle = "";
			public string WeatherType04 = "None";
			public string WeatherType03 = "None";
			public string WeatherType02 = "None";
			public string WeatherType01 = "None";
			public float WeatherType04Chance = 0;
			public float WeatherType03Chance = 0;
			public float WeatherType02Chance = 0;
			public float WeatherType01Chance = 0;

			//Adaptive
			public List<int> SpawnWithArmy = new List<int>();
			public List<string> AdaptiveKeys = new List<string>();

			public const string KEY_SIZE = "size";
			public const string KEY_RESOURCE = "resource";
			public const string KEY_AMOUNT = "amount";
			public const string KEY_COLOR = "color";
			public const string KEY_TYPE = "type";
			public const string KEY_PROP = "prop";
			public const string KEY_ORIENTATION = "orientation";
			public const string KEY_POSITION = "position";

			public const string KEY_EDITORICON = "editorIcon";
			public const string KEY_HINT = "hint";

			public const string KEY_OFFSET = "offset";
			public const string KEY_EFFECTTEMPLATE = "EffectTemplate";
			public const string KEY_SCALE = "scale";

			public const string KEY_ZOOM = "zoom";
			public const string KEY_CANSETCAMERA = "canSetCamera";
			public const string KEY_CANSYNCCAMERA = "canSyncCamera";

			public const string KEY_GRAPH = "graph";
			public const string KEY_ADJACENTTO = "adjacentTo";

			public const string KEY_CLOUDCOUNT = "cloudCount", KEY_CLOUDEMITTERSCALE = "cloudEmitterScale", KEY_CLOUDEMITTERSCALERANGE = "cloudEmitterScaleRange",
				KEY_CLOUDCOUNTRANGE = "cloudCountRange", KEY_CLOUDSPREAD = "cloudSpread", KEY_CLOUDHEIGHTRANGE = "cloudHeightRange",
				KEY_SPAWNCHANCE = "spawnChance", KEY_FORCETYPE = "ForceType", KEY_CLOUDHEIGHT = "cloudHeight";

			public const string KEY_WEATHERDRIFTDIRECTION = "WeatherDriftDirection", KEY_MAPSTYLE = "MapStyle";
			public const string KEY_WEATHERTYPE01 = "WeatherType01", KEY_WEATHERTYPE02 = "WeatherType02", KEY_WEATHERTYPE03 = "WeatherType03", KEY_WEATHERTYPE04 = "WeatherType04";
			public const string KEY_WEATHERTYPE01CHANCE = "WeatherType01Chance", KEY_WEATHERTYPE02CHANCE = "WeatherType02Chance", KEY_WEATHERTYPE03CHANCE = "WeatherType03Chance", KEY_WEATHERTYPE04CHANCE = "WeatherType04Chance";

			public enum MarkerTypes
			{
				None,
				Mass, Hydrocarbon, BlankMarker, CameraInfo,
				CombatZone,
				DefensivePoint, NavalDefensivePoint,
				ProtectedExperimentalConstruction,
				ExpansionArea, LargeExpansionArea, NavalArea,
				RallyPoint, NavalRallyPoint,
				LandPathNode, AirPathNode, WaterPathNode, AmphibiousPathNode, AutoPathNode,
				NavalLink,
				TransportMarker,
				Island,
				WeatherGenerator, WeatherDefinition,
				Effect,
				NavalExclude,
				Count
			}

			public enum MarkerLayers
			{
				All, NoAI, Land, Air, Naval, AnyPath, Other
			}

			public static string MarkerTypeToString(MarkerTypes MType)
			{
				string str1 = MType.ToString();
				string newstring = "";
				for (int i = 0; i < str1.Length; i++)
				{
					if (i > 0 && char.IsUpper(str1[i]))
						newstring += " ";
					newstring += str1[i].ToString();
				}
				return newstring;
			}

			public bool AllowByType(string Key)
			{
				switch (MarkerType)
				{
					case MarkerTypes.Mass:
						return Key == KEY_SIZE || Key == KEY_RESOURCE || Key == KEY_AMOUNT || Key == KEY_EDITORICON;
					case MarkerTypes.Hydrocarbon:
						return Key == KEY_SIZE || Key == KEY_RESOURCE || Key == KEY_AMOUNT;
					case MarkerTypes.BlankMarker:
						return false;
					case MarkerTypes.LandPathNode:
					case MarkerTypes.AirPathNode:
					case MarkerTypes.WaterPathNode:
					case MarkerTypes.AmphibiousPathNode:
					case MarkerTypes.AutoPathNode:
						return Key == KEY_HINT || Key == KEY_GRAPH || Key == KEY_ADJACENTTO;
					case MarkerTypes.NavalLink:
						return false;
					case MarkerTypes.NavalExclude:
						return Key == KEY_HINT;
					case MarkerTypes.TransportMarker:
						return false;
					case MarkerTypes.CameraInfo:
						return Key == KEY_ZOOM || Key == KEY_CANSETCAMERA || Key == KEY_CANSYNCCAMERA;
					case MarkerTypes.Effect:
						return Key == KEY_OFFSET || Key == KEY_EFFECTTEMPLATE || Key == KEY_COLOR || Key == KEY_SCALE;
					case MarkerTypes.WeatherGenerator:
						return Key == KEY_CLOUDCOUNT || Key == KEY_CLOUDEMITTERSCALE || Key == KEY_CLOUDEMITTERSCALERANGE || Key == KEY_CLOUDCOUNTRANGE || Key == KEY_CLOUDSPREAD || Key == KEY_CLOUDHEIGHTRANGE
							|| Key == KEY_SPAWNCHANCE || Key == KEY_FORCETYPE || Key == KEY_CLOUDHEIGHT;
					case MarkerTypes.WeatherDefinition:
						return Key == KEY_WEATHERDRIFTDIRECTION || Key == KEY_MAPSTYLE
							|| Key == KEY_WEATHERTYPE01 || Key == KEY_WEATHERTYPE02 || Key == KEY_WEATHERTYPE03 || Key == KEY_WEATHERTYPE04
							|| Key == KEY_WEATHERTYPE01CHANCE || Key == KEY_WEATHERTYPE02CHANCE || Key == KEY_WEATHERTYPE03CHANCE || Key == KEY_WEATHERTYPE04CHANCE;
					default:
						return Key == KEY_HINT;
				}
			}

			void DefaultsByType()
			{
				switch (MarkerType)
				{
					case MarkerTypes.Mass:
						resource = true;
						amount = 100;
						prop = "/env/common/props/markers/M_Mass_prop.bp";
						color = "ff808080";
						break;
					case MarkerTypes.Hydrocarbon:
						size = 3;
						resource = true;
						amount = 100;
						prop = "/env/common/props/markers/M_Hydrocarbon_prop.bp";
						color = "ff808080";
						break;
					case MarkerTypes.CameraInfo:
						prop = "/env/common/props/markers/M_Camera_prop.bp";
						color = "ff808000";
						canSyncCamera = true;
						canSetCamera = true;
						zoom = 30;
						break;
					case MarkerTypes.Effect:
						prop = "/env/common/props/markers/M_Defensive_prop.bp";
						color = "ffbffcd0";
						scale = 1;
						offset = Vector3.zero;
						EffectTemplate = "";
						break;
					case MarkerTypes.CombatZone:
						prop = "/env/common/props/markers/M_CombatZone_prop.bp";
						hint = true;
						color = "ff800000";
						break;
					case MarkerTypes.DefensivePoint:
						prop = "/env/common/props/markers/M_Defensive_prop.bp";
						hint = true;
						color = "ff008000";
						break;
					case MarkerTypes.NavalDefensivePoint:
						prop = "/env/common/props/markers/M_Defensive_prop.bp";
						hint = true;
						color = "ff0080FF";
						break;
					case MarkerTypes.ProtectedExperimentalConstruction:
						prop = "/env/common/props/markers/M_Expansion_prop.bp";
						hint = true;
						color = "ff0000AA";
						break;
					case MarkerTypes.ExpansionArea:
						prop = "/env/common/props/markers/M_Expansion_prop.bp";
						hint = true;
						color = "ff008080";
						break;
					case MarkerTypes.LargeExpansionArea:
						prop = "/env/common/props/markers/M_Expansion_prop.bp";
						hint = true;
						color = "ff008080";
						break;
					case MarkerTypes.NavalArea:
						prop = "/env/common/props/markers/M_Expansion_prop.bp";
						hint = true;
						color = "ff0000FF";
						break;
					case MarkerTypes.RallyPoint:
						prop = "/env/common/props/markers/M_Defensive_prop.bp";
						hint = true;
						color = "FF808000";
						break;
					case MarkerTypes.NavalRallyPoint:
						prop = "/env/common/props/markers/M_Defensive_prop.bp";
						hint = true;
						color = "ff00FFFF";
						break;
					case MarkerTypes.LandPathNode:
						prop = "/env/common/props/markers/M_Path_prop.bp";
						hint = true;
						color = "ff00ff00";
						break;
					case MarkerTypes.AirPathNode:
						prop = "/env/common/props/markers/M_Path_prop.bp";
						hint = true;
						color = "ffffffff";
						break;
					case MarkerTypes.WaterPathNode:
						prop = "/env/common/props/markers/M_Path_prop.bp";
						hint = true;
						color = "ff0000ff";
						break;
					case MarkerTypes.AmphibiousPathNode:
						prop = "/env/common/props/markers/M_Path_prop.bp";
						hint = true;
						color = "ff00ffff";
						break;
					case MarkerTypes.AutoPathNode:
						prop = "/env/common/props/markers/M_Path_prop.bp";
						hint = true;
						color = "ff800000";
						break;
					case MarkerTypes.NavalLink:
						prop = "/env/common/props/markers/M_Blank_prop.bp";
						hint = false;
						color = "ffff0000";
						break;
					case MarkerTypes.NavalExclude:
						prop = "/env/common/props/markers/M_CombatZone_prop.bp";
						hint = true;
						color = "ff0000AA";
						break;
					case MarkerTypes.TransportMarker:
						prop = "/env/common/props/markers/M_Blank_prop.bp";
						hint = false;
						color = "ff80A088";
						break;
					case MarkerTypes.Island:
						prop = "/env/common/props/markers/M_Expansion_prop.bp";
						hint = true;
						color = "ffffff";
						break;
				}


				prop = "/env/common/props/markers/M_Blank_prop.bp";

			}

			public MarkerLayers LayerByType(MarkerTypes Type)
			{
				if (Type == MarkerTypes.BlankMarker || Type == MarkerTypes.Mass || Type == MarkerTypes.Hydrocarbon || Type == MarkerTypes.CameraInfo)
					return MarkerLayers.NoAI;
				else if (Type == MarkerTypes.LandPathNode || Type == MarkerTypes.RallyPoint || Type == MarkerTypes.AmphibiousPathNode || Type == MarkerTypes.AutoPathNode)
					return MarkerLayers.Land;
				else if (Type == MarkerTypes.WaterPathNode || Type == MarkerTypes.NavalRallyPoint || Type == MarkerTypes.NavalLink || Type == MarkerTypes.AutoPathNode)
					return MarkerLayers.Naval;
				else if (Type == MarkerTypes.AirPathNode)
					return MarkerLayers.Air;
				else
					return MarkerLayers.Other;
			}

			public void ForceDefaultValues()
			{
				if (MarkerType == MarkerTypes.BlankMarker)
					color = "ff800080";
				else if (MarkerType == MarkerTypes.Mass)
				{
					orientation = Vector3.zero;
					color = "ff808080";
				}
				else if (MarkerType == MarkerTypes.Hydrocarbon)
				{
					orientation = Vector3.zero;
					color = "ff008000";
				}
				else if (MarkerType == MarkerTypes.CameraInfo)
				{

				}
				else
				{
					orientation = Vector3.zero;
				}

			}

			public Marker()
			{
			}

			public Marker(MarkerTypes Type, string NewName = "")
			{
				ConnectedToChains = new List<Chain>();
				AdjacentToMarker = new List<Marker>();

				if (string.IsNullOrEmpty(NewName))
					Name = GetLowestName(Type);
				else
					Name = NewName;

				AddNewMarker(this);
				size = 1;
				position = Vector3.zero;
				orientation = Vector3.zero;

				MarkerType = Type;
				type = MarkerTypeToString(Type);

				DefaultsByType();
			}

			public Marker(Marker CopyMarker, string NewName = "")
			{
				ConnectedToChains = new List<Chain>();
				AdjacentToMarker = new List<Marker>();

				for (int i = 0; i < CopyMarker.ConnectedToChains.Count; i++)
					ConnectedToChains.Add(CopyMarker.ConnectedToChains[i]);

				for (int i = 0; i < CopyMarker.AdjacentToMarker.Count; i++)
					AdjacentToMarker.Add(CopyMarker.AdjacentToMarker[i]);

				if (string.IsNullOrEmpty(NewName))
				{
					Name = GetLowestName(CopyMarker.MarkerType);
				}
				else
					Name = NewName;

				AddNewMarker(this);

				size = CopyMarker.size;
				amount = CopyMarker.amount;


				position = ScmapEditor.WorldPosToScmap(CopyMarker.MarkerObj.transform.position);
				orientation = CopyMarker.MarkerObj.Tr.eulerAngles;
				prop = "/env/common/props/markers/M_Blank_prop.bp";

				MarkerType = CopyMarker.MarkerType;
				type = MarkerTypeToString(CopyMarker.MarkerType);

				if (MarkerType == MarkerTypes.Mass)
				{
					resource = true;
					//amount = 100;
					prop = "/env/common/props/markers/M_Mass_prop.bp";
					color = "ff808080";
				}
				else if (MarkerType == MarkerTypes.Hydrocarbon)
				{
					//size = 3;
					resource = true;
					//amount = 100;
					prop = "/env/common/props/markers/M_Hydrocarbon_prop.bp";
					color = "ff808080";
				}
				else if (MarkerType == MarkerTypes.CameraInfo)
				{
					canSyncCamera = CopyMarker.canSyncCamera;
					canSetCamera = CopyMarker.canSetCamera;
					zoom = CopyMarker.zoom;
				}
			}

			public Marker(string name, LuaTable Table)
			{
				// Create marker from table
				Name = name;
				AddNewMarker(this);
				ConnectedToChains = new List<Chain>();
				AdjacentToMarker = new List<Marker>();
				string[] Keys = LuaParser.Read.GetTableKeys(Table);

				for (int k = 0; k < Keys.Length; k++)
				{
					switch (Keys[k])
					{
						#region Search For Keys
						case KEY_POSITION:
							position = LuaParser.Read.Vector3FromTable(Table, KEY_POSITION);
							break;
						case KEY_ORIENTATION:
							orientation = LuaParser.Read.Vector3FromTable(Table, KEY_ORIENTATION);
							break;
						case KEY_SIZE:
							size = LuaParser.Read.FloatFromTable(Table, KEY_SIZE);
							break;
						case KEY_RESOURCE:
							resource = LuaParser.Read.BoolFromTable(Table, KEY_RESOURCE);
							break;
						case KEY_AMOUNT:
							amount = LuaParser.Read.FloatFromTable(Table, KEY_AMOUNT);
							break;
						case KEY_COLOR:
							color = LuaParser.Read.StringFromTable(Table, KEY_COLOR);
							break;
						case KEY_TYPE:
							type = LuaParser.Read.StringFromTable(Table, KEY_TYPE);
							break;
						case KEY_PROP:
							prop = LuaParser.Read.StringFromTable(Table, KEY_PROP);
							break;
						case KEY_EDITORICON:
							editorIcon = LuaParser.Read.StringFromTable(Table, KEY_EDITORICON);
							break;
						case KEY_HINT:
							hint = LuaParser.Read.BoolFromTable(Table, KEY_HINT);
							break;
						case KEY_ZOOM:
							zoom = LuaParser.Read.FloatFromTable(Table, KEY_ZOOM);
							break;
						case KEY_ADJACENTTO:
							adjacentTo = LuaParser.Read.StringFromTable(Table, KEY_ADJACENTTO);
							break;
						case KEY_GRAPH:
							graph = LuaParser.Read.StringFromTable(Table, KEY_GRAPH);
							break;
						case KEY_CANSETCAMERA:
							canSetCamera = LuaParser.Read.BoolFromTable(Table, KEY_CANSETCAMERA);
							break;
						case KEY_CANSYNCCAMERA:
							canSyncCamera = LuaParser.Read.BoolFromTable(Table, KEY_CANSYNCCAMERA);
							break;
						//Effect
						case KEY_OFFSET:
							offset = LuaParser.Read.Vector3FromTable(Table, KEY_OFFSET);
							break;
						case KEY_EFFECTTEMPLATE:
							EffectTemplate = LuaParser.Read.StringFromTable(Table, KEY_EFFECTTEMPLATE);
							break;
						case KEY_SCALE:
							scale = LuaParser.Read.FloatFromTable(Table, KEY_SCALE);
							break;
						// Weather Generator
						case KEY_CLOUDCOUNT:
							cloudCount = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDCOUNT);
							break;
						case KEY_CLOUDEMITTERSCALE:
							cloudEmitterScale = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDEMITTERSCALE);
							break;
						case KEY_CLOUDEMITTERSCALERANGE:
							cloudEmitterScaleRange = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDEMITTERSCALERANGE);
							break;
						case KEY_CLOUDCOUNTRANGE:
							cloudCountRange = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDCOUNTRANGE);
							break;
						case KEY_CLOUDSPREAD:
							cloudSpread = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDSPREAD);
							break;
						case KEY_CLOUDHEIGHTRANGE:
							cloudHeightRange = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDHEIGHTRANGE);
							break;
						case KEY_SPAWNCHANCE:
							spawnChance = LuaParser.Read.FloatFromTable(Table, KEY_SPAWNCHANCE);
							break;
						case KEY_FORCETYPE:
							ForceType = LuaParser.Read.StringFromTable(Table, KEY_FORCETYPE);
							break;
						case KEY_CLOUDHEIGHT:
							cloudHeight = LuaParser.Read.FloatFromTable(Table, KEY_CLOUDHEIGHT);
							break;
						// Weather Definition
						case KEY_WEATHERDRIFTDIRECTION:
							WeatherDriftDirection = LuaParser.Read.Vector3FromTable(Table, KEY_WEATHERDRIFTDIRECTION);
							break;
						case KEY_MAPSTYLE:
							MapStyle = LuaParser.Read.StringFromTable(Table, KEY_MAPSTYLE);
							break;
						case KEY_WEATHERTYPE01:
							WeatherType01 = LuaParser.Read.StringFromTable(Table, KEY_WEATHERTYPE01);
							break;
						case KEY_WEATHERTYPE02:
							WeatherType02 = LuaParser.Read.StringFromTable(Table, KEY_WEATHERTYPE02);
							break;
						case KEY_WEATHERTYPE03:
							WeatherType03 = LuaParser.Read.StringFromTable(Table, KEY_WEATHERTYPE03);
							break;
						case KEY_WEATHERTYPE04:
							WeatherType04 = LuaParser.Read.StringFromTable(Table, KEY_WEATHERTYPE04);
							break;
						case KEY_WEATHERTYPE01CHANCE:
							WeatherType01Chance = LuaParser.Read.FloatFromTable(Table, KEY_WEATHERTYPE01CHANCE);
							break;
						case KEY_WEATHERTYPE02CHANCE:
							WeatherType02Chance = LuaParser.Read.FloatFromTable(Table, KEY_WEATHERTYPE02CHANCE);
							break;
						case KEY_WEATHERTYPE03CHANCE:
							WeatherType03Chance = LuaParser.Read.FloatFromTable(Table, KEY_WEATHERTYPE03CHANCE);
							break;
						case KEY_WEATHERTYPE04CHANCE:
							WeatherType04Chance = LuaParser.Read.FloatFromTable(Table, KEY_WEATHERTYPE04CHANCE);
							break;
							#endregion
					}
				}

				if (string.IsNullOrEmpty(type))
					MarkerType = MarkerTypes.BlankMarker;
				else
				{
					MarkerType = StringToMarkerType(type);
				}
			}

			public Marker AutoMarker_Land;
			public Marker AutoMarker_Water;
			public Marker AutoMarker_Amphibious;

			public static MarkerTypes StringToMarkerType(string value)
			{
				return (MarkerTypes)System.Enum.Parse(typeof(MarkerTypes), value.Replace(" ", ""));
			}

			public bool IsOnWater()
			{
				return ScmapEditor.Current.Teren.SampleHeight(MarkerObj.transform.position) < ScmapEditor.GetWaterLevel() && ScmapEditor.Current.map.Water.HasWater;
			}

			public void SaveMarkerValues(LuaParser.Creator LuaFile)
			{
				if (MarkerType == MarkerTypes.AutoPathNode)
				{
					// Convert to Land/Amphibious/Naval markers

					if (IsOnWater())
					{ // Water
						for (int i = 0; i < AutoMarker_Water.AdjacentToMarker.Count; i++)
							if (AutoMarker_Water.AdjacentToMarker[i].MarkerType == MarkerTypes.AutoPathNode)
							{
								if (AutoMarker_Water.AdjacentToMarker[i].AutoMarker_Water != null)
									AutoMarker_Water.AdjacentToMarker[i] = AutoMarker_Water.AdjacentToMarker[i].AutoMarker_Water;
								else
								{
									AutoMarker_Water.AdjacentToMarker.RemoveAt(i);
									i--;
								}

							}

						AutoMarker_Water.SaveMarkerValues(LuaFile);
					}
					else
					{ // Land
						for (int i = 0; i < AutoMarker_Land.AdjacentToMarker.Count; i++)
							if (AutoMarker_Land.AdjacentToMarker[i].MarkerType == MarkerTypes.AutoPathNode)
							{
								if (AutoMarker_Land.AdjacentToMarker[i].AutoMarker_Land != null)
									AutoMarker_Land.AdjacentToMarker[i] = AutoMarker_Land.AdjacentToMarker[i].AutoMarker_Land;
								else
								{
									AutoMarker_Land.AdjacentToMarker.RemoveAt(i);
									i--;
								}

							}

						AutoMarker_Land.SaveMarkerValues(LuaFile);
					}

					// Amphibious
					for (int i = 0; i < AutoMarker_Amphibious.AdjacentToMarker.Count; i++)
						if (AutoMarker_Amphibious.AdjacentToMarker[i].MarkerType == MarkerTypes.AutoPathNode)
						{
							if (AutoMarker_Amphibious.AdjacentToMarker[i].AutoMarker_Amphibious != null)
								AutoMarker_Amphibious.AdjacentToMarker[i] = AutoMarker_Amphibious.AdjacentToMarker[i].AutoMarker_Amphibious;
							else
							{
								AutoMarker_Amphibious.AdjacentToMarker.RemoveAt(i);
								i--;
							}
						}

					AutoMarker_Amphibious.SaveMarkerValues(LuaFile);
				}
				else
				{

					if (MarkerObj != null)
					{
						position = ScmapEditor.WorldPosToScmap(MarkerObj.transform.position);
						if (MarkerType != MarkerTypes.CameraInfo)
							MarkerObj.transform.localRotation = Quaternion.identity;
						orientation = MarkerObj.transform.eulerAngles;
					}

					ForceDefaultValues();

					LuaFile.OpenTab(LuaParser.Write.PropertieToLua(Name) + LuaParser.Write.OpenBracketValue);


					if (AllowByType(KEY_SIZE))
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_SIZE), size));
					if (AllowByType(KEY_RESOURCE))
						LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertieToLua(KEY_RESOURCE), resource));
					if (AllowByType(KEY_AMOUNT))
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_AMOUNT), amount));

					LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_COLOR), color));

					if (AllowByType(KEY_EDITORICON))
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_EDITORICON), editorIcon));
					if (AllowByType(KEY_HINT))
						LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertieToLua(KEY_HINT), hint));

					if (AllowByType(KEY_ADJACENTTO))
					{
						if (string.IsNullOrEmpty(graph))
						{
							switch (MarkerType)
							{
								case MarkerTypes.LandPathNode:
									graph = "DefaultLand";
									break;
								case MarkerTypes.AmphibiousPathNode:
									graph = "DefaultAmphibious";
									break;
								case MarkerTypes.WaterPathNode:
									graph = "DefaultWater";
									break;
								case MarkerTypes.AirPathNode:
									graph = "DefaultAir";
									break;
							}
						}

						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_GRAPH), graph));


						adjacentTo = "";
						for (int i = 0; i < AdjacentToMarker.Count; i++)
						{
							if (i > 0)
								adjacentTo += " ";

							adjacentTo += AdjacentToMarker[i].Name;
						}

						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_ADJACENTTO), adjacentTo));
					}

					//Type
					LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_TYPE), MarkerTypeToString(MarkerType)));
					LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_PROP), prop));

					if (AllowByType(KEY_ZOOM))
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_ZOOM), zoom));
					if (AllowByType(KEY_CANSETCAMERA))
						LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CANSETCAMERA), canSetCamera));
					if (AllowByType(KEY_CANSYNCCAMERA))
						LuaFile.AddLine(LuaParser.Write.BoolToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CANSYNCCAMERA), canSyncCamera));


					if (AllowByType(KEY_OFFSET))
						LuaFile.AddLine(LuaParser.Write.Vector3ToLuaFunction(LuaParser.Write.PropertieToLua(KEY_OFFSET), offset));
					if (AllowByType(KEY_EFFECTTEMPLATE))
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_EFFECTTEMPLATE), EffectTemplate));
					if (AllowByType(KEY_SCALE))
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_SCALE), scale));

					if (MarkerType == MarkerTypes.WeatherGenerator)
					{
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDCOUNT), cloudCount));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDCOUNTRANGE), cloudCountRange));

						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDEMITTERSCALE), cloudEmitterScale));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDEMITTERSCALERANGE), cloudEmitterScaleRange));

						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDHEIGHT), cloudHeight));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDHEIGHTRANGE), cloudHeightRange));

						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_CLOUDSPREAD), cloudSpread));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_SPAWNCHANCE), spawnChance));
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_FORCETYPE), ForceType));
					}
					else if (MarkerType == MarkerTypes.WeatherDefinition)
					{
						LuaFile.AddLine(LuaParser.Write.Vector3ToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERDRIFTDIRECTION), WeatherDriftDirection));
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_MAPSTYLE), MapStyle));

						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE01), WeatherType01));
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE02), WeatherType02));
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE03), WeatherType03));
						LuaFile.AddLine(LuaParser.Write.StringToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE04), WeatherType04));

						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE01CHANCE), WeatherType01Chance));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE02CHANCE), WeatherType02Chance));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE03CHANCE), WeatherType03Chance));
						LuaFile.AddLine(LuaParser.Write.FloatToLuaFunction(LuaParser.Write.PropertieToLua(KEY_WEATHERTYPE04CHANCE), WeatherType04Chance));
					}

					//Transform
					LuaFile.AddLine(LuaParser.Write.Vector3ToLuaFunction(LuaParser.Write.PropertieToLua(KEY_ORIENTATION), orientation));
					LuaFile.AddLine(LuaParser.Write.Vector3ToLuaFunction(LuaParser.Write.PropertieToLua(KEY_POSITION), position));

					LuaFile.CloseTab(LuaParser.Write.EndBracketNext);
				}
			}


		}
		#endregion

		/*
		static HashSet<string> AllExistingNames = new HashSet<string>();

		public static void RegisterMarkerName(string MarkerName)
		{
			if (!AllExistingNames.Contains(MarkerName))
				AllExistingNames.Add(MarkerName);
		}

		public static void RemoveMarkerName(string MarkerName)
		{
			AllExistingNames.Remove(MarkerName);
		}

		public static bool NameExist(string name)
		{
			return AllExistingNames.Contains(name);
		}
		*/

		static Dictionary<string, SaveLua.Marker> AllMarkersDictionary = new Dictionary<string, SaveLua.Marker>();

		public static void ClearMarkersDictionary()
		{
			if (AllMarkersDictionary == null)
				AllMarkersDictionary = new Dictionary<string, Marker>();
			else
				AllMarkersDictionary.Clear();
		}

		public static bool AddNewMarker(SaveLua.Marker NewMarker)
		{
			if (!AllMarkersDictionary.ContainsKey(NewMarker.Name))
			{
				AllMarkersDictionary.Add(NewMarker.Name, NewMarker);
				return true;
			}
			return false;
		}

		public static void RemoveMarker(string Name)
		{
			AllMarkersDictionary.Remove(Name);
		}

		public static SaveLua.Marker GetMarker(string Name)
		{
			if (AllMarkersDictionary.ContainsKey(Name))
				return AllMarkersDictionary[Name];

			return null;
		}

		public static bool MarkerExist(string Name, bool WithObject = true)
		{
			if (WithObject)
				return AllMarkersDictionary.ContainsKey(Name) && AllMarkersDictionary[Name].MarkerObj != null;

			return AllMarkersDictionary.ContainsKey(Name);
		}


		public static string GetLowestName(Marker.MarkerTypes Type)
		{
			string prefix = "";
			if (Type == Marker.MarkerTypes.BlankMarker || Type == Marker.MarkerTypes.Mass || Type == Marker.MarkerTypes.Hydrocarbon)
				prefix = Marker.MarkerTypeToString(Type) + " ";
			else if (Type == Marker.MarkerTypes.LandPathNode)
			{
				prefix = "LandPN";
			}
			else if (Type == Marker.MarkerTypes.WaterPathNode)
			{
				prefix = "WaterPN";
			}
			else if (Type == Marker.MarkerTypes.AmphibiousPathNode)
			{
				prefix = "AmphPN";
			}
			else if (Type == Marker.MarkerTypes.AirPathNode)
			{
				prefix = "AirPN";
			}
			else
			{
				prefix = Type.ToString() + "_";
			}

			int ID = 0;
			while (MarkerExist(prefix + ID.ToString("00")))
				ID++;

			return prefix + ID.ToString("00");
		}



		private void ConnectAdjacentMarkers()
		{
			int mc = 0;
			int Mcount = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers.Count;

			for (int m = 0; m < Mcount; m++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].adjacentTo.Length > 0)
				{
					string[] Names = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].adjacentTo.Split(" ".ToCharArray());
					//Transform Tr = MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.Tr;

					for (int e = 0; e < Names.Length; e++)
					{
						int ConM = MarkerIdByName(mc, Names[e], Mcount);

						if (ConM >= 0)
						{
							MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].AdjacentToMarker.Add(MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[ConM]);
						}

					}
				}
			}
		}

		int MarkerIdByName(int mc, string SearchName, int Mcount)
		{
			for (int m = 0; m < Mcount; m++)
			{
				if (MapLuaParser.Current.SaveLuaFile.Data.MasterChains[mc].Markers[m].MarkerObj.name == SearchName)
					return m;
			}
			return -1;
		}

		public void GenerateAutoMarkers()
		{
			for (int mc = 0; mc < Data.MasterChains.Length; mc++)
			{

				if (Data.MasterChains[mc].Markers == null)
					Data.MasterChains[mc].Markers = new List<Marker>();

				int Mcount = Data.MasterChains[mc].Markers.Count;
				for (int m = 0; m < Mcount; m++)
				{
					if (Data.MasterChains[mc].Markers[m].MarkerType == Marker.MarkerTypes.AutoPathNode)
					{
						if (Data.MasterChains[mc].Markers[m].IsOnWater())
						{
							if (Data.MasterChains[mc].Markers[m].AutoMarker_Water != null)
								Data.MasterChains[mc].Markers[m].AutoMarker_Water = null;

							Data.MasterChains[mc].Markers[m].AutoMarker_Water = new Marker(Data.MasterChains[mc].Markers[m], "APM_Water_" + Data.MasterChains[mc].Markers[m].Name);
							Data.MasterChains[mc].Markers[m].AutoMarker_Water.MarkerType = Marker.MarkerTypes.WaterPathNode;
						}
						else
						{
							if (Data.MasterChains[mc].Markers[m].AutoMarker_Land != null)
								Data.MasterChains[mc].Markers[m].AutoMarker_Land = null;
							Data.MasterChains[mc].Markers[m].AutoMarker_Land = new Marker(Data.MasterChains[mc].Markers[m], "APM_Land_" + Data.MasterChains[mc].Markers[m].Name);
							Data.MasterChains[mc].Markers[m].AutoMarker_Land.MarkerType = Marker.MarkerTypes.LandPathNode;
						}


						if (Data.MasterChains[mc].Markers[m].AutoMarker_Amphibious != null)
							Data.MasterChains[mc].Markers[m].AutoMarker_Amphibious = null;
						Data.MasterChains[mc].Markers[m].AutoMarker_Amphibious = new Marker(Data.MasterChains[mc].Markers[m], "APM_Amphibious_" + Data.MasterChains[mc].Markers[m].Name);
						Data.MasterChains[mc].Markers[m].AutoMarker_Amphibious.MarkerType = Marker.MarkerTypes.AmphibiousPathNode;
					}


				}
			}
		}


	}
}
