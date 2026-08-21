using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public enum TutorialConditionEventType
{
    None,
    Int,
    IntInt,
    SlotEnum,
    UIOpen,
    InventoryType,
}

public partial class TutorialSequencer
{
    private bool _isTutorialStepFinish = false;
    private MonoBehaviour _mono;

    public TutorialSequencer(MonoBehaviour mono)
    {
        _mono = mono;
    }

    public void StartSequence(int tutorialId)
    {
        if(tutorialId <= 0) return;

        TutorialData config = DataLoader.GetData<TutorialData>(DataType.Tutorial, tutorialId);
        _mono.RunRoutine(Loop(config));
    }

    private IEnumerator Loop(TutorialData config)
    {
        yield return null;

        EventBus.Invoke("PlayerETCDataSave");

        if(config.tutorialType == TutorialType.Banner)
        {
            EventBus.Invoke<int, string>("Banner_UI_Start", config.tutorialId, config.bannerConfig.context);
        }
        else if(config.tutorialType == TutorialType.Spotlight)
        {
            TutorialStart();
            
            bool isInvoked = false;
            int index = 0;

            while(index < config.spotlightConfig.steps.Length)
            {
                SpotlightTutorialStepData stepData = config.spotlightConfig.steps[index];

                TutorialStartEvent startEvent = stepData.startEvent;
                TutorialCondition condition = stepData.condition;
                SpotlightTargetRef reference = stepData.targetRef;
                TutorialFinishEvent[] finishEvent = stepData.finishEvent;

                if(!isInvoked)
                {
                    StartTutorialStepEventInvoke(startEvent);
                    SubscribeConditionEvent(condition);
                    EventBus.Invoke<RectTransform>("FocusStart", FindRect(reference));
                    isInvoked = true;
                }

                if(_isTutorialStepFinish)
                {
                    EndTutorialStepEventInvoke(finishEvent);

                    EventBus.Invoke("FocusEnd");
                    index++;
                    isInvoked = false;
                    _isTutorialStepFinish = false;
                    continue;
                }

                yield return null;
            }

            TutorialEnd(config);
        }
    }

    private void TutorialStart()
    {
        EventBus.Invoke("SetSaveStateUnSaveAble");

        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState", GameStateType.Tutorial, GameEnableTimeSet.False);
        EventBus.Invoke("CharacterStateAbort");
    }

    private void TutorialEnd(TutorialData config)
    {
        _isTutorialStepFinish = false;

        EventBus.Invoke<int>("EndTutorial", config.tutorialId);

        ReleaseAll_TutorialStep_FinishEvent(config.spotlightConfig.steps);

        EventBus.Invoke("SetSaveStateSaveAble");
        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState", config.tutorialEndGameState, config.enableTime);

        EventBus.Invoke("TrySave");
        if(config.isNeedInvokeQuestEvent) EventBus.Invoke<QuestType, int>("QuestManager_OnAskQuestFinish", QuestType.Act, 0);
    }
}

public partial class TutorialSequencer
{
    public RectTransform FindRect(SpotlightTargetRef reference)
    {
        if(reference.spotlightTarget == SpotlightTarget.HUD)
        {
            return EventBus.Invoke_Func<UIType, RectTransform>("GetUIIconRect", reference.spotLightUIType);
        }
        else if(reference.spotlightTarget == SpotlightTarget.UI)
        {
            if(reference.uiName == UIRectName.Inventory_UI_InventorySlotRect)
                return EventBus.Invoke_Func<int, RectTransform>("Inventory_UI_InventorySlotRect", reference.spotLightValue);
            else if(reference.uiName == UIRectName.CharacterPartyUI_CharacterIcon)
                return EventBus.Invoke_Func<int, RectTransform>("Party_UI_CharacterIconRect", reference.spotLightValue);
            else if(reference.uiName == UIRectName.CharacterGrowthUI_CharacterIcon)
                return EventBus.Invoke_Func<int, RectTransform>("Growth_UI_CharacterIconRect", reference.spotLightValue);

            return EventBus.Invoke_Func<UIType, UIRectName, RectTransform>("Get_UI_RectTransform", reference.spotLightUIType, reference.uiName);
        }

        return null;
    }
}

