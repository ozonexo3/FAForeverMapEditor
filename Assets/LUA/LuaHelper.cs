using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NLua;

public class LuaHelper : MonoBehaviour {

	/// <summary>
	/// Lua Help Table
	/// </summary>
	public class LHTable{
		public int Count;
		public LuaTable Data;
		public string[] Keys;

		public LHTable(LuaTable ReadTable){
			Data = ReadTable;
			Count = Data.Keys.Count;
			Keys = new string[Count];
			Data.Keys.CopyTo(Keys, 0);
		}

		public LHTable(NLua.LuaTable Parent, string Key){
			Data = Parent[Key] as LuaTable;
			Count = Data.Keys.Count;
			Keys = new string[Count];
			Data.Keys.CopyTo(Keys, 0);
		}

		public LHTable(LHTable Parent, string Key){
			Data = Parent.Data[Key] as LuaTable;
			Count = Data.Keys.Count;
			Keys = new string[Count];
			Data.Keys.CopyTo(Keys, 0);
		}

		public bool HasKey(string key){
			foreach (string keys in Keys)
				if (keys == key)
					return true;
			return false;
		}

		public object GetValue(string key){
			return Data [key];
		}

		public string GetStringValue(string key){
			if (Data [key] == null)
				return "";
			return Data [key].ToString();
		}

<<<<<<< HEAD
		public string[] GetStringArrayValue(string key){
			if (Data [key] == null)
				return new string[0];

			LHTable StringTable = new LHTable (this, key);

			string[] ToReturn = new string[StringTable.Count];
			for (int i = 0; i < StringTable.Count; i++) {
				ToReturn [i] = StringTable.GetStringValue (StringTable.Keys [i]);
			}
			return ToReturn;
		}

		public bool GetBoolValue(string key){
			if (Data [key] == null)
				return false;
			return (GetStringValue(key) == "True")?true:false;
		}

		public int GetIntValue(string key){
			if (Data [key] == null)
				return 0;
=======
		public int GetIntValue(string key){
>>>>>>> origin/master
			return int.Parse(Data [key].ToString());
		}

		public float GetFloatValue(string key){
<<<<<<< HEAD
			if (Data [key] == null)
				return 0;
			return float.Parse(Data [key].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		public Color GetColorValue(string key){
			Color MyColor = Color.white;
			if (ColorUtility.TryParseHtmlString ("#"+GetStringValue ("color"), out MyColor)) {
			} else {
				Debug.LogError ("Cant parse color value: " + GetStringValue ("color"));
			}
			return MyColor;
		}

=======
			return float.Parse(Data [key].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

>>>>>>> origin/master
		public void GetVector3Value(string key, out Vector3 TargetVector){
			LuaTable MarkerPos = Data[key] as LuaTable;
			TargetVector = Vector3.zero;
			try{
			TargetVector.x = float.Parse(MarkerPos[1].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			TargetVector.y = float.Parse(MarkerPos[2].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			TargetVector.z = float.Parse(MarkerPos[3].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
			}
			catch{
				Debug.LogError ("Cant read vector from key: " + key);
			}
		}

		public void GetLuaArmyGroup(string key, out string orders, out string platoon, out LHTable UnitGroups){
			LuaTable MarkerPos = Data[key] as LuaTable;
			LuaTable MarkerPos2 = MarkerPos[1] as LuaTable;

			orders = MarkerPos2["orders"].ToString();
			platoon = MarkerPos2["platoon"].ToString();
			UnitGroups = new LHTable (MarkerPos2 ["Units"] as LuaTable);
		}
	}

	public static void ReadSpawnWithArmy(out MapLuaParser.armys ArmyValue, LHTable ReadTable){
		string SpawnName = ReadTable.GetStringValue ("SpawnWithArmy");

<<<<<<< HEAD
		//Debug.Log (SpawnName);

		if (SpawnName.Contains ("ARMY") && !SpawnName.Contains ("ARMY_")) {
			SpawnName = SpawnName.Replace ("ARMY", "ARMY_");
		}

=======
>>>>>>> origin/master
		switch(SpawnName){
		case "ARMY_1":
			ArmyValue = MapLuaParser.armys.ARMY1;
			break;
		case "ARMY_2":
			ArmyValue = MapLuaParser.armys.ARMY2;
			break;
		case "ARMY_3":
			ArmyValue = MapLuaParser.armys.ARMY3;
			break;
		case "ARMY_4":
			ArmyValue = MapLuaParser.armys.ARMY4;
			break;
		case "ARMY_5":
			ArmyValue = MapLuaParser.armys.ARMY5;
			break;
		case "ARMY_6":
			ArmyValue = MapLuaParser.armys.ARMY6;
			break;
		case "ARMY_7":
			ArmyValue = MapLuaParser.armys.ARMY7;
			break;
		case "ARMY_8":
			ArmyValue = MapLuaParser.armys.ARMY8;
			break;
		case "ARMY_9":
			ArmyValue = MapLuaParser.armys.ARMY9;
			break;
		case "ARMY_10":
			ArmyValue = MapLuaParser.armys.ARMY10;
			break;
		case "ARMY_11":
			ArmyValue = MapLuaParser.armys.ARMY11;
			break;
		case "ARMY_12":
			ArmyValue = MapLuaParser.armys.ARMY12;
			break;
<<<<<<< HEAD
		case "ARMY_13":
			ArmyValue = MapLuaParser.armys.ARMY13;
			break;
		case "ARMY_14":
			ArmyValue = MapLuaParser.armys.ARMY14;
			break;
		case "ARMY_15":
			ArmyValue = MapLuaParser.armys.ARMY15;
			break;
		case "ARMY_16":
			ArmyValue = MapLuaParser.armys.ARMY16;
			break;
=======
>>>>>>> origin/master
		default:
			ArmyValue = MapLuaParser.armys.none;
			break;
		}
	}

	public static string FloatToString(float value){
		return value.ToString ("0.000000");
	}

	static System.Text.Encoding encodeType = System.Text.Encoding.ASCII;

	public static string GetStructureText(string name){
		return System.IO.File.ReadAllText(MapLuaParser.StructurePath + name, encodeType);
	}
		
}
