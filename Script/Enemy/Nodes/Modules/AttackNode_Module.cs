using UnityEngine;

public class AttackNode_Module : Enemy_NodeModule
{
    public override Node SetNode(EnemyEntityDataBox box)
    {
        float distance = box.enemyData.attackRange * box.enemyData.attackRange;

        Node search     = new SearchNode(box);
        Node checkNear  = new DistanceCondition(box, DistanceConditionType.Less, distance);
        Node turn       = new TurnNode(box, true);
        Node attackNode = new AttackNode(box);
        Node stand      = new StandNode(box, 0.5f);
        Node turnAnim   = new AnimTurnNode(box, 3);
        Node back       = new MoveBackNode(box, box.enemyData.enemyPatrolSpeed, box.enemyData.attackRange, -1f, 1f);

        Node normalAttackSequence = new Sequence(
            search,
            checkNear,
            turn,
            attackNode,
            stand,
            search,
            turnAnim,
            back,
            stand
        );

        return normalAttackSequence;
    }
}