using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEntityDataBox : LivingEntityDataBox
{
    public readonly EnemyData enemyData;
    public readonly NavMeshAgent nav;
    public readonly EnemyAnimationStorage enemyAnimationStorage;
    public readonly EnemyAnimationEventBase enemyAnimationEvent;
    public readonly EnemyAnimationProxy enemyAnimationProxy;
    public readonly EnemyHitFeedback visualFeedback;
    
    public EnemyEntityDataBox(Transform owner, int enemyId, bool isFullInit = true) : base(owner, enemyId)
    {
        nav = owner.GetComponent<NavMeshAgent>();
        enemyData = DataLoader.GetData<EnemyData>(DataType.Enemy, enemyId);
        visualFeedback = new(this);

        enemyAnimationEvent = owner.GetComponentInChildren<EnemyAnimationEventBase>();
        enemyAnimationEvent?.Initialize(this);

        if(!isFullInit) return;

        enemyAnimationStorage = new();

        enemyAnimationProxy = owner.GetComponentInChildren<EnemyAnimationProxy>();
        enemyAnimationProxy?.Initialize(this);
    }
}