using UnityEngine;
using UnityEngine.InputSystem;
public class SettingOpen : UIInputManager
{
    protected override void SubscribeEvent()
    {
        input.UI.Setting.performed += OnInput;
    }

    protected override void UnSubscribeEvent()
    {
        input.UI.Setting.performed -= OnInput;
    }

    protected override void OnInput(InputAction.CallbackContext ctx)
    {
        if(base.IsAbleInput())
        {
            EventBus.Invoke<UIType>("On_Input_UI", UIType.Setting);

            base.TimeSet();
        }
    }
}
