using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selection;

public class RenderSpawnRanges : MonoBehaviour
{

	static RenderSpawnRanges Instance;

	private void Awake()
	{
		Instance = this;
	}

	public Material lineMaterial;

	public Color BuildRangeColor;
	public Color AttackRangeColor;

	public float[] UtilityRanges;
	public float[] AtackRanges;

	public void OnRenderObject()
	{
		if (PreviewTex.IsPreview || !MapLuaParser.IsMapLoaded)
			return;

		if (!Markers.MarkersControler.Current.MarkerLayersSettings.SpawnRanges)
			return;

		lineMaterial.SetPass(0);

		var scenario = MapLuaParser.Current.ScenarioLuaFile.Data;
		for (int c = 0; c < scenario.Configurations.Length; c++)
		{
			for(int t = 0; t < scenario.Configurations[c].Teams.Length; t++)
			{
				for(int a = 0; a < scenario.Configurations[c].Teams[t].Armys.Count; a++)
				{
					MapLua.SaveLua.Marker ArmyMarker = MapLua.SaveLua.GetMarker(scenario.Configurations[c].Teams[t].Armys[a].Name);

					if (ArmyMarker != null && ArmyMarker.MarkerObj != null && ArmyMarker.MarkerType == MapLua.SaveLua.Marker.MarkerTypes.BlankMarker)
					{
						Vector3 pos = ArmyMarker.MarkerObj.transform.localPosition;
						for (int i = 0; i < UtilityRanges.Length; i++)
						{
							RenderUnitRanges.DrawDottedCircle(pos, UtilityRanges[i], BuildRangeColor);
						}
						for (int i = 0; i < AtackRanges.Length; i++)
						{
							RenderUnitRanges.DrawDottedCircle(pos, AtackRanges[i], AttackRangeColor);
						}
					}
				}
			}
		}
	}
}
