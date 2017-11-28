using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OzoneDecals;

namespace EditMap
{
	public partial class DecalsInfo : MonoBehaviour
	{


		static List<Decal> NewDecals;
		public static void MargeDecals()
		{
			Decal[] Current = ScmapEditor.Current.map.Decals.ToArray();
			int OldCount = Current.Length;
			NewDecals = new List<Decal>(OldCount);
			int nCount = 0;

			for(int i = 0; i < OldCount; i++)
			{
				if(i == 0)
				{
					Current[i].Shared = new Decal.DecalSharedSettings();
					Current[i].Shared.Load(Current[i]);
					Current[i].Shared.Ids.Add(i);
					NewDecals.Add(Current[i]);
					nCount++;
				}
				else
				{
					bool found = false;
					for(int n = 0; n < nCount; n++)
					{
						if (Current[i].Compare(NewDecals[n])) {

							Current[i].Shared = NewDecals[n].Shared;
							Current[i].Shared.Ids.Add(i);
							found = true;
							break;
						}
					}
					if (!found)
					{
						Current[i].Shared = new Decal.DecalSharedSettings();
						Current[i].Shared.Load(Current[i]);
						Current[i].Shared.Ids.Add(i);
						NewDecals.Add(Current[i]);
						Decal.AllDecalsShared.Add(Current[i].Shared);
						nCount++;
					}
				}
			}

			Debug.Log("Decal instances: " + nCount);
		}

		static void CompareDecalsAndAdd(Decal ToCompare)
		{

		}

	}
}
