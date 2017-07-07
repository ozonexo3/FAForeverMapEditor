using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditMap;

public class PropsRenderer : MonoBehaviour {

	public static PropsRenderer Current;

	bool Updating = false;
	bool BuforUpdate = false;
	Coroutine Cor;

	private void Awake()
	{
		Current = this;
	}

	public void UpdatePropsHeights()
	{
		if (Updating)
		{
			BuforUpdate = true;
		}
		else
		{
			Updating = true;
			Cor = StartCoroutine(PropsUpdater());
		}
	}

	public void StopPropsUpdate()
	{
		if(Updating)
		StopCoroutine(Cor);
	}


	const int PauseEvery = 1200;
	IEnumerator PropsUpdater()
	{
		int step = 0;
		int count = PropsInfo.AllPropsTypes.Count;
		int i = 0;
		int p = 0;
		int InstancesCount = 0;
		PropsInfo.PropTypeGroup Ptg = null;
		Vector3 LocalPos = Vector3.zero;

		for (i = 0; i < count; i++)
		{
			Ptg = PropsInfo.AllPropsTypes[i];
			InstancesCount = Ptg.PropsInstances.Count;

			for(p = 0; p < InstancesCount; p++)
			{
				LocalPos = Ptg.PropsInstances[p].Tr.localPosition;
				LocalPos.y = ScmapEditor.Current.Teren.SampleHeight(LocalPos);
				Ptg.PropsInstances[p].Tr.localPosition = LocalPos;

				step++;
				if (step > PauseEvery)
				{
					step = 0;
					yield return null;
				}
			}
		}

		yield return null;
		Updating = false;
	}
}
