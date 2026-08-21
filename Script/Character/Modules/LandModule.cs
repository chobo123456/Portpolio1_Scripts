using UnityEngine;

public class LandModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new LandAct(box);
    }
}
