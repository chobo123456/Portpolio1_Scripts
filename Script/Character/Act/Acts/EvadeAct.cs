using UnityEngine;

public class EvadeAct : SubActManager
{
    public EvadeAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Hit_EvadeAct(box),
        });

        Priority = 90;
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

public class Hit_EvadeAct : ActBase
{
    private AnimationActBase animation;

    public Hit_EvadeAct(PlayerDataBox box) : base(box)
    {
        Priority = 90;
        animation = new EvadeAnim_Act(box);
    }

    public override bool CanEnter()
    {
        if (box.hpComp.Evade) return true;

        return false;
    }

    public override void ActEnter()
    {
        box.sensor.CheckNearEnemy();
        box.rotate.OnAttackStartRotate();

        Vector3 curVel = box.rigid.linearVelocity;
        curVel *= 0f;
        curVel.y = box.rigid.linearVelocity.y;

        box.rigid.linearVelocity = curVel;

        float moveInput = box.input.GetMoveInput().x;

        if(moveInput >= 0)
            animation.SetFloat(1f);
        else if(moveInput < 0)
            animation.SetFloat(-1f);

        animation.OnEnterAnim();

        ActLock = true;
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
        animation.SetFloat(0f);
        ActLock = false;
        box.hpComp.EvadeFinish();
        box.rotate.OnAttackEndRotate();
    }
}
