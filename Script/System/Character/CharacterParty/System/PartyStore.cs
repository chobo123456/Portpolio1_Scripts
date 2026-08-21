using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PartySaveInfo
{
    public int recentIndex;
    public List<int> indexs;
    public List<int> characterIds;
}

public class PartyStore
{
    private int recentIndex;
    private Dictionary<int, int> runtimePartyInfo = new();
    private Save<PartySaveInfo> save;

    public PartyStore()
    {
        save = new("Player/Party", "PartyData", 
            () => runtimePartyInfo.Count > 0 && !GameState.IsTutorial(), 
            (data) => {
                bool isLoadAble = data.indexs.Count > 0 && data.characterIds.Count > 0;

                int recentStateInt = PlayerPref.GetPlayerPref<int>("RecentSaveState");
                if(recentStateInt == 0 || recentStateInt > 2) return false;

                return (SaveState)recentStateInt == SaveState.Saveable && isLoadAble;
            });
            
        if (HasData()) LoadPartyInfo();
        else FirstSetting();
    }

    public void OnEnable()
    {
        EventBus.Sub("TrySave", SavePartyInfo);
        EventBus.Sub_Func<Dictionary<int, int>>("CharacterPartySaveInfo_GetPartyInfo", GetPartyInfo);
    }

    public void OnDisable()
    {
        EventBus.UnSub("TrySave", SavePartyInfo);
        EventBus.UnSub_Func<Dictionary<int, int>>("CharacterPartySaveInfo_GetPartyInfo", GetPartyInfo);

        SavePartyInfo();
    }

    public void AddData(int index, int characterId)
    {
        runtimePartyInfo[index] = characterId;
    }

    public void RemoveData(int index)
    {
        runtimePartyInfo.Remove(index);
    }

    public void SavePartyInfo()
    {
        runtimePartyInfo = runtimePartyInfo.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);

        PartySaveInfo newpartyInfo = new();
        newpartyInfo.indexs = new();
        newpartyInfo.characterIds = new();

        foreach(var partyInfo in runtimePartyInfo)
        {
            int index = partyInfo.Key;
            int characterId = partyInfo.Value;

            if (characterId <= 0) continue;

            newpartyInfo.indexs.Add(index);//인덱스값을 넣음
            newpartyInfo.characterIds.Add(characterId);//캐릭터 아이디 값을 넣음
        }

        newpartyInfo.recentIndex = recentIndex;
        save.Saving(newpartyInfo); // 리스트가 0일시 다시 불러올때 이전 데이터 참조 하도록 설계시도
    }
    
    private void LoadPartyInfo()
    {
        PartySaveInfo savedInfo = save.savedData;

        runtimePartyInfo.Clear();

        for(int i = 0; i < savedInfo.indexs.Count; i++)
            runtimePartyInfo.Add(savedInfo.indexs[i], savedInfo.characterIds[i]);

        recentIndex = savedInfo.recentIndex;
    }

    private void FirstSetting()
    {
        List<int> characters = EventBus.Invoke_Func<List<int>>("GetCharacterIds");

        if(characters.Count > 0)
        {
            AddData(0, characters[0]);
            SavePartyInfo();
        }
    }

    public void SetRecentIndex(int recentIndex)
    {
        this.recentIndex = recentIndex;
    }
    
    public bool HasData() => save.IsExist();
    public int GetRecentIndex() => recentIndex;
    public Dictionary<int, int> GetPartyInfo() => runtimePartyInfo;
}