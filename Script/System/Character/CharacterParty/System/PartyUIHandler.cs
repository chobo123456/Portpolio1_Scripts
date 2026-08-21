using System;
using System.Collections.Generic;

public class PartyUIHandler
{
    private PartySystem _system;
    private PartyStore _store;
    private int selectedCharacterId = 0;
    private bool isAwaitingSlotSelection = false;
    
    public void InjectParameter(PartyStore store, PartySystem system)
    {
        _store = store;
        _system = system;
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("Party_UI_CloseClick", OnFinishSettup);
            EventBus.Sub<int>("Party_UI_IconClick", OnCharacterIconClicked);
            EventBus.Sub<int>("Party_UI_SlotClick", OnCharacterSlotClick);
            EventBus.Sub_Func<bool>("Party_System_TryClose", TryClose);
        }
        else
        {
            EventBus.UnSub("Party_UI_CloseClick", OnFinishSettup);
            EventBus.UnSub<int>("Party_UI_IconClick", OnCharacterIconClicked);
            EventBus.UnSub<int>("Party_UI_SlotClick", OnCharacterSlotClick);
            EventBus.UnSub_Func<bool>("Party_System_TryClose", TryClose);
        }
    }

    private void OnCharacterIconClicked(int characterId)
    {
        if(!_system.IsAcceptedCharacterId(characterId)) return;

        if(_system.IsTargetCharacterAlreadyInParty(characterId, out int characterSlotIndex))
        {
            EventBus.Invoke<int>("Party_UI_UnShowChooseIcon", characterId);
            _system.ChangeCharacter(characterSlotIndex, -1);

            UpdatePartyUI();
        }
        else
        {
            selectedCharacterId = characterId;

            EventBus.Invoke("Party_UI_SelectIcon");

            isAwaitingSlotSelection = true;

            if(GameState.IsTutorial())
                EventBus.Invoke<int>("On_Party_UI_ClickCharacterIcon", characterId);
        }
    }

    private void OnCharacterSlotClick(int slotIndex)
    {
        if(!_system.IsAcceptedSlotIndex(slotIndex)) return;

        EventBus.Invoke("Party_UI_UnSelectIcon");

        if(_system.TryGetCharacterIdUseSlotIndex(slotIndex, out int characterId))
            EventBus.Invoke<int>("Party_UI_UnShowChooseIcon", characterId);

        if(!isAwaitingSlotSelection)    
        {
            _system.ChangeCharacter(slotIndex, -1);
        }
        else
        {
            EventBus.Invoke<int>("Party_UI_ShowChooseIcon", selectedCharacterId);
            _system.ChangeCharacter(slotIndex, selectedCharacterId);

            isAwaitingSlotSelection = false;

            if(GameState.IsTutorial())
                EventBus.Invoke<int>("On_Party_UI_ClickCharacterSlot", slotIndex);
        }        

        UpdatePartyUI();
    }

    private bool TryClose()
    {
        bool isCloseAble = _system.IsCloseAble();
        
        if(!isCloseAble)
            ShowWarningPanel();

        return isCloseAble;
    }

    private void OnFinishSettup()
    {
        bool isCloseAble = _system.IsCloseAble();
        
        _system.LoadParty();
        HUD_OnSelect(_store.GetRecentIndex());
        EventBus.Invoke<bool>("SetPartyCam", false);
        EventBus.Invoke("Party_UI_OnClose");
        EventBus.Invoke("On_Party_UI_ClickCharacterSettingFinish");
    }

    public void HUD_OnSelect(int selectIndex)
    {
        EventBus.Invoke<int>("Party_HUD_OnSelect", selectIndex);
    }

    private void ShowWarningPanel()
    {
        EventBus.Invoke<int>("Party_UI_ShowWarningPanel", _system.GetExceptionCase());
    }

    public void UpdatePartyUI()
    {
        foreach(var map in _system.GetReLoadedPartyDictionary())
        {
            int index = map.Key;
            int characterId = map.Value;

            EventBus.Invoke<CharacterSlotElementIconPayload>("Party_UI_UpdateSlotElementIcon", _system.GetSlotElementIconPayload(index, characterId));
            EventBus.Invoke<CharacterHUDPayload>("Party_HUD_UpdateSlot", _system.GetHUDPayLoad(index, characterId));
            EventBus.Invoke<PartyPreviewerPayload>("Party_UI_UpdatePreview", _system.GetPreviewPayLoad(index, characterId));
        }
    }

    public void OnEnable()
    {
        SubscribeEvent(true);
    }

    public void OnDisable()
    {
        SubscribeEvent(false);
    }
}