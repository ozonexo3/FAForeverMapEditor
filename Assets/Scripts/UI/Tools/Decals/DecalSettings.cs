using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OzoneDecals;
using Ozone.UI;
using Selection;
using FAF.MapEditor;

namespace EditMap
{
	public class DecalSettings : MonoBehaviour
	{
		public GameObject TextureObject;
		public GameObject TypeObject;

		public RawImage Texture1;
		public InputField Texture1Path;
		public RawImage Texture2;
		public InputField Texture2Path;
		public Text Texture1Name;
		public Text Texture2Name;
		public Color TextureEmptyColor;
		public Color TextureGoodColor;
		public Color TextureUnusedColor;

		public UiTextField CutOff;
		public UiTextField NearCutOff;
		public Dropdown DecalType;
		public Button CreateBtn;
		public GameObject CreateSelected;
		public Button SelectAllBtn;



		static Decal.DecalSharedSettings Loaded;

		public static Decal.DecalSharedSettings GetLoaded
		{
			get
			{
				return Loaded;
			}
		}

		bool Loading = false;
		public void Load(Decal.DecalSharedSettings DecalSettings)
		{
			if (Creating)
			{
				if (DecalSettings == null)
					return;
				Loaded = DecalSettings;
				SwitchCreation();
				return;
			}

			UpdateSelection();

			Loaded = DecalSettings;

			if (Loaded == null)
				ClearBtn.SetActive(true);
			else
				ClearBtn.SetActive(!string.IsNullOrEmpty(Loaded.Tex2Path));

			CreateBtn.interactable = Loaded != null;
			SelectAllBtn.interactable = Loaded != null;

			SetUI();
		}

		public void SwitchCreation()
		{
			PlacementManager.Clear();
			PlacementManager.SnapToWater = false;
			PlacementManager.BeginPlacement(GetCreationObject, DecalsInfo.Current.Place, false);
		}

		void SetUI()
		{
			Loading = true;

			if (Loaded == null)
			{
				Texture1.texture = null;
				Texture1Path.text = "";

				Texture2.texture = null;
				Texture2Path.text = "";

				//CutOff.text = "500";
				//NearCutOff.text = "0";

				CreateBtn.interactable = false;
				CreateSelected.SetActive(false);
				UpdateTextureNames();
			}
			else
			{
				Texture1.texture = Loaded.Texture1;
				Texture1Path.text = Loaded.Tex1Path;

				Texture2.texture = Loaded.Texture2;
				Texture2Path.text = Loaded.Tex2Path;

				//CutOff.text = DecalSettings.CutOffLOD.ToString();
				//NearCutOff.text = DecalSettings.NearCutOffLOD.ToString();

				CreateBtn.interactable = true;
				switch (Loaded.Type)
				{
					case TerrainDecalType.TYPE_ALBEDO:
						DecalType.value = 0;
						break;
					case TerrainDecalType.TYPE_NORMALS:
						DecalType.value = 1;
						break;
					case TerrainDecalType.TYPE_NORMALS_ALPHA:
						DecalType.value = 2;
						break;
					case TerrainDecalType.TYPE_GLOW:
						DecalType.value = 3;
						break;
					case TerrainDecalType.TYPE_GLOW_MASK:
						DecalType.value = 4;
						break;
					case TerrainDecalType.TYPE_WATER_MASK:
						DecalType.value = 5;
						break;
					case TerrainDecalType.TYPE_WATER_ALBEDO:
						DecalType.value = 6;
						break;
					case TerrainDecalType.TYPE_WATER_NORMALS:
						DecalType.value = 7;
						break;
					case TerrainDecalType.TYPE_FORCE_DWORD:
						DecalType.value = 8;
						break;
					case TerrainDecalType.TYPE_UNDEFINED:
						DecalType.value = 9;
						break;
				}

				UpdateTextureNames();
			}
			Loading = false;
		}

		void UpdateTextureNames()
		{
			if(Loaded == null)
			{
				Texture1Name.text = "None";
				Texture2Name.text = "None";
				TextureObject.SetActive(false);
				TypeObject.SetActive(false);
				SelectAllBtn.gameObject.SetActive(false);
				return;
			}

			TextureObject.SetActive(true);
			TypeObject.SetActive(true);
			SelectAllBtn.gameObject.SetActive(true);

			bool NeedSecound = false;
			switch (DecalType.value)
			{
				case 0:
					Texture1Name.text = "Color";
					Texture2Name.text = "Specular";
					break;
				case 1:
					Texture1Name.text = "Normal";
					Texture2Name.text = "None";
					break;
				case 2:
					Texture1Name.text = "Normal";
					Texture2Name.text = "Alpha";
					NeedSecound = true;
					break;
				case 3:
					Texture1Name.text = "Glow";
					Texture2Name.text = "None";
					break;
				case 4:
					Texture1Name.text = "Glow";
					Texture2Name.text = "Mask";
					NeedSecound = true;
					break;
				default:
					Texture1Name.text = "Color";
					Texture2Name.text = "None";
					break;
			}

			Texture1Name.color = (Loaded.Texture1 != null) ? TextureGoodColor : TextureEmptyColor;

			if (NeedSecound)
				Texture2Name.color = (Loaded.Texture2 != null) ? TextureGoodColor : TextureEmptyColor;
			else
				Texture2Name.color = TextureUnusedColor;
		}

