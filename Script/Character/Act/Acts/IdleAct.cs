using UnityEngine;
using System.Collections.Generic;

#region Idle
public class IdleAct : SubActManager
{
    public IdleAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Idle_NormalAct(box),
            new Idle_SlopeAct(box),
        });
        Priority = 0;
    }

    public override bool CanEnter()
    {
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

public class Idle_NormalAct : ActBase
{
    private readonly IdleAnim_Act idleAnim;

    public Idle_NormalAct(PlayerDataBox box) : base(box)
    {
        idleAnim = new(box);
        Priority = 10;
    }

    public override bool CanEnter()
    {
        if (box.input.GetMoveInput().sqrMagnitude < 0.01f) return true;

        return false;
    }
    
    public override void OnFixedUpdate()
    {
        box.SetMoveState(CharacterMoveState.Idle);
        
        Vector3 curVel = box.rigid.linearVelocity;
        curVel *= 0.65f;

        box.rigid.linearVelocity = new Vector3(curVel.x, box.rigid.linearVelocity.y, curVel.z);
        idleAnim.OnUpdate();
    }
}

public class Idle_SlopeAct : ActBase
{
    private readonly IdleAnim_Act idleAnim;
    public Idle_SlopeAct(PlayerDataBox box) : base(box)
    {
        idleAnim = new(box);
        Priority = 90;
    }

    public override bool CanEnter()
    {
        if (box.input.GetMoveInput().sqrMagnitude < 0.01f &&
            box.sensor.IsSlope &&
            box.sensor.IsGround &&
            box.rigid.linearVelocity.y <= box.stat.player.GetMoveSpeed()) return true;

        return false;
    }

    public override void OnFixedUpdate()
    {   
        box.SetMoveState(CharacterMoveState.Idle);

        if (box.sensor.groundDistance >= 0.8f)
        {
            box.rigid.linearVelocity = (Physics.gravity * 2f) * Time.fixedDeltaTime;
        }
        else
        {
            box.rigid.linearVelocity = -Physics.gravity * Time.fixedDeltaTime;
        }

        idleAnim.OnUpdate();
    }
}


#endregion