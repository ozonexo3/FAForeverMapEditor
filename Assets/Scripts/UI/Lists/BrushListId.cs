using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BrushListId : MonoBehaviour {

	public		int			Id;
	public		Text		Name;
	public		Toggle		ThisToggle;
	public		RawImage	BrushImage;
	public		TerrainInfo	Controler;


	public Toggle SetBrushList(string NewName, Texture2D NewTexture, int NewId){
		Id = NewId;
		BrushImage.texture = NewTexture;
		Name.text = NewName;
		return ThisToggle;
	}

	public void PressToggle(){
		Controler.ChangeFalloff(Id);
	}
}
