using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CharacterManager : MonoBehaviour
{
    private List<int> characterIds = new();
    private CharacterCollection collections;
    private PlayerETCSaver etcSaver;
    private bool isAlreadyFinish = false;
    
    private void OnEnable()
    {
        this.RunRoutine(Booting());
    }   

    IEnumerator Booting()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        collections = new();
        etcSaver    = new();

        collections.SubscribeEvent(true);
        etcSaver.SubscribeEvent(true);

        EventBus.Sub_Func<bool>("FinishLoadCharacterData", IsLoadAll);

        isAlreadyFinish = true;
    }

    private void OnDisable()
    {
        EventBus.UnSub_Func<bool>("FinishLoadCharacterData", IsLoadAll);

        EventBus.Invoke("PlayerETCDataSave");

        collections?.SubscribeEvent(false);
        etcSaver?.SubscribeEvent(false);
    }

    private bool IsLoadAll() => isAlreadyFinish;
}

#region Collections

[System.Serializable]
public class CharacterStatus
{
    public int characterId;
    public int characterLevel;
    public float characterLevelProgress;
    public float curHp; 
}

[System.Serializable]
public class CharactersStatus
{
    public List<CharacterStatus> characters; 
}

public class CharacterCollection
{
    private Dictionary<int, float> currentCharacterLiveInfo = new();
    private CharactersStatus _currentStatus = new();
    private Save<CharactersStatus> _characterStatus;

    public CharacterCollection()
    {
        _characterStatus = new("Character", "Player_CharacterDatas");

        if(!_characterStatus.IsExist()) {

            //기본 지급 캐릭터
            GetNewCharacter(1, true); 
            GetNewCharacter(2, true);
            GetNewCharacter(3, true);
        }
        else
        {
            if(_characterStatus.IsExist())
            {
                _currentStatus = _characterStatus.savedData;
                LoadRecentHp();
            }

            //Exception(예외) 발생시 기본캐릭터 지급만
            if(_currentStatus.characters == null || _currentStatus.characters.Count <= 0)
            {
                //기본 지급 캐릭터
                GetNewCharacter(1, true); 
                GetNewCharacter(2, true);
                GetNewCharacter(3, true);
            }
        }
    }

    public void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<int, float>("OnCharacterHpChanged", OnCharacterHpChanged);
            EventBus.Sub<int>("OnCharacterLive", OnCharacterLive);
            EventBus.Sub_Func<int, bool>("IsCharacterDie", IsCharacterDie);
            EventBus.Sub_Func<int, bool>("IsExistRecentInfo", IsExistLiveInfo);
            EventBus.Sub_Func<int, float>("GetCharacterRecentHp", GetCharacterRecentHp);

