using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WavesRenderer : MonoBehaviour
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
					new Vector3(-1, 0, -1),
					new Vector3(1, 0, -1),
					new Vector3(1, 0, 1),
					new Vector3(-1, 0, 1)
				};
				_waveMesh.normals = new Vector3[]
				{
					Vector3.up, Vector3.up, Vector3.up, Vector3.up
				};
				_waveMesh.uv = new Vector2[]
				{
					new Vector2(0, 0),
					new Vector2(1, 0),
					new Vector2(1, 1),
					new Vector2(0, 1)
				};
				_waveMesh.triangles = new int[]
				{
					0, 3, 1, 2, 1, 3
				};
			}
			return _waveMesh;
		}
	}

	public Material loadedMaterial;

	const int MAX_MEMORY_ALLOCATION = 1023 * 4;
	const int MAX_STORED_ALLOCATION = 1023;
	static readonly int SHADER_ALBEDO = Shader.PropertyToID("_MainTex");

	public class WaveRenderer{

		public Texture2D waveTexture;
		public Material waveMaterial;

		public WaveRenderer(WaveGenerator wave)
		{
			Debug.Log(wave.TextureName);

			waveTexture = GetGamedataFile.LoadTexture2DFromGamedata(GetGamedataFile.EnvScd, wave.TextureName, false, true, true);
			mpb = new MaterialPropertyBlock();

			waveMaterial = new Material(Instance.waveMaterial);
			waveMaterial.SetTexture(SHADER_ALBEDO, waveTexture);

			Instance.loadedMaterial = waveMaterial;

			instancesCount = 0;
			count = 0;

			AddInstance(wave);
		}


		int instancesCount = 0;
		Matrix4x4[] instances = new Matrix4x4[MAX_MEMORY_ALLOCATION];
		BoundingSphere[] spheres = new BoundingSphere[MAX_MEMORY_ALLOCATION];

		int count = 0;
		Matrix4x4[] stored = new Matrix4x4[MAX_STORED_ALLOCATION];
		MaterialPropertyBlock mpb;

		public void AddInstance(WaveGenerator wave)
		{
			if (instancesCount >= MAX_MEMORY_ALLOCATION)
			{
				return;
			}

			Vector3 position = ScmapEditor.ScmapPosToWorld(ScmapEditor.Current.map.WaveGenerators[instancesCount].Position);
			Quaternion rotation = Quaternion.Euler(0f, ScmapEditor.Current.map.WaveGenerators[instancesCount].Rotation * Mathf.Rad2Deg, 0f);
			Vector3 scale = Vector3.one * Mathf.Max(ScmapEditor.Current.map.WaveGenerators[instancesCount].ScaleFirst, ScmapEditor.Current.map.WaveGenerators[instancesCount].ScaleSecond) * 0.1f;

			instances[instancesCount] = Matrix4x4.TRS(position + rotation * Vector3.back * Random.Range(0.1f, 0.4f), rotation, scale);

			instancesCount++;
		}

		public void FillMatrixes()
		{
			count = 0;
			for (int i = 0; i < instancesCount && i < MAX_STORED_ALLOCATION; i++)
			{
				stored[i] = instances[i];
				count++;
				// TODO fill MPB Arrays
			}

		}

		public void Draw()
		{
			if (count > 0)
			{
				Graphics.DrawMeshInstanced(waveMesh, 0, waveMaterial, stored, count, mpb, ShadowCastingMode.Off, false, 0, null, LightProbeUsage.Off, null);
			}
		}
	}

	static Dictionary<int, WaveRenderer> renderers = new Dictionary<int, WaveRenderer>();

	void LoadWaves()
	{
		if (!MapLuaParser.IsMapLoaded)
			return;

		renderers = new Dictionary<int, WaveRenderer>();

		int wavesCount = ScmapEditor.Current.map.WaveGenerators.Count;

		for (int i = 0; i < wavesCount; i++)
		{
			int id = Shader.PropertyToID(ScmapEditor.Current.map.WaveGenerators[i].TextureName);

			if (!renderers.ContainsKey(id))
			{
				renderers.Add(id, new WaveRenderer(ScmapEditor.Current.map.WaveGenerators[i]));
			}
			else
			{
				renderers[id].AddInstance(ScmapEditor.Current.map.WaveGenerators[i]);
			}
		}
	}


	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		LoadWaves();

		foreach(var rend in renderers)
		{
			rend.Value.FillMatrixes();
		}

		Debug.Log(renderers.Count);
	}

	private void Update()
	{
		//mpb.arra

		foreach (var rend in renderers)
		{
			rend.Value.Draw();
		}
	}

}
