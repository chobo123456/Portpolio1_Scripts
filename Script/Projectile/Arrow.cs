using UnityEngine;

public class Arrow : ProjectileBase
{
    private float duration = 0.25f, elapsedTime = 0f;
    public override void Execute()
    {
        Util.Log("발사");
        
        elapsedTime = 0f;
        readyToFire = true;
    }
    
    private void Update()
    {
        if(!readyToFire) return;

        if(projectileType == ProjectileType.Curve)
            Curve();
        else
            Straight();
    }

    private void Curve()
    {
        Vector3 direction = endPos - startPos;
        
        //p1
        Vector3 centerPos = (startPos + endPos) / 2f + Vector3.up * 5f;

        //t
        elapsedTime += Time.deltaTime;
        float curPer = elapsedTime / duration;

        if(curPer >= 1f)
        {
            SetPosition(endPos);
            EndArrow();
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

    private void Straight()
    {
        Vector3 direction = endPos - startPos;

        //t
        elapsedTime += Time.deltaTime;
        float curPer = elapsedTime / duration;

        if(curPer >= 1f)
        {
            SetPosition(endPos);
            EndArrow();
            return;
        }
        
        Vector3 lerpVector = Vector3.Lerp(startPos, endPos, curPer);

        Vector3 currentPos = transform.position;
        Vector3 moveDir = lerpVector - currentPos;

        float distance = moveDir.magnitude;

        if(distance > 0)
        {
            if(SetDamage(moveDir.normalized, distance))
                return;
        }

        SetPosition(lerpVector);
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

        EndArrow();

        return true;
    }

    private void EndArrow()
    {
        this.gameObject.SetActive(false);
        readyToFire = false;
    }
}
