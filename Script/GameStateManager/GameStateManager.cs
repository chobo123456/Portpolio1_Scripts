using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    private List<MonoBehaviour> enemyDetects = new();
    private bool isMoved = false;
    
    private void OnEnable()
    {
        EventSub();
        SetGameState(GameStateType.Stop, GameEnableTimeSet.True);
    }

    private void EventSub()
    {
        SubscribeEvent(true);
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("SetSaveStateUnSaveAble", SetUnsaveAble);
            EventBus.Sub("SetSaveStateSaveAble", SetSaveAble);
            EventBus.Sub<GameStateType, GameEnableTimeSet>("SetGameState", SetGameState);
            EventBus.Sub<MonoBehaviour, int, bool>("EnemyDetect", OnEnemyDetect);
            EventBus.Sub<MonoBehaviour, bool>("EnemyUnDetect",   OnEnemyUnDetect);
        }   
        else
        {
            EventBus.UnSub("SetSaveStateUnSaveAble", SetUnsaveAble);
            EventBus.UnSub("SetSaveStateSaveAble", SetSaveAble);
            EventBus.UnSub<GameStateType, GameEnableTimeSet>("SetGameState", SetGameState);
            EventBus.UnSub<MonoBehaviour, int, bool>("EnemyDetect",   OnEnemyDetect);
            EventBus.UnSub<MonoBehaviour, bool>("EnemyUnDetect", OnEnemyUnDetect); 
        } 
    }

    private void SetGameState(GameStateType state, GameEnableTimeSet enableTimeSet)
    {
        GameState.SetGameState(state, enableTimeSet);
    }
    
    private void OnDisable()
    {
        SubscribeEvent(false);

        SetGameState(GameStateType.Run, GameEnableTimeSet.True);

        enemyDetects.Clear();
    }

    private void SetUnsaveAble()
    {
        GameState.SetSaveState(SaveState.UnsaveAble);
    }

    private void SetSaveAble()
    {
        GameState.SetSaveState(SaveState.Saveable);
    }

    private void OnEnemyDetect(MonoBehaviour mono, int enemyId, bool isPlayBGM)
    {
        if(!enemyDetects.Contains(mono))
            enemyDetects.Add(mono);

        if(enemyDetects.Count > 0 && !isMoved)
        {
            isMoved = false;

            EventBus.Invoke<bool>("OnBattle", true);
            EventBus.Invoke<bool>("Lock_All_UI", true);
            if(isPlayBGM) EventBus.Invoke("OnBattleBGM");
        }
    }

    private void OnEnemyUnDetect(MonoBehaviour mono, bool isPlayBGM)
    {
        if(enemyDetects.Contains(mono))
            enemyDetects.Remove(mono);

        if(enemyDetects.Count <= 0)
        {
            isMoved = false;

            EventBus.Invoke<bool>("Lock_All_UI", false);
            EventBus.Invoke<bool>("OnBattle", false);
            if(isPlayBGM) EventBus.Invoke("OnFieldBGM");
        }
    }
}

public enum GameStateType
{
    Tutorial,
    Run,
    BossBattle,
    Stop,
}

public enum GameEnableTimeSet
{
    True,
    False
}

public enum SaveState
{
    Saveable = 1,
    UnsaveAble = 2,
}

public static class GameState
{
    private static GameStateType currentGameState;
    private static SaveState currentSaveState;

    public static void SetGameState(GameStateType type, GameEnableTimeSet timeState = GameEnableTimeSet.True)
    {
        currentGameState = type;

        if(timeState == GameEnableTimeSet.True)
            SetTime();
    }

    public static void SetSaveState(SaveState state)
    {
        currentSaveState = state;

        PlayerPref.SetPlayerPref<int>("RecentSaveState", (int)state);
    }

    private static void SetTime()
    {
        if(currentGameState == GameStateType.Run || currentGameState == GameStateType.BossBattle)
        {
            Time.timeScale = 1;
        }
        else if(currentGameState == GameStateType.Stop)
        {
            Time.timeScale = 0;
        }
    }
    
    public static bool IsBossFight()
    {
        return currentGameState == GameStateType.BossBattle;
    }

    public static bool IsActive()
    {
        return currentGameState == GameStateType.Run || currentGameState == GameStateType.BossBattle;
    }

    public static bool IsTutorial()
    {
        return currentGameState == GameStateType.Tutorial;
    }

    public static bool IsBossFighting()
    {
        return currentGameState == GameStateType.BossBattle;
    }

    public static bool IsUnsaveAble()
    {
        return currentSaveState == SaveState.UnsaveAble;
    }
}
