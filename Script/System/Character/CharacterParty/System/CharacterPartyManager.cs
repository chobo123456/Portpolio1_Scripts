using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPartyManager : MonoBehaviour
{    
    private CharacterPartyManageBox box;
    private bool isReadyUISystem = false, isReady = false;
    
    private void OnEnable()
    {
        Subscribe(true);
        this.RunRoutine(Booting());
    }

    private void OnDisable()
    {
        Subscribe(false);

        LoadStatus.SetStatus(ManagerType.Party, false);
        box?.OnDisable();
    } 

    System.Collections.IEnumerator Booting()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady && EventBus.Invoke_Func<bool>("FinishLoadCharacterData"));

        if(box == null) box = new(this.transform);

        yield return new WaitUntil(() => 
            box.IsReady && 
            isReadyUISystem);

        box.GetSystem().LoadParty();

        EventBus.Invoke<List<CharacterIconPayLoad>>("Party_UI_InitializeIcons", box.GetSystem().InitializeIconPayLoad());
        box.GetHandler().UpdatePartyUI();
        box.GetHandler().HUD_OnSelect(box.GetStore().GetRecentIndex());

        LoadStatus.SetStatus(ManagerType.Party, true);

        isReady = true;
    }

    private void OnLocalUIReady()
    {
        isReadyUISystem = true;
    }

    private void Subscribe(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub("Party_UI_LocalReady", OnLocalUIReady);
            EventBus.Sub<int>("ObtainNewCharacter", OnNewCharacterObtain);
        }
        else
        {
            EventBus.UnSub("Party_UI_LocalReady", OnLocalUIReady);
            EventBus.UnSub<int>("ObtainNewCharacter", OnNewCharacterObtain);
        }
    }

    private void OnNewCharacterObtain(int newCharacterId)
    {
        if(!isReady) return;

        EventBus.Invoke<CharacterIconPayLoad>("Party_UI_NewCharacterObtain", box.GetSystem().GetPayLoad(newCharacterId));
    }
}

