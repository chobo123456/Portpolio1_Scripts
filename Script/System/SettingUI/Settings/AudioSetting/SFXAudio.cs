using UnityEngine;

public class SFXAudio : AudioSetting
{
    protected override void SetSaveName()
    {
        base.saveName = "SFX";
    }

    protected override void OnLoad()
    {   
        type = AudioType.SFX;

        base.OnLoad();
    }
}
