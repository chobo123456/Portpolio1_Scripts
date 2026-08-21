using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine.Playables;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TimeLineEventStruct))]
public class TimeLineEndEventEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, prop);

        //타입 가져옴
        SerializedProperty typeProp = prop.FindPropertyRelative("type");
        TimeLineEventType type = (TimeLineEventType)typeProp.enumValueIndex;
        Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typeRect, typeProp, new GUIContent("타임라인 엔드 이벤트 타입"));
        
        float lineHeight    = EditorGUIUtility.singleLineHeight;
        float spacing       = 2f;
        float step          = lineHeight + spacing;

        SerializedProperty eventNameProp = prop.FindPropertyRelative("timelineEventName");
        Rect eventNameRect = new Rect(position.x, position.y + step, position.width, lineHeight);
        EditorGUI.PropertyField(eventNameRect, eventNameProp, new GUIContent("타임라인 엔드 이벤트 이름"));
        
        float currentY      = eventNameRect.y;

        currentY += lineHeight + spacing;

        switch(type)
        {
            case TimeLineEventType.Int :
                ShowProp(position, ref currentY, prop, "intValue1" ,"정수");
                break;

            case TimeLineEventType.Float :
                ShowProp(position, ref currentY, prop, "floatValue1" ,"실수");
                break;

            case TimeLineEventType.String :  
                ShowProp(position, ref currentY, prop, "stringValue1" ,"문자열");
                break;

            case TimeLineEventType.Int_Int :
                ShowProp(position, ref currentY, prop, "intValue1" ,"정수1");
                ShowProp(position, ref currentY, prop, "intValue2" ,"정수2");
                break;

            case TimeLineEventType.Int_Float :
                ShowProp(position, ref currentY, prop, "intValue1" ,"정수1");
                ShowProp(position, ref currentY, prop, "floatValue2" ,"실수2");
                break;

            case TimeLineEventType.Int_String :
                ShowProp(position, ref currentY, prop, "intValue1" ,"정수1");
                ShowProp(position, ref currentY, prop, "stringValue2" ,"문자열2");
                break;

            case TimeLineEventType.Int_Vector3 :
                ShowProp(position, ref currentY, prop, "intValue1" ,"정수1");
                ShowProp(position, ref currentY, prop, "vector3Value2" , "벡터2");
                break;

            case TimeLineEventType.Float_Int :
                ShowProp(position, ref currentY, prop, "floatValue1" ,"실수1");
                ShowProp(position, ref currentY, prop, "intValue2" ,"정수2");
                break;

            case TimeLineEventType.Float_Float :
                ShowProp(position, ref currentY, prop, "floatValue1" ,"실수1");
                ShowProp(position, ref currentY, prop, "floatValue2" ,"실수2");
                break;

            case TimeLineEventType.Float_String :
                ShowProp(position, ref currentY, prop, "floatValue1" ,"실수1");
                ShowProp(position, ref currentY, prop, "stringValue2" ,"문자열2");
                break;

            case TimeLineEventType.String_Int :
                ShowProp(position, ref currentY, prop, "stringValue1" ,"문자열1");
                ShowProp(position, ref currentY, prop, "intValue2" ,"정수2");
                break;

            case TimeLineEventType.String_Float :
                ShowProp(position, ref currentY, prop, "stringValue1" ,"문자열1");
                ShowProp(position, ref currentY, prop, "floatValue2" ,"실수2");
                break;

            case TimeLineEventType.String_String :
                ShowProp(position, ref currentY, prop, "stringValue1" ,"문자열1");
                ShowProp(position, ref currentY, prop, "stringValue2" ,"문자열2");
                break;

            case TimeLineEventType.Vector3_Int :
                ShowProp(position, ref currentY, prop, "vector3Value2" , "벡터2");
                ShowProp(position, ref currentY, prop, "intValue1" ,"정수1");
                break;
        }

        EditorGUI.EndProperty();
    }

    private void ShowProp(Rect position, ref float currentY, SerializedProperty property, string propName, string showName)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect rect = new Rect(position.x, currentY, position.width, lineHeight);
        
        EditorGUI.PropertyField(
            rect, 
            property.FindPropertyRelative(propName),
            new GUIContent(showName), 
            true);

        currentY += lineHeight + 2f;
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        SerializedProperty typeProp = prop.FindPropertyRelative("type");
        TimeLineEventType type = (TimeLineEventType)typeProp.enumValueIndex;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        int extraLines = 0;

        switch(type)
        {
            case TimeLineEventType.Int :
            case TimeLineEventType.Float :
            case TimeLineEventType.String :
                extraLines = 1;
                break;
            default :
                extraLines = 2;
                break;
        };

        int totalLine = 2 + extraLines;
        return (lineHeight + spacing) * totalLine;
    }
}
#endif

