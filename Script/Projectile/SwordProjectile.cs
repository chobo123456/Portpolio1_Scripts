using UnityEngine;

public class SwordProjectile : ProjectileBase
{
    private float duration = 0.25f, elapsedTime = 0f;
    public override void Execute()
    {
        elapsedTime = 0f;
        readyToFire = true;
    }
    
    private void Update()
    {
        if(!readyToFire) return;

        if(projectileType == ProjectileType.Curve)
            Curve();
    }

    private void Curve()
    {
        Vector3 direction = endPos - startPos;
        Vector3 shotDirection = Vector3.zero;
        
        float randomValue = Random.Range(-1, 1);

        if(randomValue <= 0f) shotDirection = Vector3.left;
        else shotDirection = Vector3.right;

        //p1
        Vector3 centerPos = (startPos + endPos) / 2f + shotDirection * 5f;

        //t
        elapsedTime += Time.deltaTime;
        float curPer = elapsedTime / duration;

        if(curPer >= 1f)
        {
            SetPosition(endPos);
            EndProjectile();
            return;
        }

        Vector3 bezier = BezierUtil.GetBezier_Vector3(startPos, centerPos, endPos, curPer);

        Vector3 currentPos = transform.position;
        Vector3 moveDir = bezier - currentPos;

        float distance = moveDir.magnitude;

        if(distance > 0)
        {
            if(SetDamage(moveDir.normalized, distance))
                return;
        }

        SetPosition(bezier);
        SetRotation(moveDir);
    }

    private void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    private void SetRotation(Vector3 direction)
    {
        if(direction == Vector3.zero) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private bool SetDamage(Vector3 direction, float distance)
    {
        if(!Physics.SphereCast(transform.position, 0.5f, direction, out var hit, distance, targetLayer)) 
            return false;

        if(!hit.collider.TryGetComponent<IDamageable>(out var damageComp)) 
            return false;

        source.knockbackDir = (endPos - startPos).normalized;
        damageComp.TakeDamage(source);

        EndProjectile();

        return true;
    }

    private void EndProjectile()
    {
        this.gameObject.SetActive(false);
        readyToFire = false;
    }
}
