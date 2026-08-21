using UnityEngine;
using System.Collections.Generic;

public class EnemyObjectPool : PoolManagerBase<Entity>
{
    public EnemyObjectPool(Transform targetTr)
    {
        SetPool(
            containerTr : targetTr, 
            conditionMethod : Condition, 
            capacity : 5, 
            type : DataType.EnemyPrefab);

        EventBus.Sub_Func<int, Entity>("Get_Enemy", GetFromPool);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, Entity>("Get_Enemy", GetFromPool);
    }

    private Entity GetFromPool(int id)
    {
        GameObject prefab = DataLoader.GetData<GameObject>(DataType.EnemyPrefab, id);
        string enemyName = DataLoader.GetData<EnemyData>(DataType.Enemy, id).enemyName;

        Entity enemy = base.GetFromPool(id, prefab, enemyName);
        return enemy;
    }

    private bool Condition(Entity entity)
    {
        return !entity.IsSpawned();
    }
}