public partial class TutorialSequencer
{
    private TutorialConditionEventType GetCurrentEventType(TutorialEvent eventName)
    {
        switch(eventName)
        {
            case TutorialEvent.On_Craft_UI_CraftClick:
            case TutorialEvent.On_Inventory_UI_ItemUseClick:
            case TutorialEvent.On_Party_UI_ClickCharacterSettingFinish:
            case TutorialEvent.On_Growth_UI_ClickedExpItem:
            case TutorialEvent.On_LevelUpgrade:
                return TutorialConditionEventType.None;

            case TutorialEvent.On_Craft_UI_RecipeClick:
            case TutorialEvent.On_Inventory_UI_ItemIconClick:
            case TutorialEvent.On_Party_UI_ClickCharacterIcon:
            case TutorialEvent.On_Growth_UI_ClickCharacterIcon:
                return TutorialConditionEventType.Int;

            case TutorialEvent.On_UseItem:
                return TutorialConditionEventType.IntInt;

            case TutorialEvent.On_Party_UI_ClickCharacterSlot:
                return TutorialConditionEventType.SlotEnum;

            case TutorialEvent.On_Input_UI:
                return TutorialConditionEventType.UIOpen;

            case TutorialEvent.On_Inventory_UI_CategoryClick:
                return TutorialConditionEventType.InventoryType;

            default:
                return TutorialConditionEventType.None;
        }
    }

    private void SubscribeConditionEvent(TutorialCondition condition)
    {
        string eventNameArg = $"{condition.eventName}";

        TutorialConditionEventType eventType = GetCurrentEventType(condition.eventName);

        switch(eventType)
        {
            case TutorialConditionEventType.None:
                EventSubscribe_None(eventNameArg);
                break;

            case TutorialConditionEventType.Int:
                EventSubscribe_Int(condition.conditionTargetInt1, eventNameArg);
                break;

            case TutorialConditionEventType.IntInt:
                EventSubscribe_IntInt(condition.conditionTargetInt1, condition.conditionTargetInt2, eventNameArg);
                break;  

            case TutorialConditionEventType.SlotEnum:
                EventSubscribe_SlotIndex(condition.slotValue, eventNameArg);
                break;

            case TutorialConditionEventType.UIOpen:
                EventSubscribe_UIOpen(condition.conditionTargetUIInteract, eventNameArg);
                break;

            case TutorialConditionEventType.InventoryType:
                EventSubscribe_InventoryType(condition.conditionTargetInventoryTarget, eventNameArg);
                break;
        }
    }

    private void EventSubscribe_None(string eventName)
    {
        Action inputAction = null;

        inputAction = () =>
        {
            _isTutorialStepFinish = true;
            EventBus.UnSub(eventName, inputAction);
        };

        EventBus.Sub(eventName, inputAction);
    }

    private void EventSubscribe_Int(int condition, string eventName)
    {
        Action<int> inputAction = null;

        inputAction = (value) =>
        {
            if(value == condition)
            {
                _isTutorialStepFinish = true;
                EventBus.UnSub<int>(eventName, inputAction);
            }
        };

        EventBus.Sub<int>(eventName, inputAction);
    }

    private void EventSubscribe_IntInt(int condition1, int condition2, string eventName)
    {
        Action<int, int> inputAction = null;

        inputAction = (value1, value2) =>
        {
            if(value1 == condition1 && value2 == condition2)
            {
                _isTutorialStepFinish = true;
                EventBus.UnSub<int, int>(eventName, inputAction);
            }
        };

        EventBus.Sub<int, int>(eventName, inputAction);
    }

    private void EventSubscribe_SlotIndex(SlotEnum slotIndex, string eventName)
    {
        Action<int> inputAction = null;

        inputAction = (value) =>
        {
            if(value == (int)slotIndex)
            {
                _isTutorialStepFinish = true;
                EventBus.UnSub<int>(eventName, inputAction);
            }
        };

        EventBus.Sub<int>(eventName, inputAction);
    }

