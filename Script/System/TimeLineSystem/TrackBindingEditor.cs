using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TrackBinding))]
public class TrackBindingEditorClass : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //프로퍼티 시작
        EditorGUI.BeginProperty(position, label, property);

        //타입 가져옴
        SerializedProperty trackTypeProp = property.FindPropertyRelative("trackType");
        TrackType type = (TrackType)trackTypeProp.enumValueIndex;

        //TrackType 드롭 다운 
        Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typeRect, trackTypeProp, new GUIContent("트랙 타입"));  

        //기본 오프셋
        float yoffset = EditorGUIUtility.singleLineHeight + 2f;

        Rect infoRect = new Rect(position.x, position.y + yoffset, position.width, EditorGUIUtility.singleLineHeight - yoffset);

        switch(type)
        {
            case TrackType.Single:
                OnSingle(infoRect, property);
                break;

            case TrackType.Hierarchy:
                OnHierarchy(infoRect, property);
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //타입 Enum 찾기
        SerializedProperty trackTypeProp = property.FindPropertyRelative("trackType");
        TrackType type = (TrackType)trackTypeProp.enumValueIndex;

        //기본 높이
        float baseHeight = EditorGUIUtility.singleLineHeight + 2f;

        //상황에 맞는 프로퍼티 찾기
        SerializedProperty infoProp = type == TrackType.Single ?
            property.FindPropertyRelative("singleInfo") :
            property.FindPropertyRelative("hierarchyInfo"); 

        return baseHeight + EditorGUI.GetPropertyHeight(infoProp, true);
    }

    private void OnSingle(Rect rect, SerializedProperty property)
    {
        ShowProp(rect, property, "singleInfo", "SingleTrackBind");
    }

    private void OnHierarchy(Rect rect, SerializedProperty property)
    {
        ShowProp(rect, property, "hierarchyInfo", "HierarchyBind");
    }

    private void ShowProp(Rect rect, SerializedProperty property, string propName, string labelName)
    {
        EditorGUI.PropertyField(
            rect, 
            property.FindPropertyRelative(propName), 
            new GUIContent(labelName),
            true);
    }
}

#endif