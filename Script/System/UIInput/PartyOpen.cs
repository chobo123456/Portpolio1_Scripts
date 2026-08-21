using UnityEngine;
using UnityEngine.InputSystem;

public class PartyOpen : UIInputManager
{
    protected override void SubscribeEvent()
    {
        input.UI.Party.performed += OnInput;
    }

    protected override void UnSubscribeEvent()
    {
        input.UI.Party.performed -= OnInput;
    }

    protected override void OnInput(InputAction.CallbackContext ctx)
    {
        if(base.IsAbleInput())
        {
            EventBus.Invoke<UIType>("On_Input_UI", UIType.Party);
            base.TimeSet();
        }
    }
}