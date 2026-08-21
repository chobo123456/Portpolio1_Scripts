using UnityEngine;
using System.Collections;
public class BossDamageComponent : EnemyHpBase<EnemyEntityDataBox>, IPoiseBase
{
    public ReactiveProperty<float> poise { get; private set; }
    public ReactiveProperty<float> maxPoise { get; private set; }

    private PoissonDiskSamplingVector poissonDisk;
    private IPhaseAble phaseClass;
    private IGroggyAble groggyClass;
    private WaitUntil waitGroggyEnd;
    private bool isDuringWaitGroggy = false;
    protected override void Initialize()
    {
        maxPoise = new (box.enemyData.poiseAmount);
        poise = new (maxPoise.Value);

        EventBus.Invoke<PoiseBar_ViewModel>("SetBossPoiseBar_ViewModel", new PoiseBar_ViewModel(this));

        poissonDisk = new PoissonDiskSamplingVector(2f, 0.7f);

        phaseClass = box as IPhaseAble;
        groggyClass = box as IGroggyAble;

        waitGroggyEnd = new WaitUntil(() => !groggyClass.isGroggy);

        EventBus.Invoke<HPBar_ViewModel, int>("SetBossHpBar_ViewModel", new HPBar_ViewModel(this), box.livingEntityId);
    }

    protected override void OnDie(DamageSource source) 
    {
        box.col.gameObject.SetActive(false);
        EventBus.Invoke<HPBar_ViewModel, int>("SetBossHpBar_ViewModel", null, box.livingEntityId);
        IsDie = true;

        EventBus.Invoke<QuestType, int>("QuestManager_OnAskQuestFinish", QuestType.DefeatBoss, box.livingEntityId);
    }
    
    protected override void OnHit(DamageSource source)
    {
        FindPhaseCase();

        if(groggyClass != null)
        {
            if(isDuringWaitGroggy) return;

            poise.Value -= source.poiseMinusAmount;

            if(poise.Value <= 0)
            {
                //그로기 처리 
                groggyClass.isGroggy = true; 

                this.RunRoutine(LoopGroggy(), "BossGroggy");
            }
        }
    }

    protected override void VisualFeedback(DamageSource source)
    {
        box.visualFeedback.VisualFeedback(source);
    }

    private void FindPhaseCase()
    {
        if(phaseClass == null) return;

        float percent = curHp.Value / maxHp.Value;

        if(box.enemyData.phaseConditionHp.Length == 2)
        {
            float phase1 = box.enemyData.phaseConditionHp[0];
            float phase2 = box.enemyData.phaseConditionHp[1];

            if(percent <= phase2)
                phaseClass.phase = 2;
            else
                phaseClass.phase = 1;
        }
        else
        {
            float phase1 = box.enemyData.phaseConditionHp[0];
            float phase2 = box.enemyData.phaseConditionHp[1];
            float phase3 = box.enemyData.phaseConditionHp[2];

            if(percent <= phase3)
                phaseClass.phase = 3;
            else if(percent <= phase2)
                phaseClass.phase = 2;
            else
                phaseClass.phase = 1;
        }
    }

    IEnumerator LoopGroggy()
    {
        isDuringWaitGroggy = true;

        float percent = 0f, currentDelta = 0f, lerpTime = 1.5f;

        float startValue    = poise.Value, 
              endValue      = maxPoise.Value;

        while(percent < 1f)
        {
            currentDelta += Time.deltaTime;
            percent = currentDelta / lerpTime;

            float currentValue = Mathf.Lerp(startValue, endValue, percent);
            poise.Value = currentValue;
            yield return YieldUtil.WaitForSeconds(0.02f); 
        }

        yield return waitGroggyEnd;
        isDuringWaitGroggy = false;
    }
}
