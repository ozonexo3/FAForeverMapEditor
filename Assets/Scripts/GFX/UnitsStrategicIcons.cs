using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsStrategicIcons : MonoBehaviour
{
	public void OnPostRender()
	{
		UnitSource.DrawAllIcons(Camera.current);
	}
}
