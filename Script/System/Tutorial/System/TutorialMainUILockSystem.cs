using UnityEngine;
using System.Collections.Generic;

public enum UILockState { NoChange, UnLock, Lock }
public enum UIVisibleState { NoChange, Show, Invisible }

[System.Serializable]
public struct UIEntryState
{
    public UILockState lockState;
    public UIVisibleState visibleState;
}

public class TutorialMainUILockSystem
{
    private Dictionary<UIType, UIEntryState> _uiConfigs = new();
    private Dictionary<UIType, UIEntryState> _noChangedConfigList = new();

    private int logValue = 1;

    public void LoadState(List<int> savedLockState)
    {
        if(savedLockState.Count <= 0)
        {
            StartTutorial(1);
            return;    
        }

        for(int i = 0; i < savedLockState.Count; i++)
        {
            TutorialUIConfig[] config = GetConfig(savedLockState[i]);
            AddRecentConfig(config);
            UnlockConfig();
        }
    }

    public void StartTutorial(int tutorialId)
    {
        TutorialUIConfig[] config = GetConfig(tutorialId);
        List<UIType> recentChangedConfigList = AddRecentConfig(config);
        ApplyConfig(recentChangedConfigList);
    }

    public void EndTutorial()
    {
        UnlockConfig();
    }

    private void ApplyConfig(List<UIType> recentChangedConfigType)
    {
        foreach(UIType uiType in System.Enum.GetValues(typeof(UIType)))
        {
            if(_uiConfigs.TryGetValue(uiType, out UIEntryState state))
            {
                if(recentChangedConfigType.Contains(uiType))
                {
                   if(state.visibleState == UIVisibleState.Show)
                    {
                        ShowUI(uiType, true);
                    }
                    else if(state.visibleState == UIVisibleState.Invisible)
                    {
                        ShowUI(uiType, false);
                    }   

                    if(state.lockState == UILockState.UnLock)
                    {
                        LockUI(uiType, false);
                        continue;
                    }
                }

                LockUI(uiType, true);
            }
            else
            {
                ShowUI(uiType, false);
                LockUI(uiType, true);
            }
        }
    }

    private void UnlockConfig()
    {
        foreach(var config in _uiConfigs)
        {
            SyncLockWithVisibility(config.Key, config.Value.visibleState);
        }

        foreach(var nochangedConfig in _noChangedConfigList)
        {
            SyncLockWithVisibility(nochangedConfig.Key, nochangedConfig.Value.visibleState);
        }

        logValue++;
        _noChangedConfigList.Clear();
    }

    private void SyncLockWithVisibility(UIType uiType, UIVisibleState visibleState)
    {
        if(visibleState == UIVisibleState.Show)
        {
            LockUI(uiType, false);
            ShowUI(uiType, true);
        }
        else if(visibleState == UIVisibleState.Invisible)
        {
            LockUI(uiType, true);
            ShowUI(uiType, false);
        }
    }

    private void LockUI(UIType type, bool isLock)
    {
        EventBus.Invoke<UIType, bool>("UILock", type, isLock);
    }

    private void ShowUI(UIType type, bool isShow)
    {
        EventBus.Invoke<UIType, bool>("UIShow", type, isShow);
    }

    private List<UIType> AddRecentConfig(TutorialUIConfig[] configs)
    {
        List<UIType> recentChangedConfigType = new();

        //현재 Configs리스트 재정의
        for(int i = 0; i < configs.Length; i++)
        {
            TutorialUIConfig config = configs[i];
            _uiConfigs[config.uiType] = config.state;

            recentChangedConfigType.Add(config.uiType);
        }

        foreach(UIType uiType in System.Enum.GetValues(typeof(UIType)))
        {
            if(_uiConfigs.TryGetValue(uiType, out UIEntryState state))
            {
                //현재 바뀌지않았다면?
                if(!recentChangedConfigType.Contains(uiType))
                {
                    //이번스텝에 바뀌지 않음 리스트에 할당
                    if(!_noChangedConfigList.ContainsKey(uiType))
                        _noChangedConfigList.Add(uiType, state);
                }
            }
        }

        return recentChangedConfigType;
    }

    private TutorialUIConfig[] GetConfig(int tutorialId)
    {
        return DataLoader.GetData<TutorialData>(DataType.Tutorial, tutorialId).config;
    }
}