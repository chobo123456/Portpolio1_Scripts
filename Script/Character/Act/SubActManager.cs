using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class SubActManager : ActBase
{
    protected List<ActBase> acts;
    protected ActBase currentAct;

    public SubActManager(PlayerDataBox box) : base(box)
    {
        acts = new();
    }
    protected void SetAct(List<ActBase> _acts)
    {
        acts = _acts;

        acts = acts.OrderByDescending(act => act.Priority).ToList();
    }
    protected ActBase GetActBase()
    {
        for (int i = 0; i < acts.Count; i++)
        {
            var act = acts[i];

            if (act.CanEnter())
            {
                return act;
            }
        }

        return null;
    }

}