using UnityEngine;
using UnityEngine.UI;


public abstract class EffectSetting : SettingBase<int> 
{
    protected Toggle toggle;
    protected EffectType type;
    protected void OnToggle(bool value)
    {
        int isOn = value == true ? 1 : 0;

        EventBus.Invoke<(EffectType, bool)>("EnabledEffect", (type, value));

        base.value = isOn;
        base.Save();
    }
    protected override void Initialize()
    {
        base.baseValue = 1;

        SetSaveName();
        
        if(toggle == null)
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnToggle);
        } 
    }

    protected override void OnLoad()
    {
        bool isOn = value == 1 ? true : false;

        toggle.isOn = isOn;
        EventBus.Invoke<(EffectType, bool)>("EnabledEffect", (type, base.value == 1 ? true : false));
    }

    protected abstract void SetSaveName();
}
