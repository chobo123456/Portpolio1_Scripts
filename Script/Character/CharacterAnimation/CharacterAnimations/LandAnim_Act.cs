using UnityEngine;

public class LandAnim_Act : AnimationActBase
{
    private static readonly int landParam = Animator.StringToHash("Landing");

    public LandAnim_Act(PlayerDataBox _box) : base(_box) {}

    public override void OnEnterAnim()
    {
        box.animator.SetBool(landParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(landParam, false);
    }

    public override bool IsFinish()
    {
        if (box.animator.IsInTransition(0)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Landing") && stateInfo.normalizedTime >= 0.95f) return true;

        return false;
    }
}