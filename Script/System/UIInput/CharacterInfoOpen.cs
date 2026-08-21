using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInfoOpen : UIInputManager
{    
    protected override void SubscribeEvent()
    {
        input.UI.CharacterInfo.performed += OnInput;
    }

    protected override void UnSubscribeEvent()
    {
        input.UI.CharacterInfo.performed -= OnInput;
    }

    protected override void OnInput(InputAction.CallbackContext ctx)
    {
        if(base.IsAbleInput())
        {
            EventBus.Invoke<UIType>("On_Input_UI", UIType.CharacterStatus);

            base.TimeSet();
        }
    }
}