using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Entity : MonoBehaviour, ISpawnable
{
    private bool isSpawned = false;

    public bool IsSpawned() => isSpawned;
    public void SetSpawn() => isSpawned = true;
}

public abstract class LivingEntity<T> : Entity where T : LivingEntityDataBox
{
    protected bool isInitializeOnce = false, isInitializeDataBox = false;
    
    protected T livingEntityDataBox;
    protected Selector mainSelecter;
    public List<Enemy_NodeModule> modules;

    public virtual void OnEnable()
    {
        if (isInitializeOnce) return;

        this.RunRoutine(Initializing());
    }

    IEnumerator Initializing()
    {
        yield return new WaitUntil(() => LoadStatus.IsReady);

        Initialize(GetId());

        yield return new WaitUntil(() => isInitializeDataBox);

        OnInitializeTask();

        InitializeBT();

        isInitializeOnce = true;
    }
    
    protected int GetId()
    {
        string gameObjectName = this.gameObject.name.Replace("(clone)", "");

        int enemyDataId = EnemyIDFinder.GetEnemyIDFromName(gameObjectName);

        return enemyDataId;
    }

    protected abstract void Initialize(int enemyDataId);
    protected virtual void InitializeBT() {}
    protected virtual void OnInitializeTask() {}
}