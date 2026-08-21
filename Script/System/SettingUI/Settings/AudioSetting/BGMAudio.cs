using UnityEngine;

public class BGMAudio : AudioSetting
{
    protected override void SetSaveName()
    {
        base.saveName = "BGM";
    }
    
    protected override void OnLoad()
    {   
        type = AudioType.BGM;

        base.OnLoad();
    }
}
