using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public interface IIgnoreDamageAble
{
    bool IgnoreDamage {get; set; }
}

public abstract class EnemyHpBase<T> : LivingEntityHpBase, IIgnoreDamageAble, IDamageable where T : EnemyEntityDataBox
{
    protected T box;

    public bool IgnoreDamage {get; set; } = false;

    private float minIgnoreDamageTime = 0.1f, damagedTime = 0f;
    
    protected virtual void OnEnable()
    {
        IsDie = false;
        
        if(box != null)
        {
            if(box.col != null) box.col.enabled = true;
            if(box.rigid != null) box.rigid.isKinematic = false;
            if(box.enemyData != null)
            {
                maxHp.Value = box.enemyData.enemyHp;
                curHp.Value = maxHp.Value;
            }
            
            VisualInitialize();
        }
    }

    public void SetDataBox(T boxArgs)
    {   
        maxHp = new(10f);
        curHp = new(10f);

        this.box = boxArgs;

        maxHp.Value = box.enemyData.enemyHp;
        curHp.Value = maxHp.Value;

        Initialize();
    }    

    public virtual void TakeDamage(DamageSource source)
    {
        if(IgnoreDamage || IsDie) return;
        if(Time.time - damagedTime <= minIgnoreDamageTime) return;
        damagedTime = Time.time;

        VisualFeedback(source);
        CalculateDamage(source);

        if(curHp.Value <= 0f)
            OnDie(source);
        else
            OnHit(source);
    }

    protected virtual void CalculateDamage(DamageSource source)
    {
        float elementCalDamage = this.OnElementCase(box.enemyData.element, source.elementType, source.damageAmount);
        curHp.Value = Mathf.Max(curHp.Value - elementCalDamage, 0f);
    }

    protected virtual void VisualInitialize() {}
    protected virtual void VisualFeedback(DamageSource source) {}
    protected virtual void OnDie(DamageSource source) {}
    protected virtual void OnHit(DamageSource source) {}
    protected virtual void Initialize() {}
}