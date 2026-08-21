using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//Skill Factory
public class SkillFactory
{
    public static SkillBase GetSkill(int skill_Id)
    {
        switch (skill_Id)
        {
            case 0:
                return new DummySkill(); 
            case 1:
                return new UnityChan_GetBuff_Skill();
            case 2:
                return new Sting();
            case 3:
                return new UnityChan_DownSword();
            default:
                return new DummySkill();
        }
    }
}

#region Skill_System
public class SkillAct : SubActManager
{
    private List<SkillBase> _skills;
    private SkillBase _currentSkill;
    private int _useableSkillCount;
    public SkillAct(PlayerDataBox box, int skillCount) : base(box)
    {
        _skills = new();
        for(int i = 0; i < skillCount; i++)
        {
            _skills.Add(new DummySkill());

            _skills[i].SetData(box, i);
        }   

        _useableSkillCount = skillCount - 1;
        Priority = 80;
    }

    public void OnSkillChange(int skillSlot, SkillBase newSkill)
    {
        int indexTrans = skillSlot - 1;

        if(_useableSkillCount < indexTrans || indexTrans < 0 || _skills == null) return;
        
        _skills[indexTrans] = newSkill;
        _skills[indexTrans].SetData(box, skillSlot);

        _skills = _skills.OrderByDescending(skill => skill.Priority).ToList();
    }

    public override bool CanEnter() 
    {
        if(_currentSkill != null && _currentSkill.Skill_ActLock)
        {
            ActLock = true;
            return true;    
        }
        else if(ActLock && !_currentSkill.Skill_ActLock)
        {
            ActLock = false;
            return false;   
        }

        if(_skills == null || _skills.Count <= 0) return false;

        var currentUseSkill = SearchUseAble_Skill();

        if (currentUseSkill != null)
        {
            _currentSkill = currentUseSkill;
            currentAct = this;
            return true;
        }

        return false;
    }

    public SkillBase SearchUseAble_Skill()
    {
        if(_skills == null || _skills.Count <= 0) return null;

        for( int i = 0; i < _skills.Count; i ++)
        {
            var skill = _skills[i];

            if(skill == null) continue;

            if(skill.CanUse())
            {
                return skill;
            }
        }

        return null;
    }

    public override void ActEnter() => _currentSkill?.Skill_Start();
    public override void OnUpdate() => _currentSkill?.Skill_During();
    public override void OnFixedUpdate() => _currentSkill?.Skill_Fixed_During();
    public override void OnLateUpdate() => _currentSkill?.Skill_Late_During();

    public override void ActEnd()
    {
        ActLock = false;
        _currentSkill?.Skill_End();    
    }
}

public abstract class SkillBase
{
    public int Priority {get; protected set;}
    public bool Skill_ActLock {get; protected set;}

    protected PlayerDataBox box;

    protected int skillSlot {get; private set;} //��ǲ��
    protected SkillData data;
    protected AnimationActBase animator;

    private float coolDown = 0f;
    private float coolDownTimer = 0f;

    private bool IsActived = false;
    public SkillBase(int id = 0)
    {
        if(id <= 0) return;
        data = DataLoader.GetData<SkillData>(DataType.Skill, id);
    }

    public void SetData(PlayerDataBox box, int skillSlot) {
        this.box = box;
        this.skillSlot = skillSlot;

        SetSkillAnimator();
    } 

    private void SetSkillAnimator()
    {
        animator = new SkillAnim_Act(box, skillSlot);
    }

    public virtual bool CanUse() { return false; }
    public virtual void Skill_Start() {
        EventBus.Invoke<int, int>("SkillUIManager_UseSkill", skillSlot, box.CharacterId);
    }

    public virtual void Skill_During() {}
    public virtual void Skill_Fixed_During() {}
    public virtual void Skill_Late_During() {}
    public virtual void Skill_End() {}
    public virtual void Skill_ExeceptionEnd() {}

