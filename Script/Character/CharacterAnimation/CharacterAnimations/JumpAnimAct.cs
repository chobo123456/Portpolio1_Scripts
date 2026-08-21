using UnityEngine;

public class JumpAnimAct : AnimationActBase
{
    private readonly int jumpParams = Animator.StringToHash("Jump");

    public JumpAnimAct(PlayerDataBox _box) : base(_box){}

    public override void OnEnterAnim()
    {
        box.animator.SetTrigger(jumpParams);
    }
}
