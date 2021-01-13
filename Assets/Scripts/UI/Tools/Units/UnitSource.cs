using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditMap;

//[ExecuteInEditMode]
public class UnitSource : MonoBehaviour
{
	public GetGamedataFile.UnitBluePrint BP;


	//Rendering
	public float[] RenderDistances;
	const int MaxMemoryAllocation = 4096;
	const int MaxInstancingCount = 1023;
	int Count;
	CullingGroup Culling;
	[System.NonSerialized]
	public HashSet<UnitInstance> Instances;
	UnitInstance[] InstancesArray;
	BoundingSphere[] SpheresArray;
	HashSet<UnitInstance> Lod;

	public Material strategicIconMaterial;

	List<int> ForceUpdate = new List<int>();
	void BakeInstances()
	{
		Count = Instances.Count;
		Instances.CopyTo(InstancesArray);

		ForceUpdate.Clear();
		for (int i = 0; i < Count; i++)
		{
			if (InstancesArray[i].SphereModified)
			{
				ForceUpdate.Add(i);
				InstancesArray[i].SphereModified = false;
			}
			SpheresArray[i] = InstancesArray[i].Sphere;
		}
		Culling.SetBoundingSpheres(SpheresArray);
		Culling.SetBoundingSphereCount(Count);


		for (int f = 0; f < ForceUpdate.Count; f++)
		{
			int i = ForceUpdate[f];
			if (Culling.GetDistance(i) > 0)
			{
				if (Lod.Contains(InstancesArray[i]))
					Lod.Remove(InstancesArray[i]);
			}
			else
			{
				if (!Lod.Contains(InstancesArray[i]))
					Lod.Add(InstancesArray[i]);
			}
		}
		IsDirty = false;
	}

	public void ApplyLods()
	{
		if (Lod == null)
			Lod = new HashSet<UnitInstance>();
		else
			Lod.Clear();

		if (Mpb == null)
			Mpb = new MaterialPropertyBlock();
		else
			Mpb.Clear();

		if (_matrices == null || _matrices.Length != MaxInstancingCount)
		{
			_matrices = new Matrix4x4[MaxInstancingCount];
			_colors = new Vector4[MaxInstancingCount];
			_wreckage = new float[MaxInstancingCount];
		}
		if (InstancesArray == null || InstancesArray.Length != MaxMemoryAllocation)
		{
			InstancesArray = new UnitInstance[MaxMemoryAllocation];
			SpheresArray = new BoundingSphere[MaxMemoryAllocation];
		}
		if (Instances == null)
			Instances = new HashSet<UnitInstance>();
		else
			Instances.Clear();
		Count = 0;

		if (Culling == null)
		{
			Culling = new CullingGroup();
		}

		Culling.SetBoundingDistances(RenderDistances);
		Culling.onStateChanged = UpdateLods;
		Culling.targetCamera = CameraControler.Current.Cam;
		Culling.SetDistanceReferencePoint(CameraControler.Current.transform);
	}

	void UpdateLods(CullingGroupEvent evt)
	{
		//Debug.Log(evt.index + ", " + evt.previousDistance + " > " + evt.currentDistance);
		int i = evt.index;
		if (evt.currentDistance == 0)
			Lod.Add(InstancesArray[i]);
		else
			Lod.Remove(InstancesArray[i]);

	}

