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


	//const int PauseEvery = 1200;
	const float MaxAllowedOverhead = 0.001f;
	IEnumerator PropsUpdater()
	{
		yield return null;
		//int step = 0;
		int count = PropsInfo.AllPropsTypes.Count;
		int i = 0;
		PropsInfo.PropTypeGroup Ptg = null;
		Vector3 LocalPos = Vector3.zero;
		float Realtime = Time.realtimeSinceStartup;
		for (i = 0; i < count; i++)
		{
			Ptg = PropsInfo.AllPropsTypes[i];
			var listEnum = Ptg.PropsInstances.GetEnumerator();
			while (listEnum.MoveNext())
			{

				LocalPos = listEnum.Current.Obj.Tr.localPosition;
				LocalPos.y = ScmapEditor.Current.Teren.SampleHeight(LocalPos);
				listEnum.Current.Obj.Tr.localPosition = LocalPos;

				if (Time.realtimeSinceStartup - Realtime > MaxAllowedOverhead)
				{
					Realtime = Time.realtimeSinceStartup;
					yield return null;
				}

				/*step++;
				if (step > PauseEvery)
				{
					step = 0;
					yield return null;
				}*/
			}
			listEnum.Dispose();
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
