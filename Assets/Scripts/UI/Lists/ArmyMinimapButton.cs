using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ArmyMinimapButton : MonoBehaviour {

	public		ArmysWindow			Controler;
	public int InstanceId;
	public		int					ArmyId;
	public int ArmyTeam;
	public		Text				Name;
	public		Image				Team;

	public void Clicked(){
		Controler.ClickedArmy(InstanceId);
	}
}
