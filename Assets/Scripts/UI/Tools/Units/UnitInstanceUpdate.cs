using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstanceUpdate : MonoBehaviour {

	public UnitInstance Uinst;

	private void LateUpdate()
	{
		Uinst.UpdateMatrixTranslated();
	}
}
