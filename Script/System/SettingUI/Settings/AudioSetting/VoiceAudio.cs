using UnityEngine;

public class VoiceAudio : AudioSetting
{
    protected override void SetSaveName()
    {
        base.saveName = "Voice";
    }

    protected override void OnLoad()
    {   
        type = AudioType.Voice;

        base.OnLoad();
    }
}
