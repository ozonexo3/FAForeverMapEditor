using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OzoneDecals;
using Selection;

namespace EditMap
{
	public class DecalsList : MonoBehaviour
	{
		public GameObject TypePrefab;
		public GameObject GroupPrefab;
		public Transform ListPrefab;

		int Page = 0;

		private void OnEnable()
		{
			GenerateTypes();
		}

		public void SwitchToTypes()
		{


		}

		public void SwitchToGroups()
		{

		}


		void Clean()
		{
			for (int i = 0; i < AllObjects.Count; i++)
				Destroy(AllObjects[i].gameObject);
			AllObjects = new List<GameObject>();
			AllListObjects = new HashSet<ListObjectDecal>();
		}

		List<GameObject> AllObjects = new List<GameObject>();
		HashSet<ListObjectDecal> AllListObjects = new HashSet<ListObjectDecal>();
		public void GenerateTypes()
		{
			Clean();

			Page = 0;

			HashSet<Decal.DecalSharedSettings>.Enumerator ListEnum = Decal.AllDecalsShared.GetEnumerator();
			int indstanceId = 0;

			while (ListEnum.MoveNext())
			{
				Decal.DecalSharedSettings Current = ListEnum.Current;

				if (Current != null)
				{

					GameObject NewListObject = Instantiate(TypePrefab, ListPrefab) as GameObject;
					ListObjectDecal lo = NewListObject.GetComponent<ListObjectDecal>();
					AllListObjects.Add(lo);

					NewListObject.transform.SetSiblingIndex((int)Current.Type);

					lo.InstanceId = indstanceId;
					lo.ListId = 0;
					lo.Setting = Current;
					lo.ClickActionId = OnClickType;
					lo.DragAction = OnDropObject;
					lo.ObjectName.text = ((TerrainDecalTypeString)((int)Current.Type)).ToString().Replace("_", " ") + "\n" + Current.Tex1Path;
					lo.Image.texture = Current.Texture1;

					AllObjects.Add(NewListObject);
					indstanceId++;

					UpdateSelection();
				}
			}
		}

		public void OnTexturesChanged()
		{
			if (Page == 0)
			{
				for(int i = 0; i < AllObjects.Count; i++)
				{
					ListObjectDecal lo = AllObjects[i].GetComponent<ListObjectDecal>();
					lo.ObjectName.text = ((TerrainDecalTypeString)((int)lo.Setting.Type)).ToString().Replace("_", " ") + "\n" + lo.Setting.Tex1Path;
					lo.Image.texture = lo.Setting.Texture1;
				}
			}
		}

		public void UpdateSelection()
		{
			if (Page == 0)
			{
				int SelectedCount = SelectionManager.Current.Selection.Ids.Count;

				HashSet<Decal.DecalSharedSettings> SelectedShared = new HashSet<Decal.DecalSharedSettings>();
				for (int i = 0; i < SelectedCount; i++)
				{
					SelectedShared.Add(SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<OzoneDecal>().Shared);
				}

				var ListEnum = AllListObjects.GetEnumerator();
				while (ListEnum.MoveNext())
				{
					if (SelectedShared.Contains(ListEnum.Current.Setting))
						ListEnum.Current.Select();
					else
						ListEnum.Current.Unselect();

				}
			}
		}

		public void OnClickType(ListObject ob)
		{
			Decal.DecalSharedSettings dss = AllObjects[ob.InstanceId].GetComponent<ListObjectDecal>().Setting;
			DecalsInfo.Current.DecalSettingsUi.Load(dss);

			/*
			SelectionManager.Current.CleanSelection();

			for(int i = 0; i < SelectionManager.Current.AffectedGameObjects.Length; i++)
			{
				if (SelectionManager.Current.AffectedGameObjects[i].GetComponent<OzoneDecal>().Component.Shared == dss)
					SelectionManager.Current.SelectObjectAdd(SelectionManager.Current.AffectedGameObjects[i]);
			}
			*/
		}

		public void OnDropObject(ListObject ob)
		{

		}

		public void OnClickGroup(ListObject ob)
		{
			Decal.DecalSharedSettings dss = AllObjects[ob.InstanceId].GetComponent<ListObjectDecal>().Setting;

			SelectionManager.Current.CleanSelection();

			for (int i = 0; i < SelectionManager.Current.AffectedGameObjects.Length; i++)
			{
				if (SelectionManager.Current.AffectedGameObjects[i].GetComponent<OzoneDecal>().Shared == dss)
					SelectionManager.Current.SelectObjectAdd(SelectionManager.Current.AffectedGameObjects[i]);
			}
		}

	}
}
