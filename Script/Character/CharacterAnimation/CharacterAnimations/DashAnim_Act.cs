using UnityEngine;

public class DashAnim_Act : AnimationActBase
{
    private static readonly int dashParam = Animator.StringToHash("Dash");

    public DashAnim_Act(PlayerDataBox _box) : base(_box) {}

    public override void OnEnterAnim()
    {
        box.animator.applyRootMotion = false;
        box.animator.SetBool(dashParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(dashParam, false);
    }

    public override bool IsFinish()
    {
        if (box.animator.IsInTransition(0)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Dash") && stateInfo.normalizedTime >= 0.7f) return true;

        return false;
    }
}
