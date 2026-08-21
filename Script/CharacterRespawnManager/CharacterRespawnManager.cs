using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public partial class CharacterRespawnManager : MonoBehaviour
{
    private List<Transform> spawnPoints = new();

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Initialize_List();
        Initialize_Event();
    }

    private void Initialize_List()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            spawnPoints.Add(child);
        }
    }

    private void Initialize_Event()
    {
        EventBus.Sub<bool, bool>("TryRespawn", RespawnCharacter);
    }

    private void OnDisable()
    {
        EventBus.UnSub<bool, bool>("TryRespawn", RespawnCharacter);
    }
}

public partial class CharacterRespawnManager : MonoBehaviour
{
    private void RespawnCharacter(bool needPanel, bool needForce)
    {
        this.RunRoutine(StartRespawn(needPanel, needForce));
    }

    IEnumerator StartRespawn(bool needPanel, bool needForce = false)
    {
        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Stop, GameEnableTimeSet.True);

        CursorManager.CursorActive(false);  
        EventBus.Invoke("OnRespawnProcess");

        if(needForce) EventBus.Invoke("ActiveFadePanelForce");
        if(!needForce && needPanel) EventBus.Invoke<float>("LoadOut", 0.5f);

        yield return YieldUtil.WaitForSecondsRealtime(0.5f);

        FindNearSpawnPointAndSetSpawnPoint();
        ReviveCharacters();

        EventBus.Invoke("LoadParty");
        EventBus.Invoke("CameraUpdatePosition");

        yield return YieldUtil.WaitForSecondsRealtime(2f);

        if(needPanel) EventBus.Invoke<float>("LoadIn", 0.5f);
        EventBus.Invoke<GameStateType, GameEnableTimeSet>("SetGameState",  GameStateType.Run, GameEnableTimeSet.True);
    }

    private void FindNearSpawnPointAndSetSpawnPoint()
    {
        Vector3 currentPlayerPos = PlayerMatch.GetPlayerPos();
        Transform currentNearSpawnPoint = spawnPoints[0];

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            float currentNearSpawnPoint_Distance = (currentPlayerPos - currentNearSpawnPoint.position).magnitude;
            float otherSpawnPoint_Distance = (currentPlayerPos - spawnPoints[i].position).magnitude;

            if (otherSpawnPoint_Distance < currentNearSpawnPoint_Distance)
            {
                currentNearSpawnPoint = spawnPoints[i];
            }
        }

        PlayerMatch.SetPlayerPos(currentNearSpawnPoint.position);
    }

    private void ReviveCharacters()
    {
        Dictionary<int, int> partyInfos = 
            EventBus.Invoke_Func<Dictionary<int, int>>("CharacterPartySaveInfo_GetPartyInfo");

        foreach(var info in partyInfos)
            EventBus.Invoke<int>("OnCharacterLive", info.Value);

        EventBus.Invoke("CharacterReloadHp");
    }
}