		public void SelectAll()
		{
			if (Loaded == null)
				return;

			Decal.DecalSharedSettings Shared = Loaded;

			SelectionManager.Current.CleanSelection();

			for (int i = 0; i < SelectionManager.Current.AffectedGameObjects.Length; i++)
			{
				if (SelectionManager.Current.AffectedGameObjects[i].GetComponent<OzoneDecal>().Dec.Shared == Shared)
					SelectionManager.Current.SelectObjectAdd(SelectionManager.Current.AffectedGameObjects[i]);
			}
		}

		void UpdateSelection()
		{
			float NearCutoffValue = -1000;
			float CutoffValue = -100;
			HashSet<OzoneDecal>.Enumerator ListEnum = DecalsInfo.Current.SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				if (NearCutoffValue < 0)
					NearCutoffValue = ListEnum.Current.NearCutOffLOD;
				else
				{
					if (ListEnum.Current.NearCutOffLOD != NearCutoffValue)
					{
						// TODO Different value
					}

				}

				if (CutoffValue < 0)
					CutoffValue = ListEnum.Current.CutOffLOD;
				else
				{
					if (ListEnum.Current.CutOffLOD != CutoffValue)
					{
						// TODO Different value
					}

				}
			}

			if (NearCutoffValue < 0)
				NearCutoffValue = NearCutOff.value;

			if (CutoffValue < 0)
				CutoffValue = CutOff.value;

			NearCutOff.SetValue(NearCutoffValue);
			CutOff.SetValue(CutoffValue);
		}

		TerrainDecalType TypeByDropdown()
		{
			
			switch (DecalType.value)
			{
				case 0:
					return TerrainDecalType.TYPE_ALBEDO;
				case 1:
					return TerrainDecalType.TYPE_NORMALS;
				case 2:
					return TerrainDecalType.TYPE_NORMALS_ALPHA;
				case 3:
					return TerrainDecalType.TYPE_GLOW;
				case 4:
					return TerrainDecalType.TYPE_GLOW_MASK;
				case 5:
					return TerrainDecalType.TYPE_WATER_MASK;
				case 6:
					return TerrainDecalType.TYPE_WATER_ALBEDO;
				case 7:
					return TerrainDecalType.TYPE_WATER_NORMALS;
				case 8:
					return TerrainDecalType.TYPE_FORCE_DWORD;
				case 9:
					return TerrainDecalType.TYPE_UNDEFINED;
			}

			return TerrainDecalType.TYPE_UNDEFINED;

		}

		public void OnSelectionChanged()
		{
			CutOff.SetValue(50);
			NearCutOff.SetValue(0);
		}

		public void OnTypeChanged()
		{
			if (Loaded == null || Loading)
				return;


			if (Loaded.Type == TypeByDropdown())
				return;

			Undo.RegisterUndo(new UndoHistory.HistoryDecalsSharedValues(), new UndoHistory.HistoryDecalsSharedValues.DecalsSharedValuesHistoryParameter(DecalSettings.GetLoaded));

			Loaded.Type = TypeByDropdown();
			UpdateTextureNames();

			Loaded.UpdateMaterial();
		}


		public void ClickTex1()
		{
			if (!string.IsNullOrEmpty(Texture1Path.text))
				ResourceBrowser.Current.LoadDecalTexture(Texture1Path.text.Remove(0, 1));
			else
				ResourceBrowser.Current.LoadDecalTexture("");
		}

		public void ClickTex2()
		{
			if (!string.IsNullOrEmpty(Texture2Path.text))
				ResourceBrowser.Current.LoadDecalTexture(Texture2Path.text.Remove(0, 1));
			else
				ResourceBrowser.Current.LoadDecalTexture("");
		}

