using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OzoneDecals;
using Selection;

namespace EditMap
{
	public partial class DecalsInfo : MonoBehaviour
	{

		public DecalSettings DecalSettingsUi;


		public void MoveUp()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal>.Enumerator ListEnum = SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				DecalsControler.MoveUp(ListEnum.Current);
			}
			ListEnum.Dispose();
			DecalsControler.Sort();
		}

		public void MoveDown()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal>.Enumerator ListEnum = SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				DecalsControler.MoveDown(ListEnum.Current);
			}
			ListEnum.Dispose();
			DecalsControler.Sort();
		}

		public void MoveTop()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal>.Enumerator ListEnum = SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				DecalsControler.MoveTop(ListEnum.Current);
			}
			ListEnum.Dispose();
			DecalsControler.Sort();
		}

		public void MoveBottom()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;

			HashSet<OzoneDecal>.Enumerator ListEnum = SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				DecalsControler.MoveBottom(ListEnum.Current);
			}
			ListEnum.Dispose();
			DecalsControler.Sort();
		}

		HashSet<OzoneDecal> SelectedDecals
		{
			get
			{
				HashSet<OzoneDecal> SelectedDecals = new HashSet<OzoneDecal>();
				int count = SelectionManager.Current.Selection.Ids.Count;
				for (int i = 0; i < count; i++)
				{
					SelectedDecals.Add(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<OzoneDecal>());
				}

				for (int s = 0; s < SelectionManager.Current.SymetrySelection.Length; s++)
				{
					count = SelectionManager.Current.SymetrySelection[s].Ids.Count;
					for (int i = 0; i < count; i++)
					{
						SelectedDecals.Add(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.SymetrySelection[s].Ids[i]].GetComponent<OzoneDecal>());
					}
				}
				return SelectedDecals;
			}
		}
	}
}