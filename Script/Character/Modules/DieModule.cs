using UnityEngine;

public class DieModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new DieAct(box);
    }
}
