using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OzoneDecals;

namespace EditMap
{
	public class DecalSettings : MonoBehaviour
	{
		public RawImage Texture1;
		public InputField Texture1Path;
		public RawImage Texture2;
		public InputField Texture2Path;
		public InputField CutOff;
		public InputField NearCutOff;
		public Dropdown DecalType;
		public Button CreateBtn;
		public GameObject CreateSelected;

		static Decal.DecalSharedSettings Loaded;

		bool Loading = false;
		public void Load(Decal.DecalSharedSettings DecalSettings)
		{
			if (Creating)
				return;
			Loading = true;
			Loaded = DecalSettings;

			if (DecalSettings == null)
			{
				Texture1.texture = null;
				Texture1Path.text = "";

				Texture2.texture = null;
				Texture2Path.text = "";

				CutOff.text = "0";
				NearCutOff.text = "0";
				CreateBtn.interactable = false;
				CreateSelected.SetActive(false);
			}
			else
			{
				Texture1.texture = DecalSettings.Texture1;
				Texture1Path.text = DecalSettings.Tex1Path;

				Texture2.texture = DecalSettings.Texture2;
				Texture2Path.text = DecalSettings.Tex2Path;

				CutOff.text = DecalSettings.CutOffLOD.ToString();
				NearCutOff.text = DecalSettings.NearCutOffLOD.ToString();

				CreateBtn.interactable = true;

				switch (DecalSettings.Type)
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
				}
			}
			Loading = false;
		}

		public void OnValueChanged()
		{
			if (Loaded == null || Loading)
				return;

			//TODO Register Undo

			Loaded.UpdateMaterial();
		}

		public void OnTypeChanged()
		{
			if (Loaded == null || Loading)
				return;

			//TODO Register Undo

			Loaded.UpdateMaterial();
		}


		public void ClickTex1()
		{
			if(!string.IsNullOrEmpty(Texture1Path.text))
				ResourceBrowser.Current.LoadStratumTexture(Texture1Path.text.Remove(0,1));
		}

		public void ClickTex2()
		{
			if (!string.IsNullOrEmpty(Texture2Path.text))
				ResourceBrowser.Current.LoadStratumTexture(Texture2Path.text.Remove(0, 1));
		}

		public void DropTex1()
		{

			if (Loaded == null || !ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
				return;
			if (ResourceBrowser.SelectedCategory == 2)
			{
				//TODO Undo.RegisterStratumChange(Selected);
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				Loaded.Tex1Path = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
				Loaded.UpdateMaterial();
				Load(Loaded);
				DecalsInfo.Current.DecalsList.OnTexturesChanged();
				//ScmapEditor.Current.Textures[Selected].Albedo = ResourceBrowser.Current.LoadedTextures[ResourceBrowser.DragedObject.InstanceId];
				//ScmapEditor.Current.Textures[Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];

				//Map.map.Layers [Selected].PathTexture = Map.Textures [Selected].AlbedoPath;
			}
		}
		
		public void DropTex2()
		{
			if (Loaded == null || !ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
				return;
			if (ResourceBrowser.SelectedCategory == 2)
			{
				//TODO Undo.RegisterStratumChange(Selected);
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				Loaded.Tex2Path = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
				Loaded.UpdateMaterial();
				Load(Loaded);
				DecalsInfo.Current.DecalsList.OnTexturesChanged();
				//ScmapEditor.Current.Textures[Selected].Albedo = ResourceBrowser.Current.LoadedTextures[ResourceBrowser.DragedObject.InstanceId];
				//ScmapEditor.Current.Textures[Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];

				//Map.map.Layers [Selected].PathTexture = Map.Textures [Selected].AlbedoPath;
			}
		}

		public void SampleCutoffFromCamera()
		{

		}

		public void SampleNearCutoffFromCamera()
		{

		}


		bool Creating = false;
		public void OnClickCreate()
		{
			Creating = !Creating;
			CreateSelected.SetActive(Creating);
			if (Creating)
			{
				//TODO Enter Creating mode
				Selection.SelectionManager.Current.ClearAffectedGameObjects(false);
				PlacementManager.InstantiateAction = CreatePrefabAction;
				PlacementManager.MinRotAngle = 0;
				PlacementManager.BeginPlacement(GetCreationObject(), Place);
			}
			else
			{
				//TODO Exit Creating mode
				if (CreationGameObject)
					Destroy(CreationGameObject);

				DecalsInfo.Current.GoToSelection();
			}
		}

		public void Place(Vector3[] Positions, Quaternion[] Rotations)
		{
			//TODO Create Objects


		}

		public GameObject CreationPrefab;
		GameObject CreationGameObject;
		GameObject GetCreationObject()
		{
			if (!CreationGameObject)
			{
				CreationGameObject = Instantiate(CreationPrefab);
				CreationGameObject.SetActive(false);
				CreatePrefabAction(CreationGameObject);
			}
			return CreationGameObject;
		}

		void CreatePrefabAction(GameObject InstancedPrefab)
		{
			OzoneDecal od = CreationGameObject.GetComponent<OzoneDecal>();
			od.Shared = Loaded;
			od.Material = Loaded.SharedMaterial;
			LOD[] Old = od.lg.GetLODs();
			Old[0].screenRelativeTransitionHeight = od.tr.localScale.z / DecalsInfo.FrustumHeightAtDistance(od.Shared.CutOffLOD * 0.102f);
			od.lg.SetLODs(Old);
		}
	}

}