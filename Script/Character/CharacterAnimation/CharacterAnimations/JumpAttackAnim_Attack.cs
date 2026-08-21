using UnityEngine;

public class JumpAttackAnim_Attack : AnimationActBase
{
    private static readonly int attackParam = Animator.StringToHash("JumpAttack");

    public JumpAttackAnim_Attack(PlayerDataBox _box) : base(_box){}

    public override void OnEnterAnim()
    {
        box.animator.SetBool(attackParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(attackParam, false);
    }

    public override bool IsFinish()
    {
        if (box.animator.IsInTransition(2)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(2);

        if (stateInfo.IsTag("JumpAttack_Attack") && stateInfo.normalizedTime >= 0.95f) return true;

        return false;
    }
}