using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EditMap
{
	public class Editing : MonoBehaviour
	{

		[Header("Objects")]
		public MapLuaParser Scenario;
		public CameraControler KameraKontroler;
		public MarkersInfo EditMarkers;
		public TerrainInfo EditTerrain;
		public StratumInfo EditStratum;
		public GameObject[] Categorys;
		public GameObject[] CategorysSelected;
		public Transform HudElements;
		public MapInfo MapInfoMenu;
		public StratumInfo TexturesMenu;

		[Header("State")]
		public bool MauseOnGameplay;
		public EditStates State = EditStates.MapStat;
		public float MirrorTolerance;


		public enum EditStates
		{
			MapStat, TerrainStat, TexturesStat, LightingStat, MarkersStat, DecalsStat, PropsStat, AIStat
		}

		void OnEnable()
		{
			ChangeCategory(0);
			State = EditStates.MapStat;
			MirrorTolerance = PlayerPrefs.GetFloat("SymmetryTolerance", 0.4f);
		}

		public void ButtonFunction(string func)
		{
			switch (func)
			{
				case "Save":
					MapLuaParser.Current.SaveMap();
					break;
				case "Map":
					State = EditStates.MapStat;
					ChangeCategory(0);
					break;
				case "Terrain":
					State = EditStates.TerrainStat;
					ChangeCategory(1);
					break;
				case "Textures":
					State = EditStates.TexturesStat;
					ChangeCategory(2);
					break;
				case "Lighting":
					State = EditStates.LightingStat;
					ChangeCategory(3);
					break;
				case "Markers":
					State = EditStates.MarkersStat;
					ChangeCategory(4);
					break;
				case "Decals":
					State = EditStates.DecalsStat;
					ChangeCategory(5);
					break;
				case "Props":
					State = EditStates.PropsStat;
					ChangeCategory(6);
					break;
				case "Ai":
					State = EditStates.AIStat;
					ChangeCategory(7);
					break;
			}
		}

		public int GetCategoryId()
		{
			return CurrentCategory;
		}

		public void SetState(EditStates es)
		{
			if (State == es)
				return;

			State = es;
			ChangeCategory((int)State);
		}

		int CurrentCategory = 0;
		public void ChangeCategory(int id = 0)
		{
			if (id == CurrentCategory)
			{
				if(!Categorys[id].activeSelf)
					Categorys[id].SetActive(true);
				if (!CategorysSelected[id].activeSelf)
					CategorysSelected[id].SetActive(true);
				return;

			}


			foreach (GameObject obj in Categorys)
			{
				obj.SetActive(false);
			}

			foreach (GameObject obj in CategorysSelected)
			{
				obj.SetActive(false);
			}
			CurrentCategory = id;

			CategorysSelected[id].SetActive(true);
			Categorys[id].SetActive(true);
		}

		public void ChangePointerInGameplay(bool on = true)
		{
			MauseOnGameplay = on;
		}

	}
}
