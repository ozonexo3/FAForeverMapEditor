using UnityEngine;
using System.Collections;

public class ParsingStructureData : MonoBehaviour {

	public static string MexName = "Mex #";
	public static string HydroName = "Hydrocarbon #";
	public static string MarkerIdToString = "00";

	public static string SpawnWithArmy = "#";
	public static string Save_Position = "                    ['position'] = VECTOR3( # ),";
	public static string Save_Rotation = "                    ['orientation'] = VECTOR3( # ),";

	public static void LoadData(){
		System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

		MexName = System.IO.File.ReadAllText(MapLuaParser.StructurePath + "markers/mex_name.txt", encodeType);
		HydroName = System.IO.File.ReadAllText(MapLuaParser.StructurePath + "markers/hydro_name.txt", encodeType);
		MarkerIdToString = System.IO.File.ReadAllText(MapLuaParser.StructurePath + "markers/marker_count_string.txt", encodeType);

		SpawnWithArmy = System.IO.File.ReadAllText(MapLuaParser.StructurePath + "saves/spawnwitharmy.txt", encodeType);
		Save_Position = System.IO.File.ReadAllText(MapLuaParser.StructurePath + "saves/position.txt", encodeType);
	}


	public static string FormatException(NLua.Exceptions.LuaException e) {
		string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
		return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
	}

	public static string ToLuaStringVaue(string value){
		return "STRING( '" + value + "' )";
	}

	public static string ToLuaBooleanVaue(bool value){
		return "BOOLEAN( " + (value?("true"):("false")) + " )";
	}

	public static string LuaMarkerProperty(string name, string value){
		return "                    ['" + name + "'] = " + value + ",";
	}

	public static string ToLuaSimpleVector3Value(Vector3 value){
		return value.x.ToString () + ", " + value.y.ToString () + ", " + value.z.ToString ();
	}
}
