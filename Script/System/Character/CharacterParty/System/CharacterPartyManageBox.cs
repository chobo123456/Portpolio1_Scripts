using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPartyManageBox
{
    private readonly MonoBehaviour _mono;
    private readonly GameObject _character;

    private readonly PartyStore _store;
    private readonly CharacterModelMaker _modelMaker;
    private readonly CharacterAnimationOverrider _characterAnimationOverrider;
    private readonly CharacterModelChanger _changer;
    private readonly PartySystem _system;
    private readonly PartyUIHandler _uiHandler;
    private readonly CharacterSwappingInput _input;

    public bool IsReady { get; private set; } = false;

    public CharacterPartyManageBox(Transform tr)
    {
        _mono                           = tr.GetComponent<MonoBehaviour>();
        _character                      = GameObject.FindAnyObjectByType<CharacterControl>(FindObjectsInactive.Include).gameObject;

        _store                          = new();
        _modelMaker                     = new();
        _characterAnimationOverrider    = new();
        _changer                        = new(); 
        _system                         = new();
        _uiHandler                      = new();       
        _input                          = new();

        //주입
        _characterAnimationOverrider.InjectParameter(_mono, _character);
        _changer.InjectParameter(_modelMaker, _characterAnimationOverrider, _character, _mono);
        _system.InjectParameter(_store, _changer);
        _uiHandler.InjectParameter(_store, _system);
        _input.InjectParameter(_system, _uiHandler);
        
        _store.OnEnable();
        _system.OnEnable();
        _input.OnEnable();
        _uiHandler.OnEnable();

        EventBus.Sub_Func<GameObject>("GetCharacterObject", GetCharacter);

        IsReady = true;
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<GameObject>("GetCharacterObject", GetCharacter);

        _store.OnDisable();
        _input.OnDisable();
        _system.OnDisable();
        _uiHandler.OnDisable();
    }

    private GameObject GetCharacter() => _character;
    public PartySystem GetSystem() => _system;
    public PartyStore GetStore() => _store;
    public PartyUIHandler GetHandler() => _uiHandler;
}