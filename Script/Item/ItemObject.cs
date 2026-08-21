using UnityEngine;
using System.Collections;

public interface IItemObject  : IInteract
{
    void ItemFollow(Transform target);
}

public class ItemObject : MonoBehaviour, IItemObject
{
    private int itemDropMinAmount = 1, itemDropMaxAmount = 1, itemId = 1;
    private int instanceId = 0;
    
    private bool follow = false, enabledInteract = false;

    private Transform target;
    
    private LayerMask exception_targetLayer;

    public void OnEnable()
    {
        exception_targetLayer = LayerMask.GetMask("Character");
    }

    private void OnDisable()
    {
        EventBus.Invoke<int>("UnShowItemInteractBar", instanceId);
    }
    
    private void Update()
    {
        if(!enabledInteract) return;

        if(follow)
        {
            if(GetDistance() <= 0.75f)
            {
                follow = false;
                gameObject.SetActive(false);
                return;
            }

            if(target == null) // Exception 감지 코드
            {
                target = FindNewTarget();

                if(target == null)
                    return;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            transform.Translate(direction * 10f * Time.deltaTime);
        }
    }

    private Transform FindNewTarget()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 4f, exception_targetLayer);

        if(cols.Length > 0)
        {
            return cols[0].transform;
        }
        return null;
    }
    
    private float GetDistance()
    {
        return (target.position - transform.position).sqrMagnitude;
    }

    private void OnTriggerEnter()
    {
        if(!enabledInteract) return;

        EventBus.Invoke<int, int>("ShowItemInteractBar", itemId, instanceId);
    }

    private void OnTriggerExit()
    {
        if(!enabledInteract) return;

        EventBus.Invoke<int>("UnShowItemInteractBar", instanceId);
    }

    private IEnumerator MoveBezzier()
    {
        yield return null;

        Vector3 direction = RandomVector3.GetRandomVector3();

        Vector3 startPoint = transform.position;
        Vector3 endPoint = startPoint + (direction * 2f);
        Vector3 centorPoint = ((endPoint + startPoint) / 2f) + (Vector3.up * 2f);

        float percent = 0f, delta = 0f, lerpTime = 0.5f;

        while(percent < 1f)
        {
            delta += Time.deltaTime;
            percent = delta / lerpTime;

            Vector3 bezzierPoint = BezierUtil.GetBezier_Vector3(startPoint, centorPoint, endPoint, percent);
            transform.position = bezzierPoint;

            yield return null;    
        }

        transform.position = endPoint;

        enabledInteract = true;
    }

    //public
    public void InitializeItem(int itemId, int itemAmount)
    {
        this.itemId = itemId;
        instanceId = RandomNumber.GetNumber();
    }

    public void Interact()
    {
        if(!enabledInteract) return;

        int randomAmount = UnityEngine.Random.Range(itemDropMinAmount, itemDropMaxAmount);
        EventBus.Invoke<int, int, bool>("GetItem", itemId, randomAmount, true);
    }

    public void ItemFollow(Transform target)
    {
        follow = false;
        this.target = target;
        follow = true;
    }

    public void MoveStart(Vector3 startPoint)
    {
        enabledInteract = false;

        transform.position = startPoint;
        gameObject.SetActive(true);

        this.RunRoutine(MoveBezzier());
    }
}


