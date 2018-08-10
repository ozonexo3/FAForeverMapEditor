using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Selection;
using FAF.MapEditor;
using MapLua;

namespace EditMap
{
	public partial class UnitsInfo : MonoBehaviour
	{
		[Header("Units")]
		public GameObject CreationPrefab;

		[Header("Units UI")]
		public Text UnitTitle;
		public Text UnitDesc;
		public RawImage UnitBg;
		public RawImage UnitIcon;
		public GameObject CreateSelected;
		public Toggle FreeRotation;
		public Toggle RandomRotation;

		GetGamedataFile.UnitDB SelectedUnit;
		SaveLua.Army.Unit SelectedUnitSource;
		public void OnDropUnit()
		{
			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Unit)
				return;

			SelectedUnit = UnitBrowser.GetDragUnit();
			SelectedUnitSource = new SaveLua.Army.Unit();
			SelectedUnitSource.Name = SelectedUnit.CodeName;
			SelectedUnitSource.type = SelectedUnit.CodeName;

			UnitTitle.text = SelectedUnit.CodeName;
			UnitDesc.text = UnitBrowser.SortDescription(SelectedUnit);

			UnitBg.texture = UnitBrowser.IconBackgrounds[SelectedUnit.Icon];
			UnitIcon.texture = UnitBrowser.GetDragUnitIcon();
			UnitIcon.enabled = UnitIcon.texture != Texture2D.whiteTexture;
			ResourceBrowser.ClearDrag();
		}

		public void DestroyUnits(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			int Count = MarkerObjects.Count;

			if (RegisterUndo && MarkerObjects.Count > 0)
			{
				List<SaveLua.Army.UnitsGroup> AllGroups = new List<SaveLua.Army.UnitsGroup>();

				for (int i = 0; i < Count; i++)
				{
					UnitInstance uinst = MarkerObjects[i].GetComponent<UnitInstance>();
					if (!AllGroups.Contains(uinst.Owner.Parent))
						AllGroups.Add(uinst.Owner.Parent);
				}
				Undo.RegisterUndo(new UndoHistory.HistoryUnitsRemove(), new UndoHistory.HistoryUnitsRemove.UnitsRemoveParam(AllGroups.ToArray()));
			}

			for (int i = 0; i < Count; i++)
			{
				//DestroyImmediate(MarkerObjects[i]);
				SaveLua.Army.Unit u = MarkerObjects[i].GetComponent<UnitInstance>().Owner;
				u.ClearInstance();
				if(u.Parent != null)
					u.Parent.RemoveUnit(u);
			}

			SelectionManager.Current.CleanSelection();
			GoToSelection();
		}

		public void SelectUnit()
		{
			if(SelectionManager.Current.Selection.Ids.Count <= 0)
			{
				
			}
			else
			{
				UnitInstance Uinst = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[0]].GetComponent<UnitInstance>();

				if (Uinst != null) {
					UnitName.SetValue(Uinst.Owner.Name);
					SelectedUnitsGroup.text = Uinst.Owner.Parent.Name + " (" + Uinst.Owner.Parent.Owner.Name + ")";
					return;
				}
			}


