using UnityEngine;
using UnityEngine.InputSystem;
public class CraftOpen : UIInputManager
{
    protected override void SubscribeEvent()
    {
        input.UI.Craft.performed += OnInput;
    }

    protected override void UnSubscribeEvent()
    {
        input.UI.Craft.performed -= OnInput;
    }

    protected override void OnInput(InputAction.CallbackContext ctx)
    {
        if(base.IsAbleInput())
        {
            EventBus.Invoke<UIType>("On_Input_UI", UIType.Craft);

            base.TimeSet();
        }
    }
}
