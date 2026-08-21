using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public abstract class UIInputManager : MonoBehaviour
{
    private static bool isInitOnce = false;
    private static bool isEnableOnce = false;
    protected static UIInputAction input;
    private float inputedTime = -999f;

    private void OnEnable()
    {
        if(!isInitOnce && input == null)
        {
            isInitOnce = true;
            input = new UIInputAction();
        }

        this.RunRoutine(DelayInit());
    }

    IEnumerator DelayInit()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        SubscribeEvent();
        
        if(!isEnableOnce)
        {
            isEnableOnce = true;
            input.Enable();
        }
    }
    private void OnDestroy()
    {
        if(isInitOnce)
        {
            UnSubscribeEvent();
            isInitOnce = false;
            isEnableOnce = false;
            input.Disable();
        }
    }

    protected abstract void SubscribeEvent();
    protected abstract void UnSubscribeEvent();
    protected abstract void OnInput(InputAction.CallbackContext ctx);
    protected virtual void TimeSet()
    {
        inputedTime = Time.time;
    }
    protected bool IsAbleInput()
    {
        return Time.unscaledTime - inputedTime >= 0.3f;        
    }
}