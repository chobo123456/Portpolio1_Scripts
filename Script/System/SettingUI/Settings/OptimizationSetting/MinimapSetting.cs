using UnityEngine;
using UnityEngine.UI;

public class MinimapSetting : SettingBase<int>
{
    private Toggle toggle;

    private void OnValueChanged(bool value)
    {
        int isOn = value == true ? 1 : 0;

        EventBus.Invoke<bool>("UseMinimap", value);

        base.value = isOn;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 1;
        base.saveName  = "MiniMap";

        if(toggle == null)
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnValueChanged);
        }
    }

    protected override void OnLoad()
    {
        bool isOn = value == 1 ? true : false;

        toggle.isOn = isOn;
        EventBus.Invoke<bool>("UseMinimap", isOn);
    }
}
