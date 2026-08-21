using UnityEngine;
using System.Collections.Generic;

public class EnemyClonePool : PoolManagerBase<IClone>
{
    public EnemyClonePool(Transform targetTr)
    {
        SetPool(
            containerTr : targetTr, 
            conditionMethod : Condition, 
            capacity : 5, 
            type : DataType.EnemyPrefab);

        EventBus.Sub_Func<int, IClone>("Get_EnemyClone", GetFromPool);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, IClone>("Get_EnemyClone", GetFromPool);
    }

    private IClone GetFromPool(int id)
    {
        GameObject prefab = DataLoader.GetData<GameObject>(DataType.EnemyPrefab, id);
        string enemyName = DataLoader.GetData<EnemyData>(DataType.Enemy, id).enemyName;
        
        IClone clone = base.GetFromPool(id, prefab, enemyName);
        return clone;
    }

    private bool Condition(IClone clone)
    {
        return !clone.IsActive();
    }
}