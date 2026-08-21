using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterGrowthSystem
{
    private const string GetCharacterIdsEventName = "GetCharacterIds";
    private bool _isNeedCheckAcceptedCharacterId = false;
    private int _accepttedCharacterId = 0;

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
            EventBus.Sub<bool, int>("Growth_System_SetCheckAcceptedCharacterIdFlag", SetCheckAcceptedCharacterId);
        }
        else
        {
            EventBus.UnSub<bool, int>("Growth_System_SetCheckAcceptedCharacterIdFlag", SetCheckAcceptedCharacterId);
        }
    }

    public Sprite GetCharacterSpriteUseCharacterId(int characterId)
    {
        return DataLoader.GetData<CharacterData>(DataType.Character, characterId).characterSprite;
    }

    public Dictionary<int, Sprite> GetCharacterIdSpriteMap()
    {
        return EventBus.Invoke_Func<List<int>>(GetCharacterIdsEventName).ToDictionary(x => x, y => GetCharacterSpriteUseCharacterId(y));
    }

    public int GetFirstIndexCharacter()
    {
        return EventBus.Invoke_Func<List<int>>(GetCharacterIdsEventName)[0];
    }

    public bool IsAcceptedCharacterId(int characterId)
    {
        if(_isNeedCheckAcceptedCharacterId)
        {
            return _accepttedCharacterId == characterId;
        }

        return true;
    }

    private void SetCheckAcceptedCharacterId(bool isNeedCheck, int slotIndex = 0)
    {
        _isNeedCheckAcceptedCharacterId = isNeedCheck;

        if(!_isNeedCheckAcceptedCharacterId)
            _accepttedCharacterId = -1;
        else
            _accepttedCharacterId = slotIndex;
    }
}

public class CharacterGrowthUIHandler
{
    private CharacterGrowthSystem _system;
    public void InjectParameter(CharacterGrowthSystem system)
    {
        _system = system;
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
            EventBus.Sub("Growth_UI_Open", OnOpen);
            EventBus.Sub<int>("Growth_UI_TryChangeSelectCharacter", OnSelectCharacter);
        }
        else
        {
            EventBus.UnSub("Growth_UI_Open", OnOpen);
            EventBus.UnSub<int>("Growth_UI_TryChangeSelectCharacter", OnSelectCharacter);
        }
    }

    private void OnSelectCharacter(int characterId)
    {
        if(!_system.IsAcceptedCharacterId(characterId)) return;
        
        //Other System
        EventBus.Invoke<int>("Growth_UI_ChangeSelectedCharacter", characterId);  

        //UI Invoke
        Sprite characterSprite = _system.GetCharacterSpriteUseCharacterId(characterId);
        EventBus.Invoke<Sprite>("Growth_UI_SetCharacterIcon", characterSprite);

        if(GameState.IsTutorial())
            EventBus.Invoke<int>("On_Growth_UI_ClickCharacterIcon", characterId);
    }

    public void OnOpen()
    {
        int firstIndexCharacterId = _system.GetFirstIndexCharacter();
        OnSelectCharacter(firstIndexCharacterId);
    }
}

public class CharacterGrowthManager : MonoBehaviour
{
    private CharacterGrowthSystem _growSystem;
    private CharacterGrowthUIHandler _growUIHandler;

    private bool _isReadyLocalUI = false,  _isReadyManager = false;
    private void OnEnable()
    {
        SubscribeEvent(true);
        this.RunRoutine(Booting());
    }

    private IEnumerator Booting()
    {
        _growSystem = new();
        _growUIHandler = new();

        yield return new WaitUntil(() => 
            LoadStatus.IsReady 
            && EventBus.Invoke_Func<bool>("FinishLoadCharacterData"));

        _growSystem.OnEnable();
        _growUIHandler.OnEnable();
        _growUIHandler.InjectParameter(_growSystem);

        yield return new WaitUntil(() => _isReadyLocalUI);

        EventBus.Invoke<Dictionary<int, Sprite>>("Growth_UI_Initialize", _growSystem.GetCharacterIdSpriteMap());

        _isReadyManager = true;

        LoadStatus.SetStatus(ManagerType.GrowthTap, true);
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("Growth_UI_Ready", OnLocalUIReady);
            EventBus.Sub<int>("ObtainNewCharacter", ObtainNewCharacter);
        }
        else
        {
            EventBus.UnSub("Growth_UI_Ready", OnLocalUIReady);
            EventBus.UnSub<int>("ObtainNewCharacter", ObtainNewCharacter);
        }
    }

    private void OnLocalUIReady()
    {
        _isReadyLocalUI = true;
    }

    private void ObtainNewCharacter(int characterId)
    {
        if(!_isReadyManager || characterId <= 0) return;
        
        EventBus.Invoke<int, Sprite>("Growth_UI_ObtainNewCharacter", characterId, _growSystem.GetCharacterSpriteUseCharacterId(characterId));
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
        _growSystem?.OnDisable();
        _growUIHandler?.OnDisable();
    }
}
