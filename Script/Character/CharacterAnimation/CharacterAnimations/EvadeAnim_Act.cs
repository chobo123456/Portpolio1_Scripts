using UnityEngine;

public class EvadeAnim_Act : AnimationActBase
{
    private static readonly int evadeParam = Animator.StringToHash("Evade");
    private static readonly int evadeFloatParam = Animator.StringToHash("EvadeDirection");
    public EvadeAnim_Act(PlayerDataBox _box) : base(_box){}

    public override void SetFloat(float value)
    {
        box.animator.SetFloat(evadeFloatParam, value);
    }

    public override void OnEnterAnim()
    {
        box.animator.applyRootMotion = true;
        box.animator.SetBool(evadeParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(evadeParam, false);
        box.animator.applyRootMotion = false;
    }

    public override bool IsFinish()
    {
        if (box.animator.IsInTransition(4)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(4);

        if (stateInfo.IsTag("Evade") && stateInfo.normalizedTime >= 0.75f) return true;

        return false;
    }
}