using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EditMap;

public class BrushListId : MonoBehaviour {

	public		int			Id;
	public		Text		Name;
	public		Toggle		ThisToggle;
	public		RawImage	BrushImage;
	public		TerrainInfo	Controler;
	public		StratumInfo	Controler2;

	public Toggle SetBrushList(string NewName, Texture2D NewTexture, int NewId){
		Id = NewId;
		BrushImage.texture = NewTexture;
		Name.text = NewName;
		return ThisToggle;
	}

	public void PressToggle(){
		if (!gameObject.activeSelf || !ThisToggle.isOn)
			return;

		if (Controler)
			Controler.ChangeFalloff (Id);
		else if (Controler2)
			Controler2.ChangeFalloff (Id);
	}
}
