using UnityEngine;

public class ShopNPC : NPCBase
{
    public override void Interact()
    {
        if(npcData == null) return;

        EventBus.Invoke<int>("ShopUI_ChangeList", npcData.npcId);
        EventBus.Invoke<TalkData>("OnTalk_UseData", npcData.startTalk);
    }
}