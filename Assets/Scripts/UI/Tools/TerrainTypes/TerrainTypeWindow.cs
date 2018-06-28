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

        [Header("Editor Settings")] [SerializeField]
        private LayerMask RayCastTerrainLayer;

        [SerializeField] private LayersSettings layersSettings;
        [SerializeField] private GameObject layerSettingsItemGO;

        [Header("Settings")] [SerializeField] private UiTextField sizeField;
        [SerializeField] private Transform layersPivot;
        [SerializeField] private ToggleGroup layersToggleGroup;

        [SerializeField] private GameObject moreInfoGO;
        [SerializeField] private RectTransform moreInfoRectTransform;
        [SerializeField] private Text indexMoreInfoText;
        [SerializeField] private Text descriptionMoreInfoText;


        private List<LayerSettingsItem> layerSettingsItems;

        private Texture2D terrainTypeTexture;

        private Texture2D TerrainTypeTexture
        {
            get
            {
                if (terrainTypeTexture == null)
                {
                    terrainTypeTexture = new Texture2D(TerrainTypeSize.x, TerrainTypeSize.y, TextureFormat.ARGB32,
                        false, false);
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

                            terrainTypeTexture.SetPixel(i, j, color);
                        }
                    }

                    terrainTypeTexture.Apply();
                }

                return terrainTypeTexture;
            }
        }

        private Texture2D brushTexture;
        private int brushTextureSize = 512;

        private Texture2D BrushTexture
        {
            get
            {
//                if (brushTexture == null)
//                {
//                    brushTexture = new Texture2D(brushTextureSize, brushTextureSize, TextureFormat.ARGB32, false,
//                        false);
//                    brushTexture.anisoLevel = 0;
//                    brushTexture.wrapMode = TextureWrapMode.Clamp;
//                    int size = brushTextureSize / 2;
//                    Color empty = new Color(0, 0, 0, 0);
//                    for (int j = -size; j < size; j++)
//                    {
//                        for (int i = -size; i < size; i++)
//                        {
//                            if (i * i + j * j > size * size)
//                            {
//                                brushTexture.SetPixel(i + size, j + size, empty);
//                            }
//                            else
//                            {
//                                brushTexture.SetPixel(i + size, j + size, Color.white);
//                            }
//                        }
//                    }
//
//                    brushTexture.Apply();
//                }

                return brushTexture;
            }
        }

        private byte oldLayerIndex;
        private Texture2D currentLayerBrush;

        private Texture2D CurrentLayerBrush
        {
            get
            {
                if (currentLayerBrush == null)
                {
                    currentLayerBrush =
                        new Texture2D(BrushTexture.width, BrushTexture.height, TextureFormat.ARGB32, false, false);
                }

                if (oldLayerIndex != currentLayer.index)
                {
                    oldLayerIndex = currentLayer.index;
                    currentLayerBrush.SetPixels(BrushTexture.GetPixels().Select(color => color * currentLayer.color)
                        .ToArray());
                    currentLayerBrush.Apply();
                }

                return currentLayerBrush;
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
                    HasHit = Physics.Raycast(TerrainRay, out terrainHit, Mathf.Infinity, RayCastTerrainLayer);
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
            get { return sizeField.value; }
        }

        private float BrushSizeRecalc
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
                Vector2 pos = new Vector2((TerrainPos2.x * 10 - BrushSizeRecalc / 4) / MapSize.x,
                    (TerrainPos2.y * 10 - BrushSizeRecalc / 4) / MapSize.y);
                return pos;
            }
        }

        private TerrainTypeLayerSettings currentLayer;

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
            if (ChangedMousePos)
            {
                UpdateBrushPos(UVPos);

                if (Input.GetMouseButton(0) && HasHit)
                {
                    Debug.LogFormat("TerrainPos2:{0}, TerrainTypeSize:{1}, BrushSize:{2}", TerrainPos2, TerrainTypeSize,
                        BrushSize);
                    Paint(TerrainPos2 * 10, BrushSize, CurrentLayerBrush);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    ApplyChanges();
                }
            }
        }

        private void Init()
        {
            sizeField.OnValueChanged.AddListener(OnSizeChanged);
            sizeField.OnEndEdit.AddListener(OnSizeChangeEnd);
//            sizeField.OnEndEdit.AddListener();
            terrainTypeSize = Vector2Int.zero;
            sizeProp = Vector2.zero;
            terrainMaterial.SetInt("_Brush", 1);
            terrainMaterial.SetTexture("_BrushTex", BrushTexture);
            terrainMaterial.SetFloat("_BrushSize", BrushSizeRecalc);
            terrainMaterial.SetTexture("_TerrainTypeAlbedo", TerrainTypeTexture);
            currentLayer = layersSettings.GetFirstLayer();

            CreateUILayerSettings();
            HideMoreLayerInfo();
            RebuildBrush(BrushSize);
        }

        private void CreateUILayerSettings()
        {
            if (layerSettingsItems != null)
            {
                return;
            }

            layerSettingsItems = new List<LayerSettingsItem>();
            foreach (TerrainTypeLayerSettings layerSettings in layersSettings)
            {
                GameObject tmpItem = Instantiate<GameObject>(layerSettingsItemGO);
                LayerSettingsItem layerSettingsItem = tmpItem.GetComponent<LayerSettingsItem>();
                tmpItem.transform.SetParent(layersPivot, false);
                layerSettingsItem.Init(layerSettings, layersToggleGroup, ShowMoreLayerInfo, HideMoreLayerInfo);
                layerSettingsItem.onActive += OnLayerChanged;

                layerSettingsItems.Add(layerSettingsItem);
            }
        }

        private void Close()
        {
            sizeField.OnValueChanged.RemoveAllListeners();
            terrainMaterial.SetInt("_Brush", 0);
            ApplyTerrainTypeChanges();
            HideMoreLayerInfo();
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
            terrainMaterial.SetFloat("_BrushSize", BrushSizeRecalc);
        }

        private void OnSizeChangeEnd()
        {
//            terrainMaterial.SetFloat("_BrushSize", BrushSizeRecalc);
            RebuildBrush(BrushSize);
        }

        private void OnLayerChanged(byte layer)
        {
            currentLayer = layersSettings[layer];
        }

        private void UpdateBrushPos(Vector2 position)
        {
            terrainMaterial.SetFloat("_BrushUvX", position.x);
            terrainMaterial.SetFloat("_BrushUvY", position.y);
//            Debug.LogFormat("Pos:{0}, Size:{1}", position, BrushSize);
        }

        private void RebuildBrush(float brushSize)
        {
//            Vector2Int brushTextureReferenceSize = new Vector2Int((int)brushSize/terrainTypeSize.x, (int)brushSize/terrainTypeSize.y);
            int size = (int) brushSize / terrainTypeSize.x * TerrainTypeTexture.width;
            brushTexture = new Texture2D(size * 2, size * 2, TextureFormat.ARGB32, false, false);
            brushTexture.anisoLevel = 0;
            brushTexture.wrapMode = TextureWrapMode.Clamp;
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

            currentLayerBrush =
                new Texture2D(BrushTexture.width, BrushTexture.height, TextureFormat.ARGB32, false, false);

            if (oldLayerIndex != currentLayer.index)
            {
                oldLayerIndex = currentLayer.index;
                currentLayerBrush.SetPixels(BrushTexture.GetPixels().Select(color => color * currentLayer.color)
                    .ToArray());
                currentLayerBrush.Apply();
            }
        }

        /// <summary>
        /// Painting on TerrainTypeLayer
        /// </summary>
        /// <param name="position">Brush position</param>
        /// <param name="brushSize"></param>
        /// <param name="layer">Layer index</param>
        private void Paint(Vector2 positionCenter, float brushSize, Texture2D layerBrush)
        {
            Rect rect = Rect.zero;
            rect.center = positionCenter;
            rect.size = new Vector2(brushSize, brushSize);

            rect.xMin = rect.xMin >= 0 ? rect.xMin : 0;
            rect.xMax = rect.xMax < TerrainTypeSize.x ? rect.xMax : 0;
            rect.yMin = rect.yMin >= 0 ? rect.yMin : 0;
            rect.xMin = rect.xMin > TerrainTypeSize.y ? rect.xMin : 0;

            Debug.LogFormat("Paint rect: {0}", rect);
            TerrainTypeTexture.SetPixels((int) rect.x, (int) rect.y, (int) rect.width, (int) rect.height,
                layerBrush.GetPixels());
        }

        private void ApplyChanges()
        {
            TerrainTypeTexture.Apply();
            ApplyTerrainTypeChanges();
        }

        private void ShowMoreLayerInfo(Rect worldRect, string index, string description)
        {
            moreInfoGO.SetActive(true);

            Vector3 min = transform.InverseTransformVector(worldRect.min);
            Vector3 max = transform.InverseTransformVector(worldRect.max);

            moreInfoRectTransform.position = min;
            moreInfoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (max - min).x);
            moreInfoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (max - min).y);

            indexMoreInfoText.text = index;
            descriptionMoreInfoText.text = description;
        }

        private void HideMoreLayerInfo()
        {
            moreInfoGO.SetActive(false);
            indexMoreInfoText.text = "null";
            descriptionMoreInfoText.text = "null";
        }
    }
}