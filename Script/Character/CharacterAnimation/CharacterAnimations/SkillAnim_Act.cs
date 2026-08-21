using UnityEngine;

public class SkillAnim_Act : AnimationActBase
{
    private readonly int skillParam;

    public SkillAnim_Act(PlayerDataBox _box, int skillSlot) : base(_box)
    {
        skillParam = Animator.StringToHash($"Skill{skillSlot}");
    }

    public override void OnEnterAnim()
    {
        box.animator.applyRootMotion = true;
        box.animator.SetBool(skillParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(skillParam, false);
        box.animator.applyRootMotion = false;
    }

    public override bool IsFinish()
    {
        if(box.animator.IsInTransition(3)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(3);
 
        if(stateInfo.IsTag("Skill") && stateInfo.normalizedTime >= 0.95f) return true;
        
        return false;
    }
}