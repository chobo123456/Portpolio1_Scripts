using UnityEngine;

public class JumpModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new JumpAct(box);
    }
}
