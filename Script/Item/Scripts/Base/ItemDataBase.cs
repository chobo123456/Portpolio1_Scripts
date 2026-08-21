using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[CreateAssetMenu(fileName = "ItemBase", menuName = "Item/ItemDataBase")]
public abstract class ItemDataBase : ScriptableObject
{
    public List<ItemData> items;
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemDataBase), true)]
public class ItemDataBaseScriptable_Editor : Editor
{
    private ReorderableList reorderableList;

    public void OnEnable()
    {
        SerializedProperty itemProp = serializedObject.FindProperty("items");

        reorderableList = new ReorderableList(serializedObject, itemProp, true, true, true, true);

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = itemProp.GetArrayElementAtIndex(index);

            rect.y += 2f;

            string label = $"Item [{index}]";

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                new GUIContent(label),
                true);
        };

        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "ItemList");
        };

        reorderableList.elementHeightCallback = (index) =>
        {
            SerializedProperty element = itemProp.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4f;
        };
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        reorderableList.DoLayoutList();
        DrawPropertiesExcluding(serializedObject, "m_script", "items");

        serializedObject.ApplyModifiedProperties();
    }
}

#endif