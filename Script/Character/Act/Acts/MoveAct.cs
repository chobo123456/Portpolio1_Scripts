using UnityEngine;
using System.Collections.Generic;

#region Move

public class MoveAct : SubActManager
{
    public MoveAct(PlayerDataBox box) : base(box)
    {
        base.SetAct(new() { 
            new Move_Act(box),
        });
        Priority = 10;
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

public class Move_Act : ActBase
{
    private readonly MoveAnim_Act moveAnim;
    private readonly RunAnim_Act runAnim;
    private Vector3 moveDir;
    private float accellation = 40f;
    public Move_Act(PlayerDataBox box) : base(box) 
    {
        moveAnim = new(box);
        runAnim  = new(box);
        Priority = 10;
    }

    public override bool CanEnter()
    {
        if (box.input.GetMoveInput().sqrMagnitude > 0.01f) return true;

        return false;
    }
    
    public override void OnUpdate()
    {
        moveDir = Input();
    }

    public override void OnFixedUpdate()
    {
        Move(moveDir); 
    }

    private Vector3 Input()
    {
        //Match CameraRotate And Move
        Vector3 moveInput = box.input.GetMoveInput();
        moveInput.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 moveDir = (cameraRight * moveInput.x) + (cameraForward * moveInput.z);
        moveDir.Normalize();

        return moveDir;
    }

    private void Move(Vector3 moveDir)
    {   
        bool isRun = box.GetMoveState() == CharacterMoveState.Running;

        float desiredSpeed = isRun ?
            box.stat.player.GetMoveSpeed() * 1.2f : box.stat.player.GetMoveSpeed();
        float currentSpeed = new Vector3(box.rigid.linearVelocity.x, 0f, box.rigid.linearVelocity.z).magnitude;
        float speed = Mathf.MoveTowards(currentSpeed, desiredSpeed, Time.fixedDeltaTime * accellation);

        Vector3 finalVel = Vector3.zero;
        Vector3 normal = box.sensor.groundNormal;

        if(box.sensor.IsSlope) //PlaneMove
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir, normal).normalized;
            finalVel = slopeDir * Mathf.Max(0f, speed); 
            
            if(normal.y < box.sensor.MinGroundDotProduct)
            {
                Vector3 slideDirection = new Vector3(normal.x, -normal.y, normal.z);
                float slideSpeed = (1 - normal.y) * (Mathf.Abs(Physics.gravity.y) * 2f);
                finalVel += slideDirection * slideSpeed;
            }
        }
        else //NormalMove
        {
            finalVel = moveDir * Mathf.Max(0f, speed); 

            if(box.sensor.IsGround)
                finalVel.y = -2f;
            else
                finalVel.y = box.rigid.linearVelocity.y;
        }

        box.rigid.linearVelocity = finalVel;

        if(isRun) runAnim.OnUpdate(); 
        else moveAnim.OnUpdate();
    }

    public override void ActEnd()
    {
        if(!GameState.IsTutorial()) return;
        
        if(box != null && !box.rigid.isKinematic) 
            box.rigid.linearVelocity = Vector3.zero;
    }
}

#endregion
