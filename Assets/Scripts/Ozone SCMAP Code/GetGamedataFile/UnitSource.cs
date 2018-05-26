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
	int Count;
	CullingGroup Culling;
	[System.NonSerialized]
	public HashSet<UnitInstance> Instances;
	UnitInstance[] InstancesArray;
	BoundingSphere[] SpheresArray;
	HashSet<Matrix4x4> Lod;

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
				if (Lod.Contains(InstancesArray[i].LocalToWorldMatrix))
					Lod.Remove(InstancesArray[i].LocalToWorldMatrix);
			}
			else
			{
				if (!Lod.Contains(InstancesArray[i].LocalToWorldMatrix))
					Lod.Add(InstancesArray[i].LocalToWorldMatrix);
			}
		}
		IsDirty = false;
	}

	public void ApplyLods()
	{
		if (Lod == null)
			Lod = new HashSet<Matrix4x4>();
		else
			Lod.Clear();

		if (_matrices == null || _matrices.Length != 1023)
			_matrices = new Matrix4x4[1023];
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
			Lod.Add(InstancesArray[i].LocalToWorldMatrix);
		else
			Lod.Remove(InstancesArray[i].LocalToWorldMatrix);

	}

	static Matrix4x4[] _matrices;
	const UnityEngine.Rendering.ShadowCastingMode ShadowCasting = UnityEngine.Rendering.ShadowCastingMode.On;
	public void Draw()
	{
		HashSet<Matrix4x4>.Enumerator ListEnum = Lod.GetEnumerator();
		int n = 0;

		while (ListEnum.MoveNext())
		{
			_matrices[n] = ListEnum.Current;
			n++;

			if (n == 1023)
			{
				Graphics.DrawMeshInstanced(BP.LODs[0].Mesh, 0, BP.LODs[0].Mat, _matrices, n, null, ShadowCasting);
				//Graphics.DrawMeshInstancedIndirect(SharedMesh, 0, SharedMaterial, , null, 0, null, ShadowCasting);
				n = 0;
			}
		}

		if (n > 0)
		{
			Graphics.DrawMeshInstanced(BP.LODs[0].Mesh, 0, BP.LODs[0].Mat, _matrices, n, null, ShadowCasting);
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

	public void CreateUnitObject(MapLua.SaveLua.Army.Unit Source, MapLua.SaveLua.Army.UnitsGroup Group)
	{
		Vector3 position = ScmapEditor.ScmapPosToWorld(Source.Position);
		Quaternion rotation = Quaternion.Euler(Source.Orientation);


		GameObject Obj = Instantiate(UnitsInfo.Current.UnitInstancePrefab, transform) as GameObject;
		Obj.name = Source.Name;

		UnitInstance UInst = Obj.GetComponent<UnitInstance>();
		//UInst.Owner = Owner;
		UInst.Group = Group;
		UInst.orders = Source.orders;
		UInst.platoon = Source.platoon;
		UInst.UnitRenderer = this;
		UInst.SetMatrix(ScmapEditor.SnapToTerrain(position), rotation);

		if (BP.Footprint.x > 0 && BP.Footprint.y > 0)
			UInst.Col.size = new Vector3(BP.Footprint.x * 0.1f, BP.Size.y * 0.1f, BP.Footprint.y * 0.1f);
		else
			UInst.Col.size = BP.Size * 0.1f;
		UInst.Col.center = Vector3.up * (BP.Size.y * 0.05f);

		AddInstance(UInst);

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

		if (Lod.Contains(UInst.LocalToWorldMatrix))
			Lod.Remove(UInst.LocalToWorldMatrix);
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
