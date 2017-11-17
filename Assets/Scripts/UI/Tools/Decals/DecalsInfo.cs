using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OzoneDecals;

namespace EditMap
{
	public partial class DecalsInfo : MonoBehaviour
	{

		public static DecalsInfo Current;

		public Transform DecalPivot;
		public GameObject DecalPrefab;

		public Material AlbedoMaterial;
		public Material NormalMaterial;

		public class PropTypeGroup
		{
			public string Blueprint = "";
			public string LoadBlueprint = "";
			public string HelpText = "";

			public Texture2D Albedo;
			public Texture2D Normal;
		}

		Dictionary<string, Texture2D> LoadedTextures;

		void Awake()
		{
			Current = this;
		}

		#region Loading Assets
		public static void UnloadDecals()
		{

		}

		public static bool LoadingDecals;
		public int LoadedCount = 0;
		public IEnumerator LoadDecals()
		{
			LoadingDecals = true;
			UnloadDecals();
			MargeDecals();

			List<Decal> Props = ScmapEditor.Current.map.Decals;
			LoadedTextures = new Dictionary<string, Texture2D>();
			const int YieldStep = 500;
			int LoadCounter = YieldStep;
			int Count = Props.Count;
			//if(Count > 100)
			//Count = 100;
			LoadedCount = 0;

			Debug.Log("Decals count: " + Count);

			Quaternion ProjectorRot = Quaternion.Euler(new Vector3(0, 0, 0)); // 90 180 180



			for (int i = 0; i < Count; i++)
			{
				if (ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_ALBEDO
					&& ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS_ALPHA
					&& ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_GLOW && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_GLOW_MASK)
					continue;

				GameObject NewDecalObject = Instantiate(DecalPrefab, DecalPivot);
				Transform Tr = NewDecalObject.transform;

				Tr.localPosition = ScmapEditor.ScmapPosToWorld(ScmapEditor.Current.map.Decals[i].Position);
				Tr.localRotation = Quaternion.Euler(ScmapEditor.Current.map.Decals[i].Rotation * Mathf.Rad2Deg) * ProjectorRot;

				//Quaternion PosRotation = Quaternion.Euler(Vector3.up * Tr.eulerAngles.y);

				Vector3 Up = Tr.forward;
				Up.y = 0;
				Up.Normalize();
				Vector3 right = Tr.right;
				right.y = 0;
				right.Normalize();

				NewDecalObject.transform.localPosition -= Up * (ScmapEditor.Current.map.Decals[i].Scale.y * 0.05f);
				NewDecalObject.transform.localPosition += right * (ScmapEditor.Current.map.Decals[i].Scale.x * 0.05f);
				NewDecalObject.transform.localScale = new Vector3(ScmapEditor.Current.map.Decals[i].Scale.x * 0.1f, 5 , ScmapEditor.Current.map.Decals[i].Scale.z * 0.1f); //ScmapEditor.Current.map.Decals[i].Scale.y * 0.1f


				OzoneDecal Dec = NewDecalObject.GetComponent<OzoneDecal>();
				Dec.WorldCutoffDistance = ScmapEditor.Current.map.Decals[i].CutOffLOD * 0.1f;
				Dec.CutOffLOD = (ScmapEditor.Current.map.Decals[i].CutOffLOD - OzoneDecalRenderer.CameraNear) / OzoneDecalRenderer.CameraFar;
				Dec.CutOffLOD *= 0.1f;
				Dec.NearCutOffLOD = (ScmapEditor.Current.map.Decals[i].NearCutOffLOD) / OzoneDecalRenderer.CameraFar;
				Dec.NearCutOffLOD *= 0.1f;
				//Dec.NearCutOffLOD

#if UNITY_EDITOR
				Dec.Text0Path = ScmapEditor.Current.map.Decals[i].TexPathes[0];
				Dec.Text1Path = ScmapEditor.Current.map.Decals[i].TexPathes[1];
#endif

				if (ScmapEditor.Current.map.Decals[i].Type == TerrainDecalType.TYPE_NORMALS)
				{
					if (ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial == null)
					{
						ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial = new Material(AlbedoMaterial);
						AssignTextureFromPath(ref ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial, "_NormalTex", ScmapEditor.Current.map.Decals[i].TexPathes[0]);
					}

					Dec.DrawAlbedo = false;
					Dec.DrawNormal = true;
					Dec.HighQualityBlending = true;

					Dec.Material = ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial;
				}
				else if(ScmapEditor.Current.map.Decals[i].Type == TerrainDecalType.TYPE_GLOW
					|| ScmapEditor.Current.map.Decals[i].Type == TerrainDecalType.TYPE_GLOW_MASK)
				{
					if (ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial == null)
					{
						ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial = new Material(AlbedoMaterial);
						AssignTextureFromPath(ref ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial, "_MainTex", ScmapEditor.Current.map.Decals[i].TexPathes[0]);
						AssignTextureFromPath(ref ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial, "_Glow", ScmapEditor.Current.map.Decals[i].TexPathes[1]);
					}

					Dec.DrawAlbedo = true;
					Dec.DrawNormal = false;
					Dec.Material = ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial;
				}
				else // Albedo
				{
					if (ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial == null)
					{
						ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial = new Material(AlbedoMaterial);
						AssignTextureFromPath(ref ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial, "_MainTex", ScmapEditor.Current.map.Decals[i].TexPathes[0]);
						AssignTextureFromPath(ref ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial, "_Mask", ScmapEditor.Current.map.Decals[i].TexPathes[1]);
					}

					Dec.DrawAlbedo = true;
					Dec.DrawNormal = false;
					Dec.Material = ScmapEditor.Current.map.Decals[i].Shared.SharedMaterial;
				}

				LoadedCount++;
				LoadCounter--;
				if (LoadCounter <= 0)
				{
					LoadCounter = YieldStep;
					yield return null;
				}
			}

			yield return null;
			LoadingDecals = false;
		}

		#endregion

		void AssignTextureFromPath(ref Material mat, string property, string path)
		{
			if (!string.IsNullOrEmpty(path))
			{

				if (LoadedTextures.ContainsKey(path))
				{
					mat.SetTexture(property, LoadedTextures[path]);
				}
				else
				{
					//Paths.Add(path);
					Texture2D Tex = GetGamedataFile.LoadTexture2DFromGamedata("env.scd", path);

					Tex.wrapMode = TextureWrapMode.Clamp;

					LoadedTextures.Add(path, Tex);

					mat.SetTexture(property, Tex);
				}

			}
		}


	}
}
