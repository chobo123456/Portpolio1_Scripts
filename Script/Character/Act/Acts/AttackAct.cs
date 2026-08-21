using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AttackAct : SubActManager
{
    private List<ActBase> moveAct;
    public AttackAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Attack_NormalAct(box),
            new Attack_JumpAct(box)
        });

        Priority = 70;
    }

    public override bool CanEnter() 
    {
        var act = GetActBase();

        if (box.weapon == null || !box.weapon.IsEnable) return false;

        if(currentAct != null && currentAct.ActLock) {
            ActLock = true;
            return true;
        }
        else if((currentAct == null || !currentAct.ActLock) && ActLock)
        {
            ActLock = false;
            return false;
        }

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

public class Attack_NormalAct : ActBase
{
    private int inputBufferCount = 0;
    private bool isAttacking = false;
    private float attackEndTime = 0f;
    private AnimationActBase attackAnim;

    public Attack_NormalAct(PlayerDataBox box) : base(box) 
    {
        attackAnim = new AttackAnim_Act(box);
        Priority = 10;
    }

    public override bool CanEnter()
    {
        if (attackAnim == null) return false;
        
        if(inputBufferCount > 0 
            && isAttacking
            && attackAnim.CanInputBuffer())
        {
            EndAttack();
            StartAttack();
            inputBufferCount = 0;
            return true;
        }

        if (inputBufferCount < 1
            && isAttacking
            && box.input.IsInput(InputType.Attack)) inputBufferCount++;
 
        if (Time.time - attackEndTime >= 0.1f &&
            box.input.IsInput(InputType.Attack)) return true;

        return false;
    }

    public override void ActEnter()
    {
        EventBus.Invoke<float>("FollowCamera_SetDamp", 0.25f);

        StartAttack();
        SetMoveDinied();

        ActLock = true;
        isAttacking = true;
    }

    public override void OnUpdate()
    {
        SetMoveDinied();
        if (attackAnim.IsFinish()) ActLock = false;
    }

    public override void ActEnd()
    {
        if (!isAttacking) return;
        EventBus.Invoke<float>("FollowCamera_SetDamp", 0.035f);

        EndAttack();

        attackEndTime = Time.time;
        isAttacking = false;
        ActLock = false;
    }

    private void StartAttack()
    {
        box.sensor.CheckNearEnemy();
        box.rotate.OnAttackStartRotate();
        attackAnim.OnEnterAnim();
    }

    private void EndAttack()
    {
        box.rotate.OnAttackEndRotate();
        attackAnim.OnExitAnim();
    }

    private void SetMoveDinied()
    {
        Vector3 curVel = box.rigid.linearVelocity;
        curVel *= 0.4f;
        box.rigid.linearVelocity = new Vector3(curVel.x, box.rigid.linearVelocity.y ,curVel.z);
    }
}

public class Attack_JumpAct : ActBase
{
    private float attackEndTime = 0f;
    private AnimationActBase attackAnim;
    public Attack_JumpAct(PlayerDataBox box) : base(box)
    {
        attackAnim = new JumpAttackAnim_Attack(box);
        Priority = 50;
    }

    public override bool CanEnter()
    {
        if (!box.sensor.IsGround &&
            attackAnim != null &&
            box.sensor.groundDistance >= 0.5f &&
            Time.time - attackEndTime >= 0.3f &&
            box.input.IsInput(InputType.Attack)) return true;

        return false;
    }

    public override void ActEnter()
    {
        box.sensor.CheckNearEnemy();
        box.rotate.OnAttackStartRotate();

        box.rotate.RotateLock = true;
        box.rigid.useGravity = false;

        Vector3 curVel = box.rigid.linearVelocity;
        curVel *= 0.2f;

        float fallForce = 5.5f;
        box.rigid.linearVelocity = new Vector3(curVel.x, fallForce, curVel.z);

        attackAnim.OnEnterAnim();
        ActLock = true;
    }

    public override void OnUpdate()
    {
        if (attackAnim.IsFinish())
        {
            ActLock = false;
        }
    }

    public override void OnFixedUpdate()
    {
        box.rigid.linearVelocity = new Vector3(0f, box.rigid.linearVelocity.y, 0f);
    }

    public override void ActEnd()
    {
        box.rotate.OnAttackEndRotate();
        
        attackAnim.OnExitAnim();

        attackEndTime = Time.time;

        ActLock = false;

        box.rigid.useGravity = true;
        box.rotate.RotateLock = false;
    }

}