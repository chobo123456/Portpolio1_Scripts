using UnityEngine;

public class ChaseNode_Module : Enemy_NodeModule
{
    public override Node SetNode(EnemyEntityDataBox box)
    {
        float stopDistance  = box.enemyData.chaseStopDistance * box.enemyData.chaseStopDistance;
        
        Node search         = new SearchNode(box);
        Node unSearch       = new SearchNode(box, true);

        Node turn           = new TurnNode(box);
        Node turnForce      = new TurnNode(box, true);

        Node stand          = new StandNode(box);
        Node move           = new MoveNode(box, box.enemyData.enemyChaseSpeed, 0.5f, 2f);
        Node checkNear      = new DistanceCondition(box, DistanceConditionType.Less, stopDistance);
        Node checkFar       = new DistanceCondition(box, DistanceConditionType.Greater, stopDistance);

        Node beware         = new AnimationTriggerNode(box, EnemyAnimationType.Beware, "Beware", "Beware");
        Node startle        = new StartleNode(box, box.enemyData.chaseDetectRadius);

        Node wasChased      = new StateCheckNode(box as NormalEnemyDataBox, EnemyAct.Chase);
        Node setChase       = new StateSetNode(box as NormalEnemyDataBox, EnemyAct.Chase);
        Node setPatrol      = new StateSetNode(box as NormalEnemyDataBox, EnemyAct.Patrol);

        Node wait           = new WaitingNode(box, 1f);

        #region startle
        
        Node startleSequence = new Sequence(
            startle,
            turnForce,
            beware
        );
        
        #endregion

        #region Move
        
        Node moveNode = new Sequence(
            checkFar,
            turn,
            move
        );

        Node stopNode = new Sequence(
            checkNear,
            turn,
            stand
        );

        Node moveSelector = new Selector (
            stopNode,
            moveNode
        );

        #endregion

        Node moveOrStartle = new Selector(
            startleSequence,
            moveSelector
        );

        Node chaseState = new Sequence(
            setChase,
            moveOrStartle
        );

        Node chaseNode = new ConditionalEvoluator(
            search,
            chaseState
        );  

        Node unDetectButWasChased = new Sequence(
            unSearch,
            wasChased,
            stand,
            wait,
            setPatrol
        );

        Node chaseChooseSelector = new Selector(
            unDetectButWasChased,
            chaseNode
        );

        return chaseChooseSelector;
    }
}
