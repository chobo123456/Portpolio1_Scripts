using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct QuestProgressState
{
    public int questId;
    public QuestState State;
    public QuestData questData;
}

public class QuestSystem
{
    private QuestStore _store;

    public void InjectParameter(QuestStore store)
    {
        _store = store;
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
        if (isSubscribe)
        {
            EventBus.Sub_Func<int, bool>("QuestSystem_IsQuestFinish", IsFinishedQuest);
        }
        else
        {
            EventBus.UnSub_Func<int, bool>("QuestSystem_IsQuestFinish", IsFinishedQuest);
        }
    }
    
    public void SetMainQuest(int questId)
    {
        _store.SetMainQuest(questId);
        InvokeEvent(questId);
    }

    private void InvokeEvent(int questId)
    {
        if(questId <= 0) return;

        QuestData questData = DataLoader.GetData<QuestData>(DataType.Quest, questId);
        string eventName = questData.eventName;

        if(!string.IsNullOrEmpty(eventName))
        {
            EventBus.Invoke(eventName);
        }
    }
    
    public QuestProgressState TryFinishQuest(QuestType questType, int inputId)
    {
        int mainQuestId = _store.GetMainQuest();
        if (mainQuestId <= 0)
        {
            return new QuestProgressState{
                State = QuestState.None,
            };
        }
        
        if (IsMatchingTarget(mainQuestId, questType, inputId))
        {
            _store.SetQuestProgress(mainQuestId);
        }

        if(IsFinishedQuest(mainQuestId))
        {
            _store.SetFinish(mainQuestId);

            return new QuestProgressState{
                questId = mainQuestId,
                State = QuestState.Finished,
                questData = DataLoader.GetData<QuestData>(DataType.Quest, mainQuestId),
            };
        }
        
        return new QuestProgressState{
            questId = mainQuestId,
            State = QuestState.InProgress,
        };
    }

    public QuestPreviewPayload Getpayload(int questId)
    {
        bool isDrawable = questId > 0;

        return new QuestPreviewPayload
        {
            isDrawable = isDrawable,
            isMainQuest = _store.GetMainQuest() == questId,
            questName = isDrawable ? GetQuestName(questId) : "",
            questDiscription = isDrawable ? GetQuestDiscription(questId) : "",
            questProgress = isDrawable ? GetQuestProgresses(questId) : 0f,
            maxQuestProgress = isDrawable ? GetMaxQuestProgress(questId) : 0f,
        };
    }

    public QuestHUDPayload GetHUDPayload(int questId)
    {
        bool isDrawable = questId > 0;

        return new QuestHUDPayload
        {
            isDrawable = isDrawable,
            questName = isDrawable ? GetQuestName(questId) : "",
            questHUDDiscription = isDrawable ? GetQuestHUDDiscription(questId) : "",
            questProgress = isDrawable ? GetQuestProgresses(questId) : 0f,
            maxQuestProgress = isDrawable ? GetMaxQuestProgress(questId) : 0f,
        };
    }

    public QuestPointPayload GetPointPayload(int questId)
    {
        bool isDrawable = IsDrawablePoint(questId);

        return new QuestPointPayload{
            isDrawable = isDrawable,
            questPoint = isDrawable ? GetQuestPoint(questId) : Vector3.zero
        };
    }

    public QuestWayTrackerPayload GetWayTrackPayload(int questId)
    {
        bool isTrackable = questId > 0 && (GetQuestType(questId) == QuestType.WayPoint);
        return new QuestWayTrackerPayload
        {
            isTrackable = isTrackable,
            trackPoint  = isTrackable ? GetQuestPoint(questId) : Vector2.zero,
        };
    }

    public QuestMinimapPointPayload GetMinimapPayload(int questId)
    {
        bool isDrawable = questId > 0 && (GetQuestType(questId) != QuestType.Craft && GetQuestType(questId) != QuestType.DefeatBoss);
        return new QuestMinimapPointPayload
        {
            isDrawable = isDrawable,
            questPoint  = isDrawable ? GetQuestPoint(questId) : Vector2.zero,
        };
    }

