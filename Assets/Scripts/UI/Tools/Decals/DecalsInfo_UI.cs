using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OzoneDecals;
using Selection;
using FAF.MapEditor;

namespace EditMap
{
	public partial class DecalsInfo : MonoBehaviour
	{

		public DecalSettings DecalSettingsUi;
		public DecalsList DecalsList;

		public void DropAtDecals() {
			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Decal)
				return;

			if (!ResourceBrowser.IsDecal())
				return;

			string DropPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
			ResourceBrowser.ClearDrag();
			if (!DropPath.StartsWith("/"))
			{
				DropPath = DropPath.ToLower().Replace("env", "/env");
			}
			Debug.Log(DropPath);

			foreach(Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				if (Shared.Tex1Path == DropPath)
				{
					SelectionManager.Current.CleanSelection();
					DecalSettingsUi.Load(Shared);
					Debug.Log("Exist");
					return;
				}
			}

			// Decal does not exist. Create it
			SelectionManager.Current.CleanSelection();
			GoToSelection();

			Decal.DecalSharedSettings NewSharedDecal = new Decal.DecalSharedSettings();

			if(DropPath.ToLower().Contains("normal"))
				NewSharedDecal.Type = TerrainDecalType.TYPE_NORMALS;
			else
				NewSharedDecal.Type = TerrainDecalType.TYPE_ALBEDO;

			NewSharedDecal.Tex1Path = DropPath;

			NewSharedDecal.UpdateMaterial();
			Decal.AllDecalsShared.Add(NewSharedDecal);

			DecalSettingsUi.Load(NewSharedDecal);
			DecalsList.GenerateTypes();
		}

		public void DropAtGameplay()
		{
			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Decal)
				return;

			if (!ResourceBrowser.IsDecal())
				return;

			DropAtDecals();
			Decal.DecalSharedSettings Shared = DecalSettings.Loaded;
			if (Shared == null)
			{
				Debug.Log("No deca setting loaded");
				return;
			}

			Vector3 MouseWorldPos = CameraControler.BufforedGameplayCursorPos;
			PlacementManager.PlaceAtPosition(MouseWorldPos, DecalSettingsUi.CreationPrefab, Place);

			GoToSelection();
			DecalSettingsUi.Load(Shared);
		}


		public void MoveUp()
		{
			if (SelectionManager.Current.Selection.Ids.Count == 0)
				return;
			HashSet<OzoneDecal> Sd = SelectedDecals;
			if (Sd.Count > 0)
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());

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
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());
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
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());
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
				Undo.RegisterUndo(new UndoHistory.HistoryDecalsChange());
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

		public void UpdateTotalCount()
		{
			DecalTotalCount.text = DecalsControler.DecalCount.ToString();
		}

		int LastDecalScreenCount = 0;
		public void UpdateScreenCount(int count)
		{
			if (count != LastDecalScreenCount)
			{
				LastDecalScreenCount = count;
				DecalScreenCount.text = count.ToString();

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

		void HideUpdate()
		{
			if (Input.GetKeyDown(KeyCode.H) && !CameraControler.IsInputFieldFocused())
			{
				HashSet<OzoneDecal> Selected = SelectedDecals;

				if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftShift))
				{
					// Unhide
					foreach (OzoneDecal Obj in Selected)
					{
						Obj.Dec.Shared.Hidden = false;
					}

					DecalSettings.Loaded.Hidden = false;

				}
				else
				{
					// Hide
					foreach (OzoneDecal Obj in Selected)
					{
						Obj.Dec.Shared.Hidden = true;
					}

					DecalSettings.Loaded.Hidden = true;
				}

				DecalsList.UpdateSelection();
			}

		}

		public void ToggleHideOther(Decal.DecalSharedSettings Connected)
		{
			foreach (Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				if (Shared == Connected)
					continue;
				Shared.Hidden = !Shared.Hidden;
			}
			DecalsControler.Sort();
			DecalsList.UpdateSelection();
		}

		public void HideAll()
		{
			foreach(Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				Shared.Hidden = true;
			}
			//DecalsControler.Sort();
			DecalsList.UpdateSelection();
		}

		public void UnhideAll()
		{
			foreach (Decal.DecalSharedSettings Shared in Decal.AllDecalsShared)
			{
				Shared.Hidden = false;
			}
			DecalsControler.Sort();
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