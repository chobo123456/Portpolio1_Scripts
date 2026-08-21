using UnityEngine;

public class EnemyPool
{
    private readonly EnemyObjectPool objectPool;
    private readonly EnemyClonePool clonePool;
    public EnemyPool(Transform targetTr)
    {
        objectPool  = new(targetTr);
        clonePool   = new(targetTr);
    }

    public void OnDisable()
    {
        objectPool.OnDisable();
        clonePool.OnDisable();
    }
}
