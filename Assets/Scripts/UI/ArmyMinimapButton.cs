using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArmyMinimapButton : MonoBehaviour {

	public		ArmysWindow			Controler;
	public		int					ArmyId;
	public		Text				Name;
	public		Image				Team;

	public void Clicked(){
		Controler.ClickedArmy(ArmyId);
	}
}
