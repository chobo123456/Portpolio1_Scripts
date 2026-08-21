using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    None,
    Finished,
    InProgress
}

public class QuestManager : MonoBehaviour
{
    private QuestSystem _system;
    private QuestStore _store;
    private QuestUIHandler _uiHandler;
    
    private bool _isReadyLocalUI = false;
    private Action<int> OnSetMainQuest, OnUpdatePreview;

    private void OnEnable()
    {
        SubscribeEvent(true);
        this.RunRoutine(Booting());
    }

    private void OnDisable()
    {
        _store?.OnDisable();
        _system?.OnDisable();
        _uiHandler?.OnDisable();

        SubscribeEvent(false);
        LoadStatus.SetStatus(ManagerType.Quest, false);
    }
    
    private void SubscribeEvent(bool isSubscribe)
    {
        if (isSubscribe)
        {
            OnSetMainQuest += SetMainQuest;
            OnUpdatePreview += UpdatePreviewUI;

            EventBus.Sub("Quest_UI_LocalReady", OnLocalUIReady);
            EventBus.Sub<int>("ReceiveQuest", ReceiveQuest);
            EventBus.Sub<QuestType, int>("QuestManager_OnAskQuestFinish", OnQuestInProgress);    
        }
        else
        {
            OnSetMainQuest -= SetMainQuest;
            OnUpdatePreview -= UpdatePreviewUI;

            EventBus.UnSub("Quest_UI_LocalReady", OnLocalUIReady);
            EventBus.UnSub<int>("ReceiveQuest", ReceiveQuest);
            EventBus.UnSub<QuestType, int>("QuestManager_OnAskQuestFinish", OnQuestInProgress);    
        }
    }

    IEnumerator Booting()
    {
        _store = new();
        _store.OnEnable();

        _system = new();
        _system.InjectParameter(_store);
        _system.OnEnable();

        _uiHandler = new();
        _uiHandler.InjectParameter(_system, OnSetMainQuest, OnUpdatePreview);
        _uiHandler.OnEnable();

        yield return new WaitUntil(() => LoadStatus.IsReady && LoadStatus.IsReady_Inventory && _isReadyLocalUI);
        
        EventBus.Invoke<List<int>, int>("Quest_UI_Initialize", _system.GetUnFinishedQuests(), _system.GetMostSelectableQuestId());
        SetMainQuest(_store.GetMainQuest());

        LoadStatus.SetStatus(ManagerType.Quest, true);
    }

    private void OnLocalUIReady()
    {
        _isReadyLocalUI = true;
    }
    
    private void ReceiveQuest(int id)
    {
        _store.SetQuest(id);
        EventBus.Invoke<int>("Quest_UI_GetNewQuest", id);
        
        SetMainQuest(id);
    }

    public void SetMainQuest(int questId)
    {
        _system.SetMainQuest(questId);
        InvokeEvent(questId);
    }

    private void InvokeEvent(int questId)
    {
        EventBus.Invoke<QuestWayTrackerPayload>("Quest_WayTracker_StartTrack", _system.GetWayTrackPayload(questId));
        UpdatePreviewUI(questId);
        EventBus.Invoke<WalkieTalkiePayload>("PlayWalkieTalkie", _system.GetQuestWalkieTalkiePayload(questId));
        EventBus.Invoke<QuestMinimapPointPayload>("Quest_Minimap_Pointing", _system.GetMinimapPayload(questId));
        EventBus.Invoke<QuestHUDPayload>("Quest_HUD_DrawUI", _system.GetHUDPayload(questId));
        EventBus.Invoke<QuestPointPayload>("Quest_Point_DrawUI", _system.GetPointPayload(questId));
        EventBus.Invoke<int>("StartTutorial", _system.GetQuestTutorialId(questId));
    }

    private void UpdatePreviewUI(int questId)
    {
        EventBus.Invoke<QuestPreviewPayload>("Quest_UI_DrawQuestPreview", _system.Getpayload(questId));
    }

    private void OnQuestInProgress(QuestType questType, int inputId)
    {
        var result = _system.TryFinishQuest(questType, inputId);

        switch(result.State)
        {
            case QuestState.Finished:
                EventBus.Invoke<int>("Quest_UI_FinishQuest", result.questId);
                EventBus.Invoke("Quest_HUD_FinishQuest");
                EventBus.Invoke("Quest_Point_FinishQuest");

                if (result.questData.nextQuestId > 0)
                    ReceiveQuest(result.questData.nextQuestId);
                else
                    SetMainQuest(-1);

                ReceiveItems(result.questData);
                PlayQuestTimeline(result.questData);
                break;

            case QuestState.InProgress:
                EventBus.Invoke<float, float>("Quest_UI_UpdateProgress", _system.GetQuestProgresses(result.questId), _system.GetMaxQuestProgress(result.questId));
                EventBus.Invoke<float, float>("Quest_HUD_UpdateProgress", _system.GetQuestProgresses(result.questId), _system.GetMaxQuestProgress(result.questId));
                break;

            case QuestState.None:
                break;
        }
    }

    private void ReceiveItems(QuestData questData)
    {
        for (int i = 0; i < questData.questReceiveItemInfo.Count; i++)
        {
            ItemReceiveInfo receiveItemInfo = questData.questReceiveItemInfo[i];
            EventBus.Invoke<int, int, bool>("GetItem", receiveItemInfo.itemId, receiveItemInfo.itemAmount, true);
        }
    }

    private void PlayQuestTimeline(QuestData questData)
    {
        if (questData.questFinishTimelineId > 0)
            EventBus.Invoke<int>("PlayTimeLine", questData.questFinishTimelineId);
    }
}
