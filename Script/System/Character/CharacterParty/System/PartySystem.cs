using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PartySystem
{
    private CharacterModelChanger _changer;
    private PartyStore _store;
    private bool _isInProcess = false, _isNeedCheckAcceptedCharacterId = false, _isNeedCheckAcceptedSlotIndex = false;

    private int _accepttedCharacterId = 0, _accepttedSlotIndex = 0;
    private const int MaxCharacterCount = 2;
    private const string GetCharacterIdsEventName = "GetCharacterIds", IsCharacterDieEventName = "IsCharacterDie",
        CharacterStateAbortEventName = "CharacterStateAbort";
        
    public Dictionary<int, int> GetReLoadedPartyDictionary()
    {
        Dictionary<int, int> reloadedParty = new();

        for(int i = 0; i < MaxCharacterCount; i++)
        {
            if(_store.GetPartyInfo().TryGetValue(i, out int characterId))
                reloadedParty.Add(i, characterId);
            else
                reloadedParty.Add(i, -1);
        }

        return reloadedParty;
    }

    public List<CharacterIconPayLoad> InitializeIconPayLoad()
    {
        List<CharacterIconPayLoad> iconPayLoad = new();

        foreach(var characterId in EventBus.Invoke_Func<List<int>>(GetCharacterIdsEventName))
        {
            iconPayLoad.Add(GetPayLoad(characterId));
        }

        return iconPayLoad;
    }

    public CharacterIconPayLoad GetPayLoad(int characterId)
    {
        return new CharacterIconPayLoad
        {
            _characterId = characterId,
            _characterIcon = GetCharacterIcon(characterId),
            _elementIcon = GetCharacterElementIcon(characterId),
            isUsedRecent = IsTargetCharacterAlreadyInParty(characterId, out int characterSlotIndex)
        };
    }

    public CharacterSlotElementIconPayload GetSlotElementIconPayload(int index, int characterId)
    {
        bool isDisable = characterId <= 0;

        return new CharacterSlotElementIconPayload
        {
            index = index,
            isDisable = isDisable,
            elementIcon = !isDisable ? GetCharacterElementIcon(characterId) : null
        };
    }

    public CharacterHUDPayload GetHUDPayLoad(int index, int characterId)
    {
        bool isDisable = characterId <= 0;

        return new CharacterHUDPayload
        {
            index = index,
            isDisableSlot = isDisable,
            characterIcon = !isDisable ? GetCharacterIcon(characterId) : null,
            elementIcon = !isDisable ? GetCharacterElementIcon(characterId) : null
        };
    }

    public PartyPreviewerPayload GetPreviewPayLoad(int index, int characterId)
    {
        bool isDisable = characterId <= 0;
        
        return new PartyPreviewerPayload
        {
            index = index,
            isDisable = isDisable,
            previewModelPrefab = !isDisable ? DataLoader.GetData<Character_Prefab_Data>(DataType.CharacterETC, characterId).inParty_prefab : null,
        };
    }

    private Sprite GetCharacterIcon(int characterId)
    {
        return DataLoader.GetData<CharacterData>(DataType.Character, characterId).characterIcon;
    }

    private Sprite GetCharacterElementIcon(int characterId)
    {
        CharacterData characterData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);
        int elementSpriteId = (int)characterData.element;
        return DataLoader.GetData<Sprite>(DataType.ElementSprite, elementSpriteId);   
    }
    

    //Int
    public int GetCharacterIdUseIndex(int slotIndex)
    {
        foreach(var map in _store.GetPartyInfo())
        {
            int recentPartyCharacterId  = map.Value;
            int recentPartySlot         = map.Key;

            if(recentPartySlot == slotIndex)
                return recentPartyCharacterId;  
        }

        return -1;
    }  

    public int GetExceptionCase()
    {
        if (_store.GetPartyInfo().Count <= 0)
            return 1;
        else
        {
            int noneHpCharacterCount = 0;

            foreach(var map in _store.GetPartyInfo())
            {
                int characterId = map.Value;
                bool isDead = IsCharacterDie(characterId);

                if (isDead)
                    noneHpCharacterCount++;
            }

            if (noneHpCharacterCount == _store.GetPartyInfo().Count)
                return 2;
        }

        return -1;
    }

    private int FindAliveCharacterIndex()
    {
        Dictionary<int, int> party = _store.GetPartyInfo();

        foreach(var map in party.OrderBy(x => x.Key))
        {
            int characterId = map.Value;
            bool isDead = IsCharacterDie(characterId);
        
            if(isDead) continue;
            else
                return map.Key;
        }
            
        return -1;
    }

    
    //Bool
    public bool TryFindAliveCharacterIndex(out int aliveCharacterIndex)
    {
        if(_store.GetPartyInfo().TryGetValue(_store.GetRecentIndex(), out int characterId) &&
            IsCharacterDie(characterId))
        {
            int findedCharacterIndex = FindAliveCharacterIndex();

            if(findedCharacterIndex >= 0)
            {
                aliveCharacterIndex = findedCharacterIndex;
                return true;
            }
            else 
            {
                aliveCharacterIndex = -1;
                return false;
            }
        }
        else if(!_store.GetPartyInfo().ContainsKey(_store.GetRecentIndex()))
        {
            if(_store.GetPartyInfo().Count > 0)
            {
                aliveCharacterIndex = _store.GetPartyInfo().Keys.First();
                return true;
            }
        }
        
        aliveCharacterIndex = _store.GetRecentIndex();
        return true;
    }

    public bool IsTargetCharacterAlreadyInParty(int id, out int characterSlotIndex)
    {
        foreach(var map in _store.GetPartyInfo())
        {
            int recentPartyCharacterId  = map.Value;
            int recentPartySlot         = map.Key;

            if(recentPartyCharacterId == id)
            {
                characterSlotIndex = recentPartySlot;
                return true;  
            }
        }

        characterSlotIndex = 0;
        return false;
    }

    public bool IsCharacterDie(int characterId)
    {
        return EventBus.Invoke_Func<int, bool>(IsCharacterDieEventName, characterId);
    }

    public bool IsChangeAble(int index)
    {
        bool isCharacterDead = _store.GetPartyInfo().TryGetValue(index, out int characterId) && IsCharacterDie(characterId);

        if(isCharacterDead || _store.GetRecentIndex() == index || !_store.GetPartyInfo().ContainsKey(index)) 
            return false;

        return true;
    }

    public bool IsCloseAble()
    {
        return !_isInProcess && GetExceptionCase() == -1;
    }
    
    public bool IsAcceptedCharacterId(int characterId)
    {
        if(_isNeedCheckAcceptedCharacterId)
        {
            return _accepttedCharacterId == characterId;
        }

        return true;
    }

    public bool IsAcceptedSlotIndex(int slotIndex)
    {
        if(_isNeedCheckAcceptedSlotIndex)
        {
            return _accepttedSlotIndex == slotIndex;
        }

        return true;
    }
    
    public bool TryGetCharacterIdUseSlotIndex(int slotIndex, out int characterId)
    {
        characterId = GetCharacterIdUseIndex(slotIndex);

        if(characterId > 0) return true;

        return false;
    }

    //Void
    public void InjectParameter(PartyStore store, CharacterModelChanger changer)
    {
        _store = store;
        _changer = changer;
    }

    public void OnEnable()
    {
        SubscribeEvent(true);
    }

    public void OnDisable()
    {
        SubscribeEvent(false);
    }

    public void ChangeCharacter(int characterIndex, int characterId)
    {
        if(IsTargetCharacterAlreadyInParty(characterId, out int recentIndex))
            _store.RemoveData(recentIndex);

        if (characterId <= 0)
            _store.RemoveData(characterIndex);
        else
            _store.AddData(characterIndex, characterId);  
    }

    public void LoadParty()
    {
        _isInProcess = true;

        if(TryFindAliveCharacterIndex(out int aliveCharacterIndex))
        {
            _store.SetRecentIndex(aliveCharacterIndex);
            _changer.LoadRecentParty(GetReLoadedPartyDictionary(), aliveCharacterIndex);
            
            CharacterStateAbort();

            _store.SavePartyInfo();
            _isInProcess = false;
        }
        else 
        {
            EventBus.Invoke<bool, bool>("TryRespawn", true, true);
            _isInProcess = false;
            return;
        }
    }

    public void CharacterStateAbort()
    {
        EventBus.Invoke(CharacterStateAbortEventName);
    }

    public bool TrySwapCurrentCharacter(int index)
    {
        if(!IsChangeAble(index)) return false;

        _store.SetRecentIndex(index);
        _changer.ActiveCharacter(index);
        CharacterStateAbort();

        return true;
    }

    private void OnCharacterDie(bool needPanel, bool needForce)
    {
        if(TryFindAliveCharacterIndex(out int aliveCharacterIndex))
        {
            _changer.ActiveCharacter(aliveCharacterIndex);
            CharacterStateAbort();
        }
        else 
            EventBus.Invoke<bool, bool>("TryRespawn", needPanel, needForce);
    }
    
    private void SetCheckAcceptedCharacterIdFlag(bool isNeedCheck, int characterId = 0)
    {
        _isNeedCheckAcceptedCharacterId = isNeedCheck;

        if(!_isNeedCheckAcceptedCharacterId)
            _accepttedCharacterId = -1;
        else
            _accepttedCharacterId = characterId;
    }

    private void SetCheckAcceptedSlotFlag(bool isNeedCheck, int slotIndex = 0)
    {
        _isNeedCheckAcceptedSlotIndex = isNeedCheck;

        if(!_isNeedCheckAcceptedSlotIndex)
            _accepttedSlotIndex = -1;
        else
            _accepttedSlotIndex = slotIndex;
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub<bool, int>("Party_System_SetCheckAcceptedCharacterIdFlag", SetCheckAcceptedCharacterIdFlag);
            EventBus.Sub<bool, int>("Party_System_SetCheckAcceptedSlotFlag", SetCheckAcceptedSlotFlag);
            EventBus.Sub<bool, bool>("OnCharacterDie", OnCharacterDie);
            EventBus.Sub("LoadParty", LoadParty);
        }
        else
        {
            EventBus.UnSub<bool, int>("Party_System_SetCheckAcceptedCharacterIdFlag", SetCheckAcceptedCharacterIdFlag);
            EventBus.UnSub<bool, int>("Party_System_SetCheckAcceptedSlotFlag", SetCheckAcceptedSlotFlag);
            EventBus.UnSub<bool, bool>("OnCharacterDie", OnCharacterDie);
            EventBus.UnSub("LoadParty", LoadParty);
        }
    }
}