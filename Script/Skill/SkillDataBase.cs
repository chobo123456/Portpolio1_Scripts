using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[CreateAssetMenu(fileName = "SkillDataBase", menuName = "Skill/SkillDataBase")]
public class SkillDataBase : ScriptableObject
{
    public List<SkillData> lists;
}


#if UNITY_EDITOR
[CustomEditor(typeof(SkillDataBase))]
public class SkillDataBaseCustomEditor : Editor
{
    private ReorderableList reorderableList;

    private void OnEnable()
    {
        SerializedProperty listProp = serializedObject.FindProperty("lists");

        reorderableList = new ReorderableList(
            serializedObject, 
            listProp, 
            true, 
            true, 
            true, 
            true);

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(index);

            rect.y += 2f;

            string labelName = $"Skill [{index}]";

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element,
                new GUIContent(labelName),
                true);
        };

        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, new GUIContent("SkillDataList"));
        };

        reorderableList.elementHeightCallback = (index) =>
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4f;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        reorderableList.DoLayoutList();

        DrawPropertiesExcluding(serializedObject, "m_script", "lists");

        serializedObject.ApplyModifiedProperties();
    }
}
#endif