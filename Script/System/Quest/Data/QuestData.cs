using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public struct ItemReceiveInfo
{
    public int itemId;
    public int itemAmount;    
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    [Tooltip("해당 퀘스트의 아이디")]
    public int questId;

    [Tooltip("퀘스트의 타입")]
    public QuestType questType;
    [TextArea]
    public string questDiscription;
    [TextArea]
    public string questHUDDiscription;
    [TextArea]
    public string questName;

    [Tooltip("최종 진행도(갯수)")]
    public int questProgress;

    [Tooltip("이전 퀘스트 아이디")]
    public int beforeQuestId;
    
    [Tooltip("목표 아이디")]
    public int targetId;

    [Tooltip("퀘스트 클리어시 줄 아이템")]
    public List<ItemReceiveInfo> questReceiveItemInfo;

    [Tooltip("퀘스트 위치")]
    public Vector3 questPoint;

    [Tooltip("퀘스트 완료시 부여되는 새 퀘스트 아이디")]
    public int nextQuestId;

    [Tooltip("튜토리얼 아이디")]
    public int tutorial_Id;

    [Tooltip("퀘스트 완료후 발생할 타임라인")]
    public int questFinishTimelineId;

    [Tooltip("워키 토키 아이디")]
    public int walkieTalkieId;

    [Tooltip("퀘스트 클리어시 발생할 이벤트 이름")]
    public string eventName;
}
public enum QuestType
{
    Hunt,
    DefeatBoss,
    Interact,
    WayPoint,
    Act,
    Craft
}

#if UNITY_EDITOR
[CustomEditor(typeof(QuestData))]
public class QuestDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        QuestData questData = (QuestData)target;

        ShowProp("questId", "퀘스트 아이디");
        ShowProp("questType", "퀘스트 타입");
        ShowProp("questDiscription", "퀘스트 설명");
        ShowProp("questHUDDiscription", "퀘스트 HUD 설명");
        ShowProp("questName", "퀘스트 이름");
        ShowProp("questProgress", "퀘스트 완료도");
        ShowProp("questReceiveItemInfo", "퀘스트 완료시 부여할 아이템");
        ShowProp("questFinishTimelineId", "퀘스트 완료시 발생될 타임라인");
        ShowProp("eventName", "퀘스트 시작시 발생할 이벤트 이름");
        ShowProp("walkieTalkieId", "퀘스트 진행중 나올 워키토키");
        
        switch(questData.questType)
        {
            case QuestType.Hunt:
                OnHunt();
                break;
            case QuestType.DefeatBoss:
                OnDefeatBoss();
                break;
            case QuestType.Interact:
                OnInteract();
                break;
            case QuestType.WayPoint:
                OnWayPoint();
                break;
            case QuestType.Act:
                OnAct();
                break;
            case QuestType.Craft:
                OnCraft();
                break;
            default:
                break;
        }   

        ShowProp("nextQuestId", "퀘스트 완료후 부여할 새로운 퀘스트");
        ShowProp("tutorial_Id", "퀘스트 튜토리얼");
        
        serializedObject.ApplyModifiedProperties();
    }

    private void OnHunt()
    {
        ShowProp("targetId", "목표 적 아이디");
        ShowProp("questPoint", "퀘스트 위치");
    }

    private void OnDefeatBoss()
    {
        ShowProp("targetId", "목표 보스 아이디");
    }

    private void OnInteract()
    {
        ShowProp("targetId", "목표 입력값(본인의 퀘스트 아이디적기)");
        ShowProp("beforeQuestId", "이전 퀘스트 아이디");
        ShowProp("questPoint", "퀘스트 위치");
    }

    private void OnWayPoint()
    {
        ShowProp("questPoint", "퀘스트 위치");
        ShowProp("targetId", "목표 위치 아이디(항상 0으로 세팅)");
    }

    private void OnAct()
    {
        ShowProp("targetId", "목표 행동 아이디");
    }

    private void OnCraft()
    {
        ShowProp("targetId", "목표 제작 아이템 아이디");
    }

    private void ShowProp(string propName, string showName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propName);
        EditorGUILayout.PropertyField(prop, new GUIContent(showName));
    }
}
#endif