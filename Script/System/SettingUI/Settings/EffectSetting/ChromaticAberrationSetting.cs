using UnityEngine;

public class ChromaticAberrationSetting : EffectSetting
{
    protected override void SetSaveName()
    {
        base.saveName  = "ChromaticAberration";
    }
    
    protected override void OnLoad()
    {
        type = EffectType.ChromaticAberration;

        base.OnLoad();
    }
}
