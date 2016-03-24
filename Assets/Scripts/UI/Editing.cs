using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EditMap{
	public class Editing : MonoBehaviour {

		[Header("Objects")]
		public		MapLuaParser		Scenario;
		public		CameraControler		KameraKontroler;
		public		EditingMarkers		EditMarkers;
		public		TerrainInfo			EditTerrain;
		public		GameObject[]		Categorys;
		public		GameObject[]		CategorysSelected;
		public		MarkersList			AllMarkersList;
		public		Transform			HudElements;

		[Header("State")]
		public		bool				MauseOnGameplay;
		public		EditStates			State = EditStates.MapStat;
		public		float				MirrorTolerance;


		public enum EditStates{
			MapStat, TerrainStat, TexturesStat, LightingStat, MarkersStat, DecalsStat, PropsStat, AIStat
		}

		void OnEnable(){
			ChangeCategory(0);
			State = EditStates.MapStat;
			MirrorTolerance = PlayerPrefs.GetFloat("SymmetryTolerance", 0.4f);
		}

		public void ButtonFunction(string func){
			switch(func){
			case "Save":
				Scenario.StartCoroutine("SaveMap");
				break;
			case "Map":
				State = EditStates.MapStat;
				ChangeCategory(0);
				EditMarkers.ClearWorkingElements();
				break;
			case "Terrain":
				State = EditStates.TerrainStat;
				ChangeCategory(1);
				EditMarkers.ClearWorkingElements();
				break;
			case "Textures":
				State = EditStates.TexturesStat;
				ChangeCategory(2);
				EditMarkers.ClearWorkingElements();
				break;
			case "Lighting":
				State = EditStates.LightingStat;
				ChangeCategory(3);
				EditMarkers.ClearWorkingElements();
				break;
			case "Markers":
				State = EditStates.MarkersStat;
				ChangeCategory(4);
				EditMarkers.GenerateAllWorkingElements();
				break;
			case "Decals":
				State = EditStates.DecalsStat;
				ChangeCategory(5);
				EditMarkers.ClearWorkingElements();
				break;
			case "Props":
				State = EditStates.PropsStat;
				ChangeCategory(6);
				EditMarkers.ClearWorkingElements();
				break;
			case "Ai":
				State = EditStates.AIStat;
				ChangeCategory(7);
				EditMarkers.ClearWorkingElements();
				break;
			}
		}


		public void ChangeCategory(int id = 0){
			foreach(GameObject obj in Categorys){
				obj.SetActive(false);
			}

			foreach(GameObject obj in CategorysSelected){
				obj.SetActive(false);
			}


			CategorysSelected[id].SetActive(true);
			Categorys[id].SetActive(true);
		}

		public void ChangePointerInGameplay(bool on = true){
			MauseOnGameplay = on;
		}

	}
}
