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
		public GameObject TermacPrefab;

		public Material AlbedoMaterial;
		public Material NormalMaterial;

		public Text DecalTotalCount;
		public Text DecalScreenCount;

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
			const int YieldStep = 500;
			int LoadCounter = YieldStep;
			int Count = Props.Count;
			LoadedCount = 0;

			//Debug.Log("Decals count: " + Count);



			for (int i = 0; i < Count; i++)
			{
				CreateGameObjectFromDecal(ScmapEditor.Current.map.Decals[i]);

				if (ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_ALBEDO
				&& ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_NORMALS_ALPHA
				&& ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_GLOW && ScmapEditor.Current.map.Decals[i].Type != TerrainDecalType.TYPE_GLOW_MASK)
				{
					Debug.LogWarning("Found different decal type! " + ScmapEditor.Current.map.Decals[i].Type, ScmapEditor.Current.map.Decals[i].Obj.gameObject);
				}

				LoadedCount++;
				LoadCounter--;
				if (LoadCounter <= 0)
				{
					LoadCounter = YieldStep;
					yield return null;
				}
			}
			//DecalsControler.Sort();

			yield return null;
			LoadingDecals = false;
		}


		public static void CreateGameObjectFromDecal(Decal Component)
		{
			GameObject NewDecalObject = Instantiate(Component.Shared.IsTarmac ? Current.TermacPrefab : Current.DecalPrefab, Current.DecalPivot);
			OzoneDecal Obj = NewDecalObject.GetComponent<OzoneDecal>();
			Component.Obj = Obj;
			Obj.Dec = Component;
			Obj.tr = NewDecalObject.transform;

			Obj.tr.localRotation = Quaternion.Euler(Component.Rotation * Mathf.Rad2Deg);
			Obj.tr.localScale = new Vector3(Component.Scale.x * 0.1f, Component.Scale.x * 0.1f, Component.Scale.z * 0.1f);

			Obj.CutOffLOD = Component.CutOffLOD;
			Obj.NearCutOffLOD = Component.NearCutOffLOD;

			Obj.MovePivotPoint(ScmapEditor.ScmapPosToWorld(Component.Position));

			Obj.Material = Component.Shared.SharedMaterial;

			Component.Shared.OnVisibilityChanged += Component.UpdateVisibility;
			//DecalsControler.Sort();
		}

		public static float FrustumHeightAtDistance(float distance)
		{
			return 2.0f * distance * Mathf.Tan(40 * 0.5f * Mathf.Deg2Rad);
		}

		public static void SnapDecal(OzoneDecal Dec)
		{

		}

		#endregion


		public static Texture2D AssignTextureFromPath(ref Material mat, string property, string path)
		{
			Texture2D Tex = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, path, false, true, true);
			Tex.wrapMode = TextureWrapMode.Clamp;
			mat.SetTexture(property, Tex);
			return Tex;
		}

	}
}
