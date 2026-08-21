using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public abstract class TaskNode<T> : LeafNode<T> where T : LivingEntityDataBox
{
    protected float duration = 0f, enterTime = 0f;
    protected bool isInitialized = false;
    public TaskNode(LivingEntityDataBox dataBox, float duration = 0f) : base(dataBox)
    {
        this.duration = duration;
    }

    public sealed override EnemyState Execute()
    {
        if(!isInitialized)
        {
            OnEnter();

            enterTime = Time.time;

            isInitialized = true;
        }   

        if(duration > 0f && Time.time - enterTime >= duration)
        {
            OnExit();
            isInitialized = false;

            return EnemyState.Success;
        }

        EnemyState state = OnUpdate();

        if(state != EnemyState.Running)
        {
            OnExit();
            isInitialized = false;
        }

        return state;
    }

    protected virtual void OnEnter() {}
    protected virtual EnemyState OnUpdate() { return EnemyState.Fail; }
    protected virtual void OnExit() {}
    public override void Undo(bool isRootUndo = false)
    {
        if(!isInitialized) return;

        OnExit();
        isInitialized = false;
    }
}