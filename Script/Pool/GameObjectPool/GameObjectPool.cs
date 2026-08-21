using UnityEngine;
using System.Collections.Generic;

public class GameObjectPool : PoolManagerBase<GameObject>
{
    public GameObjectPool(Transform targetTr)
    {
        SetPool(
            targetTr, 
            Condition, 
            capacity : 3, 
            type : DataType.Pool,
            newObjName: "Pool");

        EventBus.Sub_Func<int, GameObject>("Pool_GetGameObject", GetFromPool);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, GameObject>("Pool_GetGameObject", GetFromPool);
    }

    private bool Condition(GameObject obj)
    {
        return !obj.activeSelf || !obj.activeInHierarchy;
    }
    
    private GameObject GetFromPool(int id)
    {
        return base.GetFromPool(id);
    }
}

