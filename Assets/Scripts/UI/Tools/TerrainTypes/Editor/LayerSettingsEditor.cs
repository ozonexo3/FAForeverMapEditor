using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EditMap.TerrainTypes.Editor
{
    [CustomEditor(typeof(LayersSettings))]
    public class LayerSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty LayersList;
        private ReorderableList reorderableList;

        private float leftSpaceWidth = 10f;
        private float nameWidth = 100f;
        private float indexWidth = 50f;
        private float colorWidth = 100f;
        private float blockingWidth = 60f;
        private float styleWidth = 110f;
        private float descriptionWidth = 0.2f;
        private float descriptionHeight = 1 - 0.25f;
        private float descriptionButtonWidth = 0.2f;

        private static Vector4 border = new Vector4(0, 3, 0, 3);

        private static float defaultElementHeight = 21f;
        private static float selectedElementHeight = defaultElementHeight * 4;

        private int selectedIndex = -1;

        private void OnEnable()
        {
            LayersList = serializedObject.FindProperty("layersSettings");
            reorderableList = new ReorderableList(serializedObject, LayersList, true, true, true, true);
            reorderableList.drawElementCallback += DrawElement;
            reorderableList.onChangedCallback += list => { serializedObject.ApplyModifiedProperties(); };
            reorderableList.elementHeightCallback += GetElementHeight;
            reorderableList.drawHeaderCallback += DrawHeader;
//            reorderableList.onAddCallback += OnAddElement;
//            reorderableList.onAddDropdownCallback+= (rect, list) =>
//            {
//                GUI.W
//            }
//            reorderableList.

            UnityEditor.Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            UnityEditor.Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
//            EditorGUILayout.FloatField("elementHeight", reorderableList.elementHeight);
//            nameWidth = EditorGUILayout.FloatField("nameWidth", nameWidth);
//            indexWidth = EditorGUILayout.FloatField("indexWidth", indexWidth);
//            colorWidth = EditorGUILayout.FloatField("colorWidth", colorWidth);
//            blockingWidth = EditorGUILayout.FloatField("blockingWidth", blockingWidth);
//            styleWidth = EditorGUILayout.FloatField("styleWidth", styleWidth);
            reorderableList.DoLayoutList();
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var property = LayersList.GetArrayElementAtIndex(index);
            var nameProperty = property.FindPropertyRelative("name"); //name
            var indexProperty = property.FindPropertyRelative("index"); //index
            var colorProperty = property.FindPropertyRelative("color"); //color
            var blockingProperty = property.FindPropertyRelative("blocking"); //blocking
            var styleProperty = property.FindPropertyRelative("style"); //style
            var descriptionProperty = property.FindPropertyRelative("description"); //description
//            name
//            index
//            color
//            blocking
//            style
//            description
            rect.yMin += 3;
            rect.yMax -= 3;
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                if (!active)
                {
                    float shift = rect.x;
                    EditorGUI.PropertyField(new Rect(shift, rect.y, nameWidth, rect.height), nameProperty,
                        GUIContent.none);
                    shift += nameWidth;
                    EditorGUI.PropertyField(new Rect(shift, rect.y, indexWidth, rect.height), indexProperty,
                        GUIContent.none);
                    shift += indexWidth;
                    EditorGUI.PropertyField(new Rect(shift, rect.y, blockingWidth, rect.height), blockingProperty,
                        GUIContent.none);
                    shift += blockingWidth;
                    EditorGUI.PropertyField(new Rect(shift, rect.y, styleWidth, rect.height), styleProperty,
                        GUIContent.none);
                    shift += styleWidth;
                    EditorGUI.PropertyField(new Rect(rect.xMax - colorWidth, rect.y, colorWidth, rect.height),
                        colorProperty, GUIContent.none);
                    shift += colorWidth;
                }
                else
                {
                    float shift = rect.x;
                    EditorGUI.PropertyField(
                        new Rect(shift, rect.y, nameWidth, rect.height * (1 - descriptionHeight) - 3), nameProperty,
                        GUIContent.none);
                    shift += nameWidth;
                    EditorGUI.PropertyField(
                        new Rect(shift, rect.y, indexWidth, rect.height * (1 - descriptionHeight) - 3), indexProperty,
                        GUIContent.none);
                    shift += indexWidth;
                    EditorGUI.PropertyField(
                        new Rect(shift, rect.y, blockingWidth, rect.height * (1 - descriptionHeight) - 3),
                        blockingProperty, GUIContent.none);
                    shift += blockingWidth;
                    EditorGUI.PropertyField(
                        new Rect(shift, rect.y, styleWidth, rect.height * (1 - descriptionHeight) - 3), styleProperty,
                        GUIContent.none);
                    shift += styleWidth;
                    EditorGUI.PropertyField(
                        new Rect(rect.xMax - colorWidth, rect.y, colorWidth, rect.height * (1 - descriptionHeight) - 3),
                        colorProperty, GUIContent.none);
                    shift += colorWidth;


                    descriptionProperty.stringValue = EditorGUI.TextArea(
                        new Rect(rect.x, rect.y + rect.height * (1 - descriptionHeight), rect.width,
                            rect.height * descriptionHeight), descriptionProperty.stringValue);
                }

                if (scope.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private float GetElementHeight(int index)
        {
            if (reorderableList.index == index)
            {
                return selectedElementHeight;
            }

            return defaultElementHeight;
        }

        private void DrawHeader(Rect rect)
        {
            float shift = rect.x + leftSpaceWidth;
            EditorGUI.LabelField(new Rect(shift, rect.y, rect.width, rect.height), "name");
            shift += nameWidth;
            EditorGUI.LabelField(new Rect(shift, rect.y, rect.width, rect.height), "index");
            shift += indexWidth;
            EditorGUI.LabelField(new Rect(shift, rect.y, rect.width, rect.height), "blocking");
            shift += blockingWidth;
            EditorGUI.LabelField(new Rect(shift, rect.y, rect.width, rect.height), "style");
            shift += styleWidth;
            EditorGUI.LabelField(new Rect(rect.xMax - colorWidth, rect.y, colorWidth, rect.height), "color");
            shift += colorWidth;
        }

        private void OnAddElement(ReorderableList list)
        {
//            foreach (TerrainTypeLayerSettings layerSettings in list.list)
//            {
//                layerSettings.index
//            }

//            list.list.Add(new TerrainTypeLayerSettings());
        }

        private void DrawElementDescription(Rect rect, int index, bool active, bool focused)
        {
        }
    }
}