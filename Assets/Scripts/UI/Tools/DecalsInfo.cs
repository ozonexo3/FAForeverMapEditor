using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace EditMap
{
	public class DecalsInfo : MonoBehaviour
	{


		public static DecalsInfo Current;

		public Transform DecalPivot;
		public GameObject DecalPrefab;
		public GameObject DecalBumpPrefab;

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

		List<string> Paths;
		List<Texture2D> Textures;

		void Awake()
		{
			Current = this;
		}

		#region Loading Assets
		public static void UnloadProps()
		{

		}

		public bool LoadingDecals;
		public int LoadedCount = 0;
		public IEnumerator LoadDecals()
		{
			LoadingDecals = true;
			UnloadProps();

			List<Decal> Props = ScmapEditor.Current.map.Decals;
			Paths = new List<string>();
			Textures = new List<Texture2D>();
			const int YieldStep = 100;
			int LoadCounter = YieldStep;
			int Count = Props.Count;
			LoadedCount = 0;

			Debug.Log("Decals count: " + Count);

			Quaternion ProjectorRot = Quaternion.Euler(new Vector3(90, 180, 180));

			for (int i = 0; i < Count; i++)
			{
				//if (ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_ALBEDO && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS)
				if (ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_ALBEDO)
					continue;

				GameObject NewDecalObject = Instantiate(DecalPrefab, DecalPivot);
				Transform Tr = NewDecalObject.transform;
				//Vector3 pos = ScmapEditor.ScmapPosToWorld(new Vector3(ScmapEditor.Current.map.Decals[i].Position.x, ScmapEditor.Current.map.Decals[i].Position.y, ScmapEditor.Current.map.Decals[i].Position.z));
				//pos = new Vector3(pos.z, pos.y, pos.x);
				Tr.localPosition = ScmapEditor.ScmapPosToWorld(ScmapEditor.Current.map.Decals[i].Position);
				Tr.localRotation = Quaternion.Euler(ScmapEditor.Current.map.Decals[i].Rotation * Mathf.Rad2Deg) * ProjectorRot;

				Quaternion PosRotation = Quaternion.Euler(Vector3.up * Tr.eulerAngles.y);

				Vector3 Up = Tr.up;
				Up.y = 0;
				Up.Normalize();
				Vector3 right = Tr.right;
				right.y = 0;
				right.Normalize();

				NewDecalObject.transform.localPosition -= Up * (ScmapEditor.Current.map.Decals[i].Scale.y * 0.05f);
				NewDecalObject.transform.localPosition += right * (ScmapEditor.Current.map.Decals[i].Scale.x * 0.05f);
				//NewDecalObject.transform.localScale = ScmapEditor.Current.map.Decals[i].Scale * 0.1f;

				//float ScaleMin = Mathf.Min(ScmapEditor.Current.map.Decals[i].Scale.x, ScmapEditor.Current.map.Decals[i].Scale.y);
				//float ScaleMax = Mathf.Min(ScmapEditor.Current.map.Decals[i].Scale.x, ScmapEditor.Current.map.Decals[i].Scale.y);

				Projector proj = NewDecalObject.GetComponent<Projector>();
				proj.orthographicSize = ScmapEditor.Current.map.Decals[i].Scale.y * 0.05f;
				proj.aspectRatio = (ScmapEditor.Current.map.Decals[i].Scale.x / ScmapEditor.Current.map.Decals[i].Scale.y);
				// ?
				proj.nearClipPlane = ScmapEditor.Current.map.Decals[i].Scale.z * -0.05f;
				proj.farClipPlane = ScmapEditor.Current.map.Decals[i].Scale.z * 0.05f;

				if (ScmapEditor.Current.map.Decals[i].Type == TerrainDecalType.TYPE_ALBEDO)
				{
					Material mat = new Material(AlbedoMaterial);

					mat.SetFloat("_CutOffLOD", ScmapEditor.Current.map.Decals[i].CutOffLOD);
					mat.SetFloat("_NearCutOffLOD", ScmapEditor.Current.map.Decals[i].NearCutOffLOD);
					AssignTextureFromPath(ref mat, "_ShadowTex", ScmapEditor.Current.map.Decals[i].TexPathes[0]);
					AssignTextureFromPath(ref mat, "_SpecularTex", ScmapEditor.Current.map.Decals[i].TexPathes[1]);


					proj.material = mat;

				}
				else if (ScmapEditor.Current.map.Decals[i].Type == TerrainDecalType.TYPE_NORMALS)
				{
					Material mat = new Material(NormalMaterial);
					proj.material = mat;
					AssignTextureFromPath(ref mat, "_BumpMap", ScmapEditor.Current.map.Decals[i].TexPathes[0]);
					mat.SetFloat("_CutOffLOD", ScmapEditor.Current.map.Decals[i].CutOffLOD * 0.1f);
					mat.SetFloat("_NearCutOffLOD", ScmapEditor.Current.map.Decals[i].NearCutOffLOD * 0.1f);
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
				int TexId = 0;

				if (Paths.Contains(path))
					TexId = Paths.IndexOf(path);
				else
				{
					TexId = Paths.Count;
					Paths.Add(path);
					Textures.Add(GetGamedataFile.LoadTexture2DFromGamedata("env.scd", path));
				}

				mat.SetTexture(property, Textures[TexId]);
			}

		}



	}
}
