using UnityEngine;
using System.Collections.Generic;

public abstract class PoolManagerBase<T>
{
    protected Transform containerTr;
    private Pool<T> pool;

    protected T GetFromPool(int id, GameObject prefab = null, string name = "")
    {
        return pool.GetFromPool(id, prefab, name);
    }

    protected void SetPool(Transform containerTr = null, 
                System.Func<T, bool> conditionMethod = null, 
                System.Func<int, List<T>> initializeListMethod = null, 
                int capacity = 1, 
                DataType type = DataType.None, 
                string newObjName = "",
                PoolInitIds initInfo = default)
    {
        if(pool != null) return;

        this.containerTr = containerTr;

        pool = new Pool<T>(containerTr, conditionMethod, initializeListMethod, capacity, type, newObjName, initInfo);
    }
}
