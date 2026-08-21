using UnityEngine;

public class DashModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new DashAct(box);
    }
}
