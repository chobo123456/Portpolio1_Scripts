using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FullScreenSetting : SettingBase<int>
{
    private Toggle toggle;
    private void OnToggle(bool isOn)
    {
        Screen.fullScreen = isOn;
        base.value = isOn == true ? 1 : 0;
        base.Save();
    }
    
    protected override void Initialize()
    {
        base.baseValue = 1;
        base.saveName  = "FullScreen";
        
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

        WaitNextFrame();
    }

    private async void WaitNextFrame()
    {
        await Awaitable.NextFrameAsync();
        Screen.fullScreen = toggle.isOn;
    }
}
