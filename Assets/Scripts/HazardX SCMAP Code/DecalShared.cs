//********************************
//
// Custom decal settins shared between multiple decals for FAF Map Editor 
// * Copyright ozonexo3 2017
//
//********************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditMap;
using OzoneDecals;

public partial class Decal {

	public static HashSet<DecalSharedSettings> AllDecalsShared = new HashSet<DecalSharedSettings>();

	DecalSharedSettings _Shared;
	public DecalSharedSettings Shared
	{
		get
		{
			return _Shared;
		}
		set
		{
			if (_Shared != null)
				_Shared.OnVisibilityChanged -= UpdateVisibility;
			_Shared = value;
			if(_Shared != null)
				_Shared.OnVisibilityChanged += UpdateVisibility;
			UpdateVisibility();
		}
	}


	OzoneDecal _Obj;
	public OzoneDecal Obj
	{
		get
		{
			return _Obj;
		}
		set
		{
			_Obj = value;
		}
	}

	public void UpdateVisibility()
	{
		if (_Obj)
		{
			_Obj.gameObject.SetActive(!Shared.Hidden);
		}
	}

	[System.Serializable, PreferBinarySerialization]
	public class DecalSharedSettings
	{
		public TerrainDecalType Type;
		public Texture2D Texture1;
		public string Tex1Path;
		public Texture2D Texture2;
		public string Tex2Path;

		public Material SharedMaterial;

		public bool DrawAlbedo;
		public bool DrawNormal;

		bool _Hidden;

		public void FixPaths()
		{
			if (!Tex1Path.StartsWith("/"))
			{
				Tex1Path = Tex1Path.Replace("env", "/env");
			}

			if (!Tex2Path.StartsWith("/"))
			{
				Tex2Path = Tex2Path.Replace("env", "/env");
			}

		}

		public bool Hidden
		{
			get
			{
				return _Hidden;
			}
			set
			{
				_Hidden = value;
				OnVisibilityChanged?.Invoke();
			}

		}

		public delegate void VisibilityChanged();
		public event VisibilityChanged OnVisibilityChanged;

		public DecalSharedSettings()
		{
			Type = TerrainDecalType.TYPE_ALBEDO;
			Tex1Path = "";
			Tex2Path = "";
		}

		public void Load(Decal Source)
		{
			Type = Source.Type;
			Tex1Path = GetGamedataFile.FixMapsPath(Source.TexPathes[0]);
			Tex2Path = GetGamedataFile.FixMapsPath(Source.TexPathes[1]);

			UpdateMaterial();
		}

		public void UpdateMaterial()
		{
			if (SharedMaterial == null)
				SharedMaterial = new Material(EditMap.DecalsInfo.Current.AlbedoMaterial);


			if (Type == TerrainDecalType.TYPE_NORMALS || Type == TerrainDecalType.TYPE_NORMALS_ALPHA)
			{
				Texture1 = DecalsInfo.AssignTextureFromPath(ref SharedMaterial, "_NormalTex", Tex1Path);
				Texture1.anisoLevel = 6;
				Texture1.filterMode = FilterMode.Bilinear;

				DrawAlbedo = false;
				DrawNormal = true;
			}
			else if (Type == TerrainDecalType.TYPE_GLOW
				|| Type == TerrainDecalType.TYPE_GLOW_MASK)
			{
				Texture1 = DecalsInfo.AssignTextureFromPath(ref SharedMaterial, "_MainTex", Tex1Path);
				Texture2 = DecalsInfo.AssignTextureFromPath(ref SharedMaterial, "_Glow", Tex2Path);

				DrawAlbedo = true;
				DrawNormal = false;
			}
			else // Albedo
			{
				Texture1 = DecalsInfo.AssignTextureFromPath(ref SharedMaterial, "_MainTex", Tex1Path);
				Texture2 = DecalsInfo.AssignTextureFromPath(ref SharedMaterial, "_Mask", Tex2Path);

				DrawAlbedo = true;
				DrawNormal = false;
			}

		}
	}
}