public enum TimeLineEventType
{
    Int,
    Float,
    String,

    Int_Int,
    Int_Float,
    Int_String,
    Int_Vector3,

    Float_Int,
    Float_Float,
    Float_String,

    String_Int,
    String_Float,
    String_String,

    Vector3_Int,
}

[System.Serializable]
public struct TimeLineEventStruct
{
    public TimeLineEventType type;

    public int intValue1;
    public int intValue2;

    public float floatValue1;
    public float floatValue2;

    public string stringValue1;
    public string stringValue2;
    
    public Vector3 vector3Value2;
    public string timelineEventName;

    public void EventInvoke()
    {
        switch(type)
        {
            case TimeLineEventType.Int:
                EventBus.Invoke<int>(timelineEventName, intValue1);
                break;

            case TimeLineEventType.Float:
                EventBus.Invoke<float>(timelineEventName, floatValue1);
                break;

            case TimeLineEventType.String:
                EventBus.Invoke<string>(timelineEventName, stringValue1);
                break;

            case TimeLineEventType.Int_Int:
                EventBus.Invoke<int, int>(timelineEventName, intValue1, intValue2);
                break;

            case TimeLineEventType.Int_Float:
                EventBus.Invoke<int, float>(timelineEventName, intValue1, floatValue2);
                break;

            case TimeLineEventType.Int_String:
                EventBus.Invoke<int, string>(timelineEventName, intValue1, stringValue2);
                break;
            
            case TimeLineEventType.Int_Vector3:
                EventBus.Invoke<int, Vector3>(timelineEventName, intValue1, vector3Value2);
                break;

            case TimeLineEventType.Float_Int:
                EventBus.Invoke<float, int>(timelineEventName, floatValue1, intValue2);
                break;

            case TimeLineEventType.Float_Float:
                EventBus.Invoke<float, float>(timelineEventName, floatValue1, floatValue2);
                break;

            case TimeLineEventType.Float_String:
                EventBus.Invoke<float, string>(timelineEventName, floatValue1, stringValue2);
                break;

            case TimeLineEventType.String_Int:
                EventBus.Invoke<string, int>(timelineEventName, stringValue1, intValue2);
                break;

            case TimeLineEventType.String_Float:
                EventBus.Invoke<string, float>(timelineEventName, stringValue1, floatValue2);
                break;

            case TimeLineEventType.String_String:
                EventBus.Invoke<string, string>(timelineEventName, stringValue1, stringValue2);
                break;

            case TimeLineEventType.Vector3_Int:
                EventBus.Invoke<Vector3, int>(timelineEventName, vector3Value2, intValue1);
                break;
        }   
    }
}



[CreateAssetMenu(fileName = "TimeLineAsset", menuName = "TimeLine/TimeLineAsset")]
public class TimeLineAsset : ScriptableObject
{
    public int timeLineId;
    public TimelineAsset timeline;

    public TimeLineInfo timeLineInfo;

    public bool needHold;
    public bool needGameStateStop;
    public bool needTimelineSave;   

    [Tooltip("needHold가 켜진경우 해당 변수는 의미 없어짐")]
    public bool needMovePlayer;

    public TimeLineEventStruct timeLineStartEvent;
    public TimeLineEventStruct timeLineEndEvent;
}
