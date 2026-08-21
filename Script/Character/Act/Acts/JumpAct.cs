using UnityEngine;
using System.Collections.Generic;

#region Jump
public class JumpAct : SubActManager
{
    public JumpAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Jump_NormalAct(box),
        });

        Priority = 40;
    }

    public override bool CanEnter()
    {
        if(currentAct != null && currentAct.ActLock)
        {
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

public class Jump_NormalAct : ActBase
{
    private readonly float jumpLockTime = 0.12f, jumpForce = 5.758f;
    private readonly JumpAnimAct jumpAnim;
    private float jumpStartTime = 0f;
    
    public Jump_NormalAct(PlayerDataBox box) : base(box) 
    {
        Priority = 10;

        jumpAnim = new(box);
    }

    public override bool CanEnter()
    {
        if (box.sensor.IsGround && box.input.IsInput(InputType.Jump))
        {
            return true;
        } 

        return false;
    }

    public override void ActEnter()
    {
        box.surfaceAlignment.OnJump();
        jumpAnim.OnEnterAnim();

        ActLock = true;
        jumpStartTime = Time.time;

        box.rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public override void OnUpdate()
    {
        if(Time.time - jumpStartTime >= jumpLockTime) ActLock = false;
    }

}
#endregion