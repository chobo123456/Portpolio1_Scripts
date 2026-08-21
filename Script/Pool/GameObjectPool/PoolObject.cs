using UnityEngine;
using System.Collections;
public class PoolObject : MonoBehaviour
{
    private Coroutine routine;
    public float disableTime = 1f;
    public void OnEnable()
    {
        routine = this.RunRoutine(DelayToDisable(), routine);
    }

    IEnumerator DelayToDisable()
    {
        yield return YieldUtil.WaitForSeconds(disableTime);

        gameObject.SetActive(false);
    }
}
