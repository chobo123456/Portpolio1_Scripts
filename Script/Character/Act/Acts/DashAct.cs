using System.Collections.Generic;
using UnityEngine;
public class DashAct : SubActManager
{
    public DashAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() {
            new Dash_SlopeAct(box),
            new Dash_NormalAct(box),
        });

        Priority = 65;
    }

    public override bool CanEnter()
    {
        if (currentAct != null && currentAct.ActLock)
        {
            ActLock = true;
            return true;
        }
        else if ((currentAct == null || !currentAct.ActLock) && ActLock)
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

public class Dash_NormalAct : ActBase
{
    private float dashForce = 8f, dashEndTime = 0f, dashCoolDown = 0.4f;
    private AnimationActBase dashAnimation;
    
    public Dash_NormalAct(PlayerDataBox box) : base(box)
    {
        dashAnimation = new DashAnim_Act(box);

        Priority = 10;
    }

    public override bool CanEnter()
    {
        if(box.hpComp.Evade) ActLock = false;

        if (Time.time - dashEndTime >= dashCoolDown 
            && box.sensor.IsGround 
            && box.input.IsInput(InputType.Dash_Evade)
            && box.stamina.CanUseDash
            && !box.hpComp.Evade) return true;

        return false;
    }

    public override void ActEnter()
    {
        box.SetMoveState(CharacterMoveState.Running);

        box.rotate.RotateLock = true;
        ActLock = true;
        box.stamina?.UseStamina();

        Vector3 dir = box.rigid.gameObject.transform.forward;

        box.rigid.linearVelocity = dir * dashForce;

        dashAnimation.OnEnterAnim();
    }

    public override void OnFixedUpdate()
    {
        if(dashAnimation.IsFinish())
        {
            box.rigid.linearVelocity = box.rigid.linearVelocity * 0.85f;
            ActLock = false;
            return;
        }
    }

    public override void ActEnd()
    {
        ActLock = false;
        box.rotate.RotateLock = false;
        dashEndTime = Time.time;
        dashAnimation.OnExitAnim();
    }
}

public class Dash_SlopeAct : ActBase
{
    private float dashForce = 8f, dashEndTime = 0f, dashCoolDown = 0.4f;
    private AnimationActBase dashAnimation;

    public Dash_SlopeAct(PlayerDataBox box) : base(box)
    {
        dashAnimation = new DashAnim_Act(box);

        Priority = 90;
    }

    public override bool CanEnter()
    {
        if(box.hpComp.Evade) ActLock = false;
        
        if (Time.time - dashEndTime >= dashCoolDown 
            && box.sensor.IsGround 
            && box.input.IsInput(InputType.Dash_Evade)
            && box.stamina.CanUseDash
            && box.sensor.IsSlope
            && !box.hpComp.Evade) return true;

        return false;
    }

    public override void ActEnter()
    {
        box.SetMoveState(CharacterMoveState.Running);
        
        box.rotate.RotateLock = true;
        ActLock = true;

        box.stamina?.UseStamina();

        Vector3 dir = box.rigid.gameObject.transform.forward;

        Vector3 moveDir = Vector3.ProjectOnPlane(dir, box.sensor.groundNormal).normalized;

        float flatness = Vector3.Dot(Vector3.up, box.sensor.groundNormal);

        float speedMul = 1f / Mathf.Max(flatness, 0.01f);

        float speed = speedMul * dashForce;

        speed = Mathf.Min(speed, dashForce * 1.5f);

        box.rigid.linearVelocity = moveDir * speed;

        dashAnimation.OnEnterAnim();
    }

    public override void OnFixedUpdate()
    {
        if(dashAnimation.IsFinish())
        {
            box.rigid.linearVelocity = box.rigid.linearVelocity * 0.85f;
            ActLock = false;
            return;
        }
    }
    public override void ActEnd()
    {
        box.rotate.RotateLock = false;
        dashEndTime = Time.time;
        dashAnimation.OnExitAnim();
    }
}