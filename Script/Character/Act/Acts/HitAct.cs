using UnityEngine;
using System.Collections.Generic;

#region Hit
public class HitAct : SubActManager
{
    public HitAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Hit_EvadeAct(box),
            new Hit_NormalAct(box),
        });

        Priority = 30;
    }

    public override bool CanEnter()
    {
        if(currentAct != null && currentAct.ActLock)
        {
            ActLock = true;    
            return true;
        }
        else if((currentAct == null || !currentAct.ActLock) && ActLock)
        {
            ActLock = false;    
            return false;
        }

        var act = GetActBase();

        if (act != null)
        {
            if(currentAct != act)
                ActEnd();
                
            currentAct = act;
            return true;
        }

        return false;
    }
    public override void ActEnter() => currentAct?.ActEnter();
    public override void OnUpdate() => currentAct?.OnUpdate();
    public override void OnFixedUpdate() => currentAct?.OnFixedUpdate();
    public override void OnLateUpdate() => currentAct?.OnLateUpdate();
    public override void ActEnd()
    {
        ActLock = false;
        currentAct?.ActEnd();
    }
}

public class Hit_NormalAct : ActBase
{
    private AnimationActBase animation;

    public Hit_NormalAct(PlayerDataBox box) : base(box)
    {
        Priority = 10;
        animation = new HitAnim_Act(box);
    }

    public override bool CanEnter()
    {
        if (box.hpComp.IsHit) return true;

        return false;
    }

    public override void ActEnter()
    {
        ActLock = true;
        animation.OnEnterAnim();

        box.rigid.useGravity = true;
        box.rigid.linearVelocity = Vector3.zero;

        Vector3 hitDir = box.hpComp.CurrentSource.knockbackDir;

        box.rigid.linearVelocity = new Vector3(hitDir.x, box.rigid.linearVelocity.y, hitDir.z);
    }

    public override void OnUpdate()
    {
        if(animation.IsFinish())
        {
            ActLock = false;
        }
    }

    public override void ActEnd()
    {
        animation.OnExitAnim();
        ActLock = false;
    }
}


#endregion