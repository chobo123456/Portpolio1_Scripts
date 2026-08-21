using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(TalkData))]
public class TalkDataInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TalkData data = (TalkData)target;

        serializedObject.Update();
        
        switch(data.talkWithType)
        {
            case TalkWithType.NPC:
                break;

            case TalkWithType.TimeLine:
                OnTimeLine();
                break;
        }

        ShowPopup(GetPopup("talkWithType"), "TALK WITH:");
        ShowPopup(GetPopup("talkType"), "TALK TYPE:");
        ShowPopup(GetPopup("infos"), "TALK INFO:");

        switch(data.talkType)
        {
            case TalkType.None:
                break;
            case TalkType.Quest :
                OnQuest();
                break;
            case TalkType.Shop :
                OnShop();
                break;
        }
        

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowPopup(SerializedProperty prop, string name)
    {
        EditorGUILayout.PropertyField(prop, new GUIContent(name));
    }

    private SerializedProperty GetPopup(string name)
    {
        return serializedObject.FindProperty(name);
    }

    private void OnTimeLine()
    {
        ShowPopup(GetPopup("talkId"), "TALK ID:");
    }

    private void OnNormal()
    {
        ShowPopup(GetPopup("talkId"), "TALK DATA ID:");
    }

    private void OnQuest()
    {
        ShowPopup(GetPopup("etcInfo"), "NPC QUEST INFO:");
    }

    private void OnShop()
    {
        ShowPopup(GetPopup("shopInfo"), "NPC SHOP INFO:");
    }
}
#endif
