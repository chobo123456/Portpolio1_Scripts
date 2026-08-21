using UnityEngine;
public interface ISkillObject
{
    void SetActive(bool isActive);
    void SetRotate(Quaternion rotate);
    void SetPosition(Vector3 vector);
    void Initialize();
    void TryAttack(DamageSource source);    
}

public class Skill_Attacker : MonoBehaviour, ISkillObject
{
    private BoxCollider col;
    private LayerMask layer;
    private Vector3 size;
    public void SetRotate(Quaternion rotate)
    {
        transform.rotation = rotate;
    }

    public void SetPosition(Vector3 vector)
    {
        transform.position = vector;
    }
    
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    public void Initialize()
    {
        if(col == null) 
        {
            col     = GetComponentInChildren<BoxCollider>();
            size    = col.size / 2f;
        }
        layer = LayerMask.GetMask("EnemyDamage");
    }

    public void TryAttack(DamageSource source)
    {
        Vector3 startPos = transform.position;

        RaycastHit[] hits = Physics.BoxCastAll(
            startPos,
            size,
            transform.forward,
            transform.rotation,
            1f,
            layer
        );

        if(hits != null && hits.Length > 0)
        {
            for(int i = 0; i < hits.Length; i++)
            {
                Collider targetCol = hits[i].collider;

                var comp = targetCol.GetComponent<IDamageable>();

                if(comp != null)
                {
                    Vector3 knockBackDir = (col.transform.position - transform.position).normalized;

                    source.knockbackDir = knockBackDir;
                    comp.TakeDamage(source);
                }
            }
        }
    }
}
