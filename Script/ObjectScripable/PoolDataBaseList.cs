using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif


[CreateAssetMenu(fileName = "PoolDataBaseList", menuName = "Pool/PoolDataBaseList")]
public class PoolDataBaseList : ScriptableObject
{
    public List<TypePoolDataBase> poolDataBaseList;
}

#if UNITY_EDITOR
[CustomEditor(typeof(PoolDataBaseList))]
public class PoolDataBaseListEditor : Editor
{
    private ReorderableList reorderableList;

    public void OnEnable()
    {
        SerializedProperty prop = serializedObject.FindProperty("poolDataBaseList");

        reorderableList = new ReorderableList(
            serializedObject, 
            prop, 
            true,
            true,
            true,
            true);

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocus) =>
        {
            SerializedProperty element = prop.GetArrayElementAtIndex(index);

            rect.y += 2;

            string label = $"dataBase [{index}]";

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                new GUIContent(label),
                true);  
        };

        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "DataBaseList");  
        };

        reorderableList.elementHeightCallback = (index) =>
        {
            return EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(index), true) + 4f;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        reorderableList.DoLayoutList();

        DrawPropertiesExcluding(serializedObject, "m_script", "poolDataBaseList");

        serializedObject.ApplyModifiedProperties();
    }
}

#endif