using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EvadeModule : Module
{
    public override void SetModule(PlayerDataBox box)
    {
        _act = new EvadeAct(box);
    }
}
