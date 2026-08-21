using UnityEngine;

public class AttackAnim_Act : AnimationActBase
{
    private readonly int[] attackParams;
    private readonly float[] attackMotionWarpEndTimes;
    private readonly float[] enableNextAttackEnableTimes;
    private readonly float[] endAttackTimes;
    public int combo = 0, currentParam;
    private AnimationCurve curve;
    private float attackEndTime = 0f;

    public AttackAnim_Act(PlayerDataBox _box) : base(_box)
    {
        attackParams = new int[]
        {
            Animator.StringToHash("Attack1"),
            Animator.StringToHash("Attack2"),
            Animator.StringToHash("Attack3"),
            Animator.StringToHash("AttackFinal"),
        };

        string funcName = "OnAttackWarpEnd";

        attackMotionWarpEndTimes = new float[]
        {
            GetFuncEventTime("Attack1", funcName),
            GetFuncEventTime("Attack2", funcName),
            GetFuncEventTime("Attack3", funcName),
            GetFuncEventTime("AttackFinal", funcName),
        };

        funcName = "EnableNextAttackFlag";

        enableNextAttackEnableTimes = new float[]
        {
            GetFuncEventTime("Attack1", funcName),
            GetFuncEventTime("Attack2", funcName),
            GetFuncEventTime("Attack3", funcName),
            GetFuncEventTime("AttackFinal", funcName),
        };

        funcName = "EndAttackFlag";

        endAttackTimes = new float[]
        {
            GetFuncEventTime("Attack1", funcName),
            GetFuncEventTime("Attack2", funcName),
            GetFuncEventTime("Attack3", funcName),
            GetFuncEventTime("AttackFinal", funcName),
        };

        curve = DataLoader.GetData<AnimationCurve>(DataType.AnimationCurve, 1000);
    }

    public override void OnEnterAnim()
    {
        if (Time.time - attackEndTime >= 0.7f) combo = 0;

        box.animationProxy.SetMotionWarpEndTime(GetCurrentMotionWarpEndTime());

        currentParam = GetCurrentParam();

        SetCurveSpeed(0f);
        box.animator.applyRootMotion = true;
        box.animator.SetBool(currentParam, true);
    }

    public override void OnExitAnim()
    {
        box.animator.SetBool(currentParam, false);
        box.animator.applyRootMotion = false;
        attackEndTime = Time.time;

        combo = (combo + 1) % 4;

        SetSpeed(1f);
    }

    private int GetCurrentParam()
    {
        return attackParams[combo];
    }

    private float GetCurrentMotionWarpEndTime()
    {
        return attackMotionWarpEndTimes[combo];
    }

    public override bool IsFinish()
    {
        if(box.animator.IsInTransition(1)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(1);
        float cancelTime = endAttackTimes[combo];

        SetCurveSpeed(stateInfo.normalizedTime);

        return stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= cancelTime;
    }

    public override bool CanInputBuffer() 
    {
        if(box.animator.IsInTransition(1)) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(1);
        float enableNextAttackTime = enableNextAttackEnableTimes[combo];

        return stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= enableNextAttackTime;
    }

    private void SetCurveSpeed(float normalziedTime)
    {
        float speedMul = curve.Evaluate(normalziedTime);
        SetSpeed(speedMul);
    }

    private void SetSpeed(float speed)
    {
        box.animator.speed = speed;
    }
}