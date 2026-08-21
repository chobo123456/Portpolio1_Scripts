using UnityEngine;
using System.Collections.Generic;

public enum TalkWithType
{
    NPC,
    TimeLine,
}

public enum TalkType
{
    None,
    Quest,
    Shop,
}

[CreateAssetMenu(fileName = "TalkData", menuName = "EventScene/TalkData")]
public class TalkData : ScriptableObject
{
    public int talkId;
    public TalkWithType talkWithType;
    public TalkType talkType;
    public List<TalkInfo> infos;
    public Talk_ETCInfo etcInfo;
    public Talk_ShopInfo shopInfo;
}

[System.Serializable]
public struct TalkInfo
{
    public string talkCharacterName;
    public string talkContext;
    public int timeLineId;
}

[System.Serializable]
public struct Talk_ETCInfo
{
    public int getQuestId;
    public int finishQuestId;
}

[System.Serializable]
public struct Talk_ShopInfo
{
    public string invokeEventName;
}