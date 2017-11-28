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

		static Decal.DecalSharedSettings Loaded;

		bool Loading = false;
		public void Load(Decal.DecalSharedSettings DecalSettings)
		{
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
			}
			else
			{
				Texture1.texture = DecalSettings.Texture1;
				Texture1Path.text = DecalSettings.Tex1Path;

				Texture2.texture = DecalSettings.Texture2;
				Texture2Path.text = DecalSettings.Tex2Path;

				CutOff.text = DecalSettings.CutOffLOD.ToString();
				NearCutOff.text = DecalSettings.NearCutOffLOD.ToString();

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
				ResourceBrowser.Current.LoadStratumTexture(Texture1Path.text);
		}

		public void ClickTex2()
		{
			if (!string.IsNullOrEmpty(Texture2Path.text))
				ResourceBrowser.Current.LoadStratumTexture(Texture2Path.text);
		}

		public void DropTex1()
		{

			if (Loaded == null || !ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
				return;
			if (ResourceBrowser.SelectedCategory == 0 || ResourceBrowser.SelectedCategory == 1)
			{
				//TODO Undo.RegisterStratumChange(Selected);
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				Loaded.Tex1Path = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
				Loaded.UpdateMaterial();
				//ScmapEditor.Current.Textures[Selected].Albedo = ResourceBrowser.Current.LoadedTextures[ResourceBrowser.DragedObject.InstanceId];
				//ScmapEditor.Current.Textures[Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];

				//Map.map.Layers [Selected].PathTexture = Map.Textures [Selected].AlbedoPath;
			}
		}
		
		public void DropTex2()
		{
			if (Loaded == null || !ResourceBrowser.Current.gameObject.activeSelf && ResourceBrowser.DragedObject)
				return;
			if (ResourceBrowser.SelectedCategory == 0 || ResourceBrowser.SelectedCategory == 1)
			{
				//TODO Undo.RegisterStratumChange(Selected);
				Debug.Log(ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId]);

				Loaded.Tex2Path = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];
				Loaded.UpdateMaterial();
				//ScmapEditor.Current.Textures[Selected].Albedo = ResourceBrowser.Current.LoadedTextures[ResourceBrowser.DragedObject.InstanceId];
				//ScmapEditor.Current.Textures[Selected].AlbedoPath = ResourceBrowser.Current.LoadedPaths[ResourceBrowser.DragedObject.InstanceId];

				//Map.map.Layers [Selected].PathTexture = Map.Textures [Selected].AlbedoPath;
			}
		}
	}

}