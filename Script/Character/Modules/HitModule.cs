using UnityEngine;

public class HitModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new HitAct(box);
    }
}
