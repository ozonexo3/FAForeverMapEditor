using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EditMap
{
	public partial class WavesRenderer : MonoBehaviour
	{
		public static WavesRenderer Instance;
		public Material waveMaterial;
		static Mesh _waveMesh = null;
		public static Mesh waveMesh
		{
			get
			{
				if (_waveMesh == null)
				{
					_waveMesh = new Mesh();

					_waveMesh.vertices = new Vector3[]
					{
					new Vector3(-1, 0, -1) * 0.1f,
					new Vector3(1, 0, -1) * 0.1f,
					new Vector3(1, 0, 1) * 0.1f,
					new Vector3(-1, 0, 1) * 0.1f,
					new Vector3(-1, 0, -1) * 0.1f,
					new Vector3(1, 0, -1) * 0.1f,
					new Vector3(1, 0, 1) * 0.1f,
					new Vector3(-1, 0, 1) * 0.1f
					};
					_waveMesh.normals = new Vector3[]
					{
					Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.down, Vector3.down, Vector3.down, Vector3.down
					};
					_waveMesh.uv = new Vector2[]
					{
					new Vector2(0, 1),
					new Vector2(1, 1),
					new Vector2(1, 0),
					new Vector2(0, 0),
					new Vector2(0, 1),
					new Vector2(1, 1),
					new Vector2(1, 0),
					new Vector2(0, 0)
					};
					_waveMesh.triangles = new int[]
					{
					0, 3, 1, 2, 1, 3,
					4, 7, 5, 6, 5, 7
					};
				}
				return _waveMesh;
			}
		}

		public Material loadedMaterial;

		const int MAX_MEMORY_ALLOCATION = 1023 * 10;
		const int MAX_STORED_ALLOCATION = 1023;
		static readonly int SHADER_SUPCOMTIME = Shader.PropertyToID("_SupComTime");
		static readonly int SHADER_ALBEDO = Shader.PropertyToID("_MainTex");
		static readonly int SHADER_RAMP = Shader.PropertyToID("_Ramp");
		static readonly int SHADER_VELOCITY = Shader.PropertyToID("_Velocity");
		static readonly int SHADER_FIRST = Shader.PropertyToID("_First");
		static readonly int SHADER_SECOND = Shader.PropertyToID("_Second");
		static readonly int SHADER_FRAMES = Shader.PropertyToID("_Frames");

		public class WaveRenderer
		{

			public Texture2D waveTexture;
			public Texture2D rampTexture;
			public Material waveMaterial;

			public WaveRenderer(WaveGenerator wave)
			{
				Debug.Log(wave.TextureName);

				waveTexture = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, wave.TextureName, false, true, true);
				rampTexture = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, wave.RampName, false, true, true);

				waveTexture.wrapMode = TextureWrapMode.Clamp;
				rampTexture.wrapMode = TextureWrapMode.Clamp;
				waveTexture.mipMapBias = -0.5f;

				mpb = new MaterialPropertyBlock();

				waveMaterial = new Material(Instance.waveMaterial);
				waveMaterial.SetTexture(SHADER_ALBEDO, waveTexture);
				waveMaterial.SetTexture(SHADER_RAMP, rampTexture);

				//waveMaterial.SetVector(SHADER_VELOCITY, wave.Velocity);


				Instance.loadedMaterial = waveMaterial;

				instancesCount = 0;
				count = 0;

				AddInstance(wave);
			}


			int instancesCount = 0;
			public Matrix4x4[] instances = new Matrix4x4[MAX_MEMORY_ALLOCATION];
			public Vector4[] velocity = new Vector4[MAX_MEMORY_ALLOCATION];
			public Vector4[] first = new Vector4[MAX_MEMORY_ALLOCATION];
			public Vector4[] second = new Vector4[MAX_MEMORY_ALLOCATION];
			public Vector4[] frames = new Vector4[MAX_MEMORY_ALLOCATION];
			//public float[] lifetime = new float[MAX_MEMORY_ALLOCATION];
			BoundingSphere[] spheres = new BoundingSphere[MAX_MEMORY_ALLOCATION];
			CullingGroup group;

			HashSet<int> visible = new HashSet<int>();

			int count = 0;
			bool isDirty = false;
			public Matrix4x4[] stored = new Matrix4x4[MAX_STORED_ALLOCATION];
			public Vector4[] storedVelocity = new Vector4[MAX_STORED_ALLOCATION];
			public Vector4[] storedFirst = new Vector4[MAX_STORED_ALLOCATION];
			public Vector4[] storedSecond = new Vector4[MAX_STORED_ALLOCATION];
			public Vector4[] storedFrames = new Vector4[MAX_STORED_ALLOCATION];
			MaterialPropertyBlock mpb;

			public void AddInstance(WaveGenerator wave)
			{
				if (instancesCount >= MAX_MEMORY_ALLOCATION)
				{
					return;
				}

				Vector3 position = ScmapEditor.ScmapPosToWorld(wave.Position);
				Quaternion rotation = Quaternion.Euler(0f, wave.Rotation * Mathf.Rad2Deg, 0f);
				//Vector3 scale = Vector3.one * Mathf.Max(wave.ScaleFirst, wave.ScaleSecond) * 0.1f;
				instances[instancesCount] = Matrix4x4.TRS(position, rotation, Vector3.one); //  + rotation * Vector3.back * Random.Range(0.1f, 0.4f)  Offset them for debuging
				velocity[instancesCount] = new Vector4(wave.Velocity.x * -10f, wave.Velocity.y * 10f, wave.Velocity.z * 10f);
				first[instancesCount] = new Vector4(wave.LifetimeFirst, wave.PeriodFirst, wave.ScaleFirst, wave.FrameRateFirst);
				second[instancesCount] = new Vector4(wave.LifetimeSecond, wave.PeriodSecond, wave.ScaleSecond, wave.FrameRateSecond);
				frames[instancesCount] = new Vector4(wave.FrameCount, wave.StripCount, Random.Range(0, Mathf.Max(wave.PeriodFirst, wave.ScaleSecond)), 0);
				//lifetime[instancesCount] = 0;

				spheres[instancesCount] = new BoundingSphere(position, 2f);

				instancesCount++;
			}

			public void Clear()
			{
				if(group != null)
					group.Dispose();
				instancesCount = 0;
				mpb.Clear();
				isDirty = false;
				count = 0;
				visible.Clear();
			}

			public void Init()
			{
				group = new CullingGroup();

				group.SetBoundingDistances(new float[] { 35f, 70f });
				group.SetBoundingSpheres(spheres);
				group.SetBoundingSphereCount(instancesCount);
				group.onStateChanged += OnStateChanged;

				group.targetCamera = CameraControler.Current.Cam;
				group.SetDistanceReferencePoint(CameraControler.Current.transform);

				isDirty = true;
			}

			void OnStateChanged(CullingGroupEvent evt)
			{
				if(evt.isVisible != evt.wasVisible || evt.currentDistance != evt.previousDistance)
				{
					if (evt.isVisible && evt.currentDistance == 0)
					{
						if (visible.Add(evt.index))
						{
							//if (lifetime[evt.index] < Time.time)
							//{
								frames[evt.index].w = Time.time + frames[evt.index].z;
							//}
							isDirty = true;
						}
					}
					else if(!evt.isVisible || evt.currentDistance > 1)
					{
						if (visible.Remove(evt.index)){
							isDirty = true;
							/*if (lifetime[evt.index] < Time.time)
							{
								lifetime[evt.index] = Time.time + 10;
							}*/
						}
					}
				}
			}

			public void FillMatrixes()
			{
				//mpb.Clear();
				count = 0;

				var indexesEnum = visible.GetEnumerator();
				isDirty = false;

				while (indexesEnum.MoveNext())
				{
					int i = indexesEnum.Current;

					stored[count] = instances[i];
					storedVelocity[count] = velocity[i];
					storedFirst[count] = first[i];
					storedSecond[count] = second[i];
					storedFrames[count] = frames[i];
					count++;

					if (count >= MAX_STORED_ALLOCATION)
						break;
				}
				indexesEnum.Dispose();

				/*for (int i = 0; i < instancesCount && i < MAX_STORED_ALLOCATION; i++)
				{
					stored[i] = instances[i];
					storedVelocity[i] = velocity[i];
					storedFirst[i] = first[i];
					storedSecond[i] = second[i];
					storedFrames[i] = frames[i];
					count++;
				}*/

				mpb.SetVectorArray(SHADER_VELOCITY, storedVelocity);
				mpb.SetVectorArray(SHADER_FIRST, storedFirst);
				mpb.SetVectorArray(SHADER_SECOND, storedSecond);
				mpb.SetVectorArray(SHADER_FRAMES, storedFrames);

			
			}

			public void Draw()
			{
				if (isDirty)
				{
					FillMatrixes();
				}

				if (count > 0)
				{
					Graphics.DrawMeshInstanced(waveMesh, 0, waveMaterial, stored, count, mpb, ShadowCastingMode.Off, false, 0, null, LightProbeUsage.Off, null);
				}
			}
		}

		static Dictionary<int, WaveRenderer> renderers = new Dictionary<int, WaveRenderer>();


		public static void ReloadWaves()
		{
			if (!MapLuaParser.IsMapLoaded)
				return;

			//renderers.Clear();

			foreach (var rend in renderers)
			{
				rend.Value.Clear();
			}

			int wavesCount = ScmapEditor.Current.map.WaveGenerators.Count;

			for (int i = 0; i < wavesCount; i++)
			{
				int id = ScmapEditor.Current.map.WaveGenerators[i].propertyID;

				if (!renderers.ContainsKey(id))
				{
					renderers.Add(id, new WaveRenderer(ScmapEditor.Current.map.WaveGenerators[i]));
				}
				else
				{
					renderers[id].AddInstance(ScmapEditor.Current.map.WaveGenerators[i]);
				}
			}

			foreach (var rend in renderers)
			{
				rend.Value.Init();
			}
		}

		private void Awake()
		{
			Instance = this;
			LoadWavePatterns();
		}

		public Vector3[] TestPoints;

		private void OnEnable()
		{
			ReloadWaves();

			/*foreach (var rend in renderers)
			{
				rend.Value.FillMatrixes();
			}*/

			Vector3 normal0 = Vector3.Cross((TestPoints[3] - TestPoints[0]).normalized, (TestPoints[1] - TestPoints[0]).normalized).normalized;
			Vector3 normal1 = Vector3.Cross((TestPoints[1] - TestPoints[2]).normalized, (TestPoints[3] - TestPoints[2]).normalized).normalized;

			/*Vector3 normal0 = (TestPoints[0] - TestPoints[1]).normalized;
			Vector3 normal1 = (TestPoints[1] - TestPoints[2]).normalized;
			Vector3 normal2 = (TestPoints[2] - TestPoints[3]).normalized;
			Vector3 normal3 = (TestPoints[3] - TestPoints[0]).normalized;

			if (normal0.y > 0)
				normal0 *= -1;
			if (normal1.y > 0)
				normal1 *= -1;
			if (normal2.y > 0)
				normal2 *= -1;
			if (normal3.y > 0)
				normal3 *= -1;

			Debug.Log(normal0);
			Debug.Log(normal1);
			Debug.Log(normal2);
			Debug.Log(normal3);*/

			Vector3 finalNormal = (normal0 + normal1) / 2f;

			Debug.Log(finalNormal);

			//finalNormal.x = 0;
			//finalNormal.z = 0;
			finalNormal.y = 0;

			Debug.Log("Angle0: " + Quaternion.LookRotation(normal0.normalized).eulerAngles.y);
			Debug.Log("Angle1: " + Quaternion.LookRotation(normal1.normalized).eulerAngles.y);
			Debug.Log("Angle10: " + Quaternion.LookRotation(finalNormal.normalized).eulerAngles.y);

			//Debug.Log(renderers.Count);
		}

		private void OnDisable()
		{
			foreach (var rend in renderers)
			{
				rend.Value.Clear();
			}
		}

		private void Update()
		{
			//mpb.arra

			foreach (var rend in renderers)
			{
				rend.Value.Draw();
			}

			//DrawGizmos();

			Shader.SetGlobalFloat(SHADER_SUPCOMTIME, Time.time);
		}

		private void OnDrawGizmos()
		{
			foreach (var rend in renderers)
			{
				for (int i = 0; i < rend.Value.instances.Length; i++)
				{
					Gizmos.matrix = rend.Value.instances[i];
					Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.2f, 0, 0.05f));
				}
			}
		}
	}
}