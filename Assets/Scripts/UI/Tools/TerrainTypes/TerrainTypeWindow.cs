using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Ozone.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EditMap.TerrainTypes
{
    public class TerrainTypeWindow : MonoBehaviour
    {
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private Camera camera;

        [Header("Editor Settings")]
        [SerializeField] private LayerMask RayCastTerrainLayer;
        [SerializeField] private LayersSettings layersSettings;

        [Header("Settings")]
        [SerializeField] private Dropdown layerSelector;
        [SerializeField] private UiTextField sizeField;


        private Texture2D terrainTypeTexture;

        private Texture2D TerrainTypeTexture
        {
            get
            {
                if (terrainTypeTexture == null)
                {
                    terrainTypeTexture = new Texture2D(TerrainTypeSize.x, TerrainTypeSize.y, TextureFormat.ARGB32, false, false);
                    for (int j = 0; j < TerrainTypeSize.y; j++)
                    {
                        for (int i = 0; i < TerrainTypeSize.x; i++)
                        {
                            TerrainTypeLayerSettings layerSettings = layersSettings[TerrainTypeData2D[i, j]];
                            Color color = Color.magenta;
                            if (layerSettings != null)
                            {
                                color = layerSettings.color;
                            }
                            terrainTypeTexture.SetPixel(i,j,color);
                        }
                    }
                    terrainTypeTexture.Apply();
                }
                return terrainTypeTexture;
            }
        }
        
        [SerializeField] private Texture2D brushTexture;
        private int brushTextureSize = 512;

        private Texture2D BrushTexture
        {
            get
            {
                if (brushTexture == null)
                {
                    brushTexture = new Texture2D(brushTextureSize, brushTextureSize, TextureFormat.ARGB32, false,
                        false);
                    brushTexture.anisoLevel = 0;
                    brushTexture.wrapMode = TextureWrapMode.Clamp;
                    int size = brushTextureSize / 2;
                    Color empty = new Color(0, 0, 0, 0);
                    for (int j = -size; j < size; j++)
                    {
                        for (int i = -size; i < size; i++)
                        {
                            if (i * i + j * j > size * size)
                            {
                                brushTexture.SetPixel(i + size, j + size, empty);
                            }
                            else
                            {
                                brushTexture.SetPixel(i + size, j + size, Color.white);
                            }
                        }
                    }

                    brushTexture.Apply();
                }

                return brushTexture;
            }
        }

        private Map Map
        {
            get { return ScmapEditor.Current.map; }
        }

        private byte[] TerrainTypeData
        {
            get { return Map.TerrainTypeData; }
        }

        private byte[,] terrainTypeData2D;

        private byte[,] TerrainTypeData2D
        {
            get
            {
                if (terrainTypeData2D == null)
                {
                    terrainTypeData2D = new byte[TerrainTypeSize.x, TerrainTypeSize.y];
                    for (var j = 0; j < TerrainTypeSize.y; j++)
                    {
                        for (var i = 0; i < TerrainTypeSize.x; i++)
                        {
                            terrainTypeData2D[i, j] = TerrainTypeData[(j * TerrainTypeSize.x) + i];
                        }
                    }
                }

                return terrainTypeData2D;
            }
        }

        private Vector2Int terrainTypeSize;

        private Vector2Int TerrainTypeSize
        {
            get
            {
                if (terrainTypeSize == Vector2Int.zero)
                {
                    terrainTypeSize = new Vector2Int(Map.Width, Map.Height);
                }

                return terrainTypeSize;
            }
        }

        private Vector2 mapSize;

        private Vector2 MapSize
        {
            get
            {
                if (mapSize == Vector2.zero)
                {
                    mapSize = new Vector2(MapLuaParser.GetMapSizeX(), MapLuaParser.GetMapSizeY());
                }

                return mapSize;
            }
        }

        private Vector2 sizeProp;

        private Vector2 SizeProp
        {
            get
            {
                if (sizeProp == Vector2.zero)
                {
                    sizeProp = MapSize / 512f;
                }

                return sizeProp;
            }
        }

        private Vector3 mousePos;

        private Vector3 MousePos
        {
            get
            {
                if (ChangedMousePos)
                {
                    mousePos = Input.mousePosition;
                }

                return mousePos;
            }
        }

        private bool ChangedMousePos
        {
            get
            {
                bool changed = mousePos != Input.mousePosition;
                return changed;
            }
        }

        private Ray terrainRay;

        private Ray TerrainRay
        {
            get
            {
                if (ChangedMousePos)
                {
                    terrainRay = camera.ScreenPointToRay(MousePos);
                }

                return terrainRay;
            }
        }

        private bool ChangedTerrainRay
        {
            get { return ChangedMousePos; }
        }

        private RaycastHit terrainHit;
        private bool HasHit;

        private RaycastHit TerrainHit
        {
            get
            {
                if (ChangedTerrainRay)
                {
                    Physics.Raycast(TerrainRay, out terrainHit, Mathf.Infinity, RayCastTerrainLayer);
                    HasHit = true;
                }

                return terrainHit;
            }
        }

        private Vector3 hitPos = Vector3.zero;

        private Vector3 HitPos
        {
            get
            {
                if (ChangedTerrainRay)
                {
                    hitPos = TerrainHit.point;
                }

                return hitPos;
            }
        }

        private bool ChangedTerrainHit
        {
            get { return ChangedTerrainRay; }
        }

        private Vector2 terrainPos2;

        private Vector2 TerrainPos2
        {
            get
            {
                if (ChangedTerrainHit)
                {
                    terrainPos2 = new Vector2(HitPos.x, MapSize.y / 10 + HitPos.z);
                }

                return terrainPos2;
            }
        }

        private Vector3 TerrainPos3
        {
            get { return hitPos; }
        }

        private float BrushSize
        {
            get { return sizeField.value / ((SizeProp.x + SizeProp.y) / 2f); }
        }

        private Vector2 BrushUVSize
        {
            get
            {
                return new Vector2((int) ((sizeField.value / SizeProp.x) * 100) * 0.01f,
                    (int) ((sizeField.value / SizeProp.y) * 100) * 0.01f);
            }
        }

        private Vector2 UVPos
        {
            get
            {
                Vector2 pos = new Vector2((TerrainPos2.x * 10 - BrushSize / 4) / MapSize.x,
                    (TerrainPos2.y * 10 - BrushSize / 4) / MapSize.y);
                return pos;
            }
        }

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Close();
        }

        private void OnDestroy()
        {
            Close();
        }

        private void Update()
        {
            UpdateBrushPos(UVPos);
            if (Input.GetMouseButton(0) && ChangedMousePos)
            {
                Debug.LogFormat("TerrainPos2:{0}", TerrainPos2);
//                Paint(TerrainPos2, 0);
            }
        }

        private void Init()
        {
            sizeField.OnValueChanged.AddListener(OnSizeChanged);
//            sizeField.OnEndEdit.AddListener();
            terrainTypeSize = Vector2Int.zero;
            sizeProp = Vector2.zero;
            terrainMaterial.SetInt("_Brush", 1);
            terrainMaterial.SetTexture("_BrushTex", BrushTexture);
            terrainMaterial.SetFloat("_BrushSize", BrushSize);
            terrainMaterial.SetTexture("_TerrainTypeAlbedo", TerrainTypeTexture);
        }

        private void Close()
        {
            sizeField.OnValueChanged.RemoveAllListeners();
            terrainMaterial.SetInt("_Brush", 0);
            ApplyTerrainTypeChanges();
        }

        private void ApplyTerrainTypeChanges()
        {
            for (var j = 0; j < TerrainTypeSize.y; j++)
            {
                for (var i = 0; i < TerrainTypeSize.x; i++)
                {
                    TerrainTypeData[(j * TerrainTypeSize.x) + i] = TerrainTypeData2D[i, j];
                }
            }
        }

        private void OnSizeChanged()
        {
            terrainMaterial.SetFloat("_BrushSize", BrushSize);
        }

        private void UpdateBrushPos(Vector2 position)
        {
            terrainMaterial.SetFloat("_BrushUvX", position.x);
            terrainMaterial.SetFloat("_BrushUvY", position.y);
//            Debug.LogFormat("Pos:{0}, Size:{1}", position, BrushSize);
        }

        /// <summary>
        /// Painting on TerrainTypeLayer
        /// </summary>
        /// <param name="position">Brush position</param>
        /// <param name="brushSize"></param>
        /// <param name="layer">Layer index</param>
        private void Paint(Vector2 position, float brushSize, byte layer)
        {
        }

        private void PaintAtTexture(Vector2 positionCenter, float brushSize, Color color)
        {
        }
    }
}