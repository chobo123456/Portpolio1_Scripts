using UnityEngine;
using UnityEngine.InputSystem;
public class QuestOpen : UIInputManager
{
    protected override void SubscribeEvent()
    {
        input.UI.Quest.performed += OnInput;
    }

    protected override void UnSubscribeEvent()
    {
        input.UI.Quest.performed -= OnInput;
    }

    protected override void OnInput(InputAction.CallbackContext ctx)
    {
        if(base.IsAbleInput())
        {
            EventBus.Invoke<UIType>("On_Input_UI", UIType.Quest);

            base.TimeSet();
        }
    }
}
