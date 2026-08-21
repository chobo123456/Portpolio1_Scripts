using UnityEngine;

public class IdleAnim_Act : AnimationActBase
{
    private static readonly int moveParam = Animator.StringToHash("Speed");
    private float animation_Damp_Speed = 0.2f;

    public IdleAnim_Act(PlayerDataBox _box) : base(_box) { }

    public override void OnUpdate()
    {
        box.animator.SetFloat(moveParam, 0f, animation_Damp_Speed, Time.deltaTime);
    }
}