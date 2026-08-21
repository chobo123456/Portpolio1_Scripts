using UnityEngine;

public class IdleModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new IdleAct(box);
    }
}
