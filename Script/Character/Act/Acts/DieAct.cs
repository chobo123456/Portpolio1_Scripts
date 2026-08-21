using UnityEngine;
using System.Collections.Generic;

//캐릭터 필수상태!
#region Die
public class DieAct : SubActManager
{
    public DieAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Die_NormalAct(box),
        });

        Priority = 100;
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

public class Die_NormalAct : ActBase
{
    private AnimationActBase animation;
    private bool isActive = false;
    public Die_NormalAct(PlayerDataBox box) : base(box)
    {
        Priority = 90;
        animation = new DieAnim_Act(box);
    }

    public override bool CanEnter()
    {
        if (box.hpComp.IsDie) return true;

        return false;
    }

    public override void ActEnter()
    {
        isActive = true;
        ActLock = true;
        box.rotate.RotateLock = true;
        animation.OnEnterAnim();

        box.rigid.useGravity = true;
        box.rigid.mass = 10f;
        box.rigid.linearVelocity = new Vector3(0f, box.rigid.linearVelocity.y, 0f);
    }

    public override void OnUpdate()
    {
        if(ActLock && animation.IsFinish())
        {
            box.rigid.linearVelocity = new Vector3(0f, box.rigid.linearVelocity.y, 0f);
            ActEnd();
        }
    }

    public override void ActEnd()
    {
        if(!isActive) return;

        isActive = false;

        animation.OnExitAnim();
        ActLock = false;
        box.rotate.RotateLock = false;
        box.rigid.mass = 1f;
        
        if(box.hpComp.IsDie) EventBus.Invoke<bool, bool>("OnCharacterDie", true, false);
    }
}
#endregion