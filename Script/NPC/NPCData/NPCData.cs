using UnityEngine;

public enum NPCType
{
    QuestNPC,
    ShopNPC
}

[CreateAssetMenu(fileName = "NPCData", menuName = "NPC/NPCData")]
public class NPCData : ScriptableObject
{
    public int npcId;

    public string npcName;
    public NPCType type;

    [HideInInspector] public TalkData startTalk;
    [HideInInspector] public TalkData duringTalk;
    [HideInInspector] public TalkData endTalk;
    [HideInInspector] public ShopBuyList shopData;
}


