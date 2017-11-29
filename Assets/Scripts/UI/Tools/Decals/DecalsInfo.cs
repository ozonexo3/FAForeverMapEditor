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


		void Awake()
		{
			Current = this;
		}

		#region Loading Assets
		public static void UnloadDecals()
		{
			Decal.AllDecalsShared = new HashSet<Decal.DecalSharedSettings>();

		}

		public static bool LoadingDecals;
		public int LoadedCount = 0;
		public IEnumerator LoadDecals()
		{
			Current = this;
			LoadingDecals = true;
			UnloadDecals();
			MargeDecals();

			List<Decal> Props = ScmapEditor.Current.map.Decals;
			//LoadedTextures = new Dictionary<string, Texture2D>();
			const int YieldStep = 500;
			int LoadCounter = YieldStep;
			int Count = Props.Count;
			//if(Count > 100)
			//Count = 100;
			LoadedCount = 0;

			Debug.Log("Decals count: " + Count);

			//Quaternion ProjectorRot = Quaternion.Euler(new Vector3(0, 0, 0)); // 90 180 180



			for (int i = 0; i < Count; i++)
			{
				if (ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_ALBEDO
					&& ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS_ALPHA
					&& ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_GLOW && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_GLOW_MASK)
					continue;

				GameObject NewDecalObject = Instantiate(DecalPrefab, DecalPivot);
				OzoneDecal Dec = NewDecalObject.GetComponent<OzoneDecal>();
				Decal Component = ScmapEditor.Current.map.Decals[i];
				Dec.Shared = Component.Shared;
				Dec.tr = NewDecalObject.transform;

				Dec.tr.localRotation = Quaternion.Euler(Component.Rotation * Mathf.Rad2Deg); // * ProjectorRot;
				Dec.tr.localScale = new Vector3(Component.Scale.x * 0.1f, Component.Scale.x * 0.1f, Component.Scale.z * 0.1f); //Dec.Component.Scale.y * 0.1f

				//Dec.tr.localPosition = ScmapEditor.ScmapPosToWorld(Dec.Component.Position);

				//Quaternion PosRotation = Quaternion.Euler(Vector3.up * Tr.eulerAngles.y);

				//Vector3 Up = Dec.tr.forward;
				//Up.y = 0;
				//Up.Normalize();
				//Vector3 right = Dec.tr.right;
				//right.y = 0;
				//right.Normalize();

				//Dec.tr.localPosition -= Up * (Dec.Component.Scale.y * 0.05f);
				//Dec.tr.localPosition += right * (Dec.Component.Scale.x * 0.05f);

				Dec.MovePivotPoint(ScmapEditor.ScmapPosToWorld(Component.Position));


				//Dec.WorldCutoffDistance = 20;
				//Dec.WorldCutoffDistance = Dec.Component.CutOffLOD * 0.1f;
				/*
				Dec.CutOffLOD = (Dec.Component.CutOffLOD - OzoneDecalRenderer.CameraNear) / OzoneDecalRenderer.CameraFar;
				Dec.CutOffLOD *= 0.1f;
				Dec.NearCutOffLOD = (Dec.Component.NearCutOffLOD) / OzoneDecalRenderer.CameraFar;
				Dec.NearCutOffLOD *= 0.1f;
				*/
				//Dec.NearCutOffLOD

#if UNITY_EDITOR
				Dec.Text0Path = Component.TexPathes[0];
				Dec.Text1Path = Component.TexPathes[1];
#endif

				LOD[] Old = Dec.lg.GetLODs();
				Old[0].screenRelativeTransitionHeight = Dec.tr.localScale.z / FrustumHeightAtDistance(Component.CutOffLOD * 0.102f);
				Dec.lg.SetLODs(Old);


				Dec.Material = Dec.Shared.SharedMaterial;

				LoadedCount++;
				LoadCounter--;
				if (LoadCounter <= 0)
				{
					LoadCounter = YieldStep;
					yield return null;
				}
			}
			DecalsControler.Sort();

			yield return null;
			LoadingDecals = false;
		}

		public static float FrustumHeightAtDistance(float distance)
		{
			return 2.0f * distance * Mathf.Tan(40 * 0.5f * Mathf.Deg2Rad);
		}

		public static void SnapDecal(OzoneDecal Dec)
		{

		}

		#endregion

		/*
		static Dictionary<string, Texture2D> LoadedTextures;

		public static Texture2D AssignTextureFromPath(ref Material mat, string property, string path)
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
					//Tex.mipMapBias = 0.6f;
					//Tex.anisoLevel = 4;
					//Tex.filterMode = FilterMode.Bilinear;
					Tex.wrapMode = TextureWrapMode.Clamp;

					LoadedTextures.Add(path, Tex);

					mat.SetTexture(property, Tex);
					return Tex;
				}
			}
			return null;
		}
		*/

		public static Texture2D AssignTextureFromPath(ref Material mat, string property, string path)
		{
			Texture2D Tex = GetGamedataFile.LoadTexture2DFromGamedata("env.scd", path);
			//Tex.mipMapBias = 0.6f;
			//Tex.anisoLevel = 4;
			//Tex.filterMode = FilterMode.Bilinear;
			//Tex.wrapMode = TextureWrapMode.Clamp;
			mat.SetTexture(property, Tex);
			return Tex;
		}

	}
}
