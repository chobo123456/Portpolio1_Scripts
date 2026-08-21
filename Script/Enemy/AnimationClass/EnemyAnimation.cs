using UnityEngine;
using System.Collections.Generic;
public enum EnemyAnimationType
{
    //Normal Anim
    Move,
    Dash,
    Beware,
    Attack,
    SmallHit,
    Hit,
    Die,
    Turn,
    Pattern1,
    Pattern2,
    Pattern3,
    Pattern4,
    Pattern5,
    Pattern6,
    Groggy,
    GroggyEnd,
}

public class EnemyAnimationStorage
{
    private Dictionary<EnemyAnimationType, EnemyAnimation> _animationMap;

    public EnemyAnimationStorage()
    {
        _animationMap = new();
    }

    public void Add(EnemyAnimationType newType, EnemyAnimation newAnimationClass)
    {
        if(!_animationMap.ContainsKey(newType))
            _animationMap.Add(newType, newAnimationClass);
    }

    public T Get<T>(EnemyAnimationType type) where T : EnemyAnimation
    {
        _animationMap.TryGetValue(type, out EnemyAnimation enemyAnim);
        return enemyAnim as T;
    }

    public EnemyAnimation Get(EnemyAnimationType type)
    {
        _animationMap.TryGetValue(type, out EnemyAnimation enemyAnim);
        return enemyAnim;
    }

    public bool IsExist(EnemyAnimationType type)
    {
        return _animationMap.ContainsKey(type);
    }
}

public abstract class EnemyAnimation
{
    protected readonly EnemyEntityDataBox box;
    protected const string ANIMATION_END_FUNCTION_NAME = "AnimationEnd";
    public EnemyAnimation(EnemyEntityDataBox box)
    {
        this.box = box;
    }

    public virtual void OnActAnimEnter() {}
    public virtual void OnActAnimExit() {}
    public virtual bool IsEnd() { return false; }
}

public class EnemyMoveAnimation : EnemyAnimation
{
    private readonly int paramMoveScale, paramZ, paramX;
    public EnemyMoveAnimation(EnemyEntityDataBox box) : base(box)
    {
        paramMoveScale = Animator.StringToHash("MoveScale");
        paramZ = Animator.StringToHash("MoveZ");
        paramX = Animator.StringToHash("MoveX");
        box.animator.enabled = false;
        box.animator.enabled = true;
    }   

    public void SetMoveVelocity(float min, float max)
    {
        float moveScale  = box.rigid.linearVelocity.magnitude;

        moveScale = Mathf.Clamp(moveScale, min, max);

        if(moveScale > 1.0f)
            moveScale = 1f;
        else if(moveScale >= 0.5f)
            moveScale = 0.5f;
        else
            moveScale = 0f;
            
        box.animator.SetFloat(paramMoveScale, moveScale, 0.24f, Time.deltaTime);

        Vector3 velocity = box.rigid.transform.InverseTransformDirection(box.rigid.linearVelocity);

        float velocityX = Mathf.Clamp(velocity.x, min, max);
        float velocityZ = Mathf.Clamp(velocity.z, min, max);

        box.animator.SetFloat(paramZ, velocityZ, 0.24f, Time.deltaTime);
        box.animator.SetFloat(paramX, velocityX, 0.24f, Time.deltaTime);
    }

    public float GetFloat() => box.animator.GetFloat(paramZ);
}

public class EnemyBewareAnimation : EnemyAnimation
{
    private readonly int animationParam_beware;

    public EnemyBewareAnimation(EnemyEntityDataBox box) : base(box)
    {
        animationParam_beware = Animator.StringToHash("Beware");
    }

    public override void OnActAnimEnter()
    {
        base.OnActAnimEnter();

        box.animator.SetBool(animationParam_beware, true);
    }

    public override void OnActAnimExit()
    {
        box.animator.SetBool(animationParam_beware, false);
    }