	static Matrix4x4[] _matrices;
	static Vector4[] _colors;
	static float[] _wreckage;
	static MaterialPropertyBlock Mpb;
	const UnityEngine.Rendering.ShadowCastingMode ShadowCasting = UnityEngine.Rendering.ShadowCastingMode.On;
	public void Draw()
	{
		if (Lod == null || Lod.Count == 0)
			return;

		if (BP.LODs[0].Mesh == null)
		{
			//Missing mesh or material, skip rendering
			Debug.LogWarning("Missing mesh for blueprint " + BP.CodeName, gameObject);
			return;
		}
		else if (BP.LODs[0].Mat == null)
		{
			Debug.LogWarning("Missing material for blueprint " + BP.CodeName, gameObject);
			return;
		}

		var ListEnum = Lod.GetEnumerator();
		int n = 0;
		Mpb.Clear();


		while (ListEnum.MoveNext())
		{
			_matrices[n] = ListEnum.Current.LocalToWorldMatrix;
			_colors[n] = ListEnum.Current.ArmyColor;
			_wreckage[n] = ListEnum.Current.IsWreckage;
			n++;

			if (n == MaxInstancingCount)
			{
				Mpb.SetVectorArray("_Color", _colors);
				Mpb.SetFloatArray("_Wreckage", _wreckage);
				Graphics.DrawMeshInstanced(BP.LODs[0].Mesh, 0, BP.LODs[0].Mat, _matrices, n, Mpb, ShadowCasting, true, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
				//Graphics.DrawMeshInstanced(UnitsInfo.Current.StrategicMesh, 0, BP.strategicMaterial, _matrices, n, Mpb, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
				//Graphics.DrawMeshInstancedIndirect(SharedMesh, 0, SharedMaterial, , null, 0, null, ShadowCasting);
				n = 0;
				Mpb.Clear();
			}
		}

		if (n > 0)
		{
			Mpb.SetVectorArray("_Color", _colors);
			Mpb.SetFloatArray("_Wreckage", _wreckage);
			Graphics.DrawMeshInstanced(BP.LODs[0].Mesh, 0, BP.LODs[0].Mat, _matrices, n, Mpb, ShadowCasting, true, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
			//Graphics.DrawMeshInstanced(UnitsInfo.Current.StrategicMesh, 0, BP.strategicMaterial, _matrices, n, Mpb, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
		}
	}

	bool IsDirty = false;
	private void LateUpdate()
	{
		if (IsDirty)
			BakeInstances();

		Draw();
		//DrawIcons();
	}

	void OnDisable()
	{
		if (Culling != null) {
			Culling.Dispose();
			Culling = null;
		}
	}

	public UnitInstance CreateUnitObject(MapLua.SaveLua.Army.Unit Source, MapLua.SaveLua.Army.UnitsGroup Group)
	{
		Vector3 position = ScmapEditor.ScmapPosToWorld(Source.Position);
		Vector3 RadianOrientation = Vector3.zero;
		Quaternion rotation = UnitInstance.RotationFromScmapRotation(Source.Orientation);

		GameObject Obj = Instantiate(UnitsInfo.Current.UnitInstancePrefab, transform) as GameObject;
		return FillGameObjectValues(Obj, Source, Group, position, rotation);
	}

	public UnitInstance FillGameObjectValues(GameObject Obj, MapLua.SaveLua.Army.Unit Source, MapLua.SaveLua.Army.UnitsGroup Group, Vector3 position, Quaternion rotation)
	{
		if(Obj.transform.childCount > 0)
		{
			foreach (Transform child in Obj.transform)
			{
				Destroy(child.gameObject);
			}
		}

		Obj.name = Source.Name;

		UnitInstance UInst = Obj.GetComponent<UnitInstance>();
		//UInst.Owner = Owner;
		UInst.Owner = Source;
		Source.Instance = UInst;
		//Group.Units.Add(Source);
		UInst.orders = Source.orders;
		UInst.platoon = Source.platoon;
		UInst.UnitRenderer = this;
		UInst.SetMatrix(UInst.GetSnapPosition(position), rotation);
		UInst.ArmyColor = Group.Owner.ArmyColor;

		if (BP.SelectionSize.x > 0 && BP.SelectionSize.y > 0)
			UInst.Col.size = BP.SelectionSize * 0.1f;
		else
			UInst.Col.size = BP.Size * 0.1f;
		UInst.Col.center = Vector3.up * (BP.Size.y * 0.05f);

		if (BP.HasTermac)
		{
			List<OzoneDecals.OzoneDecal> TarmacInstances = new List<OzoneDecals.OzoneDecal>();

			if (BP.Termac_Albedo != null) {
				DecalsInfo.CreateGameObjectFromDecal(BP.Termac_Albedo);
				BP.Termac_Albedo.Obj.tr.parent = UInst.transform;
				BP.Termac_Albedo.Obj.tr.localPosition = Vector3.zero;
				TarmacInstances.Add(BP.Termac_Albedo.Obj);
			}

			if (BP.Termac_Normal != null)
			{
				DecalsInfo.CreateGameObjectFromDecal(BP.Termac_Normal);
				BP.Termac_Normal.Obj.tr.parent = UInst.transform;
				BP.Termac_Normal.Obj.tr.localPosition = Vector3.zero;
				TarmacInstances.Add(BP.Termac_Normal.Obj);
			}

			UInst.Tarmacs = TarmacInstances.ToArray();
		}

		UInst.UpdateMatrix();

		AddInstance(UInst);
		return UInst;
	}

	public void AddInstance(UnitInstance UInst, bool ForcedLod = false)
	{
		Instances.Add(UInst);
		IsDirty = true;
	}

	public void RemoveInstance(UnitInstance UInst)
	{
		if (!Instances.Contains(UInst))
			return;

		Instances.Remove(UInst);

		if (Lod.Contains(UInst))
			Lod.Remove(UInst);
		IsDirty = true;
	}

	public void UpdateAllInstances()
	{
		var ListEnum = Instances.GetEnumerator();

		while (ListEnum.MoveNext())
		{
			ListEnum.Current.SnapToTerrain();
		}
	}

	static Camera CurrentCamera;
	public static void DrawAllIcons(Camera current)
	{
		if (current == null || GetGamedataFile.LoadedUnitObjects.Count == 0)
			return;

		CurrentCamera = current;
		Rect CameraRect = CurrentCamera.pixelRect;
		//Color gc = GUI.color;
		//GUI.color = Color.red;
		GL.PushMatrix();
		GL.LoadPixelMatrix(Mathf.RoundToInt(CameraRect.x), Mathf.RoundToInt(CameraRect.x + CameraRect.width), Mathf.RoundToInt(CameraRect.y), Mathf.RoundToInt(CameraRect.y + CameraRect.height));

		foreach (var unitSource in GetGamedataFile.LoadedUnitObjects)
		{
			unitSource.Value.DrawIcons();
		}
		GL.PopMatrix();
	}


	static readonly Color WreckageColor = new Color(0.2f, 0.2f, 0.2f, 0f);
	public void DrawIcons()
	{
		if (Lod == null || Lod.Count < 0 || BP.strategicIcon == null)
			return;

		Rect drawRect = new Rect(0, 0, BP.strategicIcon.width, BP.strategicIcon.height);
		Rect texCoord = new Rect(0, 1f, 1f, 1f);
		Vector3 worldPos;
		Vector3 worldPoint;


		var ListEnum = Instances.GetEnumerator();
		while (ListEnum.MoveNext())
		{
			worldPos = ListEnum.Current.LocalToWorldMatrix.GetColumn(3);
			worldPoint = CurrentCamera.WorldToScreenPoint(worldPos);

			drawRect.x = Mathf.RoundToInt(worldPoint.x - drawRect.width / 2);
			drawRect.y = Mathf.RoundToInt(worldPoint.y - drawRect.height / 2); 
			Graphics.DrawTexture(drawRect, BP.strategicIcon, texCoord, 0, 0, 0, 0, (ListEnum.Current.IsWreckage > 0.5f ? WreckageColor : ListEnum.Current.ArmyColor), BP.strategicMaterial, 0);//
		}
		ListEnum.Dispose();


		//GUI.color = gc;
	}
}
