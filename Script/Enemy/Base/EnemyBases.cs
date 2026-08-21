using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase<T> : LivingEntity<T>, ICullable where T : NormalEnemyDataBox
{
    protected bool updateEnabled = true;

    protected override void Initialize(int enemyDataId)
    {
        livingEntityDataBox = this.GetInstance<T>(this.transform, enemyDataId);
        livingEntityDataBox.undoBt += Undo;
        livingEntityDataBox.onDie += OnDie;

        isInitializeDataBox = true;
    }

    private void OnDestroy()
    {
        if(isInitializeDataBox)
        {
            livingEntityDataBox.undoBt -= Undo;
            livingEntityDataBox.onDie -= OnDie;
        }
    }

    //Event
    protected virtual void Undo() {}
    protected virtual void OnDie() {}
    
    //BT
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

    //Cull
    public virtual Transform GetTRS() => this.transform;
    public abstract void ToggleOn();
    public abstract void DisableUpdate();
    public abstract void ToggleOff();
}