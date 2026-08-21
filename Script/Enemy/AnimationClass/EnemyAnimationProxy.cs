using UnityEngine;

public class EnemyAnimationProxy : MonoBehaviour
{
    private EnemyEntityDataBox box;
    private Vector3 originLocalPosition;
    public void Initialize(EnemyEntityDataBox box)
    {
        this.box = box;
        originLocalPosition = this.box.rigid.transform.FindTarget("Mesh").localPosition;
    }

    public void OnAnimatorMove()
    {
        if(box == null || box.animator == null) return;

        Vector3 curPos   = box.rigid.position;
        Vector3 deltaPos = box.animator.deltaPosition;
        Quaternion curRot = box.rigid.rotation;
        Quaternion deltaRot = box.animator.deltaRotation; 

        Vector3 animatorMatchPos = curPos + deltaPos;
        Quaternion animatorMatchRot = curRot * deltaRot;
        
        box.rigid.MovePosition(animatorMatchPos);
        box.rigid.MoveRotation(animatorMatchRot);

        transform.localPosition = originLocalPosition;
        transform.localRotation = Quaternion.identity;
    }
}