    public virtual float CalculateCoolDown(bool isCal = false)
    {
        float coolDownTime = data.coolDown;

        if (isCal && !IsActived && coolDown <= 0f)
        {
            IsActived = true;
            coolDownTimer = coolDownTime;
        }
        
        if(isCal)
            coolDownTimer -= Time.deltaTime;

        float percent = coolDownTimer / coolDownTime;
        coolDown = percent; 

        if (isCal && IsActived && coolDown <= 0.01f)
        {
            IsActived = false;
            coolDownTimer = 0f;
            coolDown = 0f;
        } 
        
        return coolDown;
    }

    public virtual float CurrentCoolDown()
    {
        return coolDownTimer;
    }
    public virtual bool WasActived()
    {
        return IsActived;
    }
}
#endregion

#region Skills

public interface IFireSkill
{
    void FireSkill(float damageMultipler);
}

public class DummySkill : SkillBase
{
    public DummySkill() : base(0)
    {
        Priority = 0;
    }

    public override bool CanUse() { return false; }
    public override void Skill_Start() {}
    public override void Skill_During() {}
    public override void Skill_Fixed_During() {}
    public override void Skill_Late_During() {}
    public override void Skill_End() {}
}

public class UnityChan_GetBuff_Skill : SkillBase
{
    public UnityChan_GetBuff_Skill() : base(1)
    {
        Priority = 30;
    }

    public override bool CanUse() 
    { 
        if(animator == null || data == null || box.CharacterIndex == -1) return false;

        if (!box.sensor.IsGround) return false;

        if(!WasActived() && box.input.IsSkillInput(skillSlot)) return true;

        return false; 
    }

    public override void Skill_Start()
    {
        base.Skill_Start();
        box.hpComp.IgnoreDamage = true;

        animator.OnEnterAnim();

        box.rotate.RotateLock = true;
        
        Skill_ActLock = true;
    }

    public override void Skill_Fixed_During()
    {
        if(animator.IsFinish())
            Skill_ActLock = false;

        box.rigid.linearVelocity = Vector3.zero;
        box.rigid.useGravity = false;
    }

    public override void Skill_End()
    {
        box.stat.SetDecorator(new DecoGaveInfo
        {
            deco = new AttackDamage_Mul_Effect(2f, 5f, box.stat.player),
            isMul = true,
        });

        animator.OnExitAnim();

        box.rigid.useGravity = true;
        box.rotate.RotateLock = false;
        Skill_ActLock = false;
        box.hpComp.IgnoreDamage = false;
    }
}

public class Sting : SkillBase
{
    public Sting() : base(2)
    {
        Priority = 30;
    }

    public override bool CanUse()
    {
        if (animator == null || data == null || box.CharacterIndex == -1) return false;

        if (!box.sensor.IsGround) return false;

        if (!WasActived() && box.input.IsSkillInput(skillSlot)) return true;

        return false;
    }

    public override void Skill_Start()
    {
        base.Skill_Start();
        
        box.sensor.CheckNearEnemy();
        box.rotate.OnAttackStartRotate();
        box.rotate.RotateLock = true;

        animator.OnEnterAnim();

        Skill_ActLock = true;
        box.hpComp.IgnoreDamage = true;

        EventBus.Invoke<float>("FollowCamera_SetDamp", 0.25f);
    }

    public override void Skill_Fixed_During()
    {
        if(animator.IsFinish())
            Skill_ActLock = false;

        box.rigid.linearVelocity = Vector3.zero;
        box.rigid.useGravity = false;
    }

    public override void Skill_End()
    {
        EventBus.Invoke<float>("FollowCamera_SetDamp", 0.035f);
        
        Skill_ActLock = false;

        animator.OnExitAnim();
        box.rotate.OnAttackEndRotate();
        box.hpComp.IgnoreDamage = false;
        box.rigid.useGravity = true;
    }

