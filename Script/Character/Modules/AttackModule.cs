using UnityEngine;

public class AttackModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new AttackAct(box);
    }
}

