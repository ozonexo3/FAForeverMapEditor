using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OzoneDecals;

public class DecalsControler : MonoBehaviour {

	public static DecalsControler Current;


	public List<Decal> AllDecals = new List<Decal>();

	private void Awake()
	{
		Current = this;
	}

	public static List<Decal> GetAllDecals()
	{
		int Count = Current.AllDecals.Count;
		for (int i = 0; i < Count; i++)
		{
			Current.AllDecals[i].Obj.Bake();
		}

		return Current.AllDecals;
	}

	public static GameObject[] GetAllDecalsGo(out int[] AllTypes)
	{
		GameObject[] ToReturn = new GameObject[Current.AllDecals.Count];
		AllTypes = new int[Current.AllDecals.Count];
		for (int i = 0; i < ToReturn.Length; i++)
		{

			ToReturn[i] = Current.AllDecals[i].Obj.gameObject;
			AllTypes[i] = Current.AllDecals[i].Shared.GetHashCode();
		}
		return ToReturn;
	}

	public static void ChangeDecalsList(List<Decal> NewDecalsList)
	{
		HashSet<OzoneDecal> ToDestroy = new HashSet<OzoneDecal>();

		int count = Current.AllDecals.Count;
		for(int i = 0; i < count; i++)
		{
			if (!NewDecalsList.Contains(Current.AllDecals[i]) && Current.AllDecals[i].Obj)
			{
				Current.AllDecals[i].Obj.Bake();
				//DestroyImmediate(Current.AllDecals[i].Obj.gameObject);
				ToDestroy.Add(Current.AllDecals[i].Obj);
			}
		}

		count = NewDecalsList.Count;

		for (int i = 0; i < count; i++)
		{
			if (!Current.AllDecals.Contains(NewDecalsList[i]))
			{
				// Empty, create gameObject
				EditMap.DecalsInfo.CreateGameObjectFromDecal(NewDecalsList[i]);
			}
		}

		Current.AllDecals = NewDecalsList;

		Sort();

		foreach (OzoneDecal Obj in ToDestroy)
			DestroyImmediate(Obj.gameObject);
	}

	public static void AddDecal(Decal dc, int ForceOrder = -1)
	{
		if(dc == null)
		{
			Debug.LogWarning("Trying to add NULL Decal");
			return;
		}

		if (!Current.AllDecals.Contains(dc))
		{
			if (!dc.Obj.CreationObject)
			{
				if (ForceOrder >= 0)
					Current.AllDecals.Insert(ForceOrder, dc);
				else
					Current.AllDecals.Add(dc);
			}
		}
	}

	public static void RemoveDecal(Decal dc)
	{
		if (Current.AllDecals.Contains(dc))
		{
			Current.AllDecals.Remove(dc);
		}
	}

	public static void MoveUp(Decal dc)
	{
		if (!Current.AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");
		int id = Current.AllDecals.IndexOf(dc);

		if (id < Current.AllDecals.Count - 1)
		{
			Current.AllDecals.RemoveAt(id);
			Current.AllDecals.Insert(id + 1, dc);
			Debug.Log("Move Up from: " + id +", to " + Current.AllDecals.IndexOf(dc));

		}
	}

	public static void MoveDown(Decal dc)
	{
		if (!Current.AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");
		int id = Current.AllDecals.IndexOf(dc);

		if (id > 0)
		{
			Debug.Log("Move Down");
			Current.AllDecals.RemoveAt(id);
			Current.AllDecals.Insert(id - 1, dc);
		}
	}

	public static void MoveBottom(Decal dc)
	{
		if (!Current.AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");
		int id = Current.AllDecals.IndexOf(dc);


		if (id > 0)
		{
			Debug.Log("MoveBottom");

			Current.AllDecals.RemoveAt(id);
			Current.AllDecals.Insert(0, dc);
		}
	}

	public static void MoveTop(Decal dc)
	{
		if (!Current.AllDecals.Contains(dc))
			Debug.LogError("Decal not exist in all decals list");

		int id = Current.AllDecals.IndexOf(dc);

		if (id < Current.AllDecals.Count - 1)
		{
			Debug.Log("MoveTop");

			Current.AllDecals.RemoveAt(id);
			Current.AllDecals.Insert(Current.AllDecals.Count - 1, dc);
		}
	}

	public static void Sort()
	{
		int count = Current.AllDecals.Count;

		for (int i = 0; i < count; i++)
		{
			Current.AllDecals[i].Obj.tr.SetSiblingIndex(i);
		}
		Current.gameObject.SetActive(false);
		Current.gameObject.SetActive(true);
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

	IEnumerator UpdatingDecals()
	{
		const int BreakEvery = 50;
		int UpdateCount = 0;

		for (int d = 0; d < AllDecals.Count; d++)
		{
			Vector3 Pos = AllDecals[d].Obj.GetPivotPoint();
			Pos.y = ScmapEditor.Current.Teren.SampleHeight(Pos);
			AllDecals[d].Obj.MovePivotPoint(Pos);

			//AllDecals[d].tr.position = Pos;// + (AllDecals[d].tr.forward * AllDecals[d].tr.localScale.z * 0.5f) - (AllDecals[d].tr.right * AllDecals[d].tr.localScale.x * 0.5f);


			UpdateCount++;
			if (UpdateCount > BreakEvery)
			{
				UpdateCount = 0;
				yield return null;
			}
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
