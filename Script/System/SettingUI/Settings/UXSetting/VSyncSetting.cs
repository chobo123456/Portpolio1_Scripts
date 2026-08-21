using UnityEngine;
using UnityEngine.UI;

public class VSyncSetting : SettingBase<int>
{
    private Toggle toggle;
    private void OnValueChanged(bool value)
    {
        int isOn = value == true ? 1 : 0;

        QualitySettings.vSyncCount = isOn;

        base.value = isOn;
        base.Save();
    }

    protected override void Initialize()
    {
        base.baseValue = 0;
        base.saveName  = "VSync";

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

        QualitySettings.vSyncCount = value;
    }
}
