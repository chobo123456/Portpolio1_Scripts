using UnityEngine;

public abstract class EnemyAnimationEventBase : MonoBehaviour
{
    public event System.Action<int> OnAnimationEnd;
    protected EnemyEntityDataBox box;
    protected LayerMask targetLayer;

    public void Initialize(EnemyEntityDataBox box)
    {
        this.box = box;
        targetLayer = LayerMask.GetMask("CharacterDamage");
    }

    public void AnimationEnd(int parameter)
    {
        OnAnimationEnd?.Invoke(parameter);
    }
}

public class EnemyAnimationEvent : EnemyAnimationEventBase
{
    public void OnAttack()
    {
        if(box == null || box.enemyData == null) return;

        Vector3 startCastPos = transform.position + (transform.rotation * Vector3.forward * 0.15f);
        float attackRange = box.enemyData.attackRange / 2;

        Collider[] cols = Physics.OverlapBox(
            startCastPos,
            new Vector3(attackRange, attackRange, attackRange),
            Quaternion.identity,
            targetLayer
        );

        if(cols != null && cols.Length > 0)
        {
            for(int i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                var comp = col.GetComponentInChildren<IDamageable>();
 
                if(comp == null) 
                {
                    comp = col.GetComponent<IDamageable>();
                }

                if(comp == null)
                {
                    continue;
                } 

                Vector3 knockbackDir = col.gameObject.transform.position - transform.position;

                comp.TakeDamage(new DamageSource
                {
                    damageAmount = box.enemyData.attackDamage,
                    knockbackDir = knockbackDir,
                    hit_vfxId = box.enemyData.hit_VfxId
                });
            }
        }
    }
}
