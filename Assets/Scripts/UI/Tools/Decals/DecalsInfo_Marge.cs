using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OzoneDecals;

namespace EditMap
{
	public partial class DecalsInfo : MonoBehaviour
	{


		static List<Decal> NewDecals = new List<Decal>();
		public static void MargeDecals()
		{

			Decal[] Current = ScmapEditor.Current.map.Decals.ToArray();
			int OldCount = Current.Length;
			NewDecals.Clear();
			NewDecals.Capacity = OldCount;
			//NewDecals = new List<Decal>(OldCount);
			int nCount = 0;

			for(int i = 0; i < OldCount; i++)
			{
				if(i == 0)
				{
					Current[i].Shared = new Decal.DecalSharedSettings();
					Current[i].Shared.Load(Current[i]);
					NewDecals.Add(Current[i]);
					Decal.AllDecalsShared.Add(Current[i].Shared);
					nCount++;
				}
				else
				{

					if(MargeShared(Current[i], nCount))
						nCount++;

					/*
					bool found = false;
					for(int n = 0; n < nCount; n++)
					{
						if (Current[i].Compare(NewDecals[n])) {

							Current[i].Shared = NewDecals[n].Shared;
							found = true;
							break;
						}
					}
					if (!found)
					{
						Current[i].Shared = new Decal.DecalSharedSettings();
						Current[i].Shared.Load(Current[i]);
						NewDecals.Add(Current[i]);
						Decal.AllDecalsShared.Add(Current[i].Shared);
						nCount++;
					}*/
				}
			}

			//Debug.Log("Decal types: " + nCount);
		}

		public static bool MargeShared(Decal Current, int count = 0)
		{
			if (count <= 0 && NewDecals != null )
				count = NewDecals.Count;

			bool found = false;
			for (int n = 0; n < count; n++)
			{
				if (Current.Compare(NewDecals[n]))
				{

					Current.Shared = NewDecals[n].Shared;
					found = true;
					return false;
				}
			}
			if (!found)
			{
				Current.Shared = new Decal.DecalSharedSettings();
				Current.Shared.Load(Current);
				NewDecals.Add(Current);
				Decal.AllDecalsShared.Add(Current.Shared);
				return true;
			}
			return false;
		}

		static void CompareDecalsAndAdd(Decal ToCompare)
		{

		}

	}
}
