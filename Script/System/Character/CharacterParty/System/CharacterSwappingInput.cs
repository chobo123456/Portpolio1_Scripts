using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwappingInput
{
    private PartySystem _system;
    private PartyUIHandler _handler;
    private CharacterChangeControl inputAction;

    private float _changeTime = 1f, _recentChangeTime = -999f;

    public void InjectParameter(PartySystem system, PartyUIHandler handler)
    {
        _system = system;
        _handler = handler;
    }

    private void OnChange1(InputAction.CallbackContext ctx)
    {
        OnCharacterSwap(0);
    }
    
    private void OnChange2(InputAction.CallbackContext ctx)
    {
        OnCharacterSwap(1);
    }

    private void OnCharacterSwap(int index)
    {
        if (!LoadStatus.IsReady_CharacterParty || !GameState.IsActive()) return;

        if(Time.time - _recentChangeTime >= _changeTime)
        {
            if(_system.TrySwapCurrentCharacter(index))
            {
                _recentChangeTime = Time.time;
                _handler.HUD_OnSelect(index);
            }
        }
    }

    public void OnEnable()
    {
        inputAction = new();
        inputAction.CharacterChange.Change1.performed += OnChange1;
        inputAction.CharacterChange.Change2.performed += OnChange2;
        inputAction.Enable();
    }

    public void OnDisable()
    {
        inputAction.CharacterChange.Change1.performed -= OnChange1;
        inputAction.CharacterChange.Change2.performed -= OnChange2;
        inputAction.Disable();
    }
}