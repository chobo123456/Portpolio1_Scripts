using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct QuestProgresses
{
    public List<QuestProgress> progress;
}

[System.Serializable]
public struct QuestProgress
{
    public int questId;
    public int progressCount;
    public bool isFinished;
}

[System.Serializable]
public struct MainQuest
{
    public int mainQuestId;
}

//Initialzer
public class QuestStore
{
    private Save<QuestProgresses> quests;
    private Save<MainQuest> mainQuestSave;
    private Dictionary<int, QuestProgress> questData = new();
    private int mainQuestId;


    public QuestStore()
    {
        Initialize_Quests();
        Initialize_MainQuest();
    }

    public void OnEnable()
    {
        EventSubscribe(true);
    }

    public void OnDisable()
    {
        EventSubscribe(false);
    }

    private void EventSubscribe(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub_Func<int, bool>("QuestSystem_HasQuest", HasQuest);
            
            EventBus.Sub_Func<int, QuestProgress>("QuestSystem_GetQuestProgress", GetQuestProgress);
            EventBus.Sub_Func<int>("QuestSystem_GetMainQuest", GetMainQuest);
        }  
        else
        {
            
            EventBus.UnSub_Func<int, bool>("QuestSystem_HasQuest", HasQuest);
            EventBus.UnSub_Func<int, QuestProgress>("QuestSystem_GetQuestProgress", GetQuestProgress);
            EventBus.UnSub_Func<int>("QuestSystem_GetMainQuest", GetMainQuest);
        } 
    }
        
    public void Initialize_Quests()
    {
        quests = new(
            "Player/Quest", 
            "QuestData", 
            () => !GameState.IsUnsaveAble(),
            (data) => {
                int recentStateInt = PlayerPref.GetPlayerPref<int>("RecentSaveState");
                if(recentStateInt == 0 || recentStateInt > 2) return false;

                return (SaveState)recentStateInt == SaveState.Saveable;
            });
            
        if(quests.IsExist()) Load_Quests();
        else SetQuest(1);
    } 

    public void Save_Quests()
    {
        QuestProgresses progresses = new();
        progresses.progress = new();

        foreach(var map in questData)
        {
            QuestProgress progressData = map.Value;

            if(progressData.questId <= 0) continue;
            
            progresses.progress.Add(progressData);
        }

        quests.Saving(progresses);        
    }

    public void Load_Quests()
    {
        QuestProgresses progresses = quests.savedData;

        foreach(var progress in progresses.progress)
        {
            if(!questData.ContainsKey(progress.questId))
            {
                if(progress.questId <= 0) continue;

                questData.Add(progress.questId, new QuestProgress
                {
                    questId         = progress.questId,
                    progressCount   = progress.progressCount,
                });
            }
            else
            {
                if(progress.questId <= 0) continue;

                questData[progress.questId] = new QuestProgress
                {
                    questId         = progress.questId,
                    progressCount   = progress.progressCount,
                };
            }
        }
    }
    
    public void Initialize_MainQuest()
    {
        mainQuestSave = new(
            "Player/Quest", 
            "mainQuest", 
            () => !GameState.IsUnsaveAble(),
            (data) => {
                int recentStateInt = PlayerPref.GetPlayerPref<int>("RecentSaveState");
                if(recentStateInt == 0 || recentStateInt > 2) return false;

                return (SaveState)recentStateInt == SaveState.Saveable;
            });
            
        if(mainQuestSave.IsExist()) Load_Main();
        else SetMainQuest(1);
    }   

    public void Save_Main()
    {
        MainQuest newMainQuestSave = new MainQuest { mainQuestId = mainQuestId };

        mainQuestSave.Saving(newMainQuestSave);        
    }

    public void Load_Main()
    {
        mainQuestId = mainQuestSave.savedData.mainQuestId;
    }

    public void SetQuest(int questId)
    {
        if(!questData.ContainsKey(questId)) 
            questData.Add(questId, new QuestProgress{ questId = questId });

        Save_Quests();
    }

    public void SetQuestProgress(int questId)
    {
        if(questData.TryGetValue(questId, out QuestProgress progressValue))
        {
            progressValue.progressCount += 1;
            questData[questId] = progressValue;
            Save_Quests();
        }
    }

    public void SetFinish(int questId)
    {
        if(questData.TryGetValue(questId, out QuestProgress progressValue))
        {
            progressValue.isFinished = true;
            questData[questId] = progressValue;
            Save_Quests();
        }
    }

    public QuestProgress GetQuestProgress(int questId)
    {
        if(questData.TryGetValue(questId, out QuestProgress progressValue))
        {
            return progressValue;            
        }

        return default;
    }

    public List<int> GetQuest()
    {
        List<int> ids = new();

        foreach(var quest in questData)
        {
            ids.Add(quest.Key);
        }

        return ids;
    }

    public bool HasQuest(int questId)
    {
        return questData.ContainsKey(questId);
    }

    public Dictionary<int, QuestProgress> GetQuestData() => questData;
    
    public void SetMainQuest(int id)
    {
        mainQuestId = id;

        Save_Main();
    }

    public int GetMainQuest()
    {
        return mainQuestId;
    }
}  