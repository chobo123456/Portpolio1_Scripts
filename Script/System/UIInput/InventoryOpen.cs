using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryOpen : UIInputManager
{
    protected override void SubscribeEvent()
    {
        input.UI.Inventory.performed += OnInput;
    }

    protected override void UnSubscribeEvent()
    {
        input.UI.Inventory.performed -= OnInput;
    }

    protected override void OnInput(InputAction.CallbackContext ctx)
    {
        if(base.IsAbleInput())
        {
            EventBus.Invoke<UIType>("On_Input_UI", UIType.Inventory);
            base.TimeSet();
        }
    }
}