		public void DropTex1()
		{
			if (ResourceBrowser.DragedObject == null)
				return;
			if (Loaded == null || !ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Decal)
				return;

			if (ResourceBrowser.SelectedCategory == 2)
			{

				if (Loaded.Tex1Path == ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId])
					return;

				Undo.RegisterUndo(new UndoHistory.HistoryDecalsSharedValues(), new UndoHistory.HistoryDecalsSharedValues.DecalsSharedValuesHistoryParameter(DecalSettings.GetLoaded));
				Loaded.Tex1Path = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
				Loaded.UpdateMaterial();
				Load(Loaded);
				DecalsInfo.Current.DecalsList.OnTexturesChanged();
				ResourceBrowser.ClearDrag();
			}
		}
		
		public void DropTex2()
		{
			if (ResourceBrowser.DragedObject == null)
				return;
			if (Loaded == null || !ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject || ResourceBrowser.DragedObject.ContentType != ResourceObject.ContentTypes.Decal)
				return;
			if (ResourceBrowser.SelectedCategory == 2)
			{
				if (Loaded.Tex2Path == ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId])
					return;

				Undo.RegisterUndo(new UndoHistory.HistoryDecalsSharedValues(), new UndoHistory.HistoryDecalsSharedValues.DecalsSharedValuesHistoryParameter(DecalSettings.GetLoaded));
				Loaded.Tex2Path = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
				Loaded.UpdateMaterial();
				Load(Loaded);
				DecalsInfo.Current.DecalsList.OnTexturesChanged();
				ResourceBrowser.ClearDrag();
			}
		}

		public GameObject ClearBtn;
		public void Clear2()
		{
			if (string.IsNullOrEmpty(Loaded.Tex2Path))
				return;

			Loaded.Tex2Path = "";
			Loaded.UpdateMaterial();
			Load(Loaded);
			DecalsInfo.Current.DecalsList.OnTexturesChanged();
		}

		public void SetTex1Path()
		{
			if (Loaded == null)
				return;

			Texture1Path.text = Loaded.Tex1Path;
		}

		public void SetTex2Path()
		{
			if (Loaded == null)
				return;

			Texture2Path.text = Loaded.Tex2Path;
		}

		public void OnCutoffLodChanged()
		{
			if (Loaded == null || Loading)
				return;

			Undo.RegisterUndo(new UndoHistory.HistoryDecalsValues());

			HashSet<OzoneDecal>.Enumerator ListEnum = DecalsInfo.Current.SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				ListEnum.Current.CutOffLOD = CutOff.value;
			}
			ListEnum.Dispose();

			UpdateSelection();
		}

		public void OnNearCutoffLodChanged()
		{
			if (Loaded == null || Loading)
				return;

			Undo.RegisterUndo(new UndoHistory.HistoryDecalsValues());

			HashSet<OzoneDecal>.Enumerator ListEnum = DecalsInfo.Current.SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				ListEnum.Current.NearCutOffLOD = NearCutOff.value;
			}
			ListEnum.Dispose();

			UpdateSelection();
		}

		public void SampleCutoffFromCamera()
		{
			Undo.RegisterUndo(new UndoHistory.HistoryDecalsValues());

			float Dist = (int)CameraControler.GetCurrentZoom();
			HashSet<OzoneDecal>.Enumerator ListEnum = DecalsInfo.Current.SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				ListEnum.Current.CutOffLOD = Dist;
			}
			ListEnum.Dispose();
			UpdateSelection();
		}

		public void SampleNearCutoffFromCamera()
		{
			Undo.RegisterUndo(new UndoHistory.HistoryDecalsValues());

			float Dist = (int)CameraControler.GetCurrentZoom();
			HashSet<OzoneDecal>.Enumerator ListEnum = DecalsInfo.Current.SelectedDecals.GetEnumerator();
			while (ListEnum.MoveNext())
			{
				ListEnum.Current.NearCutOffLOD = Dist;
			}
			ListEnum.Dispose();
			UpdateSelection();
		}


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
			OnClickCreate(!Creating);
		}

		public void OnClickCreate(bool Value)
		{
			Creating = Value;
			CreateSelected.SetActive(Creating);
			if (Creating)
			{
				Selection.SelectionManager.Current.ClearAffectedGameObjects(false);
				PlacementManager.InstantiateAction = CreatePrefabAction;
				PlacementManager.MinRotAngle = 0;
				PlacementManager.SnapToWater = false;
				PlacementManager.BeginPlacement(GetCreationObject, DecalsInfo.Current.Place);
				DecalsInfo.Current.DecalsList.UpdateSelection();
			}
			else
			{
				DecalsInfo.Current.GoToSelection();
			}
		}



		public GameObject CreationPrefab;
		GameObject GetCreationObject
		{
			get
			{
				return CreationPrefab;
			}
		}

		void CreatePrefabAction(GameObject InstancedPrefab)
		{
			OzoneDecal Obj = InstancedPrefab.GetComponent<OzoneDecal>();
			Decal component = new Decal();
			component.Obj = Obj;
			Obj.Dec = component;
			Obj.Dec.Shared = Loaded;
			//Dec.Shared = Loaded;
			Obj.Material = Loaded.SharedMaterial;

			Obj.CutOffLOD = CutOff.value;
			Obj.NearCutOffLOD = NearCutOff.value;
			Obj.Bake();
		}
	}

}