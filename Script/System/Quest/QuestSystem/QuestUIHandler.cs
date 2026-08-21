using System.Collections.Generic;
using UnityEngine;
using System;
public class QuestUIHandler
{
    private int _focusedQuestId = 0;
    private QuestSystem _system;
    private Action<int> OnSetMainQuest, OnUpdatePreview;
    public void InjectParameter(QuestSystem system, Action<int> setMainQuestAction, Action<int> updatePreviewAction)
    {
        _system = system;
        OnSetMainQuest = setMainQuestAction;
        OnUpdatePreview = updatePreviewAction;
    }

    public void OnEnable()
    {
        SubscribeEvent(true);
    }

    public void OnDisable()
    {
        SubscribeEvent(false);
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("Quest_UI_OnQuestAccept", OnQuestAccept);
            EventBus.Sub<int>("Quest_UI_OnQuestContextClick", OnQuestContextClicked);
        }   
        else
        {
            EventBus.UnSub("Quest_UI_OnQuestAccept", OnQuestAccept);
            EventBus.UnSub<int>("Quest_UI_OnQuestContextClick", OnQuestContextClicked);
        } 
    }

    private void OnQuestContextClicked(int questId)
    {
        _focusedQuestId = questId;
        OnUpdatePreview?.Invoke(_focusedQuestId);
    }

    private void OnQuestAccept()
    {
        if(_system.IsFinishedQuest(_focusedQuestId)) 
            _focusedQuestId = _system.GetMostSelectableQuestId();

        if (!_system.IsCurrentMainQuest(_focusedQuestId))
        {
            OnSetMainQuest?.Invoke(_focusedQuestId);
        }
        else
        {
            OnSetMainQuest?.Invoke(-1);
            OnUpdatePreview?.Invoke(_focusedQuestId);
        }
    }
}