    public override bool IsEnd()
    { 
        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("Beware") && stateInfo.normalizedTime >= 0.9f;
    }
}

public class EnemyAttackAnimation : EnemyAnimation
{
    private int comboIndex;
    private readonly int[] attackHashes;
    private bool hasEntered = false;
    public EnemyAttackAnimation(EnemyEntityDataBox box) : base(box)
    {
        attackHashes = new int[]
        {
            Animator.StringToHash("Attack1"),
            Animator.StringToHash("Attack2"),
            Animator.StringToHash("Attack3")
        };
    }   

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;

        box.animator.applyRootMotion = true;
        box.animator.SetTrigger(attackHashes[comboIndex]);

        comboIndex = (comboIndex + 1) % attackHashes.Length;

        hasEntered = false;
    }

    public override void OnActAnimExit()
    {
        if(box.animator == null) return;
        box.animator.applyRootMotion = false;
    }

    public override bool IsEnd()
    {
        if(box.animator == null) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(1);

        if(!hasEntered)
        {
            if(stateInfo.IsTag("Attack") && !box.animator.IsInTransition(1))
            {
                hasEntered = true;
            }

            return false;
        }

        if(box.animator.IsInTransition(1)) return false;
        
        return stateInfo.normalizedTime >= 0.9f || !stateInfo.IsTag("Attack");
    }
}

public class EnemyHitAnimation : EnemyAnimation
{
    private readonly int animationParam_Hit, Hit_x, Hit_z;
    public EnemyHitAnimation(EnemyEntityDataBox box) : base(box)
    {
        Hit_x              = Animator.StringToHash("HitX");
        Hit_z               = Animator.StringToHash("HitZ");
        animationParam_Hit = Animator.StringToHash("Hit");
    }   
    
    public void SetHitDirection(float x, float z)
    {
        if(box.animator == null) return;

        x = Mathf.Clamp(x, -1, 1);
        z = Mathf.Clamp(z, -1, 1);

        box.animator.SetFloat(Hit_x, x);
        box.animator.SetFloat(Hit_z, z);
    }

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;

        box.animator.applyRootMotion = true;
        box.animator.ResetTrigger(animationParam_Hit); 
        box.animator.SetTrigger(animationParam_Hit);
    }

    public override bool IsEnd()
    {
        if(box.animator == null) return false;
        
        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(2);
        return stateInfo.IsTag("Hit") && stateInfo.normalizedTime >= 0.9f;
    }

    public override void OnActAnimExit()
    {
        if(box.animator == null) return;
        
        box.animator.applyRootMotion = false;   
    }
}

public class EnemySmallHitAnimation : EnemyAnimation
{
    private readonly int animationParam_Hit, Hit_x, Hit_z;
    public EnemySmallHitAnimation(EnemyEntityDataBox box) : base(box)
    {
        Hit_x              = Animator.StringToHash("HitX");
        Hit_z               = Animator.StringToHash("HitZ");
        animationParam_Hit = Animator.StringToHash("SmallHit");
    }   
    
    public void SetHitDirection(float x, float z)
    {
        if(box.animator == null) return;
        
        x = Mathf.Clamp(x, -1, 1);
        z = Mathf.Clamp(z, -1, 1);

        box.animator.SetFloat(Hit_x, x);
        box.animator.SetFloat(Hit_z, z);
    }

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;

        box.animator.applyRootMotion = true;
        box.animator.ResetTrigger(animationParam_Hit); 
        box.animator.SetTrigger(animationParam_Hit);
    }

    public override bool IsEnd()
    {
        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(2);
        return stateInfo.IsTag("SmallHit") && stateInfo.normalizedTime >= 0.9f;
    }

    public override void OnActAnimExit()
    {
        if(box.animator == null) return;

        box.animator.ResetTrigger(animationParam_Hit); 
        box.animator.applyRootMotion = false;   
    }
}

