using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Selection;
using FAF.MapEditor;

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
		MapLua.SaveLua.Army.Unit SelectedUnitSource;
		public void OnDropUnit()
		{
			if (ResourceBrowser.DragedObject == null || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Unit)
				return;

			SelectedUnit = UnitBrowser.GetDragUnit();
			SelectedUnitSource = new MapLua.SaveLua.Army.Unit();
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
				PlacementManager.BeginPlacement(CreationPrefab, DecalsInfo.Current.Place);
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
				Undo.Current.RegisterDecalsAdd();


			for (int i = 0; i < Positions.Length; i++)
			{
				if (RandomRotation.isOn)
				{
					if (FreeRotation.isOn)
					{
						Rotations[i] = Quaternion.Euler(Vector3.up * Random.Range(0, 360f));
					}
					else
					{
						Rotations[i] = Quaternion.Euler(Vector3.up * (90 * Random.Range(0, 4)));
					}
				}


				// TODO Create unit


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
		public void RandomizeRotation()
		{

		}

		public void SelectAllOfType()
		{

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
