using UnityEngine;
using System.Collections;

public class QuestNPC : NPCBase
{
    private bool isInLoop = false;

    public override void Interact()
    {   
        if(npcData == null || isInLoop || !IsFinishBeforeQuestNpcQuest(npcData.startTalk)) return;

        isInLoop = true;

        if(CheckIsFinishQuest(npcData.startTalk)) //완료시
            this.RunRoutine(FinishQuestTalk());
        else if(CheckHasQuest(npcData.startTalk)) // 본인의 퀘스트를 받았을시
            this.RunRoutine(DuringQuestTalk());
        else // 퀘스트도 안받았을시
            this.RunRoutine(StartTalk());
    }

    IEnumerator StartTalk()
    {
        EventBus.Invoke<bool, float>("BlackPanelFade", false, 0.35f);
        yield return YieldUtil.WaitForSecondsRealtime(0.4f);
        EventBus.Invoke<bool, float>("BlackPanelFade", true, 0.35f);
        EventBus.Invoke<TalkData>("OnTalk_UseData", npcData.startTalk);

        isInLoop = false;
    }

    IEnumerator DuringQuestTalk()
    {
        EventBus.Invoke<bool, float>("BlackPanelFade", false, 0.35f);
        yield return YieldUtil.WaitForSecondsRealtime(0.4f);
        EventBus.Invoke<bool, float>("BlackPanelFade", true, 0.35f);

        EventBus.Invoke<TalkData>("OnTalk_UseData", npcData.duringTalk);

        isInLoop = false;
    }

    IEnumerator FinishQuestTalk()
    {
        EventBus.Invoke<bool, float>("BlackPanelFade", false, 0.35f);
        yield return YieldUtil.WaitForSecondsRealtime(0.4f);
        EventBus.Invoke<bool, float>("BlackPanelFade", true, 0.35f);
        EventBus.Invoke<TalkData>("OnTalk_UseData", npcData.endTalk);

        isInLoop = false;
    }

    private bool CheckIsFinishQuest(TalkData data)
    {
        return EventBus.Invoke_Func<int, bool>("QuestSystem_IsQuestFinish", data.etcInfo.getQuestId);
    }

    private bool CheckHasQuest(TalkData data)
    {
        return EventBus.Invoke_Func<int, bool>("QuestSystem_HasQuest", data.etcInfo.getQuestId);
    }

    private bool IsFinishBeforeQuestNpcQuest(TalkData data)
    {
        QuestData questData = DataLoader.GetData<QuestData>(DataType.Quest, data.etcInfo.finishQuestId);
        int beforeQuestId = questData.beforeQuestId;

        if(beforeQuestId <= 0) return true;

        return EventBus.Invoke_Func<int, bool>("QuestSystem_IsQuestFinish", beforeQuestId);
    }

    public override bool PassTrigger()
    {
        return IsFinishBeforeQuestNpcQuest(npcData.startTalk);
    }
}