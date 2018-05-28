using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditMap;

public class PropsRenderer : MonoBehaviour {

	public static PropsRenderer Current;

	static bool BufforUpdate = false;
	static Coroutine UpdateProcess;

	private void Awake()
	{
		Current = this;
	}

	public static bool IsUpdating
	{
		get
		{
			return UpdateProcess != null;
		}
	}

	public void UpdatePropsHeights()
	{
		if (IsUpdating)
			BufforUpdate = true;
		else
		{
			UpdateProcess = StartCoroutine(PropsUpdater());
		}
	}



	public static void StopPropsUpdate()
	{
		if(UpdateProcess != null)
			Current.StopCoroutine(UpdateProcess);
	}


	const int PauseEvery = 1200;
	IEnumerator PropsUpdater()
	{
		yield return null;
		int step = 0;
		int count = PropsInfo.AllPropsTypes.Count;
		int i = 0;
		PropsInfo.PropTypeGroup Ptg = null;
		Vector3 LocalPos = Vector3.zero;

		for (i = 0; i < count; i++)
		{
			Ptg = PropsInfo.AllPropsTypes[i];
			foreach(Prop PropInstance in Ptg.PropsInstances)
			{

				LocalPos = PropInstance.Obj.Tr.localPosition;
				LocalPos.y = ScmapEditor.Current.Teren.SampleHeight(LocalPos);
				PropInstance.Obj.Tr.localPosition = LocalPos;

				step++;
				if (step > PauseEvery)
				{
					step = 0;
					yield return null;
				}
			}
		}

		yield return null;
		UpdateProcess = null;

		if (BufforUpdate)
		{
			BufforUpdate = false;
			UpdatePropsHeights();
		}
	}
}