    public override void Skill_ExeceptionEnd()
    {
        Skill_End();
    }
}

public class UnityChan_DownSword : SkillBase, IFireSkill
{
    private int poolItemId = 100001;
    private LayerMask groundLayer;
    private ISkillObject currentObject;

    public UnityChan_DownSword() : base(3)
    {
        Priority = 70;
        groundLayer = LayerMask.GetMask("Ground");
        _ = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", poolItemId);
    }

    public override bool CanUse()
    {   
        if (animator == null || data == null || box.CharacterIndex == -1) return false;

        if (!box.sensor.IsGround) return false;

        if (!WasActived() && box.input.IsSkillInput(skillSlot)) return true;

        return false;
    }

    public override void Skill_Start()
    {
        base.Skill_Start();
        
        #region chracterSet
        box.sensor.CheckNearEnemy();
        box.rotate.OnAttackStartRotate();
        box.rotate.RotateLock = true;

        animator.OnEnterAnim();

        Skill_ActLock = true;
        box.hpComp.IgnoreDamage = true;
        #endregion

        #region projectileSet

        EventBus.Invoke<float>("FollowCamera_SetDamp", 0.25f);

        GameObject skillObj = EventBus.Invoke_Func<int, GameObject>("Pool_GetGameObject", poolItemId);
        skillObj.SetActive(false);

        currentObject = skillObj.GetComponentInChildren<ISkillObject>();

        if(currentObject != null)
        {
            currentObject.Initialize();
            EventBus.Invoke<IFireSkill>("ChracterAnimationEvent_SetSkillAttacker", this);
        }
        #endregion
    }

    public override void Skill_Fixed_During()
    {
        if(animator.IsFinish())
            Skill_ActLock = false;

        box.rigid.linearVelocity = Vector3.zero;
        box.rigid.useGravity = false;
    }
    
    public override void Skill_End()
    {
        EventBus.Invoke<float>("FollowCamera_SetDamp", 0.035f);
        Skill_ActLock = false;

        animator.OnExitAnim();
        box.rotate.OnAttackEndRotate();
        box.hpComp.IgnoreDamage = false;
        box.rigid.useGravity = true;
    }

    private Vector3 MatchSkillPosition(Vector3 skillObjectUseTransformPos)
    {
        Vector3 calculatePos = skillObjectUseTransformPos;

        Vector3 rayStartPos = calculatePos;
        Vector3 downDirection = Vector3.down;
        Vector3 upDirection = Vector3.up;

        if(Physics.Raycast(rayStartPos, downDirection, out RaycastHit downHit, 4f, groundLayer))
        {
            calculatePos = downHit.point + (Vector3.up * 0.05f);
        }
        else if(Physics.Raycast(rayStartPos, upDirection, out RaycastHit upHit, 4f, groundLayer))
        {
            calculatePos = upHit.point + (Vector3.up * 0.1f);
        }

        return calculatePos;
    }

    public void FireSkill(float damageMultipler)
    {
        Transform skillObjectUseTransform = box.rigid.transform.FindTarget("SkillObjectUsePos");
        Vector3 matchPosition = MatchSkillPosition(skillObjectUseTransform.position);
        currentObject.SetPosition(matchPosition);
        currentObject.SetRotate(skillObjectUseTransform.rotation);

        currentObject.SetActive(true);

        float shakeMul = damageMultipler / 5f;
        currentObject.TryAttack(new DamageSource{
            damageAmount    = box.stat.player.GetAttackDamage() * damageMultipler,
            hit_vfxId       = box.weapon.GetInfo().visualData.vfxId,
            hitTime         = damageMultipler / 50f,
            poiseMinusAmount = 1,
            cameraShakeSource = new CameraShakeSource{frequency = 0.005f * shakeMul, amplitude = 0.05f * shakeMul, duringTime = 0.05f}
        });
    }
}

#endregion