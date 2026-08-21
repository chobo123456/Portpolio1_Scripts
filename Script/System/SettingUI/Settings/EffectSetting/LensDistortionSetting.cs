using UnityEngine;

public class LensDistortionSetting : EffectSetting
{
    protected override void SetSaveName()
    {
        base.saveName  = "LensDistortion";
    }

    protected override void OnLoad()
    {
        type = EffectType.LensDistortion;

        base.OnLoad();
    }
}
