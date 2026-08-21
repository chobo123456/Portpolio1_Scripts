using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class CloneEnemyDataBox : EnemyEntityDataBox
{
    public readonly EnemyHpBase<EnemyEntityDataBox> damageComp;
    public CloneEnemyDataBox(Transform owner, int enemyId) : base(owner, enemyId, false) 
    {
        var damageComponent = owner.GetComponentInChildren<LivingEntityHpBase>();

        if(damageComponent == null) return;

        damageComp = damageComponent as EnemyHpBase<EnemyEntityDataBox>;
        damageComp?.SetDataBox(this);
    }
}