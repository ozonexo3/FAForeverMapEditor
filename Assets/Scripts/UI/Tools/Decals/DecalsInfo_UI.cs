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
		public DecalsList DecalsList;


		public void MoveUp()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal> Sd = SelectedDecals;
			if (Sd.Count > 0)
				Undo.Current.RegisterDecalsOrderChange();

			foreach (OzoneDecal Odec in Sd)
			{
				DecalsControler.MoveUp(Odec.Dec);
			}
			DecalsControler.Sort();
		}

		public void MoveDown()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal> Sd = SelectedDecals;
			if (Sd.Count > 0)
				Undo.Current.RegisterDecalsOrderChange();
			foreach (OzoneDecal Odec in Sd)
			{
				DecalsControler.MoveDown(Odec.Dec);
			}
			DecalsControler.Sort();
		}

		public void MoveTop()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal> Sd = SelectedDecals;
			if (Sd.Count > 0)
				Undo.Current.RegisterDecalsOrderChange();
			foreach (OzoneDecal Odec in Sd)
			{
				DecalsControler.MoveTop(Odec.Dec);
			}
			DecalsControler.Sort();
		}

		public void MoveBottom()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;

			HashSet<OzoneDecal> Sd = SelectedDecals;
			if (Sd.Count > 0)
				Undo.Current.RegisterDecalsOrderChange();
			foreach (OzoneDecal Odec in Sd)
			{
				DecalsControler.MoveBottom(Odec.Dec);
			}
			DecalsControler.Sort();
		}

		public HashSet<OzoneDecal> SelectedDecals
		{
			get
			{
				HashSet<OzoneDecal> SelectedDecals = new HashSet<OzoneDecal>();
				if (SelectionManager.Current.AffectedGameObjects.Length == 0)
					return SelectedDecals;

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

		public void CreateNewDecalType()
		{
			//TODO Undo Create

			SelectionManager.Current.CleanSelection();

			Decal.DecalSharedSettings NewSharedDecal = new Decal.DecalSharedSettings();

			NewSharedDecal.UpdateMaterial();
			Decal.AllDecalsShared.Add(NewSharedDecal);

			DecalSettingsUi.Load(NewSharedDecal);
			DecalsList.GenerateTypes();

		}

		public void ToggleHideOther(Decal.DecalSharedSettings Connected)
		{
			foreach (Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				if (Shared == Connected)
					continue;
				Shared.Hidden = !Shared.Hidden;
			}
			DecalsList.UpdateSelection();
		}

		public void HideAll()
		{
			foreach(Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				Shared.Hidden = true;
			}
			DecalsList.UpdateSelection();
		}

		public void UnhideAll()
		{
			foreach (Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				Shared.Hidden = false;
			}
			DecalsList.UpdateSelection();
		}

		public void ExportSelectedType()
		{

		}

		public void ImportDecalType()
		{

		}
	}
}