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
		}



		public void DestroyUnits(List<GameObject> MarkerObjects, bool RegisterUndo = true)
		{
			if (RegisterUndo && MarkerObjects.Count > 0)
				Undo.Current.RegisterDecalsRemove();

			int Count = MarkerObjects.Count;
			for (int i = 0; i < Count; i++)
			{
				DestroyImmediate(MarkerObjects[i]);
			}

			SelectionManager.Current.CleanSelection();
			GoToSelection();
		}

		public void SelectUnit()
		{
			// ToDo Reload Selection UI info

			//DecalsList.UpdateSelection();
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
			if (FirstSelected != null)
			{
				OnClickCreate(!Creating);
			}
			else
			{
				ForceExitCreate();
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

		public void OnChangeFreeRotation()
		{
			PlacementManager.MinRotAngle = FreeRotation.isOn ? (0) : (90);
		}

		void CreatePrefabAction(GameObject InstancedPrefab)
		{
			// TODO Fill prefab with render values
			UnitSource us = GetGamedataFile.LoadUnit(SelectedUnit.CodeName);
			SelectedUnitSource.Parent = FirstSelected.Source;
			SelectedUnitSource.Owner = FirstSelected.Owner;
			UnitInstance ui = us.FillGameObjectValues(InstancedPrefab, SelectedUnitSource, FirstSelected.Source, Vector3.zero, Quaternion.identity);
		}

		public void DropAtGameplay()
		{
			if (FirstSelected == null)
				return;

			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Unit)
				return;

			OnDropUnit();

			Vector3 MouseWorldPos = CameraControler.BufforedGameplayCursorPos;
			PlacementManager.PlaceAtPosition(MouseWorldPos, CreationPrefab, Place);

			GoToSelection();

		}



		public void Place(Vector3[] Positions, Quaternion[] Rotations, Vector3[] Scales)
		{
			if (FirstSelected == null)
				return;

			if (Positions.Length > 0)
			{
				//TODO Register Undo
				//Undo.Current.RegisterDecalsAdd();
				Undo.RegisterUnitsRemove(FirstSelected.Source);
			}


			for (int i = 0; i < Positions.Length; i++)
			{
				if (RandomRotation.isOn)
				{
					Rotations[i] = GetRandomRotation;
				}


				// TODO Create unit

				SaveLua.Army.Unit NewUnit = new SaveLua.Army.Unit();
				NewUnit.Name = SaveLua.Army.Unit.GetFreeName("UNIT_");
				NewUnit.type = SelectedUnit.CodeName;
				NewUnit.orders = "";
				NewUnit.platoon = "";
				NewUnit.Position = ScmapEditor.WorldPosToScmap(Positions[i]);
				NewUnit.Orientation = Rotations[i].eulerAngles * Mathf.Deg2Rad;

				FirstSelected.Source.AddUnit(NewUnit);
				NewUnit.Instantiate();

				/*
				GameObject NewDecalObject = Instantiate(DecalPrefab, DecalPivot);
				OzoneDecal Obj = NewDecalObject.GetComponent<OzoneDecal>();
				Decal component = new Decal();
				component.Obj = Obj;
				Obj.Dec = component;
				Obj.Dec.Shared = DecalSettings.GetLoaded;
				Obj.tr = NewDecalObject.transform;

				Obj.tr.localPosition = Positions[i];
				Obj.tr.localRotation = Rotations[i];
				Obj.tr.localScale = Scales[i];

				Obj.CutOffLOD = DecalSettingsUi.CutOff.value;
				Obj.NearCutOffLOD = DecalSettingsUi.NearCutOff.value;

				Obj.Material = component.Shared.SharedMaterial;

				Obj.Bake();

				DecalsControler.AddDecal(Obj.Dec);
				*/
			}
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
		}

		void RandomizeTransform(GameObject obj, int Type)
		{
			obj.transform.rotation = GetRandomRotation;
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
