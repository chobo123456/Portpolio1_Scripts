using UnityEngine;

public class MoveAnim_Act : AnimationActBase
{
    private static readonly int moveParam = Animator.StringToHash("Speed");
    private float animation_Damp_Speed = 0.2f;

    public MoveAnim_Act(PlayerDataBox _box) : base(_box) { }

    public override void OnUpdate()
    {
        box.animator.SetFloat(moveParam, 0.45f, animation_Damp_Speed, Time.deltaTime);
    }
}
