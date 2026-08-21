using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public abstract class BossBase<T> : LivingEntity<T> where T : BossDataBox
{
    protected override void Initialize(int enemyDataId)
    {
        livingEntityDataBox = this.GetInstance<T>(this.transform, enemyDataId);
        isInitializeDataBox = true;

        SomeTask();
    }

    public virtual void SomeTask() {}
    protected override void InitializeBT()
    {
        List<Node> nodes = new();

        for (int i = 0; i < modules.Count; i++)
        {
            var module = modules[i];
            Node node = module.SetNode(livingEntityDataBox);

            nodes.Add(node);
        }

        mainSelecter = new Selector(nodes);
    }
}