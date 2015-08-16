using UnityEngine;
using System.Collections;

using NLua;

public class ExampleBehaviour : MonoBehaviour {

	string source = @"ScenarioInfo = {
    name = 'Waters of ozone',
    description = 'Economy on this maps scales depend on how many players are playing in same corner. For 2 in same pleace there is additional hydro and mex. You can play here 1v1, 2v2, 3v3 and 4v4. Made by ozonex. ',
    type = 'skirmish',
    starts = true,
    preview = '',
    size = {
		512, 
		512
	},
    map = '/maps/waters of ozone.v0002/waters of ozone.scmap',
    save = '/maps/waters of ozone.v0002/waters of ozone_save.lua',
    script = '/maps/waters of ozone.v0002/waters of ozone_script.lua',
	map_version = 2,
}
";

	Lua env;

	void Awake() {
		env = new Lua();
		env.LoadCLRPackage();
		
		//System.Object[] result = new System.Object[0];
		try {
			//result = env.DoString(source);
			env.DoString(source);
		} catch(NLua.Exceptions.LuaException e) {
			Debug.LogError(FormatException(e), gameObject);
		}

		Debug.Log(env.GetTable("ScenarioInfo").RawGet("name"));
		Debug.Log(env.GetTable("ScenarioInfo").RawGet("description"));
		Debug.Log(env.GetTable("ScenarioInfo").RawGet("type"));
		Debug.Log(env.GetTable("ScenarioInfo").RawGet("map_version"));

		LuaTable particles = env["ScenarioInfo.size"] as LuaTable;
		double[] mapsize = new double[2];
		int id = 0;
		foreach (double test in particles.Values) {
			mapsize[id] = test;
			id++;
		}
		Debug.Log("Map size is: " + mapsize[0] + " x " + mapsize[1]);

		var width = env.GetTable("ScenarioInfo.size")[1].ToString();
		Debug.Log(width);
	}


	public static string FormatException(NLua.Exceptions.LuaException e) {
		string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
		return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
	}
}
