using UnityEngine;
using System.Collections.Generic;


public enum DistanceConditionType
{
    Less,
    Greater
}
public class DistanceCondition : LeafNode<EnemyEntityDataBox>
{
    private DistanceConditionType type;
    private float stopDistance = 4f;
    public DistanceCondition(EnemyEntityDataBox box, DistanceConditionType type, float stopDistance) : base(box)
    {
        this.stopDistance = stopDistance;
        this.type = type;
    }

    public override EnemyState Execute()
    {
        bool result = IsNotNearGoal();

        if(type == DistanceConditionType.Less) 
            return result ? EnemyState.Fail : EnemyState.Success;
        else if(type == DistanceConditionType.Greater) 
            return result ? EnemyState.Success : EnemyState.Fail;
        
        return EnemyState.Fail;
    }

    private bool IsNotNearGoal()
    {
        float distance = (box.nav.destination - box.rigid.transform.position).sqrMagnitude;

        if(distance > stopDistance) return true;

        return false;
    }
}

public class TimeCondition : Node
{
    private float startTime = 0f, targetTime = 1f;
    public TimeCondition(float time)
    {
        targetTime = time;
    }
    
    public override EnemyState Execute()
    {
        if(startTime > 0f)
        {
            startTime -= Time.deltaTime;

            return EnemyState.Success;
        }

        if(startTime == -1f)
        {
            startTime = targetTime;

            return EnemyState.Running;
        }

        startTime = -1f;

        return EnemyState.Fail;
    }

    public override void Undo(bool isRootUndo = false)
    {
        startTime = -1f;
    }
}

public class ConditionalEvoluator : Node
{
    private Node condition, child;
    public ConditionalEvoluator(Node condition, Node child)
    {
        this.condition = condition;
        this.child = child;
    }

    public override EnemyState Execute()
    {
        EnemyState conditionState = condition.Execute();

        if(conditionState == EnemyState.Fail)
        {
            Undo();
            return EnemyState.Fail;
        }
        
        return child.Execute();
    }

    private void Undo()
    {
        condition.Undo();
        child.Undo();
    }
}

public class GroggyCondition : LeafNode<EnemyEntityDataBox>
{
    private readonly IGroggyAble groggyClass;
    public GroggyCondition(EnemyEntityDataBox box) : base(box)
    {
        groggyClass = box as IGroggyAble;
    }
    
    public override EnemyState Execute()
    {
        if (groggyClass.isGroggy)
            return EnemyState.Success;

        return EnemyState.Fail;
    }
}

public class PhaseCondition : LeafNode<EnemyEntityDataBox>
{
    private readonly IPhaseAble phaseClass;
    private int conditionPhase;
    public PhaseCondition(EnemyEntityDataBox box, int conditionPhase = 0) : base(box)
    {
        phaseClass = box as IPhaseAble;
        this.conditionPhase = conditionPhase;
    }

    public override EnemyState Execute()
    {
        if (phaseClass.phase == conditionPhase)
            return EnemyState.Success;

        return EnemyState.Fail;
    }
}