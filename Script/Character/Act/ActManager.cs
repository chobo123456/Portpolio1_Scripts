using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ActManager
{
    private List<ActBase> acts;

    private ActBase currentAct;

    public ActManager(List<ActBase> connectedActs)
    {
        acts = connectedActs.OrderByDescending(act => act.Priority).ToList();
    }

    public void Update()
    {
        var newAct = GetAct();

        if(currentAct != null)
        {
            if(newAct.Priority < currentAct.Priority && currentAct.ActLock)
            {
                currentAct?.OnUpdate();
                return;
            }
            else if(newAct.Priority > currentAct.Priority)
                AbortState();
        }
        

        if(currentAct == null || newAct != currentAct)  
        {
            currentAct?.ActEnd();

            currentAct = newAct;

            currentAct?.ActEnter();
        }

        currentAct?.OnUpdate();
    }

    public void FixedUpdate() { currentAct?.OnFixedUpdate(); }
    public void LateUpdate() { currentAct?.OnLateUpdate(); }
    public void AbortState() { currentAct?.ActEnd(); }

    private ActBase GetAct()
    {
        for(int i = 0; i < acts.Count; i++)
        {
            var act = acts[i];

            if(act.CanEnter())
            {
                return act;
            }
        }

        return null;
    }
}
