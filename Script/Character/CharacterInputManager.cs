using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType
{
    Jump,
    Attack,
    Dash_Evade,
    Interact,
    Evade
}

public interface ICharacterInput
{
    void UpdateMoveInput();
    Vector3 GetMoveInput();
    bool IsInput(InputType type);
    bool IsPressed(InputType type);
    bool IsSkillInput(int skillSlotIndex);
}

public interface IInteract
{
    void Interact();  
}

public class CharacterInputManager : ICharacterInput
{
    private CharacterInput _inputAction;
    private Vector3 _moveInput;
    private float _evadeStartTime, _evadeMinInputTime = 0.1f;
    private bool _isDebugMode = false;

    public CharacterInputManager(MonoBehaviour mono, bool isDebugMode = false)
    {
        _isDebugMode = isDebugMode;

        _inputAction = new();

        LifecycleBoundEvent.Subscribe<bool>(
            (function) => function?.Invoke(true),
            (function) => function?.Invoke(false),
            SubscribeEvent,
            mono
        );
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            _inputAction.Enable();

            _inputAction.Character.Dash_Evade.performed += Input_Dash_Evade;
            _inputAction.Character.Dash_Evade.canceled += Input_Dash_Evade;

            EventBus.Sub<string>("CharacterInputOverride", OnOverride);
            EventBus.Sub_Func<CharacterInput>("GetCharacterInputAction", GetCharacterInputAction);
        }
        else
        {
            _inputAction.Disable();

            _inputAction.Character.Dash_Evade.performed -= Input_Dash_Evade;
            _inputAction.Character.Dash_Evade.canceled -= Input_Dash_Evade;

            EventBus.UnSub<string>("CharacterInputOverride", OnOverride);
            EventBus.UnSub_Func<CharacterInput>("GetCharacterInputAction", GetCharacterInputAction);
        }
    }

    public void UpdateMoveInput()
    {
        if (_inputAction != null) 
            _moveInput = _inputAction.Character.Move.ReadValue<Vector3>();
    }

    public Vector3 GetMoveInput() => _moveInput;

    public void Input_Dash_Evade(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _evadeStartTime = Time.time;
        }
    }

    public bool IsPressed(InputType type)
    {
        if(!_isDebugMode && !GameState.IsActive()) return false;
        
        return type switch
        {
            InputType.Dash_Evade => Time.time - _evadeStartTime <= _evadeMinInputTime,
            _ => false,
        };
    }

    public bool IsInput(InputType type)
    {
        if(!_isDebugMode && !GameState.IsActive()) return false;
        
        return type switch
        {
            InputType.Jump =>       _inputAction.Character.Jump.WasPressedThisFrame(),
            InputType.Attack =>     _inputAction.Character.Attack.WasPressedThisFrame(),
            InputType.Dash_Evade => _inputAction.Character.Dash_Evade.WasPressedThisFrame(),
            InputType.Interact =>   _inputAction.Character.Interact.WasPressedThisFrame(),
            _ => false,
        };
    }

    public bool IsSkillInput(int skillSlotIndex)
    {
        return skillSlotIndex switch
        {
            1 => _inputAction.Character.Skill1.WasPressedThisFrame(),
            2 => _inputAction.Character.Skill2.WasPressedThisFrame(),
            3 => _inputAction.Character.Skill3.WasPressedThisFrame(),
            _ => false,
        };
    }

    private void OnOverride(string overridePath)
    {
        _inputAction.Disable();
        _inputAction.LoadBindingOverridesFromJson(overridePath);
        _inputAction.Enable();
    }

    private CharacterInput GetCharacterInputAction() => _inputAction;
}