            EventBus.Sub<int, bool>("GetNewCharacter", GetNewCharacter);
            EventBus.Sub<int, int, float>("OnCharacterLevelUpgrade", SetCharacterLevelAndProgress);
            EventBus.Sub_Func<int, float>("GetCharacterLevelProgress", GetCharacterLevelProgress);
            EventBus.Sub_Func<int, int>("GetCharacterLevel", GetCharacterLevel);
            EventBus.Sub_Func<List<int>>("GetCharacterIds", GetCharacterIds);
        }   
        else
        {
            EventBus.UnSub<int, float>("OnCharacterHpChanged", OnCharacterHpChanged);
            EventBus.UnSub<int>("OnCharacterLive", OnCharacterLive);
            EventBus.UnSub_Func<int, bool>("IsCharacterDie", IsCharacterDie);
            EventBus.UnSub_Func<int, bool>("IsExistRecentInfo", IsExistLiveInfo);
            EventBus.UnSub_Func<int, float>("GetCharacterRecentHp", GetCharacterRecentHp);

            EventBus.UnSub<int, bool>("GetNewCharacter", GetNewCharacter);
            EventBus.UnSub<int, int, float>("OnCharacterLevelUpgrade", SetCharacterLevelAndProgress);
            EventBus.UnSub_Func<int, float>("GetCharacterLevelProgress", GetCharacterLevelProgress);
            EventBus.UnSub_Func<int, int>("GetCharacterLevel", GetCharacterLevel);
            EventBus.UnSub_Func<List<int>>("GetCharacterIds", GetCharacterIds);
        } 
    }

    #region LiveInfo

    //최근 체력 불러오기
    private void LoadRecentHp()
    {
        for(int i = 0; i < _currentStatus.characters.Count; i++)
        {
            var array = _currentStatus.characters[i];

            int characterId     = array.characterId;
            float recentHp      = array.curHp;

            if(!currentCharacterLiveInfo.ContainsKey(characterId))
                currentCharacterLiveInfo.Add(characterId, recentHp);
        }
    }

    //체력 변경
    private void OnCharacterHpChanged(int characterId, float curHp)
    {
        if(GameState.IsBossFighting()) return;

        if (!currentCharacterLiveInfo.ContainsKey(characterId))
            currentCharacterLiveInfo.Add(characterId, curHp);
        else
            currentCharacterLiveInfo[characterId] = curHp;
        
        SettingCurrentStatusHp(characterId, curHp);
        SaveCharacters();
    }

    //부활
    private void OnCharacterLive(int characterId)
    {
        CharacterData characterData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);
        float maxHp = characterData.levelStep.GetMaxHpUseLevel(GetCharacterLevel(characterId));

        if(!currentCharacterLiveInfo.ContainsKey(characterId))
            currentCharacterLiveInfo.Add(characterId, maxHp);
        else
            currentCharacterLiveInfo[characterId] = maxHp;

        SettingCurrentStatusHp(characterId, maxHp);
        SaveCharacters();
    }

    //status구조체 체력 설정    
    private void SettingCurrentStatusHp(int characterId, float curHp)
    {
        for(int i = 0; i < _currentStatus.characters.Count; i++)
        {
            var characterStatus = _currentStatus.characters[i];

            if(characterStatus.characterId == characterId)
            {
                characterStatus.curHp = curHp;
                break;
            }
        }
    }

    //캐릭터 죽음 여부
    private bool IsCharacterDie(int characterId)
    {
        if(currentCharacterLiveInfo.TryGetValue(characterId, out float curHp))
        {
            return curHp <= 0f;
        }

        return false;
    }
    
    //최근 체력 저장여부
    private bool IsExistLiveInfo(int characterId)
    {
        return currentCharacterLiveInfo.ContainsKey(characterId);
    }

    //최신 체력 가져오기
    private float GetCharacterRecentHp(int characterId)
    {
        currentCharacterLiveInfo.TryGetValue(characterId, out float recentHp);
    
        return recentHp;
    }
     
    #endregion

    #region Leveling

    //캐릭터의 레벨과 경험치진행도를 설정
    private void SetCharacterLevelAndProgress(int characterId, int level, float expAmount)
    {
        for(int i = 0; i < _currentStatus.characters.Count; i++)
        {
            var characterStatus = _currentStatus.characters[i];

            if(characterStatus.characterId == characterId)
            {
                characterStatus.characterLevel = level;
                characterStatus.characterLevelProgress = expAmount;
                break;
            }
        }

        SaveCharacters();
    }
    
    //캐릭터의 현재 경험치진행도 가져오기
    private float GetCharacterLevelProgress(int characterId)
    {
        for(int i = 0; i < _currentStatus.characters.Count; i++)
        {
            var characterStatus = _currentStatus.characters[i];

            if(characterStatus.characterId == characterId)
                return characterStatus.characterLevelProgress;
        }

        return 0f;
    }
    
    //캐릭터 레벨 가져오기
    private int GetCharacterLevel(int characterId)
    {
        for(int i = 0; i < _currentStatus.characters.Count; i++)
        {
            var characterStatus = _currentStatus.characters[i];

            if(characterStatus.characterId == characterId)
                return characterStatus.characterLevel;
        }

        return 1;
    }

    #endregion

    #region Collection
    //새로운 캐릭터 추가
    private void GetNewCharacter(int newCharacterId, bool isInit = false)
    {
        if(_currentStatus.characters == null) _currentStatus.characters = new();

        CharacterData characterData = DataLoader.GetData<CharacterData>(DataType.Character, newCharacterId);
        float maxHp = characterData.levelStep.GetMaxHpUseLevel(1);
        
        _currentStatus.characters.Add(new CharacterStatus{
            characterId = newCharacterId, 
            characterLevel = 1, 
            characterLevelProgress = 0,
            curHp = maxHp});

        OnCharacterLive(newCharacterId);

        SaveCharacters();

        EventBus.Invoke<bool>("OnGetNewCharacterCase", isInit);
        EventBus.Invoke<int>("ObtainNewCharacter", newCharacterId);
    }

    //현재 보유 캐릭터 리스트
    private List<int> GetCharacterIds()
    {
        List<int> newList = new();

        for(int i = 0; i < _currentStatus.characters.Count; i++)
            newList.Add(_currentStatus.characters[i].characterId);

        return newList;
    }
    #endregion

    private void SaveCharacters()
    {
        _characterStatus.Saving(_currentStatus);
    }
}

#endregion

#region ETC
[System.Serializable]
public struct PlayerETCData
{
    public Vector3 playerPos;
    public Quaternion playerRotate;   
    public int playerWasScene;
}

public class PlayerETCSaver
{
    private PlayerETCData currentETCData;
    private Save<PlayerETCData> data;
    private Vector3 startPoint = new Vector3(0, 3, -10f);

    public PlayerETCSaver()
    {
        data = new(
            "Player/ETC", 
            "etc_data", 
            () => !GameState.IsUnsaveAble(),
            (data) => {
                int recentStateInt = PlayerPref.GetPlayerPref<int>("RecentSaveState");

                if(recentStateInt == 0 || recentStateInt > 2) return false;

                return (SaveState)recentStateInt == SaveState.Saveable;
            });

        if(data.IsExist())
        {
            currentETCData = data.savedData;

            PlayerMatch.SetPlayerPos(currentETCData.playerPos);
            PlayerMatch.SetPlayerRotate(currentETCData.playerRotate);
            PlayerMatch.SetSceneId(currentETCData.playerWasScene);
        }
        else
        {
            PlayerMatch.SetPlayerPos(startPoint);
            PlayerMatch.SetSceneId(0);
        }
    }

    public void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("PlayerETCDataSave", SaveData);
        }
        else
        {
            EventBus.UnSub("PlayerETCDataSave", SaveData);
        }
    }

    public void SaveData()
    {
        currentETCData.playerPos            = PlayerMatch.GetPlayerPos();
        currentETCData.playerRotate         = PlayerMatch.GetPlayerRotate();
        currentETCData.playerWasScene       = PlayerMatch.GetSceneId();
        
        data.Saving(currentETCData);
    }
}

#endregion