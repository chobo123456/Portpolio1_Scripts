using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif


[CreateAssetMenu(fileName = "TypePoolDataBase", menuName = "Pool/TypePoolDataBase")]
public class TypePoolDataBase : ScriptableObject
{
    public List<PoolInfo> poolDatas;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TypePoolDataBase), true)]
public class TypePoolDataBaseEditor : Editor
{
    private ReorderableList _reorderableList;

    public void OnEnable()
    {
        SerializedProperty prop = serializedObject.FindProperty("poolDatas");

        _reorderableList = new ReorderableList(
            serializedObject, 
            prop, 
            true, 
            true, 
            true,
            true);

        _reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isfocus) =>
        {
            SerializedProperty element = prop.GetArrayElementAtIndex(index);

            rect.y += 2;

            string labelName = $"pool [{index}]";

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                new GUIContent(labelName),
                true);
        };

        _reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "poolDataBase");  
        };

        _reorderableList.elementHeightCallback = (index) =>
        {
            return EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(index), true) + 4;  
        };
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _reorderableList.DoLayoutList();
        
        DrawPropertiesExcluding(serializedObject, "m_script", "poolDatas");

        serializedObject.ApplyModifiedProperties();
    }
}
#endif