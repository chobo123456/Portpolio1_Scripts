using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyAct
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public class NormalEnemyDataBox : EnemyEntityDataBox
{
    public readonly EnemyHpBase<NormalEnemyDataBox> damageComp;
    public System.Action undoBt;
    public System.Action onDie;
    public EnemyAct act;
    public NormalEnemyDataBox(Transform owner, int enemyId) : base(owner, enemyId) 
    {
        damageComp = owner.GetComponentInChildren<LivingEntityHpBase>() as EnemyHpBase<NormalEnemyDataBox>;
        damageComp.SetDataBox(this);
    }
}