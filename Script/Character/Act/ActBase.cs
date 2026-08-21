using UnityEngine;

public abstract class ActBase
{
    protected PlayerDataBox box;
    public int Priority {  get; protected set; } = 0;
    public bool ActLock { get; protected set; }
    public bool ActProrityLock {get; protected set;}
    
    public ActBase(PlayerDataBox _box)
    {
        this.box = _box;
    }

    public virtual bool CanEnter() { return false; }

    public virtual void ActEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void ActEnd() { }
}