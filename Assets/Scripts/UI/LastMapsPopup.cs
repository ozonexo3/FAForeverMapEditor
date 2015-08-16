using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LastMapsPopup : MonoBehaviour {

	public			LastMapBtn[]		MapBtns;
	public			int					Selected = 0;

	[System.Serializable]
	public class LastMapBtn{
		public		GameObject	Obj;
		public		Text		Name;
		public		Text		Version;
		public		Text		Other;
		public		GameObject	Sel;
	}

	void OnEnable () {
		for(int i = 0; i < MapBtns.Length; i++){
			MapBtns[i].Name.text = PlayerPrefs.GetString("MapScenarioFile_" + i, "");
			if(string.IsNullOrEmpty(MapBtns[i].Name.text)){
				MapBtns[i].Obj.SetActive(false);
			}
			else{
				MapBtns[i].Obj.SetActive(true);
				MapBtns[i].Version.text = "-";
				MapBtns[i].Sel.SetActive(false);

			}
		}

		Selected = 0;
		MapBtns[Selected].Sel.SetActive(true);
	}

	public void LastBtnFunc(int id){
		for(int i = 0; i < MapBtns.Length; i++){
			MapBtns[Selected].Sel.SetActive(false);
		}

		Selected = id;
		MapBtns[Selected].Sel.SetActive(true);
	}
}
