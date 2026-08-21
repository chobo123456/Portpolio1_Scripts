using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class BossDataBox : EnemyEntityDataBox
{
    public readonly EnemyHpBase<EnemyEntityDataBox> damageComp;
    public BossDataBox(Transform owner, int enemyId) : base(owner, enemyId)
    {
        damageComp = owner.GetComponentInChildren<LivingEntityHpBase>() as EnemyHpBase<EnemyEntityDataBox>;
        damageComp.SetDataBox(this);
    }
}