			// Default
			UnitName.SetValue("");
			SelectedUnitsGroup.text = "";
		}

		public void OnNameChanged()
		{
			if (SelectionManager.Current.Selection.Ids.Count < 0)
				return;

			string SourceName = UnitName.text;

			if (string.IsNullOrEmpty(SourceName) || string.IsNullOrWhiteSpace(SourceName))
			{
				SelectUnit();
				return; // EmptyName
			}

			SourceName = SourceName.Replace(" ", "_");

			//TODO Register undo

			for (int i = 0; i < SelectionManager.Current.Selection.Ids.Count; i++)
			{
				UnitInstance Uinst = SelectionManager.Current.AffectedGameObjects[SelectionManager.Current.Selection.Ids[i]].GetComponent<UnitInstance>();

				if (Uinst != null)
				{
					string NewName = SourceName;

					if (SaveLua.Army.Unit.NameExist(NewName))
					{
						string[] Split = NewName.Split('_');
						int OutInt = 0;
						if(Split.Length > 1 && int.TryParse(Split[Split.Length - 1], out OutInt)){

							NewName = "";
							for (int s = 0; s < Split.Length - 1; s++)
							{
								if (s > 0)
									NewName += "_";
								NewName += Split[s];
							}
						}

						NewName = SaveLua.Army.Unit.GetFreeName(NewName + "_");
					}

					SaveLua.Army.Unit.ReplaceName(Uinst.Owner.Name, NewName);
					Uinst.Owner.Name = NewName;

					SelectUnit();
				}
			}
		}

		bool UpdateSelectedMatrixes = false;
		public void SnapAction(Transform tr, GameObject Connected)
		{
			UpdateSelectedMatrixes = true;
			if (Connected == null)
				tr.localPosition = ScmapEditor.SnapToTerrain(tr.localPosition);
			else
				tr.localPosition = Connected.GetComponent<UnitInstance>().GetSnapPosition(tr.localPosition);
		}

		#region Placement
		bool Creating = false;
		public bool IsCreating
		{
			get
			{
				return Creating;
			}
		}
		public void SwitchCreate()
		{
			if (FirstSelected == null)
			{
				ShowGroupError();
				ForceExitCreate();
			}
			else if (string.IsNullOrEmpty(SelectedUnit.CodeName))
			{
				ShowUnitError();
				ForceExitCreate();
			}
			else
			{
				OnClickCreate(!Creating);
			}
		}

		public void ForceExitCreate()
		{
			if (Creating)
				OnClickCreate(false);
		}

		public void OnClickCreate(bool Value)
		{
			Creating = Value;
			CreateSelected.SetActive(Creating);
			if (Creating)
			{
				Selection.SelectionManager.Current.ClearAffectedGameObjects(false);
				PlacementManager.InstantiateAction = CreatePrefabAction;
				PlacementManager.MinRotAngle = FreeRotation.isOn ? (0) : (90);
				PlacementManager.SnapToWater = false;
				PlacementManager.BeginPlacement(CreationPrefab, Place);
			}
			else
			{
				GoToSelection();
			}
		}

		void ShowGroupError()
		{
			if (!GenericInfoPopup.HasAnyInfo())
				GenericInfoPopup.ShowInfo("No unit group selected!");
		}

		void ShowUnitError()
		{
			if (!GenericInfoPopup.HasAnyInfo())
				GenericInfoPopup.ShowInfo("No unit selected!");
		}

		public void OnChangeFreeRotation()
		{
			int Angle = FreeRotation.isOn ? (0) : (90);
			SelectionManager.Current.ChangeMinAngle(Angle);
			PlacementManager.MinRotAngle = Angle;
		}

		void CreatePrefabAction(GameObject InstancedPrefab)
		{
			UnitSource us = GetGamedataFile.LoadUnit(SelectedUnit.CodeName);
			SelectedUnitSource.Parent = FirstSelected.Source;
			SelectedUnitSource.Owner = FirstSelected.Owner;
			us.FillGameObjectValues(InstancedPrefab, SelectedUnitSource, FirstSelected.Source, Vector3.zero, Quaternion.identity);
		}

		public void DropAtGameplay()
		{
			if (FirstSelected == null)
			{
				ShowGroupError();
				return;
			}

			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Unit)
				return;

			OnDropUnit();

			Vector3 MouseWorldPos = CameraControler.BufforedGameplayCursorPos;
			PlacementManager.PlaceAtPosition(MouseWorldPos, CreationPrefab, Place);

			GoToSelection();
		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales)
		{
			Place(Positions, Rotations, Scales, true);
		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales, bool RegisterUndo)
		{
			if (FirstSelected == null)
			{
				ShowGroupError();
				return;

			}

			if (Positions.Length > 0 && RegisterUndo)
			{
				Undo.RegisterUndo(new UndoHistory.HistoryUnitsRemove(), new UndoHistory.HistoryUnitsRemove.UnitsRemoveParam(new SaveLua.Army.UnitsGroup[] { FirstSelected.Source }));
			}


			for (int i = 0; i < Positions.Length; i++)
			{
				if (RandomRotation.isOn)
				{
					Rotations[i] = GetRandomRotation;
				}

				SaveLua.Army.Unit NewUnit = new SaveLua.Army.Unit();
				NewUnit.Name = SaveLua.Army.Unit.GetFreeName("UNIT_");
				NewUnit.type = SelectedUnit.CodeName;
				NewUnit.orders = "";
				NewUnit.platoon = "";
				NewUnit.Position = ScmapEditor.WorldPosToScmap(Positions[i]);
				NewUnit.Orientation = Rotations[i].eulerAngles * Mathf.Deg2Rad;

				FirstSelected.Source.AddUnit(NewUnit);
				NewUnit.Instantiate();

				FirstSelected.Refresh();
			}
		}
		#endregion

		#region Parrenting
		void ReparrentUnits()
		{
			if (FirstSelected == null)
				return;

			SelectionManager.DoForEverySelected(ReparrentUnit, true);
		}

		public static void ReparrentUnit(GameObject obj, int Type)
		{
			if (obj == null || FirstSelected == null)
				return;

			UnitInstance uinst = obj.GetComponent<UnitInstance>();

			if (uinst == null)
				return;

			uinst.Owner.Parent.RemoveUnit(uinst.Owner);
			FirstSelected.Source.AddUnit(uinst.Owner);
			uinst.ArmyColor = uinst.Owner.Parent.Owner.ArmyColor;
		}

		#endregion


		#region Tools
		Quaternion GetRandomRotation
		{
			get
			{
				if (FreeRotation.isOn)
				{
					return Quaternion.Euler(Vector3.up * Random.Range(0, 360f));
				}
				else
				{
					return Quaternion.Euler(Vector3.up * (90 * Random.Range(0, 4)));
				}
			}
		}

		public void RandomizeRotation()
		{
			if (IsCreating)
				return;

			SelectionManager.DoForEverySelected(RandomizeTransform);
			SelectionManager.Current.FinishSelectionChange();
		}

		void RandomizeTransform(GameObject obj, int Type)
		{
			obj.transform.rotation = GetRandomRotation;
			obj.SendMessage("UpdateMatrixTranslated");
		}


		HashSet<UnitSource> GatheredUnitTypes = new HashSet<UnitSource>();
		void GatherUnitTypes(GameObject obj, int Type)
		{
			GatheredUnitTypes.Add(obj.GetComponent<UnitInstance>().UnitRenderer);
		}

		public void SelectAllOfType()
		{
			GatheredUnitTypes.Clear();
			SelectionManager.DoForEverySelected(GatherUnitTypes);

			SelectionManager.Current.CleanSelection();

			List<GameObject> NewSelection = new List<GameObject>();
			for(int i = 0; i < SelectionManager.Current.AffectedGameObjects.Length; i++)
			{
				if (GatheredUnitTypes.Contains(SelectionManager.Current.AffectedGameObjects[i].GetComponent<UnitInstance>().UnitRenderer))
					NewSelection.Add(SelectionManager.Current.AffectedGameObjects[i]);
			}
			SelectionManager.Current.SelectObjects(NewSelection.ToArray());

			NewSelection.Clear();
			GatheredUnitTypes.Clear();
		}

		public void ReplaceSelected()
		{
			if(FirstSelected == null)
			{
				ShowGroupError();

				return;
			}

			if (string.IsNullOrEmpty(SelectedUnit.CodeName))
			{
				ShowUnitError();
				return;
			}

			List<SaveLua.Army.UnitsGroup> AllGroups = new List<SaveLua.Army.UnitsGroup>();
			List<GameObject> AllObjects = SelectionManager.GetAllSelectedGameobjects();
			int Count = AllObjects.Count;

			if (Count == 0)
				return;

			for (int i = 0; i < Count; i++)
			{
				UnitInstance uinst = AllObjects[i].GetComponent<UnitInstance>();
				if (!AllGroups.Contains(uinst.Owner.Parent))
					AllGroups.Add(uinst.Owner.Parent);
			}
			Undo.RegisterUndo(new UndoHistory.HistoryUnitsRemove(), new UndoHistory.HistoryUnitsRemove.UnitsRemoveParam(AllGroups.ToArray()));


			Vector3[] Positions = new Vector3[Count];
			Quaternion[] Rotations = new Quaternion[Count];
			Vector3[] Scales = new Vector3[Count];

			for (int i = 0; i < Count; i++)
			{
				UnitInstance uinst = AllObjects[i].GetComponent<UnitInstance>();

				Positions[i] = uinst.transform.position;
				Rotations[i] = uinst.transform.rotation;
				Scales[i] = uinst.transform.localScale;

				SaveLua.Army.Unit u = uinst.Owner;

				u.ClearInstance();
				u.Parent.RemoveUnit(u);
			}

			Place(Positions, Rotations, Scales, false);
			GoToSelection();

		}
		#endregion

		#region Import/Export
		public void ExportUnits()
		{

		}
		public void ImportUnits()
		{

		}
		#endregion
	}
}
