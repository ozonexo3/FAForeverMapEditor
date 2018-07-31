using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLua;
using EditMap;

public partial struct GetGamedataFile
{
	public struct Termac
	{
		public string Albedo;
		public string Normal;
		public int[] Orientations;
		public float Length;
		public float Width;
		public float FadeOut;

		public Termac(LuaTable Data)
		{
			object CurrentValue = null;

			CurrentValue = Data.RawGet("Albedo");
			if (CurrentValue != null)
				Albedo = CurrentValue.ToString();
			else
				Albedo = "";

			CurrentValue = Data.RawGet("Normal");
			if (CurrentValue != null)
				Normal = CurrentValue.ToString();
			else
				Normal = "";

			CurrentValue = Data.RawGet("Length");
			if (CurrentValue != null)
				Length = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				Length = 1;

			CurrentValue = Data.RawGet("FadeOut");
			if (CurrentValue != null)
				FadeOut = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				FadeOut = 150;

			CurrentValue = Data.RawGet("Width");
			if (CurrentValue != null)
				Width = LuaParser.Read.StringToFloat(CurrentValue.ToString());
			else
				Width = 1;

			if (Data.RawGet("Orientations") != null)
			{
				Orientations = LuaParser.Read.IntArrayFromTable((LuaTable)Data.RawGet("Orientations"));
			}
			else
				Orientations = new int[0];
		}
	}

	const string TarmacsDirectory = "env/Common/decals/";
	const string TarmacsFormat = ".dds";
	static void LoadTermacs(UnitBluePrint BP)
	{
		if (BP.Termacs.Length == 0)
			return;

		if (!string.IsNullOrEmpty(BP.Termacs[0].Albedo))
		{
			BP.HasTermac = true;
			BP.Termac_Albedo = new Decal();
			BP.Termac_Albedo.TexPathes = new string[2];
			BP.Termac_Albedo.TexPathes[0] = TarmacsDirectory + BP.Termacs[0].Albedo + TarmacsFormat;
			BP.Termac_Albedo.TexPathes[1] = "";
			BP.Termac_Albedo.CutOffLOD = BP.Termacs[0].FadeOut;
			BP.Termac_Albedo.Type = TerrainDecalType.TYPE_ALBEDO;

			BP.Termac_Albedo.Scale = new Vector3(BP.Termacs[0].Width, 1, BP.Termacs[0].Length);

			DecalsInfo.MargeShared(BP.Termac_Albedo);
		}

		if (!string.IsNullOrEmpty(BP.Termacs[0].Normal))
		{
			BP.HasTermac = true;
			BP.Termac_Normal = new Decal();
			BP.Termac_Normal.TexPathes = new string[2];
			BP.Termac_Normal.TexPathes[0] = TarmacsDirectory + BP.Termacs[0].Normal + TarmacsFormat;
			BP.Termac_Normal.TexPathes[1] = "";
			BP.Termac_Normal.CutOffLOD = BP.Termacs[0].FadeOut;
			BP.Termac_Normal.Type = TerrainDecalType.TYPE_NORMALS;

			BP.Termac_Normal.Scale = new Vector3(BP.Termacs[0].Width, 1, BP.Termacs[0].Length);

			DecalsInfo.MargeShared(BP.Termac_Normal);
		}
	}


}
