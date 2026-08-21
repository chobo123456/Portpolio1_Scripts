using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public struct DecoInfo
{
    public int instanceId;
    public Sprite icon;
    public float activeTime;
    public float startTime;
}

public class DecoUI_Slot : MonoBehaviour
{
    private Image icon;
    private DecoInfo info;
    private bool isSetted = false;

    public void Initialize()
    {
        if(icon == null) icon = GetComponent<Image>();
    }

    public void SetSetting(DecoInfo info)
    {
        this.info   = info;
        icon.sprite = info.icon;

        isSetted = true;
    }

    public void SettingRelease()
    {
        isSetted = false;
    }   

    public bool IsSetted() => isSetted;

    public bool IsFinish(float startTime, float activeTime)
    {
        return Time.unscaledTime - startTime >= activeTime;
    }

    public void LoopStart(float startTime, float activeTime)
    { 
        float waitSecond = activeTime - (Time.unscaledTime - startTime);
        this.RunRoutine(Loop(waitSecond));
    }

    IEnumerator Loop(float waitTime)
    {
        yield return YieldUtil.WaitForSecondsRealtime(waitTime);

        SettingRelease();
        EventBus.Invoke<DecoUI_Slot>("OnRemoveUI_Deco", this);
    }
    
    public DecoInfo GetDecoInfo()
    {
        return info;
    }
}
