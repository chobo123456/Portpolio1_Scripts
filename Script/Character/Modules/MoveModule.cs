using UnityEngine;

public class MoveModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new MoveAct(box);
    }
}
