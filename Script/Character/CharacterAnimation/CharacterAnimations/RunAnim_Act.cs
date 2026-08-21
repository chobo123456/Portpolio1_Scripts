using UnityEngine;

public class RunAnim_Act : AnimationActBase
{
    private static readonly int moveParam = Animator.StringToHash("Speed");
    private float animation_Damp_Speed = 0.2f;

    public RunAnim_Act(PlayerDataBox _box) : base(_box) { }

    public override void OnUpdate()
    {
        box.animator.SetFloat(moveParam, 1f, animation_Damp_Speed, Time.deltaTime);
    }
}