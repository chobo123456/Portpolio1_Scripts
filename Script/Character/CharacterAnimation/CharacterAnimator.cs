using UnityEngine;
using System.Collections;

public enum CharacterAnimParam
{
    Attack1,
    Attack2,
    Attack3
}

//Idle,Move,Run을 하나로 만들지않은 이유는 임계값을 상태에서 일일히 적어서 줘야하는데 상태가 애니메이션의 임계값을 알지 못하게
//하려고입니다.

public class CharacterAnimator
{
    //필드
    private static readonly int MoveInput_Param = Animator.StringToHash("MoveInput");
    private static readonly int Horizontal_Movement_Param = Animator.StringToHash("Speed");
    private static readonly int Vertical_Movement_Param = Animator.StringToHash("FallSpeed");
    private static readonly int Ground_Param = Animator.StringToHash("OnGround");
    private readonly PlayerDataBox box;

    //   떨어짐
    private float min_Vertical_Speed = -4f;
    private float max_Vertical_Speed = 0f;

    //애니메이션 댐핑 속도
    private float animation_Damp_Speed = 0.2f, animation_FallDampSpeed = 0.2f;

    public CharacterAnimator(PlayerDataBox _box)
    {
        box = _box;
    }

    public void UpdateAnimation()
    {
        if(box.animator == null) return;

        if(!GameState.IsActive()) EscapeMove();

        OnUnGround();
        OnFall();
    }

    private void EscapeMove()
    {
        Vector3 horizontalVel = new Vector3(box.rigid.linearVelocity.x, 0f, box.rigid.linearVelocity.z);

        float moveSpeed = Mathf.Abs(horizontalVel.magnitude);
        float clampedSpeed = Mathf.Clamp(moveSpeed, 0, 0.45f);

        box.animator.SetFloat(Horizontal_Movement_Param, clampedSpeed, animation_Damp_Speed, Time.deltaTime);
    }

    private void OnUnGround()
    {
        // 땅 파라미터 조절
        bool isGround = box.sensor.IsGround;
        box.animator.SetBool(Ground_Param, isGround);

        // 떨어지는 파라미터 조절
        float clampedParam_Air = Mathf.Clamp(box.rigid.linearVelocity.y, min_Vertical_Speed, max_Vertical_Speed);

        box.animator.SetFloat(Vertical_Movement_Param, clampedParam_Air, animation_FallDampSpeed, Time.deltaTime);
    }

    private void OnFall()
    {
        float input = box.input.GetMoveInput().magnitude >= 0.1f ? 1f : 0f;

        box.animator.SetFloat(MoveInput_Param, input, animation_FallDampSpeed, Time.deltaTime);
    }
}














