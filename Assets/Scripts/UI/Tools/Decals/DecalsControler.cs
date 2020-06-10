using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OzoneDecals;

public class DecalsControler : MonoBehaviour {

	public static DecalsControler Current;


	public static List<Decal> AllDecals = new List<Decal>();

	private void Awake()
	{
		Current = this;
	}

	public static int DecalCount
	{
		get
		{
			return AllDecals.Count;
		}
	}

	public static List<Decal> GetAllDecals()
	{
		int Count = AllDecals.Count;
		for (int i = 0; i < Count; i++)
		{
			AllDecals[i].Obj.Bake();
		}

		return AllDecals;
	}

	public static GameObject[] GetAllDecalsGo(out int[] AllTypes)
	{
		int Count = AllDecals.Count;
		List<GameObject> ToReturn = new List<GameObject>();
		List<int> AllTypesList = new List<int>();
		for (int i = 0; i < Count; i++)
		{
			if(AllDecals[i].Obj != null)
			{
				ToReturn.Add(AllDecals[i].Obj.gameObject);
				AllTypesList.Add(AllDecals[i].Shared.GetHashCode());
			}

			//ToReturn[i] = Current.AllDecals[i].Obj.gameObject;
			//AllTypes[i] = Current.AllDecals[i].Shared.GetHashCode();
		}
		AllTypes = AllTypesList.ToArray();

		return ToReturn.ToArray();
	}

	public static void ChangeDecalsList(List<Decal> NewDecalsList)
	{
		HashSet<OzoneDecal> ToDestroy = new HashSet<OzoneDecal>();

		int count = AllDecals.Count;
		for(int i = 0; i < count; i++)
		{
			if (!NewDecalsList.Contains(AllDecals[i]) && AllDecals[i].Obj)
			{
				AllDecals[i].Obj.Bake();
				//DestroyImmediate(Current.AllDecals[i].Obj.gameObject);
				ToDestroy.Add(AllDecals[i].Obj);
			}
		}

		count = NewDecalsList.Count;

		for (int i = 0; i < count; i++)
		{
			if (!AllDecals.Contains(NewDecalsList[i]))
			{
				// Empty, create gameObject
				EditMap.DecalsInfo.CreateGameObjectFromDecal(NewDecalsList[i]);
			}
		}

		AllDecals = NewDecalsList;

		Sort();

		foreach (OzoneDecal Obj in ToDestroy)
			DestroyImmediate(Obj.gameObject);
	}

	public static void AddDecal(Decal dc)
	{
		if(dc == null)
		{
			Debug.LogWarning("Trying to add NULL Decal");
			return;
		}

		if (!AllDecals.Contains(dc))
		{
			if (!dc.Obj.CreationObject)
			{
				AllDecals.Add(dc);

				DecalsControler.Sort();
			}
		}
	}

	public static void RemoveDecal(Decal dc)
	{
		if (AllDecals.Contains(dc))
		{
			AllDecals.Remove(dc);
		}
	}

	public static void MoveUp(Decal dc)
	{
		if (!AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");
		int id = AllDecals.IndexOf(dc);

		if (id < AllDecals.Count - 1)
		{
			AllDecals.RemoveAt(id);
			AllDecals.Insert(id + 1, dc);
		}
		Sort();

	}

	public static void MoveDown(Decal dc)
	{
		if (!AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");
		int id = AllDecals.IndexOf(dc);

		if (id > 0)
		{
			AllDecals.RemoveAt(id);
			AllDecals.Insert(id - 1, dc);
		}
		Sort();

	}

	public static void MoveBottom(Decal dc)
	{
		if (!AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");
		int id = AllDecals.IndexOf(dc);


		if (id > 0)
		{
			AllDecals.RemoveAt(id);
			AllDecals.Insert(0, dc);
		}
		Sort();

	}

	public static void MoveTop(Decal dc)
	{
		if (!AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");

		int id = AllDecals.IndexOf(dc);

		if (id < AllDecals.Count - 1)
		{
			AllDecals.RemoveAt(id);
			AllDecals.Insert(AllDecals.Count, dc);
		}
		Sort();
	}

	public static void Sort()
	{
		int count = AllDecals.Count;

		//Current.gameObject.SetActive(false);

		for (int i = 0; i < count; i++)
		{
			//AllDecals[i].Obj.tr.SetSiblingIndex(i);
			//AllDecals[i].Obj.RefreshSortingArray(i);
			AllDecals[i].Obj.Index = i;
		}

		/*for (int i = 0; i < count; i++)
		{
			AllDecals[i].Obj.gameObject.SetActive(true);
		}*/
		//Current.gameObject.SetActive(true);
	}


	#region Loading

	public void UnloadDecals()
	{
		if (UpdateProcess != null)
			StopCoroutine(UpdateProcess);

		for (int d = 0; d < AllDecals.Count; d++)
		{
			if (AllDecals[d] != null)
				Destroy(AllDecals[d].Obj.gameObject);
		}

		//AllDecals = new List<OzoneDecal>();
	}

	public static bool IsUpdating
	{
		get
		{
			return Updating;
		}
	}

	static bool Updating = false;
	static bool BufforUpdate = false;
	static Coroutine UpdateProcess;
	public static void UpdateDecals()
	{
		if (!Updating)
		{
			Updating = true;
			UpdateProcess = Current.StartCoroutine(Current.UpdatingDecals());
		}
		else
			BufforUpdate = true;
	}

	const float MaxAllowedOverhead = 0.0002f;
	IEnumerator UpdatingDecals()
	{
		//const int BreakEvery = 50;
		//int UpdateCount = 0;
		float Realtime = Time.realtimeSinceStartup;
		for (int d = 0; d < AllDecals.Count; d++)
		{
			Vector3 Pos = AllDecals[d].Obj.GetPivotPoint();
			Pos.y = ScmapEditor.Current.Teren.SampleHeight(Pos);
			AllDecals[d].Obj.MovePivotPoint(Pos);
			//AllDecals[d].tr.position = Pos;// + (AllDecals[d].tr.forward * AllDecals[d].tr.localScale.z * 0.5f) - (AllDecals[d].tr.right * AllDecals[d].tr.localScale.x * 0.5f);


			if (Time.realtimeSinceStartup - Realtime > MaxAllowedOverhead)
			{
				yield return null;
				Realtime = Time.realtimeSinceStartup;
			}

			/*UpdateCount++;
			if (UpdateCount > BreakEvery)
			{
				UpdateCount = 0;
				yield return null;
			}*/
		}


		yield return null;
		UpdateProcess = null;
		Updating = false;
		if (BufforUpdate)
		{
			BufforUpdate = false;
			UpdateDecals();
		}
	}
#endregion
}
