using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsControler : MonoBehaviour {

	public static UnitsControler Current;
	private void Awake()
	{
		Current = this;
	}

	public static bool IsUpdating
	{
		get
		{
			return Updating;
		}
	}

	static bool Updating
	{
		get
		{
			return UpdateProcess != null;
		}
	}
	static bool BufforUpdate = false;
	static Coroutine UpdateProcess;
	public static void UpdateUnitsHeights()
	{
		if (!Updating)
		{
			UpdateProcess = Current.StartCoroutine(Current.UpdatingUnitsHeights());
		}
		else
			BufforUpdate = true;
	}

	public static void StopUnitsUpdate()
	{
		if (UpdateProcess != null)
			Current.StopCoroutine(UpdateProcess);
	}
	UnitInstance[] Instances = new UnitInstance[4096];
	public IEnumerator UpdatingUnitsHeights()
	{
		var ListEnum = GetGamedataFile.LoadedUnitObjects.GetEnumerator();
		int Counter = 0;
		while (ListEnum.MoveNext())
		{
			UnitSource us = ListEnum.Current.Value;

			us.Instances.CopyTo(Instances);
			int Count = us.Instances.Count;

			for(int i = 0; i < Count; i++)
			{
				if (Instances[i].UpdateAfterTerrainChange())
				{
					Counter+=10;
				}
				else
				{
					Counter++;
				}

				if (Counter > 1024)
					yield return null;
			}
		}

		yield return null;
		UpdateProcess = null;
		if (BufforUpdate)
		{
			BufforUpdate = false;
			UpdateUnitsHeights();
		}
	}
}
