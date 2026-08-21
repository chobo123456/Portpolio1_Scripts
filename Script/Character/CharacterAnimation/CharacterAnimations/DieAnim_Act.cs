using UnityEngine;

public class DieAnim_Act : AnimationActBase
{
    private static readonly int dieParam = Animator.StringToHash("Die");

    public DieAnim_Act(PlayerDataBox _box) : base(_box){}

    public override void OnEnterAnim()
    {
        box.animator.applyRootMotion = false;
        box.animator.SetBool(dieParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(dieParam, false);

        box.animator.Update(0);
    }

    public override bool IsFinish()
    {
        if (box.animator.IsInTransition(0)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Die") && stateInfo.normalizedTime >= 0.95f) return true;

        return false;
    }
}