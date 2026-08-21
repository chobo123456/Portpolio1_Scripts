using UnityEngine;
using System.Collections;

public class EnemyDamageAnimator
{
    private readonly EnemyEntityDataBox box;
    private Coroutine hitAnimatorRoutine;
    private bool isPlayingBigHit = false;
    public bool IsFinish { get; private set; } = false;

    public EnemyDamageAnimator(EnemyEntityDataBox box)
    {
        this.box = box;

        box.enemyAnimationStorage.Add(EnemyAnimationType.SmallHit, new EnemySmallHitAnimation(this.box));
        box.enemyAnimationStorage.Add(EnemyAnimationType.Hit, new EnemyHitAnimation(this.box));
        box.enemyAnimationStorage.Add(EnemyAnimationType.Die, new EnemyDieAnimation(this.box));

        box.enemyAnimationStorage.Get(EnemyAnimationType.SmallHit).OnActAnimExit();
        box.enemyAnimationStorage.Get(EnemyAnimationType.Hit).OnActAnimExit();
        box.enemyAnimationStorage.Get(EnemyAnimationType.Die).OnActAnimExit();
    }

    public void PlaySmallHit(DamageSource source)
    {
        if(isPlayingBigHit) return;

        IsFinish = false;

        var smallHitAnimation = box.enemyAnimationStorage.Get<EnemySmallHitAnimation>(EnemyAnimationType.SmallHit);
        
        Vector3 cross = Vector3.Cross(box.col.transform.forward, source.knockbackDir.normalized);
        smallHitAnimation.SetHitDirection(cross.x, cross.z);
        smallHitAnimation.OnActAnimEnter();

        hitAnimatorRoutine = box.mono.RunRoutine(WaitHitAnimation(EnemyAnimationType.SmallHit), hitAnimatorRoutine);
    }

    public void PlayBigHit(DamageSource source)
    {
        IsFinish = false;

        isPlayingBigHit = true;

        var smallHitAnimation = box.enemyAnimationStorage.Get<EnemySmallHitAnimation>(EnemyAnimationType.SmallHit);
        smallHitAnimation.OnActAnimExit();

        var hitAnimation = box.enemyAnimationStorage.Get<EnemyHitAnimation>(EnemyAnimationType.Hit);
        
        Vector3 cross = Vector3.Cross(box.col.transform.forward, source.knockbackDir.normalized);
        hitAnimation.SetHitDirection(cross.x, cross.z);
        hitAnimation.OnActAnimEnter();

        hitAnimatorRoutine = box.mono.RunRoutine(WaitHitAnimation(EnemyAnimationType.Hit), hitAnimatorRoutine);
    }

    public void PlayDie()
    {
        IsFinish = false;

        var dieAnimation = box.enemyAnimationStorage.Get<EnemyDieAnimation>(EnemyAnimationType.Die);
        dieAnimation.OnActAnimEnter();
    }

    IEnumerator WaitHitAnimation(EnemyAnimationType type)
    {
        EnemyAnimation animation = type == EnemyAnimationType.Hit ? 
            box.enemyAnimationStorage.Get<EnemyHitAnimation>(EnemyAnimationType.Hit) :
            box.enemyAnimationStorage.Get<EnemySmallHitAnimation>(EnemyAnimationType.SmallHit);

        yield return new WaitUntil(() => animation.IsEnd());

        if(type == EnemyAnimationType.Hit)
            isPlayingBigHit = false;

        animation.OnActAnimExit();

        IsFinish = true;
    }
}