    private void EventSubscribe_UIOpen(UIType condition, string eventName)
    {
        Action<UIType> inputAction = null;

            inputAction = (type) =>
            {
                if(type == condition)
                {
                    _isTutorialStepFinish = true;
                    EventBus.UnSub<UIType>(eventName, inputAction);
                }
            };

        EventBus.Sub<UIType>(eventName, inputAction);
    }

    private void EventSubscribe_InventoryType(InventoryType condition, string eventName)
    {
        Action<InventoryType> inputAction = null;

        inputAction = (inventoryCategory) =>
        {
            if(inventoryCategory == condition)
            {
                _isTutorialStepFinish = true;
                EventBus.UnSub<InventoryType>(eventName, inputAction);
            }
        };

        EventBus.Sub<InventoryType>(eventName, inputAction);
    }
}

public partial class TutorialSequencer
{
    private void StartTutorialStepEventInvoke(TutorialStartEvent startEventData)
    {
        TutorialStartEventName startEventName = startEventData.eventName;
        string eventName = $"{startEventName}";

        int value1 = startEventData.value1;
        int value2 = startEventData.value2;

        switch(startEventName) 
        {
            case TutorialStartEventName.None:
                break;

            case TutorialStartEventName.Inventory_System_TryReceiveItem:
                EventBus.Invoke<int, int>(eventName, value1, value2);
                break;
        }
    }

    private void EndTutorialStepEventInvoke(TutorialFinishEvent[] finishEventDatas)
    {
        for(int i = 0; i < finishEventDatas.Length; i++)
        {
            TutorialFinishEvent finishEvent = finishEventDatas[i];
            InvokeTutorialFinishEvent(finishEvent);
        }   
    }

    private void ReleaseAll_TutorialStep_FinishEvent(SpotlightTutorialStepData[] steps)
    {
        for(int i = 0; i < steps.Length; i++)
        {
            SpotlightTutorialStepData step = steps[i];

            for(int j = 0; j < step.finishEvent.Length; j++)
            {
                TutorialFinishEvent finishEvent = step.finishEvent[j];
                InvokeTutorialFinishEvent(finishEvent, true);
            }  
        }
    }

    private void InvokeTutorialFinishEvent(TutorialFinishEvent finishEvent, bool isRelease = false)
    {
        TutorialFinishEventName finishEventName = finishEvent.eventName;
        string eventName = $"{finishEventName}";
        UIType uiType = finishEvent.uiType;
        bool booleanValue = finishEvent.isLock;
        int intValue1 = finishEvent.intValue1;
        SlotEnum slot = finishEvent.slotValue;

        if(isRelease) booleanValue = false;

        switch(finishEventName)
        {
            case TutorialFinishEventName.UILock:
                EventBus.Invoke<UIType, bool>(eventName, uiType, booleanValue);
                break;  

            case TutorialFinishEventName.Craft_UI_Lock_CraftButton:
            case TutorialFinishEventName.Craft_UI_Lock_RecipeButton:
            case TutorialFinishEventName.Inventory_UI_Lock_ItemUsePanel:
            case TutorialFinishEventName.Inventory_UI_Lock_CategoryPanel:
            case TutorialFinishEventName.Party_UI_LockCharacterIconClick:
            case TutorialFinishEventName.Party_UI_LockSlotClick:
                EventBus.Invoke<bool>(eventName, booleanValue);
                break;

            case TutorialFinishEventName.Party_System_SetCheckAcceptedCharacterIdFlag:
            case TutorialFinishEventName.Growth_System_SetCheckAcceptedCharacterIdFlag:
                EventBus.Invoke<bool, int>(eventName, booleanValue, intValue1);
                break;

            case TutorialFinishEventName.Party_System_SetCheckAcceptedSlotFlag:
                EventBus.Invoke<bool, int>(eventName, booleanValue, (int)slot);
                break;
        }
    }
}