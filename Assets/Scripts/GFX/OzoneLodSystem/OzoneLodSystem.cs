using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ozone
{
	public class OzoneLodSystem : MonoBehaviour
	{
		public LodLevel[] LodLevels;

		int Count;
		int LodsCount;
		HashSet<RenderInstance> Instances;
		RenderInstance[] InstancesArray;
		BoundingSphere[] SpheresArray;
		CullingGroup Culling;

#region Classes
		struct RenderInstance
		{
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 scale;

			public Matrix4x4 LocalToWorldMatrix;

			public RenderInstance(Vector3 _positon, Quaternion _rotation, Vector3 _scale)
			{
				position = _positon;
				rotation = _rotation;
				scale = _scale;
				LocalToWorldMatrix = Matrix4x4.TRS(position, rotation, scale);
			}

			public BoundingSphere Sphere
			{
				get
				{
					return new BoundingSphere(position, Mathf.Max(scale.x, scale.y, scale.z));
				}
			}
		}

		[System.Serializable]
		public class LodLevel
		{
			public float Distance;
			public Mesh SharedMesh;
			public Material SharedMaterial;
			public HashSet<Matrix4x4> Lod;
			public UnityEngine.Rendering.ShadowCastingMode ShadowCasting = UnityEngine.Rendering.ShadowCastingMode.On;

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
						Graphics.DrawMeshInstanced(SharedMesh, 0, SharedMaterial, _matrices, n, null, ShadowCasting);
						//Graphics.DrawMeshInstancedIndirect(SharedMesh, 0, SharedMaterial, , null, 0, null, ShadowCasting);
						n = 0;
					}
				}

				if (n > 0)
				{
					Graphics.DrawMeshInstanced(SharedMesh, 0, SharedMaterial, _matrices, n, null, ShadowCasting);
				}

			}
		}
		#endregion

		const int MaxMemoryAllocation = 4096;
		
		private void Start()
		{
			ApplyLods();

			Count = MaxMemoryAllocation;
			for (int i = 0; i < Count; i++)
			{
				InstancesArray[i] = new RenderInstance(
					new Vector3(Random.Range(0f, 51f), 2.5f, Random.Range(-51f, -102f)), 
					Quaternion.Euler(Vector3.up * Random.Range(0, 360) + Vector3.right * Random.Range(-45, 45)), 
					Vector3.one * Random.Range(0.1f, 0.6f)
					);
			}
			BakeInstances();
		}

		public void Add(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			Instances.Add(new RenderInstance(position, rotation, scale));
		}


		public void ApplyLods()
		{
			LodsCount = LodLevels.Length;
			float[] RenderDistances = new float[LodsCount];
			for (int i = 0; i < LodsCount; i++)
			{
				if (LodLevels[i].Lod == null)
					LodLevels[i].Lod = new HashSet<Matrix4x4>();
				else
					LodLevels[i].Lod.Clear();

				RenderDistances[i] = LodLevels[i].Distance;
			}

			_matrices = new Matrix4x4[1023];
			InstancesArray = new RenderInstance[MaxMemoryAllocation];
			SpheresArray = new BoundingSphere[MaxMemoryAllocation];
			Instances = new HashSet<RenderInstance>();
			Count = 0;

			if (Culling == null)
			{
				Culling = new CullingGroup();
				Culling.SetBoundingDistances(RenderDistances);
				Culling.onStateChanged = UpdateLods;
				Culling.targetCamera = CameraControler.Current.Cam;
			}
		}

		void BakeInstances()
		{
			Instances.CopyTo(InstancesArray);

			for (int i = 0; i < Count; i++)
			{
				SpheresArray[i] = InstancesArray[i].Sphere;
			}
			Culling.SetBoundingSpheres(SpheresArray);
			Culling.SetBoundingSphereCount(Count);
		}

		void UpdateLods(CullingGroupEvent evt)
		{
			//Debug.Log(evt.index + ", " + evt.previousDistance + " > " + evt.currentDistance);
			int i = evt.index;
			if (i >= Count)
				return;
			if(evt.previousDistance < LodsCount)
				LodLevels[evt.previousDistance].Lod.Remove(InstancesArray[i].LocalToWorldMatrix);
			if (evt.currentDistance < LodsCount)
				LodLevels[evt.currentDistance].Lod.Add(InstancesArray[i].LocalToWorldMatrix);
		}



		static Matrix4x4[] _matrices;
		private void LateUpdate()
		{
			Culling.SetDistanceReferencePoint(CameraControler.Current.Cam.transform.position);

			for (int i = 0; i < LodLevels.Length; i++)
			{
				LodLevels[i].Draw();
			}

			//Debug.Log( Culling.GetDistance(0) + ", " + Culling.IsVisible(0));
		}

		void OnDisable()
		{
			Culling.Dispose();
		}
	}

}