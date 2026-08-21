using UnityEngine;
using System.Collections.Generic;
public class LandAct : SubActManager
{
    private List<ActBase> moveAct;
    public LandAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Land_NormalAct(box)
        });

        Priority = 85;
    }

    public override bool CanEnter() 
    {
        if(currentAct != null && currentAct.ActLock) {
            ActLock = true;
            return true;
        }
        else if((currentAct == null || !currentAct.ActLock ) && ActLock)
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

public class Land_NormalAct : ActBase
{
    private AnimationActBase landAnim;
    public Land_NormalAct(PlayerDataBox box) : base(box) 
    {
        landAnim = new LandAnim_Act(box);
        Priority = 10;
    }

    public override bool CanEnter()
    {
        if (landAnim != null && 
            box.sensor.IsLandAble && 
            box.sensor.IsGround
            ) return true;

        return false;
    }

    public override void ActEnter()
    { 
        ActLock = true;

        box.rotate.RotateLock = true;

        box.rigid.linearVelocity = Vector3.zero;

        landAnim.OnEnterAnim();
    }

    public override void OnUpdate()
    {
        bool isEnd = landAnim.IsFinish();

        if(isEnd) ActLock = false;
    }

    public override void ActEnd()
    {
        box.rotate.RotateLock = false;
        landAnim.OnExitAnim();
        ActLock = false;
    }
}