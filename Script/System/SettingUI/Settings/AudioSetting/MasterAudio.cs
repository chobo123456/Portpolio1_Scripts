using UnityEngine;

public class MasterAudio : AudioSetting
{
    protected override void SetSaveName()
    {
        base.saveName = "Master";
    }

    protected override void OnLoad()
    {   
        type = AudioType.Master;

        base.OnLoad();
    }
}
