using UnityEngine;
using System.Collections.Generic;

public class PatrolNode_Module : Enemy_NodeModule
{
    public Vector3[] patrolPoint;
    public void SetPatrolPoint(Vector3[] newPatrolPoint)
    {
        patrolPoint = newPatrolPoint;
    }

    public override Node SetNode(EnemyEntityDataBox box)
    {
        float stopDistance = box.enemyData.patrolStopDistance * box.enemyData.patrolStopDistance;

        Node isGreater  = new DistanceCondition(box, DistanceConditionType.Greater, stopDistance);
        Node turn       = new TurnNode(box);
        Node move       = new MoveNode(box, box.enemyData.enemyPatrolSpeed, 0f, 0.95f);
        
        Node isLess     = new DistanceCondition(box, DistanceConditionType.Less, stopDistance);
        Node stand      = new StandNode(box);
        Node time       = new TimeCondition(box.enemyData.standTime);

        //멈춤
        Node stand_Sequence = new Sequence(
            isLess,
            stand,
            time
        );

        //걷기
        Node move_Sequence = new Sequence(
            isGreater,
            turn,
            move
        );

        //새 정찰포인트 찾기
        Node patrolNode = new PatrolPointSettingNode(box, patrolPoint, stopDistance);

        Node stopormove = new Selector(stand_Sequence, move_Sequence);

        Node patrol = new Sequence(
            patrolNode,
            stopormove
        );

        return patrol;
    }
}