    public WalkieTalkiePayload GetQuestWalkieTalkiePayload(int questId)
    {
        bool canPlay = questId > 0 && GetQuestWalkieTalkieId(questId) > 0;

        return new WalkieTalkiePayload
        {
            canPlay = canPlay,
            walkieTalkieData  = canPlay ? GetQuestWalkieTalkieData(GetQuestWalkieTalkieId(questId)) : null,
        };
    }
    
    private WalkieTalkieData GetQuestWalkieTalkieData(int walkieTalkieId)
    {
        return DataLoader.GetData<WalkieTalkieData>(DataType.WalkieTalkie, walkieTalkieId);
    }

    private QuestType GetQuestType(int questId)
    {
        return DataLoader.GetData<QuestData>(DataType.Quest, questId).questType;
    }

    private Vector3 GetQuestPoint(int questId)
    {
        return DataLoader.GetData<QuestData>(DataType.Quest, questId).questPoint;
    }
    
    private string GetQuestName(int questId)
    {
        return DataLoader.GetData<QuestData>(DataType.Quest, questId).questName;
    }

    private string GetQuestDiscription(int questId)
    {
        return DataLoader.GetData<QuestData>(DataType.Quest, questId).questDiscription;
    }

    private string GetQuestHUDDiscription(int questId)
    {
        return DataLoader.GetData<QuestData>(DataType.Quest, questId).questHUDDiscription;
    }

    public int GetQuestTutorialId(int questId)
    {
        if(questId <= 0) return -1;
        
        QuestData questData = DataLoader.GetData<QuestData>(DataType.Quest, questId);
        return questData.tutorial_Id;
    }

    private int GetQuestWalkieTalkieId(int questId)
    {
        int walkieTalkieId = DataLoader.GetData<QuestData>(DataType.Quest, questId).walkieTalkieId;
        return walkieTalkieId;
    }

    public int GetMostSelectableQuestId()
    {
        List<int> quests = GetUnFinishedQuests();
        int mainQuestId = _store.GetMainQuest();

        if (mainQuestId > 0)
        {
            bool isFinish = IsFinishedQuest(mainQuestId);

            if (!isFinish) return mainQuestId;
            else if(isFinish && quests.Count > 0)
            {
                if(mainQuestId != quests[0])
                {
                    return quests[0];
                }
            } 
        }
        else
        {
            if(quests.Count > 0) 
            {
                return quests[0];
            }
        }

        return -1;
    }

    public float GetQuestProgresses(int questId)
    {
        return _store.GetQuestProgress(questId).progressCount / DataLoader.GetData<QuestData>(DataType.Quest, questId).questProgress;
    }

    public float GetMaxQuestProgress(int questId)
    {
        return DataLoader.GetData<QuestData>(DataType.Quest, questId).questProgress;
    }

    public bool IsFinishedQuest(int questId)
    {
        if(_store.GetQuestData().TryGetValue(questId, out var quest))
        {
            return quest.progressCount >= DataLoader.GetData<QuestData>(DataType.Quest, questId).questProgress;
        }

        return false;
    }

    public bool IsCurrentMainQuest(int questId)
    {
        int mainQuestId = _store.GetMainQuest();

        return mainQuestId == questId;
    }

    private bool IsDrawablePoint(int questId)
    {
        if(questId <= 0) return false;

        QuestData questData = DataLoader.GetData<QuestData>(DataType.Quest, questId);

        return questData.questType != QuestType.Craft &&
            questData.questType != QuestType.Act && 
            questData.questType != QuestType.DefeatBoss;
    }

    private bool IsMatchingTarget(int questId, QuestType questType, int inputId)
    {
        QuestData questData = DataLoader.GetData<QuestData>(DataType.Quest, questId);

        return questData.targetId == inputId && questData.questType == questType;
    }

    public List<int> GetUnFinishedQuests()
    {
        List<int> unFinishedQuestList = new();

        for(int i = 0; i < _store.GetQuest().Count; i++)
        {
            int questId = _store.GetQuest()[i];
            if(!IsFinishedQuest(questId))
            {
                unFinishedQuestList.Add(questId);
            }
        }

        return unFinishedQuestList;
    }
}