using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditMap;

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
				Graphics.DrawMeshInstanced(BP.LODs[0].Mesh, 0, BP.LODs[0].Mat, _matrices, n, Mpb, ShadowCasting);
				//Graphics.DrawMeshInstancedIndirect(SharedMesh, 0, SharedMaterial, , null, 0, null, ShadowCasting);
				n = 0;
				Mpb.Clear();
			}
		}

		if (n > 0)
		{
			Mpb.SetVectorArray("_Color", _colors);
			Mpb.SetFloatArray("_Wreckage", _wreckage);
			Graphics.DrawMeshInstanced(BP.LODs[0].Mesh, 0, BP.LODs[0].Mat, _matrices, n, Mpb, ShadowCasting);
		}
	}

	bool IsDirty = false;
	private void LateUpdate()
	{
		if (IsDirty)
			BakeInstances();

		Culling.SetDistanceReferencePoint(CameraControler.Current.Cam.transform.position);

		Draw();
	}

	void OnDisable()
	{
		Culling.Dispose();
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

		if (BP.Footprint.x > 0 && BP.Footprint.y > 0)
			UInst.Col.size = new Vector3(BP.Footprint.x * 0.1f, BP.Size.y * 0.1f, BP.Footprint.y * 0.1f);
		else
			UInst.Col.size = BP.Size * 0.1f;
		UInst.Col.center = Vector3.up * (BP.Size.y * 0.05f);

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
}
