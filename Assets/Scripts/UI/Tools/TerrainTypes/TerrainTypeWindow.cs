using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Ozone.UI;
using UnityEngine;
using UnityEngine.UI;
using ZkovCode.Utils;
using Array = UnityScript.Lang.Array;

namespace EditMap.TerrainTypes
{
    public class TerrainTypeWindow : MonoBehaviour
    {
        private static TerrainTypeWindow instance;

        //[SerializeField] private Editing editingTool;
        //[SerializeField] private Material terrainMaterial;
        //[SerializeField] private Camera camera;

        [Header("Editor Settings")] [SerializeField]
        private LayerMask RayCastTerrainLayer;

        [SerializeField] private LayersSettings layersSettings;
        [SerializeField] private GameObject layerSettingsItemGO;

        [Header("Settings")] [SerializeField] private UiTextField sizeField;
        [SerializeField] private UiTextField layerCapacityField;
        [SerializeField] private Transform layersPivot;
        [SerializeField] private ToggleGroup layersToggleGroup;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private Button clearCurrentButton;

        [SerializeField] private GameObject moreInfoGO;
        [SerializeField] private RectTransform moreInfoRectTransform;
        [SerializeField] private Text indexMoreInfoText;
        [SerializeField] private Text descriptionMoreInfoText;

        private static TerrainTypeLayerSettings savedLayer;

        private State currentState;

        private Dictionary<byte, LayerSettingsItem> layerSettingsItems;

        private Texture2D terrainTypeTexture;

        private Texture2D TerrainTypeTexture
        {
            get
            {
                if (terrainTypeTexture == null)
                {
                    terrainTypeTexture = new Texture2D(TerrainTypeSize.x, TerrainTypeSize.y, TextureFormat.ARGB32,
                        false, false);
                    terrainTypeTexture.anisoLevel = 0;
                    terrainTypeTexture.filterMode = FilterMode.Point;
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
            set { terrainTypeTexture = value; }
        }

        private Texture2D brushTexture;
        //private int brushTextureSize = 512;

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


        private Map Map
        {
            get { return ScmapEditor.Current.map; }
        }

        private byte[] TerrainTypeData
        {
            get { return Map.TerrainTypeData; }
            set { Map.TerrainTypeData = value; }
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
            set { terrainTypeData2D = value; }
        }

        private Vector2Int TerrainTypeSize
        {
            get { return new Vector2Int(Map.Width, Map.Height); }
        }

        private Vector2 MapSize
        {
            get { return new Vector2(MapLuaParser.GetMapSizeX(), MapLuaParser.GetMapSizeY()); }
        }

        private Vector2 SizeProp
        {
            get { return MapSize / 512f; }
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
                    terrainRay = CameraControler.Current.Cam.ScreenPointToRay(MousePos);
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
            get { return sizeField.value / ((SizeProp.x + SizeProp.y) / 4); }
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
//                return (TerrainPos2*10-new Vector2(BrushSizeRecalc, BrushSizeRecalc)/4)/MapSize;
//                return TerrainPos2*10/MapSize-new Vector2(BrushSizeRecalc, BrushSizeRecalc);
                return (TerrainPos2 * 10 - new Vector2(BrushSize, BrushSize)) / MapSize;
            }
        }

        private TerrainTypeLayerSettings currentLayer;
        private TerrainTypeLayerSettings defaultLayer;

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Close();
        }

