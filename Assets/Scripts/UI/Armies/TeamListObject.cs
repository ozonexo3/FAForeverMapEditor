using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamListObject : MonoBehaviour {

	public Transform Pivot;
	public int InstanceId;

	public System.Action<TeamListObject> AddAction;

	public void AddNew()
	{
		AddAction(this);
	}
}
