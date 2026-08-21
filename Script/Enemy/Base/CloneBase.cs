using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class CloneBase<T> : LivingEntity<T>, IClone where T : CloneEnemyDataBox
{
    protected CloneCommand currentStrategy = CloneCommand.InPlace;
    public ClonePattern pattern; 

    protected override void Initialize(int enemyDataId)
    {
        livingEntityDataBox = this.GetInstance<T>(this.transform, enemyDataId);
        isInitializeDataBox = true;
    }

    public virtual void SetCommand(CloneCommand strategy = CloneCommand.InPlace) { currentStrategy = strategy; }
    public virtual bool IsActive() { return this.gameObject.activeSelf || this.gameObject.activeInHierarchy; }

    public virtual void Execute() {}
    
    public virtual void SetActive(bool isActive) { this.gameObject.SetActive(isActive); }
    public virtual void SetPosition(Vector3 position) { this.gameObject.transform.position = position; }
    public virtual void SetRotation(Quaternion rotation) { this.gameObject.transform.rotation = rotation; }

    public virtual void Exception() {}
}