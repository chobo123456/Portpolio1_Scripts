using UnityEngine;
using UnityEngine.UIElements;

public class Sensor
{
    private readonly Transform ownerTr;

    // 설정값
    private float 
        ground_check_radius = 0.22f, 
        slope_check_distance = 0.8f, 
        slope_Limit = 50f, 
        landCountTime = 0f, 
        landTime = 2f,
        focusTime = 0f,
        focusStartTime = 0f, 
        currentScore = 0f;
        
    private CapsuleCollider col;
    private LayerMask groundLayer, targetLayer;

    public bool IsGround {get; private set;}    
    public bool IsLandAble {get; private set;}
    public bool IsSlope {get; private set;} = true;
    public float groundDistance {get; private set;}
    public float MinGroundDotProduct {get; private set;} = 0f;
    public Vector3 groundNormal {get; private set;} = Vector3.zero;
    public Transform LookTarget {get; private set;}

    private bool wasGround = false;

    public Sensor(Transform ownerTr)
    {
        this.ownerTr    = ownerTr;

        if(col == null) col = ownerTr.GetComponent<CapsuleCollider>();

        groundLayer = LayerMask.GetMask("Ground");
        targetLayer = LayerMask.GetMask("Enemy");

        if(MinGroundDotProduct <= 0f) MinGroundDotProduct = Mathf.Cos(slope_Limit * Mathf.Deg2Rad);
    }

    public void UpdateCheck()
    {
        CheckGround();
        CheckSlope();
    }

    //땅감지 
    private void CheckGround()
    {
        Vector3 rayStartPos = ownerTr.TransformPoint(col.center + Vector3.down * (col.height * 0.5f));
        IsGround = Physics.CheckSphere(rayStartPos, ground_check_radius, groundLayer);

        if(wasGround) landCountTime = Time.time;

        if(Time.time - landCountTime >= landTime)
            IsLandAble = true;   
        else
            IsLandAble = false;   
            
        wasGround = IsGround;     
    }

    private void CheckSlope()
    {
        Vector3 rayStartPos = ownerTr.TransformPoint(col.center);
        Vector3 startPos = rayStartPos + (Vector3.up * 0.1f);
        float radius = col.radius * 1.5f;
        if (Physics.SphereCast(startPos, radius, Vector3.down, out RaycastHit hit, slope_check_distance, groundLayer))
        {
            groundDistance = hit.distance;
            groundNormal = Vector3.Slerp(groundNormal, hit.normal, Time.fixedDeltaTime * 5f);
            float angle = Vector3.Angle(Vector3.up, groundNormal);

            IsSlope = angle > 0.1f && groundNormal.y > MinGroundDotProduct;
        }
        else
        {
            groundNormal = Vector3.up;
            IsSlope = false;
        }
    }

    public void SetGrounded()
    {
        IsGround = true;
    }

    public void CheckNearEnemy()
    {
        if(IsMustChangeTarget(LookTarget))
        {
            if(IsFocusingMinTime()) return;
            
            Collider[] cols = Physics.OverlapSphere(ownerTr.position, 15f, targetLayer);

            if(cols != null && cols.Length > 0)
            {
                for(int i = 0; i < cols.Length; i++)
                {
                    Collider targetCol = cols[i];

                    if(LookTarget == null)
                    {
                        Transform newLookTargetTr = targetCol.transform;
                        currentScore = CalculateEnemyScore(newLookTargetTr);
                        LookTarget = newLookTargetTr;
                        focusStartTime = Time.time;

                        continue;
                    }

                    Transform otherLookTargetTr = targetCol.transform;
                    float score = CalculateEnemyScore(otherLookTargetTr);

                    if(score >= currentScore)
                    {
                        LookTarget = otherLookTargetTr;   
                        focusStartTime = Time.time;
                    } 
                }

                if(LookTarget != null)
                {
                    Vector3 enemyPos = Camera.main.WorldToViewportPoint(LookTarget.position);
                    
                    if(enemyPos.x < 0f || enemyPos.x > 1f || enemyPos.y < 0f || enemyPos.y > 1f)
                        EventBus.Invoke<Transform>("SetCameraForceRotate", LookTarget);
                }

                return;
            }
        }
        else
        {
            LookTarget = null;   
        }
    }

    private float CalculateEnemyScore(Transform enemy)
    {
        float score = 0f;

        if(enemy == null || !enemy.gameObject.activeSelf || !enemy.gameObject.activeInHierarchy) return score;
        
        //거리
        float currentEnemyDistance  = (enemy.position - ownerTr.position).magnitude;

        score -= currentEnemyDistance;

        //시야
        Vector2 enemyScreenPos = Camera.main.WorldToScreenPoint(enemy.position);
        Vector2 screenCenter   = new Vector2(Screen.width, Screen.height) / 2f; //현재 해상도의 센터값을 구함

        float distance         = (enemyScreenPos - screenCenter).magnitude; //센터에서 적의 스크린상 좌표의 거리를구함
        float maxDistance      = Vector2.Distance(Vector2.zero, screenCenter); //(코너시작) 현재 해상도에서 나올수있는 최대 거리 계산

        score += 1f - (distance / maxDistance); //값 뒤집기 -> ex) 1 - 1(거리 / 최대 해상도 거리) = 0추가

        return score;
    }

    private bool IsFocusingMinTime()
    {
        if(LookTarget != null && Time.time - focusStartTime >= focusTime) return true;
        return false;
    }

    private bool IsMustChangeTarget(Transform enemy)
    {
        if(enemy == null || !enemy.gameObject.activeSelf || !enemy.gameObject.activeInHierarchy) return true;

        float distance = (enemy.position - ownerTr.position).magnitude;

        if(distance >= 5f) return true;

        return false;
    }
}
