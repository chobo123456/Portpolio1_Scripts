using UnityEngine;

public class NotUseAnimationDamageComponent : EnemyHpBase<EnemyEntityDataBox>
{
    private Coroutine hitTimeRoutine;
    protected override void Initialize()
    {
        box.rigid.isKinematic = false;
        box.col.isTrigger = false;
    }

    protected override void OnHit(DamageSource source)
    {
        hitTimeRoutine = this.RunRoutine(OnHitTime(source.hitTime), hitTimeRoutine);
    }

    protected override void OnDie(DamageSource source)
    {
        IsDie = true;
    }

    System.Collections.IEnumerator OnHitTime(float hitTime)
    {
        Time.timeScale = 0f;

        yield return YieldUtil.WaitForSecondsRealtime(hitTime);

        Time.timeScale = 1f;
    }

    protected override void VisualFeedback(DamageSource source)
    {
        box.visualFeedback.VisualFeedback(source);
    }
}
