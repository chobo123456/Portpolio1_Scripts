using UnityEngine;

public class HitAnim_Act : AnimationActBase
{
    private static readonly int hitParam = Animator.StringToHash("Hit");

    public HitAnim_Act(PlayerDataBox _box) : base(_box){}

    public override void OnEnterAnim()
    {
        box.animator.SetBool(hitParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(hitParam, false);
    }

    public override bool IsFinish()
    {
        if (box.animator.IsInTransition(0)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Hit") && stateInfo.normalizedTime >= 0.55f) return true;

        return false;
    }
}