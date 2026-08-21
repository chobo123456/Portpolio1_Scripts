using UnityEngine;

public class CharacterAnimationProxy : MonoBehaviour
{
    private PlayerDataBox box;
    private bool warpEnable = false;
    private int curLayer;
    private float curMotionWarpEndTime, stopDistance = 3f, maxWarpSpeed = 10f;

    public void Initialize(PlayerDataBox box) => this.box = box;
    public void SetMotionWarpEndTime(float motionWarpEndTime) => curMotionWarpEndTime = motionWarpEndTime;
    public void OnAnimatorMove()
    {
        if(box == null 
            || box.animator == null 
            || box.rigid == null) return;

        if(!box.animator.applyRootMotion) return;

        AnimatorStateInfo state = box.animator.GetCurrentAnimatorStateInfo(curLayer);
        float normalizedTime = state.normalizedTime;
        float meshOffsetY = -0.5f;

        Vector3 curPos   = box.rigid.position;
        Vector3 deltaPos = box.animator.deltaPosition;
        Transform target  = box.sensor.LookTarget;

        if(target != null && warpEnable)
        {
            Vector3 diff = target.position - box.rigid.position;

            float distance = diff.magnitude;

            if(distance <= 6f && distance > stopDistance)
            {
                float timeLeft = curMotionWarpEndTime - normalizedTime;
                
                if(timeLeft > 0.01f)
                {
                    Vector3 warpDir = diff.normalized;
                    Vector3 targetPoint = target.position - (warpDir * stopDistance);

                    Vector3 moveDir = targetPoint - curPos;

                    Vector3 warpDelta = (moveDir / timeLeft) * Time.deltaTime;

                    float maxDistanceThisFrame = Time.deltaTime * maxWarpSpeed;
                    warpDelta = Vector3.ClampMagnitude(warpDelta, maxDistanceThisFrame);
                    deltaPos += warpDelta;
                }
            }
        }

        box.rigid.MovePosition(curPos + deltaPos);
        transform.localPosition = Vector3.zero + (Vector3.up * meshOffsetY);
    }

    public void OnAttackWarpStart(int layer)
    {
        curLayer = layer;

        warpEnable = true;
    }

    public void OnAttackWarpEnd()
    {
        warpEnable = false;
    }
}
