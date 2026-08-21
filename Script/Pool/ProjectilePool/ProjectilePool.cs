using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : PoolManagerBase<ProjectileBase>
{
    public ProjectilePool(Transform targetTr)
    {
        SetPool(
            targetTr, 
            Condition, 
            capacity : 4, 
            type : DataType.Pool,
            newObjName: "Projectile");

        EventBus.Sub_Func<int, ProjectileBase>("Pool_GetProjectile", this.GetFromPool);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, ProjectileBase>("Pool_GetProjectile", this.GetFromPool);
    }

    private bool Condition(ProjectileBase projectileBase)
    {
        return !projectileBase.gameObject.activeSelf || !projectileBase.gameObject.activeInHierarchy;
    }

    private ProjectileBase GetFromPool(int id)
    {
        return base.GetFromPool(id);
    }
}