        private void Update()
        {
            if (MapLuaParser.Current.EditMenu.MauseOnGameplay)
            {
                if (ChangedMousePos)
                {
                    UpdateBrushPos(UVPos);

                    if (currentState != State.Pipette)
                    {
                        if (Input.GetMouseButton(0) && HasHit)
                        {
//                    Debug.LogFormat("TerrainPos2:{0}, TerrainTypeSize:{1}, BrushSize:{2}", TerrainPos2, TerrainTypeSize, BrushSize);
                            Undo.Current.RegisterTerrainTypePaint();
                            Paint(new Vector2(TerrainPos2.x * 10, TerrainTypeSize.y - TerrainPos2.y * 10), BrushSize,
                                currentState == State.Normal ? currentLayer : defaultLayer);
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && HasHit)
                        {
                            Pipete(new Vector2(TerrainPos2.x * 10, TerrainTypeSize.y - TerrainPos2.y * 10));
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    ApplyChanges();
                }

                if (currentState != State.Pipette)
                {
                    if (Input.GetMouseButtonDown(0) && HasHit)
                    {
//                    Debug.LogFormat("TerrainPos2:{0}, TerrainTypeSize:{1}, BrushSize:{2}", TerrainPos2, TerrainTypeSize, BrushSize);
                        Undo.Current.RegisterTerrainTypePaint();
                        Paint(new Vector2(TerrainPos2.x * 10, TerrainTypeSize.y - TerrainPos2.y * 10), BrushSize,
                            currentState == State.Normal ? currentLayer : defaultLayer);
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && HasHit)
                    {
                        Pipete(new Vector2(TerrainPos2.x * 10, TerrainTypeSize.y - TerrainPos2.y * 10));
                    }
                }
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                currentState = State.Eraser;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                currentState = State.Pipette;
            }
            else
            {
                currentState = State.Normal;
            }
        }

        private void Init()
        {
            sizeField.OnValueChanged.AddListener(OnSizeChanged);
            sizeField.OnEndEdit.AddListener(OnSizeChangeEnd);
            clearAllButton.onClick.AddListener(OnClearAllButtonPressed);
            clearCurrentButton.onClick.AddListener(OnClearCurrentButtonPressed);
            layerCapacityField.OnValueChanged.AddListener(OnCapacityChanged);
//            sizeField.OnEndEdit.AddListener();

            RebuildBrush(BrushSize);

            ScmapEditor.Current.TerrainMaterial.SetInt("_Brush", 1);
            ScmapEditor.Current.TerrainMaterial.SetTexture("_BrushTex", brushTexture);
            ScmapEditor.Current.TerrainMaterial.SetFloat("_BrushSize", BrushSizeRecalc);
            ScmapEditor.Current.TerrainMaterial.SetTexture("_TerrainTypeAlbedo", TerrainTypeTexture);
            ScmapEditor.Current.TerrainMaterial.SetInt("_HideTerrainType", 0);

            layerCapacityField.SetValue(0.228f * 100);
            CreateUILayerSettings();
            HideMoreLayerInfo();
        }

        private void OnClearAllButtonPressed()
        {
//            TerrainTypeTexture.SetPixels(new byte[TerrainTypeTexture.width*TerrainTypeTexture.height].Select(b => defaultColor).ToArray());
            Undo.Current.RegisterTerrainTypePaint();
            TerrainTypeTexture = null;
            byte defaultIndex = layersSettings.GetFirstLayer().index;

            for (int j = 0; j < TerrainTypeSize.y; j++)
            {
                for (int i = 0; i < TerrainTypeSize.x; i++)
                {
                    TerrainTypeData2D[i, j] = defaultIndex;
                }
            }

            TerrainTypeData = TerrainTypeData.Select(b => defaultIndex).ToArray();
            ApplyTerrainTypeChanges();
            ScmapEditor.Current.TerrainMaterial.SetTexture("_TerrainTypeAlbedo", TerrainTypeTexture);
        }

        private void OnClearCurrentButtonPressed()
        {
            Undo.Current.RegisterTerrainTypePaint();
            //Color defaultColor = layersSettings.GetFirstLayer().color;
            byte defaultIndex = layersSettings.GetFirstLayer().index;

            for (int j = 0; j < TerrainTypeSize.y; j++)
            {
                for (int i = 0; i < TerrainTypeSize.x; i++)
                {
                    if (TerrainTypeData2D[i, j] == currentLayer.index)
                    {
                        TerrainTypeData2D[i, j] = defaultIndex;
                    }
                }
            }

//            TerrainTypeTexture.SetPixels(new byte[TerrainTypeTexture.width*TerrainTypeTexture.height].Select(b => defaultColor).ToArray());
            TerrainTypeTexture = null;
            ApplyTerrainTypeChanges();
            ScmapEditor.Current.TerrainMaterial.SetTexture("_TerrainTypeAlbedo", TerrainTypeTexture);
        }

        private void CreateUILayerSettings()
        {
            if (layerSettingsItems != null)
            {
                return;
            }

            layerSettingsItems = new Dictionary<byte, LayerSettingsItem>();

            currentLayer = defaultLayer = layersSettings.GetFirstLayer();
            if (savedLayer != null)
            {
                currentLayer = savedLayer;
            }

            LayerSettingsItem currentLayerItem = null;

            foreach (TerrainTypeLayerSettings layerSettings in layersSettings)
            {
                GameObject tmpItem = Instantiate<GameObject>(layerSettingsItemGO);
                LayerSettingsItem layerSettingsItem = tmpItem.GetComponent<LayerSettingsItem>();
                tmpItem.transform.SetParent(layersPivot, false);
                layerSettingsItem.Init(layerSettings, layersToggleGroup, ShowMoreLayerInfo, HideMoreLayerInfo);
                layerSettingsItem.onActive += OnLayerChanged;

                layerSettingsItems.Add(layerSettingsItem.index, layerSettingsItem);

                if (layerSettingsItem.index == currentLayer.index)
                {
                    currentLayerItem = layerSettingsItem;
                }
            }

            if (currentLayerItem == null)
            {
                Debug.LogError("Can`t find default layer");
                currentLayerItem = layerSettingsItems[0];
            }

            currentLayerItem.SetActive(true);
        }

        private void ClearUILayerSettings()
        {
            foreach (var layerSettingsItem in layerSettingsItems)
            {
                layerSettingsItem.Value.Clear();
                Destroy(layerSettingsItem.Value.gameObject);
            }

            layerSettingsItems.Clear();
        }

        /// <summary>
        /// Select layer in visual list
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="focus">Scroll list to this layer</param>
        private void SelectLayer(byte layerIndex, bool focus = false)
        {
            if (!layerSettingsItems.ContainsKey(layerIndex))
            {
                layerIndex = 1;// default layer index
            }
            SelectLayer(layerSettingsItems[layerIndex], focus);
        }

        /// <summary>
        /// Select layer in visual list
        /// </summary>
        /// <param name="layerItem"></param>
        /// <param name="focus">Scroll list to this layer</param>
        private void SelectLayer(LayerSettingsItem layerItem, bool focus = false)
        {
            layerItem.SetActive(true);
            if (focus)
            {
                Vector3 tmpPos = layersPivot.localPosition;
                tmpPos.y = -layerItem.transform.localPosition.y;
                layersPivot.localPosition = tmpPos;
            }
        }
        
        private void Close()
        {
            sizeField.OnValueChanged.RemoveAllListeners();
            sizeField.OnEndEdit.RemoveAllListeners();
            clearAllButton.onClick.RemoveAllListeners();
            clearCurrentButton.onClick.RemoveAllListeners();
            layerCapacityField.OnValueChanged.RemoveAllListeners();


//            terrainMaterial.SetTexture("_BrushTex", null);

            ApplyTerrainTypeChanges();
            HideMoreLayerInfo();

            ScmapEditor.Current.TerrainMaterial.SetInt("_Brush", 0);
            ScmapEditor.Current.TerrainMaterial.SetInt("_HideTerrainType", 1);
            ScmapEditor.Current.TerrainMaterial.SetTexture("_TerrainTypeAlbedo", null);

            TerrainTypeTexture = null;
            TerrainTypeData2D = null;
            savedLayer = currentLayer;
//            ClearUILayerSettings();
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
            ScmapEditor.Current.TerrainMaterial.SetFloat("_BrushSize", BrushSizeRecalc);
        }

        private void OnCapacityChanged()
        {
            ScmapEditor.Current.TerrainMaterial.SetFloat("_TerrainTypeCapacity", layerCapacityField.value * 0.01f);
        }

        private void OnSizeChangeEnd()
        {
//            terrainMaterial.SetFloat("_BrushSize", BrushSizeRecalc);
            RebuildBrush(BrushSize);
        }

        private void OnLayerChanged(byte layer)
        {
            currentLayer = layersSettings[layer];
            RebuildBrush(BrushSize);
        }

        private void UpdateBrushPos(Vector2 position)
        {
            ScmapEditor.Current.TerrainMaterial.SetFloat("_BrushUvX", position.x);
            ScmapEditor.Current.TerrainMaterial.SetFloat("_BrushUvY", position.y);
//            Debug.LogFormat("Pos:{0}, Size:{1}", position, BrushSize);
        }

        private void RebuildBrush(float brushSize)
        {
            //            Vector2Int brushTextureReferenceSize = new Vector2Int((int)brushSize/terrainTypeSize.x, (int)brushSize/terrainTypeSize.y);
            //float sizeF = brushSize / TerrainTypeSize.x * TerrainTypeSize.x;
            float sizeF = brushSize;
            int size = (int) sizeF;
            int spacing = 5;
            brushTexture = new Texture2D((int) size * 2, (int) size * 2, TextureFormat.ARGB32, false, false);
            brushTexture.anisoLevel = 0;
            brushTexture.wrapMode = TextureWrapMode.Clamp;
            brushTexture.filterMode = FilterMode.Point;
            Color empty = new Color(0, 0, 0, 0);
            for (int j = -size; j < size; j++)
            {
                for (int i = -size; i < size; i++)
                {
                    if (i * i + j * j > size * size - spacing * spacing)
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


//            oldLayerIndex = currentLayer.index;
//            currentLayerBrush.SetPixels(BrushTexture.GetPixels().Select(color => color * currentLayer.color).ToArray());
        }

        /// <summary>
        /// Painting on TerrainTypeLayer
        /// </summary>
        /// <param name="position">Brush position</param>
        /// <param name="brushSize"></param>
        /// <param name="layer">Layer index</param>
        private void Paint(Vector2 positionCenter, float brushSize, TerrainTypeLayerSettings layer)
        {
            Rect rect = Rect.MinMaxRect(
                positionCenter.x - brushSize, positionCenter.y - brushSize,
                positionCenter.x + brushSize, positionCenter.y + brushSize
            );

//            if (RectUtils.IsCrossed(new Rect(Vector2.zero, TerrainTypeSize), rect))
//            {

            var crossRect = RectUtils.Cross(new Rect(Vector2.zero, TerrainTypeSize), rect);
            int size = (int) brushSize;
//            Vector2 positionCenterLocal = positionCenter - crossRect.position;
//            Vector2 positionCenterLocal = positionCenter - crossRect.position;

            TerrainTypeTexture.SetPixels(
                Mathf.RoundToInt(crossRect.x), Mathf.RoundToInt(crossRect.y),
                Mathf.RoundToInt(crossRect.width), Mathf.RoundToInt(crossRect.height),
                TerrainTypeTexture.GetPixels(
                        Mathf.RoundToInt(crossRect.x), Mathf.RoundToInt(crossRect.y),
                        Mathf.RoundToInt(crossRect.width), Mathf.RoundToInt(crossRect.height))
                    .Select((color, i) =>
                        {
                            int x = i % Mathf.RoundToInt(crossRect.width) + Mathf.RoundToInt(crossRect.xMin) -
                                    Mathf.RoundToInt(positionCenter.x);
                            int y = i / Mathf.RoundToInt(crossRect.width) + Mathf.RoundToInt(crossRect.yMin) -
                                    Mathf.RoundToInt(positionCenter.y);

                            return (x * x + y * y > size * size) ? color : layer.color;
                        }
                    )
                    .ToArray()
            );

            for (int j = (int) crossRect.yMin; j < (int) crossRect.yMax; j++)
            {
                for (int i = (int) crossRect.xMin; i < (int) crossRect.xMax; i++)
                {
                    int x = (int) (i - positionCenter.x);
                    int y = (int) (j - positionCenter.y);
                    if (x * x + y * y <= size * size)
                    {
                        TerrainTypeData2D[i, j] = layer.index;
                    }
                }
            }

            TerrainTypeTexture.Apply();
        }

        private void Pipete(Vector2 positionCenter)
        {
            SelectLayer(TerrainTypeData2D[(int) positionCenter.x, (int) positionCenter.y], true);
        }

        private void ApplyChanges()
        {
//            Undo.Current.RegisterTerrainTypePaint();
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

        private void SetFromUndo(byte[] terrainTypeData)
        {
            TerrainTypeData = terrainTypeData;
            TerrainTypeData2D = null;
            TerrainTypeTexture = null;
            //            TerrainTypeTexture.SetPixels(texData);
            //            TerrainTypeTexture.Apply();
            ScmapEditor.Current.TerrainMaterial.SetTexture("_TerrainTypeAlbedo", TerrainTypeTexture);
        }

        private byte[] GetToUndo()
        {
            return TerrainTypeData;
        }

        private Color[] GetToUndoTex()
        {
            return TerrainTypeTexture.GetPixels();
        }

        public static void SetUndoData(byte[] terrainTypeData)
        {
            instance.SetFromUndo(terrainTypeData);
        }

        public static byte[] GetUndoData()
        {
            return instance.GetToUndo();
        }

        public static Color[] GetUndoTexData()
        {
            return instance.GetToUndoTex();
        }

        public enum State
        {
            Normal,
            Eraser,
            Pipette
        }
    }
}