public class EnemyDashAnimation : EnemyAnimation
{
    private readonly int animationParam_Dash;
    public EnemyDashAnimation(EnemyEntityDataBox box) : base(box)
    {
        animationParam_Dash = Animator.StringToHash("Dash");
    }   

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;
        
        box.animator.applyRootMotion = true;
        box.animator.SetTrigger(animationParam_Dash);
    }

    public override bool IsEnd()
    {
        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(1);
        return stateInfo.IsTag("Dash") && stateInfo.normalizedTime >= 0.9f;
    }
}

public class EnemyDieAnimation : EnemyAnimation
{
    private readonly int animationParam_Die;
    public EnemyDieAnimation(EnemyEntityDataBox box) : base(box)
    {
        animationParam_Die = Animator.StringToHash("Die");
    }   

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;
        
        box.animator.applyRootMotion = false;
        box.animator.SetTrigger(animationParam_Die);
    }
}

//보스 애니메이션

public class TriggerAnimation : EnemyAnimation
{
    private readonly int _hash;
    private bool _useRootMotion = false, _isEndFlag = false;
    
    public TriggerAnimation(
        EnemyEntityDataBox box, 
        string triggerName, 
        string animationClipName,
        bool useRootMotion = true) : base(box)
    {
        _hash             = Animator.StringToHash(triggerName);
        _useRootMotion  = useRootMotion;

        AnimationClip clip = box.animator.GetAnimationClip(animationClipName);

        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.intParameter = _hash;
        animationEvent.time = clip.length;
        animationEvent.functionName = ANIMATION_END_FUNCTION_NAME;
        
        clip.AddEvent(animationEvent);

        InitializeEvent();
    }

    private void InitializeEvent()
    {
        LifecycleBoundEvent.Subscribe<int>(
            value => box.enemyAnimationEvent.OnAnimationEnd += value,
            value => box.enemyAnimationEvent.OnAnimationEnd -= value,
            OnAnimationEnd,
            box.mono
        );
    }

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;

        _isEndFlag = false;

        box.animator.applyRootMotion = _useRootMotion;
        box.animator.SetTrigger(_hash);
    }

    public override bool IsEnd()
    {
        if(box.animator == null) return false;

        return _isEndFlag;
    }

    private void OnAnimationEnd(int _endAnimationHashCode)
    {
        if(_hash != _endAnimationHashCode) return;

        _isEndFlag = true;
    }

    public override void OnActAnimExit()
    {
        _isEndFlag = false;
        box.animator.ResetTrigger(_hash);
    }
}

public class TurnAnimation : EnemyAnimation
{
    private readonly int TurnParam, TurnDir, layer;
    private bool hasEntered = false;
    public TurnAnimation(EnemyEntityDataBox box, int layer = 3) : base(box)
    {
        this.layer           = layer;
        TurnParam            = Animator.StringToHash("Turn");
        TurnDir              = Animator.StringToHash("TurnDir");
    }   
    
    public void SetRotationDirection(float angle)
    {
        if(box.animator == null) return;
    
        box.animator.SetFloat(TurnDir, angle);
    }

    public override void OnActAnimEnter()
    {
        if(box.animator == null) return;

        box.animator.applyRootMotion = true;
        box.animator.SetTrigger(TurnParam);
        hasEntered = false;
    }

    public override bool IsEnd()
    {
        if(box.animator == null) return false;

        var stateInfo = box.animator.GetCurrentAnimatorStateInfo(layer);

        if(!hasEntered)
        {
            if(stateInfo.IsTag("Turn") && !box.animator.IsInTransition(layer))
            {
                hasEntered = true;
            }

            return false;
        }

        if (box.animator.IsInTransition(layer)) return false;

        return stateInfo.normalizedTime >= 0.98f || !stateInfo.IsTag("Turn");
    }

    public override void OnActAnimExit()
    {
        if(box.animator == null) return;
        
        hasEntered = false;
        box.animator.applyRootMotion = false;
    }
}