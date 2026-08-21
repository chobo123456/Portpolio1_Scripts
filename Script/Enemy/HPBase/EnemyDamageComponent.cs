using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDamageComponent : EnemyHpBase<NormalEnemyDataBox>
{
    private Coroutine hitTimeRoutine, animRoutine;
    private PoiseHandler poiseHandler;
    private EnemyDamageAnimator damageAnimator;
    private WaitUntil waitAnim;

    protected override void Initialize()
    {
        EventBus.Invoke<MonoBehaviour, HPBar_ViewModel>("EnemyRegisterViewModel", this, new HPBar_ViewModel(this));

        poiseHandler    = new(box.enemyData.poiseAmount);
        damageAnimator  = new(box);

        waitAnim        = new WaitUntil(() => damageAnimator.IsFinish);

        box.rigid.isKinematic = false;
        box.col.isTrigger = false;
    }

    protected override void VisualInitialize()
    {
        box.visualFeedback.OnEnable();
    }

    protected override void VisualFeedback(DamageSource source)
    {
        box.visualFeedback.VisualFeedback(source);
    }

    protected override void OnDie(DamageSource source)
    {
        EventBus.Invoke<MonoBehaviour>("EnemyUnRegisterViewModel", this);
        EventBus.Invoke<MonoBehaviour, bool>("EnemyUnDetect", box.mono, true);

        IsDie = true;

        damageAnimator.PlayDie();

        box.undoBt.Invoke();
        box.onDie.Invoke();
        
        box.rigid.isKinematic = true;
        box.col.enabled = false;

        EventBus.Invoke<QuestType, int>("QuestManager_OnAskQuestFinish", QuestType.Hunt, box.enemyData.enemyId);

        box.visualFeedback.ActiveDissolve();
    }

    protected override void OnHit(DamageSource source)
    {
        EventBus.Invoke<MonoBehaviour, int, bool>("EnemyDetect", box.mono, box.livingEntityId, true);

        IsHit = true;
        box.undoBt.Invoke(); 

        HitType hitType = poiseHandler.TakeImpact(source.impactForce);
 
        if(hitType == HitType.Big) 
        {
            damageAnimator.PlayBigHit(source);
            poiseHandler.Reset();
        }
        else damageAnimator.PlaySmallHit(source);
        
        hitTimeRoutine  = this.RunRoutine(OnHitTime(source.hitTime), hitTimeRoutine);
        animRoutine     = this.RunRoutine(WaitHit(), animRoutine);
    }

    IEnumerator OnHitTime(float hitTime)
    {
        Time.timeScale = 0f;

        yield return YieldUtil.WaitForSecondsRealtime(hitTime);

        Time.timeScale = 1f;
    }

    IEnumerator WaitHit()
    {
        yield return waitAnim;

        yield return YieldUtil.WaitForSeconds(0.75f);

        IsHit = false;
    }
}