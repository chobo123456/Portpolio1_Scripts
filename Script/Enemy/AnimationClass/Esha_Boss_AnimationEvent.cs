using UnityEngine;
using System.Collections;

public class Esha_Boss_AnimationEvent : EnemyAnimationEventBase
{
    public int prefabId;
    public void OnEnable()
    {
        this.RunRoutine(WaitEvent());
    }

    IEnumerator WaitEvent()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady && EventBus.HasEvent("Get_EnemyClone"));
        _ = EventBus.Invoke_Func<int, IClone>("Get_EnemyClone", prefabId);
    }

    #region summon
    public void SummonEshaClone()
    {
        IClone eshaClone = EventBus.Invoke_Func<int, IClone>("Get_EnemyClone", prefabId);
        if(eshaClone == null) return;

        eshaClone.SetPosition(box.rigid.transform.position);
        eshaClone.SetRotation(box.rigid.transform.rotation);

        ICloneAble cloneDataBox = box as ICloneAble;
        cloneDataBox.clones.Add(eshaClone);

        eshaClone.SetActive(true);
    }

    #endregion

    public void OnAttack(float multiply)
    {
        if (box == null || box.enemyData == null) return;

        Vector3 startCastPos = transform.position + (transform.rotation * Vector3.forward * 0.15f);
        float attackRange = box.enemyData.attackRange / 2;

        Collider[] cols = Physics.OverlapBox(
            startCastPos,
            new Vector3(attackRange, attackRange, attackRange),
            Quaternion.identity,
            targetLayer
        );

        if (cols != null && cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                var comp = col.GetComponentInChildren<IDamageable>();

                if (comp == null) comp = col.GetComponent<IDamageable>();

                if (comp == null) continue;

                Vector3 knockbackDir = col.gameObject.transform.position - transform.position;

                comp.TakeDamage(new DamageSource
                {
                    damageAmount = box.enemyData.attackDamage * multiply,
                    knockbackDir = knockbackDir,
                    hit_vfxId = box.enemyData.hit_VfxId
                });
            }
        }
    }

    public void FinishGroggy()
    {
        (box as IGroggyAble).isGroggy = false;
    }
}
